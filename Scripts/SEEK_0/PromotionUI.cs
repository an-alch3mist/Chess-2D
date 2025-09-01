/*
PromotionUI.cs - Enhanced Pawn Promotion Selection Interface
===========================================================

USAGE:
1. Create Canvas with PromotionUI prefab
2. Assign piece sprite references and TMP text components in inspector
3. Call ShowPromotionDialog() when pawn reaches last rank
4. Handle OnPromotionSelected event to get chosen piece
5. Default queen selection after configurable timeout

INSPECTOR SETUP:
- promotionPanel: UI Panel containing the promotion interface
- promotionTitle: TMP_Text showing "Choose Promotion Piece"
- queenButton, rookButton, bishopButton, knightButton: UI Buttons
- queenSprite, rookSprite, bishopSprite, knightSprite: Piece sprites
- autoSelectTimeoutSeconds: Time before auto-selecting queen (default: 3.0f)
- showCountdown: Whether to display countdown timer

INTEGRATION EXAMPLE:
```csharp
// Subscribe to promotion event
promotionUI.OnPromotionSelected.AddListener(OnPromotionPieceSelected);

// Show dialog when pawn reaches last rank
if (ChessMove.RequiresPromotion(from, to, piece))
{
    promotionUI.ShowPromotionDialog(isWhitePawn);
}

// Handle selection
private void OnPromotionPieceSelected(char promotionPiece)
{
    // Apply promotion move with selected piece
    ChessMove promotionMove = new ChessMove(from, to, pawn, promotionPiece, capturedPiece);
    ApplyMove(promotionMove);
}
```
*/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
// ~ 17,000 chars
namespace GPTDeepResearch
{
	/// <summary>
	/// Enhanced UI controller for pawn promotion selection with timeout and visual feedback
	/// </summary>
	public class PromotionUI : MonoBehaviour
	{
		[Header("UI Panel References")]
		[SerializeField] private GameObject promotionPanel;
		[SerializeField] private TMP_Text promotionTitle;
		[SerializeField] private TMP_Text countdownText;
		[SerializeField] private Image backgroundOverlay;

		[Header("Promotion Buttons")]
		[SerializeField] private Button queenButton;
		[SerializeField] private Button rookButton;
		[SerializeField] private Button bishopButton;
		[SerializeField] private Button knightButton;

		[Header("White Piece Sprites")]
		[SerializeField] private Sprite whiteQueenSprite;
		[SerializeField] private Sprite whiteRookSprite;
		[SerializeField] private Sprite whiteBishopSprite;
		[SerializeField] private Sprite whiteKnightSprite;

		[Header("Black Piece Sprites")]
		[SerializeField] private Sprite blackQueenSprite;
		[SerializeField] private Sprite blackRookSprite;
		[SerializeField] private Sprite blackBishopSprite;
		[SerializeField] private Sprite blackKnightSprite;

		[Header("Configuration")]
		[SerializeField] private float autoSelectTimeoutSeconds = 3.0f;
		[SerializeField] private bool enableAutoSelect = true;
		[SerializeField] private bool showCountdown = true;
		[SerializeField] private char defaultPromotionPiece = 'Q';
		[SerializeField] private bool pauseGameDuringSelection = true;

		[Header("Visual Effects")]
		[SerializeField] private float buttonHoverScale = 1.1f;
		[SerializeField] private float buttonPressScale = 0.95f;
		[SerializeField] private Color selectedButtonColor = Color.green;
		[SerializeField] private Color defaultButtonColor = Color.white;

		// Events
		[System.Serializable]
		public class PromotionSelectedEvent : UnityEvent<char> { }
		public PromotionSelectedEvent OnPromotionSelected = new PromotionSelectedEvent();

		[System.Serializable]
		public class PromotionCancelledEvent : UnityEvent { }
		public PromotionCancelledEvent OnPromotionCancelled = new PromotionCancelledEvent();

		// State
		private bool isVisible = false;
		private bool isWhitePromotion = true;
		private float timeRemaining = 0f;
		private Coroutine autoSelectCoroutine;
		private Coroutine countdownCoroutine;

		// Button references for effects
		private Button[] allButtons;
		private Vector3[] originalButtonScales;
		private Color[] originalButtonColors;

		#region Unity Lifecycle

		private void Awake()
		{
			// Initialize button arrays
			allButtons = new Button[] { queenButton, rookButton, bishopButton, knightButton };
			originalButtonScales = new Vector3[allButtons.Length];
			originalButtonColors = new Color[allButtons.Length];

			// Store original scales and colors
			for (int i = 0; i < allButtons.Length; i++)
			{
				if (allButtons[i] != null)
				{
					originalButtonScales[i] = allButtons[i].transform.localScale;
					Image buttonImage = allButtons[i].GetComponent<Image>();
					originalButtonColors[i] = buttonImage != null ? buttonImage.color : Color.white;
				}
			}

			// Initialize UI state
			if (promotionPanel != null)
				promotionPanel.SetActive(false);

			// Setup button listeners and effects
			SetupButtonListeners();
			SetupButtonEffects();

			// Validate configuration
			ValidateConfiguration();
		}

		private void Start()
		{
			// Ensure panel is hidden on start
			HidePromotionDialog();
		}

		#endregion

		#region Public API

		/// <summary>
		/// Show promotion selection dialog for given side
		/// </summary>
		/// <param name="isWhite">True for white pawn promotion, false for black</param>
		/// <param name="fromSquare">Source square for context (optional)</param>
		/// <param name="toSquare">Target square for context (optional)</param>
		public void ShowPromotionDialog(bool isWhite, string fromSquare = "", string toSquare = "")
		{
			if (isVisible)
			{
				UnityEngine.Debug.Log("<color=yellow>[PromotionUI] Dialog already visible</color>");
				return;
			}

			isWhitePromotion = isWhite;
			isVisible = true;
			timeRemaining = autoSelectTimeoutSeconds;

			// Update UI elements
			UpdatePromotionTitle(isWhite, fromSquare, toSquare);
			UpdateButtonSprites(isWhite);
			ResetButtonStates();

			// Show panel
			if (promotionPanel != null)
			{
				promotionPanel.SetActive(true);
			}

			// Pause game if configured
			if (pauseGameDuringSelection)
			{
				Time.timeScale = 0f;
			}

			// Start timers
			if (enableAutoSelect && autoSelectTimeoutSeconds > 0f)
			{
				autoSelectCoroutine = StartCoroutine(AutoSelectCoroutine());
			}

			if (showCountdown && countdownText != null)
			{
				countdownCoroutine = StartCoroutine(CountdownCoroutine());
			}

			UnityEngine.Debug.Log($"<color=green>[PromotionUI] Showing dialog for {(isWhite ? "White" : "Black")} promotion</color>");
		}

		/// <summary>
		/// Hide promotion dialog and restore game state
		/// </summary>
		public void HidePromotionDialog()
		{
			if (!isVisible) return;

			isVisible = false;

			// Stop all coroutines
			StopAllPromotionCoroutines();

			// Restore game time
			if (pauseGameDuringSelection)
			{
				Time.timeScale = 1f;
			}

			// Hide panel
			if (promotionPanel != null)
			{
				promotionPanel.SetActive(false);
			}

			// Reset button states
			ResetButtonStates();

			UnityEngine.Debug.Log("<color=green>[PromotionUI] Promotion dialog hidden</color>");
		}

		/// <summary>
		/// Force selection of specific piece (for testing)
		/// </summary>
		public void ForceSelection(char piece)
		{
			if (!isVisible)
			{
				UnityEngine.Debug.Log("<color=yellow>[PromotionUI] Cannot force selection - dialog not visible</color>");
				return;
			}

			SelectPromotion(piece);
		}

		/// <summary>
		/// Check if promotion dialog is currently visible
		/// </summary>
		public bool IsVisible()
		{
			return isVisible;
		}

		/// <summary>
		/// Get remaining time before auto-selection
		/// </summary>
		public float GetTimeRemaining()
		{
			return timeRemaining;
		}

		#endregion

		#region Button Event Handlers

		public void OnQueenSelected()
		{
			HighlightButton(queenButton);
			SelectPromotion('Q');
		}

		public void OnRookSelected()
		{
			HighlightButton(rookButton);
			SelectPromotion('R');
		}

		public void OnBishopSelected()
		{
			HighlightButton(bishopButton);
			SelectPromotion('B');
		}

		public void OnKnightSelected()
		{
			HighlightButton(knightButton);
			SelectPromotion('N');
		}

		/// <summary>
		/// Handle promotion piece selection with validation and effects
		/// </summary>
		private void SelectPromotion(char pieceType)
		{
			if (!isVisible) return;

			// Validate piece type
			if ("QRBN".IndexOf(char.ToUpper(pieceType)) < 0)
			{
				UnityEngine.Debug.Log($"<color=red>[PromotionUI] Invalid promotion piece: {pieceType}</color>");
				return;
			}

			// Convert to correct case based on promotion side
			char selectedPiece = isWhitePromotion ? char.ToUpper(pieceType) : char.ToLower(pieceType);

			// Hide dialog first
			HidePromotionDialog();

			// Fire selection event
			try
			{
				OnPromotionSelected?.Invoke(selectedPiece);
				UnityEngine.Debug.Log($"<color=green>[PromotionUI] Selected: {selectedPiece} ({GetPieceName(selectedPiece)})</color>");
			}
			catch (Exception e)
			{
				UnityEngine.Debug.Log($"<color=red>[PromotionUI] Error in promotion event: {e.Message}</color>");
			}
		}

		#endregion

		#region Private Methods - UI Updates

		private void UpdatePromotionTitle(bool isWhite, string fromSquare, string toSquare)
		{
			if (promotionTitle == null) return;

			string sideText = isWhite ? "White" : "Black";
			string moveText = !string.IsNullOrEmpty(fromSquare) && !string.IsNullOrEmpty(toSquare)
				? $" ({fromSquare}-{toSquare})"
				: "";

			promotionTitle.text = $"{sideText} Promotion{moveText}";
		}

		private void UpdateButtonSprites(bool isWhite)
		{
			if (isWhite)
			{
				SetButtonSprite(queenButton, whiteQueenSprite);
				SetButtonSprite(rookButton, whiteRookSprite);
				SetButtonSprite(bishopButton, whiteBishopSprite);
				SetButtonSprite(knightButton, whiteKnightSprite);
			}
			else
			{
				SetButtonSprite(queenButton, blackQueenSprite);
				SetButtonSprite(rookButton, blackRookSprite);
				SetButtonSprite(bishopButton, blackBishopSprite);
				SetButtonSprite(knightButton, blackKnightSprite);
			}
		}

		private void SetButtonSprite(Button button, Sprite sprite)
		{
			if (button != null && sprite != null)
			{
				Image buttonImage = button.GetComponent<Image>();
				if (buttonImage != null)
				{
					buttonImage.sprite = sprite;
				}
			}
		}

		private void ResetButtonStates()
		{
			for (int i = 0; i < allButtons.Length; i++)
			{
				if (allButtons[i] != null)
				{
					allButtons[i].transform.localScale = originalButtonScales[i];
					Image buttonImage = allButtons[i].GetComponent<Image>();
					if (buttonImage != null)
					{
						buttonImage.color = originalButtonColors[i];
					}
				}
			}
		}

		private void HighlightButton(Button selectedButton)
		{
			if (selectedButton == null) return;

			// Briefly scale and color the selected button
			StartCoroutine(ButtonSelectionEffect(selectedButton));
		}

		private IEnumerator ButtonSelectionEffect(Button button)
		{
			Image buttonImage = button.GetComponent<Image>();
			Vector3 originalScale = button.transform.localScale;
			Color originalColor = buttonImage != null ? buttonImage.color : Color.white;

			// Quick scale and color animation
			float duration = 0.2f;
			float elapsed = 0f;

			while (elapsed < duration)
			{
				elapsed += Time.unscaledDeltaTime; // Use unscaled time in case game is paused
				float t = elapsed / duration;

				// Scale effect
				float scale = Mathf.Lerp(buttonPressScale, 1f, t);
				button.transform.localScale = originalScale * scale;

				// Color effect
				if (buttonImage != null)
				{
					buttonImage.color = Color.Lerp(selectedButtonColor, originalColor, t);
				}

				yield return null;
			}

			// Ensure final state
			button.transform.localScale = originalScale;
			if (buttonImage != null)
			{
				buttonImage.color = originalColor;
			}
		}

		#endregion

		#region Private Methods - Setup

		private void SetupButtonListeners()
		{
			if (queenButton != null)
				queenButton.onClick.AddListener(OnQueenSelected);

			if (rookButton != null)
				rookButton.onClick.AddListener(OnRookSelected);

			if (bishopButton != null)
				bishopButton.onClick.AddListener(OnBishopSelected);

			if (knightButton != null)
				knightButton.onClick.AddListener(OnKnightSelected);
		}

		private void SetupButtonEffects()
		{
			// Add hover effects to all buttons
			for (int i = 0; i < allButtons.Length; i++)
			{
				if (allButtons[i] != null)
				{
					AddHoverEffect(allButtons[i], i);
				}
			}
		}

		private void AddHoverEffect(Button button, int buttonIndex)
		{
			// Add event triggers for hover effects
			UnityEngine.EventSystems.EventTrigger trigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
			if (trigger == null)
			{
				trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
			}

			// Pointer enter
			UnityEngine.EventSystems.EventTrigger.Entry pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry
			{
				eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
			};
			pointerEnter.callback.AddListener((data) => OnButtonHoverEnter(button, buttonIndex));
			trigger.triggers.Add(pointerEnter);

			// Pointer exit
			UnityEngine.EventSystems.EventTrigger.Entry pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry
			{
				eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit
			};
			pointerExit.callback.AddListener((data) => OnButtonHoverExit(button, buttonIndex));
			trigger.triggers.Add(pointerExit);
		}

		private void OnButtonHoverEnter(Button button, int buttonIndex)
		{
			if (!isVisible) return;

			button.transform.localScale = originalButtonScales[buttonIndex] * buttonHoverScale;
		}

		private void OnButtonHoverExit(Button button, int buttonIndex)
		{
			if (!isVisible) return;

			button.transform.localScale = originalButtonScales[buttonIndex];
		}

		private void ValidateConfiguration()
		{
			bool isValid = true;

			if (promotionPanel == null)
			{
				UnityEngine.Debug.Log("<color=red>[PromotionUI] promotionPanel is not assigned!</color>");
				isValid = false;
			}

			if (queenButton == null || rookButton == null || bishopButton == null || knightButton == null)
			{
				UnityEngine.Debug.Log("<color=red>[PromotionUI] One or more promotion buttons are not assigned!</color>");
				isValid = false;
			}

			// Check sprite assignments
			if (whiteQueenSprite == null || blackQueenSprite == null)
			{
				UnityEngine.Debug.Log("<color=yellow>[PromotionUI] Queen sprites missing - buttons may appear blank</color>");
			}

			if (autoSelectTimeoutSeconds <= 0f)
			{
				UnityEngine.Debug.Log("<color=yellow>[PromotionUI] Auto-select timeout <= 0, disabling auto-select</color>");
				enableAutoSelect = false;
			}

			if (isValid)
			{
				UnityEngine.Debug.Log("<color=green>[PromotionUI] Configuration validation passed</color>");
			}
		}

		#endregion

		#region Private Methods - Coroutines

		private void StopAllPromotionCoroutines()
		{
			if (autoSelectCoroutine != null)
			{
				StopCoroutine(autoSelectCoroutine);
				autoSelectCoroutine = null;
			}

			if (countdownCoroutine != null)
			{
				StopCoroutine(countdownCoroutine);
				countdownCoroutine = null;
			}
		}

		private IEnumerator AutoSelectCoroutine()
		{
			float elapsed = 0f;

			// Use unscaled time in case game is paused
			while (elapsed < autoSelectTimeoutSeconds && isVisible)
			{
				yield return null;
				elapsed += Time.unscaledDeltaTime;
				timeRemaining = autoSelectTimeoutSeconds - elapsed;
			}

			// Auto-select if still visible
			if (isVisible)
			{
				UnityEngine.Debug.Log($"<color=cyan>[PromotionUI] Auto-selecting {defaultPromotionPiece} after {autoSelectTimeoutSeconds}s timeout</color>");
				SelectPromotion(defaultPromotionPiece);
			}
		}

		private IEnumerator CountdownCoroutine()
		{
			float elapsed = 0f;

			while (elapsed < autoSelectTimeoutSeconds && isVisible && countdownText != null)
			{
				yield return null;
				elapsed += Time.unscaledDeltaTime;
				timeRemaining = autoSelectTimeoutSeconds - elapsed;

				// Update countdown display
				if (enableAutoSelect)
				{
					countdownText.text = $"Auto-select Queen in: {timeRemaining:F1}s";
				}
				else
				{
					countdownText.text = "Choose promotion piece";
				}
			}

			// Clear countdown text
			if (countdownText != null)
			{
				countdownText.text = "";
			}
		}

		#endregion

		#region Public Configuration Methods

		/// <summary>
		/// Get current auto-select timeout
		/// </summary>
		public float GetAutoSelectTimeout()
		{
			return autoSelectTimeoutSeconds;
		}

		/// <summary>
		/// Set auto-select timeout (runtime configuration)
		/// </summary>
		public void SetAutoSelectTimeout(float timeoutSeconds)
		{
			autoSelectTimeoutSeconds = Mathf.Max(0.5f, timeoutSeconds);
			UnityEngine.Debug.Log($"<color=green>[PromotionUI] Auto-select timeout set to {autoSelectTimeoutSeconds}s</color>");
		}

		/// <summary>
		/// Enable/disable auto-selection feature
		/// </summary>
		public void SetAutoSelectEnabled(bool enabled)
		{
			enableAutoSelect = enabled;

			// Cancel current auto-select if disabling
			if (!enabled && autoSelectCoroutine != null)
			{
				StopCoroutine(autoSelectCoroutine);
				autoSelectCoroutine = null;
			}

			UnityEngine.Debug.Log($"<color=green>[PromotionUI] Auto-select {(enabled ? "enabled" : "disabled")}</color>");
		}

		/// <summary>
		/// Set default promotion piece for auto-selection
		/// </summary>
		public void SetDefaultPromotionPiece(char piece)
		{
			char upperPiece = char.ToUpper(piece);
			if ("QRBN".IndexOf(upperPiece) >= 0)
			{
				defaultPromotionPiece = upperPiece;
				UnityEngine.Debug.Log($"<color=green>[PromotionUI] Default promotion piece set to {GetPieceName(upperPiece)}</color>");
			}
			else
			{
				UnityEngine.Debug.Log($"<color=red>[PromotionUI] Invalid default promotion piece: {piece}. Using Queen.</color>");
				defaultPromotionPiece = 'Q';
			}
		}

		/// <summary>
		/// Set whether game should pause during promotion selection
		/// </summary>
		public void SetPauseGameDuringSelection(bool pause)
		{
			pauseGameDuringSelection = pause;
			UnityEngine.Debug.Log($"<color=green>[PromotionUI] Pause during selection: {pause}</color>");
		}

		#endregion

		#region Utility Methods

		private string GetPieceName(char piece)
		{
			switch (char.ToUpper(piece))
			{
				case 'Q': return "Queen";
				case 'R': return "Rook";
				case 'B': return "Bishop";
				case 'N': return "Knight";
				default: return piece.ToString();
			}
		}

		private Sprite GetPieceSprite(char piece, bool isWhite)
		{
			char upperPiece = char.ToUpper(piece);

			if (isWhite)
			{
				switch (upperPiece)
				{
					case 'Q': return whiteQueenSprite;
					case 'R': return whiteRookSprite;
					case 'B': return whiteBishopSprite;
					case 'N': return whiteKnightSprite;
				}
			}
			else
			{
				switch (upperPiece)
				{
					case 'Q': return blackQueenSprite;
					case 'R': return blackRookSprite;
					case 'B': return blackBishopSprite;
					case 'N': return blackKnightSprite;
				}
			}

			return null;
		}

		#endregion

		#region Testing Methods

		/// <summary>
		/// Test promotion UI with all piece types
		/// </summary>
		public void TestPromotionUI()
		{
			StartCoroutine(TestPromotionSequence());
		}

		private IEnumerator TestPromotionSequence()
		{
			UnityEngine.Debug.Log("<color=cyan>[PromotionUI] Starting promotion UI test...</color>");

			// Test white promotion
			ShowPromotionDialog(true, "e7", "e8");
			yield return new WaitForSecondsRealtime(1f);

			if (isVisible)
			{
				OnQueenSelected();
				UnityEngine.Debug.Log("<color=green>[PromotionUI] White Queen promotion test passed</color>");
			}

			yield return new WaitForSecondsRealtime(0.5f);

			// Test black promotion
			ShowPromotionDialog(false, "d2", "d1");
			yield return new WaitForSecondsRealtime(1f);

			if (isVisible)
			{
				OnKnightSelected();
				UnityEngine.Debug.Log("<color=green>[PromotionUI] Black Knight promotion test passed</color>");
			}

			UnityEngine.Debug.Log("<color=green>[PromotionUI] Promotion UI test completed</color>");
		}

		/// <summary>
		/// Test auto-select functionality
		/// </summary>
		public void TestAutoSelect()
		{
			StartCoroutine(TestAutoSelectSequence());
		}

		private IEnumerator TestAutoSelectSequence()
		{
			UnityEngine.Debug.Log("<color=cyan>[PromotionUI] Testing auto-select...</color>");

			float originalTimeout = autoSelectTimeoutSeconds;
			SetAutoSelectTimeout(1f); // Short timeout for testing

			ShowPromotionDialog(true);

			// Wait for auto-select
			yield return new WaitForSecondsRealtime(1.5f);

			if (!isVisible)
			{
				UnityEngine.Debug.Log("<color=green>[PromotionUI] Auto-select test passed</color>");
			}
			else
			{
				UnityEngine.Debug.Log("<color=red>[PromotionUI] Auto-select test failed - dialog still visible</color>");
				HidePromotionDialog();
			}

			// Restore original timeout
			SetAutoSelectTimeout(originalTimeout);
		}

		#endregion
	}
}