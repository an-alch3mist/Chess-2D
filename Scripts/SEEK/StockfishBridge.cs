/*
CHANGELOG (Updated Version):
- Enhanced ParseAnalysisResult() and ParseEvaluationFromInfoLine() to prefer depth-matching info lines with multipv=1
- Added robust mate score parsing with isMate flag and mateDistance tracking  
- Replaced CentipawnsToWinProbability() with configurable logistic mapping (PROB_K = 0.004f)
- Added sophisticated mate probability mapping using MATE_C = 1000f constant
- Added stmEvaluation field to ChessAnalysisResult for side-to-move probability
- Enhanced SelectBestInfoLine() method to handle multipv and depth preferences
- Improved probability clamping to [0.0001f, 0.9999f] range with fallback to 0.5f on errors
- Added comprehensive mate-in-N detection and conversion logic
- Improved FEN validation with better error messages for incorrect board arrangements
- Enhanced stalemate vs checkmate detection logic
- Added configurable constants for easy tuning (PROB_K, MATE_C)
*/

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace GPTDeepResearch
{
	/// <summary>
	/// Unity Stockfish bridge with coroutine-first API.
	/// Provides non-blocking chess engine communication via background threads.
	/// Enhanced with crash detection and recovery mechanisms.
	/// </summary>
	/// 
	/*
		Usage:
		// Start the engine
		stockfishBridge.StartEngine();
		yield return StartCoroutine(stockfishBridge.InitializeEngineCoroutine());

		// Analyze a position
		yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(
			"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
		));

		// Get results
		var result = stockfishBridge.LastAnalysisResult;
		Debug.Log($"Best move: {result.bestMove}");
		Debug.Log($"White win probability: {result.evaluation}");
		Debug.Log($"Side-to-move probability: {result.stmEvaluation}");
		Debug.Log($"Side to move: {result.Side}");
		Debug.Log($"Game over: {result.isGameEnd}");
	*/
	public class StockfishBridge : MonoBehaviour
	{
		[Header("Engine Configuration")]
		[SerializeField] private int defaultTimeoutMs = 20000;
		[SerializeField] private bool enableDebugLogging = true;

		[Header("Default Engine Settings")]
		[SerializeField] private int defaultDepth = 1;
		[SerializeField] private int defaultElo = 400;
		[SerializeField] private int defaultSkillLevel = 0;

		// Evaluation mapping constants - easily tunable
		private const float PROB_K = 0.004f;    // Logistic function steepness for centipawn conversion
		private const float MATE_C = 1000f;     // Mate distance scaling factor

		// Events
		public UnityEvent<string> OnEngineLine = new UnityEvent<string>();

		/// <summary>
		/// Result of a chess analysis request
		/// </summary>
		[System.Serializable]
		public class ChessAnalysisResult
		{
			public string bestMove = "";           // "e2e4", "check-mate", "stale-mate", or "ERROR: message"
			public char Side = ' ';                // 'w' for white, 'b' for black - side to move from FEN
			public float evaluation = 0.5f;        // 0-1 probability for white winning (0.5 = equal, 1.0 = white wins, 0.0 = black wins)
			public float stmEvaluation = 0.5f;     // 0-1 probability for side-to-move winning
			public bool isGameEnd = false;         // True if checkmate or stalemate
			public string errorMessage = "";       // Detailed error if any
			public string rawEngineOutput = "";    // Full engine response for debugging
		}

		// Public properties
		public string LastRawOutput { get; private set; } = "";
		public ChessAnalysisResult LastAnalysisResult { get; private set; } = new ChessAnalysisResult();
		public bool IsEngineRunning
		{
			get
			{
				lock (crashDetectionLock)
				{
					return engineProcess != null && !engineProcess.HasExited && !engineCrashed;
				}
			}
		}
		public bool IsReady { get; private set; } = false;

		// Private fields
		private Process engineProcess;
		private Thread readerThread;
		private volatile bool shouldStop = false;
		private string tempEnginePath;

		// Crash detection
		private volatile bool engineCrashed = false;
		private DateTime lastCommandTime = DateTime.MinValue;
		private readonly object crashDetectionLock = new object();

		// Thread-safe communication
		private readonly ConcurrentQueue<string> incomingLines = new ConcurrentQueue<string>();
		private readonly ConcurrentQueue<string> pendingCommands = new ConcurrentQueue<string>();

		// Request tracking
		private volatile bool waitingForBestMove = false;
		private volatile bool currentRequestCompleted = false;
		private readonly List<string> currentRequestOutput = new List<string>();
		private readonly object requestLock = new object();

		#region Unity Lifecycle

		private void Awake()
		{
			StartEngine();
			StartCoroutine(InitializeEngineOnAwake());
		}

		private IEnumerator InitializeEngineOnAwake()
		{
			yield return StartCoroutine(InitializeEngineCoroutine());
		}

		private void Update()
		{
			// Main thread: drain incoming lines and fire events
			while (incomingLines.TryDequeue(out string line))
			{
				OnEngineLine?.Invoke(line);

				if (enableDebugLogging)
					UnityEngine.Debug.Log($"[Stockfish] < {line}");

				// Track lines for current request
				lock (requestLock)
				{
					if (waitingForBestMove)
					{
						currentRequestOutput.Add(line);

						if (line.StartsWith("bestmove"))
						{
							waitingForBestMove = false;
							currentRequestCompleted = true;
							LastRawOutput = string.Join("\n", currentRequestOutput);
						}
					}
				}

				// Track readiness
				if (line == "readyok")
				{
					IsReady = true;
				}
			}
		}

		// Only stop engine on application quit to persist through focus changes
		private void OnApplicationQuit()
		{
			StopEngine();
		}

		#endregion

		#region Public API

		/// <summary>
		/// Start the Stockfish engine process
		/// </summary>
		public void StartEngine()
		{
			if (IsEngineRunning)
			{
				if (enableDebugLogging)
					UnityEngine.Debug.Log("[Stockfish] Engine already running");
				return;
			}

			string enginePath = GetEngineExecutablePath();
			if (string.IsNullOrEmpty(enginePath))
			{
				UnityEngine.Debug.LogError("[Stockfish] Engine executable not found in StreamingAssets/sf-engine.exe");
				return;
			}

			bool success = false;
			try
			{
				StartEngineProcess(enginePath);
				StartBackgroundReader();
				success = true;

				if (enableDebugLogging)
					UnityEngine.Debug.Log("[Stockfish] Engine started successfully");
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogError($"[Stockfish] Failed to start engine: {e.Message}");
			}

			if (!success)
			{
				// Reset crash state if start failed
				lock (crashDetectionLock)
				{
					engineCrashed = false;
				}
			}
		}

		/// <summary>
		/// Stop the engine and clean up resources
		/// </summary>
		public void StopEngine()
		{
			if (!IsEngineRunning && readerThread == null)
				return;

			shouldStop = true;

			// Try graceful shutdown first - no return in try/catch
			bool gracefulShutdown = false;
			try
			{
				if (engineProcess != null && !engineProcess.HasExited)
				{
					SendCommand("quit");
					gracefulShutdown = engineProcess.WaitForExit(2000);
				}
			}
			catch (Exception e)
			{
				if (enableDebugLogging)
					UnityEngine.Debug.LogWarning($"[Stockfish] Exception during engine shutdown: {e.Message}");
			}

			// Force shutdown if graceful failed - no return in try/catch
			if (!gracefulShutdown)
			{
				try
				{
					if (engineProcess != null && !engineProcess.HasExited)
					{
						if (enableDebugLogging)
							UnityEngine.Debug.Log("[Stockfish] Forcing engine termination");
						engineProcess.Kill();
					}
				}
				catch (Exception e)
				{
					if (enableDebugLogging)
						UnityEngine.Debug.LogWarning($"[Stockfish] Exception during force kill: {e.Message}");
				}
			}

			// Cleanup - no returns in try/catch blocks
			CleanupResources();
		}

		/// <summary>
		/// Analyze chess position using inspector defaults
		/// </summary>
		/// <param name="fen">Position in FEN notation (or "startpos")</param>
		public IEnumerator AnalyzePositionCoroutine(string fen)
		{
			// Use inspector defaults, but ensure minimum depth for mate detection
			int minDepthForMate = Mathf.Max(defaultDepth, 3);
			yield return StartCoroutine(AnalyzePositionCoroutine(fen, -1, minDepthForMate, defaultElo, defaultSkillLevel));
		}

		/// <summary>
		/// Analyze chess position and get best move with evaluation.
		/// Returns comprehensive analysis including checkmate/stalemate detection.
		/// </summary>
		/// <param name="fen">Position in FEN notation (or "startpos")</param>
		/// <param name="movetimeMs">Time limit in milliseconds (-1 to use depth instead)</param>
		/// <param name="depth">Search depth (default: 1, -1 to use movetime instead)</param>
		/// <param name="elo">Engine strength (default: 400, -1 for maximum strength)</param>
		/// <param name="skillLevel">Skill level 0-20, where 0 is weakest (default: 0, -1 disabled)</param>
		public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs = 2000, int depth = 1, int elo = 400, int skillLevel = 0)
		{
			// Reset result
			LastAnalysisResult = new ChessAnalysisResult();

			// Extract side to move from FEN and validate
			char sideToMove = ExtractSideFromFen(fen);
			string fenValidationError = ValidateFen(fen);

			LastAnalysisResult.Side = sideToMove; // Always set side, even for errors

			if (!string.IsNullOrEmpty(fenValidationError))
			{
				SetErrorResult($"ERROR: {fenValidationError}", fenValidationError);
				yield break;
			}

			// Pre-flight engine check
			bool crashDetected = DetectAndHandleCrash();
			if (crashDetected)
			{
				if (enableDebugLogging)
					UnityEngine.Debug.LogError("[Stockfish] Engine has crashed, attempting restart...");

				yield return StartCoroutine(RestartEngineCoroutine());

				if (!IsEngineRunning)
				{
					SetErrorResult("ERROR: Engine crashed and restart failed", "Engine crashed and restart failed");
					yield break;
				}
			}

			if (!IsEngineRunning)
			{
				SetErrorResult("ERROR: Engine not running. Call StartEngine() first.", "Engine not running. Call StartEngine() first.");
				yield break;
			}

			// Ensure minimum depth for accurate mate detection
			int analysisDepth = depth;
			if (depth > 0 && depth < 3)
			{
				analysisDepth = 3; // Minimum depth for mate detection
				if (enableDebugLogging)
					UnityEngine.Debug.Log($"[Stockfish] Increasing depth from {depth} to {analysisDepth} for mate detection");
			}

			// Setup request tracking
			float startTime = Time.time;
			float timeoutSeconds = (movetimeMs > 0 ? movetimeMs + 5000 : defaultTimeoutMs) / 1000f;

			lock (requestLock)
			{
				currentRequestOutput.Clear();
				waitingForBestMove = true;
				currentRequestCompleted = false;
			}

			// Send commands sequence - check for crashes between each
			bool commandSuccess = SendCommandSequence(fen, elo, skillLevel, analysisDepth, movetimeMs);
			if (!commandSuccess)
			{
				yield break; // Error already set in SendCommandSequence
			}

			// Wait for completion - YIELDS OUTSIDE ANY TRY/CATCH
			bool requestTimedOut = false;
			bool requestAborted = false;

			while (!requestTimedOut && !requestAborted)
			{
				yield return null;

				// Check timeout
				if (Time.time - startTime > timeoutSeconds)
				{
					requestTimedOut = true;
					break;
				}

				// Check completion
				lock (requestLock)
				{
					if (currentRequestCompleted)
						break;
				}

				// Check for crash during analysis
				if (DetectAndHandleCrash())
				{
					requestAborted = true;
					break;
				}
			}

			// Handle error cases
			if (requestTimedOut)
			{
				SendCommand("stop");  // Try to stop the search
				SetErrorResult($"ERROR: Request timed out after {timeoutSeconds}s", $"Request timed out after {timeoutSeconds}s");
				if (enableDebugLogging)
					UnityEngine.Debug.LogWarning("[Stockfish] Request timed out");
				yield break;
			}

			if (requestAborted)
			{
				SetErrorResult($"ERROR: Engine crashed during analysis of position: {fen}", $"Engine crashed during analysis of position: {fen}");
				if (enableDebugLogging)
					UnityEngine.Debug.LogError("[Stockfish] Request aborted due to engine crash");
				yield break;
			}

			// Parse successful result - no returns in try/catch
			ParseAnalysisResult(LastRawOutput, analysisDepth);
		}

		/// <summary>
		/// Restart engine after crash
		/// </summary>
		public IEnumerator RestartEngineCoroutine()
		{
			if (enableDebugLogging)
				UnityEngine.Debug.Log("[Stockfish] Restarting engine after crash...");

			StopEngine();
			yield return new WaitForSeconds(1f);

			StartEngine();
			yield return StartCoroutine(InitializeEngineCoroutine());

			if (IsEngineRunning)
			{
				if (enableDebugLogging)
					UnityEngine.Debug.Log("[Stockfish] Engine restarted successfully");
			}
			else
			{
				UnityEngine.Debug.LogError("[Stockfish] Failed to restart engine");
			}
		}

		/// <summary>
		/// Detect and handle engine crash
		/// </summary>
		public bool DetectAndHandleCrash()
		{
			lock (crashDetectionLock)
			{
				if (engineProcess != null && engineProcess.HasExited)
				{
					engineCrashed = true;
					UnityEngine.Debug.LogError($"[Stockfish] Engine process has exited with code: {engineProcess.ExitCode}");
					return true;
				}

				// Check for timeout (engine not responding)
				if (lastCommandTime != DateTime.MinValue &&
					DateTime.Now.Subtract(lastCommandTime).TotalSeconds > 30)
				{
					UnityEngine.Debug.LogWarning("[Stockfish] Engine appears to be unresponsive");
					// Don't mark as crashed for timeout, but log it
				}

				return engineCrashed;
			}
		}

		/// <summary>
		/// Send arbitrary UCI command to engine
		/// </summary>
		public void SendCommand(string command)
		{
			if (DetectAndHandleCrash())
			{
				if (enableDebugLogging)
					UnityEngine.Debug.LogError("[Stockfish] Cannot send command - engine has crashed");
				return;
			}

			if (!IsEngineRunning)
			{
				if (enableDebugLogging)
					UnityEngine.Debug.LogWarning("[Stockfish] Cannot send command - engine not running");
				return;
			}

			lock (crashDetectionLock)
			{
				lastCommandTime = DateTime.Now;
			}

			// Check if process and streams are still valid - no returns in try/catch
			bool commandSent = false;
			if (engineProcess != null && !engineProcess.HasExited &&
				engineProcess.StandardInput != null && engineProcess.StandardInput.BaseStream.CanWrite)
			{
				try
				{
					engineProcess.StandardInput.WriteLine(command);
					engineProcess.StandardInput.Flush();
					commandSent = true;

					if (enableDebugLogging)
						UnityEngine.Debug.Log($"[Stockfish] > {command}");
				}
				catch (System.ObjectDisposedException)
				{
					// Process was disposed - must be caught before InvalidOperationException
					lock (crashDetectionLock)
					{
						engineCrashed = true;
					}
					UnityEngine.Debug.LogError($"[Stockfish] Process disposed while sending command '{command}'");
				}
				catch (System.InvalidOperationException)
				{
					// Process was terminated
					lock (crashDetectionLock)
					{
						engineCrashed = true;
					}
					UnityEngine.Debug.LogError($"[Stockfish] Process terminated while sending command '{command}'");
				}
				catch (System.IO.IOException)
				{
					// Stream was closed - engine likely crashed
					lock (crashDetectionLock)
					{
						engineCrashed = true;
					}
					UnityEngine.Debug.LogError($"[Stockfish] IO Error sending command '{command}' - engine likely crashed");
				}
			}

			if (!commandSent)
			{
				// Engine process or streams are not valid
				lock (crashDetectionLock)
				{
					engineCrashed = true;
				}
				UnityEngine.Debug.LogError($"[Stockfish] Cannot send command '{command}' - engine process or streams not valid");
			}
		}

		/// <summary>
		/// Initialize engine and wait until ready
		/// </summary>
		public IEnumerator InitializeEngineCoroutine()
		{
			if (!IsEngineRunning)
			{
				yield break;
			}

			IsReady = false;
			SendCommand("uci");
			SendCommand("isready");

			// Wait for readyok response
			float startTime = Time.time;
			while (!IsReady && Time.time - startTime < 10f)
			{
				yield return null;
			}

			if (!IsReady)
			{
				UnityEngine.Debug.LogError("[Stockfish] Engine failed to initialize within 10 seconds");
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Set error result and log appropriately - helper to avoid duplication
		/// </summary>
		private void SetErrorResult(string bestMove, string errorMessage)
		{
			LastAnalysisResult.bestMove = bestMove;
			LastAnalysisResult.errorMessage = errorMessage;
			LastAnalysisResult.evaluation = 0.5f;
			LastAnalysisResult.stmEvaluation = 0.5f;
			LastRawOutput = $"ERROR: {errorMessage}";

			if (enableDebugLogging)
				UnityEngine.Debug.Log($"<color=red>[Stockfish ERROR] {errorMessage}</color>");
		}

		/// <summary>
		/// Send command sequence for analysis - returns success/failure
		/// </summary>
		private bool SendCommandSequence(string fen, int elo, int skillLevel, int depth, int movetimeMs)
		{
			// Send ucinewgame before each analysis to ensure clean state
			SendCommand("ucinewgame");

			// Check for crash after ucinewgame
			if (DetectAndHandleCrash())
			{
				SetErrorResult($"ERROR: Engine crashed during ucinewgame", $"Engine crashed during ucinewgame");
				return false;
			}

			// Configure engine weakness settings
			ConfigureEngineWeakness(elo, skillLevel);

			// Check for crash after configuration
			if (DetectAndHandleCrash())
			{
				SetErrorResult($"ERROR: Engine crashed during configuration", $"Engine crashed during configuration");
				return false;
			}

			// Set position
			if (fen == "startpos" || string.IsNullOrEmpty(fen))
			{
				SendCommand("position startpos");
			}
			else
			{
				SendCommand($"position fen {fen}");
			}

			// Check if engine crashed during position setup
			if (DetectAndHandleCrash())
			{
				SetErrorResult($"ERROR: Engine crashed while processing position: {fen}", $"Engine crashed while processing position: {fen}");
				return false;
			}

			// Check if any of the SendCommand calls failed by checking engine state
			if (!IsEngineRunning)
			{
				SetErrorResult("ERROR: Engine died during position setup", "Engine died during position setup");
				return false;
			}

			// Construct and send go command
			string goCommand = ConstructGoCommand(depth, movetimeMs);
			SendCommand(goCommand);

			// Final check after sending go command
			if (!IsEngineRunning)
			{
				SetErrorResult("ERROR: Engine died during go command", "Engine died during go command");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Clean up all resources - no returns in try/catch blocks
		/// </summary>
		private void CleanupResources()
		{
			// Wait for reader thread
			if (readerThread != null)
			{
				bool threadJoined = readerThread.Join(1000);
				if (!threadJoined && enableDebugLogging)
					UnityEngine.Debug.LogWarning("[Stockfish] Reader thread did not join within timeout");
				readerThread = null;
			}

			// Dispose process
			if (engineProcess != null)
			{
				try
				{
					engineProcess.Dispose();
				}
				catch (Exception e)
				{
					if (enableDebugLogging)
						UnityEngine.Debug.LogWarning($"[Stockfish] Exception disposing process: {e.Message}");
				}
				finally
				{
					engineProcess = null;
				}
			}

			CleanupTempFile();

			// Reset states
			lock (crashDetectionLock)
			{
				engineCrashed = false;
				lastCommandTime = DateTime.MinValue;
			}

			IsReady = false;
			shouldStop = false;

			if (enableDebugLogging)
				UnityEngine.Debug.Log("[Stockfish] Engine stopped and cleaned up");
		}

		/// <summary>
		/// Extract the side to move from FEN string
		/// </summary>
		private char ExtractSideFromFen(string fen)
		{
			if (fen == "startpos" || string.IsNullOrEmpty(fen))
				return 'w'; // startpos is white to move

			string[] parts = fen.Trim().Split(' ');
			if (parts.Length >= 2)
			{
				char side = parts[1].ToLower()[0];
				return (side == 'w' || side == 'b') ? side : 'w';
			}

			return 'w';
		}

		/// <summary>
		/// Enhanced FEN validation with better error messages for board arrangement and king count
		/// </summary>
		private string ValidateFen(string fen)
		{
			if (fen == "startpos" || string.IsNullOrEmpty(fen))
				return "";  // startpos is always valid

			string[] parts = fen.Trim().Split(' ');
			if (parts.Length < 1)
				return "Invalid FEN: empty or invalid format";

			if (parts.Length == 1)
				return "Invalid FEN: missing side-to-move data (expected format: 'position w/b castling en_passant halfmove fullmove')";

			if (parts.Length < 2)
				return "Invalid FEN: missing side-to-move field";

			string position = parts[0];

			// Validate basic structure (should have 7 '/' separators for 8 ranks)
			string[] ranks = position.Split('/');
			if (ranks.Length != 8)
				return $"Invalid FEN: board must have 8 ranks, found {ranks.Length}";

			// Check each rank for valid content and correct square count
			for (int i = 0; i < ranks.Length; i++)
			{
				int squareCount = 0;
				foreach (char c in ranks[i])
				{
					if (char.IsDigit(c))
					{
						int emptySquares = c - '0';
						if (emptySquares < 1 || emptySquares > 8)
							return $"Invalid FEN: invalid empty square count '{c}' in rank {8 - i}";
						squareCount += emptySquares;
					}
					else if ("rnbqkpRNBQKP".IndexOf(c) >= 0)
					{
						squareCount += 1;
					}
					else
					{
						return $"Invalid FEN: invalid character '{c}' in rank {8 - i}";
					}
				}

				if (squareCount != 8)
					return $"Invalid FEN: rank {8 - i} has {squareCount} squares, expected 8";
			}

			// Count kings
			int whiteKings = 0;
			int blackKings = 0;
			foreach (char c in position)
			{
				if (c == 'K') whiteKings++;
				else if (c == 'k') blackKings++;
			}

			if (whiteKings != 1)
				return $"Invalid FEN: found {whiteKings} white king(s), expected exactly 1";

			if (blackKings != 1)
				return $"Invalid FEN: found {blackKings} black king(s), expected exactly 1";

			// Validate side to move
			if (parts.Length >= 2)
			{
				char side = parts[1].ToLower()[0];
				if (side != 'w' && side != 'b')
					return $"Invalid FEN: side to move must be 'w' or 'b', found '{parts[1]}'";
			}

			return "";  // Valid
		}

		/// <summary>
		/// Convert centipawn evaluation to win probability using configurable logistic function
		/// Formula: prob_white = 1.0 / (1.0 + exp(-k * cp))
		/// </summary>
		private float CentipawnsToWinProbability(float centipawns)
		{
			// Use configurable logistic mapping
			float exponent = -PROB_K * centipawns;
			return 1f / (1f + Mathf.Exp(exponent));
		}

		/// <summary>
		/// Convert mate distance to win probability with calibrated mapping
		/// For mateDistance > 0 (White mates): prob_white = 1.0f - 1.0f / (1.0f + C / mateDistance)
		/// For mateDistance < 0 (Black mates): symmetric mapping
		/// </summary>
		private float MateDistanceToWinProbability(int mateDistance)
		{
			if (mateDistance > 0)
			{
				// White mates in N: prob_white = 1.0f - 1.0f / (1.0f + C / mateDistance)
				float prob = 1.0f - 1.0f / (1.0f + MATE_C / mateDistance);
				return Mathf.Clamp(prob, 0.0001f, 0.9999f);
			}
			else if (mateDistance < 0)
			{
				// Black mates in N: symmetric mapping (prob_white near 0)
				int absMateDistance = Mathf.Abs(mateDistance);
				float prob = 1.0f / (1.0f + MATE_C / absMateDistance);
				return Mathf.Clamp(prob, 0.0001f, 0.9999f);
			}
			else
			{
				// Mate in 0 shouldn't happen, but handle gracefully
				return 0.5f;
			}
		}

		/// <summary>
		/// Select the best info line based on depth and multipv preferences
		/// Prefers: depth == targetDepth && multipv == 1, fallback to highest depth && multipv == 1
		/// </summary>
		private string SelectBestInfoLine(string[] lines, int targetDepth)
		{
			string bestLine = null;
			int bestDepth = -1;
			bool foundTargetDepth = false;

			// First pass: look for exact depth match with multipv=1 (or no multipv)
			foreach (string line in lines)
			{
				if (!line.Trim().StartsWith("info") || !line.Contains("score"))
					continue;

				var (depth, multipv, hasScore) = ParseInfoLineMetadata(line);
				if (!hasScore) continue;

				// Prefer multipv=1 or no multipv specified
				if (multipv > 1) continue;

				if (depth == targetDepth)
				{
					bestLine = line;
					foundTargetDepth = true;
					break; // Found exact match, use it
				}
			}

			// Second pass: if no exact depth match, find highest depth with multipv=1
			if (!foundTargetDepth)
			{
				foreach (string line in lines)
				{
					if (!line.Trim().StartsWith("info") || !line.Contains("score"))
						continue;

					var (depth, multipv, hasScore) = ParseInfoLineMetadata(line);
					if (!hasScore) continue;

					// Prefer multipv=1 or no multipv specified
					if (multipv > 1) continue;

					if (depth > bestDepth)
					{
						bestDepth = depth;
						bestLine = line;
					}
				}
			}

			return bestLine;
		}

		/// <summary>
		/// Parse metadata from info line (depth, multipv, hasScore)
		/// </summary>
		private (int depth, int multipv, bool hasScore) ParseInfoLineMetadata(string infoLine)
		{
			string[] tokens = infoLine.Split(' ');
			int depth = -1;
			int multipv = 1; // Default to 1 if not specified
			bool hasScore = false;

			for (int i = 0; i < tokens.Length - 1; i++)
			{
				if (tokens[i] == "depth" && int.TryParse(tokens[i + 1], out int d))
				{
					depth = d;
				}
				else if (tokens[i] == "multipv" && int.TryParse(tokens[i + 1], out int mv))
				{
					multipv = mv;
				}
				else if (tokens[i] == "score")
				{
					hasScore = true;
				}
			}

			return (depth, multipv, hasScore);
		}

		/// <summary>
		/// Parse engine output into structured analysis result with enhanced evaluation mapping
		/// </summary>
		private void ParseAnalysisResult(string rawOutput, int targetDepth)
		{
			LastAnalysisResult.rawEngineOutput = rawOutput;

			if (string.IsNullOrEmpty(rawOutput))
			{
				LastAnalysisResult.bestMove = "ERROR: No engine output";
				LastAnalysisResult.errorMessage = "No engine output received";
				return;
			}

			string[] lines = rawOutput.Split('\n');
			string bestMoveLine = "";
			float whiteWinProb = 0.5f; // Default fallback
			bool foundEvaluation = false;

			// Select best info line based on depth and multipv preferences
			string selectedInfoLine = SelectBestInfoLine(lines, targetDepth);

			if (!string.IsNullOrEmpty(selectedInfoLine))
			{
				var evalResult = ParseEvaluationFromInfoLine(selectedInfoLine);
				if (!float.IsNaN(evalResult.centipawns) || evalResult.isMate)
				{
					foundEvaluation = true;

					if (evalResult.isMate)
					{
						// Use mate-specific probability mapping
						whiteWinProb = MateDistanceToWinProbability(evalResult.mateDistance);
						if (enableDebugLogging)
							UnityEngine.Debug.Log($"[Stockfish] Mate in {Mathf.Abs(evalResult.mateDistance)} -> White prob: {whiteWinProb:F4}");
					}
					else
					{
						// Use centipawn-based logistic mapping
						whiteWinProb = CentipawnsToWinProbability(evalResult.centipawns);
						if (enableDebugLogging)
							UnityEngine.Debug.Log($"[Stockfish] {evalResult.centipawns}cp -> White prob: {whiteWinProb:F4}");
					}
				}
			}

			// Clamp probability to safe range
			whiteWinProb = Mathf.Clamp(whiteWinProb, 0.0001f, 0.9999f);
			LastAnalysisResult.evaluation = whiteWinProb;

			// Calculate side-to-move evaluation
			if (LastAnalysisResult.Side == 'b')
			{
				LastAnalysisResult.stmEvaluation = 1f - whiteWinProb;
			}
			else
			{
				LastAnalysisResult.stmEvaluation = whiteWinProb;
			}

			// Parse best move line
			foreach (string line in lines)
			{
				if (line.Trim().StartsWith("bestmove"))
				{
					bestMoveLine = line.Trim();
					break;
				}
			}

			if (!string.IsNullOrEmpty(bestMoveLine))
			{
				string[] parts = bestMoveLine.Split(' ');
				if (parts.Length >= 2)
				{
					string move = parts[1];

					// Handle special cases
					if (move == "(none)" || string.IsNullOrEmpty(move))
					{
						// No legal moves - determine if checkmate or stalemate
						// If we found a mate evaluation, it's checkmate
						if (foundEvaluation && selectedInfoLine != null)
						{
							var evalResult = ParseEvaluationFromInfoLine(selectedInfoLine);
							if (evalResult.isMate)
							{
								LastAnalysisResult.bestMove = "check-mate";
								LastAnalysisResult.isGameEnd = true;
								// Keep the mate probability we calculated
							}
							else
							{
								LastAnalysisResult.bestMove = "stale-mate";
								LastAnalysisResult.isGameEnd = true;
								LastAnalysisResult.evaluation = 0.5f; // Stalemate is draw
								LastAnalysisResult.stmEvaluation = 0.5f;
							}
						}
						else
						{
							// No evaluation found, assume stalemate (more common)
							LastAnalysisResult.bestMove = "stale-mate";
							LastAnalysisResult.isGameEnd = true;
							LastAnalysisResult.evaluation = 0.5f;
							LastAnalysisResult.stmEvaluation = 0.5f;
						}
					}
					else
					{
						// Normal move
						LastAnalysisResult.bestMove = move;
						LastAnalysisResult.isGameEnd = false;
						// Keep the calculated probabilities
					}
				}
				else
				{
					LastAnalysisResult.bestMove = "ERROR: Could not parse bestmove";
					LastAnalysisResult.errorMessage = "Invalid bestmove format";
				}
			}
			else
			{
				LastAnalysisResult.bestMove = "ERROR: No bestmove found in engine output";
				LastAnalysisResult.errorMessage = "No bestmove line found";
			}

			if (enableDebugLogging && foundEvaluation)
			{
				UnityEngine.Debug.Log($"[Stockfish] Final evaluation - White: {LastAnalysisResult.evaluation:F4}, STM: {LastAnalysisResult.stmEvaluation:F4} (Side: {LastAnalysisResult.Side})");
			}
		}

		/// <summary>
		/// Parse evaluation from UCI info line - enhanced to capture mate distance
		/// Returns: (centipawns, isMate, mateDistance)
		/// </summary>
		private (float centipawns, bool isMate, int mateDistance) ParseEvaluationFromInfoLine(string infoLine)
		{
			// Look for "score cp" or "score mate"
			int scoreIndex = infoLine.IndexOf("score");
			if (scoreIndex == -1) return (float.NaN, false, 0);

			string scorePart = infoLine.Substring(scoreIndex);
			string[] tokens = scorePart.Split(' ');

			for (int i = 0; i < tokens.Length - 1; i++)
			{
				if (tokens[i] == "cp" && i + 1 < tokens.Length)
				{
					if (int.TryParse(tokens[i + 1], out int centipawns))
					{
						return (centipawns, false, 0);  // Centipawn score
					}
				}
				else if (tokens[i] == "mate" && i + 1 < tokens.Length)
				{
					if (int.TryParse(tokens[i + 1], out int mateIn))
					{
						// Return the mate distance directly - don't convert to centipawns
						return (float.NaN, true, mateIn);
					}
				}
			}

			return (float.NaN, false, 0);
		}

		/// <summary>
		/// Configure engine weakness using available UCI options
		/// </summary>
		/// <param name="elo">Target Elo rating (-1 for max strength)</param>
		/// <param name="skillLevel">Skill level 0-20 where 0 is weakest (-1 disabled)</param>
		private void ConfigureEngineWeakness(int elo, int skillLevel)
		{
			// Only configure if engine is running
			if (!IsEngineRunning) return;

			// Configure Elo limitation if specified
			if (elo > 0)
			{
				SendCommand("setoption name UCI_LimitStrength value true");
				SendCommand($"setoption name UCI_Elo value {elo}");

				if (enableDebugLogging)
					UnityEngine.Debug.Log($"[Stockfish] Set UCI_Elo to {elo}");
			}
			else
			{
				SendCommand("setoption name UCI_LimitStrength value false");
			}

			// Configure Skill Level if specified
			if (skillLevel >= 0 && skillLevel <= 20)
			{
				SendCommand($"setoption name Skill Level value {skillLevel}");
				if (enableDebugLogging)
					UnityEngine.Debug.Log($"[Stockfish] Set Skill Level to {skillLevel}");
			}
		}

		/// <summary>
		/// Construct the 'go' command, preferring depth over movetime for deterministic weakness
		/// </summary>
		/// <param name="depth">Search depth (>0 takes precedence)</param>
		/// <param name="movetimeMs">Move time in milliseconds (used if depth <= 0)</param>
		/// <returns>UCI go command string</returns>
		private string ConstructGoCommand(int depth, int movetimeMs)
		{
			string goCommand;
			if (depth > 0)
			{
				goCommand = $"go depth {depth}";
				if (enableDebugLogging)
					UnityEngine.Debug.Log($"[Stockfish] Using depth-limited search: depth {depth}");
			}
			else
			{
				goCommand = $"go movetime {movetimeMs}";
				if (enableDebugLogging)
					UnityEngine.Debug.Log($"[Stockfish] Using time-limited search: {movetimeMs}ms");
			}
			return goCommand;
		}

		private string GetEngineExecutablePath()
		{
			string streamingAssetsPath = Path.Combine(Application.streamingAssetsPath, "sf-engine.exe");

			if (!File.Exists(streamingAssetsPath))
			{
				return null;
			}

			// On some platforms, we may need to copy to a writable location
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
			return CopyToTempLocation(streamingAssetsPath);
#else
			return streamingAssetsPath;
#endif
		}

		private string CopyToTempLocation(string sourcePath)
		{
			string tempPath = null;
			try
			{
				tempEnginePath = Path.Combine(Path.GetTempPath(), $"sf-engine-{System.Guid.NewGuid():N}.exe");
				File.Copy(sourcePath, tempEnginePath, true);
				tempPath = tempEnginePath;

				if (enableDebugLogging)
					UnityEngine.Debug.Log($"[Stockfish] Copied engine to temp location: {tempEnginePath}");
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogError($"[Stockfish] Failed to copy engine to temp location: {e.Message}");
			}

			return tempPath;
		}

		private void CleanupTempFile()
		{
			if (!string.IsNullOrEmpty(tempEnginePath) && File.Exists(tempEnginePath))
			{
				try
				{
					File.Delete(tempEnginePath);
					if (enableDebugLogging)
						UnityEngine.Debug.Log("[Stockfish] Cleaned up temp engine file");
				}
				catch (Exception e)
				{
					if (enableDebugLogging)
						UnityEngine.Debug.LogWarning($"[Stockfish] Failed to cleanup temp file: {e.Message}");
				}
				finally
				{
					tempEnginePath = null;
				}
			}
		}

		private void StartEngineProcess(string enginePath)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = enginePath,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			engineProcess = Process.Start(startInfo);

			if (engineProcess == null)
			{
				throw new Exception("Failed to start engine process");
			}
		}

		private void StartBackgroundReader()
		{
			shouldStop = false;
			readerThread = new Thread(BackgroundReaderLoop)
			{
				IsBackground = true,
				Name = "StockfishReader"
			};
			readerThread.Start();
		}

		private void BackgroundReaderLoop()
		{
			string line = null;
			bool exceptionOccurred = false;

			try
			{
				while (!shouldStop && engineProcess != null && !engineProcess.HasExited)
				{
					line = engineProcess.StandardOutput.ReadLine();
					if (line != null)
					{
						incomingLines.Enqueue(line);
					}
					else
					{
						// ReadLine returned null - engine might have closed output
						if (!shouldStop)
						{
							UnityEngine.Debug.LogWarning("[Stockfish] Engine output stream closed unexpectedly");
							break;
						}
					}
				}
			}
			catch (Exception e)
			{
				exceptionOccurred = true;
				if (!shouldStop)
				{
					lock (crashDetectionLock)
					{
						engineCrashed = true;
					}
					incomingLines.Enqueue($"ERROR: Reader thread exception: {e.Message}");
					UnityEngine.Debug.LogError($"[Stockfish] Reader thread crashed: {e.Message}");
				}
			}

			// Clean exit logging
			if (!exceptionOccurred && enableDebugLogging)
			{
				UnityEngine.Debug.Log("[Stockfish] Reader thread exited cleanly");
			}
		}

		#endregion
	}
}