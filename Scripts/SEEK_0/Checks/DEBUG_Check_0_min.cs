using UnityEngine;
using System.Collections;

using SPACE_UTIL;
using GPTDeepResearch;

namespace GPTDeepResearch
{

	public class DEBUG_Check_0_min : MonoBehaviour
	{
		//
		private void Update()
		{
			if (INPUT.M.InstantDown(0))
			{
				StopAllCoroutines();
				StartCoroutine(STIMULATE());
			}
		}

		[SerializeField] private StockfishBridge stockfishBridge;
		IEnumerator STIMULATE()
		{
			#region frame_rate
			Application.targetFrameRate = 30;
			yield return null;
			#endregion

			yield return stockfishBridge.AnalyzePositionCoroutine("8/8/8/8/8/8/k1K4p/8 b - - 0 0");
			Debug.Log(stockfishBridge.LastAnalysisResult);

			yield return this.StockfishBridge_Check();
			this.stockfishBridge.RunAllTests();
		}


		private IEnumerator StockfishBridge_Check()
		{
			// Wait for engine to be ready
			yield return stockfishBridge.InitializeEngineCoroutine();

			// Test basic analysis
			yield return stockfishBridge.AnalyzePositionCoroutine("startpos");
			var basicResult = stockfishBridge.LastAnalysisResult;

			// Test advanced analysis with custom settings
			string FEN = "rnbqkbnr/8/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1";
			yield return stockfishBridge.AnalyzePositionCoroutine(
				FEN,
				movetimeMs: -1, searchDepth: 12, evaluationDepth: 12, elo: 700, skillLevel: 10);
			var advancedResult = stockfishBridge.LastAnalysisResult;


			// Test engine management
			var engineRunning = stockfishBridge.IsEngineRunning;
			var engineReady = stockfishBridge.IsReady;
			var crashed = stockfishBridge.DetectAndHandleCrash();

			// Test nested class APIs
			var promotionDesc = advancedResult.GetPromotionDescription();
			var evalDisplay = advancedResult.GetEvaluationDisplay();
			advancedResult.ParsePromotionData();

			Debug.Log($@" API Results: 
BasicMove on Chess960 :{basicResult.bestMove} 
Move for {FEN} :{advancedResult.bestMove} 
EngineStatus/Ready/Crashed:{engineRunning},{engineReady}, {crashed} 
Promotion:{promotionDesc} 
Eval:{evalDisplay} 
==== Methods called, Coroutines completed ==== ");
			yield break;
		}

	}

}


