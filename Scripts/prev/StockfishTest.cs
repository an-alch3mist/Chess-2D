using System.Collections;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

namespace GPTDeepResearch
{
	/// <summary>
	/// Example usage of StockfishBridge
	/// </summary>
	public class StockfishTest : MonoBehaviour
	{

		[Header("Test Configuration")]
		[SerializeField] private string testFen = "startpos";
		// MODIFY: Change default movetime to shorter for weak play fallback
		[SerializeField, Tooltip("Used only if searchDepth <= 0. Use 10-50ms for very weak fallback timing.")]
		private int moveTimeMs = 200;
		// ADD: Search depth parameter for deterministic weak play
		[SerializeField, Tooltip("If >0, use 'go depth <N>'; prefer this over movetime for deterministic weak play. Use 1-3 for very weak play.")]
		private int searchDepth = 1;
		[SerializeField] private int engineElo = 400;  // -1 = max strength
													  // ADD: Skill level parameter for additional weakness
		[SerializeField, Tooltip("Stockfish Skill Level 0-20, where 0 is weakest. Combines with Elo and depth for very weak play.")]
		private int skillLevel = 0;  // -1 = disabled

		[Header("UI")]
		[SerializeField] private Button analyzeButton;
		[SerializeField] private TMP_InputField outputText;
		[SerializeField] private TMP_InputField statusText;

		private StockfishBridge stockfish;
		// ADD: Track if engine is initialized to prevent multiple button clicks
		private bool isEngineInitialized = false;

		void Start()
		{
			Debug.Log("Start(): " + this);

			// Get or create StockfishBridge component
			stockfish = GetComponent<StockfishBridge>();
			if (stockfish == null)
			{
				stockfish = gameObject.AddComponent<StockfishBridge>();
			}

			// Subscribe to engine output (optional - for real-time updates)
			stockfish.OnEngineLine.AddListener(OnEngineLineReceived);

			// Setup UI
			if (analyzeButton != null)
			{
				analyzeButton.onClick.AddListener(StartAnalysisTest);
			}

			// MODIFY: Start engine immediately but don't run auto-test
			StartCoroutine(InitializeEngineCoroutine());
		}

		/// <summary>
		/// Initialize and test the engine
		/// </summary>
		IEnumerator InitializeEngineCoroutine()
		{
			UpdateStatus("Starting Stockfish engine...");

			stockfish.StartEngine();

			// Wait for engine to initialize
			yield return StartCoroutine(stockfish.InitializeEngineCoroutine());

			if (stockfish.IsReady)
			{
				UpdateStatus("Stockfish ready! Press 'Analyze' to test.");
				Debug.Log("[Test] Stockfish engine is ready for use");
				// ADD: Mark engine as initialized
				isEngineInitialized = true;

				// REMOVE: Auto-test removed - only runs when button is pressed
				// No automatic test anymore
			}
			else
			{
				UpdateStatus("Failed to initialize Stockfish");
				Debug.LogError("[Test] Failed to initialize Stockfish engine");
				isEngineInitialized = false;
			}
		}

		/// <summary>
		/// Run a basic analysis test
		/// </summary>
		IEnumerator RunBasicTest()
		{
			// ADD: Prevent multiple simultaneous analyses
			if (analyzeButton != null)
			{
				analyzeButton.interactable = false;
			}

			UpdateStatus("Running basic analysis test...");
			Debug.Log("[Test] Starting basic analysis test");

			// MODIFY: Pass all weakness parameters to bridge
			yield return StartCoroutine(stockfish.GetNextMoveCoroutine(testFen, moveTimeMs, searchDepth, engineElo, skillLevel));

			// Display results
			string result = stockfish.LastRawOutput;
			Debug.Log($"[Test] Analysis complete. Output:\n{result}");

			if (result.Contains("bestmove"))
			{
				UpdateStatus("Test passed! Engine is working correctly.");
				UpdateOutput($"Analysis Results:\n\n{result}");

				// Extract best move for display
				string[] lines = result.Split('\n');
				foreach (string line in lines)
				{
					if (line.StartsWith("bestmove"))
					{
						Debug.Log($"[Test] Best move found: {line}");
						break;
					}
				}
			}
			else
			{
				UpdateStatus("Test failed - no bestmove found");
				UpdateOutput($"Error: No valid move found\n\n{result}");
				Debug.LogError("[Test] Analysis failed - no bestmove in output");
			}

			// ADD: Re-enable button after analysis
			if (analyzeButton != null)
			{
				analyzeButton.interactable = true;
			}
		}

		/// <summary>
		/// Manual test trigger (called by UI button)
		/// </summary>
		public void StartAnalysisTest()
		{
			// MODIFY: Check if engine is initialized and not currently analyzing
			if (stockfish != null && stockfish.IsEngineRunning && isEngineInitialized)
			{
				// Only start if button is still interactable (prevents double-clicks)
				if (analyzeButton == null || analyzeButton.interactable)
				{
					StartCoroutine(RunBasicTest());
				}
			}
			else
			{
				UpdateStatus("Engine not ready. Wait for initialization.");
			}
		}

		/// <summary>
		/// Called for each line received from engine (real-time updates)
		/// </summary>
		private void OnEngineLineReceived(string line)
		{
			// Example: Show real-time info lines (eval, depth, etc.)
			if (line.StartsWith("info") && line.Contains("score"))
			{
				Debug.Log($"[Test] Engine info: {line}");
				// Could update UI with current evaluation here
			}
		}

		/// <summary>
		/// Test with custom position
		/// </summary>
		[ContextMenu("Test Custom Position")]
		public void TestCustomPosition()
		{
			// Example: Test with a specific position
			string customFen = "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1";  // 1.e4
			this.testFen = customFen;
			// StartCoroutine(TestPositionCoroutine(customFen));
		}

		IEnumerator TestPositionCoroutine(string fen)
		{
			if (!stockfish.IsEngineRunning)
			{
				Debug.LogError("[Test] Engine not running");
				yield break;
			}

			UpdateStatus($"Analyzing position: {fen}");

			// MODIFY: Pass weakness parameters to bridge
			yield return StartCoroutine(stockfish.GetNextMoveCoroutine(fen, moveTimeMs, searchDepth, engineElo, skillLevel));

			Debug.Log($"[Test] Custom position analysis:\n{stockfish.LastRawOutput}");
			UpdateOutput($"Custom Position Results:\n\n{stockfish.LastRawOutput}");
		}

		/// <summary>
		/// Test different time controls
		/// </summary>
		[ContextMenu("Test Time Controls")]
		public void TestTimeControls()
		{
			StartCoroutine(TestTimeControlsCoroutine());
		}

		IEnumerator TestTimeControlsCoroutine()
		{
			if (!stockfish.IsEngineRunning) yield break;

			int[] timeLimits = { 500, 1000, 3000 };  // Test different move times

			foreach (int timeMs in timeLimits)
			{
				UpdateStatus($"Testing {timeMs}ms analysis...");

				// MODIFY: Pass weakness parameters to bridge
				yield return StartCoroutine(stockfish.GetNextMoveCoroutine("startpos", timeMs, searchDepth, engineElo, skillLevel));

				Debug.Log($"[Test] {timeMs}ms analysis completed");
				yield return new WaitForSeconds(0.5f);  // Brief pause between tests
			}

			UpdateStatus("Time control tests completed");
		}

		// REMOVE: OnDisable method since we don't want to stop engine on disable

		// REMOVE: OnDestroy method since we don't want to stop engine on destroy

		// UI Helper methods
		private void UpdateStatus(string message)
		{
			if (statusText != null)
			{
				statusText.text = message;
			}
			Debug.Log($"[Test] Status: {message}");
		}

		private void UpdateOutput(string output)
		{
			if (outputText != null)
			{
				outputText.text = output;
			}
		}
	}
}