using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SPACE_UTIL;

namespace GPTDeepResearch
{
	public class DEBUG_Check_0 : MonoBehaviour
	{
		// Start is called before the first frame update
		void Start()
		{
			StartCoroutine(STIMULATE());
		}

		// Update is called once per frame
		void Update()
		{

		}

		IEnumerator STIMULATE()
		{
			// Basic engine startup and analysis
			StockfishBridge bridge = GetComponent<StockfishBridge>();
			bridge.StartEngine();
			yield return bridge.InitializeEngineCoroutine();

			// Analyze starting position
			yield return bridge.AnalyzePositionCoroutine("startpos");
			ChessAnalysisResult result = bridge.LastAnalysisResult;
			Debug.Log($"Best move: {result.bestMove}, Win probability: {result.whiteWinProbability:P1}");

			// Handle promotion moves
			if (result.isPromotion)
			{
				Debug.Log($"Promotion: {result.GetPromotionDescription()}");
				ChessMove move = result.ToChessMove(currentBoard);
			}
		}

	}

}