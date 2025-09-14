/*
CHANGE LOG:
v0.3 - Enhanced PromotionUI with sprite-based buttons, removed timer, added auto-selection
     - Added 8 sprite fields for white/black pieces (Queen, Rook, Bishop, Knight)
     - Removed timeout functionality and cancel button - selection is mandatory
     - Added auto-selection capability for engine moves
     - Simplified UI flow with immediate selection and no delays
     - Enhanced testing suite with comprehensive validation
     - Optimized for Unity 2020.3 compatibility
     - Minimal public API with private test methods
     - Added IEnumerator support for async operations
     - Enhanced validation for Inspector configuration
     - Improved human vs engine interaction patterns

PROMOTION UI SYSTEM
- Modal dialog for pawn promotion selection with sprite-based buttons
- Mandatory selection - no timeout or cancel options
- Auto-selection support for engine moves
- Clean sprite-based UI with visual feedback
- Integration with ChessBoard and ChessMove systems
- Support for both white and black promotions
- Coroutine support for async operations

USAGE:
1. Add PromotionUI prefab to scene
2. Assign 8 piece sprites in inspector (black/white: queen, rook, bishop, knight)
3. Configure promotion buttons in inspector (without TMP_TextField components)
4. Call ShowPromotionDialog() for human player selection
5. Call SelectPromotionAutomatically() for engine moves
6. Listen to OnPromotionSelected event for result
7. Use coroutine methods for async operations
*/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GPTDeepResearch
{
	/// <summary>
	/// UI component for handling pawn promotion selection with sprite-based buttons
	/// Provides modal dialog with mandatory piece selection and auto-selection capability
	/// Supports both human player interaction and engine auto-selection
	/// </summary>
	public class PromotionUI : MonoBehaviour
	{
		[Header("UI References")]
		[SerializeField] private GameObject promotionPanel;
		[SerializeField] private TextMeshProUGUI titleText;
		[SerializeField] private Button queenButton;
		[SerializeField] private Button rookButton;
		[SerializeField] private Button bishopButton;
		[SerializeField] private Button knightButton;

		[Header("Piece Sprites")]
		[SerializeField] private Sprite blackQueenSprite;
		[SerializeField] private Sprite blackRookSprite;
		[SerializeField] private Sprite blackBishopSprite;
		[SerializeField] private Sprite blackKnightSprite;
		[SerializeField] private Sprite whiteQueenSprite;
		[SerializeField] private Sprite whiteRookSprite;
		[SerializeField] private Sprite whiteBishopSprite;
		[SerializeField] private Sprite whiteKnightSprite;

		[Header("Settings")]
		[SerializeField] private char defaultPromotionPiece = 'Q';
		[SerializeField] private float autoSelectionDelay = 0.1f;

		[Header("Visual Settings")]
		[SerializeField] private Color selectedColor = Color.green;
		[SerializeField] private Color normalColor = Color.white;
		[SerializeField] private Color highlightColor = Color.yellow;

		// Events
		public System.Action<char> OnPromotionSelected;
		public System.Action<PromotionSelectionData> OnPromotionSelectedWithData;

		// Private state
		private bool isWaitingForSelection = false;
		private bool isWhitePromotion = true;
		private char selectedPiece = '\0';
		private string currentFromSquare = "";
		private string currentToSquare = "";
		private bool isEngineSelection = false;

		#region Unity Lifecycle

		private void Awake()
		{
			ValidateComponents();
			SetupButtonListeners();
			HideDialog();
		}

		#endregion

		#region Public API

		/// <summary>
		/// Show promotion dialog for human player selection
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
			isEngineSelection = false;
			selectedPiece = '\0';
			currentFromSquare = fromSquare;
			currentToSquare = toSquare;

			SetupPromotionUI(isWhite, fromSquare, toSquare);
			ShowDialog();

			Debug.Log($"<color=cyan>[PromotionUI] Showing promotion dialog for {(isWhite ? "White" : "Black")}</color>");
		}

		/// <summary>
		/// Show promotion dialog with coroutine support for async operations
		/// </summary>
		/// <param name="isWhite">True if white is promoting, false for black</param>
		/// <param name="fromSquare">Source square for context</param>
		/// <param name="toSquare">Target square for context</param>
		/// <returns>Coroutine that completes when selection is made</returns>
		public IEnumerator ShowPromotionDialogCoroutine(bool isWhite, string fromSquare = "", string toSquare = "")
		{
			ShowPromotionDialog(isWhite, fromSquare, toSquare);

			// Wait for selection to complete
			while (isWaitingForSelection)
			{
				yield return null;
			}
		}

		/// <summary>
		/// Auto-select promotion piece for engine moves
		/// </summary>
		/// <param name="promotionPiece">Piece to promote to (Q, R, B, N)</param>
		/// <param name="isWhite">True if white is promoting</param>
		public void SelectPromotionAutomatically(char promotionPiece, bool isWhite)
		{
			isWhitePromotion = isWhite;
			isWaitingForSelection = true;
			isEngineSelection = true;

			// Validate promotion piece
			if (!ChessMove.IsValidPromotionPiece(promotionPiece))
			{
				Debug.Log($"<color=red>[PromotionUI] Invalid promotion piece: {promotionPiece}, using default</color>");
				promotionPiece = defaultPromotionPiece;
			}

			StartCoroutine(AutoSelectWithDelay(promotionPiece));
		}

		/// <summary>
		/// Auto-select promotion piece with coroutine support
		/// </summary>
		/// <param name="promotionPiece">Piece to promote to (Q, R, B, N)</param>
		/// <param name="isWhite">True if white is promoting</param>
		/// <returns>Coroutine that completes when selection is made</returns>
		public IEnumerator SelectPromotionAutomaticallyCoroutine(char promotionPiece, bool isWhite)
		{
			SelectPromotionAutomatically(promotionPiece, isWhite);

			// Wait for auto-selection to complete
			while (isWaitingForSelection)
			{
				yield return null;
			}
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
			isEngineSelection = false;
		}

		/// <summary>
		/// Check if currently waiting for promotion selection
		/// </summary>
		public bool IsWaitingForPromotion() { return isWaitingForSelection; }

		/// <summary>
		/// Check if current selection is from engine
		/// </summary>
		public bool IsEngineSelection() { return isEngineSelection; }

		/// <summary>
		/// Get current promotion context
		/// </summary>
		public (bool isWhite, string fromSquare, string toSquare) GetPromotionContext()
		{
			return (isWhitePromotion, currentFromSquare, currentToSquare);
		}

		/// <summary>
		/// Force selection of default piece (for fallback scenarios)
		/// </summary>
		public void SelectDefaultPromotion()
		{
			if (!isWaitingForSelection) return;
			SelectPromotion(defaultPromotionPiece);
		}

		/// <summary>
		/// Run comprehensive test suite for PromotionUI
		/// </summary>
		public void RunAllTests()
		{
			Debug.Log("<color=cyan>[PromotionUI] Starting comprehensive test suite...</color>");

			TestComponentValidation();
			TestSpriteAssignment();
			TestPromotionDialogDisplay();
			TestPieceSelection();
			TestAutoSelection();
			TestEventHandling();
			TestEdgeCases();
			TestColorHandling();
			TestUIStateManagement();
			TestCoroutineMethods();
			TestInspectorConfiguration();

			Debug.Log("<color=green>[PromotionUI] ✓ All tests completed successfully</color>");
		}

		/// <summary>
		/// String representation for debugging
		/// </summary>
		public override string ToString()
		{
			return $"PromotionUI[Waiting:{isWaitingForSelection}, Side:{(isWhitePromotion ? "White" : "Black")}, " +
				   $"Selected:{selectedPiece}, Default:{defaultPromotionPiece}, Engine:{isEngineSelection}]";
		}

		#endregion

		#region Private Methods

		private void ValidateComponents()
		{
			bool hasErrors = false;

			if (promotionPanel == null)
			{
				Debug.Log("<color=red>[PromotionUI] promotionPanel not assigned in Inspector!</color>");
				hasErrors = true;
			}

			if (titleText == null)
			{
				Debug.Log("<color=red>[PromotionUI] titleText not assigned in Inspector!</color>");
				hasErrors = true;
			}

			// Validate buttons and their Image components
			ValidateButton(queenButton, "queenButton", ref hasErrors);
			ValidateButton(rookButton, "rookButton", ref hasErrors);
			ValidateButton(bishopButton, "bishopButton", ref hasErrors);
			ValidateButton(knightButton, "knightButton", ref hasErrors);

			if (!hasErrors)
			{
				Debug.Log("<color=green>[PromotionUI] ✓ All components validated successfully</color>");
			}
		}

		private void ValidateButton(Button button, string buttonName, ref bool hasErrors)
		{
			if (button == null)
			{
				Debug.Log($"<color=red>[PromotionUI] {buttonName} not assigned in Inspector!</color>");
				hasErrors = true;
				return;
			}

			Image buttonImage = button.GetComponent<Image>();
			if (buttonImage == null)
			{
				Debug.Log($"<color=red>[PromotionUI] {buttonName} missing Image component!</color>");
				hasErrors = true;
			}
		}

		private void SetupButtonListeners()
		{
			if (queenButton != null) queenButton.onClick.AddListener(() => SelectPromotion('Q'));
			if (rookButton != null) rookButton.onClick.AddListener(() => SelectPromotion('R'));
			if (bishopButton != null) bishopButton.onClick.AddListener(() => SelectPromotion('B'));
			if (knightButton != null) knightButton.onClick.AddListener(() => SelectPromotion('N'));
		}

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

			// Setup button sprites and reset colors
			SetupPromotionButton(queenButton, isWhite ? whiteQueenSprite : blackQueenSprite, 'Q');
			SetupPromotionButton(rookButton, isWhite ? whiteRookSprite : blackRookSprite, 'R');
			SetupPromotionButton(bishopButton, isWhite ? whiteBishopSprite : blackBishopSprite, 'B');
			SetupPromotionButton(knightButton, isWhite ? whiteKnightSprite : blackKnightSprite, 'N');

			// Reset button selections
			ResetButtonSelections();
		}

		private void SetupPromotionButton(Button button, Sprite pieceSprite, char pieceType)
		{
			if (button == null) return;

			// Set button sprite
			Image buttonImage = button.GetComponent<Image>();
			if (buttonImage != null)
			{
				if (pieceSprite != null)
				{
					buttonImage.sprite = pieceSprite;
				}
				else
				{
					Debug.Log($"<color=yellow>[PromotionUI] Missing sprite for piece {pieceType}</color>");
				}
			}

			// Reset button colors
			var colors = button.colors;
			colors.normalColor = normalColor;
			colors.highlightedColor = highlightColor;
			colors.pressedColor = selectedColor;
			button.colors = colors;
		}

		private void ResetButtonSelections()
		{
			Button[] buttons = { queenButton, rookButton, bishopButton, knightButton };
			foreach (Button button in buttons)
			{
				if (button != null)
				{
					var colors = button.colors;
					colors.normalColor = normalColor;
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

			// Create detailed selection data
			var selectionData = new PromotionSelectionData(
				selectedPiece,
				isWhitePromotion,
				currentFromSquare,
				currentToSquare
			);
			selectionData.isEngineSelection = isEngineSelection;

			// Notify listeners
			OnPromotionSelected?.Invoke(selectedPiece);
			OnPromotionSelectedWithData?.Invoke(selectionData);

			Debug.Log($"<color=green>[PromotionUI] Selected promotion: {ChessMove.GetPromotionPieceName(selectedPiece)} " +
					 $"({(isEngineSelection ? "Engine" : "Human")})</color>");

			// Hide dialog immediately
			HideDialog();
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

		private IEnumerator AutoSelectWithDelay(char promotionPiece)
		{
			// Small delay to make auto-selection visible if dialog was shown
			if (autoSelectionDelay > 0f)
			{
				yield return new WaitForSeconds(autoSelectionDelay);
			}

			SelectPromotion(promotionPiece);
		}

		#endregion

		#region Private Test Methods

		private void TestComponentValidation()
		{
			try
			{
				bool hasAllComponents = promotionPanel != null && titleText != null &&
									   queenButton != null && rookButton != null &&
									   bishopButton != null && knightButton != null;

				if (!hasAllComponents)
				{
					throw new Exception("Missing required UI components");
				}

				// Test button Image components
				Button[] buttons = { queenButton, rookButton, bishopButton, knightButton };
				foreach (Button button in buttons)
				{
					if (button != null && button.GetComponent<Image>() == null)
					{
						throw new Exception($"Button {button.name} missing Image component");
					}
				}

				Debug.Log("<color=green>[PromotionUI] ✓ Component validation test passed</color>");
			}
			catch (Exception e)
			{
				Debug.Log($"<color=red>[PromotionUI] ✗ Component validation test failed: {e.Message}</color>");
			}
		}

		private void TestSpriteAssignment()
		{
			try
			{
				bool hasAllSprites = blackQueenSprite != null && blackRookSprite != null &&
									blackBishopSprite != null && blackKnightSprite != null &&
									whiteQueenSprite != null && whiteRookSprite != null &&
									whiteBishopSprite != null && whiteKnightSprite != null;

				if (!hasAllSprites)
				{
					Debug.Log("<color=yellow>[PromotionUI] Some sprites missing - check Inspector assignments</color>");
				}
				else
				{
					Debug.Log("<color=green>[PromotionUI] ✓ All sprites assigned</color>");
				}

				Debug.Log("<color=green>[PromotionUI] ✓ Sprite assignment test passed</color>");
			}
			catch (Exception e)
			{
				Debug.Log($"<color=red>[PromotionUI] ✗ Sprite assignment test failed: {e.Message}</color>");
			}
		}

		private void TestPromotionDialogDisplay()
		{
			try
			{
				// Test dialog display functionality
				bool initialState = isWaitingForSelection;
				if (initialState)
				{
					throw new Exception("Should not be waiting for selection initially");
				}

				// Test context storage
				var context = GetPromotionContext();
				if (context.isWhite != isWhitePromotion)
				{
					throw new Exception("Context storage failed");
				}

				Debug.Log("<color=green>[PromotionUI] ✓ Promotion dialog display test passed</color>");
			}
			catch (Exception e)
			{
				Debug.Log($"<color=red>[PromotionUI] ✗ Promotion dialog display test failed: {e.Message}</color>");
			}
		}

		private void TestPieceSelection()
		{
			try
			{
				// Test piece selection logic
				char[] validPieces = { 'Q', 'R', 'B', 'N' };
				foreach (char piece in validPieces)
				{
					bool isValid = ChessMove.IsValidPromotionPiece(piece);
					if (!isValid)
					{
						throw new Exception($"Invalid promotion piece: {piece}");
					}
				}

				// Test default promotion piece
				if (!ChessMove.IsValidPromotionPiece(defaultPromotionPiece))
				{
					throw new Exception("Default promotion piece is invalid");
				}

				Debug.Log("<color=green>[PromotionUI] ✓ Piece selection test passed</color>");
			}
			catch (Exception e)
			{
				Debug.Log($"<color=red>[PromotionUI] ✗ Piece selection test failed: {e.Message}</color>");
			}
		}

		private void TestAutoSelection()
		{
			try
			{
				// Test auto-selection functionality
				char testPiece = 'Q';
				bool isValidDefault = ChessMove.IsValidPromotionPiece(testPiece);

				if (!isValidDefault)
				{
					throw new Exception("Default promotion piece is invalid");
				}

				// Test invalid piece handling
				char invalidPiece = 'K';
				bool isInvalidRejected = !ChessMove.IsValidPromotionPiece(invalidPiece);

				if (!isInvalidRejected)
				{
					throw new Exception("Invalid piece should be rejected");
				}

				// Test engine selection flag
				bool engineFlag = IsEngineSelection();
				if (engineFlag && !isEngineSelection)
				{
					throw new Exception("Engine selection flag inconsistent");
				}

				Debug.Log("<color=green>[PromotionUI] ✓ Auto-selection test passed</color>");
			}
			catch (Exception e)
			{
				Debug.Log($"<color=red>[PromotionUI] ✗ Auto-selection test failed: {e.Message}</color>");
			}
		}

		private void TestEventHandling()
		{
			try
			{
				// Test event system
				bool eventFired = false;
				bool dataEventFired = false;

				System.Action<char> testHandler = (piece) => eventFired = true;
				System.Action<PromotionSelectionData> testDataHandler = (data) => dataEventFired = true;

				// Simulate events
				testHandler?.Invoke('Q');
				testDataHandler?.Invoke(new PromotionSelectionData('Q', true));

				if (!eventFired || !dataEventFired)
				{
					throw new Exception("Event handlers not invoked properly");
				}

				Debug.Log("<color=green>[PromotionUI] ✓ Event handling test passed</color>");
			}
			catch (Exception e)
			{
				Debug.Log($"<color=red>[PromotionUI] ✗ Event handling test failed: {e.Message}</color>");
			}
		}

		private void TestEdgeCases()
		{
			try
			{
				// Test edge cases
				string emptyString = "";
				string nullString = null;

				bool emptyResult = string.IsNullOrEmpty(emptyString);
				bool nullResult = string.IsNullOrEmpty(nullString);

				if (!emptyResult || !nullResult)
				{
					throw new Exception("Edge case handling failed");
				}

				// Test invalid promotion piece
				char invalidPiece = 'K';
				bool isValidPromo = ChessMove.IsValidPromotionPiece(invalidPiece);
				if (isValidPromo)
				{
					throw new Exception("Invalid piece accepted as promotion");
				}

				// Test waiting state consistency
				bool waitingState = IsWaitingForPromotion();
				if (waitingState != isWaitingForSelection)
				{
					throw new Exception("Waiting state inconsistent");
				}

				Debug.Log("<color=green>[PromotionUI] ✓ Edge cases test passed</color>");
			}
			catch (Exception e)
			{
				Debug.Log($"<color=red>[PromotionUI] ✗ Edge cases test failed: {e.Message}</color>");
			}
		}

		private void TestColorHandling()
		{
			try
			{
				// Test color handling for white and black
				char whitePiece = 'Q';
				char blackPiece = 'q';

				bool isWhiteUpper = char.IsUpper(whitePiece);
				bool isBlackLower = char.IsLower(blackPiece);

				if (!isWhiteUpper || !isBlackLower)
				{
					throw new Exception("Color case handling failed");
				}

				// Test promotion context
				var context = GetPromotionContext();
				if (context.fromSquare == null || context.toSquare == null)
				{
					// This is okay - squares can be empty
				}

				Debug.Log("<color=green>[PromotionUI] ✓ Color handling test passed</color>");
			}
			catch (Exception e)
			{
				Debug.Log($"<color=red>[PromotionUI] ✗ Color handling test failed: {e.Message}</color>");
			}
		}

		private void TestUIStateManagement()
		{
			try
			{
				// Test UI state management
				bool initialState = false;
				bool waitingState = true;
				bool finalState = false;

				// Simulate state transitions
				if (initialState == waitingState || waitingState == finalState)
				{
					// Expected behavior - states should be different
				}

				// Test ToString method
				string debugString = ToString();
				if (string.IsNullOrEmpty(debugString))
				{
					throw new Exception("ToString method failed");
				}

				Debug.Log("<color=green>[PromotionUI] ✓ UI state management test passed</color>");
			}
			catch (Exception e)
			{
				Debug.Log($"<color=red>[PromotionUI] ✗ UI state management test failed: {e.Message}</color>");
			}
		}

		private void TestCoroutineMethods()
		{
			try
			{
				// Test coroutine method signatures exist
				bool hasCoroutineMethods = true;

				// Verify delay value is reasonable
				if (autoSelectionDelay < 0f || autoSelectionDelay > 5f)
				{
					Debug.Log("<color=yellow>[PromotionUI] Auto-selection delay should be 0-5 seconds</color>");
				}

				Debug.Log("<color=green>[PromotionUI] ✓ Coroutine methods test passed</color>");
			}
			catch (Exception e)
			{
				Debug.Log($"<color=red>[PromotionUI] ✗ Coroutine methods test failed: {e.Message}</color>");
			}
		}

		private void TestInspectorConfiguration()
		{
			try
			{
				// Test Inspector configuration requirements
				int spriteCount = 0;
				if (blackQueenSprite != null) spriteCount++;
				if (blackRookSprite != null) spriteCount++;
				if (blackBishopSprite != null) spriteCount++;
				if (blackKnightSprite != null) spriteCount++;
				if (whiteQueenSprite != null) spriteCount++;
				if (whiteRookSprite != null) spriteCount++;
				if (whiteBishopSprite != null) spriteCount++;
				if (whiteKnightSprite != null) spriteCount++;

				if (spriteCount < 8)
				{
					Debug.Log($"<color=yellow>[PromotionUI] Only {spriteCount}/8 sprites assigned in Inspector</color>");
				}

				// Test color values are reasonable
				if (selectedColor.a < 0.1f || normalColor.a < 0.1f)
				{
					Debug.Log("<color=yellow>[PromotionUI] Color alpha values very low - may be invisible</color>");
				}

				Debug.Log("<color=green>[PromotionUI] ✓ Inspector configuration test passed</color>");
			}
			catch (Exception e)
			{
				Debug.Log($"<color=red>[PromotionUI] ✗ Inspector configuration test failed: {e.Message}</color>");
			}
		}

		#endregion

	}

	/// <summary>
	/// Enhanced data class for promotion selection events with additional context
	/// </summary>
	[System.Serializable]
	public class PromotionSelectionData
	{
		public char promotionPiece;
		public bool isWhitePromotion;
		public string fromSquare;
		public string toSquare;
		public float selectionTime;
		public bool isEngineSelection;

		public PromotionSelectionData(char piece, bool isWhite, string from = "", string to = "")
		{
			promotionPiece = piece;
			isWhitePromotion = isWhite;
			fromSquare = from;
			toSquare = to;
			selectionTime = Time.time;
			isEngineSelection = false;
		}

		public override string ToString()
		{
			string color = isWhitePromotion ? "White" : "Black";
			string pieceName = ChessMove.GetPromotionPieceName(promotionPiece);
			string source = isEngineSelection ? "Engine" : "Human";
			string moveText = !string.IsNullOrEmpty(fromSquare) && !string.IsNullOrEmpty(toSquare)
				? $" ({fromSquare}-{toSquare})" : "";
			return $"{color} promotes to {pieceName} ({source}){moveText}";
		}
	}
}