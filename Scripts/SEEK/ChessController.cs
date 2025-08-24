using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GPTDeepResearch;


// Basic setup in your script
public class ChessController : MonoBehaviour
{
	[SerializeField] private StockfishBridge bridge;

	void Start()
	{
		// Engine starts automatically in Awake()
		StartCoroutine(AnalyzePosition());
	}

	IEnumerator AnalyzePosition()
	{
		// Wait for engine to be ready
		yield return StartCoroutine(bridge.InitializeEngineCoroutine());

		// Analyze a position (using defaults from inspector)
		yield return StartCoroutine(bridge.AnalyzePositionCoroutine("startpos"));

		// Get results
		var result = bridge.LastAnalysisResult;
		Debug.Log($"Best move: {result.bestMove}");
		Debug.Log($"White win probability: {result.evaluation:F3}");
		Debug.Log($"Side-to-move probability: {result.stmEvaluation:F3}");

		// Custom analysis with specific parameters
		yield return StartCoroutine(bridge.AnalyzePositionCoroutine(
			"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
			movetimeMs: 3000,  // 3 second time limit
			depth: 8,          // Or specific depth
			elo: 1200,         // Engine strength
			skillLevel: 5      // Skill level 0-20
		));
	}
}