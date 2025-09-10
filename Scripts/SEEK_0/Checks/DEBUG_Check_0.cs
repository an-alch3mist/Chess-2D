using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SPACE_UTIL;

namespace GPTDeepResearch
{
	public class DEBUG_Check_0 : MonoBehaviour
	{
		private void Update()
		{
			if(INPUT.M.InstantDown(0))
			{
				StopAllCoroutines();
				StartCoroutine(STIMULATE());
			}
		}

		IEnumerator STIMULATE()
		{
			//
			Debug.Log("Started STIMULATE()");

			this.ChessBoard_Check();
			//
			yield return null;

		}

		private void ChessBoard_Check()
		{
			// Basic board creation and setup
			var board = new ChessBoard();
			Debug.Log($"<color=white>Created board with FEN: {board.ToFEN()}</color>");
			// Expected output: "Created board with FEN: rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"

			// Load custom position
			bool loadSuccess = board.LoadFromFEN("r6k/8/8/8/8/8/8/R3K2R b KQkq - 0 1");
			Debug.Log($"<color=white>Load custom FEN success: {loadSuccess}</color>");
			// Expected output: "Load custom FEN success: True"

			// Set player sides
			board.SetHumanSide('w');
			Debug.Log($"<color=white>Human: {board.GetSideName(board.humanSide)}, Engine: {board.GetSideName(board.engineSide)}</color>");
			// Expected output: "Human: White, Engine: Black"

			// Get piece information
			char piece = board.GetPiece("e1");
			Debug.Log($"<color=white>Piece at e1: {piece}</color>");
			// Expected output: "Piece at e1: K"

			// Coordinate conversion
			v2 coord = ChessBoard.AlgebraicToCoord("e4");
			string square = ChessBoard.CoordToAlgebraic(new v2(4, 3));
			Debug.Log($"<color=white>e4 -> {coord}, (4,3) -> {square}</color>");
			// Expected output: "e4 -> (4, 3), (4,3) -> e4"

			// Generate legal moves
			var legalMoves = board.GetLegalMoves();
			LOG.SaveLog(legalMoves.ToTable(name: $"sideToMove: {board.sideToMove} LIST<ChessMove>", toString: true));
			Debug.Log($"<color=white>Legal moves available: {legalMoves.Count}</color>");
			// Expected output: "Legal moves available: [varies by position]"

			// Make a move
			if (legalMoves.Count > 0)
			{
				bool moveSuccess = board.MakeMove(legalMoves[0], "Opening move");
				Debug.Log($"<color=white>Move made successfully: {moveSuccess}</color>");
				// Expected output: "Move made successfully: True"
			}

			// Update evaluation
			board.UpdateEvaluation(25.5f, 0.55f, 0f, 10);
			Debug.Log($"<color=white>Evaluation: {board.LastEvaluation}cp, Win prob: {board.LastWinProbability:F2}</color>");
			// Expected output: "Evaluation: 25.5cp, Win prob: 0.55"

			// Test undo/redo functionality
			bool canUndo = board.CanUndo();
			if (canUndo)
			{
				bool undoSuccess = board.UndoMove();
				Debug.Log($"<color=white>Undo successful: {undoSuccess}</color>");
				// Expected output: "Undo successful: True"

				bool canRedo = board.CanRedo();
				if (canRedo)
				{
					bool redoSuccess = board.RedoMove();
					Debug.Log($"<color=white>Redo successful: {redoSuccess}</color>");
					// Expected output: "Redo successful: True"
				}
			}

			// Position hashing
			ulong posHash = board.CalculatePositionHash();
			Debug.Log($"<color=white>Position hash: {posHash:X}</color>");
			// Expected output: "Position hash: [hexadecimal hash]"

			// Cache information
			var cachedInfo = board.GetCachedPositionInfo();
			if (cachedInfo.HasValue)
			{
				Debug.Log($"<color=white>Cached evaluation: {cachedInfo.Value.evaluation}</color>");
				// Expected output: "Cached evaluation: 25.5"
			}

			// Game tree information
			Debug.Log($"<color=white>Game tree nodes: {board.GameTreeNodeCount}, Current index: {board.CurrentHistoryIndex}</color>");
			// Expected output: "Game tree nodes: [count], Current index: [index]"

			// Check game result
			var gameResult = board.GetGameResult();
			Debug.Log($"<color=white>Game result: {gameResult}</color>");
			// Expected output: "Game result: InProgress"

			// Board cloning
			var clonedBoard = board.Clone();
			bool samePosition = clonedBoard.ToFEN() == board.ToFEN();
			Debug.Log($"<color=white>Clone has same position: {samePosition}</color>");
			// Expected output: "Clone has same position: True"

			// Variant support
			var chess960Board = new ChessBoard("", ChessBoard.ChessVariant.Chess960);
			Debug.Log($"<color=white>Chess960 variant created with FEN: {chess960Board.ToFEN().Substring(0, 40)}...</color>");
			// Expected output: "Chess960 variant created with FEN: [shuffled back rank]..."

			// Nested class usage - PGN metadata
			var metadata = new ChessBoard.PGNMetadata();
			metadata.Event = "Test Game";
			metadata.White = "Human";
			metadata.Black = "Engine";
			bool metadataValid = metadata.IsValid();
			Debug.Log($"<color=white>PGN metadata valid: {metadataValid}</color>");
			// Expected output: "PGN metadata valid: True"

			// Position validation
			var posInfo = new ChessBoard.PositionInfo(12345UL, 50f, 0.6f, 8);
			bool posValid = posInfo.IsValid();
			Debug.Log($"<color=white>Position info valid: {posValid}</color>");
			// Expected output: "Position info valid: True"

			// Run comprehensive tests
			Debug.Log($"<color=cyan>{'-'.repeat(24)}</color>");
			// Debug.Log("<color=cyan>Running comprehensive API tests...</color>");
			// ChessBoard.RunAllTests();
			// Expected output: Multiple test result logs with color-coded pass/fail indicators
		}
	}

}