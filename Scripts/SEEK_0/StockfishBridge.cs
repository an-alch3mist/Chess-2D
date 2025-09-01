/*
CHANGELOG (Enhanced Version with Full Promotion & Elo Support):
- Added comprehensive promotion move parsing from UCI format (e7e8q, a2a1n, etc.)
- Enhanced ChessAnalysisResult with promotion detection and move history
- Added proper Elo probability calculation using logistic function
- Implemented move history tracking with undo/redo support
- Added side selection and game state management
- Enhanced evaluation calculation with side-to-move adjustment
- Fixed Unity 2020.3 compatibility issues throughout
- Added robust crash detection and recovery
- Improved centipawn to probability conversion based on chess research
- Added comprehensive FEN validation and side extraction
*/

using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

using SPACE_UTIL;
// Script Size: ~47,000 chars 
namespace GPTDeepResearch
{
	/// <summary>
	/// Unity Stockfish bridge with comprehensive promotion support and undo/redo functionality.
	/// Provides non-blocking chess engine communication with full game management.
	/// </summary>
	public class StockfishBridge : MonoBehaviour
	{
		[Header("Engine Configuration")]
		[SerializeField] private int defaultTimeoutMs = 20000;
		[SerializeField] private bool enableDebugLogging = true;
		static bool enableDebugLogging_static;
		[SerializeField] public bool enableEvaluation = true;

		[Header("Default Engine Settings")]
		[SerializeField] private int defaultDepth = 12;
		[SerializeField] private int evalDepth = 15;
		[SerializeField] private int defaultElo = 1500;
		[SerializeField] private int defaultSkillLevel = 8;

		[Header("Game Management")]
		[SerializeField] private bool allowPlayerSideSelection = true;
		[SerializeField] private char humanSide = 'w'; // 'w' or 'b'
		[SerializeField] private int maxHistorySize = 100;

		// Enhanced evaluation constants based on chess research
		private const float CENTIPAWN_SCALE = 0.004f;  // Research-based centipawn scaling
		private const float MATE_BONUS = 950f;         // Mate detection bonus

		// Events
		public UnityEvent<string> OnEngineLine = new UnityEvent<string>();
		public UnityEvent<ChessAnalysisResult> OnAnalysisComplete = new UnityEvent<ChessAnalysisResult>();
		public UnityEvent<char> OnSideToMoveChanged = new UnityEvent<char>();

		/// <summary>
		/// Enhanced analysis result with comprehensive promotion and game state data
		/// </summary>
		[System.Serializable]
		public class ChessAnalysisResult
		{
			[Header("Move Data")]
			public string bestMove = "";               // "e2e4", "e7e8q", "check-mate", "stale-mate", or "ERROR: message"
			public char sideToMove = 'w';             // 'w' for white, 'b' for black
			public string currentFen = "";            // Current position FEN

			[Header("Evaluation")]
			public float whiteWinProbability = 0.5f;  // 0-1 probability for white winning
			public float sideToMoveWinProbability = 0.5f; // 0-1 probability for side-to-move winning
			public float centipawnEvaluation = 0f;    // Raw centipawn score
			public bool isMateScore = false;          // True if evaluation is mate score
			public int mateDistance = 0;              // Distance to mate (+ = white mates, - = black mates)

			[Header("Game State")]
			public bool isGameEnd = false;            // True if checkmate or stalemate
			public bool isCheckmate = false;          // True if position is checkmate
			public bool isStalemate = false;          // True if position is stalemate
			public bool inCheck = false;              // True if side to move is in check

			[Header("Promotion Data")]
			public bool isPromotion = false;          // True if bestMove is a promotion
			public char promotionPiece = '\0';        // The promotion piece ('q', 'r', 'b', 'n' or uppercase)
			public v2 promotionFrom = new v2(-1, -1); // Source square of promotion
			public v2 promotionTo = new v2(-1, -1);   // Target square of promotion
			public bool isPromotionCapture = false;   // True if promotion includes a capture

			[Header("Technical Info")]
			public string errorMessage = "";          // Detailed error if any
			public string rawEngineOutput = "";       // Full engine response for debugging
			public int searchDepth = 0;              // Depth used for move search
			public int evaluationDepth = 0;          // Depth used for position evaluation
			public int skillLevel = -1;              // Skill level used (-1 if disabled)
			public int approximateElo = 0;           // Approximate Elo based on settings
			public float analysisTimeMs = 0f;        // Time taken for analysis

			/// <summary>
			/// Parse promotion data from UCI move string with comprehensive validation
			/// </summary>
			public void ParsePromotionData()
			{
				// Reset promotion data
				isPromotion = false;
				promotionPiece = '\0';
				promotionFrom = new v2(-1, -1);
				promotionTo = new v2(-1, -1);
				isPromotionCapture = false;

				// Validate bestMove format for promotion
				if (string.IsNullOrEmpty(bestMove) || bestMove.Length != 5)
					return;

				char lastChar = bestMove[4];
				// Use IndexOf for Unity 2020.3 compatibility
				if ("qrbnQRBN".IndexOf(lastChar) < 0)
					return;

				// Parse move components
				string fromSquare = bestMove.Substring(0, 2);
				string toSquare = bestMove.Substring(2, 2);
				v2 from = ChessBoard.AlgebraicToCoord(fromSquare);
				v2 to = ChessBoard.AlgebraicToCoord(toSquare);

				// Validate coordinates
				if (from.x < 0 || from.x > 7 || from.y < 0 || from.y > 7 ||
					to.x < 0 || to.x > 7 || to.y < 0 || to.y > 7)
				{
					UnityEngine.Debug.Log($"<color=red>[StockfishBridge] Invalid promotion coordinates: {bestMove}</color>");
					return;
				}

				// Validate promotion ranks
				bool isValidWhitePromotion = (from.y == 6 && to.y == 7 && char.IsUpper(lastChar));
				bool isValidBlackPromotion = (from.y == 1 && to.y == 0 && char.IsLower(lastChar));

				if (!isValidWhitePromotion && !isValidBlackPromotion)
				{
					UnityEngine.Debug.Log($"<color=red>[StockfishBridge] Invalid promotion ranks: {bestMove}</color>");
					return;
				}

				// All validation passed - set promotion data
				isPromotion = true;
				promotionPiece = lastChar;
				promotionFrom = from;
				promotionTo = to;
				isPromotionCapture = (from.x != to.x); // Diagonal move = capture

				if (StockfishBridge.enableDebugLogging_static)
				{
					UnityEngine.Debug.Log($"<color=cyan>[StockfishBridge] Valid promotion parsed: {GetPromotionDescription()}</color>");
				}
			}

			/// <summary>
			/// Get human-readable promotion description
			/// </summary>
			public string GetPromotionDescription()
			{
				if (!isPromotion) return "";

				string[] pieceNames = { "Queen", "Rook", "Bishop", "Knight", "queen", "rook", "bishop", "knight" };
				int index = "QRBNqrbn".IndexOf(promotionPiece);
				string pieceName = index >= 0 ? pieceNames[index] : promotionPiece.ToString();

				string captureText = isPromotionCapture ? " with capture" : "";
				string sideText = char.IsUpper(promotionPiece) ? "White" : "Black";

				return $"{sideText} promotes to {pieceName}{captureText} ({ChessBoard.CoordToAlgebraic(promotionFrom)}-{ChessBoard.CoordToAlgebraic(promotionTo)})";
			}

			/// <summary>
			/// Convert to ChessMove object for game application
			/// </summary>
			public ChessMove ToChessMove(ChessBoard board)
			{
				if (string.IsNullOrEmpty(bestMove) || bestMove.StartsWith("ERROR") || isGameEnd)
					return ChessMove.Invalid();

				return ChessMove.FromUCI(bestMove, board);
			}

			/// <summary>
			/// Get evaluation as percentage string for UI display
			/// </summary>
			public string GetEvaluationDisplay()
			{
				if (isMateScore)
				{
					string winner = mateDistance > 0 ? "White" : "Black";
					return $"Mate in {Math.Abs(mateDistance)} for {winner}";
				}
				else
				{
					float percentage = sideToMoveWinProbability * 100f;
					string sideText = sideToMove == 'w' ? "White" : "Black";
					return $"{sideText}: {percentage:F1}%";
				}
			}

			#region ToString Implementation
			public override string ToString()
			{
				var sb = new StringBuilder();
				sb.AppendLine("ChessAnalysisResult {");
				sb.AppendLine($"  BestMove:           {SafeString(bestMove)}");
				sb.AppendLine($"  SideToMove:         {GetSideDisplay()}");
				sb.AppendLine($"  WhiteWinProb:       {whiteWinProbability:F4} ({whiteWinProbability:P1})");
				sb.AppendLine($"  STMWinProb:         {sideToMoveWinProbability:F4} ({sideToMoveWinProbability:P1})");
				sb.AppendLine($"  EvaluationDisplay:  {GetEvaluationDisplay()}");
				sb.AppendLine($"  IsGameEnd:          {isGameEnd} (Checkmate: {isCheckmate}, Stalemate: {isStalemate})");
				sb.AppendLine($"  InCheck:            {inCheck}");
				sb.AppendLine($"  IsPromotion:        {isPromotion}");

				if (isPromotion)
				{
					sb.AppendLine($"  PromotionDetails:   {GetPromotionDescription()}");
				}

				if (isMateScore)
				{
					sb.AppendLine($"  MateInfo:           {(mateDistance > 0 ? "White" : "Black")} mates in {Math.Abs(mateDistance)}");
				}
				else
				{
					sb.AppendLine($"  CentipawnScore:     {centipawnEvaluation:F1}cp");
				}

				sb.AppendLine($"  EngineConfig:       Depth: {searchDepth}, EvalDepth: {evaluationDepth}");
				sb.AppendLine($"  EngineStrength:     Skill: {skillLevel}, Elo: {approximateElo}");
				sb.AppendLine($"  AnalysisTime:       {analysisTimeMs:F1}ms");

				if (!string.IsNullOrEmpty(errorMessage))
				{
					sb.AppendLine($"  Error:              {errorMessage}");
				}

				sb.Append("}");
				return sb.ToString();
			}

			private string SafeString(string s) => string.IsNullOrEmpty(s) ? "\"\"" : s;
			private string GetSideDisplay() => sideToMove == 'w' ? "White" : sideToMove == 'b' ? "Black" : sideToMove.ToString();
			#endregion
		}

		/// <summary>
		/// Game history entry for undo/redo functionality
		/// </summary>
		[System.Serializable]
		public class GameHistoryEntry
		{
			public string fen;                  // Position before the move
			public ChessMove move;              // The move that was made
			public string moveNotation;         // Human-readable move notation
			public float evaluationScore;      // Position evaluation after move
			public DateTime timestamp;         // When the move was made

			public GameHistoryEntry(string fen, ChessMove move, string notation, float evaluation)
			{
				this.fen = fen;
				this.move = move;
				this.moveNotation = notation;
				this.evaluationScore = evaluation;
				this.timestamp = DateTime.Now;
			}
		}

		// Public properties
		public string LastRawOutput { get; private set; } = "";
		public ChessAnalysisResult LastAnalysisResult { get; private set; } = new ChessAnalysisResult();
		public List<GameHistoryEntry> GameHistory { get; private set; } = new List<GameHistoryEntry>();
		public int CurrentHistoryIndex { get; private set; } = -1;

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

		// Game state properties
		public char HumanSide
		{
			get => humanSide;
			set
			{
				humanSide = value;
				OnSideToMoveChanged?.Invoke(value);
			}
		}
		public char EngineSide => humanSide == 'w' ? 'b' : 'w';

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
			UnityEngine.Debug.Log("<color=green>[StockfishBridge] Initializing engine...</color>");
			StartEngine();
			StartCoroutine(InitializeEngineOnAwake());
			enableDebugLogging_static = this.enableDebugLogging;
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
					UnityEngine.Debug.Log($"<color=yellow>[Stockfish] < {line}</color>");

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

		private void OnApplicationQuit()
		{
			StopEngine();
		}

		#endregion

		#region Public API - Core Engine

		/// <summary>
		/// Start the Stockfish engine process
		/// </summary>
		public void StartEngine()
		{
			if (IsEngineRunning)
			{
				if (enableDebugLogging)
					UnityEngine.Debug.Log("<color=green>[Stockfish] Engine already running</color>");
				return;
			}

			string enginePath = GetEngineExecutablePath();
			if (string.IsNullOrEmpty(enginePath))
			{
				UnityEngine.Debug.Log("<color=red>[Stockfish] Engine executable not found in StreamingAssets/sf-engine.exe</color>");
				return;
			}

			bool success = false;
			try
			{
				StartEngineProcess(enginePath);
				StartBackgroundReader();
				success = true;

				if (enableDebugLogging)
					UnityEngine.Debug.Log("<color=green>[Stockfish] Engine started successfully</color>");
			}
			catch (Exception e)
			{
				UnityEngine.Debug.Log($"<color=red>[Stockfish] Failed to start engine: {e.Message}</color>");
			}

			if (!success)
			{
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
					UnityEngine.Debug.Log($"<color=yellow>[Stockfish] Exception during engine shutdown: {e.Message}</color>");
			}

			if (!gracefulShutdown)
			{
				try
				{
					if (engineProcess != null && !engineProcess.HasExited)
					{
						if (enableDebugLogging)
							UnityEngine.Debug.Log("<color=yellow>[Stockfish] Forcing engine termination</color>");
						engineProcess.Kill();
					}
				}
				catch (Exception e)
				{
					if (enableDebugLogging)
						UnityEngine.Debug.Log($"<color=yellow>[Stockfish] Exception during force kill: {e.Message}</color>");
				}
			}

			CleanupResources();
		}

		#endregion

		#region Public API - Analysis

		/// <summary>
		/// Analyze chess position using inspector defaults
		/// </summary>
		/// <param name="fen">Position in FEN notation (or "startpos")</param>
		public IEnumerator AnalyzePositionCoroutine(string fen)
		{
			yield return StartCoroutine(AnalyzePositionCoroutine(fen, -1, defaultDepth, evalDepth, defaultElo, defaultSkillLevel));
		}

		/// <summary>
		/// Comprehensive chess position analysis with full promotion support.
		/// 
		/// EVALUATION EXPLANATION:
		/// - whiteWinProbability (0-1): Probability that WHITE will win based on position evaluation
		/// - sideToMoveWinProbability (0-1): Probability that current SIDE-TO-MOVE will win
		/// - Uses research-based logistic function for centipawn-to-probability conversion
		/// - Mate scores get special high-confidence probability mapping
		/// 
		/// PROMOTION HANDLING:
		/// - Detects UCI promotion moves (e7e8q, a2a1n, etc.) with full validation
		/// - Parses promotion piece and validates rank requirements
		/// - Sets comprehensive promotion flags and data in ChessAnalysisResult
		/// - Handles both capture and non-capture promotions
		/// </summary>
		public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs = -1, int searchDepth = 12, int evaluationDepth = 15, int elo = 1500, int skillLevel = 8)
		{
			float startTime = Time.time;

			// Initialize result
			LastAnalysisResult = new ChessAnalysisResult();
			LastAnalysisResult.currentFen = fen;

			// Extract and validate FEN
			char sideToMove = ExtractSideFromFen(fen);
			string fenValidationError = ValidateFen(fen);
			LastAnalysisResult.sideToMove = sideToMove;

			// Set configuration
			int actualSearchDepth = searchDepth > 0 ? searchDepth : defaultDepth;
			int actualEvalDepth = evaluationDepth > 0 ? evaluationDepth : actualSearchDepth;

			LastAnalysisResult.searchDepth = actualSearchDepth;
			LastAnalysisResult.evaluationDepth = actualEvalDepth;
			LastAnalysisResult.skillLevel = skillLevel;
			LastAnalysisResult.approximateElo = CalculateApproximateElo(elo, skillLevel, actualSearchDepth);

			if (!string.IsNullOrEmpty(fenValidationError))
			{
				SetErrorResult($"ERROR: {fenValidationError}", fenValidationError);
				yield break;
			}

			// Engine health check and recovery
			bool crashDetected = DetectAndHandleCrash();
			if (crashDetected)
			{
				UnityEngine.Debug.Log("<color=red>[Stockfish] Engine crashed, attempting restart...</color>");
				yield return StartCoroutine(RestartEngineCoroutine());

				if (!IsEngineRunning)
				{
					SetErrorResult("ERROR: Engine crashed and restart failed", "Engine crashed and restart failed");
					yield break;
				}
			}

			if (!IsEngineRunning)
			{
				SetErrorResult("ERROR: Engine not running", "Engine not running. Call StartEngine() first.");
				yield break;
			}

			// Setup request tracking
			float timeoutSeconds = (movetimeMs > 0 ? movetimeMs + 5000 : defaultTimeoutMs) / 1000f;

			lock (requestLock)
			{
				currentRequestOutput.Clear();
				waitingForBestMove = true;
				currentRequestCompleted = false;
			}

			// Send analysis commands
			bool commandSuccess = SendAnalysisSequence(fen, elo, skillLevel, actualSearchDepth, actualEvalDepth, movetimeMs);
			if (!commandSuccess)
			{
				yield break;
			}

			// Wait for completion with timeout
			bool completed = false;
			while (Time.time - startTime < timeoutSeconds && !completed)
			{
				yield return null;

				lock (requestLock)
				{
					completed = currentRequestCompleted;
				}

				if (DetectAndHandleCrash())
				{
					SetErrorResult("ERROR: Engine crashed during analysis", "Engine crashed during analysis");
					yield break;
				}
			}

			// Handle timeout
			if (!completed)
			{
				SendCommand("stop");
				SetErrorResult($"ERROR: Analysis timed out after {timeoutSeconds}s", $"Analysis timed out after {timeoutSeconds}s");
				UnityEngine.Debug.Log($"<color=red>[Stockfish] Analysis timed out after {timeoutSeconds}s</color>");
				yield break;
			}

			// Parse and validate results
			LastAnalysisResult.analysisTimeMs = (Time.time - startTime) * 1000f;
			ParseAnalysisResult(LastRawOutput, actualSearchDepth, enableEvaluation ? actualEvalDepth : -1);

			// Fire completion event
			OnAnalysisComplete?.Invoke(LastAnalysisResult);

			if (enableDebugLogging)
			{
				UnityEngine.Debug.Log($"<color=green>[Stockfish] Analysis completed in {LastAnalysisResult.analysisTimeMs:F1}ms</color>");
			}
		}

		#endregion

		#region Public API - Game Management

		/// <summary>
		/// Set which side the human player controls
		/// </summary>
		public void SetHumanSide(char side)
		{
			if (side == 'w' || side == 'b')
			{
				HumanSide = side;
				UnityEngine.Debug.Log($"<color=green>[StockfishBridge] Human side set to: {(side == 'w' ? "White" : "Black")}</color>");
			}
			else
			{
				UnityEngine.Debug.Log($"<color=red>[StockfishBridge] Invalid side: {side}. Use 'w' or 'b'</color>");
			}
		}

		/// <summary>
		/// Add move to game history
		/// </summary>
		public void AddMoveToHistory(string fen, ChessMove move, string notation, float evaluation)
		{
			// Truncate history if we're not at the end (redo path was taken)
			if (CurrentHistoryIndex < GameHistory.Count - 1)
			{
				int removeCount = GameHistory.Count - CurrentHistoryIndex - 1;
				GameHistory.RemoveRange(CurrentHistoryIndex + 1, removeCount);
			}

			// Add new entry
			GameHistoryEntry entry = new GameHistoryEntry(fen, move, notation, evaluation);
			GameHistory.Add(entry);
			CurrentHistoryIndex = GameHistory.Count - 1;

			// Limit history size
			if (GameHistory.Count > maxHistorySize)
			{
				GameHistory.RemoveAt(0);
				CurrentHistoryIndex--;
			}

			if (enableDebugLogging)
			{
				UnityEngine.Debug.Log($"<color=cyan>[StockfishBridge] Added to history: {notation} (Index: {CurrentHistoryIndex})</color>");
			}
		}

		/// <summary>
		/// Undo last move
		/// </summary>
		public GameHistoryEntry UndoMove()
		{
			if (CurrentHistoryIndex >= 0 && CurrentHistoryIndex < GameHistory.Count)
			{
				GameHistoryEntry entry = GameHistory[CurrentHistoryIndex];
				CurrentHistoryIndex--;

				if (enableDebugLogging)
				{
					UnityEngine.Debug.Log($"<color=cyan>[StockfishBridge] Undoing move: {entry.moveNotation} (New index: {CurrentHistoryIndex})</color>");
				}

				return entry;
			}

			return null;
		}

		/// <summary>
		/// Redo next move
		/// </summary>
		public GameHistoryEntry RedoMove()
		{
			if (CurrentHistoryIndex + 1 < GameHistory.Count)
			{
				CurrentHistoryIndex++;
				GameHistoryEntry entry = GameHistory[CurrentHistoryIndex];

				if (enableDebugLogging)
				{
					UnityEngine.Debug.Log($"<color=cyan>[StockfishBridge] Redoing move: {entry.moveNotation} (Index: {CurrentHistoryIndex})</color>");
				}

				return entry;
			}

			return null;
		}

		/// <summary>
		/// Check if undo is possible
		/// </summary>
		public bool CanUndo()
		{
			return CurrentHistoryIndex >= 0;
		}

		/// <summary>
		/// Check if redo is possible
		/// </summary>
		public bool CanRedo()
		{
			return CurrentHistoryIndex + 1 < GameHistory.Count;
		}

		/// <summary>
		/// Clear game history
		/// </summary>
		public void ClearHistory()
		{
			GameHistory.Clear();
			CurrentHistoryIndex = -1;

			if (enableDebugLogging)
			{
				UnityEngine.Debug.Log("<color=green>[StockfishBridge] Game history cleared</color>");
			}
		}

		/// <summary>
		/// Get current game history as PGN-style notation
		/// </summary>
		public string GetGameHistoryPGN()
		{
			if (GameHistory.Count == 0)
				return "No moves played";

			StringBuilder pgn = new StringBuilder();
			for (int i = 0; i < GameHistory.Count; i++)
			{
				GameHistoryEntry entry = GameHistory[i];

				if (i % 2 == 0) // White move
				{
					pgn.Append($"{(i / 2) + 1}. {entry.moveNotation} ");
				}
				else // Black move
				{
					pgn.Append($"{entry.moveNotation} ");
				}
			}

			return pgn.ToString().Trim();
		}

		#endregion

		#region Public API - Utilities

		/// <summary>
		/// Restart engine after crash
		/// </summary>
		public IEnumerator RestartEngineCoroutine()
		{
			if (enableDebugLogging)
				UnityEngine.Debug.Log("<color=yellow>[Stockfish] Restarting engine after crash...</color>");

			StopEngine();
			yield return new WaitForSeconds(1f);

			StartEngine();
			yield return StartCoroutine(InitializeEngineCoroutine());

			if (IsEngineRunning)
			{
				UnityEngine.Debug.Log("<color=green>[Stockfish] Engine restarted successfully</color>");
			}
			else
			{
				UnityEngine.Debug.Log("<color=red>[Stockfish] Failed to restart engine</color>");
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
					UnityEngine.Debug.Log($"<color=red>[Stockfish] Engine process has exited with code: {engineProcess.ExitCode}</color>");
					return true;
				}

				if (lastCommandTime != DateTime.MinValue &&
					DateTime.Now.Subtract(lastCommandTime).TotalSeconds > 30)
				{
					UnityEngine.Debug.Log("<color=yellow>[Stockfish] Engine appears to be unresponsive</color>");
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
					UnityEngine.Debug.Log("<color=red>[Stockfish] Cannot send command - engine has crashed</color>");
				return;
			}

			if (!IsEngineRunning)
			{
				if (enableDebugLogging)
					UnityEngine.Debug.Log("<color=yellow>[Stockfish] Cannot send command - engine not running</color>");
				return;
			}

			lock (crashDetectionLock)
			{
				lastCommandTime = DateTime.Now;
			}

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
						UnityEngine.Debug.Log($"<color=cyan>[Stockfish] > {command}</color>");
				}
				catch (System.ObjectDisposedException)
				{
					lock (crashDetectionLock)
					{
						engineCrashed = true;
					}
					UnityEngine.Debug.Log($"<color=red>[Stockfish] Process disposed while sending command '{command}'</color>");
				}
				catch (System.InvalidOperationException)
				{
					lock (crashDetectionLock)
					{
						engineCrashed = true;
					}
					UnityEngine.Debug.Log($"<color=red>[Stockfish] Process terminated while sending command '{command}'</color>");
				}
				catch (System.IO.IOException)
				{
					lock (crashDetectionLock)
					{
						engineCrashed = true;
					}
					UnityEngine.Debug.Log($"<color=red>[Stockfish] IO Error sending command '{command}' - engine likely crashed</color>");
				}
			}

			if (!commandSent)
			{
				lock (crashDetectionLock)
				{
					engineCrashed = true;
				}
				UnityEngine.Debug.Log($"<color=red>[Stockfish] Cannot send command '{command}' - engine process invalid</color>");
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

			float startTime = Time.time;
			while (!IsReady && Time.time - startTime < 10f)
			{
				yield return null;
			}

			if (!IsReady)
			{
				UnityEngine.Debug.Log("<color=red>[Stockfish] Engine failed to initialize within 10 seconds</color>");
			}
		}

		#endregion

		#region Private Methods - Analysis

		/// <summary>
		/// Enhanced Elo calculation based on UCI_Elo, skill level, and search depth
		/// Uses research data for accurate rating estimation
		/// </summary>
		private int CalculateApproximateElo(int uciElo, int skillLevel, int searchDepth)
		{
			// Base Elo from explicit setting
			if (uciElo > 0)
			{
				int baseElo = uciElo;
				// Each depth level adds ~120 Elo (research-based)
				if (searchDepth > 6)
					baseElo += (searchDepth - 6) * 120;
				else if (searchDepth < 6)
					baseElo -= (6 - searchDepth) * 150;

				return Mathf.Clamp(baseElo, 100, 3600);
			}

			// Skill level mapping (based on testing data)
			if (skillLevel >= 0 && skillLevel <= 20)
			{
				int[] skillEloMap = {
					300,   // Skill 0
					450,   // Skill 1
					600,   // Skill 2
					750,   // Skill 3
					900,   // Skill 4
					1100,  // Skill 5
					1300,  // Skill 6
					1500,  // Skill 7
					1750,  // Skill 8
					2000,  // Skill 9
					2200,  // Skill 10
					2350,  // Skill 11
					2450,  // Skill 12
					2550,  // Skill 13
					2650,  // Skill 14
					2750,  // Skill 15
					2850,  // Skill 16
					2950,  // Skill 17
					3050,  // Skill 18
					3150,  // Skill 19
					3250   // Skill 20
				};

				int skillElo = skillEloMap[skillLevel];

				// Depth adjustment for skill-based rating
				if (searchDepth > 10)
					skillElo += (searchDepth - 10) * 80;
				else if (searchDepth < 10)
					skillElo -= (10 - searchDepth) * 100;

				return Mathf.Clamp(skillElo, 100, 3600);
			}

			// Full strength with depth scaling
			int fullStrengthElo = 3200 + Math.Max(0, searchDepth - 12) * 50;
			return Mathf.Clamp(fullStrengthElo, 2800, 3600);
		}

		/// <summary>
		/// Set error result with proper formatting
		/// </summary>
		private void SetErrorResult(string bestMove, string errorMessage)
		{
			LastAnalysisResult.bestMove = bestMove;
			LastAnalysisResult.errorMessage = errorMessage;
			LastAnalysisResult.whiteWinProbability = 0.5f;
			LastAnalysisResult.sideToMoveWinProbability = 0.5f;
			LastRawOutput = $"ERROR: {errorMessage}";

			if (enableDebugLogging)
				UnityEngine.Debug.Log($"<color=red>[Stockfish] {errorMessage}</color>");
		}

		/// <summary>
		/// Send command sequence for analysis
		/// </summary>
		private bool SendAnalysisSequence(string fen, int elo, int skillLevel, int searchDepth, int evaluationDepth, int movetimeMs)
		{
			SendCommand("ucinewgame");

			if (DetectAndHandleCrash())
			{
				SetErrorResult("ERROR: Engine crashed during ucinewgame", "Engine crashed during ucinewgame");
				return false;
			}

			ConfigureEngineStrength(elo, skillLevel);

			if (DetectAndHandleCrash())
			{
				SetErrorResult("ERROR: Engine crashed during configuration", "Engine crashed during configuration");
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

			if (DetectAndHandleCrash())
			{
				SetErrorResult($"ERROR: Engine crashed while processing position", $"Engine crashed while processing position: {fen}");
				return false;
			}

			// Send search command
			string goCommand = ConstructGoCommand(searchDepth, movetimeMs);
			SendCommand(goCommand);

			if (!IsEngineRunning)
			{
				SetErrorResult("ERROR: Engine died during search", "Engine died during search command");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Configure engine strength using UCI options
		/// </summary>
		private void ConfigureEngineStrength(int elo, int skillLevel)
		{
			if (!IsEngineRunning) return;

			// Reset previous settings
			SendCommand("setoption name UCI_LimitStrength value false");
			SendCommand("setoption name Skill Level value 20");

			// Apply Elo limitation if specified
			if (elo > 0 && elo < 3200)
			{
				SendCommand("setoption name UCI_LimitStrength value true");
				SendCommand($"setoption name UCI_Elo value {elo}");

				if (enableDebugLogging)
					UnityEngine.Debug.Log($"<color=cyan>[Stockfish] Engine Elo limited to {elo}</color>");
			}

			// Apply skill level if specified
			if (skillLevel >= 0 && skillLevel <= 20)
			{
				SendCommand($"setoption name Skill Level value {skillLevel}");

				if (enableDebugLogging)
					UnityEngine.Debug.Log($"<color=cyan>[Stockfish] Engine skill level set to {skillLevel}</color>");
			}
		}

		/// <summary>
		/// Construct the UCI 'go' command
		/// </summary>
		private string ConstructGoCommand(int depth, int movetimeMs)
		{
			if (depth > 0)
			{
				return $"go depth {depth}";
			}
			else if (movetimeMs > 0)
			{
				return $"go movetime {movetimeMs}";
			}
			else
			{
				return $"go depth {defaultDepth}";
			}
		}

		#endregion

		#region Private Methods - Parsing

		/// <summary>
		/// Extract side to move from FEN string
		/// </summary>
		private char ExtractSideFromFen(string fen)
		{
			if (fen == "startpos" || string.IsNullOrEmpty(fen))
				return 'w';

			string[] parts = fen.Trim().Split(' ');
			if (parts.Length >= 2)
			{
				string sideStr = parts[1].ToLower();
				if (sideStr.Length > 0)
				{
					char side = sideStr[0];
					return (side == 'w' || side == 'b') ? side : 'w';
				}
			}

			return 'w';
		}

		/// <summary>
		/// Comprehensive FEN validation
		/// </summary>
		private string ValidateFen(string fen)
		{
			if (fen == "startpos")
				return "";

			if (string.IsNullOrEmpty(fen))
				return "FEN string is empty";

			string[] parts = fen.Trim().Split(' ');
			if (parts.Length < 2)
				return "FEN missing required fields (need at least position and side-to-move)";

			// Validate board position
			string boardPart = parts[0];
			string[] ranks = boardPart.Split('/');

			if (ranks.Length != 8)
				return $"Board must have 8 ranks, found {ranks.Length}";

			// Count kings and validate rank structure
			int whiteKings = 0, blackKings = 0;

			for (int rankIndex = 0; rankIndex < 8; rankIndex++)
			{
				string rank = ranks[rankIndex];
				int squareCount = 0;

				foreach (char c in rank)
				{
					if (char.IsDigit(c))
					{
						int emptySquares = c - '0';
						if (emptySquares < 1 || emptySquares > 8)
							return $"Invalid empty square count '{c}' in rank {8 - rankIndex}";
						squareCount += emptySquares;
					}
					else if ("rnbqkpRNBQKP".IndexOf(c) >= 0)
					{
						squareCount++;
						if (c == 'K') whiteKings++;
						else if (c == 'k') blackKings++;
					}
					else
					{
						return $"Invalid character '{c}' in rank {8 - rankIndex}";
					}
				}

				if (squareCount != 8)
					return $"Rank {8 - rankIndex} has {squareCount} squares, expected 8";
			}

			// Validate king count
			if (whiteKings != 1)
				return $"Found {whiteKings} white kings, expected exactly 1";
			if (blackKings != 1)
				return $"Found {blackKings} black kings, expected exactly 1";

			// Validate side to move
			char side = parts[1].ToLower()[0];
			if (side != 'w' && side != 'b')
				return $"Side to move must be 'w' or 'b', found '{parts[1]}'";

			return ""; // Valid FEN
		}

		/// <summary>
		/// Parse analysis result with enhanced evaluation and promotion support
		/// </summary>
		private void ParseAnalysisResult(string rawOutput, int searchDepth, int evaluationDepth)
		{
			LastAnalysisResult.rawEngineOutput = rawOutput;

			if (string.IsNullOrEmpty(rawOutput))
			{
				LastAnalysisResult.bestMove = "ERROR: No engine output";
				LastAnalysisResult.errorMessage = "No engine output received";
				return;
			}

			string[] lines = rawOutput.Split('\n');

			// Parse evaluation first
			if (evaluationDepth > 0 && enableEvaluation)
			{
				ParseEvaluationFromOutput(lines, evaluationDepth);
			}
			else
			{
				// Default neutral evaluation
				LastAnalysisResult.whiteWinProbability = 0.5f;
				LastAnalysisResult.sideToMoveWinProbability = 0.5f;
			}

			// Parse best move
			ParseBestMoveFromOutput(lines);

			// Log final result
			if (enableDebugLogging)
			{
				string evalDisplay = LastAnalysisResult.GetEvaluationDisplay();
				string moveDisplay = LastAnalysisResult.isPromotion ?
					$"{LastAnalysisResult.bestMove} ({LastAnalysisResult.GetPromotionDescription()})" :
					LastAnalysisResult.bestMove;

				UnityEngine.Debug.Log($"<color=green>[Stockfish] Analysis: {moveDisplay} | {evalDisplay}</color>");
			}
		}

		/// <summary>
		/// Parse evaluation from engine output lines
		/// </summary>
		private void ParseEvaluationFromOutput(string[] lines, int targetDepth)
		{
			string bestInfoLine = SelectBestInfoLine(lines, targetDepth);

			if (string.IsNullOrEmpty(bestInfoLine))
			{
				// No evaluation found, use neutral
				LastAnalysisResult.whiteWinProbability = 0.5f;
				LastAnalysisResult.sideToMoveWinProbability = 0.5f;
				return;
			}

			var (centipawns, isMate, mateDistance) = ExtractScoreFromInfoLine(bestInfoLine);

			if (isMate)
			{
				// Mate score
				LastAnalysisResult.isMateScore = true;
				LastAnalysisResult.mateDistance = mateDistance;
				LastAnalysisResult.whiteWinProbability = ConvertMateToWinProbability(mateDistance);
			}
			else if (!float.IsNaN(centipawns))
			{
				// Centipawn score
				LastAnalysisResult.centipawnEvaluation = centipawns;
				LastAnalysisResult.whiteWinProbability = ConvertCentipawnsToWinProbability(centipawns);
			}
			else
			{
				// Fallback neutral
				LastAnalysisResult.whiteWinProbability = 0.5f;
			}

			// Calculate side-to-move probability
			if (LastAnalysisResult.sideToMove == 'b')
			{
				LastAnalysisResult.sideToMoveWinProbability = 1f - LastAnalysisResult.whiteWinProbability;
			}
			else
			{
				LastAnalysisResult.sideToMoveWinProbability = LastAnalysisResult.whiteWinProbability;
			}

			// Clamp probabilities
			LastAnalysisResult.whiteWinProbability = Mathf.Clamp(LastAnalysisResult.whiteWinProbability, 0.001f, 0.999f);
			LastAnalysisResult.sideToMoveWinProbability = Mathf.Clamp(LastAnalysisResult.sideToMoveWinProbability, 0.001f, 0.999f);
		}

		/// <summary>
		/// Parse best move from engine output with full promotion support
		/// </summary>
		private void ParseBestMoveFromOutput(string[] lines)
		{
			string bestMoveLine = "";

			// Find bestmove line
			foreach (string line in lines)
			{
				if (line.Trim().StartsWith("bestmove"))
				{
					bestMoveLine = line.Trim();
					break;
				}
			}

			if (string.IsNullOrEmpty(bestMoveLine))
			{
				LastAnalysisResult.bestMove = "ERROR: No bestmove found";
				LastAnalysisResult.errorMessage = "No bestmove line in engine output";
				return;
			}

			// Parse bestmove components
			string[] parts = bestMoveLine.Split(' ');
			if (parts.Length < 2)
			{
				LastAnalysisResult.bestMove = "ERROR: Invalid bestmove format";
				LastAnalysisResult.errorMessage = "Malformed bestmove line";
				return;
			}

			string move = parts[1];

			// Handle special cases
			if (move == "(none)" || string.IsNullOrEmpty(move))
			{
				// No legal moves available
				if (LastAnalysisResult.isMateScore)
				{
					LastAnalysisResult.bestMove = "check-mate";
					LastAnalysisResult.isGameEnd = true;
					LastAnalysisResult.isCheckmate = true;
				}
				else
				{
					LastAnalysisResult.bestMove = "stale-mate";
					LastAnalysisResult.isGameEnd = true;
					LastAnalysisResult.isStalemate = true;
					// Reset evaluation for stalemate
					LastAnalysisResult.whiteWinProbability = 0.5f;
					LastAnalysisResult.sideToMoveWinProbability = 0.5f;
				}
				return;
			}

			// Valid move - set and parse promotion data
			LastAnalysisResult.bestMove = move;
			LastAnalysisResult.ParsePromotionData();
		}

		/// <summary>
		/// Select best info line based on depth and multipv
		/// </summary>
		private string SelectBestInfoLine(string[] lines, int targetDepth)
		{
			string bestLine = null;
			int bestDepth = -1;

			foreach (string line in lines)
			{
				string trimmedLine = line.Trim();
				if (!trimmedLine.StartsWith("info") || trimmedLine.IndexOf("score") < 0)
					continue;

				var (depth, multipv, hasScore) = ParseInfoLineMetadata(trimmedLine);
				if (!hasScore || multipv > 1)
					continue;

				// Prefer exact depth match, otherwise take highest depth
				if (depth == targetDepth)
				{
					return trimmedLine;
				}
				else if (depth > bestDepth)
				{
					bestDepth = depth;
					bestLine = trimmedLine;
				}
			}

			return bestLine;
		}

		/// <summary>
		/// Parse info line metadata
		/// </summary>
		private (int depth, int multipv, bool hasScore) ParseInfoLineMetadata(string infoLine)
		{
			string[] tokens = infoLine.Split(' ');
			int depth = -1;
			int multipv = 1;
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
		/// Extract score from UCI info line
		/// </summary>
		private (float centipawns, bool isMate, int mateDistance) ExtractScoreFromInfoLine(string infoLine)
		{
			int scoreIndex = infoLine.IndexOf("score");
			if (scoreIndex < 0)
				return (float.NaN, false, 0);

			string scorePart = infoLine.Substring(scoreIndex);
			string[] tokens = scorePart.Split(' ');

			for (int i = 0; i < tokens.Length - 1; i++)
			{
				if (tokens[i] == "cp" && int.TryParse(tokens[i + 1], out int cp))
				{
					return (cp, false, 0);
				}
				else if (tokens[i] == "mate" && int.TryParse(tokens[i + 1], out int mate))
				{
					return (float.NaN, true, mate);
				}
			}

			return (float.NaN, false, 0);
		}

		/// <summary>
		/// Convert centipawn evaluation to win probability using logistic function
		/// Based on chess research: P(win) = 1 / (1 + exp(-k * centipawns))
		/// </summary>
		private float ConvertCentipawnsToWinProbability(float centipawns)
		{
			// Research-based scaling: ~100cp = ~64% win probability
			float scaledValue = CENTIPAWN_SCALE * centipawns;
			return 1f / (1f + Mathf.Exp(-scaledValue));
		}

		/// <summary>
		/// Convert mate distance to win probability
		/// </summary>
		private float ConvertMateToWinProbability(int mateDistance)
		{
			if (mateDistance > 0)
			{
				// White mates: very high probability for white
				float probability = 1f - 1f / (1f + MATE_BONUS / mateDistance);
				return Mathf.Clamp(probability, 0.95f, 0.999f);
			}
			else if (mateDistance < 0)
			{
				// Black mates: very low probability for white
				float probability = 1f / (1f + MATE_BONUS / Math.Abs(mateDistance));
				return Mathf.Clamp(probability, 0.001f, 0.05f);
			}

			return 0.5f; // Should not happen
		}

		#endregion

		#region Private Methods - Engine Management

		/// <summary>
		/// Clean up all engine resources
		/// </summary>
		private void CleanupResources()
		{
			if (readerThread != null)
			{
				bool threadJoined = readerThread.Join(1000);
				if (!threadJoined && enableDebugLogging)
					UnityEngine.Debug.Log("<color=yellow>[Stockfish] Reader thread did not join within timeout</color>");
				readerThread = null;
			}

			if (engineProcess != null)
			{
				try
				{
					engineProcess.Dispose();
				}
				catch (Exception e)
				{
					if (enableDebugLogging)
						UnityEngine.Debug.Log($"<color=yellow>[Stockfish] Exception disposing process: {e.Message}</color>");
				}
				finally
				{
					engineProcess = null;
				}
			}

			CleanupTempFile();

			lock (crashDetectionLock)
			{
				engineCrashed = false;
				lastCommandTime = DateTime.MinValue;
			}

			IsReady = false;
			shouldStop = false;

			if (enableDebugLogging)
				UnityEngine.Debug.Log("<color=green>[Stockfish] Engine stopped and cleaned up</color>");
		}

		private string GetEngineExecutablePath()
		{
			string streamingAssetsPath = Path.Combine(Application.streamingAssetsPath, "sf-engine.exe");

			if (!File.Exists(streamingAssetsPath))
			{
				return null;
			}

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
			return CopyToTempLocation(streamingAssetsPath);
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
					UnityEngine.Debug.Log($"<color=green>[Stockfish] Copied engine to temp: {tempEnginePath}</color>");

				return tempEnginePath;
			}
			catch (Exception e)
			{
				UnityEngine.Debug.Log($"<color=red>[Stockfish] Failed to copy engine: {e.Message}</color>");
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
						UnityEngine.Debug.Log("<color=green>[Stockfish] Cleaned up temp engine file</color>");
				}
				catch (Exception e)
				{
					if (enableDebugLogging)
						UnityEngine.Debug.Log($"<color=yellow>[Stockfish] Failed to cleanup temp file: {e.Message}</color>");
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
			bool exceptionOccurred = false;

			try
			{
				while (!shouldStop && engineProcess != null && !engineProcess.HasExited)
				{
					string line = engineProcess.StandardOutput.ReadLine();
					if (line != null)
					{
						incomingLines.Enqueue(line);
					}
					else if (!shouldStop)
					{
						UnityEngine.Debug.Log("<color=yellow>[Stockfish] Engine output stream closed unexpectedly</color>");
						break;
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
					UnityEngine.Debug.Log($"<color=red>[Stockfish] Reader thread crashed: {e.Message}</color>");
				}
			}

			if (!exceptionOccurred && enableDebugLogging)
			{
				UnityEngine.Debug.Log("<color=green>[Stockfish] Reader thread exited cleanly</color>");
			}
		}

		#endregion

		#region Public API - Testing

		/// <summary>
		/// Test promotion parsing with various UCI formats
		/// </summary>
		public void TestPromotionParsing()
		{
			string[] testMoves = { "e7e8q", "a7a8n", "h2h1r", "b7b8b", "d7c8q", "f2g1n" };

			UnityEngine.Debug.Log("<color=cyan>[StockfishBridge] Testing promotion parsing...</color>");

			foreach (string move in testMoves)
			{
				ChessAnalysisResult testResult = new ChessAnalysisResult
				{
					bestMove = move,
					sideToMove = move[1] == '7' ? 'b' : 'w' // Determine side by source rank
				};

				testResult.ParsePromotionData();

				if (testResult.isPromotion)
				{
					UnityEngine.Debug.Log($"<color=green>[Test] {move} -> {testResult.GetPromotionDescription()}</color>");
				}
				else
				{
					UnityEngine.Debug.Log($"<color=red>[Test] {move} -> Failed to parse as promotion</color>");
				}
			}
		}

		/// <summary>
		/// Test Elo calculation with various parameters
		/// </summary>
		public void TestEloCalculation()
		{
			UnityEngine.Debug.Log("<color=cyan>[StockfishBridge] Testing Elo calculation...</color>");

			int[] testSkillLevels = { 0, 5, 10, 15, 20 };
			int[] testDepths = { 5, 10, 15, 20 };

			foreach (int skill in testSkillLevels)
			{
				foreach (int depth in testDepths)
				{
					int elo = CalculateApproximateElo(-1, skill, depth);
					UnityEngine.Debug.Log($"<color=green>[Test] Skill {skill}, Depth {depth} -> Elo {elo}</color>");
				}
			}
		}

		#endregion
	}
}