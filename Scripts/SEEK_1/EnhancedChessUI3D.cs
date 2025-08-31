/*
CHANGELOG (New File):
- Enhanced 3D chess UI with evaluation bars and move history
- Integration with existing MinimalChessUI for engine management
- Real-time position evaluation display
- Move history in algebraic notation
- Engine configuration controls
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GPTDeepResearch
{
	/// <summary>
	/// Enhanced 3D chess UI that extends MinimalChessUI with visual enhancements.
	/// Provides evaluation bars, move history, and engine configuration.
	/// </summary>
	public class EnhancedChessUI3D : MonoBehaviour
	{
		[Header("3D Board Reference")]
		[SerializeField] private ChessBoard3D chessBoard3D;
		[SerializeField] private MinimalChessUI baseChessUI;

		[Header("Evaluation UI")]
		[SerializeField] private Slider evaluationBar;
		[SerializeField] private TMP_Text evaluationText;
		[SerializeField] private TMP_Text positionText;

		[Header("Move History UI")]
		[SerializeField] private TMP_Text moveHistoryDisplay;
		[SerializeField] private ScrollRect moveHistoryScroll;

		[Header("Engine Configuration")]
		[SerializeField] private Slider depthSlider;
		[SerializeField] private TMP_Text depthLabel;
		[SerializeField] private Slider skillSlider;
		[SerializeField] private TMP_Text skillLabel;
		[SerializeField] private TMP_InputField eloInput;
		[SerializeField] private Toggle humanPlaysWhiteToggle;

		[Header("Game Controls")]
		[SerializeField] private Button newGameButton;
		[SerializeField] private Button undoButton;
		[SerializeField] private Button flipBoardButton;
		[SerializeField] private TMP_Text gameStatusText;

		// State
		private List<string> moveHistory = new List<string>();
		private float currentEvaluation = 0.5f;
		private bool isEngineThinking = false;

		void Start()
		{
			InitializeUI();
			SetupEventListeners();
		}

		void InitializeUI()
		{
			// Initialize evaluation bar
			if (evaluationBar != null)
			{
				evaluationBar.minValue = 0f;
				evaluationBar.maxValue = 1f;
				evaluationBar.value = 0.5f;
			}

			// Initialize engine configuration
			if (depthSlider != null)
			{
				depthSlider.minValue = 1;
				depthSlider.maxValue = 20;
				depthSlider.value = 5;
				UpdateDepthLabel();
			}

			if (skillSlider != null)
			{
				skillSlider.minValue = 0;
				skillSlider.maxValue = 20;
				skillSlider.value = 10;
				UpdateSkillLabel();
			}

			if (eloInput != null)
			{
				eloInput.text = "1500";
			}

			UpdateMoveHistory();
			UpdateGameStatus("New Game");
		}

		void SetupEventListeners()
		{
			// Engine configuration
			if (depthSlider != null)
				depthSlider.onValueChanged.AddListener(OnDepthChanged);

			if (skillSlider != null)
				skillSlider.onValueChanged.AddListener(OnSkillChanged);

			if (eloInput != null)
				eloInput.onEndEdit.AddListener(OnEloChanged);

			// Game controls
			if (newGameButton != null)
				newGameButton.onClick.AddListener(OnNewGame);

			if (undoButton != null)
				undoButton.onClick.AddListener(OnUndo);

			if (flipBoardButton != null)
				flipBoardButton.onClick.AddListener(OnFlipBoard);

			if (humanPlaysWhiteToggle != null)
				humanPlaysWhiteToggle.onValueChanged.AddListener(OnSideChanged);
		}

		#region Engine Configuration

		private void OnDepthChanged(float value)
		{
			UpdateDepthLabel();
			if (baseChessUI != null)
			{
				// Update base UI engine depth via reflection or public property
				// baseChessUI.engineDepth = (int)value;
			}
		}

		private void OnSkillChanged(float value)
		{
			UpdateSkillLabel();
			if (baseChessUI != null)
			{
				// Update base UI engine skill via reflection or public property
				// baseChessUI.engineSkill = (int)value;
			}
		}

		private void OnEloChanged(string value)
		{
			if (int.TryParse(value, out int elo))
			{
				if (baseChessUI != null)
				{
					// Update base UI engine Elo
					// baseChessUI.engineElo = elo;
				}
			}
		}

		private void OnSideChanged(bool humanPlaysWhite)
		{
			if (baseChessUI != null)
			{
				// Update base UI side setting
				// baseChessUI.humanPlaysWhite = humanPlaysWhite;
			}
		}

		private void UpdateDepthLabel()
		{
			if (depthLabel != null && depthSlider != null)
			{
				depthLabel.text = $"Depth: {(int)depthSlider.value}";
			}
		}

		private void UpdateSkillLabel()
		{
			if (skillLabel != null && skillSlider != null)
			{
				skillLabel.text = $"Skill: {(int)skillSlider.value}";
			}
		}

		#endregion

		#region Game Controls

		private void OnNewGame()
		{
			if (baseChessUI != null)
			{
				baseChessUI.StartNewGame();
			}

			if (chessBoard3D != null)
			{
				chessBoard3D.SetPosition("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
			}

			moveHistory.Clear();
			UpdateMoveHistory();
			UpdateEvaluation(0.5f);
			UpdateGameStatus("New Game");
		}

		private void OnUndo()
		{
			if (baseChessUI != null)
			{
				// Trigger undo in base UI
				// baseChessUI.OnUndoMoveClicked();
			}

			// Remove last move from history
			if (moveHistory.Count > 0)
			{
				moveHistory.RemoveAt(moveHistory.Count - 1);
				UpdateMoveHistory();
			}
		}

		private void OnFlipBoard()
		{
			if (chessBoard3D != null)
			{
				chessBoard3D.FlipBoard();
			}
		}

		#endregion

		#region UI Updates

		public void UpdateEvaluation(float whiteWinProbability)
		{
			currentEvaluation = whiteWinProbability;

			if (evaluationBar != null)
			{
				evaluationBar.value = whiteWinProbability;
			}

			if (evaluationText != null)
			{
				float advantage = (whiteWinProbability - 0.5f) * 2f; // Convert to -1 to +1
				string advantageText = advantage > 0 ? $"+{advantage:F2}" : $"{advantage:F2}";
				evaluationText.text = $"Eval: {advantageText}";
			}
		}

		public void AddMoveToHistory(string move, bool isWhiteMove)
		{
			if (isWhiteMove && moveHistory.Count % 2 == 0)
			{
				moveHistory.Add($"{(moveHistory.Count / 2) + 1}. {move}");
			}
			else
			{
				if (moveHistory.Count > 0)
					moveHistory[moveHistory.Count - 1] += $" {move}";
				else
					moveHistory.Add($"1... {move}");
			}

			UpdateMoveHistory();
		}

		private void UpdateMoveHistory()
		{
			if (moveHistoryDisplay == null) return;

			string historyText = "Move History:\n\n";
			foreach (string move in moveHistory)
			{
				historyText += move + "\n";
			}

			moveHistoryDisplay.text = historyText;

			// Scroll to bottom
			if (moveHistoryScroll != null)
			{
				StartCoroutine(ScrollToBottom());
			}
		}

		private IEnumerator ScrollToBottom()
		{
			yield return new WaitForEndOfFrame();
			if (moveHistoryScroll != null)
				moveHistoryScroll.verticalNormalizedPosition = 0f;
		}

		public void UpdateGameStatus(string status)
		{
			if (gameStatusText != null)
			{
				gameStatusText.text = status;
			}
		}

		public void UpdatePositionInfo(string fen)
		{
			if (positionText != null)
			{
				ChessBoard tempBoard = new ChessBoard(fen);
				string info = $"To Move: {(tempBoard.sideToMove == 'w' ? "White" : "Black")}\n";
				info += $"Move #{tempBoard.fullmoveNumber}\n";
				info += $"50-move: {tempBoard.halfmoveClock}";

				if (tempBoard.enPassantSquare != "-")
					info += $"\nEn passant: {tempBoard.enPassantSquare}";

				if (tempBoard.castlingRights != "-")
					info += $"\nCastling: {tempBoard.castlingRights}";

				positionText.text = info;
			}
		}

		#endregion

		#region Engine Integration

		public void OnEngineThinking(bool thinking)
		{
			isEngineThinking = thinking;

			if (thinking)
			{
				UpdateGameStatus("Engine thinking...");
			}
		}

		public void OnEngineMove(string move, float evaluation)
		{
			// Add move to history
			bool isWhiteMove = chessBoard3D != null ?
				chessBoard3D.GetCurrentFEN().Contains(" w ") : true;
			AddMoveToHistory(move, !isWhiteMove); // Inverted because side switches after move

			// Update evaluation
			UpdateEvaluation(evaluation);

			UpdateGameStatus($"Engine played: {move}");
		}

		public void OnHumanMove(string move)
		{
			// Add move to history
			bool isWhiteMove = chessBoard3D != null ?
				!chessBoard3D.GetCurrentFEN().Contains(" w ") : false; // Inverted because side switches after move
			AddMoveToHistory(move, isWhiteMove);

			UpdateGameStatus($"You played: {move}");
		}

		#endregion

		#region Public Interface for Integration

		public float GetCurrentEvaluation()
		{
			return currentEvaluation;
		}

		public int GetEngineDepth()
		{
			return depthSlider != null ? (int)depthSlider.value : 5;
		}

		public int GetEngineSkill()
		{
			return skillSlider != null ? (int)skillSlider.value : 10;
		}

		public int GetEngineElo()
		{
			if (eloInput != null && int.TryParse(eloInput.text, out int elo))
				return elo;
			return 1500;
		}

		public bool GetHumanPlaysWhite()
		{
			return humanPlaysWhiteToggle != null ? humanPlaysWhiteToggle.isOn : true;
		}

		#endregion
	}
}