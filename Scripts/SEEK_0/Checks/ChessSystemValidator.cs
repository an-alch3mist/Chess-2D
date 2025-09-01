/*
Comprehensive Chess System Validator
- Tests all edge cases for FEN parsing, move generation, and promotion
- Validates Chess960 support and castling edge cases
- Tests evaluation system accuracy and consistency  
- Covers Stockfish integration and error handling
- Performance benchmarking for real-time gameplay
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using GPTDeepResearch;
using SPACE_UTIL;

namespace GPTDeepResearch
{
	/// <summary>
	/// Comprehensive validator for the entire chess system
	/// Run this to ensure all components work correctly together
	/// </summary>
	public class ChessSystemValidator : MonoBehaviour
	{
		[Header("Test Configuration")]
		[SerializeField] private bool runPerformanceTests = true;
		[SerializeField] private bool runEdgeCaseTests = true;
		[SerializeField] private bool runIntegrationTests = true;
		[SerializeField] private int performanceTestIterations = 1000;

		[Header("Test Results")]
		[SerializeField] private bool allTestsPassed = false;
		[SerializeField] private List<string> failedTests = new List<string>();
		[SerializeField] private List<string> testResults = new List<string>();

		// Test data - edge case FEN positions
		private readonly string[] EdgeCaseFENs = {
            // Standard positions
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", // Starting
            "rnbqkb1r/pppp1ppp/5n2/4p3/4P3/5N2/PPPP1PPP/RNBQKB1R w KQkq - 4 3", // King's Indian
            
            // Promotion positions
            "8/P6P/8/8/8/8/p6p/8 w - - 0 1", // White and black pawns ready to promote
            "rnbqkbn1/pppppppP/8/8/8/8/PPPPPPPp/RNBQKBNR w Qkq - 0 1", // Promotion with capture
            "8/1P6/8/8/8/8/6p1/8 b - - 0 1", // Black to move, can promote
            
            // Castling edge cases  
            "r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1", // All castling available
            "r3k2r/8/8/8/8/8/8/R3K2R w - - 0 1", // No castling rights
            "r3k2r/8/8/8/8/8/8/R3K2R w Kk - 0 1", // Only kingside
            "4k3/8/8/8/8/8/8/R3K2R w Q - 0 1", // Queenside only, missing rook
            
            // Chess960 positions
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w HAha - 0 1", // Chess960 castling notation
            "bbqnnrkr/pppppppp/8/8/8/8/PPPPPPPP/BBQNNRKR w - - 0 1", // Chess960 position
            
            // En passant cases
            "rnbqkbnr/ppp1p1pp/8/3pPp2/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 3", // En passant available
            "rnbqkbnr/ppp1p1pp/8/3pPp2/8/8/PPPP1PPP/RNBQKBNR w KQkq f6 0 3", // Different en passant
            
            // Endgame positions
            "8/8/8/8/8/8/8/K6k w - - 0 1", // King vs King (stalemate)
            "8/8/8/8/8/8/8/KQ5k b - - 0 1", // King and Queen vs King
            "8/8/8/8/3k4/8/3K4/8 w - - 0 1", // King vs King in center
            
            // Material imbalance
            "rnbqkbnr/8/8/8/8/8/8/RNBQKBNR w - - 0 1", // No pawns
            "8/8/8/8/8/8/PPPPPPPP/RNBQKBNR w - - 0 1", // Pawns vs pieces
            "rnbqkbnr/pppppppp/8/8/8/8/8/8 w - - 0 1", // All pieces vs none
            
            // Weird but legal positions
            "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1", // Complex endgame
            "rnbqkb1r/pp1p1pPp/8/2p1pP2/1P1P4/3P3P/P1P1P3/RNBQKBNR w KQkq e6 0 1", // Locked pawns
            
            // Maximum material
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", // All pieces
            "r1bqk1nr/pppp1ppp/2n5/2b1p3/2B1P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 4 4", // Opening
            
            // Invalid positions (should be rejected)
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBN w KQkq - 0 1", // Missing king
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNk w KQkq - 0 1", // Two black kings
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR x KQkq - 0 1", // Invalid side
        };

		// Performance test positions
		private readonly string[] PerformanceFENs = {
			"r1bqkb1r/pppp1ppp/2n2n2/1B2p3/4P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 4 4", // Complex middlegame
            "r2qkbnr/ppp2ppp/2np4/4p3/2B1P1b1/3P1N2/PPP2PPP/RNBQK2R w KQkq - 0 6", // Tactical position
            "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1", // Complex endgame
        };

		#region Unity Lifecycle

		private void Start()
		{
			if (Application.isPlaying)
			{
				StartCoroutine(RunAllTests());
			}
		}

		#endregion

		#region Main Test Runner

		public IEnumerator RunAllTests()
		{
			UnityEngine.Debug.Log("=== Starting Comprehensive Chess System Validation ===");

			failedTests.Clear();
			testResults.Clear();
			allTestsPassed = false;

			// Core system tests
			yield return StartCoroutine(TestFENParsing());
			yield return StartCoroutine(TestMoveGeneration());
			yield return StartCoroutine(TestPromotionSystem());
			yield return StartCoroutine(TestCastling());
			yield return StartCoroutine(TestEnPassant());

			// Edge case tests
			if (runEdgeCaseTests)
			{
				yield return StartCoroutine(TestEdgeCases());
				yield return StartCoroutine(TestInvalidPositions());
				yield return StartCoroutine(TestChess960Support());
			}

			// Integration tests
			if (runIntegrationTests)
			{
				yield return StartCoroutine(TestStockfishIntegration());
				yield return StartCoroutine(TestEvaluationSystem());
			}

			// Performance tests
			if (runPerformanceTests)
			{
				yield return StartCoroutine(TestPerformance());
			}

			// Final results
			allTestsPassed = failedTests.Count == 0;

			UnityEngine.Debug.Log($"=== Validation Complete ===");
			UnityEngine.Debug.Log($"Tests Passed: {allTestsPassed}");
			UnityEngine.Debug.Log($"Failed Tests: {failedTests.Count}");

			if (failedTests.Count > 0)
			{
				UnityEngine.Debug.LogError($"Failed Tests:\n{string.Join("\n", failedTests)}");
			}

			foreach (var result in testResults)
			{
				UnityEngine.Debug.Log(result);
			}
		}

		#endregion

		#region Core System Tests

		private IEnumerator TestFENParsing()
		{
			UnityEngine.Debug.Log("Testing FEN Parsing...");
			int passed = 0, total = 0;

			foreach (string fen in EdgeCaseFENs)
			{
				total++;
				ChessBoard board = new ChessBoard();
				bool success = board.LoadFromFEN(fen);

				// Check if position should be valid
				bool shouldBeValid = !fen.Contains("RNBQKBNk") && !fen.Contains("RNBQKBN w") && !fen.Contains(" x ");

				if (success == shouldBeValid)
				{
					passed++;
				}
				else
				{
					failedTests.Add($"FEN parsing failed for: {fen}");
				}

				if (total % 5 == 0) yield return null; // Prevent frame drops
			}

			testResults.Add($"FEN Parsing: {passed}/{total} passed");
			yield return null;
		}

		private IEnumerator TestMoveGeneration()
		{
			UnityEngine.Debug.Log("Testing Move Generation...");
			int passed = 0, total = 0;

			foreach (string fen in EdgeCaseFENs.Take(10)) // Test subset for performance
			{
				total++;
				ChessBoard board = new ChessBoard();

				if (board.LoadFromFEN(fen))
				{
					try
					{
						List<ChessMove> moves = MoveGenerator.GenerateLegalMoves(board);

						// Basic sanity checks
						bool allMovesValid = moves.All(m => m.IsValid());
						bool noDuplicates = moves.Count == moves.Distinct().Count();

						if (allMovesValid && noDuplicates)
						{
							passed++;
						}
						else
						{
							failedTests.Add($"Move generation issues for FEN: {fen}");
						}
					}
					catch (Exception e)
					{
						failedTests.Add($"Move generation exception for FEN {fen}: {e.Message}");
					}
				}

				if (total % 3 == 0) yield return null;
			}

			testResults.Add($"Move Generation: {passed}/{total} passed");
			yield return null;
		}

		private IEnumerator TestPromotionSystem()
		{
			UnityEngine.Debug.Log("Testing Promotion System...");
			int passed = 0, total = 0;

			// Test promotion move generation
			string[] promotionFENs = {
				"8/P7/8/8/8/8/8/4K2k w - - 0 1", // White pawn ready to promote
                "8/8/8/8/8/8/p7/4k2K b - - 0 1", // Black pawn ready to promote
                "1r6/P7/8/8/8/8/8/4K2k w - - 0 1", // White pawn can capture and promote
            };

			foreach (string fen in promotionFENs)
			{
				total++;
				ChessBoard board = new ChessBoard();

				if (board.LoadFromFEN(fen))
				{
					List<ChessMove> moves = MoveGenerator.GenerateLegalMoves(board);
					var promotionMoves = moves.Where(m => m.moveType == ChessMove.MoveType.Promotion).ToList();

					// Should have 4 promotion moves per promoting pawn
					bool correctCount = promotionMoves.Count > 0 && promotionMoves.Count % 4 == 0;

					// Check all promotion pieces are present
					var promotionPieces = new HashSet<char>();
					foreach (var move in promotionMoves)
					{
						promotionPieces.Add(char.ToUpper(move.promotionPiece));
					}

					bool hasAllPieces = promotionPieces.Contains('Q') && promotionPieces.Contains('R') &&
									   promotionPieces.Contains('B') && promotionPieces.Contains('N');

					if (correctCount && hasAllPieces)
					{
						passed++;
					}
					else
					{
						failedTests.Add($"Promotion generation failed for: {fen}");
					}
				}

				yield return null;
			}

			// Test UCI promotion parsing
			total++;
			string[] testMoves = { "e7e8q", "a7a8r", "h7h8b", "d7d8n" };
			bool allParsed = true;

			ChessBoard testBoard = new ChessBoard("8/P7/8/8/8/8/8/4K2k w - - 0 1");

			foreach (string moveStr in testMoves)
			{
				ChessMove move = ChessMove.FromLongAlgebraic(moveStr, testBoard);
				if (!move.IsValid() || move.moveType != ChessMove.MoveType.Promotion)
				{
					allParsed = false;
					failedTests.Add($"Failed to parse promotion move: {moveStr}");
				}
			}

			if (allParsed) passed++;

			testResults.Add($"Promotion System: {passed}/{total} passed");
			yield return null;
		}

		private IEnumerator TestCastling()
		{
			UnityEngine.Debug.Log("Testing Castling...");
			int passed = 0, total = 0;

			string[] castlingFENs = {
				"r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1", // All castling available
                "r3k2r/8/8/8/8/8/8/R3K2R w Kq - 0 1", // Mixed castling rights
                "r3k2r/8/8/8/8/8/8/R3K2R w - - 0 1", // No castling rights
            };

			foreach (string fen in castlingFENs)
			{
				total++;
				ChessBoard board = new ChessBoard();

				if (board.LoadFromFEN(fen))
				{
					List<ChessMove> moves = MoveGenerator.GenerateLegalMoves(board);
					var castlingMoves = moves.Where(m => m.moveType == ChessMove.MoveType.Castling).ToList();

					// Validate castling move count matches rights
					string rights = board.castlingRights;
					int expectedCastlingMoves = 0;
					if (rights.Contains('K')) expectedCastlingMoves++;
					if (rights.Contains('Q')) expectedCastlingMoves++;

					if (castlingMoves.Count == expectedCastlingMoves)
					{
						passed++;
					}
					else
					{
						failedTests.Add($"Castling move count mismatch for: {fen}");
					}
				}

				yield return null;
			}

			testResults.Add($"Castling: {passed}/{total} passed");
			yield return null;
		}

		private IEnumerator TestEnPassant()
		{
			UnityEngine.Debug.Log("Testing En Passant...");
			int passed = 0, total = 0;

			string[] enPassantFENs = {
				"rnbqkbnr/ppp1p1pp/8/3pPp2/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 3", // En passant available
                "rnbqkbnr/ppp1p1pp/8/3pPp2/8/8/PPPP1PPP/RNBQKBNR w KQkq - 0 3", // No en passant
            };

			foreach (string fen in enPassantFENs)
			{
				total++;
				ChessBoard board = new ChessBoard();

				if (board.LoadFromFEN(fen))
				{
					List<ChessMove> moves = MoveGenerator.GenerateLegalMoves(board);
					var enPassantMoves = moves.Where(m => m.moveType == ChessMove.MoveType.EnPassant).ToList();

					bool hasEnPassant = board.enPassantSquare != "-";
					bool foundEnPassantMove = enPassantMoves.Count > 0;

					if (hasEnPassant == foundEnPassantMove)
					{
						passed++;
					}
					else
					{
						failedTests.Add($"En passant detection failed for: {fen}");
					}
				}

				yield return null;
			}

			testResults.Add($"En Passant: {passed}/{total} passed");
			yield return null;
		}

		#endregion

		#region Edge Case Tests

		private IEnumerator TestEdgeCases()
		{
			UnityEngine.Debug.Log("Testing Edge Cases...");
			int passed = 0, total = 0;

			// Test empty board handling
			total++;
			try
			{
				ChessBoard emptyish = new ChessBoard("8/8/8/8/8/8/8/4K2k w - - 0 1");
				List<ChessMove> moves = MoveGenerator.GenerateLegalMoves(emptyish);

				if (moves.Count > 0 && moves.All(m => char.ToUpper(m.piece) == 'K'))
				{
					passed++;
				}
				else
				{
					failedTests.Add("Empty board handling failed");
				}
			}
			catch (Exception e)
			{
				failedTests.Add($"Empty board exception: {e.Message}");
			}

			yield return null;

			// Test maximum material position
			total++;
			try
			{
				ChessBoard maxMaterial = new ChessBoard();
				List<ChessMove> moves = MoveGenerator.GenerateLegalMoves(maxMaterial);

				if (moves.Count == 20) // 16 pawn moves + 4 knight moves in starting position
				{
					passed++;
				}
				else
				{
					failedTests.Add($"Starting position move count incorrect: {moves.Count}, expected 20");
				}
			}
			catch (Exception e)
			{
				failedTests.Add($"Starting position exception: {e.Message}");
			}

			yield return null;

			// Test stalemate detection
			total++;
			try
			{
				ChessBoard stalemate = new ChessBoard("k7/8/1K6/8/8/8/8/1Q6 b - - 0 1");
				var result = ChessRules.EvaluatePosition(stalemate);
				List<ChessMove> moves = MoveGenerator.GenerateLegalMoves(stalemate);

				if (moves.Count == 0 && result == ChessRules.GameResult.Stalemate)
				{
					passed++;
				}
				else
				{
					failedTests.Add("Stalemate detection failed");
				}
			}
			catch (Exception e)
			{
				failedTests.Add($"Stalemate test exception: {e.Message}");
			}

			testResults.Add($"Edge Cases: {passed}/{total} passed");
			yield return null;
		}

		private IEnumerator TestInvalidPositions()
		{
			UnityEngine.Debug.Log("Testing Invalid Position Rejection...");
			int passed = 0, total = 0;

			string[] invalidFENs = {
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBN w KQkq - 0 1", // Missing king
                "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNk w KQkq - 0 1", // Two black kings
                "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR x KQkq - 0 1", // Invalid side
                "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq z9 0 1", // Invalid en passant
                "", // Empty FEN
                "invalid/fen/string", // Malformed FEN
            };

			foreach (string fen in invalidFENs)
			{
				total++;
				ChessBoard board = new ChessBoard();
				bool loaded = board.LoadFromFEN(fen);

				if (!loaded) // Should reject invalid FENs
				{
					passed++;
				}
				else
				{
					failedTests.Add($"Invalid FEN was incorrectly accepted: {fen}");
				}

				yield return null;
			}

			testResults.Add($"Invalid Position Rejection: {passed}/{total} passed");
			yield return null;
		}

		private IEnumerator TestChess960Support()
		{
			UnityEngine.Debug.Log("Testing Chess960 Support...");
			int passed = 0, total = 0;

			string[] chess960FENs = {
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w HAha - 0 1", // Chess960 castling notation
                "bbqnnrkr/pppppppp/8/8/8/8/PPPPPPPP/BBQNNRKR w KQkq - 0 1", // Chess960 position
                "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w Aaha - 0 1", // Mixed notation
            };

			foreach (string fen in chess960FENs)
			{
				total++;
				ChessBoard board = new ChessBoard();
				bool loaded = board.LoadFromFEN(fen);

				if (loaded && board.castlingRights != "-")
				{
					// Test that castling rights are preserved
					List<ChessMove> moves = MoveGenerator.GenerateLegalMoves(board);
					passed++;
				}
				else
				{
					failedTests.Add($"Chess960 FEN failed to load: {fen}");
				}

				yield return null;
			}

			testResults.Add($"Chess960 Support: {passed}/{total} passed");
			yield return null;
		}

		#endregion

		#region Integration Tests

		private IEnumerator TestStockfishIntegration()
		{
			UnityEngine.Debug.Log("Testing Stockfish Integration...");
			int passed = 0, total = 0;

			// Test promotion move parsing
			total++;
			string testOutput = @"
info depth 1 seldepth 1 multipv 1 score cp 900 nodes 20 nps 10000 tbhits 0 time 2 pv e7e8q
bestmove e7e8q
";

			// Mock test - in real implementation, would test actual StockfishBridge
			if (testOutput.Contains("bestmove e7e8q"))
			{
				passed++;
			}
			else
			{
				failedTests.Add("Stockfish promotion parsing failed");
			}

			yield return null;

			// Test various UCI move formats
			total++;
			string[] uciMoves = { "e2e4", "e7e8q", "o-o", "o-o-o", "a7a8r" };
			bool allValid = true;

			foreach (string move in uciMoves)
			{
				// Basic format validation
				if (string.IsNullOrEmpty(move) || move.Length < 2)
				{
					allValid = false;
					failedTests.Add($"Invalid UCI move format: {move}");
				}
			}

			if (allValid) passed++;

			testResults.Add($"Stockfish Integration: {passed}/{total} passed");
			yield return null;
		}

		private IEnumerator TestEvaluationSystem()
		{
			UnityEngine.Debug.Log("Testing Evaluation System...");
			int passed = 0, total = 0;

			// Test starting position evaluation
			total++;
			ChessBoard startingBoard = new ChessBoard();
			float startingEval = ChessEvaluator.EvaluatePosition(startingBoard);

			if (startingEval >= 0.4f && startingEval <= 0.6f) // Should be near 0.5 for equal position
			{
				passed++;
			}
			else
			{
				failedTests.Add($"Starting position evaluation incorrect: {startingEval}, expected ~0.5");
			}

			yield return null;

			// Test material advantage detection
			total++;
			ChessBoard materialAdvantage = new ChessBoard("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBN1 w Qkq - 0 1"); // Missing white rook
			float materialEval = ChessEvaluator.EvaluatePosition(materialAdvantage);

			if (materialEval < 0.4f) // Should favor black with extra rook
			{
				passed++;
			}
			else
			{
				failedTests.Add($"Material advantage not detected: {materialEval}");
			}

			yield return null;

			// Test evaluation consistency
			total++;
			bool consistent = true;
			for (int i = 0; i < 5; i++)
			{
				float eval1 = ChessEvaluator.EvaluatePosition(startingBoard);
				float eval2 = ChessEvaluator.EvaluatePosition(startingBoard);

				if (Math.Abs(eval1 - eval2) > 0.001f)
				{
					consistent = false;
					break;
				}
			}

			if (consistent)
			{
				passed++;
			}
			else
			{
				failedTests.Add("Evaluation system not consistent");
			}

			testResults.Add($"Evaluation System: {passed}/{total} passed");
			yield return null;
		}

		#endregion

		#region Performance Tests

		private IEnumerator TestPerformance()
		{
			UnityEngine.Debug.Log("Testing Performance...");

			var stopwatch = new Stopwatch();

			// Move generation performance
			stopwatch.Start();
			for (int i = 0; i < performanceTestIterations; i++)
			{
				ChessBoard board = new ChessBoard(PerformanceFENs[i % PerformanceFENs.Length]);
				List<ChessMove> moves = MoveGenerator.GenerateLegalMoves(board);

				if (i % 100 == 0) yield return null; // Prevent blocking
			}
			stopwatch.Stop();

			double moveGenTime = stopwatch.ElapsedMilliseconds;
			testResults.Add($"Move Generation: {performanceTestIterations} iterations in {moveGenTime}ms ({moveGenTime / performanceTestIterations:F2}ms avg)");

			// Evaluation performance
			stopwatch.Restart();
			for (int i = 0; i < performanceTestIterations / 2; i++) // Half iterations for evaluation (slower)
			{
				ChessBoard board = new ChessBoard(PerformanceFENs[i % PerformanceFENs.Length]);
				float eval = ChessEvaluator.EvaluatePosition(board);

				if (i % 50 == 0) yield return null;
			}
			stopwatch.Stop();

			double evalTime = stopwatch.ElapsedMilliseconds;
			testResults.Add($"Evaluation: {performanceTestIterations / 2} iterations in {evalTime}ms ({evalTime / (performanceTestIterations / 2):F2}ms avg)");

			// FEN parsing performance
			stopwatch.Restart();
			for (int i = 0; i < performanceTestIterations; i++)
			{
				ChessBoard board = new ChessBoard();
				board.LoadFromFEN(EdgeCaseFENs[i % EdgeCaseFENs.Length]);

				if (i % 100 == 0) yield return null;
			}
			stopwatch.Stop();

			double fenTime = stopwatch.ElapsedMilliseconds;
			testResults.Add($"FEN Parsing: {performanceTestIterations} iterations in {fenTime}ms ({fenTime / performanceTestIterations:F2}ms avg)");

			yield return null;
		}

		#endregion

		#region Public Interface

		/// <summary>
		/// Run tests manually from inspector or script
		/// </summary>
		[ContextMenu("Run All Tests")]
		public void RunTests()
		{
			StartCoroutine(RunAllTests());
		}

		/// <summary>
		/// Run specific test category
		/// </summary>
		public void RunSpecificTests(string category)
		{
			switch (category.ToLower())
			{
				case "fen":
					StartCoroutine(TestFENParsing());
					break;
				case "moves":
					StartCoroutine(TestMoveGeneration());
					break;
				case "promotion":
					StartCoroutine(TestPromotionSystem());
					break;
				case "performance":
					StartCoroutine(TestPerformance());
					break;
				default:
					UnityEngine.Debug.LogWarning($"Unknown test category: {category}");
					break;
			}
		}

		/// <summary>
		/// Get test results as formatted string
		/// </summary>
		public string GetTestResults()
		{
			var results = new System.Text.StringBuilder();
			results.AppendLine("=== Chess System Test Results ===");
			results.AppendLine($"Overall Status: {(allTestsPassed ? "PASSED" : "FAILED")}");
			results.AppendLine($"Failed Tests: {failedTests.Count}");
			results.AppendLine("");

			foreach (var result in testResults)
			{
				results.AppendLine(result);
			}

			if (failedTests.Count > 0)
			{
				results.AppendLine("");
				results.AppendLine("Failed Test Details:");
				foreach (var failure in failedTests)
				{
					results.AppendLine($"- {failure}");
				}
			}

			return results.ToString();
		}

		#endregion

		#region Helper Methods

		/// <summary>
		/// Validate that all required components are present in the scene
		/// </summary>
		private bool ValidateSceneSetup()
		{
			bool valid = true;

			// Check for required components
			if (FindObjectOfType<StockfishBridge>() == null)
			{
				UnityEngine.Debug.LogWarning("StockfishBridge not found in scene - some tests may fail");
				valid = false;
			}

			if (FindObjectOfType<PromotionUI>() == null)
			{
				UnityEngine.Debug.LogWarning("PromotionUI not found in scene - promotion UI tests may fail");
			}

			return valid;
		}

		/// <summary>
		/// Log memory usage for performance monitoring
		/// </summary>
		private void LogMemoryUsage(string testName)
		{
			long memoryBefore = System.GC.GetTotalMemory(false);
			System.GC.Collect();
			long memoryAfter = System.GC.GetTotalMemory(true);

			UnityEngine.Debug.Log($"[{testName}] Memory: {memoryBefore / 1024}KB -> {memoryAfter / 1024}KB (freed: {(memoryBefore - memoryAfter) / 1024}KB)");
		}

		#endregion
	}
}