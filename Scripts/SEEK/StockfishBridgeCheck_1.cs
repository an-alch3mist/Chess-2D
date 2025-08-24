using System.Collections;
using UnityEngine;

namespace GPTDeepResearch
{
	/// <summary>
	/// Testing script for StockfishBridge to validate various edge cases
	/// including FEN validation, mate detection, stalemate, and evaluation accuracy
	/// </summary>
	public class StockfishBridgeCheck_1 : MonoBehaviour
	{
		[Header("StockfishBridge Reference")]
		[SerializeField] private StockfishBridge stockfishBridge;

		[Header("Test Configuration")]
		[SerializeField] private bool runTestsOnStart = false;
		[SerializeField] private bool enableDetailedLogging = true;

		// Test FENs for various scenarios
		private readonly string[] testFens = {
            // Valid positions
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", // Starting position
            "startpos", // Alternative starting position
            "8/8/8/8/8/8/8/k6K w - - 0 1", // Simple endgame
            
            // Mate positions
            "rnbqkbnr/pppp1ppp/8/4p3/6P1/5P2/PPPPP2P/RNBQKBNR b KQkq - 0 2", // Back rank weakness
            "6k1/6p1/6K1/8/8/8/8/7R w - - 0 1", // Rook vs King endgame (mate threat)
            
            // Stalemate positions
            "k7/8/K7/8/8/8/8/8 b - - 0 1", // Stalemate for black
            
            // Invalid FENs
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", // Missing side to move
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR x KQkq - 0 1", // Invalid side to move
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBN w KQkq - 0 1", // Missing white king
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNRK w KQkq - 0 1", // Two white kings
            "rnbqkbn/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", // Missing rank data (7 ranks instead of 8)
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR9 w KQkq - 0 1", // Invalid character
        };

		private void Start()
		{
			if (runTestsOnStart)
			{
				StartCoroutine(RunAllTestsCoroutine());
			}
		}

		/// <summary>
		/// Public method to run all tests - can be called from Inspector or other scripts
		/// </summary>
		[ContextMenu("Run All Tests")]
		public void RunAllTests()
		{
			StartCoroutine(RunAllTestsCoroutine());
		}

		/// <summary>
		/// Run comprehensive test suite
		/// </summary>
		public IEnumerator RunAllTestsCoroutine()
		{
			Log("=== Starting StockfishBridge Test Suite ===");

			// Validate bridge reference
			if (!ValidateStockfishBridge())
			{
				LogError("StockfishBridge reference validation failed. Stopping tests.");
				yield break;
			}

			// Wait for engine initialization
			yield return StartCoroutine(WaitForEngineReady());

			if (!stockfishBridge.IsEngineRunning)
			{
				LogError("Engine failed to start. Cannot run tests.");
				yield break;
			}

			// Run test categories
			yield return StartCoroutine(TestFenValidation());
			yield return StartCoroutine(TestPositionAnalysis());
			yield return StartCoroutine(TestMateDetection());
			yield return StartCoroutine(TestStalemateDetection());
			yield return StartCoroutine(TestEvaluationAccuracy());
			yield return StartCoroutine(TestSideToMoveCalculation());

			Log("=== Test Suite Complete ===");
		}

		/// <summary>
		/// Validate StockfishBridge component reference and basic setup
		/// </summary>
		private bool ValidateStockfishBridge()
		{
			if (stockfishBridge == null)
			{
				LogError("StockfishBridge reference is null. Please assign it in the inspector.");
				return false;
			}

			Log("StockfishBridge reference validated successfully.");
			return true;
		}

		/// <summary>
		/// Wait for engine to be ready with timeout
		/// </summary>
		private IEnumerator WaitForEngineReady()
		{
			Log("Waiting for Stockfish engine to be ready...");

			float timeout = 15f;
			float elapsed = 0f;

			while (!stockfishBridge.IsEngineRunning && elapsed < timeout)
			{
				yield return new WaitForSeconds(0.5f);
				elapsed += 0.5f;
			}

			if (stockfishBridge.IsEngineRunning)
			{
				Log("Engine is ready for testing.");
			}
			else
			{
				LogError($"Engine failed to start within {timeout} seconds.");
			}
		}

		/// <summary>
		/// Test FEN validation with various invalid inputs
		/// </summary>
		private IEnumerator TestFenValidation()
		{
			Log("\n--- Testing FEN Validation ---");

			for (int i = 0; i < testFens.Length; i++)
			{
				string fen = testFens[i];
				Log($"Testing FEN {i + 1}: {fen}");

				// Call analysis outside of try/catch to avoid yield return issue
				yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(fen, 1000, 3));

				var result = stockfishBridge.LastAnalysisResult;

				if (result == null)
				{
					LogError($"  No result returned for FEN test");
					yield return new WaitForSeconds(0.5f);
					continue;
				}

				if (result.bestMove.StartsWith("ERROR:"))
				{
					Log($"  ✓ Correctly rejected invalid FEN: {result.errorMessage}");
				}
				else if (IsValidTestFen(fen))
				{
					Log($"  ✓ Correctly accepted valid FEN. Best move: {result.bestMove}");
				}
				else
				{
					LogWarning($"  ⚠ Expected error for invalid FEN but got: {result.bestMove}");
				}

				yield return new WaitForSeconds(0.2f);
			}
		}

		/// <summary>
		/// Test basic position analysis functionality
		/// </summary>
		private IEnumerator TestPositionAnalysis()
		{
			Log("\n--- Testing Position Analysis ---");

			// Test starting position
			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine("startpos", 2000, 5));
			var result = stockfishBridge.LastAnalysisResult;

			Log($"Starting position analysis:");
			Log($"  Best move: {result.bestMove}");
			Log($"  White probability: {result.evaluation:F4}");
			Log($"  STM probability: {result.stmEvaluation:F4}");
			Log($"  Side to move: {result.Side}");
			Log($"  Game ended: {result.isGameEnd}");

			// Validate starting position expectations
			if (result.bestMove.StartsWith("ERROR:"))
			{
				LogError("  ✗ Starting position analysis failed");
			}
			else if (result.evaluation >= 0.4f && result.evaluation <= 0.6f)
			{
				Log("  ✓ Starting position evaluation is near balanced");
			}
			else
			{
				LogWarning($"  ⚠ Starting position evaluation seems unusual: {result.evaluation:F4}");
			}

			if (result.Side == 'w')
			{
				Log("  ✓ Correctly identified white to move");
			}
			else
			{
				LogError("  ✗ Incorrect side to move identification");
			}
		}

		/// <summary>
		/// Test mate detection with known mate positions
		/// </summary>
		private IEnumerator TestMateDetection()
		{
			Log("\n--- Testing Mate Detection ---");

			// Test mate in 1 for white
			string mateIn1White = "6k1/5ppp/8/8/8/8/8/4R2K w - - 0 1";
			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(mateIn1White, 3000, 6));

			var result = stockfishBridge.LastAnalysisResult;
			Log($"Mate in 1 for White test:");
			Log($"  Best move: {result.bestMove}");
			Log($"  White probability: {result.evaluation:F4}");
			Log($"  Expected: Very high white probability (>0.95)");

			if (result.evaluation > 0.95f)
			{
				Log("  ✓ Correctly detected strong white advantage");
			}
			else
			{
				LogWarning($"  ⚠ Expected higher white probability for mate position");
			}

			yield return new WaitForSeconds(0.5f);

			// Test mate in 1 for black  
			string mateIn1Black = "4r2k/8/8/8/8/8/5PPP/6K1 b - - 0 1";
			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(mateIn1Black, 3000, 6));

			result = stockfishBridge.LastAnalysisResult;
			Log($"Mate in 1 for Black test:");
			Log($"  Best move: {result.bestMove}");
			Log($"  White probability: {result.evaluation:F4}");
			Log($"  Expected: Very low white probability (<0.05)");

			if (result.evaluation < 0.05f)
			{
				Log("  ✓ Correctly detected strong black advantage");
			}
			else
			{
				LogWarning($"  ⚠ Expected lower white probability for black mate position");
			}
		}

		/// <summary>
		/// Test stalemate detection
		/// </summary>
		private IEnumerator TestStalemateDetection()
		{
			Log("\n--- Testing Stalemate Detection ---");

			// Stalemate position: Black king trapped, no legal moves, not in check
			string stalematePos = "7k/5Q2/5K2/8/8/8/8/8 b - - 0 1";
			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(stalematePos, 3000, 6));

			var result = stockfishBridge.LastAnalysisResult;
			Log($"Stalemate position test:");
			Log($"  Best move: {result.bestMove}");
			Log($"  White probability: {result.evaluation:F4}");
			Log($"  STM probability: {result.stmEvaluation:F4}");
			Log($"  Game ended: {result.isGameEnd}");

			if (result.bestMove == "stale-mate")
			{
				Log("  ✓ Correctly detected stalemate");
			}
			else
			{
				LogWarning($"  ⚠ Expected 'stale-mate' but got: {result.bestMove}");
			}

			if (result.isGameEnd)
			{
				Log("  ✓ Correctly marked game as ended");
			}

			if (Mathf.Abs(result.evaluation - 0.5f) < 0.1f)
			{
				Log("  ✓ Stalemate evaluation near 0.5 (draw)");
			}
			else
			{
				LogWarning($"  ⚠ Stalemate should be near 0.5, got {result.evaluation:F4}");
			}
		}

		/// <summary>
		/// Test evaluation accuracy with known advantage positions
		/// </summary>
		private IEnumerator TestEvaluationAccuracy()
		{
			Log("\n--- Testing Evaluation Accuracy ---");

			// Test large material advantage (should favor white significantly)
			string materialAdvantage = "rnbqk3/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQq - 0 1";
			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(materialAdvantage, 2000, 5));

			var result = stockfishBridge.LastAnalysisResult;
			Log($"Material advantage test (White +Rook):");
			Log($"  White probability: {result.evaluation:F4}");
			Log($"  Expected: Significantly above 0.5 (around 0.7-0.9)");

			if (result.evaluation > 0.7f)
			{
				Log("  ✓ Correctly evaluated material advantage");
			}
			else
			{
				LogWarning($"  ⚠ Expected higher evaluation for material advantage");
			}

			yield return new WaitForSeconds(0.5f);

			// Test balanced middlegame position
			string balancedPos = "rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 0 2";
			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(balancedPos, 2000, 5));

			result = stockfishBridge.LastAnalysisResult;
			Log($"Balanced middlegame test:");
			Log($"  White probability: {result.evaluation:F4}");
			Log($"  Expected: Near 0.5 (0.4-0.6)");

			if (result.evaluation >= 0.4f && result.evaluation <= 0.6f)
			{
				Log("  ✓ Correctly evaluated balanced position");
			}
			else
			{
				LogWarning($"  ⚠ Expected more balanced evaluation");
			}
		}

		/// <summary>
		/// Test side-to-move evaluation calculation
		/// </summary>
		private IEnumerator TestSideToMoveCalculation()
		{
			Log("\n--- Testing Side-to-Move Calculation ---");

			// White to move position
			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine("startpos", 1000, 3));
			var whiteToMoveResult = stockfishBridge.LastAnalysisResult;

			// Black to move position (same but black's turn)
			string blackToMove = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1";
			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(blackToMove, 1000, 3));
			var blackToMoveResult = stockfishBridge.LastAnalysisResult;

			Log($"White to move - White prob: {whiteToMoveResult.evaluation:F4}, STM prob: {whiteToMoveResult.stmEvaluation:F4}");
			Log($"Black to move - White prob: {blackToMoveResult.evaluation:F4}, STM prob: {blackToMoveResult.stmEvaluation:F4}");

			// For white to move: stmEvaluation should equal evaluation
			if (Mathf.Abs(whiteToMoveResult.evaluation - whiteToMoveResult.stmEvaluation) < 0.01f)
			{
				Log("  ✓ Correct STM calculation for white to move");
			}
			else
			{
				LogWarning("  ⚠ STM calculation error for white to move");
			}

			// For black to move: stmEvaluation should equal (1 - evaluation)
			float expectedBlackStm = 1f - blackToMoveResult.evaluation;
			if (Mathf.Abs(expectedBlackStm - blackToMoveResult.stmEvaluation) < 0.01f)
			{
				Log("  ✓ Correct STM calculation for black to move");
			}
			else
			{
				LogWarning("  ⚠ STM calculation error for black to move");
			}
		}

		/// <summary>
		/// Helper method to check if a FEN should be considered valid for testing
		/// </summary>
		private bool IsValidTestFen(string fen)
		{
			// Known valid FENs from our test array
			return fen == "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1" ||
				   fen == "startpos" ||
				   fen == "8/8/8/8/8/8/8/k6K w - - 0 1" ||
				   fen == "rnbqkbnr/pppp1ppp/8/4p3/6P1/5P2/PPPPP2P/RNBQKBNR b KQkq - 0 2" ||
				   fen == "6k1/6p1/6K1/8/8/8/8/7R w - - 0 1" ||
				   fen == "k7/8/K7/8/8/8/8/8 b - - 0 1";
		}

		/// <summary>
		/// Logging helper methods - using colored Debug.Log to prevent Unity crashes
		/// </summary>
		private void Log(string message)
		{
			if (enableDetailedLogging)
			{
				Debug.Log($"<color=cyan>[StockfishBridgeCheck]</color> {message}");
			}
		}

		private void LogWarning(string message)
		{
			Debug.Log($"<color=yellow>[StockfishBridgeCheck WARNING]</color> {message}");
		}

		private void LogError(string message)
		{
			Debug.Log($"<color=red>[StockfishBridgeCheck ERROR]</color> {message}");
		}

		/// <summary>
		/// Individual test methods that can be called separately
		/// </summary>
		[ContextMenu("Test FEN Validation Only")]
		public void TestFenValidationOnly()
		{
			StartCoroutine(TestFenValidation());
		}

		[ContextMenu("Test Basic Analysis Only")]
		public void TestBasicAnalysisOnly()
		{
			StartCoroutine(TestPositionAnalysis());
		}

		[ContextMenu("Test Mate Detection Only")]
		public void TestMateDetectionOnly()
		{
			StartCoroutine(TestMateDetection());
		}

		[ContextMenu("Test Engine Status")]
		public void TestEngineStatus()
		{
			if (stockfishBridge == null)
			{
				LogError("StockfishBridge reference is null");
				return;
			}

			Log($"Engine Running: {stockfishBridge.IsEngineRunning}");
			Log($"Engine Ready: {stockfishBridge.IsReady}");

			if (stockfishBridge.LastAnalysisResult != null)
			{
				var result = stockfishBridge.LastAnalysisResult;
				Log($"Last Result - Move: {result.bestMove}, White: {result.evaluation:F4}, STM: {result.stmEvaluation:F4}");
			}
		}
	}
}
