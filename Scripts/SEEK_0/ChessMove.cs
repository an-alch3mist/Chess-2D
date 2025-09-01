/*
CHANGELOG (Enhanced Version):
- Fixed Unity 2020.3 compatibility: replaced string.Contains(char) with IndexOf
- Enhanced promotion move parsing with proper UCI format validation
- Added comprehensive promotion move creation methods
- Improved castling detection and handling for Chess960
- Added validation for promotion requirements
- Enhanced error handling and edge case management
- Added promotion piece validation and case handling
- Fixed string.Contains(char) compatibility issue for Unity 2020.3
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
		/// Parse move from UCI format (e.g., "e2e4", "e7e8q")
		/// Supports promotion notation with trailing piece letter
		/// Enhanced with better validation and Chess960 support
		/// FIXED: Unity 2020.3 compatibility for string operations
		/// </summary>
		public static ChessMove FromUCI(string uciMove, ChessBoard board)
		{
			if (string.IsNullOrEmpty(uciMove) || uciMove.Length < 4)
				return Invalid();

			// Handle special bestmove responses from engine
			if (uciMove == "0000" || uciMove == "(none)" || uciMove == "null")
				return Invalid();

			// Handle castling notation variants
			if (uciMove == "O-O" || uciMove == "o-o" || uciMove == "0-0")
			{
				return CreateCastlingMove(board, true); // Kingside
			}
			if (uciMove == "O-O-O" || uciMove == "o-o-o" || uciMove == "0-0-0")
			{
				return CreateCastlingMove(board, false); // Queenside
			}

			// Parse basic move components
			string fromSquare = uciMove.Substring(0, 2);
			string toSquare = uciMove.Substring(2, 2);
			v2 from = ChessBoard.AlgebraicToCoord(fromSquare);
			v2 to = ChessBoard.AlgebraicToCoord(toSquare);

			if (from.x < 0 || to.x < 0) // Invalid coordinates
			{
				Debug.Log($"<color=red>[ChessMove] Invalid coordinates in UCI move: {uciMove}</color>");
				return Invalid();
			}

			char piece = board.GetPiece(fromSquare);
			char capturedPiece = board.GetPiece(toSquare);
			if (capturedPiece == '.') capturedPiece = '\0';

			// Check for promotion (UCI format: e7e8q)
			if (uciMove.Length >= 5)
			{
				char promotionChar = uciMove[4];
				// FIXED: Use IndexOf instead of Contains for Unity 2020.3 compatibility
				if ("QRBNqrbn".IndexOf(promotionChar) >= 0)
				{
					// Validate promotion is legal
					if (!RequiresPromotion(from, to, piece))
					{
						Debug.Log($"<color=yellow>[ChessMove] Invalid promotion move: {uciMove} - piece {piece} not on promotion rank</color>");
						return Invalid();
					}

					// Adjust case based on piece color
					if (char.IsLower(piece))
						promotionChar = char.ToLower(promotionChar);
					else
						promotionChar = char.ToUpper(promotionChar);

					Debug.Log($"<color=green>[ChessMove] Parsed promotion move: {uciMove} -> {promotionChar}</color>");
					return new ChessMove(from, to, piece, promotionChar, capturedPiece);
				}
			}

			// Auto-detect promotion for pawn moves to last rank
			if (RequiresPromotion(from, to, piece))
			{
				// Default to queen promotion if no piece specified
				char defaultPromotion = char.IsLower(piece) ? 'q' : 'Q';
				Debug.Log($"<color=yellow>[ChessMove] Auto-promoting to queen: {uciMove}</color>");
				return new ChessMove(from, to, piece, defaultPromotion, capturedPiece);
			}

			// Check if this is a king move that could be castling
			if (char.ToUpper(piece) == 'K' && Math.Abs(to.x - from.x) >= 2)
			{
				// This looks like a castling move
				bool kingside = to.x > from.x;
				ChessMove castlingMove = CreateCastlingMove(board, kingside);

				// Verify the move matches what we expect for castling
				if (castlingMove.IsValid() && castlingMove.from == from && castlingMove.to == to)
				{
					Debug.Log($"<color=green>[ChessMove] Parsed castling move: {uciMove}</color>");
					return castlingMove;
				}
			}

			// Check for en passant
			if (char.ToLower(piece) == 'p' && capturedPiece == '\0' && from.x != to.x)
			{
				// Pawn moving diagonally without capturing = en passant
				ChessMove move = new ChessMove(from, to, piece, capturedPiece);
				move.moveType = MoveType.EnPassant;
				move.capturedPiece = board.sideToMove == 'w' ? 'p' : 'P'; // The captured pawn
				Debug.Log($"<color=green>[ChessMove] Parsed en passant move: {uciMove}</color>");
				return move;
			}

			return new ChessMove(from, to, piece, capturedPiece);
		}

		/// <summary>
		/// Create a promotion move from basic parameters
		/// Automatically determines promotion piece case based on moving piece
		/// </summary>
		public static ChessMove CreatePromotionMove(v2 from, v2 to, char movingPiece, char promotionType, char capturedPiece = '\0')
		{
			// Validate promotion is legal
			if (!RequiresPromotion(from, to, movingPiece))
			{
				Debug.Log($"<color=red>[ChessMove] Invalid promotion: {movingPiece} from {from} to {to}</color>");
				return Invalid();
			}

			// Ensure promotion piece has correct case
			char promotionPiece = char.IsLower(movingPiece) ?
				char.ToLower(promotionType) : char.ToUpper(promotionType);

			return new ChessMove(from, to, movingPiece, promotionPiece, capturedPiece);
		}

		/// <summary>
		/// Parse move from long algebraic notation (e.g., "e2e4", "e7e8q")
		/// This is an alias for FromUCI for backward compatibility
		/// </summary>
		public static ChessMove FromLongAlgebraic(string moveString, ChessBoard board)
		{
			return FromUCI(moveString, board);
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

			if (kingPos.x < 0) return Invalid(); // King not found

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

			if (rookPos.x < 0) return Invalid(); // Rook not found

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
		/// Convert move to UCI notation (long algebraic)
		/// </summary>
		public string ToUCI()
		{
			if (!IsValid()) return "";

			if (moveType == MoveType.Castling)
			{
				// Return the actual king move for UCI format
				return ChessBoard.CoordToAlgebraic(from) + ChessBoard.CoordToAlgebraic(to);
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
		/// Convert move to long algebraic notation
		/// This is an alias for ToUCI for backward compatibility
		/// </summary>
		public string ToLongAlgebraic()
		{
			return ToUCI();
		}

		/// <summary>
		/// Convert move to short algebraic notation (SAN)
		/// Requires board context for disambiguation
		/// </summary>
		public string ToShortAlgebraic(ChessBoard board)
		{
			if (!IsValid()) return "";

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

			// Add en passant notation
			if (moveType == MoveType.EnPassant)
			{
				result += " e.p.";
			}

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

		/// <summary>
		/// Check if this move requires promotion (pawn reaching last rank)
		/// </summary>
		public static bool RequiresPromotion(v2 from, v2 to, char piece)
		{
			if (char.ToUpper(piece) != 'P') return false;

			bool isWhite = char.IsUpper(piece);
			int promotionRank = isWhite ? 7 : 0;

			return to.y == promotionRank;
		}

		/// <summary>
		/// Validate promotion piece type
		/// FIXED: Use IndexOf for Unity 2020.3 compatibility
		/// </summary>
		public static bool IsValidPromotionPiece(char piece)
		{
			return "QRBNqrbn".IndexOf(piece) >= 0;
		}

		/// <summary>
		/// Get default promotion piece for given side
		/// </summary>
		public static char GetDefaultPromotionPiece(bool isWhite)
		{
			return isWhite ? 'Q' : 'q';
		}

		/// <summary>
		/// Create invalid/empty move for error cases
		/// </summary>
		public static ChessMove Invalid()
		{
			return new ChessMove
			{
				from = new v2(-1, -1),
				to = new v2(-1, -1),
				piece = '\0',
				capturedPiece = '\0',
				moveType = MoveType.Normal,
				promotionPiece = '\0',
				rookFrom = new v2(-1, -1),
				rookTo = new v2(-1, -1)
			};
		}

		/// <summary>
		/// Test promotion parsing with various UCI formats
		/// </summary>
		public static void TestPromotionParsing()
		{
			Debug.Log("<color=cyan>[ChessMove] Testing promotion parsing...</color>");

			// Setup test board with white pawn on 7th rank
			ChessBoard testBoard = new ChessBoard();
			testBoard.board.ST(new v2(4, 6), 'P'); // e7
			testBoard.board.ST(new v2(4, 7), '.'); // e8 empty

			// Test cases
			string[] testMoves = { "e7e8q", "e7e8r", "e7e8b", "e7e8n", "a7a8Q", "h7h8N" };
			char[] expectedPieces = { 'Q', 'R', 'B', 'N', 'Q', 'N' };

			for (int i = 0; i < testMoves.Length; i++)
			{
				ChessMove move = FromUCI(testMoves[i], testBoard);
				if (move.moveType == MoveType.Promotion && move.promotionPiece == expectedPieces[i])
				{
					Debug.Log($"<color=green>[ChessMove] ✓ {testMoves[i]} -> {move.promotionPiece}</color>");
				}
				else
				{
					Debug.Log($"<color=red>[ChessMove] ✗ {testMoves[i]} failed - got {move.promotionPiece}, expected {expectedPieces[i]}</color>");
				}
			}

			// Test black pawn promotion
			testBoard.board.ST(new v2(3, 1), 'p'); // d2
			testBoard.board.ST(new v2(3, 0), '.'); // d1 empty
			testBoard.sideToMove = 'b';

			ChessMove blackPromotion = FromUCI("d2d1q", testBoard);
			if (blackPromotion.moveType == MoveType.Promotion && blackPromotion.promotionPiece == 'q')
			{
				Debug.Log("<color=green>[ChessMove] ✓ Black promotion parsing works</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessMove] ✗ Black promotion parsing failed</color>");
			}
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
			string result = ToUCI();
			if (string.IsNullOrEmpty(result))
				result = $"{ChessBoard.CoordToAlgebraic(from)}-{ChessBoard.CoordToAlgebraic(to)}";

			if (moveType == MoveType.EnPassant)
				result += " (e.p.)";
			else if (moveType == MoveType.Castling)
				result += " (castling)";
			else if (moveType == MoveType.Promotion)
				result += $" (={char.ToUpper(promotionPiece)})";
			else if (IsCapture())
				result += $" (captures {capturedPiece})";

			return result;
		}
	}
}