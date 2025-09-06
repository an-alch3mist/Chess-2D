# Source: `PromotionUI.cs` — Unity modal dialog system for chess pawn promotion selection with auto-timeout

## Short description
Implements a Unity UI component for handling chess pawn promotion selection through a modal dialog interface. Provides piece selection buttons (Queen, Rook, Bishop, Knight) with configurable auto-timeout functionality and visual feedback for both white and black piece promotions.

## Metadata
* **Filename:** `PromotionUI.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependencies:** System, System.Collections, UnityEngine, UnityEngine.UI, TMPro
* **Public types:** `PromotionUI (class), PromotionSelectionData (class)`
* **Unity version:** Uses TextMeshPro and modern UI system (Unity 2019.3+)

## Public API Summary
| Type | Member | Signature | Short Purpose | OneLiner Call |
|------|---------|-----------|---------------|---------------|
| System.Action<char> | Event | public System.Action<char> OnPromotionSelected | Fired when piece selected | ui.OnPromotionSelected += (piece) => {}; |
| System.Action | Event | public System.Action OnPromotionCancelled | Fired when promotion cancelled | ui.OnPromotionCancelled += () => {}; |
| void | Method | public void ShowPromotionDialog(bool isWhite, string fromSquare = "", string toSquare = "") | Displays promotion dialog | ui.ShowPromotionDialog(true, "e7", "e8"); |
| void | Method | public void HideDialog() | Hides promotion dialog | ui.HideDialog(); |
| bool | Method | public bool IsWaitingForPromotion() | Checks if awaiting selection | var waiting = ui.IsWaitingForPromotion(); |
| void | Method | public void SelectDefaultPromotion() | Forces default piece selection | ui.SelectDefaultPromotion(); |
| void | Method | public void TestPromotionUI() | Tests UI functionality | ui.TestPromotionUI(); |
| void | Method | public void TestTimeout() | Tests timeout behavior | ui.TestTimeout(); |
| PromotionSelectionData | Constructor | public PromotionSelectionData(char piece, bool isWhite, string from = "", string to = "") | Creates selection data | var data = new PromotionSelectionData('Q', true); |
| string | Method | public override string ToString() | Returns formatted string | var str = data.ToString(); |

## MonoBehaviour Integration
**Note:** This class inherits MonoBehaviour.

**Unity Lifecycle Methods:**
* `Awake()` - Validates UI component references, sets up button click listeners, hides dialog initially
* No other Unity lifecycle methods are implemented

**SerializeField Dependencies:**
* `[SerializeField] private GameObject promotionPanel` - Main dialog panel container
* `[SerializeField] private TextMeshProUGUI titleText` - Dialog title text component
* `[SerializeField] private TextMeshProUGUI timeoutText` - Countdown timer text display
* `[SerializeField] private Button queenButton` - Queen selection button
* `[SerializeField] private Button rookButton` - Rook selection button
* `[SerializeField] private Button bishopButton` - Bishop selection button
* `[SerializeField] private Button knightButton` - Knight selection button
* `[SerializeField] private Button cancelButton` - Cancel/close dialog button
* `[SerializeField] private float timeoutSeconds` - Auto-timeout duration (default 3s)
* `[SerializeField] private char defaultPromotionPiece` - Default piece selection (default 'Q')
* `[SerializeField] private bool showTimeoutCountdown` - Show countdown text toggle
* `[SerializeField] private Color whiteButtonColor` - Button color for white promotions
* `[SerializeField] private Color blackButtonColor` - Button color for black promotions
* `[SerializeField] private Color selectedColor` - Highlight color for selected button

## Important Types

### `PromotionUI`
* **Kind:** class inheriting MonoBehaviour
* **Responsibility:** Manages pawn promotion UI dialog with timeout functionality and visual feedback
* **Constructor(s):** Unity MonoBehaviour (no explicit constructor)
* **Public Properties:** None
* **Public Methods:**
  * **`public void ShowPromotionDialog(bool isWhite, string fromSquare = "", string toSquare = "")`**
    * Description: Displays modal promotion dialog for specified player color
    * Parameters: `isWhite : bool — true for white promotion, false for black`, `fromSquare : string — source square notation (optional)`, `toSquare : string — target square notation (optional)`
    * Returns: `void` + call example: `promotionUI.ShowPromotionDialog(true, "e7", "e8");`
    * Notes: Starts timeout coroutine and configures UI colors based on player
  * **`public bool IsWaitingForPromotion()`**
    * Description: Returns current dialog state
    * Parameters: None
    * Returns: `bool — true if dialog is active and awaiting selection` + call example: `var waiting = ui.IsWaitingForPromotion();`

### `PromotionSelectionData`
* **Kind:** class marked [System.Serializable]
* **Responsibility:** Data container for promotion selection events with metadata
* **Constructor(s):** `public PromotionSelectionData(char piece, bool isWhite, string from = "", string to = "")` - initializes all fields including timestamp
* **Public Properties:**
  * `promotionPiece` — `char` — Selected piece character ('Q', 'R', 'B', 'N') (`get/set`)
  * `isWhitePromotion` — `bool` — Player color flag (`get/set`)
  * `fromSquare` — `string` — Source square notation (`get/set`)
  * `toSquare` — `string` — Target square notation (`get/set`)
  * `selectionTime` — `float` — Unity Time.time when selection made (`get/set`)
* **Public Methods:**
  * **`public override string ToString()`**
    * Description: Returns formatted promotion description
    * Parameters: None
    * Returns: `string — formatted as "Color promotes to PieceName"` + call example: `var desc = data.ToString();`

## Example Usage
**Required namespaces:**
```csharp
// using System;
// using UnityEngine;
// using GPTDeepResearch;
```

**For MonoBehaviour classes:**
```csharp
public class ExampleUsage : MonoBehaviour 
{
    [SerializeField] private PromotionUI promotionUI; // Assign in Inspector
    
    private void PromotionUI_Check()
    {
        // Setup event handlers
        promotionUI.OnPromotionSelected += OnPieceSelected;
        promotionUI.OnPromotionCancelled += OnPromotionCancelled;
        
        // Test dialog display
        promotionUI.ShowPromotionDialog(true, "e7", "e8");
        var isWaiting = promotionUI.IsWaitingForPromotion();
        
        // Test data class
        var selectionData = new PromotionSelectionData('Q', true, "e7", "e8");
        var dataString = selectionData.ToString();
        
        Debug.Log($"API Results: Dialog shown, Waiting: {isWaiting}, Data: {dataString}");
    }
    
    private void OnPieceSelected(char piece) { /* Handle selection */ }
    private void OnPromotionCancelled() { /* Handle cancellation */ }
}
```

## Control Flow & Responsibilities
Dialog display → button setup → timeout coroutine → piece selection → event notification → dialog hide

## Performance & Threading
Main thread UI operations, coroutine-based timeout system, minimal allocations during selection

## Cross-file Dependencies
References ChessMove.GetPromotionPieceName() for piece name formatting and display text generation

## Major Functionality
Modal promotion dialog, auto-timeout with countdown, visual button feedback, event-driven selection notification

`checksum: A7F3D92E v0.3.min`