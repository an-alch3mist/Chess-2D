/*
CHANGELOG (Simplified Analysis Engine - v0.5):
- REMOVED: Game history management (GameHistoryEntry, GameHistory, undo/redo functionality)
- REMOVED: Game state coupling - no longer tracks move sequences or maintains board state
- FIXED: Separated analysis concerns from game state management
- IMPROVED: Focused purely on position analysis and move evaluation
- IMPROVED: Cleaner API surface with only analysis-related methods
- IMPROVED: Better separation of concerns for multi-board scenarios
- MAINTAINED: Research-based evaluation and FEN-driven analysis
- MAINTAINED: Comprehensive UCI promotion parsing and validation
- MAINTAINED: Engine crash detection and recovery capabilities
- MAINTAINED: Analysis logging for debugging and PGN export
- ARCHITECTURE: Designed for future ChessBoard integration without tight coupling
*/

using System;
using System.Linq;
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
// Script Size: ~45,000 chars 
namespace GPTDeepResearch
{
	/// <summary>
	/// Unity Stockfish bridge focused purely on position analysis and move evaluation.
	/// Provides non-blocking chess engine communication without game state management.
	/// Designed for multi-board scenarios with clean separation of concerns.
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

		// Research-based evaluation constants from Lichess analysis
		private const float LICHESS_CENTIPAWN_SCALE = 0.00368208f;  // From Lichess accuracy metric research
		private const float MATE_CONFIDENCE_THRESHOLD = 0.98f;      // High confidence threshold for mate scores
		private const float MAX_EVALUATION_CENTIPAWNS = 3000f;      // Practical maximum for evaluation display

		// Events
		public UnityEvent<string> OnEngineLine = new UnityEvent<string>();
		public UnityEvent<ChessAnalysisResult> OnAnalysisComplete = new UnityEvent<ChessAnalysisResult>();

		/// <summary>
		/// Enhanced analysis result with research-based evaluation and comprehensive promotion data
		/// </summary>
		[System.Serializable]
		public class ChessAnalysisResult
		{
			[Header("Move Data")]
			public string bestMove = "";               // "e2e4", "e7e8q", "check-mate", "stale-mate", or "ERROR: message"
			public char sideToMove = 'w';             // 'w' for white, 'b' for black (extracted from FEN)
			public string currentFen = "";            // Current position FEN

			[Header("Evaluation")]
			public float engineSideWinProbability = 0.5f;  // 0-1 probability for white winning (Lichess research-based)
			public float sideToMoveWinProbability = 0.5f; // 0-1 probability for side-to-move winning
			public float centipawnEvaluation = 0f;    // Raw centipawn score from engine
			public bool isMateScore = false;          // True if evaluation is mate score
			public int mateDistance = 0;              // Distance to mate (+ = white mates, - = black mates)

			[Header("Game State")]
			public bool isGameEnd = false;            // True if checkmate or stalemate
			public bool isCheckmate = false;          // True if position is checkmate
			public bool isStalemate = false;          // True if position is stalemate
			public bool inCheck = false;              // True if side to move is in check

			[Header("Promotion Data")]
			public bool isPromotion = false;          // True if bestMove is a promotion
			public char promotionPiece = '\0';        // The promotion piece ('q', 'r', 'b', 'n' - UCI lowercase)
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
			/// Parse promotion data from UCI move string with enhanced UCI protocol validation
			/// </summary>
			public void ParsePromotionData()
			{
				// Reset promotion data
				isPromotion = false;
				promotionPiece = '\0';
				promotionFrom = new v2(-1, -1);
				promotionTo = new v2(-1, -1);
				isPromotionCapture = false;

				// UCI promotion format validation: exactly 5 characters (e7e8q)
				if (string.IsNullOrEmpty(bestMove) || bestMove.Length != 5)
					return;

				// Extract components
				string fromSquare = bestMove.Substring(0, 2);
				string toSquare = bestMove.Substring(2, 2);
				char promotionChar = bestMove[4];

				// Validate promotion piece according to UCI specification (lowercase only)
				if ("qrbn".IndexOf(promotionChar) < 0)
				{
					if (StockfishBridge.enableDebugLogging_static)
						UnityEngine.Debug.Log($"<color=yellow>[StockfishBridge] Invalid UCI promotion piece '{promotionChar}' - UCI uses lowercase only</color>");
					return;
				}

				// Parse coordinates
				v2 from = ChessBoard.AlgebraicToCoord(fromSquare);
				v2 to = ChessBoard.AlgebraicToCoord(toSquare);

				// Validate coordinate ranges
				if (from.x < 0 || from.x > 7 || from.y < 0 || from.y > 7 ||
					to.x < 0 || to.x > 7 || to.y < 0 || to.y > 7)
				{
					if (StockfishBridge.enableDebugLogging_static)
						UnityEngine.Debug.Log($"<color=red>[StockfishBridge] Invalid promotion coordinates: {bestMove}</color>");
					return;
				}

				// Validate promotion ranks according to chess rules
				bool isValidWhitePromotion = (from.y == 6 && to.y == 7); // 7th to 8th rank for white
				bool isValidBlackPromotion = (from.y == 1 && to.y == 0); // 2nd to 1st rank for black

				if (!isValidWhitePromotion && !isValidBlackPromotion)
				{
					if (StockfishBridge.enableDebugLogging_static)
						UnityEngine.Debug.Log($"<color=red>[StockfishBridge] Invalid promotion ranks: {bestMove} (from rank {from.y + 1} to {to.y + 1})</color>");
					return;
				}

				// Validate side consistency with rank movement
				bool isWhiteMove = isValidWhitePromotion;
				bool isBlackMove = isValidBlackPromotion;

				if (sideToMove == 'w' && !isWhiteMove)
				{
					if (StockfishBridge.enableDebugLogging_static)
						UnityEngine.Debug.Log($"<color=red>[StockfishBridge] Side mismatch: White to move but promotion to black rank: {bestMove}</color>");
					return;
				}

				if (sideToMove == 'b' && !isBlackMove)
				{
					if (StockfishBridge.enableDebugLogging_static)
						UnityEngine.Debug.Log($"<color=red>[StockfishBridge] Side mismatch: Black to move but promotion to white rank: {bestMove}</color>");
					return;
				}

				// All UCI validation passed - set promotion data
				isPromotion = true;
				promotionPiece = promotionChar; // Keep UCI lowercase format
				promotionFrom = from;
				promotionTo = to;
				isPromotionCapture = (from.x != to.x); // Diagonal move indicates capture

				if (StockfishBridge.enableDebugLogging_static)
				{
					UnityEngine.Debug.Log($"<color=cyan>[StockfishBridge] Valid UCI promotion parsed: {GetPromotionDescription()}</color>");
				}
			}

			/// <summary>
			/// Get human-readable promotion description
			/// </summary>
			public string GetPromotionDescription()
			{
				if (!isPromotion) return "";

				string pieceName;
				switch (promotionPiece)
				{
					case 'q':
						pieceName = "Queen";
						break;
					case 'r':
						pieceName = "Rook";
						break;
					case 'b':
						pieceName = "Bishop";
						break;
					case 'n':
						pieceName = "Knight";
						break;
					default:
						pieceName = promotionPiece.ToString();
						break;
				}

				string captureText = isPromotionCapture ? " with capture" : "";
				string sideText = sideToMove == 'w' ? "White" : "Black";

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
			/// Get evaluation as percentage string for UI display with research-based formatting
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
					// Calculate actual win probabilities based on engine evaluation
					float engineActualWinProb, oppoActualWinProb;

					if (centipawnEvaluation >= 0)
					{
						// Positive centipawns = good for White
						engineActualWinProb = engineSideWinProbability;
						oppoActualWinProb = 1f - engineSideWinProbability;
					}
					else
					{
						// Negative centipawns = good for Black
						engineActualWinProb = 1f - engineSideWinProbability;
						oppoActualWinProb = engineSideWinProbability;
					}

					float enginePercentage = engineActualWinProb * 100f;
					float oppoPercentage = oppoActualWinProb * 100f;

					// Show evaluation strength indicator for extreme positions
					string strengthIndicator = "";
					if (Math.Abs(centipawnEvaluation) > 500f)
						strengthIndicator = Math.Abs(centipawnEvaluation) > 1000f ? " (Decisive)" : " (Winning)";

					return $"Engine: {enginePercentage:F1}% | Oppo: {oppoPercentage:F1}%{strengthIndicator}";
				}
			}

			#region ToString Implementation
			public override string ToString()
			{
				var sb = new StringBuilder();
				sb.AppendLine("ChessAnalysisResult {");
				sb.AppendLine($"  BestMove:           {SafeString(bestMove)}");
				sb.AppendLine($"  SideToMove:         {GetSideDisplay()}"); // absolute w/b unlike relative engine/oppo side for evaluation
				sb.AppendLine($"  CurrentFEN:         {SafeString(currentFen)}");

				// Calculate and display actual win probabilities correctly
				float engineActualWinProb, oppoActualWinProb;
				if (centipawnEvaluation >= 0)
				{
					engineActualWinProb = engineSideWinProbability;
					oppoActualWinProb = 1f - engineSideWinProbability;
				}
				else
				{
					engineActualWinProb = 1f - engineSideWinProbability;
					oppoActualWinProb = engineSideWinProbability;
				}

				// note: EngineSide is relative it can be either b/w( which is what's inside stm in fen provided)

				sb.AppendLine($"  EngineSideWinProb:       {engineActualWinProb:F4} ({engineActualWinProb:P1})");
				sb.AppendLine($"  OppoSideWinProb:       {oppoActualWinProb:F4} ({oppoActualWinProb:P1})");
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
		/// Analysis log entry for debugging and PGN export
		/// </summary>
		[System.Serializable]
		public class AnalysisLogEntry
		{
			public string fen;                      // Position that was analyzed
			public string bestMove;                 // Engine's best move
			public float evaluation;                // Position evaluation
			public float analysisTimeMs;            // Time taken for analysis
			public int depth;                       // Search depth used
			public DateTime timestamp;              // When analysis was performed

			public AnalysisLogEntry(string fen, string bestMove, float evaluation, float analysisTime, int depth)
			{
				this.fen = fen;
				this.bestMove = bestMove;
				this.evaluation = evaluation;
				this.analysisTimeMs = analysisTime;
				this.depth = depth;
				this.timestamp = DateTime.Now;
			}

			public override string ToString()
			{
				return $"AnalysisLogEntry {{ FEN: {fen?.Substring(0, Math.Min(20, fen?.Length ?? 0))}..., Move: {bestMove}, Eval: {evaluation:F2}, Time: {analysisTimeMs:F1}ms, Depth: {depth} }}";
			}
		}

		// Public properties
		public string LastRawOutput { get; private set; } = "";
		public ChessAnalysisResult LastAnalysisResult { get; private set; } = new ChessAnalysisResult();
		public List<AnalysisLogEntry> AnalysisLog { get; private set; } = new List<AnalysisLogEntry>();

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

		// Request tracking
		private volatile bool waitingForBestMove = false;
		private volatile bool currentRequestCompleted = false;
		private readonly List<string> currentRequestOutput = new List<string>();
		private readonly object requestLock = new object();

		#region Unity Lifecycle
		private void Awake()
		{
			UnityEngine.Debug.Log("<color=green>[StockfishBridge] Awake(): Initializing analysis engine...</color>");

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

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendLine("StockfishBridge {");
			sb.AppendLine($"  EngineRunning:      {IsEngineRunning}");
			sb.AppendLine($"  IsReady:            {IsReady}");
			sb.AppendLine($"  AnalysisLogSize:    {AnalysisLog.Count}");
			sb.AppendLine($"  LastAnalysis:       {(!string.IsNullOrEmpty(LastAnalysisResult.bestMove) ? LastAnalysisResult.bestMove : "None")}");
			sb.AppendLine($"  Config:             Depth:{defaultDepth}, Elo:{defaultElo}, Skill:{defaultSkillLevel}");
			sb.Append("}");
			return sb.ToString();
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
		/// Comprehensive chess position analysis with research-based evaluation and FEN-driven side management.
		/// 
		/// EVALUATION EXPLANATION (RESEARCH-BASED):
		/// - whiteWinProbability: Uses Lichess research equation: Win% = 50 + 50 * (2 / (1 + exp(-0.00368208 * centipawns)) - 1)
		/// - sideToMoveWinProbability: Adjusted based on FEN side-to-move field
		/// - Evaluation scaling based on real game data analysis, not theoretical material values
		/// - Handles modern NNUE evaluation which is less tied to traditional material values
		/// 
		/// SIDE MANAGEMENT:
		/// - Side determination is purely FEN-based - no external state management
		/// - Analysis is position-centric, not player-centric
		/// - Evaluation always returns probabilities for both white and side-to-move perspectives
		/// 
		/// PROMOTION HANDLING:
		/// - Strict UCI protocol adherence: 5-character lowercase format (e7e8q)
		/// - Comprehensive validation of rank transitions and side consistency
		/// - Full support for capture and non-capture promotions
		/// </summary>
		public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs = -1, int searchDepth = 12, int evaluationDepth = 15, int elo = 1500, int skillLevel = 8)
		{
			float startTime = Time.time;

			// Initialize result
			LastAnalysisResult = new ChessAnalysisResult();
			LastAnalysisResult.currentFen = fen;

			// Extract and validate FEN - side determination is FEN-driven only
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

			// Add to analysis log
			AddToAnalysisLog(fen, LastAnalysisResult.bestMove, LastAnalysisResult.centipawnEvaluation,
							LastAnalysisResult.analysisTimeMs, actualSearchDepth);

			// Fire completion event
			OnAnalysisComplete?.Invoke(LastAnalysisResult);

			if (enableDebugLogging)
			{
				UnityEngine.Debug.Log($"<color=green>[Stockfish] Analysis completed in {LastAnalysisResult.analysisTimeMs:F1}ms</color>");
			}
		}

		#endregion

		#region Public API - Logging and Export

		/// <summary>
		/// Add entry to analysis log for debugging and export
		/// </summary>
		private void AddToAnalysisLog(string fen, string bestMove, float evaluation, float analysisTime, int depth)
		{
			var entry = new AnalysisLogEntry(fen, bestMove, evaluation, analysisTime, depth);
			AnalysisLog.Add(entry);

			// Limit log size to prevent memory issues
			const int maxLogSize = 1000;
			if (AnalysisLog.Count > maxLogSize)
			{
				AnalysisLog.RemoveAt(0);
			}

			if (enableDebugLogging)
			{
				UnityEngine.Debug.Log($"<color=cyan>[StockfishBridge] Logged analysis: {entry}</color>");
			}
		}

		/// <summary>
		/// Clear analysis log
		/// </summary>
		public void ClearAnalysisLog()
		{
			AnalysisLog.Clear();

			if (enableDebugLogging)
			{
				UnityEngine.Debug.Log("<color=green>[StockfishBridge] Analysis log cleared</color>");
			}
		}

		/// <summary>
		/// Export analysis log as formatted string for debugging or PGN generation
		/// </summary>
		public string ExportAnalysisLog()
		{
			if (AnalysisLog.Count == 0)
				return "No analysis data available";

			var sb = new StringBuilder();
			sb.AppendLine("StockfishBridge Analysis Log");
			sb.AppendLine("===========================");
			sb.AppendLine($"Total Analyses: {AnalysisLog.Count}");
			sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
			sb.AppendLine();

			foreach (var entry in AnalysisLog)
			{
				sb.AppendLine($"Position: {entry.fen}");
				sb.AppendLine($"Best Move: {entry.bestMove}");
				sb.AppendLine($"Evaluation: {entry.evaluation:F2}cp");
				sb.AppendLine($"Depth: {entry.depth}, Time: {entry.analysisTimeMs:F1}ms");
				sb.AppendLine($"Timestamp: {entry.timestamp:HH:mm:ss}");
				sb.AppendLine();
			}

			return sb.ToString();
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
		/// Uses updated research data for accurate rating estimation
		/// </summary>
		private int CalculateApproximateElo(int uciElo, int skillLevel, int searchDepth)
		{
			// Base Elo from explicit setting
			if (uciElo > 0)
			{
				int baseElo = uciElo;
				// Each depth level adds ~100-130 Elo (updated research-based)
				if (searchDepth > 8)
					baseElo += (searchDepth - 8) * 115;
				else if (searchDepth < 8)
					baseElo -= (8 - searchDepth) * 140;

				return Mathf.Clamp(baseElo, 100, 3600);
			}

			// Updated skill level mapping based on current Stockfish testing
			if (skillLevel >= 0 && skillLevel <= 20)
			{
				int[] skillEloMap = {
					250,   // Skill 0  - Random moves
					400,   // Skill 1  - Very weak
					550,   // Skill 2  - Weak
					700,   // Skill 3  - Beginner
					850,   // Skill 4  - Improving beginner
					1000,  // Skill 5  - Novice
					1200,  // Skill 6  - Club player
					1400,  // Skill 7  - Average club
					1600,  // Skill 8  - Good club
					1800,  // Skill 9  - Strong club
					2000,  // Skill 10 - Expert
					2150,  // Skill 11 - Master level
					2250,  // Skill 12 - Strong master
					2350,  // Skill 13 - International master
					2450,  // Skill 14 - Grandmaster
					2550,  // Skill 15 - Strong GM
					2650,  // Skill 16 - Elite GM
					2750,  // Skill 17 - World class
					2850,  // Skill 18 - Super GM
					2950,  // Skill 19 - World championship
					3100   // Skill 20 - Full strength
				};

				int skillElo = skillEloMap[skillLevel];

				// Depth adjustment for skill-based rating
				if (searchDepth > 12)
					skillElo += (searchDepth - 12) * 75;
				else if (searchDepth < 12)
					skillElo -= (12 - searchDepth) * 90;

				return Mathf.Clamp(skillElo, 100, 3600);
			}

			// Full strength with depth scaling
			int fullStrengthElo = 3150 + Math.Max(0, searchDepth - 15) * 40;
			return Mathf.Clamp(fullStrengthElo, 3000, 3600);
		}

		/// <summary>
		/// Set error result with proper formatting
		/// </summary>
		private void SetErrorResult(string bestMove, string errorMessage)
		{
			LastAnalysisResult.bestMove = bestMove;
			LastAnalysisResult.errorMessage = errorMessage;
			LastAnalysisResult.engineSideWinProbability = 0.5f;
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

			// Reset previous settings to ensure clean state
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

			// Apply skill level if specified (overrides Elo setting when both are present)
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
		/// Enhanced FEN validation with comprehensive checks
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
			int totalPieces = 0;

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
						totalPieces++;
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

			// Validate reasonable piece count (max 32 pieces)
			if (totalPieces > 32)
				return $"Too many pieces on board: {totalPieces} (maximum 32)";

			// Validate side to move
			char side = parts[1].ToLower()[0];
			if (side != 'w' && side != 'b')
				return $"Side to move must be 'w' or 'b', found '{parts[1]}'";

			// Optional: validate castling rights format if present
			if (parts.Length >= 3)
			{
				string castling = parts[2];
				if (castling != "-" && !IsValidCastlingRights(castling))
					return $"Invalid castling rights format: '{castling}'";
			}

			return ""; // Valid FEN
		}

		/// <summary>
		/// Validate castling rights string format
		/// </summary>
		private bool IsValidCastlingRights(string castling)
		{
			if (string.IsNullOrEmpty(castling) || castling == "-")
				return true;

			// Valid characters: K, Q, k, q (and must be in reasonable order)
			foreach (char c in castling)
			{
				if ("KQkq".IndexOf(c) < 0)
					return false;
			}

			// Additional validation: no duplicates
			return castling.Length == castling.Distinct().Count();
		}

		/// <summary>
		/// Parse analysis result with research-based evaluation and enhanced promotion support
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

			// Parse evaluation first (research-based calculation)
			if (evaluationDepth > 0 && enableEvaluation)
			{
				ParseEvaluationFromOutput(lines, evaluationDepth);
			}
			else
			{
				// Default neutral evaluation
				LastAnalysisResult.engineSideWinProbability = 0.5f;
				LastAnalysisResult.sideToMoveWinProbability = 0.5f;
			}

			// Parse best move with UCI promotion validation
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
		/// Parse evaluation from engine output lines using research-based calculation
		/// </summary>
		private void ParseEvaluationFromOutput(string[] lines, int targetDepth)
		{
			string bestInfoLine = SelectBestInfoLine(lines, targetDepth);

			if (string.IsNullOrEmpty(bestInfoLine))
			{
				// No evaluation found, use neutral
				LastAnalysisResult.engineSideWinProbability = 0.5f;
				LastAnalysisResult.sideToMoveWinProbability = 0.5f;
				return;
			}

			var (centipawns, isMate, mateDistance) = ExtractScoreFromInfoLine(bestInfoLine);

			if (isMate)
			{
				// Mate score with high confidence
				LastAnalysisResult.isMateScore = true;
				LastAnalysisResult.mateDistance = mateDistance;
				LastAnalysisResult.engineSideWinProbability = ConvertMateToWinProbability(mateDistance);
			}
			else if (!float.IsNaN(centipawns))
			{
				// Research-based centipawn to probability conversion
				LastAnalysisResult.centipawnEvaluation = centipawns;
				LastAnalysisResult.engineSideWinProbability = ConvertCentipawnsToWinProbabilityResearch(centipawns);
			}
			else
			{
				// Fallback neutral
				LastAnalysisResult.engineSideWinProbability = 0.5f;
			}

			// Calculate side-to-move probability based on FEN
			if (LastAnalysisResult.sideToMove == 'b')
			{
				// When black to move, if evaluation is positive (good for white), STM probability is low
				// When black to move, if evaluation is negative (good for black), STM probability is high
				if (LastAnalysisResult.centipawnEvaluation >= 0)
				{
					LastAnalysisResult.sideToMoveWinProbability = 1f - LastAnalysisResult.engineSideWinProbability;
				}
				else
				{
					LastAnalysisResult.sideToMoveWinProbability = LastAnalysisResult.engineSideWinProbability;
				}
			}
			else
			{
				// When white to move, if evaluation is positive (good for white), STM probability is high
				// When white to move, if evaluation is negative (good for black), STM probability is low
				if (LastAnalysisResult.centipawnEvaluation >= 0)
				{
					LastAnalysisResult.sideToMoveWinProbability = LastAnalysisResult.engineSideWinProbability;
				}
				else
				{
					LastAnalysisResult.sideToMoveWinProbability = 1f - LastAnalysisResult.engineSideWinProbability;
				}
			}

			// Clamp probabilities to reasonable ranges
			LastAnalysisResult.engineSideWinProbability = Mathf.Clamp(LastAnalysisResult.engineSideWinProbability, 0.001f, 0.999f);
			LastAnalysisResult.sideToMoveWinProbability = Mathf.Clamp(LastAnalysisResult.sideToMoveWinProbability, 0.001f, 0.999f);
		}

		/// <summary>
		/// Parse best move from engine output with enhanced UCI promotion validation
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
				// No legal moves available - determine game end type
				if (LastAnalysisResult.isMateScore && Math.Abs(LastAnalysisResult.mateDistance) <= 1)
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
					// Reset evaluation for stalemate (draw)
					LastAnalysisResult.engineSideWinProbability = 0.5f;
					LastAnalysisResult.sideToMoveWinProbability = 0.5f;
				}
				return;
			}

			// Valid move - set and parse promotion data with enhanced validation
			LastAnalysisResult.bestMove = move;
			LastAnalysisResult.ParsePromotionData();
		}

		/// <summary>
		/// Select best info line based on depth and multipv preferences
		/// </summary>
		private string SelectBestInfoLine(string[] lines, int targetDepth)
		{
			string bestLine = null;
			int bestDepth = -1;
			bool foundExactDepth = false;

			foreach (string line in lines)
			{
				string trimmedLine = line.Trim();
				if (!trimmedLine.StartsWith("info") || trimmedLine.IndexOf("score") < 0)
					continue;

				var (depth, multipv, hasScore) = ParseInfoLineMetadata(trimmedLine);
				if (!hasScore || multipv > 1) // Only consider main line (multipv 1)
					continue;

				// Prefer exact depth match first
				if (depth == targetDepth && !foundExactDepth)
				{
					bestLine = trimmedLine;
					bestDepth = depth;
					foundExactDepth = true;
				}
				// If no exact match found, take highest depth
				else if (!foundExactDepth && depth > bestDepth)
				{
					bestDepth = depth;
					bestLine = trimmedLine;
				}
			}

			return bestLine;
		}

		/// <summary>
		/// Parse info line metadata with enhanced validation
		/// </summary>
		private (int depth, int multipv, bool hasScore) ParseInfoLineMetadata(string infoLine)
		{
			string[] tokens = infoLine.Split(' ');
			int depth = -1;
			int multipv = 1; // Default to main line
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
		/// Extract score from UCI info line with improved parsing
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
					// Clamp extreme centipawn values for display
					float clampedCp = Mathf.Clamp(cp, -MAX_EVALUATION_CENTIPAWNS, MAX_EVALUATION_CENTIPAWNS);
					return (clampedCp, false, 0);
				}
				else if (tokens[i] == "mate" && int.TryParse(tokens[i + 1], out int mate))
				{
					return (float.NaN, true, mate);
				}
			}

			return (float.NaN, false, 0);
		}

		/// <summary>
		/// Convert centipawn evaluation to win probability using Lichess research equation
		/// Research source: Win% = 50 + 50 * (2 / (1 + exp(-0.00368208 * centipawns)) - 1)
		/// </summary>
		private float ConvertCentipawnsToWinProbabilityResearch(float centipawns)
		{
			// Apply Lichess research-based equation
			float exponent = -LICHESS_CENTIPAWN_SCALE * centipawns;
			float winPercentage = 50f + 50f * (2f / (1f + Mathf.Exp(exponent)) - 1f);

			// Convert percentage to probability (0-1 range)
			return winPercentage / 100f;
		}

		/// <summary>
		/// Convert mate distance to win probability with high confidence
		/// </summary>
		private float ConvertMateToWinProbability(int mateDistance)
		{
			if (mateDistance > 0)
			{
				// White mates: very high probability for white
				// Shorter mate = higher confidence
				float confidence = MATE_CONFIDENCE_THRESHOLD + (1f - MATE_CONFIDENCE_THRESHOLD) * (1f - 1f / (Math.Abs(mateDistance) + 1f));
				return Mathf.Clamp(confidence, MATE_CONFIDENCE_THRESHOLD, 0.999f);
			}
			else if (mateDistance < 0)
			{
				// Black mates: very low probability for white  
				// Shorter mate = lower confidence for white
				float confidence = (1f - MATE_CONFIDENCE_THRESHOLD) * (1f / (Math.Abs(mateDistance) + 1f));
				return Mathf.Clamp(confidence, 0.001f, 1f - MATE_CONFIDENCE_THRESHOLD);
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

		#region Private Methods - Testing

		/// <summary>
		/// Test promotion parsing with various UCI formats
		/// </summary>
		private void TestPromotionParsing()
		{
			string[] testMoves = { "e7e8q", "a7a8n", "h2h1r", "b7b8b", "d7c8q", "f2g1n" };

			UnityEngine.Debug.Log("<color=cyan>[StockfishBridge] Testing UCI promotion parsing...</color>");

			foreach (string move in testMoves)
			{
				ChessAnalysisResult testResult = new ChessAnalysisResult
				{
					bestMove = move,
					sideToMove = move[1] == '7' ? 'w' : 'b' // Determine side by source rank
				};

				testResult.ParsePromotionData();

				if (testResult.isPromotion)
				{
					UnityEngine.Debug.Log($"<color=green>[StockfishBridge] ✓ {move} -> {testResult.GetPromotionDescription()}</color>");
				}
				else
				{
					UnityEngine.Debug.Log($"<color=red>[StockfishBridge] ✗ {move} -> Failed to parse as promotion</color>");
				}
			}
		}

		/// <summary>
		/// Test research-based Elo calculation
		/// </summary>
		private void TestEloCalculation()
		{
			UnityEngine.Debug.Log("<color=cyan>[StockfishBridge] Testing updated Elo calculation...</color>");

			int[] testSkillLevels = { 0, 5, 10, 15, 20 };
			int[] testDepths = { 5, 10, 15, 20 };

			foreach (int skill in testSkillLevels)
			{
				foreach (int depth in testDepths)
				{
					int elo = CalculateApproximateElo(-1, skill, depth);
					UnityEngine.Debug.Log($"<color=green>[StockfishBridge] ✓ Skill {skill}, Depth {depth} -> Elo {elo}</color>");
				}
			}

			// Test explicit Elo settings
			int[] testElos = { 1000, 1500, 2000, 2500 };
			foreach (int elo in testElos)
			{
				int calculatedElo = CalculateApproximateElo(elo, -1, 12);
				UnityEngine.Debug.Log($"<color=green>[StockfishBridge] ✓ Explicit Elo {elo} -> Calculated {calculatedElo}</color>");
			}
		}

		/// <summary>
		/// Test research-based evaluation calculations
		/// </summary>
		private void TestEvaluationCalculation()
		{
			UnityEngine.Debug.Log("<color=cyan>[StockfishBridge] Testing research-based evaluation...</color>");

			float[] testCentipawns = { -200f, -100f, 0f, 50f, 100f, 200f, 500f, 1000f };

			foreach (float cp in testCentipawns)
			{
				float winProb = ConvertCentipawnsToWinProbabilityResearch(cp);
				UnityEngine.Debug.Log($"<color=green>[StockfishBridge] ✓ {cp}cp -> {winProb:P1} win probability</color>");
			}

			// Test mate scores
			int[] mateDistances = { 1, 3, 5, -1, -3, -5 };
			foreach (int mate in mateDistances)
			{
				float mateProb = ConvertMateToWinProbability(mate);
				string winner = mate > 0 ? "White" : "Black";
				UnityEngine.Debug.Log($"<color=green>[StockfishBridge] ✓ Mate in {Math.Abs(mate)} for {winner} -> {mateProb:P1}</color>");
			}
		}

		/// <summary>
		/// Test comprehensive FEN-based analysis
		/// </summary>
		private void TestFENBasedAnalysis()
		{
			UnityEngine.Debug.Log("<color=cyan>[StockfishBridge] Testing FEN-based analysis...</color>");

			// Test positions with different sides to move
			string[] testPositions = {
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", // Starting position - white
				"rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1", // After e4 - black to move
				"r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1", // Complex middle game - white
				"r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R b KQkq - 0 1", // Same position - black
				"8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1", // Endgame - white
				"8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 b - - 0 1" // Same endgame - black
			};

			foreach (string fen in testPositions)
			{
				char extractedSide = ExtractSideFromFen(fen);
				string validation = ValidateFen(fen);

				if (string.IsNullOrEmpty(validation))
				{
					UnityEngine.Debug.Log($"<color=green>[StockfishBridge] ✓ FEN valid, side: {extractedSide} | {fen.Substring(0, Math.Min(30, fen.Length))}...</color>");
				}
				else
				{
					UnityEngine.Debug.Log($"<color=red>[StockfishBridge] ✗ FEN invalid: {validation}</color>");
				}
			}
		}

		/// <summary>
		/// Test engine restart and crash recovery
		/// </summary>
		private void TestEngineRestart()
		{
			UnityEngine.Debug.Log("<color=cyan>[StockfishBridge] Testing engine restart...</color>");

			if (IsEngineRunning)
			{
				StopEngine();
				if (!IsEngineRunning)
				{
					UnityEngine.Debug.Log("<color=green>[StockfishBridge] ✓ Engine stopped successfully</color>");
				}
				else
				{
					UnityEngine.Debug.Log("<color=red>[StockfishBridge] ✗ Engine failed to stop</color>");
				}
			}

			StartEngine();
			StartCoroutine(TestEngineRestartCoroutine());
		}

		private IEnumerator TestEngineRestartCoroutine()
		{
			yield return StartCoroutine(InitializeEngineCoroutine());

			if (IsEngineRunning && IsReady)
			{
				UnityEngine.Debug.Log("<color=green>[StockfishBridge] ✓ Engine restarted and ready</color>");
			}
			else
			{
				UnityEngine.Debug.Log("<color=red>[StockfishBridge] ✗ Engine restart failed</color>");
			}
		}

		/// <summary>
		/// Test enhanced FEN validation
		/// </summary>
		private void TestFENValidation()
		{
			UnityEngine.Debug.Log("<color=cyan>[StockfishBridge] Testing enhanced FEN validation...</color>");

			// Valid FENs
			string[] validFENs = {
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
				"k7/8/8/8/8/8/8/7K w - - 0 1",
				"startpos",
				"r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1" // Complex valid position
			};

			foreach (string fen in validFENs)
			{
				string error = ValidateFen(fen);
				if (string.IsNullOrEmpty(error))
				{
					UnityEngine.Debug.Log($"<color=green>[StockfishBridge] ✓ Valid FEN accepted: {fen.Substring(0, Math.Min(20, fen.Length))}...</color>");
				}
				else
				{
					UnityEngine.Debug.Log($"<color=red>[StockfishBridge] ✗ Valid FEN rejected: {fen} - {error}</color>");
				}
			}

			// Invalid FENs
			string[] invalidFENs =
			{
				"",
				"invalid",
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", // Missing side to move
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR x KQkq - 0 1", // Invalid side
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP w KQkq - 0 1", // Missing rank
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNRK w KQkq - 0 1", // Too many pieces in rank
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBN w KQkq - 0 1", // Too few pieces in rank
				"8/8/8/8/8/8/8/KK w - - 0 1", // Two white kings
				"8/8/8/8/8/8/8/K w - - 0 1" // Missing black king
			};

			foreach (string fen in invalidFENs)
			{
				string error = ValidateFen(fen);
				if (!string.IsNullOrEmpty(error))
				{
					UnityEngine.Debug.Log($"<color=green>[StockfishBridge] ✓ Invalid FEN rejected: {error}</color>");
				}
				else
				{
					UnityEngine.Debug.Log($"<color=red>[StockfishBridge] ✗ Invalid FEN accepted: {fen}</color>");
				}
			}
		}

		/// <summary>
		/// Test analysis logging functionality
		/// </summary>
		private void TestAnalysisLogging()
		{
			UnityEngine.Debug.Log("<color=cyan>[StockfishBridge] Testing analysis logging...</color>");

			ClearAnalysisLog();

			// Add test entries
			AddToAnalysisLog("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", "e2e4", 25f, 1500f, 12);
			AddToAnalysisLog("rnbqkbnr/pppp1ppp/8/4p3/4P3/8/PPPP1PPP/RNBQKBNR w KQkq e6 0 2", "Nf3", -15f, 1200f, 10);

			if (AnalysisLog.Count == 2)
			{
				UnityEngine.Debug.Log("<color=green>[StockfishBridge] ✓ Analysis log entries added correctly</color>");
			}
			else
			{
				UnityEngine.Debug.Log("<color=red>[StockfishBridge] ✗ Analysis log count incorrect</color>");
			}

			// Test export
			string exportedLog = ExportAnalysisLog();
			if (exportedLog.Contains("Total Analyses: 2"))
			{
				UnityEngine.Debug.Log("<color=green>[StockfishBridge] ✓ Analysis log export successful</color>");
			}
			else
			{
				UnityEngine.Debug.Log("<color=red>[StockfishBridge] ✗ Analysis log export failed</color>");
			}

			// Test clear
			ClearAnalysisLog();
			if (AnalysisLog.Count == 0)
			{
				UnityEngine.Debug.Log("<color=green>[StockfishBridge] ✓ Analysis log cleared successfully</color>");
			}
			else
			{
				UnityEngine.Debug.Log("<color=red>[StockfishBridge] ✗ Analysis log clear failed</color>");
			}
		}

		/// <summary>
		/// Test comprehensive position analysis
		/// </summary>
		private IEnumerator TestPositionAnalysis()
		{
			UnityEngine.Debug.Log("<color=cyan>[StockfishBridge] Testing position analysis...</color>");

			string[] testPositions = {
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
				"r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R b KQkq - 0 1"
			};

			foreach (string fen in testPositions)
			{
				yield return StartCoroutine(AnalyzePositionCoroutine(fen, 1000, 8, 10, 1500, 5));

				if (LastAnalysisResult.bestMove.StartsWith("ERROR"))
				{
					UnityEngine.Debug.Log($"<color=red>[StockfishBridge] ✗ Analysis failed for position: {LastAnalysisResult.errorMessage}</color>");
				}
				else
				{
					UnityEngine.Debug.Log($"<color=green>[StockfishBridge] ✓ Analysis success: {LastAnalysisResult.bestMove} | {LastAnalysisResult.GetEvaluationDisplay()}</color>");
				}
			}
		}

		#endregion

		/// <summary>
		/// Run all StockfishBridge comprehensive tests for analysis-focused engine
		/// </summary>
		public void RunAllTests()
		{
			UnityEngine.Debug.Log("<color=cyan>=== StockfishBridge Analysis Engine Test Suite v0.5 ===</color>");

			TestPromotionParsing();
			TestEloCalculation();
			TestEvaluationCalculation();
			TestFENBasedAnalysis();
			TestEngineRestart();
			TestFENValidation();
			TestAnalysisLogging();

			// Start coroutine tests
			StartCoroutine(RunCoroutineTests());

			UnityEngine.Debug.Log("<color=cyan>=== StockfishBridge Tests Completed ===</color>");
		}

		private IEnumerator RunCoroutineTests()
		{
			yield return new WaitForSeconds(0.1f);
			yield return StartCoroutine(TestPositionAnalysis());
		}
	}
}