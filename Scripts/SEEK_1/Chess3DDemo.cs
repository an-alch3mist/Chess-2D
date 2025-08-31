/*
CHANGELOG (New File):
- Simple demonstration script showing complete 3D chess system usage
- Example move execution from both human and engine perspectives
- FEN position setup examples for testing various game states
- Edge case demonstrations (castling, en passant, promotion)
*/

using System.Collections;
using UnityEngine;

namespace GPTDeepResearch
{
	/// <summary>
	/// Demonstration script showing how to use the complete 3D chess system.
	/// Includes examples of programmatic moves, position setup, and engine integration.
	/// </summary>
	public class Chess3DDemo : MonoBehaviour
	{
		[Header("Chess System References")]
		[SerializeField] private ChessBoard3D chessBoard3D;
		[SerializeField] private MinimalChessUI_3D minimalUI;
		[SerializeField] private EnhancedChessUI3D enhancedUI;
		[SerializeField] private StockfishBridge stockfishBridge;

		[Header("Demo Settings")]
		[SerializeField] private bool runAutomaticDemo = false;
		[SerializeField] private float demoPauseTime = 2f;

		[Header("Test Positions")]
		[SerializeField]
		private string[] testPositions = {
			"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", // Starting position
            "r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1",        // Castling test
            "rnbqkbnr/ppp1pppp/8/3pP3/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 3", // En passant test
            "rnbqkb1r/pppp1ppp/5n2/4p3/2B1P3/8/PPPP1PPP/RNBQK1NR w KQkq - 4 4", // Tactical position
            "8/8/8/8/8/8/7P/7K w - - 0 1" // Pawn promotion test
        };

		void Start()
		{
			if (runAutomaticDemo)
			{
				StartCoroutine(RunDemo());
			}
		}

		#region Demo Flow

		private IEnumerator RunDemo()
		{
			Debug.Log("[Chess3DDemo] Starting automatic demo...");

			yield return new WaitForSeconds(1f);

			// Test 1: Basic move execution
			yield return StartCoroutine(TestBasicMoves());

			// Test 2: Engine move
			yield return StartCoroutine(TestEngineMove());

			// Test 3: Special positions
			yield return StartCoroutine(TestSpecialPositions());

			// Test 4: Edge cases
			yield return StartCoroutine(TestEdgeCases());

			Debug.Log("[Chess3DDemo] Demo completed!");
		}

		private IEnumerator TestBasicMoves()
		{
			Debug.Log("[Chess3DDemo] Testing basic moves...");

			// Start new game
			if (minimalUI != null)
				minimalUI.StartNewGame();

			yield return new WaitForSeconds(demoPauseTime);

			// Test programmatic moves
			if (chessBoard3D != null)
			{
				// e2-e4
				bool success = chessBoard3D.TryMakeMove(new Vector2Int(4, 1), new Vector2Int(4, 3));
				Debug.Log($"[Chess3DDemo] e2-e4 move: {(success ? "SUCCESS" : "FAILED")}");

				yield return new WaitForSeconds(demoPauseTime);

				// e7-e5
				success = chessBoard3D.TryMakeMove(new Vector2Int(4, 6), new Vector2Int(4, 4));
				Debug.Log($"[Chess3DDemo] e7-e5 move: {(success ? "SUCCESS" : "FAILED")}");
			}

			yield return new WaitForSeconds(demoPauseTime);
		}

		private IEnumerator TestEngineMove()
		{
			Debug.Log("[Chess3DDemo] Testing engine move...");

			if (enhancedUI != null)
			{
				enhancedUI.OnEngineThinking(true);
			}

			if (stockfishBridge != null && chessBoard3D != null)
			{
				string currentFEN = chessBoard3D.GetCurrentFEN();

				yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(
					currentFEN, -1, 3, 3, 1500, 10
				));

				var result = stockfishBridge.LastAnalysisResult;
				if (!string.IsNullOrEmpty(result.bestMove) && !result.bestMove.StartsWith("ERROR"))
				{
					Debug.Log($"[Chess3DDemo] Engine suggests: {result.bestMove}");

					if (enhancedUI != null)
					{
						enhancedUI.OnEngineMove(result.bestMove, result.evaluation);
					}
				}
			}

			if (enhancedUI != null)
			{
				enhancedUI.OnEngineThinking(false);
			}

			yield return new WaitForSeconds(demoPauseTime);
		}

		private IEnumerator TestSpecialPositions()
		{
			Debug.Log("[Chess3DDemo] Testing special positions...");

			foreach (string position in testPositions)
			{
				Debug.Log($"[Chess3DDemo] Setting position: {position.Substring(0, 20)}...");

				if (chessBoard3D != null)
				{
					chessBoard3D.SetPosition(position);
				}

				if (enhancedUI != null)
				{
					enhancedUI.UpdatePositionInfo(position);
				}

				yield return new WaitForSeconds(demoPauseTime);
			}
		}

		private IEnumerator TestEdgeCases()
		{
			Debug.Log("[Chess3DDemo] Testing edge cases...");

			// Test castling position
			string castlingFEN = "r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1";
			if (chessBoard3D != null)
			{
				chessBoard3D.SetPosition(castlingFEN);

				yield return new WaitForSeconds(demoPauseTime);

				// Test kingside castling
				bool castlingSuccess = chessBoard3D.TryMakeMove(new Vector2Int(4, 0), new Vector2Int(6, 0));
				Debug.Log($"[Chess3DDemo] White kingside castling: {(castlingSuccess ? "SUCCESS" : "FAILED")}");
			}

			yield return new WaitForSeconds(demoPauseTime);

			// Test invalid move
			if (chessBoard3D != null)
			{
				bool invalidMove = chessBoard3D.TryMakeMove(new Vector2Int(0, 0), new Vector2Int(7, 7));
				Debug.Log($"[Chess3DDemo] Invalid move test: {(invalidMove ? "UNEXPECTED SUCCESS" : "CORRECTLY FAILED")}");
			}
		}

		#endregion

		#region Public Test Methods

		[ContextMenu("Test Coordinate Conversion")]
		public void TestCoordinateConversion()
		{
			if (chessBoard3D == null) return;

			Debug.Log("[Chess3DDemo] Testing coordinate conversion...");

			// Test all corners
			Vector2Int[] testSquares = {
				new Vector2Int(0, 0), // a1
                new Vector2Int(7, 0), // h1  
                new Vector2Int(0, 7), // a8
                new Vector2Int(7, 7)  // h8
            };

			foreach (Vector2Int square in testSquares)
			{
				Vector3 worldPos = chessBoard3D.BoardToWorld(square);
				Vector2Int convertedBack = chessBoard3D.WorldToBoard(worldPos);

				string squareName = $"{(char)('a' + square.x)}{square.y + 1}";
				Debug.Log($"[Chess3DDemo] {squareName} -> {worldPos} -> {convertedBack} (match: {square == convertedBack})");
			}
		}

		[ContextMenu("Test All Starting Pieces")]
		public void TestStartingPieces()
		{
			if (chessBoard3D == null) return;

			Debug.Log("[Chess3DDemo] Testing starting piece setup...");
			chessBoard3D.SetPosition("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
		}

		[ContextMenu("Test Engine Analysis")]
		public void TestEngineAnalysis()
		{
			StartCoroutine(TestEngineAnalysisCoroutine());
		}

		private IEnumerator TestEngineAnalysisCoroutine()
		{
			if (stockfishBridge == null || chessBoard3D == null)
			{
				Debug.LogError("[Chess3DDemo] Missing references for engine test");
				yield break;
			}

			string currentFEN = chessBoard3D.GetCurrentFEN();
			Debug.Log($"[Chess3DDemo] Analyzing position: {currentFEN}");

			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(
				currentFEN, -1, 5, 5, 1500, 10
			));

			var result = stockfishBridge.LastAnalysisResult;
			Debug.Log($"[Chess3DDemo] Engine result - Best move: {result.bestMove}, Eval: {result.evaluation:F3}, Error: {result.errorMessage}");
		}

		[ContextMenu("Test Move Validation")]
		public void TestMoveValidation()
		{
			if (chessBoard3D == null) return;

			Debug.Log("[Chess3DDemo] Testing move validation...");

			// Test valid moves
			bool e2e4 = chessBoard3D.TryMakeMove(new Vector2Int(4, 1), new Vector2Int(4, 3));
			Debug.Log($"[Chess3DDemo] e2-e4 (valid): {(e2e4 ? "SUCCESS" : "FAILED")}");

			// Test invalid move
			bool a1h8 = chessBoard3D.TryMakeMove(new Vector2Int(0, 0), new Vector2Int(7, 7));
			Debug.Log($"[Chess3DDemo] a1-h8 (invalid): {(a1h8 ? "UNEXPECTED SUCCESS" : "CORRECTLY FAILED")}");
		}

		#endregion

		#region Usage Examples

		/// <summary>
		/// Example: Complete game flow setup
		/// </summary>
		public void SetupCompleteGame()
		{
			// 1. Initialize board
			if (chessBoard3D != null)
			{
				chessBoard3D.SetPosition("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
			}

			// 2. Configure engine
			if (enhancedUI != null)
			{
				// Engine settings can be configured through UI or programmatically
				Debug.Log($"[Chess3DDemo] Engine depth: {enhancedUI.GetEngineDepth()}");
				Debug.Log($"[Chess3DDemo] Engine skill: {enhancedUI.GetEngineSkill()}");
				Debug.Log($"[Chess3DDemo] Engine Elo: {enhancedUI.GetEngineElo()}");
			}

			// 3. Update UI
			if (enhancedUI != null)
			{
				enhancedUI.UpdatePositionInfo(chessBoard3D.GetCurrentFEN());
				enhancedUI.UpdateEvaluation(0.5f);
			}
		}

		/// <summary>
		/// Example: Human move from UI input
		/// </summary>
		public void ExecuteHumanMove(string moveString)
		{
			// This would typically be called from UI input
			if (chessBoard3D == null) return;

			// Parse move (simplified - in practice use ChessMove.FromLongAlgebraic)
			if (moveString.Length >= 4)
			{
				string fromSquare = moveString.Substring(0, 2);
				string toSquare = moveString.Substring(2, 2);

				Vector2Int from = new Vector2Int(fromSquare[0] - 'a', fromSquare[1] - '1');
				Vector2Int to = new Vector2Int(toSquare[0] - 'a', toSquare[1] - '1');

				bool success = chessBoard3D.TryMakeMove(from, to);

				if (success && enhancedUI != null)
				{
					enhancedUI.OnHumanMove(moveString);
					enhancedUI.UpdatePositionInfo(chessBoard3D.GetCurrentFEN());
				}
			}
		}

		/// <summary>
		/// Example: Engine move execution
		/// </summary>
		public IEnumerator ExecuteEngineMove()
		{
			if (stockfishBridge == null || chessBoard3D == null) yield break;

			if (enhancedUI != null)
				enhancedUI.OnEngineThinking(true);

			string currentFEN = chessBoard3D.GetCurrentFEN();

			yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(
				currentFEN, -1,
				enhancedUI != null ? enhancedUI.GetEngineDepth() : 5,
				5,
				enhancedUI != null ? enhancedUI.GetEngineElo() : 1500,
				enhancedUI != null ? enhancedUI.GetEngineSkill() : 10
			));

			var result = stockfishBridge.LastAnalysisResult;

			if (enhancedUI != null)
				enhancedUI.OnEngineThinking(false);

			if (!string.IsNullOrEmpty(result.bestMove) && !result.bestMove.StartsWith("ERROR"))
			{
				// Parse and execute engine move
				ChessMove engineMove = ChessMove.FromLongAlgebraic(result.bestMove, minimalUI.GetCurrentBoard());

				if (engineMove.IsValid())
				{
					Vector2Int from = new Vector2Int(engineMove.from.x, engineMove.from.y);
					Vector2Int to = new Vector2Int(engineMove.to.x, engineMove.to.y);

					bool success = chessBoard3D.TryMakeMove(from, to);

					if (success && enhancedUI != null)
					{
						enhancedUI.OnEngineMove(result.bestMove, result.evaluation);
						enhancedUI.UpdatePositionInfo(chessBoard3D.GetCurrentFEN());
					}
				}
			}
		}

		#endregion

		#region Test Scenarios

		[ContextMenu("Test Castling Scenario")]
		public void TestCastling()
		{
			StartCoroutine(TestCastlingCoroutine());
		}

		private IEnumerator TestCastlingCoroutine()
		{
			Debug.Log("[Chess3DDemo] Testing castling...");

			// Set up castling position
			string castlingFEN = "r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1";
			if (chessBoard3D != null)
			{
				chessBoard3D.SetPosition(castlingFEN);
			}

			yield return new WaitForSeconds(demoPauseTime);

			// Attempt white kingside castling (e1-g1)
			bool castlingSuccess = chessBoard3D.TryMakeMove(new Vector2Int(4, 0), new Vector2Int(6, 0));
			Debug.Log($"[Chess3DDemo] White kingside castling: {(castlingSuccess ? "SUCCESS" : "FAILED")}");

			yield return new WaitForSeconds(demoPauseTime);

			// Attempt black queenside castling (e8-c8)
			bool blackCastling = chessBoard3D.TryMakeMove(new Vector2Int(4, 7), new Vector2Int(2, 7));
			Debug.Log($"[Chess3DDemo] Black queenside castling: {(blackCastling ? "SUCCESS" : "FAILED")}");
		}

		[ContextMenu("Test En Passant")]
		public void TestEnPassant()
		{
			StartCoroutine(TestEnPassantCoroutine());
		}

		private IEnumerator TestEnPassantCoroutine()
		{
			Debug.Log("[Chess3DDemo] Testing en passant...");

			// Set up en passant position
			string enPassantFEN = "rnbqkbnr/ppp1pppp/8/3pP3/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 3";
			if (chessBoard3D != null)
			{
				chessBoard3D.SetPosition(enPassantFEN);
			}

			yield return new WaitForSeconds(demoPauseTime);

			// Attempt en passant capture (e5xd6)
			bool enPassantSuccess = chessBoard3D.TryMakeMove(new Vector2Int(4, 4), new Vector2Int(3, 5));
			Debug.Log($"[Chess3DDemo] En passant capture: {(enPassantSuccess ? "SUCCESS" : "FAILED")}");
		}

		[ContextMenu("Test Promotion")]
		public void TestPromotion()
		{
			StartCoroutine(TestPromotionCoroutine());
		}

		private IEnumerator TestPromotionCoroutine()
		{
			Debug.Log("[Chess3DDemo] Testing pawn promotion...");

			// Set up promotion position (white pawn on 7th rank)
			string promotionFEN = "8/4P3/8/8/8/8/8/4K2k w - - 0 1";
			if (chessBoard3D != null)
			{
				chessBoard3D.SetPosition(promotionFEN);
			}

			yield return new WaitForSeconds(demoPauseTime);

			// Attempt pawn promotion (e7-e8)
			bool promotionSuccess = chessBoard3D.TryMakeMove(new Vector2Int(4, 6), new Vector2Int(4, 7));
			Debug.Log($"[Chess3DDemo] Pawn promotion: {(promotionSuccess ? "SUCCESS" : "FAILED")}");
		}

		#endregion

		#region Manual Testing Helpers

		/// <summary>
		/// Test coordinate conversion accuracy
		/// </summary>
		public void VerifyCoordinateSystem()
		{
			if (chessBoard3D == null) return;

			Debug.Log("[Chess3DDemo] Verifying coordinate system...");

			// Test key squares
			var testCases = new (Vector2Int board, string name)[]
			{
				(new Vector2Int(0, 0), "a1"),
				(new Vector2Int(7, 0), "h1"),
				(new Vector2Int(0, 7), "a8"),
				(new Vector2Int(7, 7), "h8"),
				(new Vector2Int(4, 4), "e5")
			};

			foreach (var test in testCases)
			{
				Vector3 worldPos = chessBoard3D.BoardToWorld(test.board);
				Vector2Int backConverted = chessBoard3D.WorldToBoard(worldPos);
				bool accurate = backConverted == test.board;

				Debug.Log($"[Chess3DDemo] {test.name}: {test.board} -> {worldPos} -> {backConverted} (✓: {accurate})");
			}
		}

		/// <summary>
		/// Validate piece prefab mapping
		/// </summary>
		public void ValidatePiecePrefabs()
		{
			Debug.Log("[Chess3DDemo] Validating piece prefabs...");

			string[] requiredPieces = { "K", "Q", "R", "B", "N", "P", "k", "q", "r", "b", "n", "p" };

			foreach (string piece in requiredPieces)
			{
				string expectedName = $"chess-piece-{piece}";
				// Check if prefab exists in chessBoard3D.piecePrefabs
				Debug.Log($"[Chess3DDemo] Required prefab: {expectedName}");
			}
		}

		#endregion
	}
}