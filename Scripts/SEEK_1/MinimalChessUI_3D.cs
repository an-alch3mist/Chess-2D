/*
CHANGELOG (Modified from MinimalChessUI):
- Added use3D toggle to enable/disable 3D board functionality
- Conditional 3D board integration only when toggle is enabled
- Preserved all original MinimalChessUI functionality for 2D mode
- Added public engine settings access for 3D UI components
- Enhanced with 3D board synchronization and move coordination
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
	/// Enhanced minimal chess UI with optional 3D board integration.
	/// Maintains full compatibility with original 2D interface while adding 3D support.
	/// </summary>
	public class MinimalChessUI_3D : MonoBehaviour
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
		[SerializeField] private int engineElo = -1;
		[SerializeField] private int engineSkill = 0;
		[SerializeField] private bool humanPlaysWhite = true;
		[SerializeField] private bool enableEngineEvaluation = true;

		[Header("3D Integration")]
		[SerializeField] private bool use3D = false;
		[SerializeField] private ChessBoard3D chessBoard3D;
		[SerializeField] private EnhancedChessUI3D enhancedUI;

		[Header("Game State")]
		[SerializeField] private bool gameActive = true;
		[SerializeField] private string currentGameResult = "In Progress";

		private ChessBoard currentBoard;
		private List<ChessMove> moveHistory = new List<ChessMove>();
		private List<string> moveHistoryStrings = new List<string>();
		public bool waitingForEngine = false;

		// Public accessors for engine settings (for 3D UI integration)
		public int EngineDepth => engineDepth;
		public int EngineElo => engineElo;
		public int EngineSkill => engineSkill;
		public bool HumanPlaysWhite => humanPlaysWhite;
		public bool Use3D => use3D;

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

		private void Initialize3DIntegration()
		{
			if (!use3D) return;

			if (chessBoard3D != null)
			{
				chessBoard3D.SetPosition(currentBoard.ToFEN());
			}

			if (enhancedUI != null)
			{
				enhancedUI.UpdatePositionInfo(currentBoard.ToFEN());
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
			Initialize3DIntegration();

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

				// 3D Integration - notify enhanced UI
				if (use3D && enhancedUI != null)
				{
					enhancedUI.OnHumanMove(moveString);
					enhancedUI.UpdatePositionInfo(currentBoard.ToFEN());
				}

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

		[SerializeField] float engineWaitBeforeMove = 1f;
		private IEnumerator MakeEngineMove()
		{
			if (!gameActive || waitingForEngine)
				yield break;

			waitingForEngine = true;
			SetStatusMessage("Engine thinking...");

			// 3D Integration - notify enhanced UI
			if (use3D && enhancedUI != null)
			{
				enhancedUI.OnEngineThinking(true);
			}

			yield return new WaitForSeconds(this.engineWaitBeforeMove);

			// Configure engine settings from 3D UI if available
			int currentDepth = engineDepth;
			int currentElo = engineElo;
			int currentSkill = engineSkill;

			if (use3D && enhancedUI != null)
			{
				currentDepth = enhancedUI.GetEngineDepth();
				currentElo = enhancedUI.GetEngineElo();
				currentSkill = enhancedUI.GetEngineSkill();
			}

			// Configure engine settings
			if (stockfishBridge != null)
			{
				yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(
					currentBoard.ToFEN(),
					-1, // Use depth instead of time
					currentDepth,
					enableEngineEvaluation ? currentDepth : -1,
					currentElo,
					currentSkill
				));

				var result = stockfishBridge.LastAnalysisResult;

				// 3D Integration - stop thinking indicator
				if (use3D && enhancedUI != null)
				{
					enhancedUI.OnEngineThinking(false);
				}

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
						SetStatusMessage($"Engine played: {result.bestMove}{evalText}, with approx engine elo: {result.approximateElo}");

						UpdateDisplay();

						// 3D Integration - update 3D board and UI
						if (use3D)
						{
							if (chessBoard3D != null)
							{
								chessBoard3D.SetPosition(currentBoard.ToFEN());
							}

							if (enhancedUI != null)
							{
								enhancedUI.OnEngineMove(result.bestMove, result.evaluation);
								enhancedUI.UpdatePositionInfo(currentBoard.ToFEN());
							}
						}

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
			info += $"3D Mode: {(use3D ? "Enabled" : "Disabled")}\n";
			info += $"Engine Elo: {engineElo}";

			if (!string.IsNullOrEmpty(currentBoard.enPassantSquare) && currentBoard.enPassantSquare != "-")
			{
				info += $"\nEn passant: {currentBoard.enPassantSquare}";
			}

			if (!string.IsNullOrEmpty(currentBoard.castlingRights) && currentBoard.castlingRights != "-")
			{
				info += $"\nCastling: {currentBoard.castlingRights}";
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

			// 3D Integration - sync board state
			if (use3D && chessBoard3D != null)
			{
				chessBoard3D.SetPosition(currentBoard.ToFEN());
			}

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
			Initialize3DIntegration();
		}

		/// <summary>
		/// Get current FEN position
		/// </summary>
		public string GetCurrentFEN()
		{
			return currentBoard?.ToFEN() ?? "";
		}

		/// <summary>
		/// Get current board state
		/// </summary>
		public ChessBoard GetCurrentBoard()
		{
			return currentBoard;
		}

		/// <summary>
		/// Configure engine settings (for 3D UI integration)
		/// </summary>
		public void SetEngineSettings(int depth, int elo, int skill)
		{
			engineDepth = depth;
			engineElo = elo;
			engineSkill = skill;
		}

		/// <summary>
		/// Set which side human plays (for 3D UI integration)
		/// </summary>
		public void SetHumanSide(bool playsWhite)
		{
			humanPlaysWhite = playsWhite;
		}

		/// <summary>
		/// Toggle 3D mode on/off
		/// </summary>
		public void SetUse3D(bool enable3D)
		{
			use3D = enable3D;

			if (use3D)
			{
				Initialize3DIntegration();
			}

			UpdateDisplay(); // Refresh UI to show 3D mode status
		}

		/// <summary>
		/// Process move from 3D board (called by ChessBoard3D)
		/// </summary>
		public bool ProcessMoveFrom3D(ChessMove move, string moveString)
		{
			if (!gameActive || waitingForEngine || !IsHumanTurn())
				return false;

			if (ChessRules.ValidateMove(currentBoard, move))
			{
				if (ChessRules.MakeMove(currentBoard, move))
				{
					moveHistory.Add(move);
					moveHistoryStrings.Add(moveString);

					SetStatusMessage($"3D Move played: {moveString}");
					UpdateDisplay();

					// Notify enhanced UI
					if (enhancedUI != null)
					{
						enhancedUI.OnHumanMove(moveString);
						enhancedUI.UpdatePositionInfo(currentBoard.ToFEN());
					}

					CheckGameState();

					// If game is still active and it's engine's turn, make engine move
					if (gameActive && !IsHumanTurn())
					{
						StartCoroutine(MakeEngineMove());
					}

					return true;
				}
			}

			return false;
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