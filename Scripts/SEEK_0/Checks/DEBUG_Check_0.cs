using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
			// this.SimpleVerification_ChessMove_Check();
			// this.ChessMove_Check();
			// this.MoveGenerator_Check();
			// this.ChessRules_Check();

			// TODO: PromotionUI functionality, and integrate everything with StockfishEngine on Chess-2D Visual
			this.PromotionUI_Check();
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

		// CHecked ChessMove.cs >>
		private void SimpleVerification_ChessMove_Check()
		{
			// After e4, Nf3 manually check the position
			ChessBoard testBoard = new ChessBoard();
			testBoard.MakeMove(ChessMove.FromUCI("e2e4", testBoard));
			Debug.Log($"Position after e2e4: {testBoard.ToFEN()}");

			testBoard.MakeMove(ChessMove.FromPGN("e5", testBoard)); // black turn
			Debug.Log($"Position after e2e4: {testBoard.ToFEN()}");

			testBoard.MakeMove(ChessMove.FromUCI("f1c4", testBoard));
			Debug.Log($"Position after f1c4: {testBoard.ToFEN()}");

			testBoard.MakeMove(ChessMove.FromPGN("Qh4", testBoard)); // black turn
			Debug.Log($"Position after Qh4: {testBoard.ToFEN()}");
			return;
		}
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
		// << Checked ChessMove.cs

		// Checked MoveGenerator, when num of king on w, h side != 1 -> TODO
		private void MoveGenerator_Check()
		{
			// Initialize starting position
			ChessBoard board = new ChessBoard();

			// Generate all legal moves
			List<ChessMove> moves = MoveGenerator.GenerateLegalMoves(board);

			LOG.SaveLog(moves.ToTable(name: "LIST<moves>", toString: true));
			// Expected output: "Generated 20 legal moves from starting position"
			Debug.Log($"<color=white>Generated {moves.Count} legal moves from starting position</color>");


			// Check specific move legality
			ChessMove testMove = ChessMove.FromUCI("e2e4", board);
			bool isLegal = MoveGenerator.IsLegalMove(board, testMove);
			// Expected output: "Move e2-e4 is legal: True"
			Debug.Log($"<color=white>Move e2-e4 is legal: {isLegal}</color>");

			board.MakeMove(testMove);
			board.MakeMove(ChessMove.FromPGN("d5", board));		// black turn
			board.MakeMove(ChessMove.FromUCI("e4d5", board));
			board.MakeMove(ChessMove.FromPGN("Qxd5", board));	// black turn
			board.MakeMove(ChessMove.FromPGN("a3", board));
			board.MakeMove(ChessMove.FromPGN("Qe5", board));	// black turn
			// board.MakeMove(ChessMove.FromPGN("Qe2", board));
			Debug.Log(board.ToFEN());

			// Test square attack detection
			v2 kingSquare = new v2(4, 0); // e1
			bool kingAttacked = MoveGenerator.IsSquareAttacked(board, kingSquare, 'b');

			// Expected output: "King square e1 under attack: False"
			Debug.Log($"<color=white>King square e1 under attack: {kingAttacked}</color>");

			// Test promotion scenario
			ChessBoard promoBoard = new ChessBoard("8/P7/8/8/8/8/8/k6K w - - 0 1");
			List<ChessMove> promoMoves = MoveGenerator.GenerateLegalMoves(promoBoard);
			// LOG.SaveLog(promoMoves.ToTable(name: "LIST<promoMoves>"));
			int promotionCount = promoMoves.Count(m => m.moveType == ChessMove.MoveType.Promotion);

			// Expected output: "Promotion moves available: 4"
			Debug.Log($"<color=white>Promotion moves available: {promotionCount}</color>");

			// Test castling availability
			ChessBoard castleBoard = new ChessBoard("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");
			List<ChessMove> castleMoves = MoveGenerator.GenerateLegalMoves(castleBoard);
			// LOG.SaveLog(castleMoves.ToTable(name: "LIST<castleMoves>"));
			int castlingCount = castleMoves.Count(m => m.moveType == ChessMove.MoveType.Castling);

			// Expected output: "Castling moves available: 2"
			Debug.Log($"<color=white>Castling moves available: {castlingCount}</color>");

			// Test en passant
			ChessBoard epBoard = new ChessBoard("8/8/8/pP6/8/8/8/k6K w - a6 0 1");
			List<ChessMove> epMoves = MoveGenerator.GenerateLegalMoves(epBoard);
			LOG.SaveLog(epMoves.ToTable(name: "LIST<enPassantMoves>"));
			int enPassantCount = epMoves.Count(m => m.moveType == ChessMove.MoveType.EnPassant);

			// Expected output: "En passant moves available: 1"
			Debug.Log($"<color=white>En passant moves available: {enPassantCount}</color>");
			// Expected output: Various colored test results showing pass/fail status
			Debug.Log("<color=white>==== Move generation testing completed ====</color>");

			// Run comprehensive tests
			Debug.Log("MoveGenerator.RunAllTests() for MoveGenerator static class");
			MoveGenerator.RunAllTests();
		}
		// << Checked MoveGenerator

		// Checked ChessRules, when Three folds check, is'nt working
		private void ChessRules_Check()
		{
			// Create test board
			ChessBoard board = new ChessBoard();
			Debug.Log(board.ToFEN());
			// Test position evaluation
			ChessRules.GameResult result = ChessRules.EvaluatePosition(board);
			Debug.Log($"<color=white>Game state: {result}</color>");
			// Expected output: "Game state: InProgress"

			// Test check detection
			bool whiteInCheck = ChessRules.IsInCheck(board, 'w');
			bool blackInCheck = ChessRules.IsInCheck(board, 'b');
			Debug.Log($"<color=white>White in check: {whiteInCheck}, Black in check: {blackInCheck}</color>");
			// Expected output: "White in check: False, Black in check: False"

			// Test move validation
			ChessMove pawnMove = ChessMove.FromUCI("e2e4", board);
			bool isValid = ChessRules.ValidateMove(board, pawnMove);
			Debug.Log($"<color=white>Move e2e4 valid: {isValid}</color>");
			// Expected output: "Move e2e4 valid: True"

			// Test promotion requirement
			ChessBoard promotionBoard = new ChessBoard("8/P7/8/8/8/8/8/K6k w - - 0 1");
			ChessMove promotionMove = ChessMove.FromUCI("a7a8q", promotionBoard);
			bool needsPromotion = ChessRules.RequiresPromotion(promotionBoard, promotionMove);
			Debug.Log($"<color=white>Needs promotion: {needsPromotion}</color>");
			// Expected output: "Needs promotion: True"

			// Test move application
			ChessBoard testBoard = board.Clone();
			bool moveApplied = ChessRules.MakeMove(testBoard, pawnMove);
			Debug.Log($"<color=white>Move applied: {moveApplied}</color>");
			// Expected output: "Move applied: True"

			// Test evaluation info
			ChessRules.EvaluationInfo evalInfo = ChessRules.GetEvaluationInfo(
				board: board, 
				centipawns: 25.5f, 
				winProbability: 0.6f, 
				mateDistance: 0f);
			string displayText = evalInfo.GetDisplayText();
			Debug.Log($"<color=white>Evaluation: {displayText}</color>");
			// Expected output: "Evaluation: +25.50"

			// Test position validation
			bool positionValid = ChessRules.ValidatePosition(board);
			Debug.Log($"<color=white>Position valid: {positionValid}</color>");
			// Expected output: "Position valid: True"

			// Test king finding
			v2 whiteKing = ChessRules.FindKing(board, 'K');
			v2 blackKing = ChessRules.FindKing(board, 'k');
			Debug.Log($"<color=white>White king at: {whiteKing}, Black king at: {blackKing}</color>");
			// Expected output: "White king at: (4, 0), Black king at: (4, 7)"

			// Test attacking pieces
			List<v2> attackers = ChessRules.GetAttackingPieces(board, new v2(4, 4), 'w');
			Debug.Log($"<color=white>White pieces attacking e4: {attackers.Count}</color>");
			// Expected output: "White pieces attacking e4: 0"

			// Test stalemate detection
			ChessBoard stalemateBoard = new ChessBoard("7k/5Q2/6K1/8/8/8/8/8 b - - 0 1");
			ChessRules.GameResult stalemateResult = ChessRules.EvaluatePosition(stalemateBoard);
			Debug.Log($"<color=white>Stalemate result: {stalemateResult}</color>");
			// Expected output: "Stalemate result: Stalemate"

			// Test insufficient material
			ChessBoard insufficientBoard = new ChessBoard("8/8/8/8/8/8/8/K6k w - - 0 1");
			ChessRules.GameResult insufficientResult = ChessRules.EvaluatePosition(insufficientBoard);
			Debug.Log($"<color=white>Insufficient material result: {insufficientResult}</color>");
			// Expected output: "Insufficient material result: InsufficientMaterial"

			// Run comprehensive tests
			ChessRules.RunAllTests();
			// Expected output: Multiple test results with colored pass/fail indicators
		}
		// << Checked ChessRules


		[SerializeField] private PromotionUI promotionUI; // Assign in Inspector

		private IEnumerator PromotionUI_Check()
		{
			// Setup event handlers
			promotionUI.OnPromotionSelected += HandlePromotion;
			promotionUI.OnPromotionSelectedWithData += HandlePromotionData;

			// Test human selection dialog
			promotionUI.ShowPromotionDialog(true, "e7", "e8");
			yield return new WaitUntil(() => !promotionUI.IsWaitingForPromotion());

			// Test engine auto-selection
			promotionUI.SelectPromotionAutomatically('Q', false);

			// Test context and state
			var context = promotionUI.GetPromotionContext();
			bool waiting = promotionUI.IsWaitingForPromotion();
			bool engine = promotionUI.IsEngineSelection();

			// Test data object
			var data = new PromotionSelectionData('R', true, "a7", "a8");
			string description = data.ToString();

			Debug.Log($"API Results: Context={context}, Waiting={waiting}, Engine={engine}, Data={description}");
			yield break;
		}

		private void HandlePromotion(char piece) { }
		private void HandlePromotionData(PromotionSelectionData data) { }

	}
}