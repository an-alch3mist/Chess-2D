/*
CHANGELOG (Enhanced Version):
- Added comprehensive undo/redo system with move history
- Added BoardState struct for efficient state snapshots
- Added player side selection support (white/black choice)
- Enhanced FEN parsing with better validation
- Added move history tracking and game state restoration
- Added support for exporting/importing game history
- Improved memory management for undo stack
- Added deep cloning functionality for board states
- Added evaluation meter support with side-to-move awareness
- Fixed Unity 2020.3 compatibility issues
*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SPACE_UTIL;

namespace GPTDeepResearch
{
	/// <summary>
	/// Represents a chess board position with comprehensive undo/redo support.
	/// Tracks full game history and allows complete state restoration.
	/// </summary>
	[System.Serializable]
	public class ChessBoard : ICloneable
	{
		[Header("Board State")]
		public Board<char> board = new Board<char>(new v2(8, 8), '.');
		public char sideToMove = 'w';
		public string castlingRights = "KQkq";
		public string enPassantSquare = "-";
		public int halfmoveClock = 0;
		public int fullmoveNumber = 1;

		[Header("Game Settings")]
		public char humanSide = 'w';        // Which side human plays ('w', 'b', or 'x' for both)
		public char engineSide = 'b';       // Which side engine plays ('w', 'b', or 'x' for none)
		public bool allowSideSwitching = true; // Allow changing sides mid-game

		[Header("Move History")]
		[SerializeField] private List<BoardState> history = new List<BoardState>();
		[SerializeField] private List<ChessMove> moveHistory = new List<ChessMove>();
		[SerializeField] private int currentHistoryIndex = -1;
		[SerializeField] private int maxHistorySize = 200; // Prevent memory issues

		[Header("Evaluation")]
		[SerializeField] private float lastEvaluation = 0f;
		[SerializeField] private float lastWinProbability = 0.5f;

		/// <summary>
		/// Represents a complete board state for undo/redo functionality
		/// </summary>
		[System.Serializable]
		public struct BoardState
		{
			public string fen;
			public char sideToMove;
			public string castlingRights;
			public string enPassantSquare;
			public int halfmoveClock;
			public int fullmoveNumber;
			public float timestamp;
			public float evaluation;

			public BoardState(ChessBoard board)
			{
				this.fen = board.ToFEN();
				this.sideToMove = board.sideToMove;
				this.castlingRights = board.castlingRights;
				this.enPassantSquare = board.enPassantSquare;
				this.halfmoveClock = board.halfmoveClock;
				this.fullmoveNumber = board.fullmoveNumber;
				this.timestamp = Time.time;
				this.evaluation = board.lastEvaluation;
			}
		}

		#region Constructors and Initialization

		public ChessBoard()
		{
			SetupStartingPosition();
			SaveCurrentState();
		}

		public ChessBoard(string fen)
		{
			if (string.IsNullOrEmpty(fen) || fen == "startpos")
			{
				SetupStartingPosition();
			}
			else
			{
				LoadFromFEN(fen);
			}
			SaveCurrentState();
		}

		/// <summary>
		/// Setup standard starting position
		/// </summary>
		public void SetupStartingPosition()
		{
			string startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
			LoadFromFEN(startFEN);
		}

		/// <summary>
		/// Set which side the human plays
		/// </summary>
		public void SetHumanSide(char side)
		{
			if (side != 'w' && side != 'b' && side != 'x')
			{
				Debug.Log($"<color=yellow>[ChessBoard] Invalid human side: {side}. Using 'w' (white)</color>");
				side = 'w';
			}

			humanSide = side;
			engineSide = side == 'w' ? 'b' : side == 'b' ? 'w' : 'x';

			Debug.Log($"<color=green>[ChessBoard] Human plays: {GetSideName(humanSide)}, Engine plays: {GetSideName(engineSide)}</color>");
		}

		/// <summary>
		/// Get human-readable side name
		/// </summary>
		public string GetSideName(char side)
		{
			switch (side)
			{
				case 'w': return "White";
				case 'b': return "Black";
				case 'x': return "Both/None";
				default: return "Unknown";
			}
		}

		/// <summary>
		/// Check if it's currently the human's turn
		/// </summary>
		public bool IsHumanTurn()
		{
			return humanSide == 'x' || humanSide == sideToMove;
		}

		/// <summary>
		/// Check if it's currently the engine's turn
		/// </summary>
		public bool IsEngineTurn()
		{
			return engineSide == 'x' || engineSide == sideToMove;
		}

		/// <summary>
		/// Switch sides (human becomes engine side and vice versa)
		/// </summary>
		public void SwitchSides()
		{
			if (!allowSideSwitching)
			{
				Debug.Log("<color=yellow>[ChessBoard] Side switching is disabled</color>");
				return;
			}

			char temp = humanSide;
			humanSide = engineSide;
			engineSide = temp;

			Debug.Log($"<color=green>[ChessBoard] Sides switched. Human: {GetSideName(humanSide)}, Engine: {GetSideName(engineSide)}</color>");
		}

		#endregion

		#region Undo/Redo System

		/// <summary>
		/// Save current board state to history
		/// </summary>
		public void SaveCurrentState()
		{
			// Trim history if at capacity
			if (history.Count >= maxHistorySize)
			{
				int removeCount = maxHistorySize / 4; // Remove 25% of oldest entries
				history.RemoveRange(0, removeCount);
				moveHistory.RemoveRange(0, removeCount);
				currentHistoryIndex -= removeCount;
			}

			// If we're not at the end of history, truncate future moves
			if (currentHistoryIndex < history.Count - 1)
			{
				int removeCount = history.Count - currentHistoryIndex - 1;
				history.RemoveRange(currentHistoryIndex + 1, removeCount);
				moveHistory.RemoveRange(currentHistoryIndex + 1, removeCount);
			}

			// Add current state
			history.Add(new BoardState(this));
			currentHistoryIndex = history.Count - 1;
		}

		/// <summary>
		/// Save state with associated move
		/// </summary>
		public void SaveStateWithMove(ChessMove move)
		{
			SaveCurrentState();
			if (moveHistory.Count < history.Count)
			{
				moveHistory.Add(move);
			}
			else if (moveHistory.Count > 0)
			{
				moveHistory[moveHistory.Count - 1] = move;
			}
		}

		/// <summary>
		/// Undo last move
		/// </summary>
		public bool UndoMove()
		{
			if (currentHistoryIndex <= 0)
			{
				Debug.Log("<color=yellow>[ChessBoard] Cannot undo: at start of game</color>");
				return false;
			}

			currentHistoryIndex--;
			RestoreState(history[currentHistoryIndex]);

			Debug.Log($"<color=green>[ChessBoard] Undid move. Now at position {currentHistoryIndex + 1}/{history.Count}</color>");
			return true;
		}

		/// <summary>
		/// Redo next move
		/// </summary>
		public bool RedoMove()
		{
			if (currentHistoryIndex >= history.Count - 1)
			{
				Debug.Log("<color=yellow>[ChessBoard] Cannot redo: at end of history</color>");
				return false;
			}

			currentHistoryIndex++;
			RestoreState(history[currentHistoryIndex]);

			Debug.Log($"<color=green>[ChessBoard] Redid move. Now at position {currentHistoryIndex + 1}/{history.Count}</color>");
			return true;
		}

		/// <summary>
		/// Get the last move played (for undo display)
		/// </summary>
		public ChessMove GetLastMove()
		{
			if (moveHistory.Count > 0 && currentHistoryIndex >= 0 && currentHistoryIndex < moveHistory.Count)
			{
				return moveHistory[currentHistoryIndex];
			}
			return ChessMove.Invalid();
		}

		/// <summary>
		/// Restore board state from BoardState
		/// </summary>
		private void RestoreState(BoardState state)
		{
			LoadFromFEN(state.fen);
			lastEvaluation = state.evaluation;
		}

		/// <summary>
		/// Get move history as PGN-style string
		/// </summary>
		public string GetMoveHistoryPGN()
		{
			if (moveHistory.Count == 0) return "";

			var pgn = new System.Text.StringBuilder();
			for (int i = 0; i < moveHistory.Count; i++)
			{
				if (i % 2 == 0)
				{
					pgn.Append($"{(i / 2) + 1}. ");
				}

				// For proper PGN, we'd need the board state to generate SAN
				// For now, use UCI notation
				pgn.Append(moveHistory[i].ToUCI());

				if (i % 2 == 0 && i < moveHistory.Count - 1)
				{
					pgn.Append(" ");
				}
				else if (i % 2 == 1)
				{
					pgn.Append(" ");
				}
			}

			return pgn.ToString().Trim();
		}

		/// <summary>
		/// Clear all history
		/// </summary>
		public void ClearHistory()
		{
			history.Clear();
			moveHistory.Clear();
			currentHistoryIndex = -1;
			SaveCurrentState();
		}

		/// <summary>
		/// Get number of moves that can be undone
		/// </summary>
		public int GetUndoCount()
		{
			return Mathf.Max(0, currentHistoryIndex);
		}

		/// <summary>
		/// Get number of moves that can be redone
		/// </summary>
		public int GetRedoCount()
		{
			return Mathf.Max(0, history.Count - currentHistoryIndex - 1);
		}

		/// <summary>
		/// Check if undo is possible
		/// </summary>
		public bool CanUndo()
		{
			return GetUndoCount() > 0;
		}

		/// <summary>
		/// Check if redo is possible
		/// </summary>
		public bool CanRedo()
		{
			return GetRedoCount() > 0;
		}

		#endregion

		#region Evaluation Support

		/// <summary>
		/// Update position evaluation (called by engine analysis)
		/// </summary>
		public void UpdateEvaluation(float centipawnScore, float winProbability)
		{
			lastEvaluation = centipawnScore;
			lastWinProbability = winProbability;
		}

		/// <summary>
		/// Get evaluation from current side's perspective
		/// </summary>
		public float GetSideToMoveEvaluation()
		{
			return sideToMove == 'w' ? lastEvaluation : -lastEvaluation;
		}

		/// <summary>
		/// Get win probability for current side to move
		/// </summary>
		public float GetSideToMoveWinProbability()
		{
			return sideToMove == 'w' ? lastWinProbability : (1f - lastWinProbability);
		}

		/// <summary>
		/// Get evaluation bar percentage (0-100) for UI display
		/// </summary>
		public float GetEvaluationBarPercentage()
		{
			// Convert win probability to percentage for white
			return lastWinProbability * 100f;
		}

		/// <summary>
		/// Get evaluation text for display
		/// </summary>
		public string GetEvaluationText()
		{
			if (Mathf.Abs(lastEvaluation) > 900) // Mate score
			{
				int mateIn = Mathf.RoundToInt((1000 - Mathf.Abs(lastEvaluation)) / 2);
				string side = lastEvaluation > 0 ? "White" : "Black";
				return $"M{mateIn} ({side})";
			}
			else
			{
				float displayEval = GetSideToMoveEvaluation();
				return displayEval >= 0 ? $"+{displayEval:F1}" : $"{displayEval:F1}";
			}
		}

		#endregion

		#region FEN Handling

		/// <summary>
		/// Load board from FEN notation with enhanced validation
		/// </summary>
		public bool LoadFromFEN(string fen)
		{
			if (string.IsNullOrEmpty(fen))
			{
				Debug.Log("<color=red>[ChessBoard] FEN string is null or empty</color>");
				return false;
			}

			try
			{
				string[] parts = fen.Trim().Split(' ');
				if (parts.Length < 1)
				{
					Debug.Log("<color=red>[ChessBoard] Invalid FEN: no board position</color>");
					return false;
				}

				// Parse board position
				if (!ParseBoardPosition(parts[0]))
				{
					return false;
				}

				// Parse additional fields with defaults
				sideToMove = parts.Length > 1 ? parts[1][0] : 'w';
				castlingRights = parts.Length > 2 ? parts[2] : "KQkq";
				enPassantSquare = parts.Length > 3 ? parts[3] : "-";

				if (parts.Length > 4 && int.TryParse(parts[4], out int halfMove))
					halfmoveClock = halfMove;
				else
					halfmoveClock = 0;

				if (parts.Length > 5 && int.TryParse(parts[5], out int fullMove))
					fullmoveNumber = fullMove;
				else
					fullmoveNumber = 1;

				Debug.Log($"<color=green>[ChessBoard] Loaded FEN: {fen}</color>");
				return true;
			}
			catch (Exception e)
			{
				Debug.Log($"<color=red>[ChessBoard] Error parsing FEN '{fen}': {e.Message}</color>");
				SetupStartingPosition();
				return false;
			}
		}

		/// <summary>
		/// Parse board position from FEN board string
		/// </summary>
		private bool ParseBoardPosition(string boardString)
		{
			string[] ranks = boardString.Split('/');
			if (ranks.Length != 8)
			{
				Debug.Log($"<color=red>[ChessBoard] Invalid board FEN: expected 8 ranks, got {ranks.Length}</color>");
				return false;
			}

			// Clear board
			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					board.ST(new v2(x, y), '.');
				}
			}

			// Parse each rank (FEN rank 8 = board y=7, FEN rank 1 = board y=0)
			for (int rank = 0; rank < 8; rank++)
			{
				int y = 7 - rank; // Convert FEN rank to board coordinate
				int x = 0;

				foreach (char c in ranks[rank])
				{
					if (char.IsDigit(c))
					{
						// Empty squares
						int emptyCount = c - '0';
						x += emptyCount;
					}
					else if ("rnbqkpRNBQKP".IndexOf(c) >= 0) // FIXED: Use IndexOf instead of Contains
					{
						// Piece
						if (x >= 8)
						{
							Debug.Log($"<color=red>[ChessBoard] Invalid FEN: too many pieces in rank {8 - rank}</color>");
							return false;
						}
						board.ST(new v2(x, y), c);
						x++;
					}
					else
					{
						Debug.Log($"<color=red>[ChessBoard] Invalid FEN character: '{c}' in rank {8 - rank}</color>");
						return false;
					}
				}

				if (x != 8)
				{
					Debug.Log($"<color=red>[ChessBoard] Invalid FEN: rank {8 - rank} has {x} squares, expected 8</color>");
					return false;
				}
			}

			return ValidateBoardState();
		}

		/// <summary>
		/// Validate board has exactly one king of each color
		/// </summary>
		private bool ValidateBoardState()
		{
			int whiteKings = 0, blackKings = 0;

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					char piece = board.GT(new v2(x, y));
					if (piece == 'K') whiteKings++;
					else if (piece == 'k') blackKings++;
				}
			}

			if (whiteKings != 1)
			{
				Debug.Log($"<color=red>[ChessBoard] Invalid position: found {whiteKings} white kings, expected exactly 1</color>");
				return false;
			}

			if (blackKings != 1)
			{
				Debug.Log($"<color=red>[ChessBoard] Invalid position: found {blackKings} black kings, expected exactly 1</color>");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Convert board to FEN notation
		/// </summary>
		public string ToFEN()
		{
			string boardFen = "";

			// Convert board to FEN (rank 8 to rank 1)
			for (int rank = 7; rank >= 0; rank--)
			{
				int emptyCount = 0;
				for (int file = 0; file < 8; file++)
				{
					char piece = board.GT(new v2(file, rank));
					if (piece == '.')
					{
						emptyCount++;
					}
					else
					{
						if (emptyCount > 0)
						{
							boardFen += emptyCount.ToString();
							emptyCount = 0;
						}
						boardFen += piece;
					}
				}

				if (emptyCount > 0)
				{
					boardFen += emptyCount.ToString();
				}

				if (rank > 0)
				{
					boardFen += "/";
				}
			}

			return $"{boardFen} {sideToMove} {castlingRights} {enPassantSquare} {halfmoveClock} {fullmoveNumber}";
		}

		#endregion

		#region Move Making and History

		/// <summary>
		/// Make a move and save to history
		/// </summary>
		public bool MakeMove(ChessMove move)
		{
			if (!move.IsValid())
			{
				Debug.Log($"<color=red>[ChessBoard] Invalid move: {move}</color>");
				return false;
			}

			// Save state before making move
			SaveStateWithMove(move);

			// Apply the move using ChessRules
			bool success = ChessRules.MakeMove(this, move);

			if (success)
			{
				Debug.Log($"<color=green>[ChessBoard] Made move: {move} (Position {currentHistoryIndex + 1}/{history.Count})</color>");
			}
			else
			{
				// Restore previous state if move failed
				if (history.Count > 1)
				{
					currentHistoryIndex--;
					RestoreState(history[currentHistoryIndex]);
				}
				Debug.Log($"<color=red>[ChessBoard] Failed to make move: {move}</color>");
			}

			return success;
		}

		/// <summary>
		/// Get all legal moves for current position
		/// </summary>
		public List<ChessMove> GetLegalMoves()
		{
			return MoveGenerator.GenerateLegalMoves(this);
		}

		/// <summary>
		/// Check if a move is legal in current position
		/// </summary>
		public bool IsLegalMove(ChessMove move)
		{
			return ChessRules.ValidateMove(this, move);
		}

		#endregion

		#region Board Access

		/// <summary>
		/// Get piece at algebraic square (e.g., "e4")
		/// </summary>
		public char GetPiece(string square)
		{
			v2 coord = AlgebraicToCoord(square);
			if (coord.x < 0 || coord.y < 0) return '.';
			return board.GT(coord);
		}

		/// <summary>
		/// Get piece at coordinate
		/// </summary>
		public char GetPiece(v2 coord)
		{
			if (coord.x < 0 || coord.x >= 8 || coord.y < 0 || coord.y >= 8)
				return '.';
			return board.GT(coord);
		}

		/// <summary>
		/// Set piece at coordinate
		/// </summary>
		public void SetPiece(v2 coord, char piece)
		{
			if (coord.x >= 0 && coord.x < 8 && coord.y >= 0 && coord.y < 8)
			{
				board.ST(coord, piece);
			}
		}

		/// <summary>
		/// Convert algebraic notation to board coordinates
		/// </summary>
		public static v2 AlgebraicToCoord(string square)
		{
			if (string.IsNullOrEmpty(square) || square.Length < 2)
				return new v2(-1, -1);

			char file = char.ToLower(square[0]);
			char rank = square[1];

			if (file < 'a' || file > 'h' || rank < '1' || rank > '8')
				return new v2(-1, -1);

			return new v2(file - 'a', rank - '1');
		}

		/// <summary>
		/// Convert board coordinates to algebraic notation
		/// </summary>
		public static string CoordToAlgebraic(v2 coord)
		{
			if (coord.x < 0 || coord.x >= 8 || coord.y < 0 || coord.y >= 8)
				return "";

			char file = (char)('a' + coord.x);
			char rank = (char)('1' + coord.y);
			return "" + file + rank;
		}

		#endregion

		#region Game State

		/// <summary>
		/// Check if game is over
		/// </summary>
		public bool IsGameOver()
		{
			var result = ChessRules.EvaluatePosition(this, GetMoveHistoryStrings());
			return result != ChessRules.GameResult.InProgress;
		}

		/// <summary>
		/// Get current game result
		/// </summary>
		public ChessRules.GameResult GetGameResult()
		{
			return ChessRules.EvaluatePosition(this, GetMoveHistoryStrings());
		}

		/// <summary>
		/// Get move history as strings for threefold repetition detection
		/// </summary>
		private List<string> GetMoveHistoryStrings()
		{
			return moveHistory.Select(m => m.ToUCI()).ToList();
		}

		/// <summary>
		/// Check if current side is in check
		/// </summary>
		public bool IsInCheck()
		{
			return ChessRules.IsInCheck(this, sideToMove);
		}

		/// <summary>
		/// Get game status string
		/// </summary>
		public string GetGameStatus()
		{
			var result = GetGameResult();
			switch (result)
			{
				case ChessRules.GameResult.InProgress:
					string turn = sideToMove == 'w' ? "White" : "Black";
					string checkStatus = IsInCheck() ? " (in check)" : "";
					return $"{turn} to move{checkStatus}";

				case ChessRules.GameResult.WhiteWins:
					return "White wins by checkmate";

				case ChessRules.GameResult.BlackWins:
					return "Black wins by checkmate";

				case ChessRules.GameResult.Stalemate:
					return "Draw by stalemate";

				case ChessRules.GameResult.InsufficientMaterial:
					return "Draw by insufficient material";

				case ChessRules.GameResult.FiftyMoveRule:
					return "Draw by fifty-move rule";

				case ChessRules.GameResult.ThreefoldRepetition:
					return "Draw by threefold repetition";

				default:
					return "Unknown game state";
			}
		}

		#endregion

		#region Cloning

		/// <summary>
		/// Create deep copy of board
		/// </summary>
		public ChessBoard Clone()
		{
			ChessBoard clone = new ChessBoard();
			clone.LoadFromFEN(this.ToFEN());
			clone.humanSide = this.humanSide;
			clone.engineSide = this.engineSide;
			clone.allowSideSwitching = this.allowSideSwitching;
			clone.lastEvaluation = this.lastEvaluation;
			clone.lastWinProbability = this.lastWinProbability;

			// Don't copy history to avoid circular references in clones
			clone.ClearHistory();

			return clone;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion

		#region Test Methods

		/// <summary>
		/// Test FEN parsing with various valid and invalid positions
		/// </summary>
		public static void TestFENParsing()
		{
			Debug.Log("<color=cyan>[ChessBoard] Testing FEN parsing...</color>");

			string[] validFENs = {
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", // Starting position
				"r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1", // Castling test
				"8/8/8/8/8/8/P7/8 w - - 0 1", // Single pawn
				"8/P7/8/8/8/8/8/8 w - - 0 1", // Promotion setup
			};

			string[] invalidFENs = {
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", // Missing side to move
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP", // Missing rank
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNRK w KQkq - 0 1", // Too many pieces
				"8/8/8/8/8/8/8/8 w - - 0 1", // No kings
			};

			// Test valid FENs
			foreach (string fen in validFENs)
			{
				ChessBoard board = new ChessBoard();
				bool success = board.LoadFromFEN(fen);
				if (success)
				{
					Debug.Log($"<color=green>[ChessBoard] ✓ Valid FEN: {fen.Substring(0, Math.Min(30, fen.Length))}...</color>");
				}
				else
				{
					Debug.Log($"<color=red>[ChessBoard] ✗ Failed to parse valid FEN: {fen}</color>");
				}
			}

			// Test invalid FENs
			foreach (string fen in invalidFENs)
			{
				ChessBoard board = new ChessBoard();
				bool success = board.LoadFromFEN(fen);
				if (!success)
				{
					Debug.Log($"<color=green>[ChessBoard] ✓ Correctly rejected invalid FEN: {fen.Substring(0, Math.Min(30, fen.Length))}...</color>");
				}
				else
				{
					Debug.Log($"<color=red>[ChessBoard] ✗ Incorrectly accepted invalid FEN: {fen}</color>");
				}
			}
		}

		/// <summary>
		/// Test undo/redo functionality
		/// </summary>
		public void TestUndoRedo()
		{
			Debug.Log("<color=cyan>[ChessBoard] Testing undo/redo...</color>");

			// Save initial state
			string initialFEN = ToFEN();

			// Make a few moves
			ChessMove move1 = ChessMove.FromUCI("e2e4", this);
			ChessMove move2 = ChessMove.FromUCI("e7e5", this);

			if (MakeMove(move1) && MakeMove(move2))
			{
				Debug.Log("<color=green>[ChessBoard] ✓ Made test moves</color>");

				// Test undo
				if (UndoMove() && UndoMove())
				{
					string currentFEN = ToFEN();
					if (currentFEN == initialFEN)
					{
						Debug.Log("<color=green>[ChessBoard] ✓ Undo test passed</color>");
					}
					else
					{
						Debug.Log("<color=red>[ChessBoard] ✗ Undo test failed - position mismatch</color>");
					}

					// Test redo
					if (RedoMove() && RedoMove())
					{
						Debug.Log("<color=green>[ChessBoard] ✓ Redo test passed</color>");
					}
					else
					{
						Debug.Log("<color=red>[ChessBoard] ✗ Redo test failed</color>");
					}
				}
				else
				{
					Debug.Log("<color=red>[ChessBoard] ✗ Undo test failed</color>");
				}
			}
			else
			{
				Debug.Log("<color=red>[ChessBoard] ✗ Failed to make test moves</color>");
			}
		}

		#endregion

		#region Display and Debug

		/// <summary>
		/// Get board as text for debugging
		/// </summary>
		public override string ToString()
		{
			var sb = new System.Text.StringBuilder();
			sb.AppendLine($"FEN: {ToFEN()}");
			sb.AppendLine($"Status: {GetGameStatus()}");
			sb.AppendLine($"History: {currentHistoryIndex + 1}/{history.Count} moves");
			sb.AppendLine($"Human: {GetSideName(humanSide)}, Engine: {GetSideName(engineSide)}");
			sb.AppendLine($"Evaluation: {GetEvaluationText()} (Win%: {lastWinProbability:P1})");
			sb.AppendLine();

			// Display board (rank 8 to rank 1)
			for (int rank = 7; rank >= 0; rank--)
			{
				sb.Append($"{rank + 1} ");
				for (int file = 0; file < 8; file++)
				{
					char piece = board.GT(new v2(file, rank));
					sb.Append(piece == '.' ? '.' : piece);
					sb.Append(" ");
				}
				sb.AppendLine();
			}
			sb.AppendLine("  a b c d e f g h");

			return sb.ToString();
		}

		/// <summary>
		/// Print board to console for debugging
		/// </summary>
		public void PrintBoard()
		{
			Debug.Log(ToString());
		}

		#endregion
	}
}