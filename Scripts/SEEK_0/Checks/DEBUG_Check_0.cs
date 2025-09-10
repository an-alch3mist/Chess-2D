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

			// this.ChessBoard_Check();
			this.ChessMove_Check();

			//
			yield return null;

		}

		// Checked ChessBoard.cs >>
		private void ChessBoard_Check()
		{
			// Initialize board with starting position
			var chessBoard = new ChessBoard();
			Debug.Log($"<color=white>Created board: {chessBoard.ToFEN()}</color>");

			// Load position from FEN
			var customBoard = new ChessBoard("r1bqkb1r/pppp1ppp/2n2n2/4p3/2B1P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 4 4");
			Debug.Log($"<color=white>Loaded custom position: {customBoard.ToFEN()}</color>");

			// Make moves with validation
			var e4Move = ChessMove.FromUCI("e2e4", chessBoard);
			bool moveSuccess = chessBoard.MakeMove(e4Move);
			Debug.Log($"<color=white>Move e2-e4 success: {moveSuccess}</color>");
			// Expected output: "Move e2-e4 success: True"

			// Get legal moves
			var legalMoves = chessBoard.GetLegalMoves();
			Debug.Log($"<color=white>Legal moves available: {legalMoves.Count}</color>");
			// Expected output: "Legal moves available: 20"

			// Update evaluation from engine
			chessBoard.UpdateEvaluation(15.0f, 0.52f, 0f, 12);
			Debug.Log($"<color=white>Evaluation: {chessBoard.LastEvaluation:F1}cp, WinProb: {chessBoard.LastWinProbability:F2}</color>");
			// Expected output: "Evaluation: 15.0cp, WinProb: 0.52"

			// Test undo/redo functionality
			if (chessBoard.CanUndo())
			{
				bool undoSuccess = chessBoard.UndoMove();
				Debug.Log($"<color=white>Undo success: {undoSuccess}</color>");
				// Expected output: "Undo success: True"

				if (chessBoard.CanRedo())
				{
					bool redoSuccess = chessBoard.RedoMove();
					Debug.Log($"<color=white>Redo success: {redoSuccess}</color>");
					// Expected output: "Redo success: True"
				}
			}

			// Position hashing and caching
			ulong posHash = chessBoard.CalculatePositionHash();
			Debug.Log($"<color=white>Position hash: {posHash:X}</color>");
			// Expected output: "Position hash: A1B2C3D4E5F6G7H8" (example hex)

			var cachedInfo = chessBoard.GetCachedPositionInfo();
			if (cachedInfo.HasValue)
			{
				Debug.Log($"<color=white>Cached evaluation: {cachedInfo.Value.evaluation:F1}</color>");
				// Expected output: "Cached evaluation: 15.0"
			}

			// Side management
			chessBoard.SetHumanSide('b');
			Debug.Log($"<color=white>Human side: {chessBoard.GetSideName(chessBoard.humanSide)}</color>");
			// Expected output: "Human side: Black"

			bool isHumanTurn = chessBoard.IsHumanTurn();
			bool isEngineTurn = chessBoard.IsEngineTurn();
			Debug.Log($"<color=white>Human turn: {isHumanTurn}, Engine turn: {isEngineTurn}</color>");
			// Expected output: "Human turn: False, Engine turn: True"

			// Piece access
			char piece = chessBoard.GetPiece("e4");
			Debug.Log($"<color=white>Piece at e4: {piece}</color>");
			// Expected output: "Piece at e4: P"

			// Algebraic notation conversion
			v2 coord = ChessBoard.AlgebraicToCoord("e4");
			string square = ChessBoard.CoordToAlgebraic(coord);
			Debug.Log($"<color=white>e4 coordinate: {coord}, back to algebraic: {square}</color>");
			// Expected output: "e4 coordinate: (4, 3), back to algebraic: e4"

			// Game result evaluation
			var gameResult = chessBoard.GetGameResult();
			Debug.Log($"<color=white>Game result: {gameResult}</color>");
			// Expected output: "Game result: InProgress"

			// Board cloning
			var clonedBoard = chessBoard.Clone();
			Debug.Log($"<color=white>Cloned board FEN: {clonedBoard.ToFEN()}</color>");
			// Expected output: "Cloned board FEN: rnbqkbnr/pppp1ppp/8/4p3/4P3/8/PPPP1PPP/RNBQKBNR w KQkq e6 0 2"

			// Test threefold repetition
			var repBoard = new ChessBoard();
			var moves = new string[] { "g1f3", "g8f6", "f3g1", "f6g8", "g1f3", "g8f6", "f3g1", "f6g8" };
			foreach (string uciMove in moves)
			{
				var move = ChessMove.FromUCI(uciMove, repBoard);
				repBoard.MakeMove(move);
			}
			bool isRepetition = repBoard.IsThreefoldRepetition();
			Debug.Log($"<color=white>Threefold repetition detected: {isRepetition}</color>");
			// Expected output: "Threefold repetition detected: True"

			// Chess960 variant
			var chess960Board = new ChessBoard("", ChessBoard.ChessVariant.Chess960);
			Debug.Log($"<color=white>Chess960 starting position: {chess960Board.ToFEN()}</color>");
			// Expected output: "Chess960 starting position: rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1" (or shuffled)

			// Nested type usage - PositionInfo
			var posInfo = new ChessBoard.PositionInfo(posHash, 25.5f, 0.6f, 15);
			bool isValidCache = posInfo.IsValid();
			Debug.Log($"<color=white>Position cache valid: {isValidCache}</color>");
			// Expected output: "Position cache valid: True"

			// Nested type usage - PGNMetadata
			var pgnMeta = new ChessBoard.PGNMetadata();
			pgnMeta.Event = "Test Game";
			pgnMeta.White = "Player1";
			pgnMeta.Black = "Player2";
			Debug.Log($"<color=white>PGN metadata: {pgnMeta.ToString()}</color>");
			// Expected output: "PGN metadata: PGN[Test Game at Unity Chess, 2024.01.15] Player1 vs Player2: *"

			// Run comprehensive tests
			ChessBoard.RunAllTests();
			Debug.Log("<color=white>ChessBoard test suite completed</color>");
			// Expected output: Multiple test result lines with color-coded pass/fail status
		}
		// << Checked ChessBoard.cs


		private void ChessMove_Check()
		{
			// Initialize test board
			ChessBoard board = new ChessBoard();

			// Create moves using different constructors
			v2 from = new v2(4, 1); // e2
			v2 to = new v2(4, 3);   // e4
			ChessMove pawnMove = new ChessMove(from, to, 'P');

			// Expected output: "Move created: e2e4"
			Debug.Log($"<color=white>Move created: {pawnMove.ToUCI()}</color>");

			// Parse UCI notation with caching
			ChessMove parsed = ChessMove.FromUCI("e2e4", board);
			if (parsed.IsValid())
			{
				// Expected output: "UCI parsed successfully: e2e4"
				Debug.Log($"<color=white>UCI parsed successfully: {parsed.ToString()}, {parsed.ToUCI()}</color>");
			}

			// Parse PGN notation
			ChessMove pgnMove = ChessMove.FromPGN("Nf3", board);
			if (pgnMove.IsValid())
			{
				// Expected output: "PGN parsed: Nf3 -> g1f3"
				Debug.Log($"<color=white>PGN parsed: Nf3 -> {pgnMove.ToUCI()}</color>");
			}

			// Create promotion move
			v2 promoFrom = new v2(4, 6); // e7
			v2 promoTo = new v2(4, 7);   // e8
			ChessMove promotion = ChessMove.CreatePromotionMove(promoFrom, promoTo, 'P', 'N');

			// Expected output: "Promotion: e7e8q"
			Debug.Log($"<color=white>Promotion: {promotion.ToUCI()}</color>");

			// Add analysis data
			ChessMove analyzed = promotion.WithAnalysisData(1500f, 12, 0.25f);
			ChessMove annotated = analyzed.WithAnnotation(ChessMove.Annotations.Good);

			// Expected output: "Analysis: Eval: +0.25, Depth: 12, Time: 1500ms"
			Debug.Log($"<color=white>Analysis: {annotated.GetAnalysisSummary()}</color>");

			// Test move properties
			bool isCapture = pawnMove.IsCapture();
			bool isQuiet = pawnMove.IsQuiet();
			int distance = pawnMove.GetDistance();

			// Expected output: "Move properties: Capture=False, Quiet=True, Distance=2"
			Debug.Log($"<color=white>Move properties: Capture={isCapture}, Quiet={isQuiet}, Distance={distance}</color>");

			// Test promotion utilities
			bool needsPromotion = ChessMove.RequiresPromotion(promoFrom, promoTo, 'P');
			char[] options = ChessMove.GetPromotionOptions(true);
			string pieceName = ChessMove.GetPromotionPieceName('R');

			// Expected output: "Promotion check: Needs=True, Options=4, Queen name=Queen"
			Debug.Log($"<color=white>Promotion check: Needs={needsPromotion}, Options={options.Length}, Queen name={pieceName}</color>");

			// Test equality
			ChessMove move1 = new ChessMove(from, to, 'P');
			ChessMove move2 = new ChessMove(from, to, 'P');
			bool areEqual = move1.Equals(move2);

			// Expected output: "Equality test: True"
			Debug.Log($"<color=white>Equality test: {areEqual}</color>");

			// Create castling move
			v2 kingFrom = new v2(4, 0);
			v2 kingTo = new v2(6, 0);
			v2 rookFrom = new v2(7, 0);
			v2 rookTo = new v2(5, 0);
			ChessMove castling = new ChessMove(kingFrom, kingTo, rookFrom, rookTo, 'K');

			// Expected output: "Castling move: e1g1"
			Debug.Log($"<color=white>Castling move: {castling.ToUCI()}</color>");

			// Run comprehensive tests
			ChessMove.RunAllTests();

			// Expected output: "ChessMove example completed successfully"
			Debug.Log("<color=cyan>ChessMove example completed successfully</color>");
		}
	}
}