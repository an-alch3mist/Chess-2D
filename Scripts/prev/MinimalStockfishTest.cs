using System.Collections;
using UnityEngine;

using SPACE_UTIL;

namespace GPTDeepResearch
{
	/// <summary>
	/// Minimal test script to get raw Stockfish output from a FEN position
	/// Usage: Attach to any GameObject in empty scene, set FEN in inspector, play scene
	/// </summary>
	public class MinimalStockfishTest : MonoBehaviour
	{
		[TextArea(minLines: 10, maxLines: 20)]
		[SerializeField] string FEN = @"// Test these valid FEN positions instead:

// 1. Basic King vs King endgame
8/8/8/8/8/8/3k4/3K4 w - - 0 1

// 2. King and Rook vs King (legal endgame)
8/8/8/8/8/8/3k4/R2K4 w - - 0 1

// 3. Your original idea but with both kings present
k7/8/8/8/8/8/8/RR4K1 w - - 0 1

// 4. Starting position (always works)
rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1

// 5. Simple middle game position
r1bqkbnr/pppp1ppp/2n5/4p3/4P3/5N2/PPPP1PPP/RNBQKB1R w KQkq - 2 3";



		[Header("Test Input")]
		[SerializeField, Tooltip("FEN position to analyze, or 'startpos' for starting position")]
		private string testFen = "startpos";

		[SerializeField, Tooltip("Analysis time in milliseconds")]
		private int analysisTimeMs = 2000;

		[SerializeField, Tooltip("Search depth (overrides time if > 0)")]
		private int searchDepth = -1;

		[SerializeField, Tooltip("Engine Elo (-1 for max strength)")]
		private int engineElo = -1;

		[SerializeField, Tooltip("Skill level 0-20 (-1 disabled)")]
		private int skillLevel = -1;

		[Header("Quick Test FENs - Copy these to testFen field above")]
		[SerializeField, TextArea(5, 10)]
		private string quickTestFENs =
			"Safe test positions (copy to testFen above):\n\n" +
			"startpos\n\n" +
			"8/8/8/3k4/8/8/8/3K4 w - - 0 1\n" +
			"(King vs King endgame)\n\n" +
			"8/8/8/3k4/8/8/8/R2K4 w - - 0 1\n" +
			"(King + Rook vs King)\n\n" +
			"k7/8/8/8/8/8/7K/R7 w - - 0 1\n" +
			"(Your original idea - legal version)";

		[Header("Output")]
		[SerializeField, TextArea(10, 20)]
		private string rawOutput = "Analysis result will appear here...";

		private StockfishBridge stockfish;

		void Start()
		{
			Debug.Log("[MinimalTest] Starting minimal Stockfish test...");

			// Create StockfishBridge component
			stockfish = gameObject.AddComponent<StockfishBridge>();

			// Optional: Subscribe to real-time engine output
			stockfish.OnEngineLine.AddListener(OnEngineLineReceived);

			// Start the analysis process
			// StartCoroutine(RunAnalysis());
		}

		private void Update()
		{
			if(INPUT.M.InstantDown(0))
			{
				StopAllCoroutines();
				StartCoroutine(RunAnalysis());
			}
		}

		void OnDestroy()
		{
			// Clean shutdown
			if (stockfish != null)
			{
				stockfish.StopEngine();
			}
		}

		IEnumerator RunAnalysis()
		{
			Debug.Log("[MinimalTest] Initializing engine...");

			// Step 1: Start engine
			stockfish.StartEngine();

			// Step 2: Wait for engine to initialize
			yield return StartCoroutine(stockfish.InitializeEngineCoroutine());

			if (!stockfish.IsReady)
			{
				Debug.LogError("[MinimalTest] Engine failed to initialize!");
				rawOutput = "ERROR: Engine failed to initialize";
				yield break;
			}

			Debug.Log("[MinimalTest] Engine ready, starting analysis...");
			Debug.Log($"[MinimalTest] Testing FEN: {testFen}");

			// Step 3: Basic validation (but allow the test to continue)
			if (!IsBasicValidFEN(testFen))
			{
				Debug.LogWarning("[MinimalTest] FEN validation failed - but continuing test to see engine behavior");
			}

			// Step 4: Send ucinewgame to reset engine state
			Debug.Log("[MinimalTest] Sending ucinewgame to reset engine state...");
			stockfish.SendCommand("ucinewgame");
			yield return new WaitForSeconds(0.1f); // Brief pause

			// Check if engine is still running before analysis
			if (!stockfish.IsEngineRunning)
			{
				Debug.LogError("[MinimalTest] Engine not running before analysis");
				rawOutput = "ERROR: Engine died before analysis";
				yield break;
			}

			// Step 5: Get analysis with crash detection
			bool engineWasRunning = stockfish.IsEngineRunning;

			Debug.Log("[MinimalTest] Starting engine analysis...");
			yield return StartCoroutine(stockfish.GetNextMoveCoroutine(
				testFen,
				analysisTimeMs,
				searchDepth,
				engineElo,
				skillLevel
			));

			// Step 6: Check results and engine status
			if (engineWasRunning && !stockfish.IsEngineRunning)
			{
				Debug.LogError("[MinimalTest] Engine crashed during analysis!");
				rawOutput = $"ERROR: Engine crashed during analysis of FEN: {testFen}\n\n" +
						   "This could be due to:\n" +
						   "• A bug in this version of Stockfish\n" +
						   "• An edge case position that triggers engine issues\n" +
						   "• Memory/threading problems\n\n" +
						   "ASCII representation of your position:\n" +
						   "8  k . . . . . . .  (black king a8)\n" +
						   "7  . . . . . . . .\n" +
						   "6  . . . . . . . .\n" +
						   "5  . . . . . . . .\n" +
						   "4  . . . . . . . .\n" +
						   "3  . . . . . . . .\n" +
						   "2  . . . . . . . .\n" +
						   "1  R R . . . . K .  (rooks a1,b1, king g1)\n\n" +
						   "Position appears legal - this may be an engine bug.";

				// Attempt to restart engine
				Debug.Log("[MinimalTest] Attempting to restart engine...");
				yield return new WaitForSeconds(1f);
				stockfish.StartEngine();
				yield return StartCoroutine(stockfish.InitializeEngineCoroutine());
				yield break;
			}

			// Step 7: Display results
			rawOutput = stockfish.LastRawOutput;

			if (string.IsNullOrEmpty(rawOutput) || rawOutput.Contains("ERROR"))
			{
				Debug.LogError($"[MinimalTest] Analysis failed: {rawOutput}");
				if (string.IsNullOrEmpty(rawOutput))
				{
					rawOutput = "ERROR: Empty response from engine - possible timeout or crash";
				}
			}
			else
			{
				Debug.Log($"[MinimalTest] Analysis successful!\n{rawOutput}");
				ExtractBestMove(rawOutput);
			}
		}

		/// <summary>
		/// Optional: Real-time engine output (for debugging)
		/// </summary>
		void OnEngineLineReceived(string line)
		{
			Debug.Log($"[Engine] {line}");
		}

		/// <summary>
		/// Extract and log the best move from raw output
		/// </summary>
		void ExtractBestMove(string output)
		{
			if (string.IsNullOrEmpty(output)) return;

			string[] lines = output.Split('\n');
			foreach (string line in lines)
			{
				if (line.StartsWith("bestmove"))
				{
					Debug.Log($"[MinimalTest] Best move: {line}");
					break;
				}
			}
		}

		/// <summary>
		/// Call this from Inspector or code to run analysis again
		/// </summary>
		[ContextMenu("Run Analysis Again")]
		public void RunAnalysisAgain()
		{
			if (stockfish != null && stockfish.IsReady)
			{
				StartCoroutine(RunAnalysis());
			}
			else
			{
				Debug.LogWarning("[MinimalTest] Engine not ready!");
			}
		}

		// REPLACE: The IsBasicValidFEN method with more comprehensive validation
		/// <summary>
		/// Enhanced FEN validation - checks for common illegal position issues
		/// </summary>
		bool IsBasicValidFEN(string fen)
		{
			if (string.IsNullOrEmpty(fen) || fen == "startpos") return true;

			string[] parts = fen.Split(' ');
			if (parts.Length != 6)
			{
				Debug.LogError($"[MinimalTest] Invalid FEN - Wrong number of parts: {parts.Length}");
				return false;
			}

			string position = parts[0];
			string[] ranks = position.Split('/');
			if (ranks.Length != 8)
			{
				Debug.LogError($"[MinimalTest] Invalid FEN - Wrong number of ranks: {ranks.Length}");
				return false;
			}

			// Count kings and pieces, check for obvious issues
			int whiteKings = 0, blackKings = 0;
			int whitePieces = 0, blackPieces = 0;
			bool hasIllegalPawnPlacement = false;
			bool hasIllegalPieceCount = false;

			for (int rankIndex = 0; rankIndex < ranks.Length; rankIndex++)
			{
				string rank = ranks[rankIndex];
				int fileCount = 0;

				foreach (char c in rank)
				{
					if (char.IsDigit(c))
					{
						fileCount += int.Parse(c.ToString());
					}
					else
					{
						fileCount++;
						if (c == 'K') { whiteKings++; whitePieces++; }
						else if (c == 'k') { blackKings++; blackPieces++; }
						else if (char.IsUpper(c)) whitePieces++;
						else if (char.IsLower(c)) blackPieces++;

						// Check for pawns on back ranks (1st or 8th rank)
						if ((c == 'P' || c == 'p') && (rankIndex == 0 || rankIndex == 7))
						{
							hasIllegalPawnPlacement = true;
							Debug.LogError($"[MinimalTest] Invalid FEN - Pawn on back rank (rank {rankIndex + 1})");
						}
					}
				}

				// Each rank must have exactly 8 squares
				if (fileCount != 8)
				{
					Debug.LogError($"[MinimalTest] Invalid FEN - Rank {rankIndex + 1} has {fileCount} squares, expected 8");
					return false;
				}
			}

			// Must have exactly one king per side
			if (whiteKings != 1 || blackKings != 1)
			{
				Debug.LogError($"[MinimalTest] Invalid FEN - White Kings: {whiteKings}, Black Kings: {blackKings}");
				return false;
			}

			if (hasIllegalPawnPlacement)
			{
				return false;
			}

			// Check for excessive piece count (more than starting position)
			if (whitePieces > 16 || blackPieces > 16)
			{
				Debug.LogError($"[MinimalTest] Invalid FEN - Too many pieces: White={whitePieces}, Black={blackPieces}");
				return false;
			}

			// Check if kings are adjacent (also illegal)
			if (AreKingsAdjacent(position))
			{
				Debug.LogError("[MinimalTest] Invalid FEN - Kings are adjacent");
				return false;
			}

			// ADD: Check for check validation on side not to move
			if (IsKingInCheckIllegal(position, parts[1]))
			{
				Debug.LogError("[MinimalTest] Invalid FEN - King not to move is in check");
				return false;
			}

			return true;
		}

		// ADD: New method to check if the king not to move is in check (illegal position)
		/// <summary>
		/// Check if the king not to move is in check (which would be illegal)
		/// </summary>
		bool IsKingInCheckIllegal(string position, string activeColor)
		{
			// This is a simplified check - in a real chess program you'd want more thorough validation
			// For now, we'll just check some basic cases that commonly cause Stockfish crashes

			string[] ranks = position.Split('/');

			// Find both kings
			int whiteKingFile = -1, whiteKingRank = -1;
			int blackKingFile = -1, blackKingRank = -1;

			for (int rankIndex = 0; rankIndex < ranks.Length; rankIndex++)
			{
				string rank = ranks[rankIndex];
				int fileIndex = 0;

				foreach (char c in rank)
				{
					if (char.IsDigit(c))
					{
						fileIndex += int.Parse(c.ToString());
					}
					else
					{
						if (c == 'K')
						{
							whiteKingFile = fileIndex;
							whiteKingRank = rankIndex;
						}
						else if (c == 'k')
						{
							blackKingFile = fileIndex;
							blackKingRank = rankIndex;
						}
						fileIndex++;
					}
				}
			}

			// Basic check: if kings are on same file/rank/diagonal with no pieces between
			// and there are attacking pieces, this might be an illegal position
			if (whiteKingFile >= 0 && blackKingFile >= 0)
			{
				// Check for rooks/queens attacking the inactive king
				bool whiteToMove = activeColor.ToLower() == "w";
				int targetKingFile = whiteToMove ? blackKingFile : whiteKingFile;
				int targetKingRank = whiteToMove ? blackKingRank : whiteKingRank;

				// This is a very basic check - look for rooks on same rank/file
				// A full implementation would check all pieces and their attack patterns
				for (int rankIndex = 0; rankIndex < ranks.Length; rankIndex++)
				{
					string rank = ranks[rankIndex];
					int fileIndex = 0;

					foreach (char c in rank)
					{
						if (char.IsDigit(c))
						{
							fileIndex += int.Parse(c.ToString());
						}
						else
						{
							// Check if opposing rook/queen is attacking inactive king
							if ((whiteToMove && (c == 'R' || c == 'Q')) || (!whiteToMove && (c == 'r' || c == 'q')))
							{
								if ((rankIndex == targetKingRank || fileIndex == targetKingFile) &&
									IsLineClear(ranks, fileIndex, rankIndex, targetKingFile, targetKingRank))
								{
									return true; // Illegal position - inactive king in check
								}
							}
							fileIndex++;
						}
					}
				}
			}

			return false;
		}

		// ADD: Helper method to check if a line is clear between two squares
		/// <summary>
		/// Check if there are no pieces between two squares on the same rank, file, or diagonal
		/// </summary>
		bool IsLineClear(string[] ranks, int fromFile, int fromRank, int toFile, int toRank)
		{
			if (fromFile == toFile && fromRank == toRank) return false;

			int fileDiff = toFile - fromFile;
			int rankDiff = toRank - fromRank;

			// Only check horizontal, vertical, or diagonal lines
			if (fileDiff != 0 && rankDiff != 0 && Mathf.Abs(fileDiff) != Mathf.Abs(rankDiff))
				return false;

			int fileStep = fileDiff == 0 ? 0 : (fileDiff > 0 ? 1 : -1);
			int rankStep = rankDiff == 0 ? 0 : (rankDiff > 0 ? 1 : -1);

			int currentFile = fromFile + fileStep;
			int currentRank = fromRank + rankStep;

			while (currentFile != toFile || currentRank != toRank)
			{
				if (currentRank < 0 || currentRank >= 8) return false;

				string rank = ranks[currentRank];
				int fileIndex = 0;

				foreach (char c in rank)
				{
					if (char.IsDigit(c))
					{
						fileIndex += int.Parse(c.ToString());
					}
					else
					{
						if (fileIndex == currentFile)
						{
							return false; // Piece found in the way
						}
						fileIndex++;
					}
				}

				currentFile += fileStep;
				currentRank += rankStep;
			}

			return true;
		}

		/// <summary>
		/// Check if kings are adjacent to each other (illegal)
		/// </summary>
		bool AreKingsAdjacent(string position)
		{
			// Find king positions
			string[] ranks = position.Split('/');
			int whiteKingFile = -1, whiteKingRank = -1;
			int blackKingFile = -1, blackKingRank = -1;

			for (int rankIndex = 0; rankIndex < ranks.Length; rankIndex++)
			{
				string rank = ranks[rankIndex];
				int fileIndex = 0;

				foreach (char c in rank)
				{
					if (char.IsDigit(c))
					{
						fileIndex += int.Parse(c.ToString());
					}
					else
					{
						if (c == 'K')
						{
							whiteKingFile = fileIndex;
							whiteKingRank = rankIndex;
						}
						else if (c == 'k')
						{
							blackKingFile = fileIndex;
							blackKingRank = rankIndex;
						}
						fileIndex++;
					}
				}
			}

			// Check if kings are adjacent (distance <= 1 in both rank and file)
			if (whiteKingFile >= 0 && blackKingFile >= 0)
			{
				int fileDist = Mathf.Abs(whiteKingFile - blackKingFile);
				int rankDist = Mathf.Abs(whiteKingRank - blackKingRank);
				return fileDist <= 1 && rankDist <= 1;
			}

			return false;
		}
	}
}