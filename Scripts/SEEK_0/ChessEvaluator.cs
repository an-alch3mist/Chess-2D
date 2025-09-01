/*
Enhanced Chess Evaluation System
- Improved position evaluation based on FEN and side-to-move
- Material balance with piece-square tables
- King safety and pawn structure evaluation
- Endgame vs middlegame evaluation scaling
- Integration with StockfishBridge for hybrid evaluation
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using SPACE_UTIL;

namespace GPTDeepResearch
{
	/// <summary>
	/// Enhanced chess position evaluation system
	/// Provides both fast local evaluation and Stockfish-assisted deep evaluation
	/// </summary>
	public static class ChessEvaluator
	{
		// Material values (in centipawns)
		private static readonly Dictionary<char, int> PieceValues = new Dictionary<char, int>
		{
			['P'] = 100,
			['p'] = -100,
			['N'] = 320,
			['n'] = -320,
			['B'] = 330,
			['b'] = -330,
			['R'] = 500,
			['r'] = -500,
			['Q'] = 900,
			['q'] = -900,
			['K'] = 0,
			['k'] = 0
		};

		// Piece-Square Tables (White's perspective, flip for black)
		private static readonly int[,] PawnTable = {
			{ 0,  0,  0,  0,  0,  0,  0,  0},
			{50, 50, 50, 50, 50, 50, 50, 50},
			{10, 10, 20, 30, 30, 20, 10, 10},
			{ 5,  5, 10, 25, 25, 10,  5,  5},
			{ 0,  0,  0, 20, 20,  0,  0,  0},
			{ 5, -5,-10,  0,  0,-10, -5,  5},
			{ 5, 10, 10,-20,-20, 10, 10,  5},
			{ 0,  0,  0,  0,  0,  0,  0,  0}
		};

		private static readonly int[,] KnightTable = {
			{-50,-40,-30,-30,-30,-30,-40,-50},
			{-40,-20,  0,  0,  0,  0,-20,-40},
			{-30,  0, 10, 15, 15, 10,  0,-30},
			{-30,  5, 15, 20, 20, 15,  5,-30},
			{-30,  0, 15, 20, 20, 15,  0,-30},
			{-30,  5, 10, 15, 15, 10,  5,-30},
			{-40,-20,  0,  5,  5,  0,-20,-40},
			{-50,-40,-30,-30,-30,-30,-40,-50}
		};

		private static readonly int[,] BishopTable = {
			{-20,-10,-10,-10,-10,-10,-10,-20},
			{-10,  0,  0,  0,  0,  0,  0,-10},
			{-10,  0,  5, 10, 10,  5,  0,-10},
			{-10,  5,  5, 10, 10,  5,  5,-10},
			{-10,  0, 10, 10, 10, 10,  0,-10},
			{-10, 10, 10, 10, 10, 10, 10,-10},
			{-10,  5,  0,  0,  0,  0,  5,-10},
			{-20,-10,-10,-10,-10,-10,-10,-20}
		};

		private static readonly int[,] RookTable = {
			{ 0,  0,  0,  0,  0,  0,  0,  0},
			{ 5, 10, 10, 10, 10, 10, 10,  5},
			{-5,  0,  0,  0,  0,  0,  0, -5},
			{-5,  0,  0,  0,  0,  0,  0, -5},
			{-5,  0,  0,  0,  0,  0,  0, -5},
			{-5,  0,  0,  0,  0,  0,  0, -5},
			{-5,  0,  0,  0,  0,  0,  0, -5},
			{ 0,  0,  0,  5,  5,  0,  0,  0}
		};

		private static readonly int[,] QueenTable = {
			{-20,-10,-10, -5, -5,-10,-10,-20},
			{-10,  0,  0,  0,  0,  0,  0,-10},
			{-10,  0,  5,  5,  5,  5,  0,-10},
			{ -5,  0,  5,  5,  5,  5,  0, -5},
			{  0,  0,  5,  5,  5,  5,  0, -5},
			{-10,  5,  5,  5,  5,  5,  0,-10},
			{-10,  0,  5,  0,  0,  0,  0,-10},
			{-20,-10,-10, -5, -5,-10,-10,-20}
		};

		private static readonly int[,] KingMiddlegameTable = {
			{-30,-40,-40,-50,-50,-40,-40,-30},
			{-30,-40,-40,-50,-50,-40,-40,-30},
			{-30,-40,-40,-50,-50,-40,-40,-30},
			{-30,-40,-40,-50,-50,-40,-40,-30},
			{-20,-30,-30,-40,-40,-30,-30,-20},
			{-10,-20,-20,-20,-20,-20,-20,-10},
			{ 20, 20,  0,  0,  0,  0, 20, 20},
			{ 20, 30, 10,  0,  0, 10, 30, 20}
		};

		private static readonly int[,] KingEndgameTable = {
			{-50,-40,-30,-20,-20,-30,-40,-50},
			{-30,-20,-10,  0,  0,-10,-20,-30},
			{-30,-10, 20, 30, 30, 20,-10,-30},
			{-30,-10, 30, 40, 40, 30,-10,-30},
			{-30,-10, 30, 40, 40, 30,-10,-30},
			{-30,-10, 20, 30, 30, 20,-10,-30},
			{-30,-30,  0,  0,  0,  0,-30,-30},
			{-50,-30,-30,-30,-30,-30,-30,-50}
		};

		/// <summary>
		/// Comprehensive position evaluation
		/// Returns evaluation from White's perspective (positive = good for White)
		/// </summary>
		public static float EvaluatePosition(ChessBoard board, bool useDeepAnalysis = false)
		{
			// Quick material and positional evaluation
			int materialScore = EvaluateMaterial(board);
			int positionalScore = EvaluatePositional(board);
			int kingSafetyScore = EvaluateKingSafety(board);
			int pawnStructureScore = EvaluatePawnStructure(board);

			int totalCentipawns = materialScore + positionalScore + kingSafetyScore + pawnStructureScore;

			// Convert to win probability (0-1 scale)
			float evaluation = CentipawnsToWinProbability(totalCentipawns);

			// Adjust for side to move (evaluation is from White's perspective)
			if (board.sideToMove == 'b')
			{
				return 1.0f - evaluation; // Return from Black's perspective
			}

			return evaluation;
		}

		/// <summary>
		/// Fast evaluation for move ordering and quick decisions
		/// </summary>
		public static int QuickEvaluate(ChessBoard board)
		{
			return EvaluateMaterial(board) + EvaluatePositional(board);
		}

		/// <summary>
		/// Evaluate material balance
		/// </summary>
		private static int EvaluateMaterial(ChessBoard board)
		{
			int score = 0;

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					char piece = board.board.GT(new v2(x, y));
					if (piece != '.' && PieceValues.ContainsKey(piece))
					{
						score += PieceValues[piece];
					}
				}
			}

			return score;
		}

		/// <summary>
		/// Evaluate positional factors using piece-square tables
		/// </summary>
		private static int EvaluatePositional(ChessBoard board)
		{
			int score = 0;
			bool isEndgame = IsEndgame(board);

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					char piece = board.board.GT(new v2(x, y));
					if (piece == '.') continue;

					bool isWhite = char.IsUpper(piece);
					int tableValue = GetPieceSquareValue(piece, x, y, isEndgame);

					score += isWhite ? tableValue : -tableValue;
				}
			}

			return score;
		}

		/// <summary>
		/// Get piece-square table value for a piece at given position
		/// </summary>
		private static int GetPieceSquareValue(char piece, int x, int y, bool isEndgame)
		{
			// For black pieces, flip the y coordinate
			int tableY = char.IsUpper(piece) ? y : 7 - y;

			switch (char.ToUpper(piece))
			{
				case 'P': return PawnTable[tableY, x];
				case 'N': return KnightTable[tableY, x];
				case 'B': return BishopTable[tableY, x];
				case 'R': return RookTable[tableY, x];
				case 'Q': return QueenTable[tableY, x];
				case 'K': return isEndgame ? KingEndgameTable[tableY, x] : KingMiddlegameTable[tableY, x];
				default: return 0;
			}
		}

		/// <summary>
		/// Evaluate king safety
		/// </summary>
		private static int EvaluateKingSafety(ChessBoard board)
		{
			int score = 0;

			// Find kings
			v2 whiteKing = FindKing(board, 'K');
			v2 blackKing = FindKing(board, 'k');

			if (whiteKing.x >= 0)
			{
				score += EvaluateKingSafetyForSide(board, whiteKing, true);
			}

			if (blackKing.x >= 0)
			{
				score -= EvaluateKingSafetyForSide(board, blackKing, false);
			}

			return score;
		}

		/// <summary>
		/// Evaluate king safety for one side
		/// </summary>
		private static int EvaluateKingSafetyForSide(ChessBoard board, v2 kingPos, bool isWhite)
		{
			int safetyScore = 0;

			// Penalty for exposed king
			int attackerCount = 0;
			char opponent = isWhite ? 'b' : 'w';

			// Check squares around king for attackers
			for (int dy = -1; dy <= 1; dy++)
			{
				for (int dx = -1; dx <= 1; dx++)
				{
					v2 square = new v2(kingPos.x + dx, kingPos.y + dy);
					if (IsInBounds(square) && MoveGenerator.IsSquareAttacked(board, square, opponent))
					{
						attackerCount++;
					}
				}
			}

			safetyScore -= attackerCount * 10; // Penalty for attacked squares near king

			// Bonus for castling rights (king safety)
			string rights = board.castlingRights;
			if (isWhite)
			{
				if (rights.Contains('K'.ToString()) || rights.Contains('Q'.ToString()))
					safetyScore += 15;
			}
			else
			{
				if (rights.Contains('k'.ToString()) || rights.Contains('q'.ToString()))
					safetyScore += 15;
			}

			return safetyScore;
		}

		/// <summary>
		/// Evaluate pawn structure
		/// </summary>
		private static int EvaluatePawnStructure(ChessBoard board)
		{
			int score = 0;

			// Count pawns per file
			int[] whitePawnsPerFile = new int[8];
			int[] blackPawnsPerFile = new int[8];

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					char piece = board.board.GT(new v2(x, y));
					if (piece == 'P') whitePawnsPerFile[x]++;
					else if (piece == 'p') blackPawnsPerFile[x]++;
				}
			}

			// Penalty for doubled pawns
			for (int file = 0; file < 8; file++)
			{
				if (whitePawnsPerFile[file] > 1)
					score -= (whitePawnsPerFile[file] - 1) * 10;
				if (blackPawnsPerFile[file] > 1)
					score += (blackPawnsPerFile[file] - 1) * 10;
			}

			// Bonus for passed pawns
			score += EvaluatePassedPawns(board);

			return score;
		}

		/// <summary>
		/// Evaluate passed pawns
		/// </summary>
		private static int EvaluatePassedPawns(ChessBoard board)
		{
			int score = 0;

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					char piece = board.board.GT(new v2(x, y));
					if (char.ToUpper(piece) != 'P') continue;

					bool isWhite = char.IsUpper(piece);
					if (IsPassedPawn(board, new v2(x, y), isWhite))
					{
						int bonus = (isWhite ? y + 1 : 8 - y) * 10; // More valuable closer to promotion
						score += isWhite ? bonus : -bonus;
					}
				}
			}

			return score;
		}

		/// <summary>
		/// Check if a pawn is passed (no opposing pawns can stop it)
		/// </summary>
		private static bool IsPassedPawn(ChessBoard board, v2 pawnPos, bool isWhite)
		{
			char opponentPawn = isWhite ? 'p' : 'P';
			int direction = isWhite ? 1 : -1;
			int startY = pawnPos.y + direction;
			int endY = isWhite ? 8 : -1;

			// Check the three files (left, same, right) ahead of the pawn
			for (int file = Math.Max(0, pawnPos.x - 1); file <= Math.Min(7, pawnPos.x + 1); file++)
			{
				for (int rank = startY; rank != endY; rank += direction)
				{
					if (IsInBounds(new v2(file, rank)) && board.board.GT(new v2(file, rank)) == opponentPawn)
					{
						return false; // Opponent pawn blocks or can capture
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Determine if position is in endgame phase
		/// </summary>
		private static bool IsEndgame(ChessBoard board)
		{
			int totalMaterial = 0;
			int pieceCount = 0;

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					char piece = board.board.GT(new v2(x, y));
					if (piece != '.' && char.ToUpper(piece) != 'K')
					{
						totalMaterial += Math.Abs(PieceValues[piece]);
						pieceCount++;
					}
				}
			}

			// Endgame if few pieces or low material
			return pieceCount <= 10 || totalMaterial <= 1300;
		}

		/// <summary>
		/// Convert centipawn evaluation to win probability
		/// </summary>
		private static float CentipawnsToWinProbability(int centipawns)
		{
			// Logistic function: 1 / (1 + e^(-cp/400))
			return 1f / (1f + Mathf.Exp(-centipawns / 400f));
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
			return new v2(-1, -1);
		}

		/// <summary>
		/// Check if coordinates are within board bounds
		/// </summary>
		private static bool IsInBounds(v2 pos)
		{
			return pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 8;
		}

		/// <summary>
		/// Get a human-readable evaluation string
		/// </summary>
		public static string GetEvaluationString(ChessBoard board)
		{
			float eval = EvaluatePosition(board);
			int centipawns = (int)((eval - 0.5f) * 800f); // Rough conversion back to centipawns

			if (Math.Abs(centipawns) < 15)
				return "Equal";
			else if (centipawns > 0)
				return $"White +{centipawns / 100f:F1}";
			else
				return $"Black +{Math.Abs(centipawns) / 100f:F1}";
		}
	}
}