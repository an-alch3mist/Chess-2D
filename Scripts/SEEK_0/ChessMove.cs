/*
CHANGELOG (Enhanced Version - Unity 2020.3 Compatible):
v1.1.0 - Unity 2020.3 Compatibility Updates:
- Fixed string.Contains(char) compatibility issues for .NET 2.0
- Minimized public API surface area by making utility methods private
- Consolidated testing framework into private methods with single public runner
- Enhanced error handling with Unity-compatible logging patterns
- Optimized caching system with better memory management
- Improved PGN parsing with more robust disambiguation logic
- Added comprehensive edge case handling for promotion moves
- Enhanced performance with string operation optimizations
- Improved UCI parsing with better validation and error recovery
- Added move annotation support with comprehensive validation
*/

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using SPACE_UTIL;

namespace GPTDeepResearch
{
	/// <summary>
	/// Represents a chess move with comprehensive parsing support for UCI, PGN, and annotations.
	/// Enhanced with performance optimizations and Unity 2020.3 compatibility.
	/// </summary>
	[System.Serializable]
	public struct ChessMove : IEquatable<ChessMove>
	{
		[Header("Move Data")]
		public v2 from;
		public v2 to;
		public char piece;
		public char capturedPiece;

		public enum MoveType
		{
			Normal,
			Castling,
			EnPassant,
			Promotion,
			CastlingPromotion
		}

		[Header("Special Moves")]
		public MoveType moveType;
		public char promotionPiece;

		[Header("Castling Data")]
		public v2 rookFrom;
		public v2 rookTo;

		[Header("Analysis Data")]
		public float analysisTime;    // Time taken for analysis (ms)
		public string annotation;     // Move annotation (+, #, !, ?, etc.)
		public int engineDepth;       // Search depth if from engine
		public float engineEval;      // Engine evaluation of position after move

		/// <summary>
		/// Move annotations for PGN support
		/// </summary>
		public static class Annotations
		{
			public const string Check = "+";
			public const string Checkmate = "#";
			public const string Brilliant = "!!";
			public const string Good = "!";
			public const string Interesting = "!?";
			public const string Dubious = "?!";
			public const string Mistake = "?";
			public const string Blunder = "??";
		}

		// Move cache for performance - made private
		private static Dictionary<string, ChessMove> uciCache = new Dictionary<string, ChessMove>();
		private const int MAX_CACHE_SIZE = 1000;

		#region Constructors

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
			this.analysisTime = 0f;
			this.annotation = "";
			this.engineDepth = 0;
			this.engineEval = 0f;
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
			this.analysisTime = 0f;
			this.annotation = "";
			this.engineDepth = 0;
			this.engineEval = 0f;
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
			this.analysisTime = 0f;
			this.annotation = "";
			this.engineDepth = 0;
			this.engineEval = 0f;
		}

		#endregion

		#region Public Parsing Methods

		/// <summary>
		/// Parse move from PGN (Standard Algebraic Notation)
		/// </summary>
		public static ChessMove FromPGN(string pgnMove, ChessBoard board, List<ChessMove> legalMoves = null)
		{
			if (string.IsNullOrEmpty(pgnMove))
			{
				Debug.Log("<color=red>[ChessMove] Invalid PGN move: null or empty</color>");
				return Invalid();
			}

			// Get legal moves if not provided
			if (legalMoves == null)
			{
				legalMoves = board.GetLegalMoves();
			}

			// Clean the move string
			string cleanMove = CleanPGNMove(pgnMove);

			// Handle castling
			if (cleanMove == "O-O" || cleanMove == "0-0")
			{
				return FindCastlingMove(board, legalMoves, true);
			}
			if (cleanMove == "O-O-O" || cleanMove == "0-0-0")
			{
				return FindCastlingMove(board, legalMoves, false);
			}

			// Parse the move components
			PGNComponents components = ParsePGNComponents(cleanMove);
			if (components.targetSquare == null)
			{
				Debug.Log("<color=red>[ChessMove] Failed to parse PGN move: " + pgnMove + "</color>");
				return Invalid();
			}

			// Find matching legal moves
			List<ChessMove> candidates = FindCandidateMoves(legalMoves, components);

			if (candidates.Count == 0)
			{
				Debug.Log("<color=red>[ChessMove] No legal moves match PGN: " + pgnMove + "</color>");
				return Invalid();
			}

			if (candidates.Count == 1)
			{
				ChessMove result = candidates[0];
				result.annotation = ExtractAnnotation(pgnMove);
				Debug.Log("<color=green>[ChessMove] Parsed PGN: " + pgnMove + " -> " + result.ToUCI() + "</color>");
				return result;
			}

			// Disambiguate multiple candidates
			ChessMove disambiguated = DisambiguateMove(candidates, components);
			if (disambiguated.IsValid())
			{
				disambiguated.annotation = ExtractAnnotation(pgnMove);
				Debug.Log("<color=green>[ChessMove] Disambiguated PGN: " + pgnMove + " -> " + disambiguated.ToUCI() + "</color>");
				return disambiguated;
			}

			Debug.Log("<color=red>[ChessMove] Could not disambiguate PGN move: " + pgnMove + "</color>");
			return Invalid();
		}

		/// <summary>
		/// Enhanced UCI parsing with performance optimizations
		/// </summary>
		public static ChessMove FromUCI(string uciMove, ChessBoard board)
		{
			if (string.IsNullOrEmpty(uciMove) || uciMove.Length < 4)
			{
				return Invalid();
			}

			// Check cache first
			if (uciCache.ContainsKey(uciMove))
			{
				var cached = uciCache[uciMove];
				// Validate cache against current board (piece at from square)
				if (board.GetPiece(cached.from) == cached.piece)
				{
					return cached;
				}
				uciCache.Remove(uciMove); // Invalid cache entry
			}

			// Parse with optimized string operations
			char[] moveChars = uciMove.ToCharArray();
			if (moveChars.Length < 4) return Invalid();

			// Direct coordinate calculation (optimized)
			int fromX = moveChars[0] - 'a';
			int fromY = moveChars[1] - '1';
			int toX = moveChars[2] - 'a';
			int toY = moveChars[3] - '1';

			if (fromX < 0 || fromX > 7 || fromY < 0 || fromY > 7 ||
				toX < 0 || toX > 7 || toY < 0 || toY > 7)
			{
				return Invalid();
			}

			v2 from = new v2(fromX, fromY);
			v2 to = new v2(toX, toY);
			char piece = board.GetPiece(from);
			char capturedPiece = board.GetPiece(to);
			if (capturedPiece == '.') capturedPiece = '\0';

			ChessMove result;

			// Handle promotion
			if (moveChars.Length >= 5)
			{
				char promotionChar = moveChars[4];
				if (IsValidPromotionCharacter(promotionChar))
				{
					char finalPromotionPiece = char.IsLower(piece) ?
						char.ToLower(promotionChar) : char.ToUpper(promotionChar);
					result = new ChessMove(from, to, piece, finalPromotionPiece, capturedPiece);
				}
				else
				{
					result = new ChessMove(from, to, piece, capturedPiece);
				}
			}
			else
			{
				// Auto-detect promotion
				if (RequiresPromotion(from, to, piece))
				{
					char defaultPromotion = char.IsLower(piece) ? 'q' : 'Q';
					result = new ChessMove(from, to, piece, defaultPromotion, capturedPiece);
				}
				else
				{
					result = new ChessMove(from, to, piece, capturedPiece);
				}
			}

			// Cache for future use
			if (uciCache.Count < MAX_CACHE_SIZE)
			{
				uciCache[uciMove] = result;
			}

			return result;
		}

		/// <summary>
		/// Create a promotion move with validation
		/// </summary>
		public static ChessMove CreatePromotionMove(v2 from, v2 to, char movingPiece, char promotionType, char capturedPiece = '\0')
		{
			if (!RequiresPromotion(from, to, movingPiece))
			{
				Debug.Log("<color=red>[ChessMove] Invalid promotion: " + movingPiece + " from " + ChessBoard.CoordToAlgebraic(from) + " to " + ChessBoard.CoordToAlgebraic(to) + "</color>");
				return Invalid();
			}

			char promotionPiece = char.IsLower(movingPiece) ?
				char.ToLower(promotionType) : char.ToUpper(promotionType);

			return new ChessMove(from, to, movingPiece, promotionPiece, capturedPiece);
		}

		/// <summary>
		/// Invalid move constant
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
				rookTo = new v2(-1, -1),
				analysisTime = 0f,
				annotation = "",
				engineDepth = 0,
				engineEval = 0f
			};
		}

		#endregion

		#region Output Methods

		/// <summary>
		/// Enhanced PGN output with proper disambiguation
		/// </summary>
		public string ToPGN(ChessBoard board, List<ChessMove> legalMoves = null)
		{
			if (!IsValid()) return "";

			if (moveType == MoveType.Castling)
			{
				bool kingside = to.x > from.x;
				return (kingside ? "O-O" : "O-O-O") + annotation;
			}

			if (legalMoves == null)
			{
				legalMoves = board.GetLegalMoves();
			}

			StringBuilder pgn = new StringBuilder();
			char movingPiece = char.ToUpper(piece);

			// Add piece letter (except for pawns)
			if (movingPiece != 'P')
			{
				pgn.Append(movingPiece);
			}

			// Add disambiguation if needed
			string disambiguation = GetDisambiguation(legalMoves);
			pgn.Append(disambiguation);

			// Add capture notation
			if (capturedPiece != '\0' || moveType == MoveType.EnPassant)
			{
				if (movingPiece == 'P' && string.IsNullOrEmpty(disambiguation))
				{
					pgn.Append((char)('a' + from.x)); // Pawn captures show file
				}
				pgn.Append('x');
			}

			// Add target square
			pgn.Append(ChessBoard.CoordToAlgebraic(to));

			// Add promotion
			if (moveType == MoveType.Promotion && promotionPiece != '\0')
			{
				pgn.Append('=');
				pgn.Append(char.ToUpper(promotionPiece));
			}

			// Add en passant notation
			if (moveType == MoveType.EnPassant)
			{
				pgn.Append(" e.p.");
			}

			// Add annotation
			pgn.Append(annotation);

			return pgn.ToString();
		}

		/// <summary>
		/// Enhanced UCI output
		/// </summary>
		public string ToUCI()
		{
			if (!IsValid()) return "";

			if (moveType == MoveType.Castling)
			{
				return ChessBoard.CoordToAlgebraic(from) + ChessBoard.CoordToAlgebraic(to);
			}

			string fromSquare = ChessBoard.CoordToAlgebraic(from);
			string toSquare = ChessBoard.CoordToAlgebraic(to);

			if (string.IsNullOrEmpty(fromSquare) || string.IsNullOrEmpty(toSquare))
				return "";

			StringBuilder result = new StringBuilder(5);
			result.Append(fromSquare);
			result.Append(toSquare);

			if (moveType == MoveType.Promotion && promotionPiece != '\0')
			{
				result.Append(char.ToLower(promotionPiece));
			}

			return result.ToString();
		}

		#endregion

		#region Analysis and Timing Support

		/// <summary>
		/// Set analysis data for engine integration
		/// </summary>
		public ChessMove WithAnalysisData(float analysisTimeMs, int depth, float evaluation)
		{
			ChessMove result = this;
			result.analysisTime = analysisTimeMs;
			result.engineDepth = depth;
			result.engineEval = evaluation;
			return result;
		}

		/// <summary>
		/// Set move annotation
		/// </summary>
		public ChessMove WithAnnotation(string annotation)
		{
			ChessMove result = this;
			result.annotation = annotation ?? "";
			return result;
		}

		/// <summary>
		/// Get analysis summary string
		/// </summary>
		public string GetAnalysisSummary()
		{
			if (engineDepth == 0 && analysisTime == 0f)
			{
				return "";
			}

			StringBuilder summary = new StringBuilder();

			if (engineEval != 0f)
			{
				summary.Append("Eval: ");
				if (engineEval > 0) summary.Append("+");
				summary.Append(engineEval.ToString("0.00"));
			}

			if (engineDepth > 0)
			{
				if (summary.Length > 0) summary.Append(", ");
				summary.Append("Depth: " + engineDepth);
			}

			if (analysisTime > 0f)
			{
				if (summary.Length > 0) summary.Append(", ");
				summary.Append("Time: " + analysisTime.ToString("F0") + "ms");
			}

			return summary.ToString();
		}

		#endregion

		#region Validation and Utility

		/// <summary>
		/// Enhanced validation
		/// </summary>
		public bool IsValid()
		{
			return from.x >= 0 && from.x < 8 && from.y >= 0 && from.y < 8 &&
				   to.x >= 0 && to.x < 8 && to.y >= 0 && to.y < 8 &&
				   piece != '\0' && (from.x != to.x || from.y != to.y);
		}

		/// <summary>
		/// Validate move is legal on given board
		/// </summary>
		public bool IsLegal(ChessBoard board)
		{
			if (!IsValid()) return false;
			var legalMoves = board.GetLegalMoves();
			return legalMoves.Contains(this);
		}

		public bool IsCapture()
		{
			return capturedPiece != '\0' || moveType == MoveType.EnPassant;
		}

		public bool IsQuiet()
		{
			return !IsCapture() && moveType == MoveType.Normal;
		}

		public int GetDistance()
		{
			return Math.Abs(to.x - from.x) + Math.Abs(to.y - from.y);
		}

		public static bool RequiresPromotion(v2 from, v2 to, char piece)
		{
			if (char.ToUpper(piece) != 'P') return false;
			bool isWhite = char.IsUpper(piece);
			int promotionRank = isWhite ? 7 : 0;
			return to.y == promotionRank;
		}

		public static bool IsValidPromotionPiece(char piece)
		{
			return "QRBNqrbn".IndexOf(piece) >= 0;
		}

		public static char GetDefaultPromotionPiece(bool isWhite)
		{
			return isWhite ? 'Q' : 'q';
		}

		public static char[] GetPromotionOptions(bool isWhite)
		{
			return isWhite ? new char[] { 'Q', 'R', 'B', 'N' } : new char[] { 'q', 'r', 'b', 'n' };
		}

		public static string GetPromotionPieceName(char piece)
		{
			switch (char.ToUpper(piece))
			{
				case 'Q': return "Queen";
				case 'R': return "Rook";
				case 'B': return "Bishop";
				case 'N': return "Knight";
				default: return "Unknown";
			}
		}

		#endregion

		#region Private Helper Methods

		private struct PGNComponents
		{
			public char pieceType;        // 'N', 'B', 'R', 'Q', 'K', or '\0' for pawn
			public string targetSquare;   // "e4", "c5", etc.
			public bool isCapture;        // true if contains 'x'
			public char fromFile;         // for disambiguation ('a'-'h' or '\0')
			public char fromRank;         // for disambiguation ('1'-'8' or '\0')
			public char promotionPiece;   // promotion piece or '\0'
			public bool isCheck;          // ends with '+'
			public bool isCheckmate;      // ends with '#'
		}

		private static string CleanPGNMove(string move)
		{
			StringBuilder cleaned = new StringBuilder();
			bool inAnnotation = false;

			foreach (char c in move)
			{
				if (c == '{')
				{
					inAnnotation = true;
					continue;
				}
				if (c == '}')
				{
					inAnnotation = false;
					continue;
				}
				if (inAnnotation) continue;

				// Keep structural characters
				if (char.IsLetterOrDigit(c) || c == '-' || c == 'x' || c == '=' || c == '+' || c == '#' || c == 'O')
				{
					cleaned.Append(c);
				}
			}

			return cleaned.ToString().Trim();
		}

		private static PGNComponents ParsePGNComponents(string move)
		{
			var components = new PGNComponents();
			int index = 0;

			// Check for piece type - Unity 2020.3 compatible
			if (index < move.Length && "NBRQK".IndexOf(move[index].ToString()) >= 0)
			{
				components.pieceType = move[index];
				index++;
			}

			// Look for disambiguation and capture
			while (index < move.Length - 2)
			{
				char c = move[index];

				if (c == 'x')
				{
					components.isCapture = true;
					index++;
				}
				else if (c >= 'a' && c <= 'h' && components.fromFile == '\0')
				{
					// Could be file disambiguation or target
					if (index + 1 < move.Length && move[index + 1] >= '1' && move[index + 1] <= '8')
					{
						// This looks like target square
						break;
					}
					else
					{
						components.fromFile = c;
						index++;
					}
				}
				else if (c >= '1' && c <= '8' && components.fromRank == '\0')
				{
					components.fromRank = c;
					index++;
				}
				else
				{
					break;
				}
			}

			// Extract target square
			if (index < move.Length - 1)
			{
				components.targetSquare = move.Substring(index, 2);
				index += 2;
			}

			// Check for promotion
			if (index < move.Length && move[index] == '=')
			{
				index++;
				if (index < move.Length && "QRBN".IndexOf(move[index].ToString()) >= 0)
				{
					components.promotionPiece = move[index];
					index++;
				}
			}

			// Check for check/checkmate
			if (index < move.Length)
			{
				if (move[index] == '#')
				{
					components.isCheckmate = true;
				}
				else if (move[index] == '+')
				{
					components.isCheck = true;
				}
			}

			return components;
		}

		private static List<ChessMove> FindCandidateMoves(List<ChessMove> legalMoves, PGNComponents components)
		{
			var candidates = new List<ChessMove>();
			v2 targetCoord = ChessBoard.AlgebraicToCoord(components.targetSquare);

			if (targetCoord.x < 0) return candidates;

			foreach (var move in legalMoves)
			{
				// Check if target matches
				if (move.to != targetCoord) continue;

				// Check piece type
				char movePieceType = char.ToUpper(move.piece);
				char expectedType = components.pieceType == '\0' ? 'P' : components.pieceType;

				if (movePieceType != expectedType) continue;

				// Check capture requirement
				if (components.isCapture && !move.IsCapture()) continue;
				if (!components.isCapture && move.IsCapture() && movePieceType != 'P') continue;

				// Check promotion
				if (components.promotionPiece != '\0')
				{
					if (move.moveType != MoveType.Promotion ||
						char.ToUpper(move.promotionPiece) != components.promotionPiece)
						continue;
				}

				candidates.Add(move);
			}

			return candidates;
		}

		private static ChessMove DisambiguateMove(List<ChessMove> candidates, PGNComponents components)
		{
			// Filter by file disambiguation
			if (components.fromFile != '\0')
			{
				candidates = candidates.FindAll(m => (char)('a' + m.from.x) == components.fromFile);
			}

			// Filter by rank disambiguation
			if (components.fromRank != '\0')
			{
				candidates = candidates.FindAll(m => (char)('1' + m.from.y) == components.fromRank);
			}

			return candidates.Count == 1 ? candidates[0] : Invalid();
		}

		private static ChessMove FindCastlingMove(ChessBoard board, List<ChessMove> legalMoves, bool kingside)
		{
			foreach (var move in legalMoves)
			{
				if (move.moveType == MoveType.Castling)
				{
					bool moveIsKingside = move.to.x > move.from.x;
					if (moveIsKingside == kingside)
					{
						return move;
					}
				}
			}
			return Invalid();
		}

		private static string ExtractAnnotation(string pgnMove)
		{
			StringBuilder annotation = new StringBuilder();
			bool foundAnnotation = false;

			for (int i = pgnMove.Length - 1; i >= 0; i--)
			{
				char c = pgnMove[i];
				if (c == '!' || c == '?' || c == '+' || c == '#')
				{
					annotation.Insert(0, c);
					foundAnnotation = true;
				}
				else if (foundAnnotation)
				{
					break;
				}
			}

			return annotation.ToString();
		}

		private string GetDisambiguation(List<ChessMove> legalMoves)
		{
			// Find other moves to the same square with same piece
			var conflictingMoves = new List<ChessMove>();
			char movingPieceType = char.ToUpper(piece);

			foreach (var move in legalMoves)
			{
				if (move.to == to && char.ToUpper(move.piece) == movingPieceType && move.from != from)
				{
					conflictingMoves.Add(move);
				}
			}

			if (conflictingMoves.Count == 0)
			{
				return "";
			}

			// Check if file disambiguation is sufficient
			bool needFileDisambiguation = false;
			bool needRankDisambiguation = false;

			foreach (var move in conflictingMoves)
			{
				if (move.from.x == from.x)
				{
					needRankDisambiguation = true;
				}
				if (move.from.y == from.y)
				{
					needFileDisambiguation = true;
				}
			}

			// Prefer file disambiguation if possible
			if (!needRankDisambiguation || needFileDisambiguation)
			{
				return "" + (char)('a' + from.x);
			}
			else
			{
				return "" + (char)('1' + from.y);
			}
		}

		private static bool IsValidPromotionCharacter(char c)
		{
			return "QRBNqrbn".IndexOf(c.ToString()) >= 0;
		}

		#endregion

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

		#region ToString Override

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();
			result.Append(ToUCI());

			if (moveType == MoveType.EnPassant)
				result.Append(" (e.p.)");
			else if (moveType == MoveType.Castling)
				result.Append(" (castling)");
			else if (moveType == MoveType.Promotion)
				result.Append(" (=" + char.ToUpper(promotionPiece) + ")");
			else if (IsCapture())
				result.Append(" (captures " + capturedPiece + ")");

			if (!string.IsNullOrEmpty(annotation))
				result.Append(" " + annotation);

			string analysisSummary = GetAnalysisSummary();
			if (!string.IsNullOrEmpty(analysisSummary))
				result.Append(" [" + analysisSummary + "]");

			return result.ToString();
		}

		#endregion

		#region Testing Framework

		/// <summary>
		/// Test UCI promotion parsing with comprehensive cases
		/// </summary>
		private static void TestUCIPromotionParsing()
		{
			Debug.Log("<color=cyan>[ChessMove] Testing UCI promotion parsing...</color>");

			// Setup test board with promotion scenarios
			ChessBoard testBoard = new ChessBoard();

			// White pawn on 7th rank
			testBoard.board.ST(new v2(4, 6), 'P'); // e7
			testBoard.board.ST(new v2(4, 7), '.'); // e8 empty
			testBoard.sideToMove = 'w';

			// Test white promotion cases
			string[] whiteCases = { "e7e8q", "e7e8r", "e7e8b", "e7e8n" };
			char[] expectedWhite = { 'Q', 'R', 'B', 'N' };

			for (int i = 0; i < whiteCases.Length; i++)
			{
				ChessMove move = FromUCI(whiteCases[i], testBoard);
				if (move.moveType == MoveType.Promotion && move.promotionPiece == expectedWhite[i])
				{
					Debug.Log("<color=green>[ChessMove] ✓ UCI white promotion: " + whiteCases[i] + " -> " + move.promotionPiece + "</color>");
				}
				else
				{
					Debug.Log("<color=red>[ChessMove] ✗ UCI white promotion failed: " + whiteCases[i] + " -> got " + move.promotionPiece + ", expected " + expectedWhite[i] + "</color>");
				}
			}

			// Test black pawn promotion
			testBoard.board.ST(new v2(3, 1), 'p'); // d2
			testBoard.board.ST(new v2(3, 0), '.'); // d1 empty
			testBoard.sideToMove = 'b';

			string[] blackCases = { "d2d1q", "d2d1r", "d2d1b", "d2d1n" };
			char[] expectedBlack = { 'q', 'r', 'b', 'n' };

			for (int i = 0; i < blackCases.Length; i++)
			{
				ChessMove move = FromUCI(blackCases[i], testBoard);
				if (move.moveType == MoveType.Promotion && move.promotionPiece == expectedBlack[i])
				{
					Debug.Log("<color=green>[ChessMove] ✓ UCI black promotion: " + blackCases[i] + " -> " + move.promotionPiece + "</color>");
				}
				else
				{
					Debug.Log("<color=red>[ChessMove] ✗ UCI black promotion failed: " + blackCases[i] + " -> got " + move.promotionPiece + ", expected " + expectedBlack[i] + "</color>");
				}
			}

			// Test promotion capture
			testBoard.board.ST(new v2(5, 7), 'r'); // f8 black rook
			ChessMove capturePromotion = FromUCI("e7f8q", testBoard);
			if (capturePromotion.moveType == MoveType.Promotion &&
				capturePromotion.promotionPiece == 'Q' &&
				capturePromotion.capturedPiece == 'r')
			{
				Debug.Log("<color=green>[ChessMove] ✓ UCI promotion with capture works</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessMove] ✗ UCI promotion with capture failed</color>");
			}

			// Test invalid cases
			ChessMove invalidMove = FromUCI("e7e8x", testBoard);
			if (!invalidMove.IsValid() || invalidMove.moveType != MoveType.Promotion)
			{
				Debug.Log("<color=green>[ChessMove] ✓ Invalid promotion piece rejected</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessMove] ✗ Invalid promotion piece accepted</color>");
			}
		}

		/// <summary>
		/// Test move creation and validation
		/// </summary>
		private static void TestMoveCreation()
		{
			Debug.Log("<color=cyan>[ChessMove] Testing move creation...</color>");

			// Test normal move
			ChessMove normalMove = new ChessMove(new v2(4, 1), new v2(4, 3), 'P');
			if (normalMove.IsValid() && normalMove.moveType == MoveType.Normal)
			{
				Debug.Log("<color=green>[ChessMove] ✓ Normal move creation works</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessMove] ✗ Normal move creation failed</color>");
			}

			// Test promotion move creation
			ChessMove promotion = CreatePromotionMove(new v2(4, 6), new v2(4, 7), 'P', 'Q');
			if (promotion.IsValid() && promotion.moveType == MoveType.Promotion && promotion.promotionPiece == 'Q')
			{
				Debug.Log("<color=green>[ChessMove] ✓ Promotion move creation works</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessMove] ✗ Promotion move creation failed</color>");
			}

			// Test invalid promotion
			ChessMove invalidPromotion = CreatePromotionMove(new v2(4, 3), new v2(4, 4), 'P', 'Q');
			if (!invalidPromotion.IsValid())
			{
				Debug.Log("<color=green>[ChessMove] ✓ Invalid promotion correctly rejected</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessMove] ✗ Invalid promotion incorrectly accepted</color>");
			}

			// Test castling move
			ChessMove castling = new ChessMove(new v2(4, 0), new v2(6, 0), new v2(7, 0), new v2(5, 0), 'K');
			if (castling.IsValid() && castling.moveType == MoveType.Castling)
			{
				Debug.Log("<color=green>[ChessMove] ✓ Castling move creation works</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessMove] ✗ Castling move creation failed</color>");
			}
		}

		/// <summary>
		/// Test PGN parsing with comprehensive cases
		/// </summary>
		private static void TestPGNParsing()
		{
			Debug.Log("<color=cyan>[ChessMove] Testing PGN parsing...</color>");

			ChessBoard testBoard = new ChessBoard();
			var legalMoves = testBoard.GetLegalMoves();

			// Test basic pawn moves
			string[] basicMoves = { "e4", "Nf3", "Bc4" };
			int successCount = 0;

			foreach (string moveStr in basicMoves)
			{
				ChessMove move = FromPGN(moveStr, testBoard, legalMoves);
				if (move.IsValid())
				{
					Debug.Log("<color=green>[ChessMove] ✓ PGN parsed: " + moveStr + " -> " + move.ToUCI() + "</color>");
					successCount++;
				}
				else
				{
					Debug.Log("<color=yellow>[ChessMove] ? PGN parse failed: " + moveStr + "</color>");
				}
			}

			// Test castling
			ChessMove castlingMove = FromPGN("O-O", testBoard);
			if (castlingMove.IsValid() && castlingMove.moveType == MoveType.Castling)
			{
				Debug.Log("<color=green>[ChessMove] ✓ PGN castling works</color>");
				successCount++;
			}

			// Test annotations
			ChessMove annotated = FromPGN("e4!", testBoard);
			if (annotated.IsValid() && annotated.annotation == "!")
			{
				Debug.Log("<color=green>[ChessMove] ✓ PGN annotations preserved</color>");
				successCount++;
			}

			Debug.Log("<color=cyan>[ChessMove] PGN parsing completed: " + successCount + " tests passed</color>");
		}

		/// <summary>
		/// Test performance optimizations
		/// </summary>
		private static void TestPerformanceOptimizations()
		{
			Debug.Log("<color=cyan>[ChessMove] Testing performance optimizations...</color>");

			ChessBoard testBoard = new ChessBoard();
			float startTime = Time.realtimeSinceStartup;

			// Test UCI cache
			string[] testMoves = { "e2e4", "e7e5", "g1f3", "b8c6", "f1c4" };

			// Warm up cache
			for (int i = 0; i < 50; i++)
			{
				foreach (string uci in testMoves)
				{
					ChessMove.FromUCI(uci, testBoard);
				}
			}

			float cacheTime = Time.realtimeSinceStartup - startTime;
			Debug.Log("<color=green>[ChessMove] Cache performance: " + (cacheTime * 1000f).ToString("F2") + "ms for 250 parses</color>");

			// Test cache validation
			if (uciCache.Count > 0)
			{
				Debug.Log("<color=green>[ChessMove] ✓ Cache system working</color>");
			}
			else
			{
				Debug.Log("<color=yellow>[ChessMove] ? Cache system may not be populating</color>");
			}
		}

		/// <summary>
		/// Test utility methods
		/// </summary>
		private static void TestUtilityMethods()
		{
			Debug.Log("<color=cyan>[ChessMove] Testing utility methods...</color>");

			// Test promotion detection
			bool needsPromo = RequiresPromotion(new v2(4, 6), new v2(4, 7), 'P');
			if (needsPromo)
			{
				Debug.Log("<color=green>[ChessMove] ✓ Promotion detection works</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessMove] ✗ Promotion detection failed</color>");
			}

			// Test promotion piece validation
			bool validPiece = IsValidPromotionPiece('Q');
			bool invalidPiece = !IsValidPromotionPiece('P');
			if (validPiece && invalidPiece)
			{
				Debug.Log("<color=green>[ChessMove] ✓ Promotion piece validation works</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessMove] ✗ Promotion piece validation failed</color>");
			}

			// Test promotion options
			char[] whiteOptions = GetPromotionOptions(true);
			char[] blackOptions = GetPromotionOptions(false);
			if (whiteOptions.Length == 4 && blackOptions.Length == 4)
			{
				Debug.Log("<color=green>[ChessMove] ✓ Promotion options correct</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessMove] ✗ Promotion options incorrect</color>");
			}

			// Test piece names
			string queenName = GetPromotionPieceName('Q');
			if (queenName == "Queen")
			{
				Debug.Log("<color=green>[ChessMove] ✓ Piece name mapping works</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessMove] ✗ Piece name mapping failed</color>");
			}
		}

		/// <summary>
		/// Test equality and comparison
		/// </summary>
		private static void TestEqualityAndComparison()
		{
			Debug.Log("<color=cyan>[ChessMove] Testing equality and comparison...</color>");

			ChessMove move1 = new ChessMove(new v2(4, 1), new v2(4, 3), 'P');
			ChessMove move2 = new ChessMove(new v2(4, 1), new v2(4, 3), 'P');
			ChessMove move3 = new ChessMove(new v2(4, 1), new v2(4, 4), 'P');

			bool equalityWorks = move1.Equals(move2) && move1 == move2;
			bool inequalityWorks = !move1.Equals(move3) && move1 != move3;
			bool hashCodesMatch = move1.GetHashCode() == move2.GetHashCode();

			if (equalityWorks && inequalityWorks && hashCodesMatch)
			{
				Debug.Log("<color=green>[ChessMove] ✓ Equality and comparison works</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessMove] ✗ Equality and comparison failed</color>");
			}
		}

		/// <summary>
		/// Test analysis data functionality
		/// </summary>
		private static void TestAnalysisData()
		{
			Debug.Log("<color=cyan>[ChessMove] Testing analysis data...</color>");

			ChessMove move = new ChessMove(new v2(4, 1), new v2(4, 3), 'P');
			ChessMove withAnalysis = move.WithAnalysisData(1500f, 12, 0.25f);
			ChessMove withAnnotation = withAnalysis.WithAnnotation("!");

			string summary = withAnnotation.GetAnalysisSummary();
			bool hasAnalysisData = withAnnotation.analysisTime > 0 && withAnnotation.engineDepth > 0;
			bool hasAnnotation = withAnnotation.annotation == "!";

			if (hasAnalysisData && hasAnnotation && !string.IsNullOrEmpty(summary))
			{
				Debug.Log("<color=green>[ChessMove] ✓ Analysis data functionality works</color>");
				Debug.Log("<color=cyan>[ChessMove] Analysis summary: " + summary + "</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessMove] ✗ Analysis data functionality failed</color>");
			}
		}

		/// <summary>
		/// Run all enhanced ChessMove tests
		/// </summary>
		public static void RunAllTests()
		{
			Debug.Log("<color=cyan>=== Enhanced ChessMove Test Suite ===</color>");

			TestUCIPromotionParsing();
			TestMoveCreation();
			TestPGNParsing();
			TestPerformanceOptimizations();
			TestUtilityMethods();
			TestEqualityAndComparison();
			TestAnalysisData();

			Debug.Log("<color=cyan>=== Enhanced ChessMove Tests Completed ===</color>");
		}

		#endregion
	}
}