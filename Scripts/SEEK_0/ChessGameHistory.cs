/*
CHESS GAME HISTORY SYSTEM
=========================

Features:
- Complete undo/redo functionality with board state restoration
- Move notation history (SAN and UCI formats)
- FEN history for each position
- Captured pieces tracking
- Side selection support (play as white/black)
- Integration with promotion system
- Memory efficient with configurable limits
*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace GPTDeepResearch
{
	/// <summary>
	/// Manages chess game history with undo/redo functionality
	/// Tracks moves, board states, and captured pieces
	/// </summary>
	public class ChessGameHistory : MonoBehaviour
	{
		[Header("Configuration")]
		[SerializeField] private int maxHistorySize = 500;
		[SerializeField] private bool enableDebugLogging = true;

		[Header("Player Settings")]
		[SerializeField] private PlayerSide humanPlayer = PlayerSide.White;
		[SerializeField] private bool allowSideChange = true;

		public enum PlayerSide
		{
			White,
			Black,
			Both // For analysis mode
		}

		/// <summary>
		/// Represents a single move in game history
		/// </summary>
		[System.Serializable]
		public class HistoryEntry
		{
			public ChessMove move;                    // The move that was played
			public string fenBefore;                  // Board state before the move
			public string fenAfter;                   // Board state after the move
			public string sanNotation;                // Move in Standard Algebraic Notation
			public string uciNotation;                // Move in UCI notation
			public List<char> capturedPieces;        // All pieces captured up to this point
			public int moveNumber;                    // Full move number
			public char sideToMove;                   // Who played this move
			public DateTime timestamp;                // When the move was played
			public float evaluationBefore;            // Position evaluation before move
			public float evaluationAfter;             // Position evaluation after move

			public HistoryEntry()
			{
				capturedPieces = new List<char>();
				timestamp = DateTime.Now;
			}

			public override string ToString()
			{
				string moveDisplay = !string.IsNullOrEmpty(sanNotation) ? sanNotation : uciNotation;
				return $"{moveNumber}{(sideToMove == 'w' ? '.' : '_')} {moveDisplay}";
			}
		}

		// Events
		[System.Serializable]
		public class HistoryChangedEvent : UnityEvent<int, int> { } // (currentIndex, totalMoves)
		public HistoryChangedEvent OnHistoryChanged = new HistoryChangedEvent();

		[System.Serializable]
		public class SideChangedEvent : UnityEvent<PlayerSide> { }
		public SideChangedEvent OnSideChanged = new SideChangedEvent();

		// History data
		private List<HistoryEntry> gameHistory = new List<HistoryEntry>();
		private int currentHistoryIndex = -1; // -1 means at start position
		private string startingFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

		// Captured pieces tracking
		private List<char> allCapturedPieces = new List<char>();

		#region Public API

		/// <summary>
		/// Initialize history with starting position
		/// </summary>
		public void InitializeGame(string startFen = null)
		{
			if (!string.IsNullOrEmpty(startFen))
				startingFen = startFen;

			gameHistory.Clear();
			currentHistoryIndex = -1;
			allCapturedPieces.Clear();

			OnHistoryChanged?.Invoke(currentHistoryIndex, gameHistory.Count);

			if (enableDebugLogging)
				Debug.Log($"<color=green>[History] Game initialized with starting FEN: {startingFen}</color>");
		}

		/// <summary>
		/// Add a move to history
		/// </summary>
		public void AddMove(ChessMove move, string fenBefore, string fenAfter, float evalBefore = 0.5f, float evalAfter = 0.5f)
		{
			// If we're not at the end of history, truncate future moves
			if (currentHistoryIndex < gameHistory.Count - 1)
			{
				int removeCount = gameHistory.Count - (currentHistoryIndex + 1);
				gameHistory.RemoveRange(currentHistoryIndex + 1, removeCount);

				if (enableDebugLogging)
					Debug.Log($"<color=yellow>[History] Truncated {removeCount} future moves</color>");
			}

			// Create history entry
			HistoryEntry entry = new HistoryEntry
			{
				move = move,
				fenBefore = fenBefore,
				fenAfter = fenAfter,
				uciNotation = move.ToUCI(),
				sideToMove = ExtractSideFromFen(fenBefore),
				evaluationBefore = evalBefore,
				evaluationAfter = evalAfter
			};

			// Calculate move number
			entry.moveNumber = (gameHistory.Count / 2) + 1;

			// Track captured pieces
			if (move.IsCapture())
			{
				char captured = move.capturedPiece;
				if (captured != '\0')
				{
					allCapturedPieces.Add(captured);
				}
			}
			entry.capturedPieces = new List<char>(allCapturedPieces);

			// Add to history
			gameHistory.Add(entry);
			currentHistoryIndex = gameHistory.Count - 1;

			// Enforce size limit
			if (gameHistory.Count > maxHistorySize)
			{
				gameHistory.RemoveAt(0);
				currentHistoryIndex--;
			}

			OnHistoryChanged?.Invoke(currentHistoryIndex, gameHistory.Count);

			if (enableDebugLogging)
				Debug.Log($"<color=green>[History] Added move: {entry}</color>");
		}

		/// <summary>
		/// Undo last move
		/// </summary>
		/// <returns>FEN string of position after undo, or null if can't undo</returns>
		public string UndoMove()
		{
			if (!CanUndo())
			{
				if (enableDebugLogging)
					Debug.Log("<color=yellow>[History] Cannot undo - at start position</color>");
				return null;
			}

			currentHistoryIndex--;

			// Update captured pieces
			UpdateCapturedPiecesForIndex();

			string resultFen = currentHistoryIndex >= 0 ?
				gameHistory[currentHistoryIndex].fenAfter : startingFen;

			OnHistoryChanged?.Invoke(currentHistoryIndex, gameHistory.Count);

			if (enableDebugLogging)
			{
				string moveUndone = currentHistoryIndex + 1 < gameHistory.Count ?
					gameHistory[currentHistoryIndex + 1].ToString() : "unknown";
				Debug.Log($"<color=cyan>[History] Undid move: {moveUndone}</color>");
			}

			return resultFen;
		}

		/// <summary>
		/// Redo next move
		/// </summary>
		/// <returns>FEN string of position after redo, or null if can't redo</returns>
		public string RedoMove()
		{
			if (!CanRedo())
			{
				if (enableDebugLogging)
					Debug.Log("<color=yellow>[History] Cannot redo - at latest position</color>");
				return null;
			}

			currentHistoryIndex++;

			// Update captured pieces
			UpdateCapturedPiecesForIndex();

			string resultFen = gameHistory[currentHistoryIndex].fenAfter;

			OnHistoryChanged?.Invoke(currentHistoryIndex, gameHistory.Count);

			if (enableDebugLogging)
			{
				Debug.Log($"<color=cyan>[History] Redid move: {gameHistory[currentHistoryIndex]}</color>");
			}

			return resultFen;
		}

		/// <summary>
		/// Change which side the human plays
		/// </summary>
		public void SetHumanSide(PlayerSide side)
		{
			if (!allowSideChange)
			{
				if (enableDebugLogging)
					Debug.Log("<color=yellow>[History] Side changes disabled</color>");
				return;
			}

			humanPlayer = side;
			OnSideChanged?.Invoke(side);

			if (enableDebugLogging)
				Debug.Log($"<color=cyan>[History] Human player changed to: {side}</color>");
		}

		/// <summary>
		/// Check if it's the human player's turn
		/// </summary>
		public bool IsHumanTurn(char sideToMove)
		{
			if (humanPlayer == PlayerSide.Both)
				return true; // Analysis mode - human controls both sides

			return (humanPlayer == PlayerSide.White && sideToMove == 'w') ||
				   (humanPlayer == PlayerSide.Black && sideToMove == 'b');
		}

		/// <summary>
		/// Get current position FEN
		/// </summary>
		public string GetCurrentFen()
		{
			if (currentHistoryIndex >= 0 && currentHistoryIndex < gameHistory.Count)
				return gameHistory[currentHistoryIndex].fenAfter;
			return startingFen;
		}

		/// <summary>
		/// Get move history in PGN format
		/// </summary>
		public string GetPGNMoves()
		{
			if (gameHistory.Count == 0)
				return "";

			var moves = new List<string>();
			for (int i = 0; i < gameHistory.Count; i++)
			{
				var entry = gameHistory[i];
				if (entry.sideToMove == 'w')
				{
					moves.Add($"{entry.moveNumber}. {entry.sanNotation}");
				}
				else
				{
					if (moves.Count > 0)
						moves[moves.Count - 1] += $" {entry.sanNotation}";
					else
						moves.Add($"{entry.moveNumber}... {entry.sanNotation}");
				}
			}

			return string.Join(" ", moves);
		}

		/// <summary>
		/// Get all captured pieces by side
		/// </summary>
		public (List<char> whiteCaptured, List<char> blackCaptured) GetCapturedPieces()
		{
			var whiteCaptured = allCapturedPieces.Where(p => char.IsLower(p)).ToList();
			var blackCaptured = allCapturedPieces.Where(p => char.IsUpper(p)).ToList();
			return (whiteCaptured, blackCaptured);
		}

		// Properties
		public bool CanUndo() => currentHistoryIndex >= 0;
		public bool CanRedo() => currentHistoryIndex < gameHistory.Count - 1;
		public int MoveCount => gameHistory.Count;
		public int CurrentMoveIndex => currentHistoryIndex;
		public PlayerSide HumanSide => humanPlayer;
		public List<HistoryEntry> History => new List<HistoryEntry>(gameHistory); // Read-only copy

		#endregion

		#region Private Methods

		private void UpdateCapturedPiecesForIndex()
		{
			allCapturedPieces.Clear();

			// Rebuild captured pieces list up to current index
			for (int i = 0; i <= currentHistoryIndex && i < gameHistory.Count; i++)
			{
				if (gameHistory[i].move.IsCapture() && gameHistory[i].move.capturedPiece != '\0')
				{
					allCapturedPieces.Add(gameHistory[i].move.capturedPiece);
				}
			}
		}

		private char ExtractSideFromFen(string fen)
		{
			if (string.IsNullOrEmpty(fen))
				return 'w';

			string[] parts = fen.Split(' ');
			if (parts.Length >= 2)
			{
				char side = parts[1].ToLower()[0];
				return (side == 'w' || side == 'b') ? side : 'w';
			}
			return 'w';
		}

		#endregion

		#region Unity Editor
#if UNITY_EDITOR
		private void OnValidate()
		{
			if (maxHistorySize <= 0)
				maxHistorySize = 500;
		}
#endif
		#endregion
	}

	/// <summary>
	/// Integration helper for connecting history with chess game
	/// </summary>
	public static class ChessGameHistoryHelper
	{
		/// <summary>
		/// Example integration with main game controller
		/// </summary>
		public static void ExampleUsage()
		{
			/*
			// In your main chess game script:
			
			[SerializeField] private ChessGameHistory gameHistory;
			[SerializeField] private ChessBoard currentBoard;
			[SerializeField] private StockfishBridge stockfish;
			
			// When human makes a move:
			private void OnHumanMove(ChessMove move)
			{
				string fenBefore = currentBoard.ToFEN();
				
				// Apply move to board
				ChessRules.MakeMove(currentBoard, move);
				
				string fenAfter = currentBoard.ToFEN();
				
				// Add to history
				gameHistory.AddMove(move, fenBefore, fenAfter);
				
				// Check if it's game end
				var gameResult = ChessRules.EvaluatePosition(currentBoard);
				if (gameResult != ChessRules.GameResult.InProgress)
				{
					HandleGameEnd(gameResult);
					return;
				}
				
				// If not human's turn, get engine move
				if (!gameHistory.IsHumanTurn(currentBoard.sideToMove))
				{
					StartCoroutine(GetEngineMove());
				}
			}
			
			// Undo/Redo buttons:
			public void OnUndoClicked()
			{
				string newFen = gameHistory.UndoMove();
				if (newFen != null)
				{
					currentBoard.LoadFromFEN(newFen);
					UpdateBoardVisuals();
				}
			}
			
			public void OnRedoClicked()
			{
				string newFen = gameHistory.RedoMove();
				if (newFen != null)
				{
					currentBoard.LoadFromFEN(newFen);
					UpdateBoardVisuals();
				}
			}
			
			// Side selection:
			public void SetPlayerSide(int sideIndex)
			{
				PlayerSide newSide = (PlayerSide)sideIndex;
				gameHistory.SetHumanSide(newSide);
			}
			
			*/
		}
	}
}