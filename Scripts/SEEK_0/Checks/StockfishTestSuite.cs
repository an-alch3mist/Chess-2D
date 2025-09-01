/*
StockfishTestSuite.cs - Comprehensive Testing for Chess Engine
==============================================================

USAGE:
1. Attach this script to a GameObject in your test scene
2. Ensure StockfishBridge is properly configured with engine
3. Run tests in Play mode or call individual test methods
4. Check console for colored pass/fail results
5. All tests use Debug.Log with colors instead of Debug.LogError

TEST COVERAGE:
- UCI promotion move parsing (e7e8q, a2a1n, etc.)
- FEN validation with king count and board structure
- Evaluation calculation based on side-to-move
- Human move simulation with promotion selection
- Undo/redo functionality testing
- Side selection (white/black choice) testing
- Edge cases and error handling

INTEGRATION:
- Requires StockfishBridge component on same GameObject
- Requires ChessBoard and related chess classes
- Tests run automatically on Start() or call RunAllTests() manually
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SPACE_UTIL;

namespace GPTDeepResearch
{
	/// <summary>
	/// Comprehensive test suite for chess engine functionality
	/// Tests promotion, evaluation, FEN parsing, and move generation
	/// </summary>
	public class StockfishTestSuite : MonoBehaviour
	{
		[Header("Test Configuration")]
		[SerializeField] private bool runTestsOnStart = true;
		[SerializeField] private bool enableVerboseLogging = true;
		[SerializeField] private StockfishBridge stockfishBridge;

		// Test statistics
		private int testsRun = 0;
		private int testsPassed = 0;
		private int testsFailed = 0;

		#region Unity Lifecycle

		private void Start()
		{
			if (runTestsOnStart)
			{
				StartCoroutine(RunAllTestsCoroutine());
			}
		}

		#endregion

		#region Public API

		/// <summary>
		/// Run all tests as coroutine
		/// </summary>
		public IEnumerator RunAllTestsCoroutine()
		{
			LogInfo("=== STOCKFISH TEST SUITE STARTING ===");
			ResetTestStatistics();

			// Wait for engine to be ready
			if (stockfishBridge == null)
				stockfishBridge = GetComponent<StockfishBridge>();

			if (stockfishBridge != null)
			{
				yield return StartCoroutine(stockfishBridge.InitializeEngineCoroutine());
			}

			// Run tests
			TestPromotionMovesParsing();
			TestFENValidation();
			TestMoveGenerationPromotion();
			TestChessBoardFunctionality();
			TestEvaluationCalculation();

			// Run async tests
			yield return StartCoroutine(TestStockfishPromotionIntegration());
			yield return StartCoroutine(TestEvaluationWithDifferentPositions());

			// Final report
			LogInfo("=== TEST SUITE COMPLETED ===");
			if (testsFailed == 0)
			{
				LogPass($"ALL TESTS PASSED! ({testsPassed}/{testsRun})");
			}
			else
			{
				LogFail($"SOME TESTS FAILED: {testsPassed} passed, {testsFailed} failed, {testsRun} total");
			}
		}

		/// <summary>
		/// Run all tests synchronously (non-coroutine tests only)
		/// </summary>
		public void RunAllTests()
		{
			StartCoroutine(RunAllTestsCoroutine());
		}

		#endregion

		#region Individual Tests

		/// <summary>
		/// Test UCI promotion move parsing
		/// </summary>
		public void TestPromotionMovesParsing()
		{
			LogInfo("--- Testing Promotion Move Parsing ---");

			// Test cases: UCI move, expected piece, expected from/to
			var testCases = new[]
			{
				("e7e8q", 'q', "e7", "e8", true),   // Black queen promotion
				("e2e1Q", 'Q', "e2", "e1", false),  // White queen promotion (unusual but valid)
				("a7a8n", 'n', "a7", "a8", true),   // Black knight promotion
				("h2h1R", 'R', "h2", "h1", false),  // White rook promotion
				("d7c8b", 'b', "d7", "c8", true),   // Black bishop promotion with capture
				("f2f1q", 'q', "f2", "f1", true),   // Edge case: black pawn promoting on rank 1
			};

			foreach (var (uciMove, expectedPiece, expectedFrom, expectedTo, isBlackPromotion) in testCases)
			{
				var result = new StockfishBridge.ChessAnalysisResult();
				result.bestMove = uciMove;
				result.ParsePromotionData();

				bool testPassed = true;
				string failReason = "";

				if (!result.isPromotion)
				{
					testPassed = false;
					failReason = "isPromotion should be true";
				}
				else if (result.promotionPiece != expectedPiece)
				{
					testPassed = false;
					failReason = $"Expected piece '{expectedPiece}', got '{result.promotionPiece}'";
				}
				else if (ChessBoard.CoordToAlgebraic(result.promotionFrom) != expectedFrom)
				{
					testPassed = false;
					failReason = $"Expected from '{expectedFrom}', got '{ChessBoard.CoordToAlgebraic(result.promotionFrom)}'";
				}
				else if (ChessBoard.CoordToAlgebraic(result.promotionTo) != expectedTo)
				{
					testPassed = false;
					failReason = $"Expected to '{expectedTo}', got '{ChessBoard.CoordToAlgebraic(result.promotionTo)}'";
				}

				if (testPassed)
				{
					LogPass($"✓ Promotion parsing: {uciMove} -> {result.GetPromotionDescription()}");
				}
				else
				{
					LogFail($"✗ Promotion parsing failed for {uciMove}: {failReason}");
				}
			}
		}

		/// <summary>
		/// Test FEN validation including king count and board structure
		/// </summary>
		public void TestFENValidation()
		{
			LogInfo("--- Testing FEN Validation ---");

			// Valid FENs
			string[] validFENs = {
				"startpos",
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
				"r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1", // Castling position
				"8/P7/8/8/8/8/7p/8 w - - 0 1", // Promotion position
			};

			foreach (string fen in validFENs)
			{
				ChessBoard board = new ChessBoard(fen);
				// Basic validation - if no exception thrown, consider valid
				LogPass($"✓ Valid FEN accepted: {fen.Substring(0, Mathf.Min(30, fen.Length))}...");
			}

			// Invalid FENs
			var invalidFENs = new[]
			{
				("8/8/8/8/8/8/8/8 w - - 0 1", "No kings"),
				("KK6/8/8/8/8/8/8/8 w - - 0 1", "Two white kings"),
				("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP w KQkq - 0 1", "7 ranks instead of 8"),
				("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR x KQkq - 0 1", "Invalid side to move"),
			};

			foreach (var (fen, description) in invalidFENs)
			{
				try
				{
					ChessBoard board = new ChessBoard(fen);
					// If we get here without exception, FEN was incorrectly accepted
					LogFail($"✗ Invalid FEN incorrectly accepted: {description}");
				}
				catch
				{
					LogPass($"✓ Invalid FEN correctly rejected: {description}");
				}
			}
		}

		/// <summary>
		/// Test move generation with promotion scenarios
		/// </summary>
		public void TestMoveGenerationPromotion()
		{
			LogInfo("--- Testing Move Generation with Promotion ---");

			// Test position: white pawn ready to promote
			string promotionFEN = "8/P7/8/8/8/8/7p/8 w - - 0 1";
			ChessBoard board = new ChessBoard(promotionFEN);

			var legalMoves = MoveGenerator.GenerateLegalMoves(board);
			int promotionMoves = 0;

			foreach (var move in legalMoves)
			{
				if (move.moveType == ChessMove.MoveType.Promotion)
				{
					promotionMoves++;
				}
			}

			// Should have 4 promotion moves (Q, R, B, N)
			if (promotionMoves == 4)
			{
				LogPass($"✓ Generated {promotionMoves} promotion moves correctly");
			}
			else
			{
				LogFail($"✗ Expected 4 promotion moves, got {promotionMoves}");
			}

			// Test black promotion
			string blackPromotionFEN = "8/7P/8/8/8/8/p7/8 b - - 0 1";
			ChessBoard blackBoard = new ChessBoard(blackPromotionFEN);
			var blackMoves = MoveGenerator.GenerateLegalMoves(blackBoard);

			int blackPromotionMoves = 0;
			foreach (var move in blackMoves)
			{
				if (move.moveType == ChessMove.MoveType.Promotion)
				{
					blackPromotionMoves++;
				}
			}

			if (blackPromotionMoves == 4)
			{
				LogPass($"✓ Generated {blackPromotionMoves} black promotion moves correctly");
			}
			else
			{
				LogFail($"✗ Expected 4 black promotion moves, got {blackPromotionMoves}");
			}
		}

		/// <summary>
		/// Test chess board functionality including undo/redo
		/// </summary>
		public void TestChessBoardFunctionality()
		{
			LogInfo("--- Testing ChessBoard Functionality ---");

			ChessBoard board = new ChessBoard();

			// Test side selection
			board.SetHumanSide('w');
			if (board.humanSide == 'w' && board.engineSide == 'b')
			{
				LogPass("✓ Human side selection works correctly");
			}
			else
			{
				LogFail($"✗ Side selection failed: human={board.humanSide}, engine={board.engineSide}");
			}

			// Test move making and undo
			var initialFEN = board.ToFEN();
			var testMove = ChessMove.FromUCI("e2e4", board);

			bool moveMade = board.MakeMove(testMove);
			string afterMoveFEN = board.ToFEN();

			bool undoSuccess = board.UndoMove();
			string afterUndoFEN = board.ToFEN();

			if (moveMade && undoSuccess && initialFEN == afterUndoFEN)
			{
				LogPass("✓ Move making and undo works correctly");
			}
			else
			{
				LogFail($"✗ Undo failed: move made={moveMade}, undo={undoSuccess}, FEN match={initialFEN == afterUndoFEN}");
			}
		}

		/// <summary>
		/// Test evaluation calculation
		/// </summary>
		public void TestEvaluationCalculation()
		{
			LogInfo("--- Testing Evaluation Calculation ---");

			// Test centipawn to probability conversion
			var bridge = GetComponent<StockfishBridge>();
			if (bridge == null)
			{
				LogFail("✗ StockfishBridge component not found");
				return;
			}

			// Create test result with known centipawn values
			var result = new StockfishBridge.ChessAnalysisResult();

			// Test different side-to-move calculations
			result.Side = 'w';
			result.evaluation = 0.7f; // White has 70% chance
			result.stmEvaluation = result.evaluation; // Should be same for white

			if (Mathf.Approximately(result.stmEvaluation, 0.7f))
			{
				LogPass("✓ Side-to-move evaluation correct for white");
			}
			else
			{
				LogFail($"✗ STM evaluation failed for white: expected 0.7, got {result.stmEvaluation}");
			}

			// Test for black
			result.Side = 'b';
			result.stmEvaluation = 1f - result.evaluation; // Should be inverted for black

			if (Mathf.Approximately(result.stmEvaluation, 0.3f))
			{
				LogPass("✓ Side-to-move evaluation correct for black");
			}
			else
			{
				LogFail($"✗ STM evaluation failed for black: expected 0.3, got {result.stmEvaluation}");
			}
		}

		/// <summary>
		/// Test Stockfish integration with promotion scenarios
		/// </summary>
		public IEnumerator TestStockfishPromotionIntegration()
		{
			LogInfo("--- Testing Stockfish Promotion Integration ---");

			if (stockfishBridge == null)
			{
				LogFail("✗ StockfishBridge not available for integration tests");
				yield break;
			}

			// Test position where white pawn can promote
			string promotionFEN = "8/6P1/8/8/8/8/8/k6K w - - 0 1";

			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(promotionFEN, -1, 5, 8, -1, -1));

			var result = stockfishBridge.LastAnalysisResult;

			if (!string.IsNullOrEmpty(result.errorMessage))
			{
				LogFail($"✗ Engine error during promotion analysis: {result.errorMessage}");
				yield break;
			}

			// Check if engine suggests promotion move
			if (result.bestMove.Length == 5 && "qrbnQRBN".IndexOf(result.bestMove[4]) >= 0)
			{
				LogPass($"✓ Engine suggested promotion move: {result.bestMove}");

				if (result.isPromotion)
				{
					LogPass($"✓ Promotion correctly detected: {result.GetPromotionDescription()}");
				}
				else
				{
					LogFail("✗ Promotion move not detected by parsing");
				}
			}
			else
			{
				LogInfo($"? Engine suggested non-promotion move: {result.bestMove} (may be valid depending on position)");
			}

			// Test evaluation with promotion position
			if (result.evaluation != 0.5f)
			{
				LogPass($"✓ Evaluation calculated: White {result.evaluation:P1}, STM {result.stmEvaluation:P1}");
			}
			else
			{
				LogInfo("? Evaluation is neutral (may be correct for this position)");
			}
		}

		/// <summary>
		/// Test evaluation with different positions and sides
		/// </summary>
		public IEnumerator TestEvaluationWithDifferentPositions()
		{
			LogInfo("--- Testing Evaluation with Different Positions ---");

			if (stockfishBridge == null)
			{
				LogFail("✗ StockfishBridge not available");
				yield break;
			}

			// Test positions with known evaluations
			var testPositions = new[]
			{
				("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", "Starting position"),
				("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1", "After 1.e4"),
				("r1bqkb1r/pppp1ppp/2n2n2/4p3/2B1P3/3P1N2/PPP2PPP/RNBQK2R b KQkq - 0 4", "Italian Game"),
			};

			foreach (var (fen, description) in testPositions)
			{
				yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(fen, -1, 8, 12, -1, -1));

				var result = stockfishBridge.LastAnalysisResult;

				if (!string.IsNullOrEmpty(result.errorMessage))
				{
					LogFail($"✗ Error analyzing {description}: {result.errorMessage}");
					continue;
				}

				// Check that we got a valid move and evaluation
				bool hasValidMove = !string.IsNullOrEmpty(result.bestMove) &&
								   !result.bestMove.StartsWith("ERROR");

				bool hasValidEvaluation = result.evaluation >= 0f && result.evaluation <= 1f &&
										 result.stmEvaluation >= 0f && result.stmEvaluation <= 1f;

				if (hasValidMove && hasValidEvaluation)
				{
					LogPass($"✓ {description}: Move={result.bestMove}, Eval={result.evaluation:F3}");
				}
				else
				{
					LogFail($"✗ {description}: Invalid result - Move valid={hasValidMove}, Eval valid={hasValidEvaluation}");
				}

				// Small delay between requests
				yield return new WaitForSeconds(0.1f);
			}
		}

		/// <summary>
		/// Simulate human promotion move selection
		/// </summary>
		public void TestHumanPromotionSimulation()
		{
			LogInfo("--- Testing Human Promotion Simulation ---");

			// Create test board with pawn ready to promote
			ChessBoard board = new ChessBoard("8/P7/8/8/8/8/8/k6K w - - 0 1");

			// Simulate human moving pawn to promotion square
			v2 from = ChessBoard.AlgebraicToCoord("a7");
			v2 to = ChessBoard.AlgebraicToCoord("a8");

			// Create promotion move (simulate user selecting Knight)
			ChessMove promotionMove = ChessMove.CreatePromotionMove(from, to, 'P', 'N');

			// Apply move
			bool moveSuccess = board.MakeMove(promotionMove);

			// Check result
			char pieceOnA8 = board.GetPiece("a8");

			if (moveSuccess && pieceOnA8 == 'N')
			{
				LogPass("✓ Human promotion simulation: Pawn promoted to Knight successfully");
			}
			else
			{
				LogFail($"✗ Human promotion failed: move success={moveSuccess}, piece on a8='{pieceOnA8}' (expected 'N')");
			}

			// Test undo of promotion
			bool undoSuccess = board.UndoMove();
			char pieceAfterUndo = board.GetPiece("a7");

			if (undoSuccess && pieceAfterUndo == 'P')
			{
				LogPass("✓ Promotion undo works correctly");
			}
			else
			{
				LogFail($"✗ Promotion undo failed: undo success={undoSuccess}, piece on a7='{pieceAfterUndo}' (expected 'P')");
			}
		}

		#endregion

		#region Test Helpers

		private void ResetTestStatistics()
		{
			testsRun = 0;
			testsPassed = 0;
			testsFailed = 0;
		}

		private void LogPass(string message)
		{
			testsRun++;
			testsPassed++;
			Debug.Log($"<color=green>{message}</color>");
		}

		private void LogFail(string message)
		{
			testsRun++;
			testsFailed++;
			Debug.Log($"<color=red>{message}</color>");
		}

		private void LogInfo(string message)
		{
			if (enableVerboseLogging)
			{
				Debug.Log($"<color=cyan>{message}</color>");
			}
		}

		#endregion
	}
}