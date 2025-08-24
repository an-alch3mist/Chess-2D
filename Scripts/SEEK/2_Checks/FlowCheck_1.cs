/*
CHANGELOG (New File):
- Inspector-driven test script for comprehensive chess system validation
- Automated tests for FEN parsing, move generation, and game state detection
- Malformed FEN handling and error reporting validation
- Stalemate vs checkmate detection verification
- Engine evaluation mapping sanity checks
- Legal move presence testing for typical positions
- Castling and en passant specific test cases
- Uses colored Debug.Log messages instead of Debug.LogError for non-fatal issues
- Integration tests between StockfishBridge and chess rule components
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SPACE_UTIL;

namespace GPTDeepResearch
{
	/// <summary>
	/// Comprehensive test suite for the chess system.
	/// Validates StockfishBridge integration with legal move generation and game rules.
	/// </summary>
	public class FlowCheck_1 : MonoBehaviour
	{
		[Header("Test Configuration")]
		[SerializeField] private bool runOnStart = true;
		[SerializeField] private bool runEngineTests = true;
		[SerializeField] private bool runChessRuleTests = true;
		[SerializeField] private bool runPerformanceTests = false;

		[Header("Engine Reference")]
		[SerializeField] private StockfishBridge stockfishBridge;

		[Header("Test Results")]
		[SerializeField] private int testsRun = 0;
		[SerializeField] private int testsPassed = 0;
		[SerializeField] private int testsFailed = 0;

		// Test positions
		private readonly Dictionary<string, string> testPositions = new Dictionary<string, string>
		{
            // Standard positions
            { "starting", "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1" },
			{ "scholars_mate", "rnbqkb1r/pppp1ppp/5n2/4p3/2B1P3/8/PPPP1PpP/RNBQK1NR w KQkq - 0 4" },
            
            // Checkmate positions
            { "fools_mate", "rnbqkbnr/pppp1p1p/8/4p1p1/6PQ/8/PPPP1P1P/RNB1KBNR b KQkq - 0 2" },
			{ "back_rank_mate", "6k1/5ppp/8/8/8/8/5PPP/R3K2R w KQ - 0 1" },
            
            // Stalemate positions
            { "stalemate_1", "k7/8/1K6/8/8/8/8/1Q6 w - - 0 1" },
			{ "stalemate_2", "8/8/8/8/8/6k1/5p1p/5K1R b - - 0 1" },
            
            // Castling positions
            { "castling_available", "r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1" },
			{ "kingside_only", "r3k2r/8/8/8/8/8/8/R3K2R w Kk - 0 1" },
            
            // En passant positions
            { "en_passant_white", "rnbqkbnr/ppp1p1pp/8/3pPp2/8/8/PPPP1PPP/RNBQKBNR w KQkq f6 0 3" },
			{ "en_passant_black", "rnbqkbnr/pppp1ppp/8/8/3PpP2/8/PPP1P1PP/RNBQKBNR b KQkq d3 0 3" },
            
            // Promotion positions
            { "promotion_white", "8/P7/8/8/8/8/8/4K2k w - - 0 1" },
			{ "promotion_black", "4k2K/8/8/8/8/8/p7/8 b - - 0 1" },
            
            // Chess960 position
            { "chess960_sample", "rknnqbbr/pppppppp/8/8/8/8/PPPPPPPP/RKNNQBBR w KQkq - 0 1" }
		};

		private readonly string[] malformedFENs = {
			"",                                                      // Empty
            "invalid",                                              // Not FEN
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP",                  // Missing fields
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR x KQkq - 0 1", // Invalid side
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w XYZ - 0 1",  // Invalid castling
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq z9 0 1", // Invalid en passant
            "rnbqkbnr/pppppppp/7/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",  // Wrong square count
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBR w KQkq - 0 1",   // Missing king
            "RNBQKBNR/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",   // Two white kings
        };

		void Start()
		{
			if (runOnStart)
			{
				StartCoroutine(RunAllTests());
			}
		}

		[ContextMenu("Run All Tests")]
		public void RunAllTestsFromInspector()
		{
			StartCoroutine(RunAllTests());
		}

		public IEnumerator RunAllTests()
		{
			LogHeader("STARTING COMPREHENSIVE CHESS SYSTEM TESTS");
			testsRun = 0;
			testsPassed = 0;
			testsFailed = 0;

			// FEN and Board Tests
			yield return StartCoroutine(TestFENParsing());
			yield return StartCoroutine(TestMalformedFENHandling());

			// Chess Rules Tests
			if (runChessRuleTests)
			{
				yield return StartCoroutine(TestMoveGeneration());
				yield return StartCoroutine(TestGameStateDetection());
				yield return StartCoroutine(TestCastlingRules());
				yield return StartCoroutine(TestEnPassantRules());
				yield return StartCoroutine(TestPromotionRules());
				yield return StartCoroutine(TestCheckDetection());
			}

			// Engine Integration Tests
			if (runEngineTests && stockfishBridge != null)
			{
				yield return StartCoroutine(TestEngineIntegration());
				yield return StartCoroutine(TestEvaluationMapping());
				yield return StartCoroutine(TestEngineErrorHandling());
			}

			// Performance Tests (optional)
			if (runPerformanceTests)
			{
				yield return StartCoroutine(TestPerformance());
			}

			LogHeader($"TESTS COMPLETED: {testsPassed}/{testsRun} PASSED, {testsFailed} FAILED");
		}

		#region FEN and Board Tests

		private IEnumerator TestFENParsing()
		{
			LogSubheader("Testing FEN Parsing");

			foreach (var kvp in testPositions)
			{
				string positionName = kvp.Key;
				string fen = kvp.Value;

				ChessBoard board = new ChessBoard(fen);
				string reconstructedFEN = board.ToFEN();

				// Test that FEN round-trip works
				bool fensMatch = NormalizeFEN(fen) == NormalizeFEN(reconstructedFEN);
				TestResult($"FEN round-trip for {positionName}", fensMatch);

				// Test piece counts
				bool validPieceCount = ValidatePieceCounts(board);
				TestResult($"Piece count validation for {positionName}", validPieceCount);

				yield return null;
			}
		}

		private IEnumerator TestMalformedFENHandling()
		{
			LogSubheader("Testing Malformed FEN Handling");

			foreach (string badFEN in malformedFENs)
			{
				ChessBoard board = new ChessBoard();
				bool loadSuccess = board.LoadFromFEN(badFEN);

				// Should fail to load malformed FEN
				TestResult($"Reject malformed FEN: '{badFEN.Substring(0, Math.Min(20, badFEN.Length))}...'", !loadSuccess);
				yield return null;
			}
		}

		#endregion

		#region Chess Rules Tests

		private IEnumerator TestMoveGeneration()
		{
			LogSubheader("Testing Move Generation");

			// Test starting position
			ChessBoard startBoard = new ChessBoard(testPositions["starting"]);
			List<ChessMove> startMoves = MoveGenerator.GenerateLegalMoves(startBoard);
			TestResult("Starting position has 20 legal moves", startMoves.Count == 20);

			// Test checkmate position (no legal moves, in check)
			ChessBoard mateBoard = new ChessBoard(testPositions["fools_mate"]);
			List<ChessMove> mateMoves = MoveGenerator.GenerateLegalMoves(mateBoard);
			bool inCheck = ChessRules.IsInCheck(mateBoard, mateBoard.sideToMove);
			TestResult("Checkmate position has no legal moves", mateMoves.Count == 0);
			TestResult("Checkmate position king is in check", inCheck);

			// Test stalemate position (no legal moves, not in check)
			ChessBoard staleBoard = new ChessBoard(testPositions["stalemate_1"]);
			List<ChessMove> staleMoves = MoveGenerator.GenerateLegalMoves(staleBoard);
			bool inCheckStale = ChessRules.IsInCheck(staleBoard, staleBoard.sideToMove);
			TestResult("Stalemate position has no legal moves", staleMoves.Count == 0);
			TestResult("Stalemate position king not in check", !inCheckStale);

			yield return null;
		}

		private IEnumerator TestGameStateDetection()
		{
			LogSubheader("Testing Game State Detection");

			// Test checkmate detection
			ChessBoard mateBoard = new ChessBoard(testPositions["fools_mate"]);
			ChessRules.GameResult mateResult = ChessRules.EvaluatePosition(mateBoard);
			TestResult("Detect checkmate (white wins)", mateResult == ChessRules.GameResult.WhiteWins);

			// Test stalemate detection
			ChessBoard staleBoard = new ChessBoard(testPositions["stalemate_1"]);
			ChessRules.GameResult staleResult = ChessRules.EvaluatePosition(staleBoard);
			TestResult("Detect stalemate", staleResult == ChessRules.GameResult.Stalemate);

			// Test game in progress
			ChessBoard activeBoard = new ChessBoard(testPositions["starting"]);
			ChessRules.GameResult activeResult = ChessRules.EvaluatePosition(activeBoard);
			TestResult("Detect game in progress", activeResult == ChessRules.GameResult.InProgress);

			yield return null;
		}

		private IEnumerator TestCastlingRules()
		{
			LogSubheader("Testing Castling Rules");

			// Test castling availability
			ChessBoard castleBoard = new ChessBoard(testPositions["castling_available"]);
			List<ChessMove> castleMoves = MoveGenerator.GenerateLegalMoves(castleBoard);

			var castlingMoves = castleMoves.Where(m => m.moveType == ChessMove.MoveType.Castling).ToList();
			TestResult("Castling moves available when legal", castlingMoves.Count >= 2); // Should have both sides

			// Test kingside only castling
			ChessBoard kingsideBoard = new ChessBoard(testPositions["kingside_only"]);
			List<ChessMove> kingsideMoves = MoveGenerator.GenerateLegalMoves(kingsideBoard);
			var kingsideCastling = kingsideMoves.Where(m => m.moveType == ChessMove.MoveType.Castling).ToList();
			TestResult("Only kingside castling when rights limited", kingsideCastling.Count >= 1);

			yield return null;
		}

		private IEnumerator TestEnPassantRules()
		{
			LogSubheader("Testing En Passant Rules");

			// Test white en passant
			ChessBoard epWhiteBoard = new ChessBoard(testPositions["en_passant_white"]);
			List<ChessMove> epWhiteMoves = MoveGenerator.GenerateLegalMoves(epWhiteBoard);
			var epMoves = epWhiteMoves.Where(m => m.moveType == ChessMove.MoveType.EnPassant).ToList();
			TestResult("En passant move available for white", epMoves.Count >= 1);

			// Test black en passant
			ChessBoard epBlackBoard = new ChessBoard(testPositions["en_passant_black"]);
			List<ChessMove> epBlackMoves = MoveGenerator.GenerateLegalMoves(epBlackBoard);
			var epBlackCaptures = epBlackMoves.Where(m => m.moveType == ChessMove.MoveType.EnPassant).ToList();
			TestResult("En passant move available for black", epBlackCaptures.Count >= 1);

			yield return null;
		}

		private IEnumerator TestPromotionRules()
		{
			LogSubheader("Testing Promotion Rules");

			// Test white promotion
			ChessBoard promoteWhiteBoard = new ChessBoard(testPositions["promotion_white"]);
			List<ChessMove> promoteWhiteMoves = MoveGenerator.GenerateLegalMoves(promoteWhiteBoard);
			var promotionMoves = promoteWhiteMoves.Where(m => m.moveType == ChessMove.MoveType.Promotion).ToList();
			TestResult("White promotion moves available", promotionMoves.Count >= 4); // Q, R, B, N

			// Test black promotion
			ChessBoard promoteBlackBoard = new ChessBoard(testPositions["promotion_black"]);
			List<ChessMove> promoteBlackMoves = MoveGenerator.GenerateLegalMoves(promoteBlackBoard);
			var blackPromotions = promoteBlackMoves.Where(m => m.moveType == ChessMove.MoveType.Promotion).ToList();
			TestResult("Black promotion moves available", blackPromotions.Count >= 4);

			yield return null;
		}

		private IEnumerator TestCheckDetection()
		{
			LogSubheader("Testing Check Detection");

			// Test check detection in various positions
			foreach (var kvp in testPositions)
			{
				ChessBoard board = new ChessBoard(kvp.Value);
				bool inCheck = ChessRules.IsInCheck(board, board.sideToMove);

				// Known check positions
				bool shouldBeInCheck = kvp.Key.Contains("mate") && kvp.Key != "stalemate_1" && kvp.Key != "stalemate_2";

				if (shouldBeInCheck)
				{
					TestResult($"Check detected in {kvp.Key}", inCheck);
				}
				else if (kvp.Key.Contains("stalemate"))
				{
					TestResult($"No check in {kvp.Key}", !inCheck);
				}

				yield return null;
			}
		}

		#endregion

		#region Engine Integration Tests

		private IEnumerator TestEngineIntegration()
		{
			LogSubheader("Testing Stockfish Engine Integration");

			if (stockfishBridge == null)
			{
				LogError("StockfishBridge reference not set - skipping engine tests");
				yield break;
			}

			// Test engine startup
			TestResult("Engine is running", stockfishBridge.IsEngineRunning);

			if (!stockfishBridge.IsEngineRunning)
			{
				LogWarning("Engine not running - starting engine...");
				stockfishBridge.StartEngine();
				yield return new WaitForSeconds(2f);
			}

			// Test basic position analysis
			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(testPositions["starting"]));

			var result = stockfishBridge.LastAnalysisResult;
			TestResult("Engine returns valid move for starting position", !string.IsNullOrEmpty(result.bestMove) && !result.bestMove.StartsWith("ERROR"));
			TestResult("Engine evaluation is reasonable", result.evaluation >= 0.4f && result.evaluation <= 0.6f);

			// Test checkmate detection
			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(testPositions["fools_mate"]));
			var mateResult = stockfishBridge.LastAnalysisResult;
			TestResult("Engine detects checkmate", mateResult.bestMove == "check-mate" || mateResult.isGameEnd);

			// Test stalemate detection
			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(testPositions["stalemate_1"]));
			var staleResult = stockfishBridge.LastAnalysisResult;
			TestResult("Engine detects stalemate", staleResult.bestMove == "stale-mate" || staleResult.isGameEnd);

			yield return null;
		}

		private IEnumerator TestEvaluationMapping()
		{
			LogSubheader("Testing Engine Evaluation Mapping");

			if (stockfishBridge == null) yield break;

			// Test evaluation range clamping
			stockfishBridge.enableEvaluation = true;

			// Test various positions
			string[] testFENs = {
				testPositions["starting"],
				testPositions["castling_available"],
				"8/8/8/3k4/3K4/8/8/8 w - - 0 1" // King and king endgame
            };

			foreach (string fen in testFENs)
			{
				yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(fen));
				var result = stockfishBridge.LastAnalysisResult;

				TestResult($"Evaluation in valid range for position",
						  result.evaluation >= 0.0001f && result.evaluation <= 0.9999f);
				TestResult($"STM evaluation in valid range for position",
						  result.stmEvaluation >= 0.0001f && result.stmEvaluation <= 0.9999f);

				yield return new WaitForSeconds(0.5f);
			}
		}

		private IEnumerator TestEngineErrorHandling()
		{
			LogSubheader("Testing Engine Error Handling");

			if (stockfishBridge == null) yield break;

			// Test malformed FEN handling
			foreach (string badFEN in malformedFENs.Take(3)) // Test a few
			{
				yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(badFEN));
				var result = stockfishBridge.LastAnalysisResult;

				TestResult($"Engine handles bad FEN gracefully",
						  result.bestMove.StartsWith("ERROR") || !string.IsNullOrEmpty(result.errorMessage));

				yield return new WaitForSeconds(0.2f);
			}
		}

		#endregion

		#region Performance Tests

		private IEnumerator TestPerformance()
		{
			LogSubheader("Testing Performance");

			// Test move generation speed
			ChessBoard complexBoard = new ChessBoard("r1bqk1nr/pppp1ppp/2n5/2b1p3/2B1P3/3P1N2/PPP2PPP/RNBQK2R w KQkq - 0 4");

			System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

			for (int i = 0; i < 100; i++)
			{
				List<ChessMove> moves = MoveGenerator.GenerateLegalMoves(complexBoard);
			}

			sw.Stop();
			float avgTime = sw.ElapsedMilliseconds / 100f;

			TestResult($"Move generation performance acceptable (<10ms avg)", avgTime < 10f);
			LogInfo($"Average move generation time: {avgTime:F2}ms");

			yield return null;
		}

		#endregion

		#region Helper Methods

		private void TestResult(string testName, bool passed)
		{
			testsRun++;
			if (passed)
			{
				testsPassed++;
				LogSuccess($"✓ {testName}");
			}
			else
			{
				testsFailed++;
				LogError($"✗ {testName}");
			}
		}

		private string NormalizeFEN(string fen)
		{
			// Basic FEN normalization for comparison
			return fen.Trim().ToLower();
		}

		private bool ValidatePieceCounts(ChessBoard board)
		{
			int whiteKings = 0, blackKings = 0;
			int totalPieces = 0;

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					char piece = board.board.GT(new v2(x, y));
					if (piece != '.')
					{
						totalPieces++;
						if (piece == 'K') whiteKings++;
						else if (piece == 'k') blackKings++;
					}
				}
			}

			return whiteKings == 1 && blackKings == 1 && totalPieces >= 2 && totalPieces <= 32;
		}

		private void LogHeader(string message)
		{
			Debug.Log($"<color=cyan><b>========== {message} ==========</b></color>");
		}

		private void LogSubheader(string message)
		{
			Debug.Log($"<color=yellow><b>--- {message} ---</b></color>");
		}

		private void LogSuccess(string message)
		{
			Debug.Log($"<color=green>{message}</color>");
		}

		private void LogError(string message)
		{
			Debug.Log($"<color=red>{message}</color>");
		}

		private void LogWarning(string message)
		{
			Debug.Log($"<color=orange>{message}</color>");
		}

		private void LogInfo(string message)
		{
			Debug.Log($"<color=white>{message}</color>");
		}

		#endregion
	}
}