/*
CHANGELOG (New File):
- Legal move generation for all piece types (pawn, rook, knight, bishop, queen, king)
- Chess960 castling support with flexible rook positions
- En passant move generation and validation
- Promotion move generation for pawns reaching last rank
- King safety checking (no moves into check allowed)
- Pin detection and handling (pinned pieces can only move along pin rays)
- Check detection and evasion move generation
- Efficient pseudo-legal move generation with legality filtering
*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SPACE_UTIL;

namespace GPTDeepResearch
{
	/// <summary>
	/// Generates legal chess moves for any position.
	/// Supports standard chess and Chess960 with full rule compliance.
	/// </summary>
	public static class MoveGenerator
	{
		// Direction vectors for piece movement
		private static readonly v2[] ROOK_DIRECTIONS = { (1, 0), (-1, 0), (0, 1), (0, -1) };
		private static readonly v2[] BISHOP_DIRECTIONS = { (1, 1), (1, -1), (-1, 1), (-1, -1) };
		private static readonly v2[] QUEEN_DIRECTIONS = { (1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (1, -1), (-1, 1), (-1, -1) };
		private static readonly v2[] KING_DIRECTIONS = { (1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (1, -1), (-1, 1), (-1, -1) };
		private static readonly v2[] KNIGHT_MOVES = { (2, 1), (2, -1), (-2, 1), (-2, -1), (1, 2), (1, -2), (-1, 2), (-1, -2) };

		/// <summary>
		/// Generate all legal moves for the current position
		/// </summary>
		public static List<ChessMove> GenerateLegalMoves(ChessBoard board)
		{
			List<ChessMove> pseudoLegalMoves = GeneratePseudoLegalMoves(board);
			List<ChessMove> legalMoves = new List<ChessMove>();

			// Filter out moves that leave king in check
			foreach (ChessMove move in pseudoLegalMoves)
			{
				if (IsLegalMove(board, move))
				{
					legalMoves.Add(move);
				}
			}

			return legalMoves;
		}

		/// <summary>
		/// Check if a move is legal (doesn't leave own king in check)
		/// </summary>
		public static bool IsLegalMove(ChessBoard board, ChessMove move)
		{
			// Make the move temporarily
			ChessBoard testBoard = board.Clone();
			if (!MakeMove(testBoard, move))
				return false;

			// Check if our king is in check after the move
			char ourKing = board.sideToMove == 'w' ? 'K' : 'k';
			v2 kingPos = FindKing(testBoard, ourKing);

			if (kingPos.x < 0) return false; // King not found (shouldn't happen)

			return !IsSquareAttacked(testBoard, kingPos, board.sideToMove == 'w' ? 'b' : 'w');
		}

		/// <summary>
		/// Generate all pseudo-legal moves (may leave king in check)
		/// </summary>
		private static List<ChessMove> GeneratePseudoLegalMoves(ChessBoard board)
		{
			List<ChessMove> moves = new List<ChessMove>();
			char sideToMove = board.sideToMove;

			// Generate moves for all pieces of the current side
			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					v2 pos = new v2(x, y);
					char piece = board.board.GT(pos);

					if (piece == '.' || !IsPieceColor(piece, sideToMove))
						continue;

					switch (char.ToUpper(piece))
					{
						case 'P':
							moves.AddRange(GeneratePawnMoves(board, pos));
							break;
						case 'R':
							moves.AddRange(GenerateRookMoves(board, pos));
							break;
						case 'N':
							moves.AddRange(GenerateKnightMoves(board, pos));
							break;
						case 'B':
							moves.AddRange(GenerateBishopMoves(board, pos));
							break;
						case 'Q':
							moves.AddRange(GenerateQueenMoves(board, pos));
							break;
						case 'K':
							moves.AddRange(GenerateKingMoves(board, pos));
							break;
					}
				}
			}

			// Add castling moves
			moves.AddRange(GenerateCastlingMoves(board));

			return moves;
		}

		/// <summary>
		/// Generate pawn moves including captures, en passant, and promotions
		/// </summary>
		private static List<ChessMove> GeneratePawnMoves(ChessBoard board, v2 from)
		{
			List<ChessMove> moves = new List<ChessMove>();
			char pawn = board.board.GT(from);
			bool isWhite = char.IsUpper(pawn);
			int direction = isWhite ? 1 : -1;
			int startRank = isWhite ? 1 : 6;
			int promotionRank = isWhite ? 7 : 0;

			// Forward move
			v2 oneForward = from + new v2(0, direction);
			if (IsInBounds(oneForward) && board.board.GT(oneForward) == '.')
			{
				if (oneForward.y == promotionRank)
				{
					// Promotion
					char[] promotionPieces = isWhite ? new char[] { 'Q', 'R', 'B', 'N' } : new char[] { 'q', 'r', 'b', 'n' };
					foreach (char promoPiece in promotionPieces)
					{
						moves.Add(new ChessMove(from, oneForward, pawn, promoPiece));
					}
				}
				else
				{
					moves.Add(new ChessMove(from, oneForward, pawn));
				}

				// Double pawn move from starting position
				if (from.y == startRank)
				{
					v2 twoForward = from + new v2(0, direction * 2);
					if (IsInBounds(twoForward) && board.board.GT(twoForward) == '.')
					{
						moves.Add(new ChessMove(from, twoForward, pawn));
					}
				}
			}

			// Captures
			v2[] captureDirections = { new v2(-1, direction), new v2(1, direction) };
			foreach (v2 captureDir in captureDirections)
			{
				v2 captureSquare = from + captureDir;
				if (!IsInBounds(captureSquare))
					continue;

				char targetPiece = board.board.GT(captureSquare);

				// Regular capture
				if (targetPiece != '.' && IsPieceColor(targetPiece, isWhite ? 'b' : 'w'))
				{
					if (captureSquare.y == promotionRank)
					{
						// Capture with promotion
						char[] promotionPieces = isWhite ? new char[] { 'Q', 'R', 'B', 'N' } : new char[] { 'q', 'r', 'b', 'n' };
						foreach (char promoPiece in promotionPieces)
						{
							moves.Add(new ChessMove(from, captureSquare, pawn, promoPiece, targetPiece));
						}
					}
					else
					{
						moves.Add(new ChessMove(from, captureSquare, pawn, targetPiece));
					}
				}

				// En passant capture
				if (targetPiece == '.' && board.enPassantSquare == ChessBoard.CoordToAlgebraic(captureSquare))
				{
					ChessMove enPassantMove = new ChessMove(from, captureSquare, pawn);
					enPassantMove.moveType = ChessMove.MoveType.EnPassant;
					enPassantMove.capturedPiece = isWhite ? 'p' : 'P'; // The captured pawn
					moves.Add(enPassantMove);
				}
			}

			return moves;
		}

		/// <summary>
		/// Generate rook moves (straight lines)
		/// </summary>
		private static List<ChessMove> GenerateRookMoves(ChessBoard board, v2 from)
		{
			return GenerateSlidingMoves(board, from, ROOK_DIRECTIONS);
		}

		/// <summary>
		/// Generate knight moves (L-shaped)
		/// </summary>
		private static List<ChessMove> GenerateKnightMoves(ChessBoard board, v2 from)
		{
			List<ChessMove> moves = new List<ChessMove>();
			char knight = board.board.GT(from);
			char sideToMove = board.sideToMove;

			foreach (v2 knightMove in KNIGHT_MOVES)
			{
				v2 to = from + knightMove;
				if (!IsInBounds(to))
					continue;

				char targetPiece = board.board.GT(to);

				// Empty square or opponent piece
				if (targetPiece == '.' || !IsPieceColor(targetPiece, sideToMove))
				{
					moves.Add(new ChessMove(from, to, knight, targetPiece == '.' ? '\0' : targetPiece));
				}
			}

			return moves;
		}

		/// <summary>
		/// Generate bishop moves (diagonal lines)
		/// </summary>
		private static List<ChessMove> GenerateBishopMoves(ChessBoard board, v2 from)
		{
			return GenerateSlidingMoves(board, from, BISHOP_DIRECTIONS);
		}

		/// <summary>
		/// Generate queen moves (combination of rook and bishop)
		/// </summary>
		private static List<ChessMove> GenerateQueenMoves(ChessBoard board, v2 from)
		{
			return GenerateSlidingMoves(board, from, QUEEN_DIRECTIONS);
		}

		/// <summary>
		/// Generate king moves (one square in any direction)
		/// </summary>
		private static List<ChessMove> GenerateKingMoves(ChessBoard board, v2 from)
		{
			List<ChessMove> moves = new List<ChessMove>();
			char king = board.board.GT(from);
			char sideToMove = board.sideToMove;

			foreach (v2 kingMove in KING_DIRECTIONS)
			{
				v2 to = from + kingMove;
				if (!IsInBounds(to))
					continue;

				char targetPiece = board.board.GT(to);

				// Empty square or opponent piece
				if (targetPiece == '.' || !IsPieceColor(targetPiece, sideToMove))
				{
					moves.Add(new ChessMove(from, to, king, targetPiece == '.' ? '\0' : targetPiece));
				}
			}

			return moves;
		}

		/// <summary>
		/// Generate sliding piece moves (rook, bishop, queen)
		/// </summary>
		private static List<ChessMove> GenerateSlidingMoves(ChessBoard board, v2 from, v2[] directions)
		{
			List<ChessMove> moves = new List<ChessMove>();
			char piece = board.board.GT(from);
			char sideToMove = board.sideToMove;

			foreach (v2 direction in directions)
			{
				v2 current = from + direction;

				while (IsInBounds(current))
				{
					char targetPiece = board.board.GT(current);

					if (targetPiece == '.')
					{
						// Empty square - can move here and continue
						moves.Add(new ChessMove(from, current, piece));
					}
					else if (!IsPieceColor(targetPiece, sideToMove))
					{
						// Opponent piece - can capture but cannot continue
						moves.Add(new ChessMove(from, current, piece, targetPiece));
						break;
					}
					else
					{
						// Own piece - cannot move here
						break;
					}

					current += direction;
				}
			}

			return moves;
		}

		/// <summary>
		/// Generate castling moves with Chess960 support
		/// </summary>
		private static List<ChessMove> GenerateCastlingMoves(ChessBoard board)
		{
			List<ChessMove> moves = new List<ChessMove>();
			char sideToMove = board.sideToMove;
			char king = sideToMove == 'w' ? 'K' : 'k';
			char rook = sideToMove == 'w' ? 'R' : 'r';
			int rank = sideToMove == 'w' ? 0 : 7;

			// Find king position
			v2 kingPos = FindKing(board, king);
			if (kingPos.x < 0) return moves; // King not found

			// Check if king is in check (cannot castle from check)
			if (IsSquareAttacked(board, kingPos, sideToMove == 'w' ? 'b' : 'w'))
				return moves;

			// Parse castling rights
			string rights = board.castlingRights;
			if (rights == "-") return moves;

			// Check kingside castling
			if (CanCastle(rights, sideToMove, true))
			{
				ChessMove kingsideCastle = GenerateCastlingMove(board, kingPos, true);
				if (kingsideCastle.IsValid() && IsCastlingLegal(board, kingsideCastle))
				{
					moves.Add(kingsideCastle);
				}
			}

			// Check queenside castling
			if (CanCastle(rights, sideToMove, false))
			{
				ChessMove queensideCastle = GenerateCastlingMove(board, kingPos, false);
				if (queensideCastle.IsValid() && IsCastlingLegal(board, queensideCastle))
				{
					moves.Add(queensideCastle);
				}
			}

			return moves;
		}

		/// <summary>
		/// Check if castling is allowed based on castling rights
		/// </summary>
		private static bool CanCastle(string rights, char sideToMove, bool kingside)
		{
			if (sideToMove == 'w')
			{
				return kingside ? rights.Contains('K') : rights.Contains('Q');
			}
			else
			{
				return kingside ? rights.Contains('k') : rights.Contains('q');
			}
		}

		// Key fixes needed in MoveGenerator.cs for the GenerateCastlingMove method:

		/// <summary>
		/// Generate castling move for given side
		/// </summary>
		private static ChessMove GenerateCastlingMove(ChessBoard board, v2 kingPos, bool kingside)
		{
			char sideToMove = board.sideToMove;
			char rook = sideToMove == 'w' ? 'R' : 'r';
			char king = sideToMove == 'w' ? 'K' : 'k';
			int rank = kingPos.y;

			// Find appropriate rook
			v2 rookPos = new v2(-1, -1);
			if (kingside)
			{
				// Find rightmost rook
				for (int file = 7; file > kingPos.x; file--)
				{
					if (board.board.GT(new v2(file, rank)) == rook)
					{
						rookPos = new v2(file, rank);
						break;
					}
				}
			}
			else
			{
				// Find leftmost rook
				for (int file = 0; file < kingPos.x; file++)
				{
					if (board.board.GT(new v2(file, rank)) == rook)
					{
						rookPos = new v2(file, rank);
						break;
					}
				}
			}

			if (rookPos.x < 0) return new ChessMove(); // Rook not found

			// Standard target squares for both regular chess and Chess960
			v2 kingTarget = new v2(kingside ? 6 : 2, rank);  // g1/g8 or c1/c8
			v2 rookTarget = new v2(kingside ? 5 : 3, rank);  // f1/f8 or d1/d8

			return new ChessMove(kingPos, kingTarget, rookPos, rookTarget, king);
		}

		/// <summary>
		/// Check if castling move is legal (path clear, no squares under attack)
		/// </summary>
		private static bool IsCastlingLegal(ChessBoard board, ChessMove castlingMove)
		{
			char opponent = board.sideToMove == 'w' ? 'b' : 'w';

			// Check if squares between king and target are clear and not attacked
			int minFile = Math.Min(castlingMove.from.x, castlingMove.to.x);
			int maxFile = Math.Max(castlingMove.from.x, castlingMove.to.x);

			for (int file = minFile; file <= maxFile; file++)
			{
				v2 square = new v2(file, castlingMove.from.y);

				// Skip king's starting position
				if (square == castlingMove.from) continue;

				// Check if square is occupied (except by king or rook involved in castling)
				char piece = board.board.GT(square);
				if (piece != '.' && square != castlingMove.rookFrom)
					return false;

				// Check if square is under attack
				if (IsSquareAttacked(board, square, opponent))
					return false;
			}

			// Check rook path (for Chess960 where rook might jump over pieces)
			int minRookFile = Math.Min(castlingMove.rookFrom.x, castlingMove.rookTo.x);
			int maxRookFile = Math.Max(castlingMove.rookFrom.x, castlingMove.rookTo.x);

			for (int file = minRookFile; file <= maxRookFile; file++)
			{
				v2 square = new v2(file, castlingMove.rookFrom.y);

				// Skip rook's starting position and king positions
				if (square == castlingMove.rookFrom || square == castlingMove.from || square == castlingMove.to)
					continue;

				// Check if square is occupied
				char piece = board.board.GT(square);
				if (piece != '.')
					return false;
			}

			return true;
		}

		/// <summary>
		/// Check if a square is attacked by the given side
		/// </summary>
		public static bool IsSquareAttacked(ChessBoard board, v2 square, char attackingSide)
		{
			// Check for pawn attacks
			if (IsSquareAttackedByPawn(board, square, attackingSide))
				return true;

			// Check for piece attacks
			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					v2 pos = new v2(x, y);
					char piece = board.board.GT(pos);

					if (piece == '.' || !IsPieceColor(piece, attackingSide))
						continue;

					if (DoesPieceAttackSquare(board, pos, square))
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Check if square is attacked by a pawn of the given side
		/// </summary>
		private static bool IsSquareAttackedByPawn(ChessBoard board, v2 square, char attackingSide)
		{
			bool isWhiteAttacking = attackingSide == 'w';
			int pawnRank = square.y + (isWhiteAttacking ? -1 : 1);
			char pawn = isWhiteAttacking ? 'P' : 'p';

			// Check both diagonal attack squares
			v2[] pawnAttacks = { new v2(square.x - 1, pawnRank), new v2(square.x + 1, pawnRank) };

			foreach (v2 pawnPos in pawnAttacks)
			{
				if (IsInBounds(pawnPos) && board.board.GT(pawnPos) == pawn)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Check if a piece at given position attacks the target square
		/// </summary>
		private static bool DoesPieceAttackSquare(ChessBoard board, v2 piecePos, v2 targetSquare)
		{
			char piece = board.board.GT(piecePos);

			switch (char.ToUpper(piece))
			{
				case 'R':
					return DoesRookAttackSquare(board, piecePos, targetSquare);
				case 'N':
					return DoesKnightAttackSquare(piecePos, targetSquare);
				case 'B':
					return DoesBishopAttackSquare(board, piecePos, targetSquare);
				case 'Q':
					return DoesQueenAttackSquare(board, piecePos, targetSquare);
				case 'K':
					return DoesKingAttackSquare(piecePos, targetSquare);
				default:
					return false;
			}
		}

		private static bool DoesRookAttackSquare(ChessBoard board, v2 rookPos, v2 targetSquare)
		{
			// Must be on same rank or file
			if (rookPos.x != targetSquare.x && rookPos.y != targetSquare.y)
				return false;

			// Check if path is clear
			return IsPathClear(board, rookPos, targetSquare);
		}

		private static bool DoesKnightAttackSquare(v2 knightPos, v2 targetSquare)
		{
			v2 diff = new v2(Math.Abs(targetSquare.x - knightPos.x), Math.Abs(targetSquare.y - knightPos.y));
			return (diff.x == 2 && diff.y == 1) || (diff.x == 1 && diff.y == 2);
		}

		private static bool DoesBishopAttackSquare(ChessBoard board, v2 bishopPos, v2 targetSquare)
		{
			// Must be on diagonal
			if (Math.Abs(targetSquare.x - bishopPos.x) != Math.Abs(targetSquare.y - bishopPos.y))
				return false;

			// Check if path is clear
			return IsPathClear(board, bishopPos, targetSquare);
		}

		private static bool DoesQueenAttackSquare(ChessBoard board, v2 queenPos, v2 targetSquare)
		{
			return DoesRookAttackSquare(board, queenPos, targetSquare) || DoesBishopAttackSquare(board, queenPos, targetSquare);
		}

		private static bool DoesKingAttackSquare(v2 kingPos, v2 targetSquare)
		{
			v2 diff = new v2(Math.Abs(targetSquare.x - kingPos.x), Math.Abs(targetSquare.y - kingPos.y));
			return diff.x <= 1 && diff.y <= 1 && (diff.x != 0 || diff.y != 0);
		}

		/// <summary>
		/// Check if path between two squares is clear (exclusive of endpoints)
		/// </summary>
		private static bool IsPathClear(ChessBoard board, v2 from, v2 to)
		{
			v2 direction = new v2(Math.Sign(to.x - from.x), Math.Sign(to.y - from.y));
			v2 current = from + direction;

			while (current != to)
			{
				if (board.board.GT(current) != '.')
					return false;
				current += direction;
			}

			return true;
		}

		/// <summary>
		/// Make a move on the board (for testing legality)
		/// </summary>
		private static bool MakeMove(ChessBoard board, ChessMove move)
		{
			if (!move.IsValid())
				return false;

			// Handle special moves
			switch (move.moveType)
			{
				case ChessMove.MoveType.Castling:
					// Move king
					board.board.ST(move.from, '.');
					board.board.ST(move.to, move.piece);
					// Move rook
					board.board.ST(move.rookFrom, '.');
					board.board.ST(move.rookTo, board.sideToMove == 'w' ? 'R' : 'r');
					break;

				case ChessMove.MoveType.EnPassant:
					// Move pawn
					board.board.ST(move.from, '.');
					board.board.ST(move.to, move.piece);
					// Remove captured pawn
					int capturedPawnRank = board.sideToMove == 'w' ? move.to.y - 1 : move.to.y + 1;
					board.board.ST(new v2(move.to.x, capturedPawnRank), '.');
					break;

				case ChessMove.MoveType.Promotion:
					// Remove pawn, place promoted piece
					board.board.ST(move.from, '.');
					board.board.ST(move.to, move.promotionPiece);
					break;

				default:
					// Regular move
					board.board.ST(move.from, '.');
					board.board.ST(move.to, move.piece);
					break;
			}

			// Switch side to move
			board.sideToMove = board.sideToMove == 'w' ? 'b' : 'w';

			return true;
		}

		/// <summary>
		/// Find king position for given side
		/// </summary>
		private static v2 FindKing(ChessBoard board, char king)
		{
			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					if (board.board.GT(new v2(x, y)) == king)
						return new v2(x, y);
				}
			}
			return new v2(-1, -1); // Not found
		}

		/// <summary>
		/// Check if piece belongs to given side
		/// </summary>
		private static bool IsPieceColor(char piece, char side)
		{
			if (side == 'w') return char.IsUpper(piece);
			else return char.IsLower(piece);
		}

		/// <summary>
		/// Check if coordinates are within board bounds
		/// </summary>
		private static bool IsInBounds(v2 pos)
		{
			return pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 8;
		}
	}
}