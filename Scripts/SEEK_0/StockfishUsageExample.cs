/*
StockfishUsageExample.cs - Complete Usage Guide and Examples
===========================================================

USAGE EXAMPLES:
1. Basic position analysis with evaluation
2. Custom FEN analysis with different engine strengths
3. Promotion move handling and UI integration
4. Side selection and turn management
5. Undo/redo move history

SETUP REQUIREMENTS:
- StockfishBridge component on GameObject
- PromotionUI component for human promotion selection
- ChessBoard instance for position management
- Stockfish engine executable in StreamingAssets/sf-engine.exe

INTEGRATION NOTES:
- Use coroutines for all engine communication
- Handle promotion moves through PromotionUI events
- Manage game state with ChessBoard undo/redo system
- Customize engine strength with Elo and skill levels
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SPACE_UTIL;

namespace GPTDeepResearch
{
	/// <summary>
	/// Complete usage examples for the chess engine system
	/// Demonstrates best practices and common use cases
	/// </summary>
	public class StockfishUsageExample : MonoBehaviour
	{
		[Header("Component References")]
		[SerializeField] private StockfishBridge stockfishBridge;
		[SerializeField] private PromotionUI promotionUI;
		[SerializeField] private Text statusText;
		[SerializeField] private Text evaluationText;

		[Header("Example Configuration")]
		[SerializeField] private bool runExamplesOnStart = false;
		[SerializeField] private string testFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

		// Game state
		private ChessBoard gameBoard;
		private bool awaitingPromotion = false;
		private ChessMove pendingPromotionMove;

		#region Unity Lifecycle

		private void Start()
		{
			// Initialize game board
			gameBoard = new ChessBoard();
			gameBoard.SetHumanSide('w'); // Human plays white by default

			// Setup promotion UI event
			if (promotionUI != null)
			{
				promotionUI.OnPromotionSelected.AddListener(OnPromotionPieceSelected);
			}

			// Run examples if enabled
			if (runExamplesOnStart)
			{
				StartCoroutine(RunExamples());
			}

			UpdateStatusDisplay();
		}

		#endregion

		#region Usage Examples

		/// <summary>
		/// EXAMPLE 1: Basic position analysis
		/// Shows how to get best move and evaluation for any FEN
		/// </summary>
		public IEnumerator ExampleBasicAnalysis()
		{
			Debug.Log("<color=yellow>=== EXAMPLE 1: Basic Position Analysis ===</color>");

			if (stockfishBridge == null)
			{
				Debug.Log("<color=red>StockfishBridge not assigned!</color>");
				yield break;
			}

			// Analyze starting position
			string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

			Debug.Log($"Analyzing FEN: {fen}");
			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(
				fen,           // Position
				-1,            // No time limit (use depth)
				12,            // Search depth
				15,            // Evaluation depth
				1500,          // Engine Elo
				8              // Skill level
			));

			var result = stockfishBridge.LastAnalysisResult;

			if (string.IsNullOrEmpty(result.errorMessage))
			{
				Debug.Log($"<color=green>Best Move: {result.bestMove}</color>");
				Debug.Log($"<color=green>White Win Probability: {result.evaluation:P1}</color>");
				Debug.Log($"<color=green>Side-to-Move Win Probability: {result.stmEvaluation:P1}</color>");
				Debug.Log($"<color=green>Engine Strength: ~{result.approximateElo} Elo</color>");

				if (result.isPromotion)
				{
					Debug.Log($"<color=cyan>Promotion Detected: {result.GetPromotionDescription()}</color>");
				}
			}
			else
			{
				Debug.Log($"<color=red>Analysis Error: {result.errorMessage}</color>");
			}
		}

		/// <summary>
		/// EXAMPLE 2: Promotion position analysis
		/// Shows how to handle positions where promotion is possible
		/// </summary>
		public IEnumerator ExamplePromotionAnalysis()
		{
			Debug.Log("<color=yellow>=== EXAMPLE 2: Promotion Position Analysis ===</color>");

			// Position where white pawn can promote
			string promotionFEN = "r1bqk2r/pppp1Ppp/2n2n2/2b1p3/2B1P3/3P1N2/PPP2PPP/RNBQK2R w KQkq - 0 8";

			Debug.Log($"Analyzing promotion position: {promotionFEN}");

			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(promotionFEN));

			var result = stockfishBridge.LastAnalysisResult;

			if (string.IsNullOrEmpty(result.errorMessage))
			{
				Debug.Log($"<color=green>Engine suggests: {result.bestMove}</color>");

				if (result.isPromotion)
				{
					Debug.Log($"<color=cyan>PROMOTION MOVE DETECTED!</color>");
					Debug.Log($"<color=cyan>From: {ChessBoard.CoordToAlgebraic(result.promotionFrom)}</color>");
					Debug.Log($"<color=cyan>To: {ChessBoard.CoordToAlgebraic(result.promotionTo)}</color>");
					Debug.Log($"<color=cyan>Piece: {result.promotionPiece}</color>");
					Debug.Log($"<color=cyan>Description: {result.GetPromotionDescription()}</color>");
				}
				else
				{
					Debug.Log("<color=white>No promotion in suggested move</color>");
				}
			}
			else
			{
				Debug.Log($"<color=red>Error: {result.errorMessage}</color>");
			}
		}

		/// <summary>
		/// EXAMPLE 3: Engine strength comparison
		/// Shows how different Elo/skill settings affect analysis
		/// </summary>
		public IEnumerator ExampleEngineStrengthComparison()
		{
			Debug.Log("<color=yellow>=== EXAMPLE 3: Engine Strength Comparison ===</color>");

			string testFEN = "r1bqkb1r/pppp1ppp/2n2n2/4p3/2B1P3/3P1N2/PPP2PPP/RNBQK2R w KQkq - 0 4";

			// Test different engine strengths
			var strengthConfigs = new[]
			{
				(400, 0, "Beginner"),
				(1200, 5, "Intermediate"),
				(2000, 12, "Advanced"),
				(-1, -1, "Maximum Strength")
			};

			foreach (var (elo, skill, description) in strengthConfigs)
			{
				Debug.Log($"Testing {description} (Elo: {elo}, Skill: {skill})...");

				yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(
					testFEN, -1, 8, 10, elo, skill));

				var result = stockfishBridge.LastAnalysisResult;

				if (string.IsNullOrEmpty(result.errorMessage))
				{
					Debug.Log($"<color=green>{description}: {result.bestMove} " +
							 $"(~{result.approximateElo} Elo, Eval: {result.stmEvaluation:P1})</color>");
				}
				else
				{
					Debug.Log($"<color=red>{description} failed: {result.errorMessage}</color>");
				}

				yield return new WaitForSeconds(0.2f);
			}
		}

		/// <summary>
		/// EXAMPLE 4: Complete game flow with promotion handling
		/// Shows full integration of human moves, engine moves, and promotion UI
		/// </summary>
		public IEnumerator ExampleCompleteGameFlow()
		{
			Debug.Log("<color=yellow>=== EXAMPLE 4: Complete Game Flow ===</color>");

			// Setup new game
			gameBoard = new ChessBoard();
			gameBoard.SetHumanSide('w');

			Debug.Log("Starting new game - Human plays White");
			Debug.Log($"Initial position: {gameBoard.ToFEN()}");

			// Simulate a few moves leading to promotion scenario
			string[] moves = { "e2e4", "e7e5", "d2d4", "exd4", "c2c3", "dxc3", "bxc3", "d7d6" };

			foreach (string move in moves)
			{
				ChessMove chessMove = ChessMove.FromUCI(move, gameBoard);
				if (chessMove.IsValid())
				{
					gameBoard.MakeMove(chessMove);
					Debug.Log($"Made move: {move}");
				}
			}

			// Now analyze position and get engine move
			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(gameBoard.ToFEN()));

			var result = stockfishBridge.LastAnalysisResult;
			if (!string.IsNullOrEmpty(result.bestMove) && !result.bestMove.StartsWith("ERROR"))
			{
				Debug.Log($"<color=green>Engine suggests: {result.bestMove}</color>");

				if (result.isPromotion)
				{
					Debug.Log($"<color=cyan>Engine promotion: {result.GetPromotionDescription()}</color>");

					// Apply engine promotion move
					ChessMove engineMove = ChessMove.FromUCI(result.bestMove, gameBoard);
					gameBoard.MakeMove(engineMove);
				}
			}

			UpdateStatusDisplay();
		}

		/// <summary>
		/// EXAMPLE 5: Side switching and evaluation from different perspectives
		/// </summary>
		public IEnumerator ExampleSideSwitchingAndEvaluation()
		{
			Debug.Log("<color=yellow>=== EXAMPLE 5: Side Switching and Evaluation ===</color>");

			string middlegameFEN = "r1bqk2r/pppp1ppp/2n2n2/2b1p3/2B1P3/3P1N2/PPP2PPP/RNBQK2R w KQkq - 0 4";

			// Analyze from white's perspective
			gameBoard = new ChessBoard(middlegameFEN);
			gameBoard.SetHumanSide('w');

			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(middlegameFEN));
			var whiteResult = stockfishBridge.LastAnalysisResult;

			Debug.Log($"<color=white>White to move: STM eval = {whiteResult.stmEvaluation:P1}</color>");

			// Switch to black's perspective  
			gameBoard.SetHumanSide('b');

			// Analyze same position but from black's perspective after white moves
			ChessMove whiteMove = ChessMove.FromUCI(whiteResult.bestMove, gameBoard);
			if (whiteMove.IsValid())
			{
				gameBoard.MakeMove(whiteMove);

				yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(gameBoard.ToFEN()));
				var blackResult = stockfishBridge.LastAnalysisResult;

				Debug.Log($"<color=gray>Black to move: STM eval = {blackResult.stmEvaluation:P1}</color>");
				Debug.Log($"<color=yellow>Evaluation perspective correctly switches with side-to-move</color>");
			}
		}

		/// <summary>
		/// Run all usage examples
		/// </summary>
		public IEnumerator RunExamples()
		{
			yield return StartCoroutine(ExampleBasicAnalysis());
			yield return new WaitForSeconds(1f);

			yield return StartCoroutine(ExamplePromotionAnalysis());
			yield return new WaitForSeconds(1f);

			yield return StartCoroutine(ExampleEngineStrengthComparison());
			yield return new WaitForSeconds(1f);

			yield return StartCoroutine(ExampleCompleteGameFlow());
			yield return new WaitForSeconds(1f);

			yield return StartCoroutine(ExampleSideSwitchingAndEvaluation());

			Debug.Log("<color=green>=== ALL EXAMPLES COMPLETED ===</color>");
		}

		#endregion

		#region Human Move Simulation with Promotion

		/// <summary>
		/// Simulate human making a move (with promotion handling)
		/// This shows how to integrate promotion UI into actual gameplay
		/// </summary>
		public void SimulateHumanMove(string uciMove)
		{
			if (gameBoard == null)
			{
				gameBoard = new ChessBoard();
			}

			// Check if it's human's turn
			if (!gameBoard.IsHumanTurn())
			{
				Debug.Log("<color=orange>Not human's turn!</color>");
				return;
			}

			ChessMove move = ChessMove.FromUCI(uciMove, gameBoard);

			if (!move.IsValid())
			{
				Debug.Log($"<color=red>Invalid move: {uciMove}</color>");
				return;
			}

			// Check if move requires promotion
			if (ChessMove.RequiresPromotion(move.from, move.to, move.piece))
			{
				Debug.Log($"<color=cyan>Move requires promotion: {uciMove}</color>");

				// Store pending move and show promotion UI
				pendingPromotionMove = move;
				awaitingPromotion = true;

				bool isWhite = char.IsUpper(move.piece);
				if (promotionUI != null)
				{
					promotionUI.ShowPromotionDialog(isWhite);
				}
				else
				{
					// Fallback: auto-promote to queen
					Debug.Log("<color=orange>No PromotionUI - auto-promoting to Queen</color>");
					CompletePromotionMove('Q');
				}
			}
			else
			{
				// Regular move
				bool success = gameBoard.MakeMove(move);
				if (success)
				{
					Debug.Log($"<color=green>Human move: {uciMove}</color>");
					UpdateStatusDisplay();

					// Get engine response
					StartCoroutine(GetEngineMove());
				}
				else
				{
					Debug.Log($"<color=red>Move failed: {uciMove}</color>");
				}
			}
		}

		/// <summary>
		/// Handle promotion piece selection from UI
		/// </summary>
		private void OnPromotionPieceSelected(char promotionPiece)
		{
			if (!awaitingPromotion)
			{
				Debug.Log("<color=orange>Received promotion selection but not awaiting promotion</color>");
				return;
			}

			CompletePromotionMove(promotionPiece);
		}

		/// <summary>
		/// Complete promotion move with selected piece
		/// </summary>
		private void CompletePromotionMove(char promotionPiece)
		{
			if (!awaitingPromotion || !pendingPromotionMove.IsValid())
			{
				Debug.Log("<color=red>No pending promotion move!</color>");
				return;
			}

			// Create promotion move with selected piece
			ChessMove promotionMove = new ChessMove(
				pendingPromotionMove.from,
				pendingPromotionMove.to,
				pendingPromotionMove.piece,
				promotionPiece,
				pendingPromotionMove.capturedPiece
			);

			bool success = gameBoard.MakeMove(promotionMove);

			awaitingPromotion = false;
			pendingPromotionMove = new ChessMove();

			if (success)
			{
				Debug.Log($"<color=green>Promotion completed: {promotionMove.ToUCI()}</color>");
				UpdateStatusDisplay();

				// Get engine response
				StartCoroutine(GetEngineMove());
			}
			else
			{
				Debug.Log($"<color=red>Promotion move failed!</color>");
			}
		}

		/// <summary>
		/// Get engine move for current position
		/// </summary>
		public IEnumerator GetEngineMove()
		{
			if (!gameBoard.IsEngineTurn())
			{
				Debug.Log("<color=orange>Not engine's turn</color>");
				yield break;
			}

			if (gameBoard.IsGameOver())
			{
				Debug.Log($"<color=yellow>Game Over: {gameBoard.GetGameStatus()}</color>");
				yield break;
			}

			Debug.Log("Getting engine move...");

			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(gameBoard.ToFEN()));

			var result = stockfishBridge.LastAnalysisResult;

			if (!string.IsNullOrEmpty(result.errorMessage))
			{
				Debug.Log($"<color=red>Engine error: {result.errorMessage}</color>");
				yield break;
			}

			if (result.isGameEnd)
			{
				Debug.Log($"<color=yellow>Game ended: {result.bestMove}</color>");
				UpdateStatusDisplay();
				yield break;
			}

			// Apply engine move
			ChessMove engineMove = ChessMove.FromUCI(result.bestMove, gameBoard);

			if (engineMove.IsValid())
			{
				bool success = gameBoard.MakeMove(engineMove);
				if (success)
				{
					Debug.Log($"<color=blue>Engine move: {result.bestMove}</color>");

					if (result.isPromotion)
					{
						Debug.Log($"<color=cyan>Engine promoted: {result.GetPromotionDescription()}</color>");
					}

					UpdateStatusDisplay();
				}
				else
				{
					Debug.Log($"<color=red>Engine move failed: {result.bestMove}</color>");
				}
			}
			else
			{
				Debug.Log($"<color=red>Invalid engine move: {result.bestMove}</color>");
			}
		}

		#endregion

		#region UI and Display

		/// <summary>
		/// Update status and evaluation display
		/// </summary>
		private void UpdateStatusDisplay()
		{
			if (gameBoard == null) return;

			// Update status text
			if (statusText != null)
			{
				string status = gameBoard.GetGameStatus();
				string turnInfo = $"Turn: {(gameBoard.sideToMove == 'w' ? "White" : "Black")}";
				string undoInfo = $"Undo: {gameBoard.GetUndoCount()}, Redo: {gameBoard.GetRedoCount()}";

				statusText.text = $"{status}\n{turnInfo}\n{undoInfo}";
			}

			// Update evaluation display (if available)
			if (evaluationText != null && stockfishBridge != null)
			{
				var lastResult = stockfishBridge.LastAnalysisResult;
				if (lastResult != null)
				{
					string evalText = $"Evaluation:\n" +
									 $"White: {lastResult.evaluation:P1}\n" +
									 $"STM: {lastResult.stmEvaluation:P1}\n" +
									 $"Depth: {lastResult.evaluationDepth}";

					if (lastResult.isMateScore)
					{
						string mateInfo = lastResult.mateDistance > 0 ?
							$"White mates in {lastResult.mateDistance}" :
							$"Black mates in {-lastResult.mateDistance}";
						evalText += $"\n{mateInfo}";
					}

					evaluationText.text = evalText;
				}
			}
		}

		#endregion

		#region Public Interface Methods

		/// <summary>
		/// Start new game with specified human side
		/// </summary>
		public void StartNewGame(char humanSide = 'w')
		{
			gameBoard = new ChessBoard();
			gameBoard.SetHumanSide(humanSide);

			Debug.Log($"<color=green>New game started - Human plays {gameBoard.GetSideName(humanSide)}</color>");
			UpdateStatusDisplay();

			// If human plays black, get engine move first
			if (humanSide == 'b')
			{
				StartCoroutine(GetEngineMove());
			}
		}

		/// <summary>
		/// Load position from FEN
		/// </summary>
		public void LoadPosition(string fen)
		{
			if (gameBoard == null)
				gameBoard = new ChessBoard();

			bool success = gameBoard.LoadFromFEN(fen);
			if (success)
			{
				Debug.Log($"<color=green>Position loaded: {fen}</color>");
				UpdateStatusDisplay();
			}
			else
			{
				Debug.Log($"<color=red>Failed to load FEN: {fen}</color>");
			}
		}

		/// <summary>
		/// Undo last move
		/// </summary>
		public void UndoMove()
		{
			if (gameBoard != null && gameBoard.UndoMove())
			{
				Debug.Log("<color=green>Move undone</color>");
				UpdateStatusDisplay();
			}
			else
			{
				Debug.Log("<color=orange>Cannot undo - no moves to undo</color>");
			}
		}

		/// <summary>
		/// Redo next move
		/// </summary>
		public void RedoMove()
		{
			if (gameBoard != null && gameBoard.RedoMove())
			{
				Debug.Log("<color=green>Move redone</color>");
				UpdateStatusDisplay();
			}
			else
			{
				Debug.Log("<color=orange>Cannot redo - no moves to redo</color>");
			}
		}

		/// <summary>
		/// Switch human side
		/// </summary>
		public void SwitchSides()
		{
			if (gameBoard != null && gameBoard.allowSideSwitching)
			{
				char newSide = gameBoard.humanSide == 'w' ? 'b' : 'w';
				gameBoard.SetHumanSide(newSide);

				Debug.Log($"<color=yellow>Switched sides - Human now plays {gameBoard.GetSideName(newSide)}</color>");
				UpdateStatusDisplay();

				// If it's now engine's turn, get engine move
				if (gameBoard.IsEngineTurn())
				{
					StartCoroutine(GetEngineMove());
				}
			}
			else
			{
				Debug.Log("<color=orange>Side switching not allowed or no active game</color>");
			}
		}

		/// <summary>
		/// Analyze current position with custom settings
		/// </summary>
		public void AnalyzeCurrentPosition(int depth = 15, int elo = -1, int skillLevel = -1)
		{
			if (gameBoard == null)
			{
				Debug.Log("<color=red>No active game board</color>");
				return;
			}

			StartCoroutine(AnalyzePositionCoroutine(depth, elo, skillLevel));
		}

		private IEnumerator AnalyzePositionCoroutine(int depth, int elo, int skillLevel)
		{
			Debug.Log($"<color=cyan>Analyzing position (depth={depth}, elo={elo}, skill={skillLevel})...</color>");

			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(
				gameBoard.ToFEN(), -1, depth, depth + 3, elo, skillLevel));

			var result = stockfishBridge.LastAnalysisResult;

			if (string.IsNullOrEmpty(result.errorMessage))
			{
				Debug.Log($"<color=green>Analysis: {result.bestMove}</color>");
				Debug.Log($"<color=green>Evaluation: {result.stmEvaluation:P1} for {gameBoard.GetSideName(gameBoard.sideToMove)}</color>");

				if (result.isPromotion)
				{
					Debug.Log($"<color=cyan>Suggested promotion: {result.GetPromotionDescription()}</color>");
				}
			}
			else
			{
				Debug.Log($"<color=red>Analysis failed: {result.errorMessage}</color>");
			}

			UpdateStatusDisplay();
		}

		#endregion

		#region Button Event Handlers (for UI integration)

		public void OnNewGameButtonClicked()
		{
			StartNewGame('w');
		}

		public void OnSwitchSidesButtonClicked()
		{
			SwitchSides();
		}

		public void OnUndoButtonClicked()
		{
			UndoMove();
		}

		public void OnRedoButtonClicked()
		{
			RedoMove();
		}

		public void OnAnalyzeButtonClicked()
		{
			AnalyzeCurrentPosition();
		}

		/// <summary>
		/// Example move buttons (connect these to UI buttons)
		/// </summary>
		public void OnMoveE2E4() { SimulateHumanMove("e2e4"); }
		public void OnMoveE7E5() { SimulateHumanMove("e7e5"); }
		public void OnMoveD2D4() { SimulateHumanMove("d2d4"); }

		#endregion

		#region Test Specific Helper Methods

		/// <summary>
		/// Quick test method for individual FEN evaluation
		/// </summary>
		public void TestSingleFEN(string fen)
		{
			StartCoroutine(TestSingleFENCoroutine(fen));
		}

		private IEnumerator TestSingleFENCoroutine(string fen)
		{
			Debug.Log($"<color=cyan>Testing FEN: {fen}</color>");

			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(fen));

			var result = stockfishBridge.LastAnalysisResult;

			if (string.IsNullOrEmpty(result.errorMessage))
			{
				Debug.Log($"<color=green>Best Move: {result.bestMove}</color>");
				Debug.Log($"<color=green>Evaluation: W={result.evaluation:P1}, STM={result.stmEvaluation:P1}</color>");
				Debug.Log($"<color=green>Engine: ~{result.approximateElo} Elo, Depth: {result.searchDepth}/{result.evaluationDepth}</color>");

				if (result.isPromotion)
				{
					Debug.Log($"<color=cyan>Promotion: {result.GetPromotionDescription()}</color>");
				}
			}
			else
			{
				Debug.Log($"<color=red>Error: {result.errorMessage}</color>");
			}
		}

		#endregion
	}
}

/*
MINIMAL SCENE SETUP NOTES:
========================

1. CREATE GAMEOBJECT:
   - Create empty GameObject named "ChessEngine"
   - Add StockfishBridge component
   - Add StockfishUsageExample component
   - Add StockfishTestSuite component

2. STOCKFISHBRIDGE INSPECTOR SETTINGS:
   - Enable Evaluation: true
   - Default Depth: 12
   - Eval Depth: 15
   - Default Elo: 1500
   - Default Skill Level: 8
   - Enable Debug Logging: true

3. PROMOTION UI SETUP (Optional):
   - Create Canvas with PromotionUI prefab
   - Create Panel for promotion dialog
   - Add 4 buttons (Queen, Rook, Bishop, Knight)
   - Assign piece sprites to PromotionUI component
   - Set Auto Select Timeout: 3.0 seconds

4. TESTING:
   - Run in Play mode
   - Check console for colored test results
   - Use TestSingleFEN() for custom position testing
   - Use SimulateHumanMove() for move testing

5. ENGINE EXECUTABLE:
   - Place stockfish.exe in StreamingAssets/sf-engine.exe
   - Ensure executable permissions on non-Windows platforms

EXAMPLE USAGE IN CODE:
====================

// Basic analysis
yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine("your_fen_here"));
var result = stockfishBridge.LastAnalysisResult;

// Check for promotion
if (result.isPromotion) {
    Debug.Log($"Promotion to: {result.promotionPiece}");
}

// Use evaluation
float whiteChance = result.evaluation;
float sideToMoveChance = result.stmEvaluation;
*/
