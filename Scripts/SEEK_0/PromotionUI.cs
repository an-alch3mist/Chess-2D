/*
PROMOTION UI SYSTEM
- Modal dialog for pawn promotion selection
- Auto-timeout with configurable default (Queen)
- Clean UI with piece buttons and visual feedback
- Integration with ChessBoard and ChessMove systems
- Support for both white and black promotions

USAGE:
1. Add PromotionUI prefab to scene
2. Configure timeout and default piece in inspector
3. Call ShowPromotionDialog() when pawn reaches last rank
4. Listen to OnPromotionSelected event for result
*/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GPTDeepResearch
{
	/// <summary>
	/// UI component for handling pawn promotion selection
	/// Provides modal dialog with piece selection and auto-timeout
	/// </summary>
	public class PromotionUI : MonoBehaviour
	{
		[Header("UI References")]
		[SerializeField] private GameObject promotionPanel;
		[SerializeField] private TextMeshProUGUI titleText;
		[SerializeField] private TextMeshProUGUI timeoutText;
		[SerializeField] private Button queenButton;
		[SerializeField] private Button rookButton;
		[SerializeField] private Button bishopButton;
		[SerializeField] private Button knightButton;
		[SerializeField] private Button cancelButton;

		[Header("Settings")]
		[SerializeField] private float timeoutSeconds = 3f;
		[SerializeField] private char defaultPromotionPiece = 'Q';
		[SerializeField] private bool showTimeoutCountdown = true;

		[Header("Visual Settings")]
		[SerializeField] private Color whiteButtonColor = Color.white;
		[SerializeField] private Color blackButtonColor = Color.gray;
		[SerializeField] private Color selectedColor = Color.green;

		// Events
		public System.Action<char> OnPromotionSelected;
		public System.Action OnPromotionCancelled;

		// Private state
		private bool isWaitingForSelection = false;
		private bool isWhitePromotion = true;
		private char selectedPiece = '\0';
		private Coroutine timeoutCoroutine;

		#region Unity Lifecycle

		private void Awake()
		{
			ValidateComponents();
			SetupButtonListeners();
			HideDialog();
		}

		private void ValidateComponents()
		{
			if (promotionPanel == null) Debug.Log("<color=red>[PromotionUI] promotionPanel not assigned!</color>");
			if (titleText == null) Debug.Log("<color=red>[PromotionUI] titleText not assigned!</color>");
			if (queenButton == null) Debug.Log("<color=red>[PromotionUI] queenButton not assigned!</color>");
			if (rookButton == null) Debug.Log("<color=red>[PromotionUI] rookButton not assigned!</color>");
			if (bishopButton == null) Debug.Log("<color=red>[PromotionUI] bishopButton not assigned!</color>");
			if (knightButton == null) Debug.Log("<color=red>[PromotionUI] knightButton not assigned!</color>");
		}

		private void SetupButtonListeners()
		{
			if (queenButton != null) queenButton.onClick.AddListener(() => SelectPromotion('Q'));
			if (rookButton != null) rookButton.onClick.AddListener(() => SelectPromotion('R'));
			if (bishopButton != null) bishopButton.onClick.AddListener(() => SelectPromotion('B'));
			if (knightButton != null) knightButton.onClick.AddListener(() => SelectPromotion('N'));
			if (cancelButton != null) cancelButton.onClick.AddListener(CancelPromotion);
		}

		#endregion

		#region Public API

		/// <summary>
		/// Show promotion dialog for human player
		/// </summary>
		/// <param name="isWhite">True if white is promoting, false for black</param>
		/// <param name="fromSquare">Source square for context (optional)</param>
		/// <param name="toSquare">Target square for context (optional)</param>
		public void ShowPromotionDialog(bool isWhite, string fromSquare = "", string toSquare = "")
		{
			if (isWaitingForSelection)
			{
				Debug.Log("<color=yellow>[PromotionUI] Already waiting for promotion selection</color>");
				return;
			}

			isWhitePromotion = isWhite;
			isWaitingForSelection = true;
			selectedPiece = '\0';

			// Setup UI
			SetupPromotionUI(isWhite, fromSquare, toSquare);
			ShowDialog();

			// Start timeout countdown
			if (timeoutCoroutine != null)
			{
				StopCoroutine(timeoutCoroutine);
			}
			timeoutCoroutine = StartCoroutine(TimeoutCountdown());

			Debug.Log($"<color=cyan>[PromotionUI] Showing promotion dialog for {(isWhite ? "White" : "Black")}</color>");
		}

		/// <summary>
		/// Hide promotion dialog
		/// </summary>
		public void HideDialog()
		{
			if (promotionPanel != null)
			{
				promotionPanel.SetActive(false);
			}

			isWaitingForSelection = false;

			if (timeoutCoroutine != null)
			{
				StopCoroutine(timeoutCoroutine);
				timeoutCoroutine = null;
			}
		}

		/// <summary>
		/// Check if currently waiting for promotion selection
		/// </summary>
		public bool IsWaitingForPromotion()
		{
			return isWaitingForSelection;
		}

		/// <summary>
		/// Force selection of default piece (for programmatic use)
		/// </summary>
		public void SelectDefaultPromotion()
		{
			SelectPromotion(defaultPromotionPiece);
		}

		#endregion

		#region Private Methods

		private void SetupPromotionUI(bool isWhite, string fromSquare, string toSquare)
		{
			// Set title
			if (titleText != null)
			{
				string color = isWhite ? "White" : "Black";
				string moveText = !string.IsNullOrEmpty(fromSquare) && !string.IsNullOrEmpty(toSquare)
					? $" ({fromSquare}-{toSquare})" : "";
				titleText.text = $"{color} Promotion{moveText}";
			}

			// Setup button colors and text
			Color buttonColor = isWhite ? whiteButtonColor : blackButtonColor;
			SetupPromotionButton(queenButton, "Queen", 'Q', buttonColor);
			SetupPromotionButton(rookButton, "Rook", 'R', buttonColor);
			SetupPromotionButton(bishopButton, "Bishop", 'B', buttonColor);
			SetupPromotionButton(knightButton, "Knight", 'N', buttonColor);

			// Reset button selections
			ResetButtonSelections();
		}

		private void SetupPromotionButton(Button button, string pieceName, char pieceType, Color baseColor)
		{
			if (button == null) return;

			// Set button color
			var colors = button.colors;
			colors.normalColor = baseColor;
			colors.highlightedColor = Color.Lerp(baseColor, Color.white, 0.2f);
			colors.pressedColor = selectedColor;
			button.colors = colors;

			// Set button text
			TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
			if (buttonText != null)
			{
				buttonText.text = pieceName;
			}
		}

		private void ResetButtonSelections()
		{
			Button[] buttons = { queenButton, rookButton, bishopButton, knightButton };
			foreach (Button button in buttons)
			{
				if (button != null)
				{
					var colors = button.colors;
					colors.normalColor = isWhitePromotion ? whiteButtonColor : blackButtonColor;
					button.colors = colors;
				}
			}
		}

		private void ShowDialog()
		{
			if (promotionPanel != null)
			{
				promotionPanel.SetActive(true);
			}
		}

		private void SelectPromotion(char piece)
		{
			if (!isWaitingForSelection) return;

			// Adjust case based on promotion color
			selectedPiece = isWhitePromotion ? char.ToUpper(piece) : char.ToLower(piece);

			// Visual feedback
			HighlightSelectedButton(piece);

			// Notify listeners
			OnPromotionSelected?.Invoke(selectedPiece);

			Debug.Log($"<color=green>[PromotionUI] Selected promotion: {ChessMove.GetPromotionPieceName(selectedPiece)}</color>");

			// Hide dialog after short delay for visual feedback
			StartCoroutine(DelayedHide(0.2f));
		}

		private void HighlightSelectedButton(char piece)
		{
			Button selectedButton = null;

			switch (char.ToUpper(piece))
			{
				case 'Q': selectedButton = queenButton; break;
				case 'R': selectedButton = rookButton; break;
				case 'B': selectedButton = bishopButton; break;
				case 'N': selectedButton = knightButton; break;
			}

			if (selectedButton != null)
			{
				var colors = selectedButton.colors;
				colors.normalColor = selectedColor;
				selectedButton.colors = colors;
			}
		}

		private void CancelPromotion()
		{
			Debug.Log("<color=yellow>[PromotionUI] Promotion cancelled by user</color>");
			OnPromotionCancelled?.Invoke();
			HideDialog();
		}

		private IEnumerator DelayedHide(float delay)
		{
			yield return new WaitForSeconds(delay);
			HideDialog();
		}

		private IEnumerator TimeoutCountdown()
		{
			float remainingTime = timeoutSeconds;

			while (remainingTime > 0 && isWaitingForSelection)
			{
				if (showTimeoutCountdown && timeoutText != null)
				{
					timeoutText.text = $"Auto-select {ChessMove.GetPromotionPieceName(defaultPromotionPiece)} in {remainingTime:F1}s";
				}

				yield return new WaitForSeconds(0.1f);
				remainingTime -= 0.1f;
			}

			// Timeout reached - select default
			if (isWaitingForSelection)
			{
				Debug.Log($"<color=yellow>[PromotionUI] Timeout reached, selecting default: {ChessMove.GetPromotionPieceName(defaultPromotionPiece)}</color>");
				SelectPromotion(defaultPromotionPiece);
			}
		}

		#endregion

		#region Testing

		/// <summary>
		/// Test promotion UI functionality
		/// </summary>
		public void TestPromotionUI()
		{
			Debug.Log("<color=cyan>[PromotionUI] Testing promotion UI...</color>");

			// Test white promotion
			ShowPromotionDialog(true, "e7", "e8");

			// Wait a moment then simulate queen selection
			StartCoroutine(TestSelectionSequence());
		}

		private IEnumerator TestSelectionSequence()
		{
			yield return new WaitForSeconds(0.5f);

			// Simulate queen selection
			SelectPromotion('Q');

			yield return new WaitForSeconds(1f);

			// Test black promotion
			ShowPromotionDialog(false, "d2", "d1");

			yield return new WaitForSeconds(0.5f);

			// Simulate knight selection
			SelectPromotion('N');

			Debug.Log("<color=green>[PromotionUI] ✓ Promotion UI test completed</color>");
		}

		/// <summary>
		/// Test timeout functionality
		/// </summary>
		public void TestTimeout()
		{
			Debug.Log("<color=cyan>[PromotionUI] Testing timeout functionality...</color>");

			// Set short timeout for testing
			float originalTimeout = timeoutSeconds;
			timeoutSeconds = 1f;

			ShowPromotionDialog(true);

			// Restore original timeout after test
			StartCoroutine(RestoreTimeoutAfterTest(originalTimeout));
		}

		private IEnumerator RestoreTimeoutAfterTest(float originalTimeout)
		{
			yield return new WaitForSeconds(2f);
			timeoutSeconds = originalTimeout;
			Debug.Log("<color=green>[PromotionUI] ✓ Timeout test completed</color>");
		}

		#endregion
	}

	/// <summary>
	/// Data class for promotion selection events
	/// </summary>
	[System.Serializable]
	public class PromotionSelectionData
	{
		public char promotionPiece;
		public bool isWhitePromotion;
		public string fromSquare;
		public string toSquare;
		public float selectionTime;

		public PromotionSelectionData(char piece, bool isWhite, string from = "", string to = "")
		{
			promotionPiece = piece;
			isWhitePromotion = isWhite;
			fromSquare = from;
			toSquare = to;
			selectionTime = Time.time;
		}

		public override string ToString()
		{
			string color = isWhitePromotion ? "White" : "Black";
			string pieceName = ChessMove.GetPromotionPieceName(promotionPiece);
			return $"{color} promotes to {pieceName}";
		}
	}
}