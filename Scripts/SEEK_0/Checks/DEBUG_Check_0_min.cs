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

		IEnumerator STIMULATE()
		{
			#region frame_rate
			Application.targetFrameRate = 30;
			yield return null;
			#endregion

			yield return stockfishBridge.AnalyzePositionCoroutine("8/8/8/8/8/8/k1K4p/8 w - - 0 0");
			Debug.Log(stockfishBridge.LastAnalysisResult);
			
		}


		[SerializeField] private StockfishBridge stockfishBridge;
		//

	}
}
