using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GPTDeepResearch;
using SPACE_UTIL;

namespace GPTDeepResearch
{
	/// <summary>
	/// Complete usage example demonstrating chess engine integration
	/// Shows how to get best moves, evaluation scores, and handle all chess functionality
	/// </summary>
	public class ChessEngineUsageExample : MonoBehaviour
	{
		[Header("Chess Components")]
		public StockfishBridge stockfishBridge;
		public PromotionUI promotionUI;

		[Header("Example Configuration")]
		public string testFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
		public int analysisTimeMs = 2000;
		public int searchDepth = 12;
		public int engineElo = 1500;
		public int skillLevel = 10;

		[Header("Current Game State")]
		public ChessBoard currentBoard;
		public bool waitingForEngineMove = false;
		public bool waitingForPromotion = false;

		void Start()
		{
			SetupChessEngine();
			StartCoroutine(DemonstrateChessUsage());
		}

		/// <summary>
		/// Setup chess engine and components
		/// </summary>
		void SetupChessEngine()
		{
			Debug.Log("<color=cyan>[ChessExample] Setting up chess engine...</color>");

			// Initialize chess board
			currentBoard = new ChessBoard(testFEN);
			currentBoard.SetHumanSide('w'); // Human plays white

			// Setup Stockfish bridge if available
			if (stockfishBridge != null)
			{
				stockfishBridge.OnAnalysisComplete.AddListener(OnEngineAnalysisComplete);
				stockfishBridge.SetHumanSide('w');
				Debug.Log("<color=green>[ChessExample] ✓ Stockfish bridge connected</color>");
			}

			// Setup promotion UI if available
			if (promotionUI != null)
			{
				promotionUI.OnPromotionSelected += OnPromotionSelected;
				promotionUI.OnPromotionCancelled += OnPromotionCancelled;
				Debug.Log("<color=green>[ChessExample] ✓ Promotion UI connected</color>");
			}
		}

		/// <summary>
		/// Demonstrate complete chess usage workflow
		/// </summary>
		IEnumerator DemonstrateChessUsage()
		{
			Debug.Log("<color=cyan>=== Chess Engine Usage Example ===</color>");

			// Wait for engine to initialize
			if (stockfishBridge != null)
			{
				yield return StartCoroutine(stockfishBridge.InitializeEngineCoroutine());
				Debug.Log("<color=green>[ChessExample] ✓ Engine initialized</color>");
			}

			// Example 1: Analyze starting position
			yield return StartCoroutine(AnalyzePosition(testFEN));

			// Example 2: Play a few moves and analyze
			yield return StartCoroutine(PlayExampleGame());

			// Example 3: Handle promotion scenario
			yield return StartCoroutine(TestPromotionScenario());

			// Example 4: Demonstrate undo/redo
			yield return StartCoroutine(TestUndoRedo());

			// Example 5: Test position evaluation
			yield return StartCoroutine(TestPositionEvaluation());

			Debug.Log("<color=green>=== Chess Usage Examples Completed ===</color>");
		}

		/// <summary>
		/// Example 1: Analyze any chess position and get best move + evaluation
		/// </summary>
		IEnumerator AnalyzePosition(string fen)
		{
			Debug.Log($"<color=cyan>[ChessExample] Analyzing position: {fen}</color>");

			// Load position into board
			currentBoard.LoadFromFEN(fen);

			// Get legal moves count
			var legalMoves = currentBoard.GetLegalMoves();
			Debug.Log($"<color=yellow>[ChessExample] Legal moves available: {legalMoves.Count}</color>");

			// If we have Stockfish, get engine analysis
			if (stockfishBridge != null && stockfishBridge.IsEngineRunning)
			{
				waitingForEngineMove = true;

				// Request analysis with custom settings
				yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(
					fen, analysisTimeMs, searchDepth, searchDepth, engineElo, skillLevel));

				waitingForEngineMove = false;

				// Results are handled in OnEngineAnalysisComplete callback
			}
			else
			{
				Debug.Log("<color=yellow>[ChessExample] No engine available, using basic analysis</color>");

				// Basic position analysis without engine
				var gameResult = currentBoard.GetGameResult();
				Debug.Log($"<color=yellow>[ChessExample] Game status: {gameResult}</color>");

				if (legalMoves.Count > 0)
				{
					var randomMove = legalMoves[Random.Range(0, legalMoves.Count)];
					Debug.Log($"<color=yellow>[ChessExample] Random legal move: {randomMove.ToUCI()}</color>");
				}
			}

			yield return new WaitForSeconds(0.5f);
		}

		/// <summary>
		/// Example 2: Play a sequence of moves with analysis
		/// </summary>
		IEnumerator PlayExampleGame()
		{
			Debug.Log("<color=cyan>[ChessExample] Playing example game...</color>");

			// Reset to starting position
			currentBoard.SetupStartingPosition();

			// Play some opening moves
			string[] openingMoves = { "e2e4", "e7e5", "g1f3", "b8c6", "f1c4" };

			foreach (string uciMove in openingMoves)
			{
				Debug.Log($"<color=cyan>[ChessExample] Playing move: {uciMove}</color>");

				var move = ChessMove.FromUCI(uciMove, currentBoard);
				if (move.IsValid() && ChessRules.ValidateMove(currentBoard, move))
				{
					// Make the move
					if (currentBoard.MakeMove(move))
					{
						Debug.Log($"<color=green>[ChessExample] ✓ Move made: {move.ToPGN(currentBoard)}</color>");

						// Analyze position after move
						yield return StartCoroutine(AnalyzeCurrentPosition());
					}
					else
					{
						Debug.Log($"<color=red>[ChessExample] ✗ Failed to make move: {uciMove}</color>");
						break;
					}
				}
				else
				{
					Debug.Log($"<color=red>[ChessExample] ✗ Invalid move: {uciMove}</color>");
					break;
				}

				yield return new WaitForSeconds(1f);
			}

			Debug.Log($"<color=green>[ChessExample] Game tree now has {currentBoard.GameTreeNodeCount} positions</color>");
		}

		/// <summary>
		/// Example 3: Handle pawn promotion scenario
		/// </summary>
		IEnumerator TestPromotionScenario()
		{
			Debug.Log("<color=cyan>[ChessExample] Testing promotion scenario...</color>");

			// Setup position where pawn can promote
			string promotionFEN = "8/P7/8/8/8/8/7k/7K w - - 0 1";
			currentBoard.LoadFromFEN(promotionFEN);

			Debug.Log("<color=yellow>[ChessExample] Position loaded: White pawn on a7, can promote</color>");

			// Check if promotion is required for this move
			var promotionMove = new ChessMove(new v2(0, 6), new v2(0, 7), 'P'); // a7-a8

			if (ChessRules.RequiresPromotion(currentBoard, promotionMove))
			{
				Debug.Log("<color=yellow>[ChessExample] Move requires promotion</color>");

				// Show promotion UI if available
				if (promotionUI != null && !promotionUI.IsWaitingForPromotion())
				{
					waitingForPromotion = true;
					promotionUI.ShowPromotionDialog(true, "a7", "a8");

					// Wait for user selection or timeout
					float timeout = 10f;
					float elapsed = 0f;

					while (waitingForPromotion && elapsed < timeout)
					{
						elapsed += Time.deltaTime;
						yield return null;
					}

					if (waitingForPromotion)
					{
						// Timeout - force default promotion
						Debug.Log("<color=yellow>[ChessExample] Promotion timeout - selecting Queen</color>");
						promotionUI.SelectDefaultPromotion();
						waitingForPromotion = false;
					}
				}
				else
				{
					// No UI - promote to queen by default
					Debug.Log("<color=yellow>[ChessExample] No promotion UI - defaulting to Queen</color>");
					var queenPromotion = ChessMove.CreatePromotionMove(new v2(0, 6), new v2(0, 7), 'P', 'Q');

					if (currentBoard.MakeMove(queenPromotion))
					{
						Debug.Log("<color=green>[ChessExample] ✓ Promoted to Queen successfully</color>");
					}
				}
			}

			yield return new WaitForSeconds(0.5f);
		}

		/// <summary>
		/// Example 4: Demonstrate undo/redo functionality
		/// </summary>
		IEnumerator TestUndoRedo()
		{
			Debug.Log("<color=cyan>[ChessExample] Testing undo/redo functionality...</color>");

			// Make sure we have some moves to undo
			if (currentBoard.GameTreeNodeCount < 2)
			{
				// Make a quick move
				var legalMoves = currentBoard.GetLegalMoves();
				if (legalMoves.Count > 0)
				{
					currentBoard.MakeMove(legalMoves[0]);
				}
			}

			Debug.Log($"<color=yellow>[ChessExample] Current position: {currentBoard.CurrentHistoryIndex + 1}/{currentBoard.GameTreeNodeCount}</color>");

			// Test undo
			if (currentBoard.CanUndo())
			{
				string beforeFEN = currentBoard.ToFEN();

				if (currentBoard.UndoMove())
				{
					string afterFEN = currentBoard.ToFEN();
					Debug.Log("<color=green>[ChessExample] ✓ Undo successful</color>");
					Debug.Log($"<color=yellow>[ChessExample] Now at position: {currentBoard.CurrentHistoryIndex + 1}/{currentBoard.GameTreeNodeCount}</color>");

					yield return new WaitForSeconds(1f);

					// Test redo
					if (currentBoard.CanRedo())
					{
						if (currentBoard.RedoMove())
						{
							string redoFEN = currentBoard.ToFEN();

							if (redoFEN == beforeFEN)
							{
								Debug.Log("<color=green>[ChessExample] ✓ Redo successful - position restored</color>");
							}
							else
							{
								Debug.Log("<color=red>[ChessExample] ✗ Redo failed - position mismatch</color>");
							}
						}
					}
					else
					{
						Debug.Log("<color=yellow>[ChessExample] Cannot redo from current position</color>");
					}
				}
			}
			else
			{
				Debug.Log("<color=yellow>[ChessExample] Cannot undo from current position</color>");
			}

			yield return new WaitForSeconds(0.5f);
		}

		/// <summary>
		/// Example 5: Test position evaluation and game state
		/// </summary>
		IEnumerator TestPositionEvaluation()
		{
			Debug.Log("<color=cyan>[ChessExample] Testing position evaluation...</color>");

			// Test various position types
			string[] testPositions = {
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", // Starting position
                "rnb1kbnr/pppp1ppp/4p3/8/6Pq/5P2/PPPPP2P/RNBQKBNR w KQkq - 1 3", // Fool's mate
                "8/8/8/8/8/8/8/4K2k w - - 50 75", // King vs King endgame
                "r1bqkb1r/pppp1ppp/2n2n2/1B2p3/4P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 4 4" // Italian opening
            };

			foreach (string fen in testPositions)
			{
				Debug.Log($"<color=cyan>[ChessExample] Evaluating: {fen.Substring(0, 30)}...</color>");

				currentBoard.LoadFromFEN(fen);

				// Basic evaluation
				var gameResult = currentBoard.GetGameResult();
				var legalMoves = currentBoard.GetLegalMoves();
				bool isCheck = ChessRules.IsInCheck(currentBoard, currentBoard.sideToMove);

				Debug.Log($"<color=yellow>[ChessExample] Game result: {gameResult}</color>");
				Debug.Log($"<color=yellow>[ChessExample] Legal moves: {legalMoves.Count}</color>");
				Debug.Log($"<color=yellow>[ChessExample] In check: {isCheck}</color>");
				Debug.Log($"<color=yellow>[ChessExample] Side to move: {currentBoard.GetSideName(currentBoard.sideToMove)}</color>");

				// Engine evaluation if available
				if (stockfishBridge != null && stockfishBridge.IsEngineRunning)
				{
					yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(fen, 1000, 10, 10, -1, -1));
				}

				Debug.Log($"<color=green>[ChessExample] Evaluation: {currentBoard.LastEvaluation:F1} centipawns</color>");
				Debug.Log($"<color=green>[ChessExample] Win probability: {currentBoard.LastWinProbability:P1}</color>");

				yield return new WaitForSeconds(0.5f);
			}
		}

		/// <summary>
		/// Analyze current board position
		/// </summary>
		IEnumerator AnalyzeCurrentPosition()
		{
			if (stockfishBridge != null && stockfishBridge.IsEngineRunning)
			{
				string currentFEN = currentBoard.ToFEN();
				yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(currentFEN, 1000, 10, 10, engineElo, skillLevel));
			}
		}

		#region Event Handlers

		/// <summary>
		/// Handle engine analysis completion
		/// </summary>
		void OnEngineAnalysisComplete(StockfishBridge.ChessAnalysisResult result)
		{
			Debug.Log($"<color=green>[ChessExample] Engine analysis complete:</color>");
			Debug.Log($"<color=green>  Best move: {result.bestMove}</color>");
			Debug.Log($"<color=green>  Evaluation: {result.centipawnEvaluation:F1} centipawns</color>");
			Debug.Log($"<color=green>  Win probability: {result.whiteWinProbability:P1} (White) / {(1f - result.whiteWinProbability):P1} (Black)</color>");
			Debug.Log($"<color=green>  Search depth: {result.searchDepth}</color>");

			if (result.isMateScore)
			{
				Debug.Log($"<color=magenta>  Mate in {result.mateDistance} moves!</color>");
			}

			if (result.isPromotion)
			{
				Debug.Log($"<color=cyan>  Promotion move: {result.GetPromotionDescription()}</color>");
			}

			if (result.isGameEnd)
			{
				Debug.Log($"<color=yellow>  Game over: Checkmate={result.isCheckmate}, Stalemate={result.isStalemate}</color>");
			}

			// Update board evaluation
			currentBoard.UpdateEvaluation(
				result.centipawnEvaluation,
				result.sideToMoveWinProbability,
				result.mateDistance,
				result.searchDepth
			);

			// Make engine move if it's engine's turn
			if (!waitingForEngineMove && currentBoard.IsEngineTurn() && !string.IsNullOrEmpty(result.bestMove))
			{
				StartCoroutine(MakeEngineMove(result));
			}
		}

		/// <summary>
		/// Make the engine's move
		/// </summary>
		IEnumerator MakeEngineMove(StockfishBridge.ChessAnalysisResult result)
		{
			Debug.Log($"<color=cyan>[ChessExample] Engine making move: {result.bestMove}</color>");

			var engineMove = result.ToChessMove(currentBoard);

			if (engineMove.IsValid())
			{
				// Check if move requires promotion
				if (result.isPromotion && promotionUI != null)
				{
					// For engine moves, we can auto-promote to the suggested piece
					if (ChessMove.IsValidPromotionPiece(result.promotionPiece))
					{
						engineMove.promotionPiece = result.promotionPiece;
						engineMove.moveType = ChessMove.MoveType.Promotion;
					}
				}

				if (currentBoard.MakeMove(engineMove))
				{
					Debug.Log($"<color=green>[ChessExample] ✓ Engine move completed: {engineMove.ToPGN(currentBoard)}</color>");
				}
				else
				{
					Debug.Log($"<color=red>[ChessExample] ✗ Engine move failed</color>");
				}
			}
			else
			{
				Debug.Log($"<color=red>[ChessExample] ✗ Invalid engine move: {result.bestMove}</color>");
			}

			yield return null;
		}

		/// <summary>
		/// Handle promotion piece selection
		/// </summary>
		void OnPromotionSelected(char promotionPiece)
		{
			Debug.Log($"<color=green>[ChessExample] Promotion selected: {ChessMove.GetPromotionPieceName(promotionPiece)}</color>");

			waitingForPromotion = false;

			// Create and execute promotion move
			var promotionMove = ChessMove.CreatePromotionMove(new v2(0, 6), new v2(0, 7), 'P', promotionPiece);

			if (currentBoard.MakeMove(promotionMove))
			{
				Debug.Log("<color=green>[ChessExample] ✓ Promotion move completed</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessExample] ✗ Promotion move failed</color>");
			}
		}

		/// <summary>
		/// Handle promotion cancellation
		/// </summary>
		void OnPromotionCancelled()
		{
			Debug.Log("<color=yellow>[ChessExample] Promotion cancelled - defaulting to Queen</color>");
			waitingForPromotion = false;
			OnPromotionSelected('Q'); // Default to Queen
		}

		#endregion

		#region Public Interface Methods

		/// <summary>
		/// Get best move for current position (public interface)
		/// </summary>
		public void GetBestMove()
		{
			if (stockfishBridge != null && stockfishBridge.IsEngineRunning)
			{
				string currentFEN = currentBoard.ToFEN();
				StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(currentFEN, analysisTimeMs, searchDepth, searchDepth, engineElo, skillLevel));
			}
			else
			{
				Debug.Log("<color=yellow>[ChessExample] No engine available for analysis</color>");
			}
		}

		/// <summary>
		/// Load custom FEN position (public interface)
		/// </summary>
		public void LoadPosition(string fen)
		{
			if (currentBoard.LoadFromFEN(fen))
			{
				Debug.Log($"<color=green>[ChessExample] ✓ Loaded position: {fen}</color>");
				StartCoroutine(AnalyzeCurrentPosition());
			}
			else
			{
				Debug.Log($"<color=red>[ChessExample] ✗ Failed to load FEN: {fen}</color>");
			}
		}

		/// <summary>
		/// Make a move from UCI notation (public interface)
		/// </summary>
		public bool MakeMove(string uciMove)
		{
			var move = ChessMove.FromUCI(uciMove, currentBoard);

			if (move.IsValid())
			{
				return currentBoard.MakeMove(move);
			}

			Debug.Log($"<color=red>[ChessExample] Invalid move: {uciMove}</color>");
			return false;
		}

		/// <summary>
		/// Get current evaluation as string (public interface)
		/// </summary>
		public string GetEvaluationText()
		{
			if (Mathf.Abs(currentBoard.LastMateDistance) > 0.1f)
			{
				return $"Mate in {Mathf.Abs(currentBoard.LastMateDistance):F0}";
			}
			else
			{
				return $"{currentBoard.LastEvaluation:F1}cp ({currentBoard.LastWinProbability:P1})";
			}
		}

		#endregion

		void OnDestroy()
		{
			// Clean up event listeners
			if (stockfishBridge != null)
			{
				stockfishBridge.OnAnalysisComplete.RemoveListener(OnEngineAnalysisComplete);
			}

			if (promotionUI != null)
			{
				promotionUI.OnPromotionSelected -= OnPromotionSelected;
				promotionUI.OnPromotionCancelled -= OnPromotionCancelled;
			}
		}
	}
}