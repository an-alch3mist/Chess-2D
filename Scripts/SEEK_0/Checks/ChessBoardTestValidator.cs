using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GPTDeepResearch;
using SPACE_UTIL;

namespace GPTDeepResearch
{
	/// <summary>
	/// Comprehensive test validator for enhanced ChessBoard functionality
	/// Tests edge cases, error handling, and integration with chess engine components
	/// </summary>
	public class ChessBoardTestValidator : MonoBehaviour
	{
		[Header("Test Configuration")]
		public bool runTestsOnStart = true;
		public bool includeStressTests = true;
		public int stressTestIterations = 100;

		void Start()
		{
			if (runTestsOnStart)
			{
				StartCoroutine(RunComprehensiveTests());
			}
		}

		/// <summary>
		/// Run all comprehensive tests including edge cases
		/// </summary>
		public IEnumerator RunComprehensiveTests()
		{
			Debug.Log("<color=cyan>=== Starting Comprehensive ChessBoard Validation ===</color>");

			yield return StartCoroutine(TestBasicFunctionality());
			yield return StartCoroutine(TestEngineIntegration());
			yield return StartCoroutine(TestPromotionScenarios());
			yield return StartCoroutine(TestErrorHandling());
			yield return StartCoroutine(TestPerformance());

			if (includeStressTests)
			{
				yield return StartCoroutine(StressTestGameTree());
			}

			// Run built-in ChessBoard tests
			ChessBoard.RunAllTests();

			Debug.Log("<color=green>=== Comprehensive ChessBoard Validation Completed ===</color>");
		}

		/// <summary>
		/// Test basic functionality with edge cases
		/// </summary>
		private IEnumerator TestBasicFunctionality()
		{
			Debug.Log("<color=cyan>[TestValidator] Testing basic functionality...</color>");

			// Test constructor variants
			var board1 = new ChessBoard();
			var board2 = new ChessBoard("startpos");
			var board3 = new ChessBoard("", ChessBoard.ChessVariant.Chess960);

			if (board1.ToFEN() == board2.ToFEN())
			{
				Debug.Log("<color=green>[TestValidator] ✓ Constructor variants work correctly</color>");
			}
			else
			{
				Debug.Log("<color=red>[TestValidator] ✗ Constructor variants failed</color>");
			}

			// Test FEN edge cases
			string[] complexFENs = {
				"r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1", // Complex middle game
                "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1", // Endgame
                "rnbqkb1r/pp1p1ppp/5n2/2p1p3/2B1P3/3P1N2/PPP2PPP/RNBQK2R b KQkq - 0 4" // Italian opening
            };

			foreach (string fen in complexFENs)
			{
				var testBoard = new ChessBoard(fen);
				string roundTrip = testBoard.ToFEN();

				if (fen == roundTrip)
				{
					Debug.Log($"<color=green>[TestValidator] ✓ Complex FEN round-trip: {fen.Substring(0, 20)}...</color>");
				}
				else
				{
					Debug.Log($"<color=yellow>[TestValidator] ? FEN round-trip differs for: {fen.Substring(0, 20)}...</color>");
				}
			}

			yield return null;
		}

		/// <summary>
		/// Test integration with engine components
		/// </summary>
		private IEnumerator TestEngineIntegration()
		{
			Debug.Log("<color=cyan>[TestValidator] Testing engine integration...</color>");

			var board = new ChessBoard();

			// Test evaluation updates
			float[] testEvals = { -150.5f, 0f, 250.75f, -1000f, 1000f };
			float[] testProbs = { 0.25f, 0.5f, 0.75f, 0f, 1f };

			for (int i = 0; i < testEvals.Length; i++)
			{
				board.UpdateEvaluation(testEvals[i], testProbs[i], 0f, i + 5);

				if (Mathf.Abs(board.LastEvaluation - testEvals[i]) < 0.01f &&
					Mathf.Abs(board.LastWinProbability - Mathf.Clamp01(testProbs[i])) < 0.01f)
				{
					Debug.Log($"<color=green>[TestValidator] ✓ Evaluation update {i}: {testEvals[i]}cp, {testProbs[i]} prob</color>");
				}
				else
				{
					Debug.Log($"<color=red>[TestValidator] ✗ Evaluation update {i} failed</color>");
				}
			}

			// Test move history and game tree
			string[] testMoves = { "e2e4", "e7e5", "g1f3", "b8c6", "f1c4", "f8c5" };

			foreach (string uciMove in testMoves)
			{
				var move = ChessMove.FromUCI(uciMove, board);
				if (move.IsValid() && board.MakeMove(move))
				{
					Debug.Log($"<color=green>[TestValidator] ✓ Made move: {uciMove}</color>");
				}
				else
				{
					Debug.Log($"<color=red>[TestValidator] ✗ Failed to make move: {uciMove}</color>");
				}
				yield return null;
			}

			// Test undo/redo chain
			int undoCount = 0;
			while (board.CanUndo() && undoCount < 5)
			{
				if (board.UndoMove())
				{
					undoCount++;
				}
				else
				{
					break;
				}
				yield return null;
			}

			int redoCount = 0;
			while (board.CanRedo() && redoCount < undoCount)
			{
				if (board.RedoMove())
				{
					redoCount++;
				}
				else
				{
					break;
				}
				yield return null;
			}

			if (undoCount > 0 && redoCount == undoCount)
			{
				Debug.Log($"<color=green>[TestValidator] ✓ Undo/Redo chain: {undoCount} operations</color>");
			}
			else
			{
				Debug.Log($"<color=red>[TestValidator] ✗ Undo/Redo chain failed: {undoCount} undos, {redoCount} redos</color>");
			}

			yield return null;
		}

		/// <summary>
		/// Test pawn promotion scenarios
		/// </summary>
		private IEnumerator TestPromotionScenarios()
		{
			Debug.Log("<color=cyan>[TestValidator] Testing promotion scenarios...</color>");

			// Test white promotion
			var board = new ChessBoard("8/P7/8/8/8/8/8/4K2k w - - 0 1");
			var promotionMove = ChessMove.CreatePromotionMove(new v2(0, 6), new v2(0, 7), 'P', 'Q');

			if (board.MakeMove(promotionMove))
			{
				if (board.GetPiece("a8") == 'Q')
				{
					Debug.Log("<color=green>[TestValidator] ✓ White pawn promotion to queen works</color>");
				}
				else
				{
					Debug.Log("<color=red>[TestValidator] ✗ White pawn promotion failed - wrong piece</color>");
				}
			}
			else
			{
				Debug.Log("<color=red>[TestValidator] ✗ White pawn promotion move rejected</color>");
			}

			// Test black promotion with capture
			board.LoadFromFEN("4k3/8/8/8/8/8/1p6/R3K3 b - - 0 1");
			var capturePromotion = ChessMove.CreatePromotionMove(new v2(1, 1), new v2(0, 0), 'p', 'q', 'R');

			if (board.MakeMove(capturePromotion))
			{
				if (board.GetPiece("a1") == 'q')
				{
					Debug.Log("<color=green>[TestValidator] ✓ Black pawn promotion with capture works</color>");
				}
				else
				{
					Debug.Log("<color=red>[TestValidator] ✗ Black pawn promotion with capture failed</color>");
				}
			}
			else
			{
				Debug.Log("<color=red>[TestValidator] ✗ Black pawn promotion with capture rejected</color>");
			}

			// Test all promotion pieces
			char[] promotionPieces = { 'Q', 'R', 'B', 'N' };
			foreach (char piece in promotionPieces)
			{
				board.LoadFromFEN("8/P7/8/8/8/8/8/4K2k w - - 0 1");
				var testMove = ChessMove.CreatePromotionMove(new v2(0, 6), new v2(0, 7), 'P', piece);

				if (ChessMove.IsValidPromotionPiece(piece) && board.MakeMove(testMove))
				{
					Debug.Log($"<color=green>[TestValidator] ✓ Promotion to {ChessMove.GetPromotionPieceName(piece)} works</color>");
				}
				else
				{
					Debug.Log($"<color=red>[TestValidator] ✗ Promotion to {piece} failed</color>");
				}
				yield return null;
			}

			yield return null;
		}

		/// <summary>
		/// Test error handling and edge cases
		/// </summary>
		private IEnumerator TestErrorHandling()
		{
			Debug.Log("<color=cyan>[TestValidator] Testing error handling...</color>");

			var board = new ChessBoard();

			// Test invalid FEN handling
			string[] invalidFENs = {
				null,
				"",
				"invalid_fen_string",
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0", // Missing fullmove
                "rnbqkbnr/pppppppp/9/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", // Invalid rank
                "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNRX w KQkq - 0 1" // Invalid piece
            };

			int rejectedCount = 0;
			foreach (string invalidFEN in invalidFENs)
			{
				var testBoard = new ChessBoard();
				if (!testBoard.LoadFromFEN(invalidFEN))
				{
					rejectedCount++;
				}
				yield return null;
			}

			if (rejectedCount == invalidFENs.Length)
			{
				Debug.Log($"<color=green>[TestValidator] ✓ All {rejectedCount} invalid FENs correctly rejected</color>");
			}
			else
			{
				Debug.Log($"<color=red>[TestValidator] ✗ Only {rejectedCount}/{invalidFENs.Length} invalid FENs rejected</color>");
			}

			// Test invalid move handling
			var invalidMoves = new ChessMove[]
			{
				new ChessMove(new v2(-1, -1), new v2(0, 0), 'P'), // Invalid coordinates
                new ChessMove(new v2(0, 0), new v2(7, 7), 'X'), // Invalid piece
                new ChessMove(new v2(0, 0), new v2(0, 7), 'R') // Impossible rook move
            };

			int rejectedMoves = 0;
			foreach (var invalidMove in invalidMoves)
			{
				if (!board.MakeMove(invalidMove))
				{
					rejectedMoves++;
				}
				yield return null;
			}

			if (rejectedMoves == invalidMoves.Length)
			{
				Debug.Log($"<color=green>[TestValidator] ✓ All {rejectedMoves} invalid moves correctly rejected</color>");
			}
			else
			{
				Debug.Log($"<color=red>[TestValidator] ✗ Only {rejectedMoves}/{invalidMoves.Length} invalid moves rejected</color>");
			}

			// Test out-of-bounds access
			char oobPiece = board.GetPiece(new v2(10, 10));
			if (oobPiece == '.')
			{
				Debug.Log("<color=green>[TestValidator] ✓ Out-of-bounds access handled correctly</color>");
			}
			else
			{
				Debug.Log("<color=red>[TestValidator] ✗ Out-of-bounds access not handled</color>");
			}

			yield return null;
		}

		/// <summary>
		/// Test performance with typical usage patterns
		/// </summary>
		private IEnumerator TestPerformance()
		{
			Debug.Log("<color=cyan>[TestValidator] Testing performance...</color>");

			float startTime = Time.realtimeSinceStartup;

			// Test rapid FEN parsing
			string testFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
			for (int i = 0; i < 1000; i++)
			{
				var board = new ChessBoard(testFEN);
				if (i % 100 == 0) yield return null; // Don't block frame
			}

			float fenTime = Time.realtimeSinceStartup - startTime;
			Debug.Log($"<color=green>[TestValidator] FEN parsing: 1000 parses in {fenTime * 1000f:F1}ms</color>");

			// Test position hashing performance
			startTime = Time.realtimeSinceStartup;
			var hashBoard = new ChessBoard();

			for (int i = 0; i < 1000; i++)
			{
				ulong hash = hashBoard.CalculatePositionHash();
				if (hash == 0)
				{
					Debug.Log("<color=red>[TestValidator] ✗ Position hash returned 0</color>");
					break;
				}
				if (i % 100 == 0) yield return null;
			}

			float hashTime = Time.realtimeSinceStartup - startTime;
			Debug.Log($"<color=green>[TestValidator] Position hashing: 1000 hashes in {hashTime * 1000f:F1}ms</color>");

			// Test move generation performance
			startTime = Time.realtimeSinceStartup;
			for (int i = 0; i < 100; i++)
			{
				var moves = hashBoard.GetLegalMoves();
				if (moves.Count == 0)
				{
					Debug.Log("<color=red>[TestValidator] ✗ No legal moves generated</color>");
					break;
				}
				if (i % 10 == 0) yield return null;
			}

			float moveTime = Time.realtimeSinceStartup - startTime;
			Debug.Log($"<color=green>[TestValidator] Move generation: 100 generations in {moveTime * 1000f:F1}ms</color>");

			yield return null;
		}

		/// <summary>
		/// Stress test game tree with many moves
		/// </summary>
		private IEnumerator StressTestGameTree()
		{
			Debug.Log("<color=cyan>[TestValidator] Running stress test...</color>");

			var board = new ChessBoard();
			int successfulMoves = 0;
			int maxMoves = stressTestIterations;

			// Play random legal moves to stress test the game tree
			for (int i = 0; i < maxMoves; i++)
			{
				var legalMoves = board.GetLegalMoves();
				if (legalMoves.Count == 0)
				{
					Debug.Log($"<color=yellow>[TestValidator] Game ended at move {i} - no legal moves</color>");
					break;
				}

				// Pick a random legal move
				int randomIndex = Random.Range(0, legalMoves.Count);
				var randomMove = legalMoves[randomIndex];

				if (board.MakeMove(randomMove))
				{
					successfulMoves++;

					// Test hash consistency every 10 moves
					if (i % 10 == 0)
					{
						ulong hash = board.CalculatePositionHash();
						if (hash == 0)
						{
							Debug.Log($"<color=red>[TestValidator] ✗ Hash calculation failed at move {i}</color>");
							break;
						}
					}

					// Test undo occasionally
					if (i % 25 == 0 && board.CanUndo())
					{
						if (board.UndoMove() && board.CanRedo())
						{
							board.RedoMove();
						}
					}

					// Check game result
					var result = board.GetGameResult();
					if (result != ChessRules.GameResult.InProgress)
					{
						Debug.Log($"<color=yellow>[TestValidator] Game ended at move {i}: {result}</color>");
						break;
					}
				}
				else
				{
					Debug.Log($"<color=red>[TestValidator] ✗ Failed to make legal move at iteration {i}</color>");
					break;
				}

				// Don't block the frame
				if (i % 5 == 0) yield return null;
			}

			if (successfulMoves > 10)
			{
				Debug.Log($"<color=green>[TestValidator] ✓ Stress test completed: {successfulMoves} moves played</color>");
				Debug.Log($"<color=green>[TestValidator] ✓ Game tree size: {board.GameTreeNodeCount} nodes</color>");
			}
			else
			{
				Debug.Log($"<color=red>[TestValidator] ✗ Stress test failed after {successfulMoves} moves</color>");
			}

			yield return null;
		}

		/// <summary>
		/// Manual test trigger for inspector
		/// </summary>
		[ContextMenu("Run Tests")]
		public void RunTestsManually()
		{
			StartCoroutine(RunComprehensiveTests());
		}

		/// <summary>
		/// Test specific FEN position
		/// </summary>
		[ContextMenu("Test Custom FEN")]
		public void TestCustomFEN()
		{
			string testFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

			var board = new ChessBoard(testFEN);
			Debug.Log($"<color=cyan>Testing FEN: {testFEN}</color>");
			Debug.Log($"<color=green>Generated FEN: {board.ToFEN()}</color>");
			Debug.Log($"<color=green>Legal moves: {board.GetLegalMoves().Count}</color>");
			Debug.Log($"<color=green>Position hash: {board.CalculatePositionHash()}</color>");
		}
	}
}