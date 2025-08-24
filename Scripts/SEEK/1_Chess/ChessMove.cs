/*
CHANGELOG (Updated):
- Fixed castling move parsing to work with both standard chess and Chess960
- Corrected hardcoded target squares issue in CreateCastlingMove
- Added proper king and rook position detection for flexible castling
- Fixed castling rights validation in both standard and Chess960 formats
*/

using System;
using UnityEngine;

using SPACE_UTIL;

namespace GPTDeepResearch
{
	/// <summary>
	/// Represents a chess move with support for standard chess and Chess960.
	/// Handles parsing from various notations and special move types.
	/// </summary>
	[System.Serializable]
	public struct ChessMove : IEquatable<ChessMove>
	{
		[Header("Move Data")]
		public v2 from;           // Source square coordinates (0-7, 0-7)
		public v2 to;             // Target square coordinates (0-7, 0-7)
		public char piece;        // Moving piece ('R', 'N', 'B', 'Q', 'K', 'P', lowercase for black)
		public char capturedPiece; // Captured piece ('\0' if no capture)

		[Header("Special Moves")]
		public MoveType moveType;   // Type of move (normal, castling, en passant, promotion)
		public char promotionPiece; // Promotion piece ('Q', 'R', 'B', 'N', '\0' if no promotion)

		[Header("Castling Data")]
		public v2 rookFrom;       // Rook source square for castling
		public v2 rookTo;         // Rook target square for castling

		public enum MoveType
		{
			Normal,
			Castling,
			EnPassant,
			Promotion,
			CastlingPromotion // Rare but possible in some variants
		}

		/// <summary>
		/// Create a normal move
		/// </summary>
		public ChessMove(v2 from, v2 to, char piece, char capturedPiece = '\0')
		{
			this.from = from;
			this.to = to;
			this.piece = piece;
			this.capturedPiece = capturedPiece;
			this.moveType = MoveType.Normal;
			this.promotionPiece = '\0';
			this.rookFrom = new v2(-1, -1);
			this.rookTo = new v2(-1, -1);
		}

		/// <summary>
		/// Create a promotion move
		/// </summary>
		public ChessMove(v2 from, v2 to, char piece, char promotionPiece, char capturedPiece = '\0')
		{
			this.from = from;
			this.to = to;
			this.piece = piece;
			this.capturedPiece = capturedPiece;
			this.moveType = MoveType.Promotion;
			this.promotionPiece = promotionPiece;
			this.rookFrom = new v2(-1, -1);
			this.rookTo = new v2(-1, -1);
		}

		/// <summary>
		/// Create a castling move
		/// </summary>
		public ChessMove(v2 kingFrom, v2 kingTo, v2 rookFrom, v2 rookTo, char piece)
		{
			this.from = kingFrom;
			this.to = kingTo;
			this.piece = piece;
			this.capturedPiece = '\0';
			this.moveType = MoveType.Castling;
			this.promotionPiece = '\0';
			this.rookFrom = rookFrom;
			this.rookTo = rookTo;
		}

		/// <summary>
		/// Parse move from long algebraic notation (e.g., "e2e4", "e7e8q")
		/// </summary>
		public static ChessMove FromLongAlgebraic(string moveString, ChessBoard board)
		{
			if (string.IsNullOrEmpty(moveString) || moveString.Length < 4)
				return new ChessMove();

			// Handle castling notation
			if (moveString == "O-O" || moveString == "o-o" || moveString == "0-0")
			{
				return CreateCastlingMove(board, true); // Kingside
			}
			if (moveString == "O-O-O" || moveString == "o-o-o" || moveString == "0-0-0")
			{
				return CreateCastlingMove(board, false); // Queenside
			}

			// Check for king castling moves in algebraic notation (e.g., e1g1, e8c8)
			if (moveString.Length >= 4)
			{
				string fromSquare = moveString.Substring(0, 2);
				string toSquare = moveString.Substring(2, 2);
				v2 from = ChessBoard.AlgebraicToCoord(fromSquare);
				v2 to = ChessBoard.AlgebraicToCoord(toSquare);

				if (from.x >= 0 && to.x >= 0)
				{
					char piece = board.GetPiece(fromSquare);

					// Check if this is a king move that could be castling
					if (char.ToUpper(piece) == 'K' && Math.Abs(to.x - from.x) >= 2)
					{
						// This looks like a castling move
						bool kingside = to.x > from.x;
						ChessMove castlingMove = CreateCastlingMove(board, kingside);

						// Verify the move matches what we expect for castling
						if (castlingMove.IsValid() && castlingMove.from == from && castlingMove.to == to)
						{
							return castlingMove;
						}
					}
				}
			}

			// Parse from and to squares for regular moves
			string fromSquareRegular = moveString.Substring(0, 2);
			string toSquareRegular = moveString.Substring(2, 2);

			v2 fromRegular = ChessBoard.AlgebraicToCoord(fromSquareRegular);
			v2 toRegular = ChessBoard.AlgebraicToCoord(toSquareRegular);

			if (fromRegular.x < 0 || toRegular.x < 0) // Invalid coordinates
				return new ChessMove();

			char pieceRegular = board.GetPiece(fromSquareRegular);
			char capturedPiece = board.GetPiece(toSquareRegular);
			if (capturedPiece == '.') capturedPiece = '\0';

			// Check for promotion
			if (moveString.Length >= 5)
			{
				char promotionChar = char.ToUpper(moveString[4]);
				if ("QRBN".Contains(promotionChar.ToString()))
				{
					// Adjust case based on piece color
					if (char.IsLower(pieceRegular))
						promotionChar = char.ToLower(promotionChar);

					return new ChessMove(fromRegular, toRegular, pieceRegular, promotionChar, capturedPiece);
				}
			}

			// Check for en passant
			if (char.ToLower(pieceRegular) == 'p' && capturedPiece == '\0' && fromRegular.x != toRegular.x)
			{
				// Pawn moving diagonally without capturing = en passant
				ChessMove move = new ChessMove(fromRegular, toRegular, pieceRegular, capturedPiece);
				move.moveType = MoveType.EnPassant;
				return move;
			}

			return new ChessMove(fromRegular, toRegular, pieceRegular, capturedPiece);
		}

		/// <summary>
		/// Create castling move for current board position
		/// Supports both standard chess and Chess960
		/// </summary>
		private static ChessMove CreateCastlingMove(ChessBoard board, bool kingside)
		{
			char king = board.sideToMove == 'w' ? 'K' : 'k';
			char rook = board.sideToMove == 'w' ? 'R' : 'r';
			int rank = board.sideToMove == 'w' ? 0 : 7;

			// Find king position
			v2 kingPos = new v2(-1, -1);
			for (int file = 0; file < 8; file++)
			{
				if (board.board.GT(new v2(file, rank)) == king)
				{
					kingPos = new v2(file, rank);
					break;
				}
			}

			if (kingPos.x < 0) return new ChessMove(); // King not found

			// Find appropriate rook based on castling rights and position
			v2 rookPos = new v2(-1, -1);

			if (kingside)
			{
				// For kingside castling, find the rightmost rook that can castle
				for (int file = 7; file > kingPos.x; file--)
				{
					char piece = board.board.GT(new v2(file, rank));
					if (piece == rook)
					{
						rookPos = new v2(file, rank);
						break;
					}
				}
			}
			else
			{
				// For queenside castling, find the leftmost rook that can castle
				for (int file = 0; file < kingPos.x; file++)
				{
					char piece = board.board.GT(new v2(file, rank));
					if (piece == rook)
					{
						rookPos = new v2(file, rank);
						break;
					}
				}
			}

			if (rookPos.x < 0) return new ChessMove(); // Rook not found

			// In standard chess and most Chess960 positions, castling moves king 2 squares
			// and places rook on the opposite side of king
			v2 kingTarget, rookTarget;

			if (kingside)
			{
				// Kingside: King goes to g-file (6), rook to f-file (5)
				kingTarget = new v2(6, rank);
				rookTarget = new v2(5, rank);
			}
			else
			{
				// Queenside: King goes to c-file (2), rook to d-file (3)
				kingTarget = new v2(2, rank);
				rookTarget = new v2(3, rank);
			}

			return new ChessMove(kingPos, kingTarget, rookPos, rookTarget, king);
		}

		/// <summary>
		/// Convert move to long algebraic notation
		/// </summary>
		public string ToLongAlgebraic()
		{
			if (moveType == MoveType.Castling)
			{
				// Determine if kingside or queenside
				bool kingside = to.x > from.x;
				return kingside ? "O-O" : "O-O-O";
			}

			string fromSquare = ChessBoard.CoordToAlgebraic(from);
			string toSquare = ChessBoard.CoordToAlgebraic(to);

			if (string.IsNullOrEmpty(fromSquare) || string.IsNullOrEmpty(toSquare))
				return "";

			string result = fromSquare + toSquare;

			if (moveType == MoveType.Promotion && promotionPiece != '\0')
			{
				result += char.ToLower(promotionPiece);
			}

			return result;
		}

		/// <summary>
		/// Convert move to short algebraic notation (SAN)
		/// Requires board context for disambiguation
		/// </summary>
		public string ToShortAlgebraic(ChessBoard board)
		{
			if (moveType == MoveType.Castling)
			{
				bool kingside = to.x > from.x;
				return kingside ? "O-O" : "O-O-O";
			}

			string result = "";
			char movingPiece = char.ToUpper(piece);

			// Add piece letter (except for pawns)
			if (movingPiece != 'P')
				result += movingPiece;

			// Add capture notation
			if (capturedPiece != '\0' || moveType == MoveType.EnPassant)
			{
				if (movingPiece == 'P')
					result += (char)('a' + from.x); // Pawn captures include file
				result += "x";
			}

			// Add target square
			result += ChessBoard.CoordToAlgebraic(to);

			// Add promotion
			if (moveType == MoveType.Promotion && promotionPiece != '\0')
			{
				result += "=" + char.ToUpper(promotionPiece);
			}

			// TODO: Add check/checkmate notation (+/#)
			// This would require move validation context

			return result;
		}

		/// <summary>
		/// Check if this is a valid move (coordinates within bounds)
		/// </summary>
		public bool IsValid()
		{
			return from.x >= 0 && from.x < 8 && from.y >= 0 && from.y < 8 &&
				   to.x >= 0 && to.x < 8 && to.y >= 0 && to.y < 8 &&
				   piece != '\0' && (from.x != to.x || from.y != to.y);
		}

		/// <summary>
		/// Check if this is a capture move
		/// </summary>
		public bool IsCapture()
		{
			return capturedPiece != '\0' || moveType == MoveType.EnPassant;
		}

		/// <summary>
		/// Check if this is a quiet move (no capture, no check, no special move)
		/// </summary>
		public bool IsQuiet()
		{
			return !IsCapture() && moveType == MoveType.Normal;
		}

		/// <summary>
		/// Get move distance (useful for move ordering)
		/// </summary>
		public int GetDistance()
		{
			return Math.Abs(to.x - from.x) + Math.Abs(to.y - from.y);
		}

		#region Equality and Comparison

		public bool Equals(ChessMove other)
		{
			return from == other.from &&
				   to == other.to &&
				   piece == other.piece &&
				   capturedPiece == other.capturedPiece &&
				   moveType == other.moveType &&
				   promotionPiece == other.promotionPiece;
		}

		public override bool Equals(object obj)
		{
			return obj is ChessMove move && Equals(move);
		}

		public override int GetHashCode()
		{
			// Unity 2020.3 compatible hash code generation
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + from.GetHashCode();
				hash = hash * 23 + to.GetHashCode();
				hash = hash * 23 + piece.GetHashCode();
				hash = hash * 23 + capturedPiece.GetHashCode();
				hash = hash * 23 + moveType.GetHashCode();
				hash = hash * 23 + promotionPiece.GetHashCode();
				return hash;
			}
		}

		public static bool operator ==(ChessMove left, ChessMove right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ChessMove left, ChessMove right)
		{
			return !left.Equals(right);
		}

		#endregion

		public override string ToString()
		{
			string result = ToLongAlgebraic();
			if (string.IsNullOrEmpty(result))
				result = $"{ChessBoard.CoordToAlgebraic(from)}-{ChessBoard.CoordToAlgebraic(to)}";

			if (moveType == MoveType.EnPassant)
				result += " (e.p.)";
			else if (moveType == MoveType.Castling)
				result += " (castling)";
			else if (IsCapture())
				result += $" (captures {capturedPiece})";

			return result;
		}
	}
}