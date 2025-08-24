using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SPACE_UTIL;

namespace GPTDeepResearch
{
	// Basic setup in your script
	public class ChessController : MonoBehaviour
	{
		[SerializeField] private StockfishBridge bridge;

		void Start()
		{
			Debug.Log("engine running: " + this.bridge.IsEngineRunning);
		}

		private void Update()
		{
			if (INPUT.M.InstantDown(0))
			{
				StopAllCoroutines();
				StartCoroutine(AnalyzePosition());
			}
		}

		IEnumerator AnalyzePosition()
		{
			// Use inspector defaults (search: 1, eval: 5)
			yield return StartCoroutine(bridge.AnalyzePositionCoroutine("startpos"));

			// Custom depths
			yield return bridge.AnalyzePositionCoroutine(
				fen: "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
			);

			// Check results
			var result = bridge.LastAnalysisResult;

			LOG.SaveLog(result.ToString());

			Debug.Log($"Best move: {result.bestMove}");
			Debug.Log($"Search depth: {result.searchDepth}, Eval depth: {result.evaluationDepth}");
			Debug.Log($"White win probability: {result.evaluation:P1}");
		}
	}
}