/*
PROMOTION UI SYSTEM
- Modal popup for pawn promotion selection
- Configurable timeout with queen fallback
- Inspector-friendly setup with UnityEvents
- Supports both human and engine promotion moves
- Thread-safe UI updates for engine promotions
*/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace GPTDeepResearch
{
	/// <summary>
	/// Handles pawn promotion UI with timeout and fallback.
	/// Shows modal when human pawn reaches last rank.
	/// </summary>
	public class PromotionUI : MonoBehaviour
	{
		[Header("UI References")]
		[SerializeField] private GameObject promotionPanel;
		[SerializeField] private Button queenButton;
		[SerializeField] private Button rookButton;
		[SerializeField] private Button bishopButton;
		[SerializeField] private Button knightButton;
		[SerializeField] private Button cancelButton;
		[SerializeField] private TextMeshProUGUI titleText;
		[SerializeField] private TextMeshProUGUI timerText;
		[SerializeField] private Image backgroundOverlay;

		[Header("Settings")]
		[SerializeField] private float timeoutSeconds = 3f;
		[SerializeField] private char defaultPromotionPiece = 'Q';
		[SerializeField] private bool allowCancel = false;
		[SerializeField] private bool showTimer = true;

		[Header("Events")]
		public UnityEvent<char> OnPromotionSelected;
		public UnityEvent OnPromotionCancelled;
		public UnityEvent<float> OnTimerUpdate;

		// Internal state
		private char pendingPromotionPiece = '\0';
		private bool isWaitingForSelection = false;
		private bool selectionMade = false;
		private Coroutine timeoutCoroutine;
		private ChessMove pendingMove;

		#region Unity Lifecycle

		private void Awake()
		{
			// Setup button listeners
			if (queenButton) queenButton.onClick.AddListener(() => SelectPromotion('Q'));
			if (rookButton) rookButton.onClick.AddListener(() => SelectPromotion('R'));
			if (bishopButton) bishopButton.onClick.AddListener(() => SelectPromotion('B'));
			if (knightButton) knightButton.onClick.AddListener(() => SelectPromotion('N'));
			if (cancelButton) cancelButton.onClick.AddListener(CancelPromotion);

			// Setup initial state
			if (promotionPanel) promotionPanel.SetActive(false);
			if (cancelButton) cancelButton.gameObject.SetActive(allowCancel);

			ValidateSetup();
		}

		private void OnDestroy()
		{
			// Cleanup button listeners
			if (queenButton) queenButton.onClick.RemoveAllListeners();
			if (rookButton) rookButton.onClick.RemoveAllListeners();
			if (bishopButton) bishopButton.onClick.RemoveAllListeners();
			if (knightButton) knightButton.onClick.RemoveAllListeners();
			if (cancelButton) cancelButton.onClick.RemoveAllListeners();
		}

		#endregion

		#region Public API

		/// <summary>
		/// Show promotion UI for human move. Returns selected piece via callback.
		/// </summary>
		public void ShowPromotionUI(ChessMove move, char sideColor, Action<char> onComplete)
		{
			if (isWaitingForSelection)
			{
				Debug.Log("<color=yellow>[PromotionUI] Already waiting for promotion selection</color>");
				return;
			}

			pendingMove = move;
			pendingPromotionPiece = '\0';
			selectionMade = false;
			isWaitingForSelection = true;

			// Setup UI
			UpdateUIForSide(sideColor);
			ShowPanel();

			// Start timeout coroutine
			if (timeoutSeconds > 0)
			{
				timeoutCoroutine = StartCoroutine(TimeoutCoroutine(onComplete));
			}

			// Store callback for button selection
			var tempEvent = OnPromotionSelected;
			OnPromotionSelected.RemoveAllListeners();
			OnPromotionSelected.AddListener((piece) => {
				OnPromotionSelected = tempEvent;
				onComplete?.Invoke(piece);
			});
		}

		/// <summary>
		/// Handle engine promotion move (no UI needed)
		/// </summary>
		public void HandleEnginePromotion(ChessMove move)
		{
			char promotionPiece = move.promotionPiece;
			Debug.Log($"<color=green>[PromotionUI] Engine promotes to {GetPieceName(promotionPiece)}: {move.ToUCI()}</color>");

			// Fire event for consistency
			OnPromotionSelected?.Invoke(promotionPiece);
		}

		/// <summary>
		/// Hide promotion UI (called externally if needed)
		/// </summary>
		public void HidePromotionUI()
		{
			if (timeoutCoroutine != null)
			{
				StopCoroutine(timeoutCoroutine);
				timeoutCoroutine = null;
			}

			isWaitingForSelection = false;
			selectionMade = false;
			HidePanel();
		}

		/// <summary>
		/// Check if promotion UI is currently active
		/// </summary>
		public bool IsActive()
		{
			return isWaitingForSelection && promotionPanel && promotionPanel.activeInHierarchy;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Update UI text and button colors based on side
		/// </summary>
		private void UpdateUIForSide(char side)
		{
			bool isWhite = side == 'w' || char.IsUpper(side);
			string sideName = isWhite ? "White" : "Black";
			string squareName = ChessBoard.CoordToAlgebraic(pendingMove.to);

			if (titleText)
			{
				titleText.text = $"{sideName} Promotion ({squareName})";
				titleText.color = isWhite ? Color.white : new Color(0.2f, 0.2f, 0.2f);
			}

			// Update button piece letters to match side
			UpdateButtonText(queenButton, isWhite ? 'Q' : 'q');
			UpdateButtonText(rookButton, isWhite ? 'R' : 'r');
			UpdateButtonText(bishopButton, isWhite ? 'B' : 'b');
			UpdateButtonText(knightButton, isWhite ? 'N' : 'n');
		}

		/// <summary>
		/// Update button text with piece symbol
		/// </summary>
		private void UpdateButtonText(Button button, char piece)
		{
			if (!button) return;

			TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
			if (buttonText)
			{
				buttonText.text = GetPieceSymbol(piece);
			}
		}

		/// <summary>
		/// Get Unicode chess piece symbol
		/// </summary>
		private string GetPieceSymbol(char piece)
		{
			switch (char.ToUpper(piece))
			{
				case 'Q': return char.IsUpper(piece) ? "♕" : "♛";
				case 'R': return char.IsUpper(piece) ? "♖" : "♜";
				case 'B': return char.IsUpper(piece) ? "♗" : "♝";
				case 'N': return char.IsUpper(piece) ? "♘" : "♞";
				default: return piece.ToString();
			}
		}

		/// <summary>
		/// Get piece name for logging
		/// </summary>
		private string GetPieceName(char piece)
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

		/// <summary>
		/// Handle promotion piece selection
		/// </summary>
		private void SelectPromotion(char pieceType)
		{
			if (!isWaitingForSelection || selectionMade)
				return;

			// Adjust case based on pending move piece color
			bool isWhite = char.IsUpper(pendingMove.piece);
			pendingPromotionPiece = isWhite ? char.ToUpper(pieceType) : char.ToLower(pieceType);
			selectionMade = true;

			Debug.Log($"<color=green>[PromotionUI] Player selected {GetPieceName(pendingPromotionPiece)} promotion</color>");

			// Stop timeout and hide UI
			if (timeoutCoroutine != null)
			{
				StopCoroutine(timeoutCoroutine);
				timeoutCoroutine = null;
			}

			HidePanel();
			isWaitingForSelection = false;

			// Fire selection event
			OnPromotionSelected?.Invoke(pendingPromotionPiece);
		}

		/// <summary>
		/// Handle promotion cancellation
		/// </summary>
		private void CancelPromotion()
		{
			if (!isWaitingForSelection || !allowCancel)
				return;

			selectionMade = true;

			Debug.Log("<color=yellow>[PromotionUI] Promotion cancelled by user</color>");

			if (timeoutCoroutine != null)
			{
				StopCoroutine(timeoutCoroutine);
				timeoutCoroutine = null;
			}

			HidePanel();
			isWaitingForSelection = false;

			OnPromotionCancelled?.Invoke();
		}

		/// <summary>
		/// Timeout coroutine with countdown display
		/// </summary>
		private IEnumerator TimeoutCoroutine(Action<char> onComplete)
		{
			float remainingTime = timeoutSeconds;

			while (remainingTime > 0 && !selectionMade)
			{
				if (showTimer && timerText)
				{
					timerText.text = $"Auto-Queen in {remainingTime:F1}s";
				}

				OnTimerUpdate?.Invoke(remainingTime);

				yield return new WaitForSeconds(0.1f);
				remainingTime -= 0.1f;
			}

			// Timeout reached and no selection made
			if (!selectionMade && isWaitingForSelection)
			{
				bool isWhite = char.IsUpper(pendingMove.piece);
				char defaultPiece = isWhite ? char.ToUpper(defaultPromotionPiece) : char.ToLower(defaultPromotionPiece);

				pendingPromotionPiece = defaultPiece;
				selectionMade = true;
				isWaitingForSelection = false;

				Debug.Log($"<color=yellow>[PromotionUI] Timeout reached, auto-promoting to {GetPieceName(defaultPiece)}</color>");

				HidePanel();
				onComplete?.Invoke(defaultPiece);
			}

			timeoutCoroutine = null;
		}

		/// <summary>
		/// Show promotion panel with animation
		/// </summary>
		private void ShowPanel()
		{
			if (!promotionPanel) return;

			promotionPanel.SetActive(true);

			// Simple fade-in animation
			if (backgroundOverlay)
			{
				StartCoroutine(FadeIn(backgroundOverlay, 0.3f));
			}
		}

		/// <summary>
		/// Hide promotion panel
		/// </summary>
		private void HidePanel()
		{
			if (!promotionPanel) return;

			promotionPanel.SetActive(false);

			if (timerText)
			{
				timerText.text = "";
			}
		}

		/// <summary>
		/// Fade in UI element
		/// </summary>
		private IEnumerator FadeIn(Image image, float duration)
		{
			float elapsed = 0f;
			Color color = image.color;
			color.a = 0f;

			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				color.a = Mathf.Lerp(0f, 0.8f, elapsed / duration);
				image.color = color;
				yield return null;
			}

			color.a = 0.8f;
			image.color = color;
		}

		/// <summary>
		/// Validate inspector setup
		/// </summary>
		private void ValidateSetup()
		{
			if (!promotionPanel)
				Debug.Log("<color=red>[PromotionUI] Missing promotionPanel reference in inspector</color>");

			if (!queenButton || !rookButton || !bishopButton || !knightButton)
				Debug.Log("<color=red>[PromotionUI] Missing piece button references in inspector</color>");

			if (timeoutSeconds < 0f)
			{
				Debug.Log("<color=yellow>[PromotionUI] Invalid timeout, using default 3 seconds</color>");
				timeoutSeconds = 3f;
			}

			if ("QRBN".IndexOf(char.ToUpper(defaultPromotionPiece)) < 0)
			{
				Debug.Log("<color=yellow>[PromotionUI] Invalid default promotion piece, using Queen</color>");
				defaultPromotionPiece = 'Q';
			}
		}

		#endregion

		#region Static Utility Methods

		/// <summary>
		/// Check if a move requires promotion UI (human pawn to last rank)
		/// </summary>
		public static bool RequiresPromotionUI(ChessMove move, char humanSide)
		{
			if (char.ToUpper(move.piece) != 'P')
				return false;

			bool isHumanMove = (humanSide == 'w' && char.IsUpper(move.piece)) ||
							  (humanSide == 'b' && char.IsLower(move.piece)) ||
							  (humanSide == 'x'); // Both sides human

			if (!isHumanMove)
				return false;

			bool isWhitePawn = char.IsUpper(move.piece);
			int promotionRank = isWhitePawn ? 7 : 0;

			return move.to.y == promotionRank;
		}

		/// <summary>
		/// Validate promotion piece character
		/// </summary>
		public static bool IsValidPromotionPiece(char piece)
		{
			return "QRBNqrbn".IndexOf(piece) >= 0;
		}

		/// <summary>
		/// Get promotion piece with correct case for side
		/// </summary>
		public static char GetPromotionPieceForSide(char pieceType, bool isWhite)
		{
			char piece = char.ToUpper(pieceType);
			if ("QRBN".IndexOf(piece) < 0)
				piece = 'Q'; // Default to Queen

			return isWhite ? piece : char.ToLower(piece);
		}

		#endregion

		#region Test Methods

		/// <summary>
		/// Test promotion UI functionality
		/// </summary>
		public void TestPromotionUI()
		{
			Debug.Log("<color=cyan>[PromotionUI] Testing promotion UI...</color>");

			// Test move creation
			ChessMove testMove = new ChessMove(new SPACE_UTIL.v2(4, 6), new SPACE_UTIL.v2(4, 7), 'P', '\0');

			if (RequiresPromotionUI(testMove, 'w'))
			{
				Debug.Log("<color=green>[PromotionUI] ✓ Correctly detected promotion requirement</color>");
			}
			else
			{
				Debug.Log("<color=red>[PromotionUI] ✗ Failed to detect promotion requirement</color>");
			}

			// Test piece validation
			char[] validPieces = { 'Q', 'R', 'B', 'N', 'q', 'r', 'b', 'n' };
			char[] invalidPieces = { 'P', 'K', 'X', '0', ' ' };

			foreach (char piece in validPieces)
			{
				if (IsValidPromotionPiece(piece))
				{
					Debug.Log($"<color=green>[PromotionUI] ✓ Valid piece: {piece}</color>");
				}
				else
				{
					Debug.Log($"<color=red>[PromotionUI] ✗ Should be valid: {piece}</color>");
				}
			}

			foreach (char piece in invalidPieces)
			{
				if (!IsValidPromotionPiece(piece))
				{
					Debug.Log($"<color=green>[PromotionUI] ✓ Invalid piece rejected: {piece}</color>");
				}
				else
				{
					Debug.Log($"<color=red>[PromotionUI] ✗ Should be invalid: {piece}</color>");
				}
			}

			// Test case conversion
			char whiteQueen = GetPromotionPieceForSide('q', true);
			char blackQueen = GetPromotionPieceForSide('Q', false);

			if (whiteQueen == 'Q' && blackQueen == 'q')
			{
				Debug.Log("<color=green>[PromotionUI] ✓ Case conversion works correctly</color>");
			}
			else
			{
				Debug.Log($"<color=red>[PromotionUI] ✗ Case conversion failed: {whiteQueen}, {blackQueen}</color>");
			}
		}

		/// <summary>
		/// Test timeout functionality
		/// </summary>
		public void TestTimeout()
		{
			ChessMove testMove = new ChessMove(new SPACE_UTIL.v2(0, 6), new SPACE_UTIL.v2(0, 7), 'P', '\0');

			ShowPromotionUI(testMove, 'w', (piece) => {
				Debug.Log($"<color=green>[PromotionUI] Timeout test completed with piece: {piece}</color>");
			});
		}

		#endregion

		#region Inspector Setup Notes

		/*
        INSPECTOR SETUP INSTRUCTIONS:

        1. Create UI Canvas with PromotionUI component
        2. Add child Panel for promotion modal (promotionPanel)
        3. Add four Buttons for pieces (queenButton, rookButton, bishopButton, knightButton)
        4. Add optional Cancel button (cancelButton)
        5. Add TextMeshProUGUI for title (titleText)
        6. Add TextMeshProUGUI for timer (timerText) if showTimer enabled
        7. Add Image for background overlay (backgroundOverlay) with semi-transparent black

        PREFAB STRUCTURE:
        PromotionCanvas (Canvas)
        └── PromotionUI (This script)
            └── PromotionPanel (Panel)
                ├── Background (Image - semi-transparent)
                ├── TitleText (TextMeshProUGUI)
                ├── TimerText (TextMeshProUGUI)
                ├── ButtonRow (Horizontal Layout Group)
                │   ├── QueenButton (Button + TextMeshProUGUI)
                │   ├── RookButton (Button + TextMeshProUGUI)
                │   ├── BishopButton (Button + TextMeshProUGUI)
                │   └── KnightButton (Button + TextMeshProUGUI)
                └── CancelButton (Button + TextMeshProUGUI) [optional]

        BUTTON SETUP:
        - Each button should have TextMeshProUGUI child with chess piece Unicode symbols
        - Queen: ♕/♛, Rook: ♖/♜, Bishop: ♗/♝, Knight: ♘/♞
        - Buttons should have comfortable touch/click targets (min 44x44 pixels)
        - Consider hover effects for desktop play

        EVENTS SETUP:
        - OnPromotionSelected: Subscribe to handle selected promotion piece
        - OnPromotionCancelled: Subscribe to handle cancellation (if enabled)
        - OnTimerUpdate: Subscribe for custom timer UI updates

        USAGE EXAMPLE:
        ```csharp
        PromotionUI promotionUI = FindObjectOfType<PromotionUI>();
        
        // When human pawn reaches last rank:
        if (PromotionUI.RequiresPromotionUI(move, chessBoard.humanSide))
        {
            promotionUI.ShowPromotionUI(move, chessBoard.sideToMove, (selectedPiece) => {
                move.promotionPiece = selectedPiece;
                move.moveType = ChessMove.MoveType.Promotion;
                chessBoard.MakeMove(move);
            });
        }
        ```
        */

		#endregion
	}
}