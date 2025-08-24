/*
CHANGELOG (Updated):
- Removed rich text formatting from TMP_InputField components
- Fixed castling move validation to work with both standard chess and Chess960
- Improved error messages for better debugging
- Enhanced move parsing to handle algebraic castling notation (e1g1, e1c1)
*/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GPTDeepResearch
{
	/// <summary>
	/// Minimal chess UI for testing and demonstrating the complete chess system.
	/// Provides human vs engine gameplay with ASCII board representation.
	/// </summary>
	public class MinimalChessUI : MonoBehaviour
	{
		[Header("UI References")]
		[SerializeField] private TMP_InputField boardDisplay;
		[SerializeField] private TMP_InputField moveInput;
		[SerializeField] private Button makeMoveButton;
		[SerializeField] private Button newGameButton;
		[SerializeField] private Button undoMoveButton;
		[SerializeField] private TMP_InputField gameStatusText;
		[SerializeField] private TMP_InputField moveHistoryText;

		[Header("Engine Settings")]
		[SerializeField] private StockfishBridge stockfishBridge;
		[SerializeField] private int engineDepth = 3;
		[SerializeField] private int engineElo = 1200;
		[SerializeField] private bool humanPlaysWhite = true;
		[SerializeField] private bool enableEngineEvaluation = true;

		[Header("Game State")]
		[SerializeField] private bool gameActive = true;
		[SerializeField] private string currentGameResult = "In Progress";

		private ChessBoard currentBoard;
		private List<ChessMove> moveHistory = new List<ChessMove>();
		private List<string> moveHistoryStrings = new List<string>();
		private bool waitingForEngine = false;

		void Start()
		{
			InitializeUI();
			StartNewGame();
		}

		void InitializeUI()
		{
			if (makeMoveButton != null)
				makeMoveButton.onClick.AddListener(OnMakeMoveClicked);

			if (newGameButton != null)
				newGameButton.onClick.AddListener(OnNewGameClicked);

			if (undoMoveButton != null)
				undoMoveButton.onClick.AddListener(OnUndoMoveClicked);

			if (moveInput != null)
			{
				moveInput.onSubmit.AddListener(OnMoveInputSubmit);
				moveInput.onEndEdit.AddListener(OnMoveInputSubmit);
			}

			// Ensure engine is running
			if (stockfishBridge != null)
			{
				stockfishBridge.enableEvaluation = enableEngineEvaluation;
				if (!stockfishBridge.IsEngineRunning)
				{
					stockfishBridge.StartEngine();
				}
			}
		}

		public void StartNewGame()
		{
			currentBoard = new ChessBoard(); // Start with standard position
			moveHistory.Clear();
			moveHistoryStrings.Clear();
			gameActive = true;
			waitingForEngine = false;
			currentGameResult = "In Progress";

			UpdateDisplay();

			// If engine plays white, make first move
			if (!humanPlaysWhite && gameActive)
			{
				StartCoroutine(MakeEngineMove());
			}
		}

		void Update()
		{
			// Update UI states
			if (makeMoveButton != null)
				makeMoveButton.interactable = gameActive && !waitingForEngine && IsHumanTurn();

			if (undoMoveButton != null)
				undoMoveButton.interactable = gameActive && !waitingForEngine && moveHistory.Count > 0;

			if (moveInput != null)
				moveInput.interactable = gameActive && !waitingForEngine && IsHumanTurn();
		}

		#region Move Handling

		private void OnMoveInputSubmit(string moveString)
		{
			if (!gameActive || waitingForEngine || !IsHumanTurn())
				return;

			ProcessHumanMove(moveString.Trim());
		}

		private void OnMakeMoveClicked()
		{
			if (moveInput != null)
				ProcessHumanMove(moveInput.text.Trim());
		}

		private void ProcessHumanMove(string moveString)
		{
			if (string.IsNullOrEmpty(moveString))
				return;

			// Parse and validate move
			ChessMove move = ParseMoveInput(moveString);
			if (!move.IsValid())
			{
				SetStatusMessage($"Invalid move format: {moveString}");
				return;
			}

			// Check if move is legal
			if (!ChessRules.ValidateMove(currentBoard, move))
			{
				SetStatusMessage($"Illegal move: {moveString}");

				// Debug information for castling moves
				if (move.moveType == ChessMove.MoveType.Castling)
				{
					Debug.Log($"[ChessUI] Castling move details - From: {ChessBoard.CoordToAlgebraic(move.from)}, To: {ChessBoard.CoordToAlgebraic(move.to)}");
					Debug.Log($"[ChessUI] Rook From: {ChessBoard.CoordToAlgebraic(move.rookFrom)}, Rook To: {ChessBoard.CoordToAlgebraic(move.rookTo)}");
					Debug.Log($"[ChessUI] Castling Rights: {currentBoard.castlingRights}");
					Debug.Log($"[ChessUI] King in check: {ChessRules.IsInCheck(currentBoard, currentBoard.sideToMove)}");
				}

				return;
			}

			// Make the move
			if (ChessRules.MakeMove(currentBoard, move))
			{
				moveHistory.Add(move);
				moveHistoryStrings.Add(moveString);

				SetStatusMessage($"Move played: {moveString}");

				if (moveInput != null)
					moveInput.text = "";

				UpdateDisplay();
				CheckGameState();

				// If game is still active and it's engine's turn, make engine move
				if (gameActive && !IsHumanTurn())
				{
					StartCoroutine(MakeEngineMove());
				}
			}
			else
			{
				SetStatusMessage($"Failed to make move: {moveString}");
			}
		}

		private ChessMove ParseMoveInput(string input)
		{
			// Handle different move input formats
			input = input.Trim().Replace(" ", "");

			// Try parsing as long algebraic notation first
			ChessMove move = ChessMove.FromLongAlgebraic(input, currentBoard);
			if (move.IsValid())
				return move;

			// TODO: Add support for short algebraic notation (Nf3, Bxe5, etc.)
			// For now, only support long algebraic notation and castling

			return new ChessMove(); // Invalid move
		}


		[SerializeField] float engineWaitBeforeMove = 0.5f;
		private IEnumerator MakeEngineMove()
		{
			if (!gameActive || waitingForEngine)
				yield break;

			waitingForEngine = true;
			SetStatusMessage("Engine thinking...");

			yield return new WaitForSeconds(this.engineWaitBeforeMove);
			// Configure engine settings
			if (stockfishBridge != null)
			{
				yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(
					currentBoard.ToFEN(),
					-1, // Use depth instead of time
					engineDepth,
					enableEngineEvaluation ? engineDepth : -1,
					engineElo,
					-1 // No skill level limitation
				));

				var result = stockfishBridge.LastAnalysisResult;

				if (!string.IsNullOrEmpty(result.errorMessage))
				{
					SetStatusMessage($"Engine error: {result.errorMessage}");
					waitingForEngine = false;
					yield break;
				}

				if (result.bestMove == "check-mate")
				{
					gameActive = false;
					currentGameResult = humanPlaysWhite ? "Black wins by checkmate" : "White wins by checkmate";
					SetStatusMessage($"{currentGameResult}");
				}
				else if (result.bestMove == "stale-mate")
				{
					gameActive = false;
					currentGameResult = "Draw by stalemate";
					SetStatusMessage($"{currentGameResult}");
				}
				else if (result.bestMove.StartsWith("ERROR"))
				{
					SetStatusMessage($"{result.bestMove}");
				}
				else
				{
					// Parse and make engine move
					ChessMove engineMove = ChessMove.FromLongAlgebraic(result.bestMove, currentBoard);

					if (engineMove.IsValid() && ChessRules.ValidateMove(currentBoard, engineMove))
					{
						ChessRules.MakeMove(currentBoard, engineMove);
						moveHistory.Add(engineMove);
						moveHistoryStrings.Add(result.bestMove);

						string evalText = enableEngineEvaluation ?
							$" (Eval: {result.stmEvaluation:P1})" : "";
						SetStatusMessage($"Engine played: {result.bestMove}{evalText}");

						UpdateDisplay();
						CheckGameState();
					}
					else
					{
						SetStatusMessage($"Engine returned invalid move: {result.bestMove}");
					}
				}
			}
			else
			{
				SetStatusMessage("No engine available");
			}

			waitingForEngine = false;
		}

		#endregion

		#region Game State Management

		private void CheckGameState()
		{
			if (!gameActive) return;

			ChessRules.GameResult result = ChessRules.EvaluatePosition(currentBoard, moveHistoryStrings);

			switch (result)
			{
				case ChessRules.GameResult.WhiteWins:
					gameActive = false;
					currentGameResult = "White wins by checkmate";
					SetStatusMessage($"{currentGameResult}");
					break;

				case ChessRules.GameResult.BlackWins:
					gameActive = false;
					currentGameResult = "Black wins by checkmate";
					SetStatusMessage($"{currentGameResult}");
					break;

				case ChessRules.GameResult.Stalemate:
					gameActive = false;
					currentGameResult = "Draw by stalemate";
					SetStatusMessage($"{currentGameResult}");
					break;

				case ChessRules.GameResult.InsufficientMaterial:
					gameActive = false;
					currentGameResult = "Draw by insufficient material";
					SetStatusMessage($"{currentGameResult}");
					break;

				case ChessRules.GameResult.FiftyMoveRule:
					gameActive = false;
					currentGameResult = "Draw by fifty-move rule";
					SetStatusMessage($"{currentGameResult}");
					break;

				case ChessRules.GameResult.ThreefoldRepetition:
					gameActive = false;
					currentGameResult = "Draw by threefold repetition";
					SetStatusMessage($"{currentGameResult}");
					break;

				case ChessRules.GameResult.InProgress:
					// Check for check
					if (ChessRules.IsInCheck(currentBoard, currentBoard.sideToMove))
					{
						string sideInCheck = currentBoard.sideToMove == 'w' ? "White" : "Black";
						SetStatusMessage($"{sideInCheck} is in check!");
					}
					break;
			}
		}

		private bool IsHumanTurn()
		{
			return (humanPlaysWhite && currentBoard.sideToMove == 'w') ||
				   (!humanPlaysWhite && currentBoard.sideToMove == 'b');
		}

		#endregion

		#region UI Updates

		private void UpdateDisplay()
		{
			UpdateBoardDisplay();
			UpdateMoveHistory();
			UpdateGameInfo();
		}

		private void UpdateBoardDisplay()
		{
			if (boardDisplay == null) return;

			string display = GenerateASCIIBoard();
			boardDisplay.text = display;
		}

		private string GenerateASCIIBoard()
		{
			string board = "";

			// Add column labels
			board += "   a b c d e f g h\n";

			// Add rows from rank 8 to rank 1
			for (int rank = 7; rank >= 0; rank--)
			{
				board += $"{rank + 1}  ";

				for (int file = 0; file < 8; file++)
				{
					char piece = currentBoard.board.GT(new SPACE_UTIL.v2(file, rank));
					string pieceStr = piece == '.' ? "." : piece.ToString();
					board += pieceStr + " ";
				}

				board += $" {rank + 1}\n";
			}

			// Add column labels again
			board += "   a b c d e f g h\n";

			return board;
		}

		private void UpdateMoveHistory()
		{
			if (moveHistoryText == null) return;

			string history = "Move History:\n";

			for (int i = 0; i < moveHistoryStrings.Count; i++)
			{
				int moveNumber = (i / 2) + 1;

				if (i % 2 == 0)
				{
					history += $"{moveNumber}. {moveHistoryStrings[i]}";
				}
				else
				{
					history += $" {moveHistoryStrings[i]}\n";
				}
			}

			moveHistoryText.text = history;
		}

		private void UpdateGameInfo()
		{
			if (gameStatusText == null) return;

			string info = $"Game Status: {currentGameResult}\n";
			info += $"To Move: {(currentBoard.sideToMove == 'w' ? "White" : "Black")}\n";
			info += $"Move #{currentBoard.fullmoveNumber}\n";
			info += $"50-move clock: {currentBoard.halfmoveClock}\n";

			if (!string.IsNullOrEmpty(currentBoard.enPassantSquare) && currentBoard.enPassantSquare != "-")
			{
				info += $"En passant: {currentBoard.enPassantSquare}\n";
			}

			if (!string.IsNullOrEmpty(currentBoard.castlingRights) && currentBoard.castlingRights != "-")
			{
				info += $"Castling: {currentBoard.castlingRights}\n";
			}

			gameStatusText.text = info;
		}

		private void SetStatusMessage(string message)
		{
			Debug.Log($"[ChessUI] {message}");
			// Could also display in a separate status text field if desired
		}

		#endregion

		#region Button Handlers

		private void OnNewGameClicked()
		{
			StartNewGame();
		}

		private void OnUndoMoveClicked()
		{
			if (moveHistory.Count == 0 || !gameActive || waitingForEngine)
				return;

			// Simple undo - just restart and replay moves except the last one
			List<ChessMove> movesToReplay = new List<ChessMove>(moveHistory);
			List<string> stringsToReplay = new List<string>(moveHistoryStrings);

			// Remove last move(s) - if playing against engine, remove both human and engine moves
			int movesToRemove = IsHumanTurn() ? 1 : 2;
			movesToRemove = Mathf.Min(movesToRemove, movesToReplay.Count);

			for (int i = 0; i < movesToRemove; i++)
			{
				movesToReplay.RemoveAt(movesToReplay.Count - 1);
				stringsToReplay.RemoveAt(stringsToReplay.Count - 1);
			}

			// Restart game and replay moves
			StartNewGame();

			foreach (var move in movesToReplay)
			{
				ChessRules.MakeMove(currentBoard, move);
			}

			moveHistory = movesToReplay;
			moveHistoryStrings = stringsToReplay;

			UpdateDisplay();
			SetStatusMessage("Move undone");
		}

		#endregion

		#region Public Interface

		/// <summary>
		/// Set up a specific position for testing
		/// </summary>
		public void SetPosition(string fen)
		{
			currentBoard = new ChessBoard(fen);
			moveHistory.Clear();
			moveHistoryStrings.Clear();
			gameActive = true;
			waitingForEngine = false;
			currentGameResult = "In Progress";

			UpdateDisplay();
		}

		/// <summary>
		/// Get current FEN position
		/// </summary>
		public string GetCurrentFEN()
		{
			return currentBoard?.ToFEN() ?? "";
		}

		/// <summary>
		/// Force engine to make a move (for testing)
		/// </summary>
		[ContextMenu("Force Engine Move")]
		public void ForceEngineMove()
		{
			if (gameActive && !waitingForEngine)
			{
				StartCoroutine(MakeEngineMove());
			}
		}

		#endregion
	}
}