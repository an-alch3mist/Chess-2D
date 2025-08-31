/*
CHANGELOG (New File):
- Complete system usage example showing the entire 3D chess flow
- Integration pattern between all chess components
- Scene setup and prefab configuration guide
- Step-by-step workflow from initialization to gameplay
*/

using System.Collections;
using UnityEngine;

namespace GPTDeepResearch
{
	/// <summary>
	/// Simple example showing how to use the complete 3D interactive chess system.
	/// This script demonstrates the full workflow without missing any critical steps.
	/// </summary>
	public class SimpleChessUsage : MonoBehaviour
	{
		[Header("SCENE SETUP - Drag these from your scene")]
		[SerializeField] private MinimalChessUI_3D chessUI;
		[SerializeField] private ChessBoard3D board3D;
		[SerializeField] private EnhancedChessUI3D enhancedUI;
		[SerializeField] private StockfishBridge engine;

		[Header("TESTING")]
		[SerializeField] private bool autoPlay = false;

		void Start()
		{
			Debug.Log("=== CHESS 3D SYSTEM STARTUP ===");

			// Step 1: Verify all components are connected
			if (!ValidateSetup())
			{
				Debug.LogError("[SimpleChessUsage] Setup validation failed! Check inspector references.");
				return;
			}

			// Step 2: Initialize the complete system
			InitializeChessSystem();

			// Step 3: Start a game
			StartCompleteGame();

			if (autoPlay)
			{
				StartCoroutine(DemoCompleteFlow());
			}
		}

		#region System Validation and Setup

		private bool ValidateSetup()
		{
			bool valid = true;

			if (chessUI == null)
			{
				Debug.LogError("[SimpleChessUsage] MinimalChessUI_3D reference missing!");
				valid = false;
			}

			if (board3D == null)
			{
				Debug.LogError("[SimpleChessUsage] ChessBoard3D reference missing!");
				valid = false;
			}

			if (enhancedUI == null)
			{
				Debug.LogWarning("[SimpleChessUsage] EnhancedChessUI3D reference missing - 3D UI features disabled");
			}

			if (engine == null)
			{
				Debug.LogError("[SimpleChessUsage] StockfishBridge reference missing!");
				valid = false;
			}

			return valid;
		}

		private void InitializeChessSystem()
		{
			Debug.Log("[SimpleChessUsage] Initializing chess system...");

			// 1. Enable 3D mode in the UI
			if (chessUI != null)
			{
				chessUI.SetUse3D(true);
			}

			// 2. Configure engine settings (optional)
			if (enhancedUI != null)
			{
				// These values will be picked up by the engine
				Debug.Log($"[SimpleChessUsage] Engine configured - Depth: {enhancedUI.GetEngineDepth()}, Elo: {enhancedUI.GetEngineElo()}");
			}

			// 3. Ensure engine is running
			if (engine != null && !engine.IsEngineRunning)
			{
				engine.StartEngine();
				Debug.Log("[SimpleChessUsage] Stockfish engine started");
			}
		}

		private void StartCompleteGame()
		{
			Debug.Log("[SimpleChessUsage] Starting new game...");

			// Start new game through UI (this will sync everything)
			if (chessUI != null)
			{
				chessUI.StartNewGame();
			}

			Debug.Log("[SimpleChessUsage] Game ready! Players can now:");
			Debug.Log("- Click and drag pieces in 3D");
			Debug.Log("- See legal move indicators");
			Debug.Log("- View real-time evaluation");
			Debug.Log("- Engine will respond automatically");
		}

		#endregion

		#region Complete Demo Flow

		private IEnumerator DemoCompleteFlow()
		{
			Debug.Log("=== COMPLETE CHESS SYSTEM DEMO ===");
			yield return new WaitForSeconds(2f);

			// Demo 1: Human move via 3D interaction (simulated)
			Debug.Log("[Demo] Simulating human move: e2-e4");
			bool humanMove = SimulateHumanMove("e2", "e4");
			Debug.Log($"[Demo] Human move result: {(humanMove ? "SUCCESS" : "FAILED")}");

			yield return new WaitForSeconds(2f);

			// Demo 2: Engine response
			Debug.Log("[Demo] Waiting for engine response...");
			yield return new WaitForSeconds(3f); // Engine should respond automatically

			// Demo 3: Check current game state
			if (chessUI != null)
			{
				string currentFEN = chessUI.GetCurrentFEN();
				Debug.Log($"[Demo] Current position: {currentFEN}");
			}

			// Demo 4: Show evaluation if available
			if (enhancedUI != null)
			{
				float eval = enhancedUI.GetCurrentEvaluation();
				Debug.Log($"[Demo] Current evaluation: {eval:F3} (white perspective)");
			}

			yield return new WaitForSeconds(2f);

			// Demo 5: Test special position
			Debug.Log("[Demo] Testing castling position...");
			TestCastlingPosition();

			Debug.Log("=== DEMO COMPLETE ===");
		}

		private bool SimulateHumanMove(string fromSquare, string toSquare)
		{
			if (board3D == null) return false;

			// Convert algebraic to coordinates
			Vector2Int from = new Vector2Int(fromSquare[0] - 'a', fromSquare[1] - '1');
			Vector2Int to = new Vector2Int(toSquare[0] - 'a', toSquare[1] - '1');

			return board3D.TryMakeMove(from, to);
		}

		private void TestCastlingPosition()
		{
			string castlingFEN = "r3k2r/pppppppp/8/8/8/8/PPPPPPPP/R3K2R w KQkq - 0 1";

			if (chessUI != null)
			{
				chessUI.SetPosition(castlingFEN);
				Debug.Log("[Demo] Castling position set. Try moving the white king 2 squares!");
			}
		}

		#endregion

		#region Public Testing Interface

		/// <summary>
		/// Test the complete system with a full game flow
		/// </summary>
		[ContextMenu("Test Complete Game Flow")]
		public void TestCompleteGameFlow()
		{
			StartCoroutine(TestCompleteGameFlowCoroutine());
		}

		private IEnumerator TestCompleteGameFlowCoroutine()
		{
			Debug.Log("=== TESTING COMPLETE GAME FLOW ===");

			// 1. New game
			chessUI?.StartNewGame();
			yield return new WaitForSeconds(1f);

			// 2. Human move
			bool move1 = SimulateHumanMove("e2", "e4");
			Debug.Log($"1. e2-e4: {(move1 ? "✓" : "✗")}");
			yield return new WaitForSeconds(2f);

			// 3. Second human move  
			bool move2 = SimulateHumanMove("d2", "d4");
			Debug.Log($"2. d2-d4: {(move2 ? "✓" : "✗")}");
			yield return new WaitForSeconds(2f);

			// 4. Test invalid move
			bool invalidMove = SimulateHumanMove("a1", "h8");
			Debug.Log($"3. Invalid move a1-h8: {(invalidMove ? "✗ SHOULD FAIL" : "✓ CORRECTLY REJECTED")}");

			// 5. Check final state
			if (chessUI != null)
			{
				Debug.Log($"Final FEN: {chessUI.GetCurrentFEN()}");
			}

			Debug.Log("=== GAME FLOW TEST COMPLETE ===");
		}

		/// <summary>
		/// Test engine analysis on current position
		/// </summary>
		[ContextMenu("Test Engine Analysis")]
		public void TestEngineOnCurrentPosition()
		{
			if (engine == null || chessUI == null) return;

			StartCoroutine(TestEngineAnalysisCoroutine());
		}

		private IEnumerator TestEngineAnalysisCoroutine()
		{
			Debug.Log("[SimpleChessUsage] Testing engine analysis...");

			string currentFEN = chessUI.GetCurrentFEN();

			yield return StartCoroutine(engine.AnalyzePositionCoroutine(
				currentFEN, -1, 5, 5, 1500, 10
			));

			var result = engine.LastAnalysisResult;
			Debug.Log($"[Engine Result]");
			Debug.Log($"  Best Move: {result.bestMove}");
			Debug.Log($"  Evaluation: {result.evaluation:F3}");
			Debug.Log($"  Side-to-move eval: {result.stmEvaluation:F3}");
			Debug.Log($"  Search depth: {result.searchDepth}");
			Debug.Log($"  Approx Elo: {result.approximateElo}");
			Debug.Log($"  Error: {result.errorMessage ?? "None"}");
		}

		#endregion

		#region Scene Setup Guide (for Documentation)

		/// <summary>
		/// SCENE SETUP INSTRUCTIONS (Execute in Editor)
		/// 
		/// 1. CAMERA SETUP:
		///    - Position: (4, 8, -6)
		///    - Rotation: (45, 0, 0)
		///    - Projection: Perspective
		///    - Layer Mask: Everything except UI
		/// 
		/// 2. CHESS BOARD 3D OBJECT:
		///    - Empty GameObject with ChessBoard3D script
		///    - Position: (0, 0, 0)
		///    - Tile Size: 1.0
		///    - Board Origin: (0, 0, 0)
		/// 
		/// 3. PIECE PREFABS (Required naming):
		///    - chess-piece-K (White King)
		///    - chess-piece-Q (White Queen)  
		///    - chess-piece-R (White Rook)
		///    - chess-piece-B (White Bishop)
		///    - chess-piece-N (White Knight)
		///    - chess-piece-P (White Pawn)
		///    - chess-piece-k (Black King)
		///    - chess-piece-q (Black Queen)
		///    - chess-piece-r (Black Rook)
		///    - chess-piece-b (Black Bishop)
		///    - chess-piece-n (Black Knight)
		///    - chess-piece-p (Black Pawn)
		/// 
		/// 4. EACH PIECE PREFAB MUST HAVE:
		///    - MeshRenderer (with piece model)
		///    - BoxCollider (NOT trigger, sized to encompass piece)
		///    - ChessPieceController script
		///    - Optional: Animator with states (Idle, Move, Attack, Captured)
		/// 
		/// 5. VISUAL FEEDBACK PREFABS:
		///    - validMovePrefab: Green cube (0.8, 0.1, 0.8)
		///    - captureTargetPrefab: Red cube (0.8, 0.1, 0.8)
		///    - selectedTilePrefab: Yellow outline (1, 0.05, 1)
		/// 
		/// 6. UI CANVAS SETUP:
		///    - MinimalChessUI_3D script on Canvas or dedicated GameObject
		///    - EnhancedChessUI3D script for evaluation bars and controls
		///    - All UI elements assigned in inspector
		/// 
		/// 7. ENGINE SETUP:
		///    - StockfishBridge script on GameObject
		///    - sf-engine.exe in StreamingAssets folder
		/// 
		/// 8. INTEGRATION:
		///    - All components reference each other through inspector
		///    - SimpleChessUsage script for testing and validation
		/// </summary>
		[ContextMenu("Show Scene Setup Guide")]
		public void ShowSceneSetupGuide()
		{
			Debug.Log("Scene setup guide printed in code comments above this method!");
		}

		#endregion

		#region Usage Examples

		/// <summary>
		/// EXAMPLE 1: Start a new game programmatically
		/// </summary>
		public void ExampleStartNewGame()
		{
			// This is the simplest way to start a complete chess game
			if (chessUI != null)
			{
				chessUI.StartNewGame(); // Handles everything: board setup, 3D sync, engine readiness
			}
		}

		/// <summary>
		/// EXAMPLE 2: Set up a specific position for analysis
		/// </summary>
		public void ExampleSetSpecificPosition()
		{
			// Test position with tactical possibilities
			string testFEN = "rnbqkb1r/pppp1ppp/5n2/4p3/2B1P3/8/PPPP1PPP/RNBQK1NR w KQkq - 4 4";

			if (chessUI != null)
			{
				chessUI.SetPosition(testFEN); // Automatically syncs 3D board
			}
		}

		/// <summary>
		/// EXAMPLE 3: Human vs Engine interaction flow
		/// </summary>
		public IEnumerator ExampleHumanVsEngine()
		{
			// 1. Human makes move by dragging piece in 3D
			// (This happens automatically through ChessPieceController)

			// 2. Engine automatically responds (handled by MinimalChessUI_3D)
			// Wait for engine move
			yield return new WaitUntil(() => !chessUI.gameObject.GetComponent<MinimalChessUI_3D>().waitingForEngine);

			// 3. Check game result
			string fen = chessUI.GetCurrentFEN();
			Debug.Log($"Position after engine move: {fen}");
		}

		/// <summary>
		/// EXAMPLE 4: Configure engine difficulty
		/// </summary>
		public void ExampleConfigureEngine()
		{
			if (chessUI != null)
			{
				// Beginner settings
				chessUI.SetEngineSettings(depth: 2, elo: 800, skill: 5);

				// Advanced settings  
				// chessUI.SetEngineSettings(depth: 15, elo: 2500, skill: 20);
			}
		}

		/// <summary>
		/// EXAMPLE 5: Monitor game state and evaluation
		/// </summary>
		public void ExampleMonitorGame()
		{
			if (enhancedUI != null)
			{
				float currentEval = enhancedUI.GetCurrentEvaluation();
				Debug.Log($"Current evaluation: {currentEval:F3} (0.0=Black winning, 1.0=White winning)");
			}

			if (chessUI != null)
			{
				ChessBoard board = chessUI.GetCurrentBoard();
				var gameResult = ChessRules.EvaluatePosition(board);
				Debug.Log($"Game state: {gameResult}");
			}
		}

		#endregion

		#region Complete System Validation

		/// <summary>
		/// Validate the entire system is working correctly
		/// </summary>
		[ContextMenu("Validate Complete System")]
		public void ValidateCompleteSystem()
		{
			Debug.Log("=== COMPLETE SYSTEM VALIDATION ===");

			// 1. Check components
			bool allComponentsReady = ValidateSetup();
			Debug.Log($"1. Components Ready: {(allComponentsReady ? "✓" : "✗")}");

			// 2. Check engine
			bool engineReady = engine != null && engine.IsEngineRunning;
			Debug.Log($"2. Engine Ready: {(engineReady ? "✓" : "✗")}");

			// 3. Check 3D board
			bool board3DReady = board3D != null;
			Debug.Log($"3. 3D Board Ready: {(board3DReady ? "✓" : "✗")}");

			// 4. Check current position
			if (chessUI != null)
			{
				string fen = chessUI.GetCurrentFEN();
				bool validFEN = !string.IsNullOrEmpty(fen);
				Debug.Log($"4. Valid Position: {(validFEN ? "✓" : "✗")} - {fen}");
			}

			// 5. Test coordinate conversion
			if (board3D != null)
			{
				Vector3 worldPos = board3D.BoardToWorld(new Vector2Int(4, 4)); // e5
				Vector2Int backConverted = board3D.WorldToBoard(worldPos);
				bool coordsWork = backConverted == new Vector2Int(4, 4);
				Debug.Log($"5. Coordinate System: {(coordsWork ? "✓" : "✗")} - e5 -> {worldPos} -> e5");
			}

			Debug.Log("=== VALIDATION COMPLETE ===");
			Debug.Log("If all checks show ✓, the system is ready for gameplay!");
		}

		#endregion

		#region Manual Testing Helpers

		/// <summary>
		/// Quick test: Make a few moves and verify everything works
		/// </summary>
		[ContextMenu("Quick Gameplay Test")]
		public void QuickGameplayTest()
		{
			StartCoroutine(QuickTestCoroutine());
		}

		private IEnumerator QuickTestCoroutine()
		{
			Debug.Log("[Quick Test] Starting gameplay test...");

			// Reset to starting position
			chessUI?.StartNewGame();
			yield return new WaitForSeconds(1f);

			// Make a few moves
			bool move1 = SimulateHumanMove("e2", "e4");
			Debug.Log($"[Quick Test] 1. e2-e4: {(move1 ? "✓" : "✗")}");
			yield return new WaitForSeconds(1f);

			bool move2 = SimulateHumanMove("d2", "d4");
			Debug.Log($"[Quick Test] 2. d2-d4: {(move2 ? "✓" : "✗")}");
			yield return new WaitForSeconds(1f);

			bool move3 = SimulateHumanMove("g1", "f3");
			Debug.Log($"[Quick Test] 3. Ng1-f3: {(move3 ? "✓" : "✗")}");

			Debug.Log("[Quick Test] Gameplay test complete!");
		}

		/// <summary>
		/// Emergency reset if system gets into bad state
		/// </summary>
		[ContextMenu("Emergency Reset")]
		public void EmergencyReset()
		{
			Debug.Log("[SimpleChessUsage] Emergency reset initiated...");

			if (chessUI != null)
			{
				chessUI.StartNewGame();
			}

			if (engine != null)
			{
				engine.StopEngine();
				engine.StartEngine();
			}

			Debug.Log("[SimpleChessUsage] System reset complete!");
		}

		#endregion
	}
}