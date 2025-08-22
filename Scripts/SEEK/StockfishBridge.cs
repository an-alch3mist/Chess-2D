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
			"startpos", 
			depth: 1,        // default: shallow for speed
			elo: 400,        // default: weak engine
			skillLevel: 0    // default: minimum skill
		));

		// Get results
		var result = stockfishBridge.LastAnalysisResult;
		Debug.Log($"Best move: {result.bestMove}");
		Debug.Log($"Evaluation: {result.evaluation} centipawns");
		Debug.Log($"Game over: {result.isGameEnd}");
	*/
	public class StockfishBridge : MonoBehaviour
	{
		[Header("Engine Configuration")]
		[SerializeField] private int defaultTimeoutMs = 20000;
		[SerializeField] private bool enableDebugLogging = true;

		// Events
		public UnityEvent<string> OnEngineLine = new UnityEvent<string>();

		/// <summary>
		/// Result of a chess analysis request
		/// </summary>
		[System.Serializable]
		public class ChessAnalysisResult
		{
			public string bestMove = "";           // "e2e4", "check-mate", "stale-mate", or "ERROR: message"
			public float evaluation = 0f;          // Centipawn evaluation from white's perspective
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

			try
			{
				string enginePath = GetEngineExecutablePath();
				if (string.IsNullOrEmpty(enginePath))
				{
					UnityEngine.Debug.LogError("[Stockfish] Engine executable not found in StreamingAssets/sf-engine.exe");
					return;
				}

				StartEngineProcess(enginePath);
				StartBackgroundReader();

				if (enableDebugLogging)
					UnityEngine.Debug.Log("[Stockfish] Engine started successfully");
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogError($"[Stockfish] Failed to start engine: {e.Message}");
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

			try
			{
				// Try graceful shutdown first
				if (engineProcess != null && !engineProcess.HasExited)
				{
					SendCommand("quit");

					if (!engineProcess.WaitForExit(2000))
					{
						if (enableDebugLogging)
							UnityEngine.Debug.Log("[Stockfish] Forcing engine termination");
						engineProcess.Kill();
					}
				}
			}
			catch (Exception e)
			{
				if (enableDebugLogging)
					UnityEngine.Debug.LogWarning($"[Stockfish] Exception during engine shutdown: {e.Message}");
			}
			finally
			{
				// Cleanup
				if (readerThread != null)
				{
					readerThread.Join(1000);
					readerThread = null;
				}

				if (engineProcess != null)
				{
					engineProcess.Dispose();
					engineProcess = null;
				}

				CleanupTempFile();

				// Reset crash detection
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

			// Validate FEN format first
			string fenValidationError = ValidateFen(fen);
			if (!string.IsNullOrEmpty(fenValidationError))
			{
				LastAnalysisResult.bestMove = $"ERROR: {fenValidationError}";
				LastAnalysisResult.errorMessage = fenValidationError;
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
					string errorMsg = "Engine crashed and restart failed";
					LastAnalysisResult.bestMove = $"ERROR: {errorMsg}";
					LastAnalysisResult.errorMessage = errorMsg;
					LastRawOutput = $"ERROR: {errorMsg}";
					yield break;
				}
			}

			if (!IsEngineRunning)
			{
				string errorMsg = "Engine not running. Call StartEngine() first.";
				LastAnalysisResult.bestMove = $"ERROR: {errorMsg}";
				LastAnalysisResult.errorMessage = errorMsg;
				LastRawOutput = $"ERROR: {errorMsg}";
				UnityEngine.Debug.LogError("[Stockfish] " + errorMsg);
				yield break;
			}

			bool requestTimedOut = false;
			bool requestAborted = false;
			float startTime = Time.time;
			float timeoutSeconds = (movetimeMs > 0 ? movetimeMs + 5000 : defaultTimeoutMs) / 1000f;

			// Setup request tracking
			lock (requestLock)
			{
				currentRequestOutput.Clear();
				waitingForBestMove = true;
				currentRequestCompleted = false;
			}

			// Send ucinewgame before each analysis to ensure clean state
			SendCommand("ucinewgame");

			// Configure engine weakness settings
			ConfigureEngineWeakness(elo, skillLevel);

			// Set position
			if (fen == "startpos" || string.IsNullOrEmpty(fen))
			{
				SendCommand("position startpos");
			}
			else
			{
				SendCommand($"position fen {fen}");
			}

			// Brief pause to let engine process position
			yield return new WaitForSeconds(0.1f);

			// Check if engine crashed during position setup
			crashDetected = DetectAndHandleCrash();
			if (crashDetected)
			{
				string errorMsg = $"Engine crashed while processing position: {fen}";
				LastAnalysisResult.bestMove = $"ERROR: {errorMsg}";
				LastAnalysisResult.errorMessage = errorMsg;
				LastRawOutput = $"ERROR: {errorMsg}";
				yield break;
			}

			// Check if any of the SendCommand calls failed by checking engine state
			if (!IsEngineRunning)
			{
				string errorMsg = "Engine died during position setup";
				LastAnalysisResult.bestMove = $"ERROR: {errorMsg}";
				LastAnalysisResult.errorMessage = errorMsg;
				LastRawOutput = $"ERROR: {errorMsg}";
				yield break;
			}

			// Construct and send go command
			string goCommand = ConstructGoCommand(depth, movetimeMs);
			SendCommand(goCommand);

			// Final check after sending go command
			if (!IsEngineRunning)
			{
				string errorMsg = "Engine died during go command";
				LastAnalysisResult.bestMove = $"ERROR: {errorMsg}";
				LastAnalysisResult.errorMessage = errorMsg;
				LastRawOutput = $"ERROR: {errorMsg}";
				yield break;
			}

			// Wait for completion or timeout - YIELDS OUTSIDE ANY TRY/CATCH
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

			// Handle error cases first
			if (requestTimedOut)
			{
				SendCommand("stop");  // Try to stop the search
				string errorMsg = $"Request timed out after {timeoutSeconds}s";
				LastAnalysisResult.bestMove = $"ERROR: {errorMsg}";
				LastAnalysisResult.errorMessage = errorMsg;
				LastRawOutput = $"ERROR: {errorMsg}";
				if (enableDebugLogging)
					UnityEngine.Debug.LogWarning("[Stockfish] Request timed out");
				yield break;
			}

			if (requestAborted)
			{
				string errorMsg = $"Engine crashed during analysis of position: {fen}";
				LastAnalysisResult.bestMove = $"ERROR: {errorMsg}";
				LastAnalysisResult.errorMessage = errorMsg;
				LastRawOutput = $"ERROR: {errorMsg}";
				if (enableDebugLogging)
					UnityEngine.Debug.LogError("[Stockfish] Request aborted due to engine crash");
				yield break;
			}

			// Parse successful result
			ParseAnalysisResult(LastRawOutput);
		}

		/// <summary>
		/// Legacy method - now calls AnalyzePositionCoroutine for backward compatibility
		/// </summary>
		public IEnumerator GetNextMoveCoroutine(string fen, int movetimeMs = 2000, int depth = -1, int elo = -1, int skillLevel = -1)
		{
			// Convert legacy parameters to new defaults
			int finalDepth = depth == -1 ? 1 : depth;
			int finalElo = elo == -1 ? 400 : elo;
			int finalSkillLevel = skillLevel == -1 ? 0 : skillLevel;

			yield return StartCoroutine(AnalyzePositionCoroutine(fen, movetimeMs, finalDepth, finalElo, finalSkillLevel));
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

			// Check if process and streams are still valid
			if (engineProcess != null && !engineProcess.HasExited &&
				engineProcess.StandardInput != null && engineProcess.StandardInput.BaseStream.CanWrite)
			{
				try
				{
					engineProcess.StandardInput.WriteLine(command);
					engineProcess.StandardInput.Flush();

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
			else
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
		/// Validate FEN format and ensure exactly one king per side
		/// </summary>
		private string ValidateFen(string fen)
		{
			if (fen == "startpos" || string.IsNullOrEmpty(fen))
				return "";  // startpos is always valid

			try
			{
				string[] parts = fen.Trim().Split(' ');
				if (parts.Length < 1)
					return "Invalid FEN: missing position data";

				string position = parts[0];
				int whiteKings = 0;
				int blackKings = 0;

				foreach (char c in position)
				{
					if (c == 'K') whiteKings++;
					else if (c == 'k') blackKings++;
				}

				if (whiteKings != 1)
					return $"Invalid FEN: found {whiteKings} white kings, expected exactly 1";

				if (blackKings != 1)
					return $"Invalid FEN: found {blackKings} black kings, expected exactly 1";

				return "";  // Valid
			}
			catch (Exception e)
			{
				return $"Invalid FEN format: {e.Message}";
			}
		}

		/// <summary>
		/// Parse engine output into structured analysis result
		/// </summary>
		private void ParseAnalysisResult(string rawOutput)
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
			float lastEvaluation = 0f;
			bool foundEvaluation = false;

			// Parse info lines for evaluation
			foreach (string line in lines)
			{
				string trimmedLine = line.Trim();

				if (trimmedLine.StartsWith("info") && trimmedLine.Contains("score"))
				{
					float evaluation = ParseEvaluationFromInfoLine(trimmedLine);
					if (!float.IsNaN(evaluation))
					{
						lastEvaluation = evaluation;
						foundEvaluation = true;
					}
				}
				else if (trimmedLine.StartsWith("bestmove"))
				{
					bestMoveLine = trimmedLine;
				}
			}

			// Set evaluation
			if (foundEvaluation)
			{
				LastAnalysisResult.evaluation = lastEvaluation;
			}

			// Parse best move line
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
						// In minimal depth analysis, we need to infer from evaluation
						if (foundEvaluation && (Mathf.Abs(lastEvaluation) > 9000))
						{
							LastAnalysisResult.bestMove = "check-mate";
							LastAnalysisResult.isGameEnd = true;
						}
						else
						{
							LastAnalysisResult.bestMove = "stale-mate";
							LastAnalysisResult.isGameEnd = true;
						}
					}
					else
					{
						// Normal move
						LastAnalysisResult.bestMove = move;
						LastAnalysisResult.isGameEnd = false;
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
		}

		/// <summary>
		/// Parse evaluation from UCI info line (centipawns from white's perspective)
		/// </summary>
		private float ParseEvaluationFromInfoLine(string infoLine)
		{
			try
			{
				// Look for "score cp" or "score mate"
				int scoreIndex = infoLine.IndexOf("score");
				if (scoreIndex == -1) return float.NaN;

				string scorePart = infoLine.Substring(scoreIndex);
				string[] tokens = scorePart.Split(' ');

				for (int i = 0; i < tokens.Length - 1; i++)
				{
					if (tokens[i] == "cp" && i + 1 < tokens.Length)
					{
						if (int.TryParse(tokens[i + 1], out int centipawns))
						{
							return centipawns;  // Already in centipawns
						}
					}
					else if (tokens[i] == "mate" && i + 1 < tokens.Length)
					{
						if (int.TryParse(tokens[i + 1], out int mateIn))
						{
							// Convert mate-in-N to a large evaluation
							return mateIn > 0 ? 10000 - mateIn : -10000 - mateIn;
						}
					}
				}
			}
			catch (Exception e)
			{
				if (enableDebugLogging)
					UnityEngine.Debug.LogWarning($"[Stockfish] Error parsing evaluation: {e.Message}");
			}

			return float.NaN;
		}

		/// <summary>
		/// Check if the engine has crashed and attempt recovery
		/// </summary>
		private bool DetectAndHandleCrash()
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
		/// Restart the engine after a crash
		/// </summary>
		private IEnumerator RestartEngineCoroutine()
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
			try
			{
				// Try to execute from StreamingAssets first
				return streamingAssetsPath;
			}
			catch
			{
				// If that fails, copy to temp directory
				return CopyToTempLocation(streamingAssetsPath);
			}
#else
			return streamingAssetsPath;
#endif
		}

		private string CopyToTempLocation(string sourcePath)
		{
			try
			{
				tempEnginePath = Path.Combine(Path.GetTempPath(), $"sf-engine-{System.Guid.NewGuid():N}.exe");
				File.Copy(sourcePath, tempEnginePath, true);

				if (enableDebugLogging)
					UnityEngine.Debug.Log($"[Stockfish] Copied engine to temp location: {tempEnginePath}");

				return tempEnginePath;
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogError($"[Stockfish] Failed to copy engine to temp location: {e.Message}");
				return null;
			}
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
			try
			{
				string line;
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
		}

		#endregion
	}
}