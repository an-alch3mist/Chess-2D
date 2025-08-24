using System.Collections;
using UnityEngine;

namespace GPTDeepResearch
{
	/// <summary>
	/// Test script for StockfishBridge edge cases and validation
	/// Handles various scenarios: invalid FEN, stalemate, checkmate, evaluation testing
	/// </summary>
	public class StockfishBridgeCheck_1 : MonoBehaviour
	{
		[Header("StockfishBridge Reference")]
		[SerializeField] private StockfishBridge stockfishBridge;

		[Header("Test Configuration")]
		[SerializeField] private bool runTestsOnStart = true;
		[SerializeField] private bool enableDetailedLogging = true;

		private void Start()
		{
			if (runTestsOnStart)
			{
				StartCoroutine(RunAllTestsCoroutine());
			}
		}

		[ContextMenu("Run All Tests")]
		public void RunAllTests()
		{
			StartCoroutine(RunAllTestsCoroutine());
		}

		private IEnumerator RunAllTestsCoroutine()
		{
			if (stockfishBridge == null)
			{
				Debug.LogError("[StockfishCheck] No StockfishBridge reference assigned!");
				yield break;
			}

			Debug.Log("<color=cyan>[StockfishCheck] Starting comprehensive test suite...</color>");
			yield return new WaitForSeconds(1f);

			// Test 1: Invalid FEN formats
			yield return StartCoroutine(TestInvalidFEN());
			yield return new WaitForSeconds(0.5f);

			// Test 2: King count validation
			yield return StartCoroutine(TestKingCountValidation());
			yield return new WaitForSeconds(0.5f);

			// Test 3: Stalemate position
			yield return StartCoroutine(TestStalematePosition());
			yield return new WaitForSeconds(0.5f);

			// Test 4: Checkmate position
			yield return StartCoroutine(TestCheckmatePosition());
			yield return new WaitForSeconds(0.5f);

			// Test 5: Normal position with evaluation
			yield return StartCoroutine(TestNormalPositionWithEvaluation());
			yield return new WaitForSeconds(0.5f);

			// Test 6: Side-to-move evaluation accuracy
			yield return StartCoroutine(TestSideToMoveEvaluation());
			yield return new WaitForSeconds(0.5f);

			// Test 7: Depth and skill level effects
			yield return StartCoroutine(TestDepthAndSkillEffects());
			yield return new WaitForSeconds(0.5f);

			// Test 8: Evaluation disabled vs enabled
			yield return StartCoroutine(TestEvaluationToggle());

			Debug.Log("<color=green>[StockfishCheck] All tests completed!</color>");
		}

		private IEnumerator TestInvalidFEN()
		{
			Debug.Log("<color=yellow>[Test 1] Invalid FEN Formats</color>");

			// Test cases with expected error messages
			string[] invalidFENs = {
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP",  // Missing side-to-move
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR x - - 0 1",  // Invalid side
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKB",  // Too few ranks
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR/8 w - - 0 1",  // Too many ranks
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNZ w - - 0 1",  // Invalid piece
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w - - 0 1",  // No kings (should pass basic but fail king count)
				"krnbqbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w - - 0 1",  // Two black kings
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQQBNR w - - 0 1"   // No white king
			};

			for (int i = 0; i < invalidFENs.Length; i++)
			{
				string fen = invalidFENs[i];
				LogTest($"Testing invalid FEN {i + 1}: {fen.Substring(0, Mathf.Min(30, fen.Length))}...");

				yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(fen));

				var result = stockfishBridge.LastAnalysisResult;
				bool hasError = result.bestMove.StartsWith("ERROR:");

				if (hasError)
				{
					LogSuccess($"✓ Correctly rejected invalid FEN: {result.errorMessage}");
				}
				else
				{
					LogError($"✗ Should have rejected invalid FEN but got: {result.bestMove}");
				}
			}
		}

		private IEnumerator TestKingCountValidation()
		{
			Debug.Log("<color=yellow>[Test 2] King Count Validation</color>");

			// Zero kings
			yield return TestFENValidation(
				"rnbq1bnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQ1BNR w - - 0 1",
				"No kings"
			);

			// Multiple white kings
			yield return TestFENValidation(
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/KNBQKBNR w - - 0 1",
				"Multiple white kings"
			);

			// Multiple black kings
			yield return TestFENValidation(
				"knbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w - - 0 1",
				"Multiple black kings"
			);
		}

		private IEnumerator TestFENValidation(string fen, string description)
		{
			LogTest($"Testing {description}: {fen}");
			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(fen));

			var result = stockfishBridge.LastAnalysisResult;
			bool hasError = result.bestMove.StartsWith("ERROR:");

			if (hasError)
			{
				LogSuccess($"✓ Correctly rejected {description}: {result.errorMessage}");
			}
			else
			{
				LogError($"✗ Should have rejected {description} but got: {result.bestMove}");
			}
		}

		private IEnumerator TestStalematePosition()
		{
			Debug.Log("<color=yellow>[Test 3] Stalemate Position</color>");

			// Classic stalemate position: King vs King + Pawn
			string stalemateFEN = "8/8/8/8/8/5k2/5p2/5K2 w - - 0 1";
			LogTest($"Testing stalemate FEN: {stalemateFEN}");

			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(stalemateFEN, 2000, 5, 5));

			var result = stockfishBridge.LastAnalysisResult;
			LogTest($"Result: {result.bestMove}, IsGameEnd: {result.isGameEnd}");

			if (result.bestMove == "stale-mate" && result.isGameEnd)
			{
				LogSuccess("✓ Correctly identified stalemate");
				LogTest($"Evaluation: W={result.evaluation:F3}, STM={result.stmEvaluation:F3} (should be ~0.5 for draw)");
			}
			else
			{
				LogError($"✗ Failed to identify stalemate. Got: {result.bestMove}, IsGameEnd: {result.isGameEnd}");
			}
		}

		private IEnumerator TestCheckmatePosition()
		{
			Debug.Log("<color=yellow>[Test 4] Checkmate Position</color>");

			// Scholar's mate position - White checkmates Black
			string checkmateFEN = "r1bqkb1r/pppp1ppp/2n2n2/4p2Q/2B1P3/8/PPPP1PPP/RNB1K1NR w KQkq - 4 4";
			LogTest($"Testing checkmate FEN: {checkmateFEN}");

			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(checkmateFEN, 2000, 5, 5));

			var result = stockfishBridge.LastAnalysisResult;
			LogTest($"Result: {result.bestMove}, IsGameEnd: {result.isGameEnd}");

			if (result.bestMove == "check-mate" && result.isGameEnd)
			{
				LogSuccess("✓ Correctly identified checkmate");
				LogTest($"Evaluation: W={result.evaluation:F3}, STM={result.stmEvaluation:F3}");
				LogTest($"Side to move: {result.Side} (White should be very close to 1.0, Black close to 0.0)");
			}
			else if (!result.bestMove.StartsWith("ERROR:"))
			{
				// Might be a forcing move toward checkmate
				LogWarning($"⚠ Got move instead of checkmate: {result.bestMove}. This might be a multi-move mate.");
				LogTest($"Evaluation: W={result.evaluation:F3}, STM={result.stmEvaluation:F3}");
			}
			else
			{
				LogError($"✗ Error analyzing checkmate position: {result.errorMessage}");
			}
		}

		private IEnumerator TestNormalPositionWithEvaluation()
		{
			Debug.Log("<color=yellow>[Test 5] Normal Position with Evaluation</color>");

			// Italian Game opening - should favor White slightly
			string normalFEN = "r1bqkbnr/pppp1ppp/2n5/4p3/2B1P3/5N2/PPPP1PPP/RNBQK2R b KQkq - 3 3";
			LogTest($"Testing normal FEN: {normalFEN}");

			// Enable evaluation in the bridge for this test
			var originalEvalSetting = stockfishBridge.enableEvaluation;
			stockfishBridge.enableEvaluation = true;

			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(normalFEN, 3000, 3, 5));

			var result = stockfishBridge.LastAnalysisResult;

			if (!result.bestMove.StartsWith("ERROR:") && !result.isGameEnd)
			{
				LogSuccess($"✓ Got valid move: {result.bestMove}");
				LogTest($"Side to move: {result.Side} (Black)");
				LogTest($"White evaluation: {result.evaluation:F3} ({result.evaluation:P1})");
				LogTest($"STM evaluation: {result.stmEvaluation:F3} ({result.stmEvaluation:P1})");
				LogTest($"Search depth: {result.searchDepth}, Eval depth: {result.evaluationDepth}");
				LogTest($"Skill level: {result.skillLevel}, Approx Elo: {result.approximateElo}");

				// Validation checks
				if (result.evaluation != 0.5f || result.stmEvaluation != 0.5f)
				{
					LogSuccess("✓ Evaluation computed (not default 0.5)");
				}
				else
				{
					LogWarning("⚠ Evaluation appears to be default values");
				}

				// Side-to-move logic check
				float expectedSTM = result.Side == 'b' ? 1f - result.evaluation : result.evaluation;
				if (Mathf.Abs(result.stmEvaluation - expectedSTM) < 0.001f)
				{
					LogSuccess("✓ Side-to-move evaluation correctly calculated");
				}
				else
				{
					LogError($"✗ STM eval mismatch. Expected: {expectedSTM:F3}, Got: {result.stmEvaluation:F3}");
				}
			}
			else
			{
				LogError($"✗ Failed to analyze normal position: {result.bestMove}");
			}

			// Restore original setting
			stockfishBridge.enableEvaluation = originalEvalSetting;
		}

		private IEnumerator TestSideToMoveEvaluation()
		{
			Debug.Log("<color=yellow>[Test 6] Side-to-Move Evaluation</color>");

			// Test same position from both sides
			string whiteFEN = "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1"; // Black to move
			string blackFEN = "rnbqkbnr/pppp1ppp/8/4p3/4P3/8/PPPP1PPP/RNBQKBNR w KQkq e6 0 2"; // White to move

			var originalEvalSetting = stockfishBridge.enableEvaluation;
			stockfishBridge.enableEvaluation = true;

			LogTest("Testing Black to move position...");
			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(whiteFEN, 2000, 3, 5));
			var blackToMoveResult = stockfishBridge.LastAnalysisResult;

			LogTest("Testing White to move position...");
			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(blackFEN, 2000, 3, 5));
			var whiteToMoveResult = stockfishBridge.LastAnalysisResult;

			LogTest($"Black to move - W: {blackToMoveResult.evaluation:F3}, STM: {blackToMoveResult.stmEvaluation:F3}");
			LogTest($"White to move - W: {whiteToMoveResult.evaluation:F3}, STM: {whiteToMoveResult.stmEvaluation:F3}");

			// Validation: STM should be adjusted correctly
			if (blackToMoveResult.Side == 'b' && whiteToMoveResult.Side == 'w')
			{
				LogSuccess("✓ Sides correctly identified");

				// Check STM calculation
				float blackSTMExpected = 1f - blackToMoveResult.evaluation;
				float whiteSTMExpected = whiteToMoveResult.evaluation;

				bool blackSTMCorrect = Mathf.Abs(blackToMoveResult.stmEvaluation - blackSTMExpected) < 0.01f;
				bool whiteSTMCorrect = Mathf.Abs(whiteToMoveResult.stmEvaluation - whiteSTMExpected) < 0.01f;

				if (blackSTMCorrect && whiteSTMCorrect)
				{
					LogSuccess("✓ Side-to-move evaluations correctly calculated");
				}
				else
				{
					LogError("✗ Side-to-move evaluation calculation errors");
				}
			}

			stockfishBridge.enableEvaluation = originalEvalSetting;
		}

		private IEnumerator TestDepthAndSkillEffects()
		{
			Debug.Log("<color=yellow>[Test 7] Depth and Skill Level Effects</color>");

			string testFEN = "startpos";

			// Test different depths
			LogTest("Testing depth 1 vs depth 5...");
			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(testFEN, -1, 1, -1, -1, 0));
			var depth1Result = stockfishBridge.LastAnalysisResult;

			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(testFEN, -1, 5, -1, -1, 0));
			var depth5Result = stockfishBridge.LastAnalysisResult;

			LogTest($"Depth 1: Move={depth1Result.bestMove}, Elo={depth1Result.approximateElo}");
			LogTest($"Depth 5: Move={depth5Result.bestMove}, Elo={depth5Result.approximateElo}");

			if (depth5Result.approximateElo > depth1Result.approximateElo)
			{
				LogSuccess("✓ Higher depth results in higher Elo estimate");
			}
			else
			{
				LogWarning("⚠ Depth effect on Elo might not be working as expected");
			}

			// Test skill levels
			LogTest("Testing skill level 0 vs skill level 10...");
			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(testFEN, -1, 3, -1, -1, 0));
			var skill0Result = stockfishBridge.LastAnalysisResult;

			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(testFEN, -1, 3, -1, -1, 10));
			var skill10Result = stockfishBridge.LastAnalysisResult;

			LogTest($"Skill 0: Move={skill0Result.bestMove}, Elo={skill0Result.approximateElo}");
			LogTest($"Skill 10: Move={skill10Result.bestMove}, Elo={skill10Result.approximateElo}");

			if (skill10Result.approximateElo > skill0Result.approximateElo)
			{
				LogSuccess("✓ Higher skill level results in higher Elo estimate");
			}
			else
			{
				LogWarning("⚠ Skill level effect on Elo might not be working as expected");
			}
		}

		private IEnumerator TestEvaluationToggle()
		{
			Debug.Log("<color=yellow>[Test 8] Evaluation Toggle Test</color>");

			string testFEN = "startpos";

			// Test with evaluation disabled
			stockfishBridge.enableEvaluation = false;
			LogTest("Testing with evaluation DISABLED...");
			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(testFEN, 2000, 3, 5));
			var noEvalResult = stockfishBridge.LastAnalysisResult;

			// Test with evaluation enabled
			stockfishBridge.enableEvaluation = true;
			LogTest("Testing with evaluation ENABLED...");
			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(testFEN, 2000, 3, 5));
			var withEvalResult = stockfishBridge.LastAnalysisResult;

			LogTest($"No eval: W={noEvalResult.evaluation:F3}, STM={noEvalResult.stmEvaluation:F3}");
			LogTest($"With eval: W={withEvalResult.evaluation:F3}, STM={withEvalResult.stmEvaluation:F3}");

			// Validation
			if (noEvalResult.evaluation == 0.5f && noEvalResult.stmEvaluation == 0.5f)
			{
				LogSuccess("✓ Evaluation correctly defaults to 0.5 when disabled");
			}
			else
			{
				LogError($"✗ Expected 0.5 when disabled, got W={noEvalResult.evaluation:F3}, STM={noEvalResult.stmEvaluation:F3}");
			}

			if (withEvalResult.evaluation != 0.5f || withEvalResult.stmEvaluation != 0.5f)
			{
				LogSuccess("✓ Evaluation computed when enabled");
			}
			else
			{
				LogWarning("⚠ Evaluation still 0.5 even when enabled - might indicate issue");
			}

			// Restore to inspector default
			stockfishBridge.enableEvaluation = false; // Default from inspector
		}

		#region Helper Methods

		private void LogTest(string message)
		{
			if (enableDetailedLogging)
				Debug.Log($"[StockfishCheck] {message}");
		}

		private void LogSuccess(string message)
		{
			Debug.Log($"<color=green>[StockfishCheck] {message}</color>");
		}

		private void LogWarning(string message)
		{
			Debug.LogWarning($"<color=orange>[StockfishCheck] {message}</color>");
		}

		private void LogError(string message)
		{
			Debug.LogError($"<color=red>[StockfishCheck] {message}</color>");
		}

		#endregion

		#region Manual Test Methods

		[ContextMenu("Test Invalid FEN Only")]
		public void TestInvalidFENOnly()
		{
			StartCoroutine(TestInvalidFEN());
		}

		[ContextMenu("Test Stalemate Only")]
		public void TestStalemateOnly()
		{
			StartCoroutine(TestStalematePosition());
		}

		[ContextMenu("Test Checkmate Only")]
		public void TestCheckmateOnly()
		{
			StartCoroutine(TestCheckmatePosition());
		}

		[ContextMenu("Test Evaluation Only")]
		public void TestEvaluationOnly()
		{
			StartCoroutine(TestNormalPositionWithEvaluation());
		}

		#endregion
	}
}