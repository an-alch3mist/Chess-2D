# Source: `PromotionUI.cs` — Modal UI component for chess pawn promotion selection with timeout

UI system that displays a modal dialog when a pawn reaches the last rank, allowing players to select promotion piece (Queen, Rook, Bishop, Knight) with configurable auto-timeout defaulting to Queen. Integrates with ChessBoard and ChessMove systems through event-driven architecture.

## Short description (2–4 sentences)
Implements a Unity UI component for handling chess pawn promotion selection through a modal dialog interface. Provides visual feedback with piece selection buttons, configurable timeout with countdown display, and automatic fallback to Queen selection. Features comprehensive testing methods and supports both white and black piece promotions with appropriate visual styling.

## Metadata

* **Filename:** `PromotionUI.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `System, System.Collections, UnityEngine, UnityEngine.UI, TMPro`
* **Estimated lines:** 430
* **Estimated chars:** 15,200
* **Public types:** `PromotionUI (class inherits MonoBehaviour), PromotionSelectionData (class)`
* **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
* **Dependencies:** ChessMove.cs (for GetPromotionPieceName, IsValidPromotionPiece methods)

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| System.Action<char> (delegate) | OnPromotionSelected | public System.Action<char> OnPromotionSelected | Event fired when player selects promotion piece | promotionUI.OnPromotionSelected += (piece) => {}; |
| System.Action (delegate) | OnPromotionCancelled | public System.Action OnPromotionCancelled | Event fired when promotion is cancelled | promotionUI.OnPromotionCancelled += () => {}; |
| void | ShowPromotionDialog | public void ShowPromotionDialog(bool isWhite, string fromSquare = "", string toSquare = "") | Display promotion dialog for piece selection | promotionUI.ShowPromotionDialog(true, "e7", "e8"); |
| void | HideDialog | public void HideDialog() | Hide the promotion dialog | promotionUI.HideDialog(); |
| bool | IsWaitingForPromotion | public bool IsWaitingForPromotion() | Check if dialog is currently waiting for selection | bool waiting = promotionUI.IsWaitingForPromotion(); |
| void | SelectDefaultPromotion | public void SelectDefaultPromotion() | Force selection of default piece programmatically | promotionUI.SelectDefaultPromotion(); |
| void | RunAllTests | public void RunAllTests() | Execute comprehensive test suite | promotionUI.RunAllTests(); |
| string | ToString | public override string ToString() | Debug string representation | string info = promotionUI.ToString(); |

## Important types — details

### `PromotionUI` (class inherits MonoBehaviour)
* **Kind:** class inherits MonoBehaviour
* **Responsibility:** Manages pawn promotion UI dialog with timeout functionality and event handling.
* **Constructor(s):** MonoBehaviour (Unity managed lifecycle)

* **Public properties / fields:**
  * `OnPromotionSelected — System.Action<char> — Event triggered when promotion piece is selected`
  * `OnPromotionCancelled — System.Action — Event triggered when promotion is cancelled`

* **Public methods:**
  * **Signature:** `public void ShowPromotionDialog(bool isWhite, string fromSquare = "", string toSquare = "")`
    * **Description:** Displays the promotion dialog for the specified player color.
    * **Parameters:**
      * `isWhite : bool — True if white is promoting, false for black`
      * `fromSquare : string — Source square notation (optional, for display context)`
      * `toSquare : string — Target square notation (optional, for display context)`
    * **Returns:** void — promotionUI.ShowPromotionDialog(true, "e7", "e8")
    * **Side effects / state changes:** Sets isWaitingForSelection=true, starts timeout coroutine, shows UI panel
    * **Notes:** Validates not already waiting, sets up UI colors based on piece color

  * **Signature:** `public void HideDialog()`
    * **Description:** Hides the promotion dialog and resets state.
    * **Returns:** void — promotionUI.HideDialog()
    * **Side effects / state changes:** Sets panel inactive, stops timeout coroutine, resets waiting state

  * **Signature:** `public bool IsWaitingForPromotion()`
    * **Description:** Returns whether dialog is currently waiting for user selection.
    * **Returns:** bool waiting = promotionUI.IsWaitingForPromotion()

  * **Signature:** `public void SelectDefaultPromotion()`
    * **Description:** Programmatically selects the default promotion piece.
    * **Returns:** void — promotionUI.SelectDefaultPromotion()
    * **Side effects / state changes:** Triggers selection of defaultPromotionPiece, fires OnPromotionSelected event

  * **Signature:** `public void RunAllTests()`
    * **Description:** Executes comprehensive test suite for validation.
    * **Returns:** void — promotionUI.RunAllTests()
    * **Notes:** Logs test results with color-coded Debug messages

  * **Signature:** `public override string ToString()`
    * **Description:** Returns debug string with current state information.
    * **Returns:** string info = promotionUI.ToString()

**Note: MonoBehaviour**

* **Awake()** - Called on script load. Validates UI component references (calls ValidateComponents()), sets up button click listeners (calls SetupButtonListeners()), and hides the dialog panel initially (calls HideDialog()).

### `PromotionSelectionData` (class)
* **Kind:** class marked [System.Serializable]
* **Responsibility:** Data container for promotion selection event information.
* **Constructor(s):** `public PromotionSelectionData(char piece, bool isWhite, string from = "", string to = "")`

* **Public properties / fields:**
  * `promotionPiece — char — The selected promotion piece character`
  * `isWhitePromotion — bool — Whether this is a white piece promotion`
  * `fromSquare — string — Source square notation`
  * `toSquare — string — Target square notation`
  * `selectionTime — float — Unity Time.time when selection was made`

* **Public methods:**
  * **Signature:** `public override string ToString()`
    * **Description:** Returns human-readable description of the promotion.
    * **Returns:** string description = selectionData.ToString()

## Example usage

```csharp
// Required namespaces:
// using System;
// using System.Collections;
// using UnityEngine;
// using GPTDeepResearch;

public class ExampleUsage : MonoBehaviour 
{
    [SerializeField] private PromotionUI promotionUI; // Assign in Inspector
    
    private IEnumerator PromotionUI_Check()
    {
        // Subscribe to promotion events
        promotionUI.OnPromotionSelected += OnPieceSelected;
        promotionUI.OnPromotionCancelled += OnPromotionCancelled;
        
        // Show promotion dialog for white player
        promotionUI.ShowPromotionDialog(true, "e7", "e8");
        // Expected output: "[PromotionUI] Showing promotion dialog for White"
        Debug.Log("<color=cyan>Promotion dialog shown for white</color>");
        
        // Check if waiting for selection
        bool isWaiting = promotionUI.IsWaitingForPromotion();
        // Expected output: "Currently waiting: True"
        Debug.Log($"<color=green>Currently waiting: {isWaiting}</color>");
        
        // Wait for user selection or timeout
        yield return new WaitForSeconds(1.0f);
        
        // Force default selection programmatically
        if (promotionUI.IsWaitingForPromotion())
        {
            promotionUI.SelectDefaultPromotion();
            // Expected output: "[PromotionUI] Selected promotion: Queen"
            Debug.Log("<color=green>Default promotion selected</color>");
        }
        
        // Create promotion selection data
        var selectionData = new PromotionSelectionData('Q', true, "e7", "e8");
        // Expected output: "White promotes to Queen"
        Debug.Log($"<color=green>Selection: {selectionData.ToString()}</color>");
        
        // Test the component
        promotionUI.RunAllTests();
        // Expected output: "[PromotionUI] ✓ All tests completed successfully"
        
        // Show current state
        // Expected output: "PromotionUI[Waiting:False, Side:White, Selected:Q, Default:Q, Timeout:3s]"
        Debug.Log($"<color=cyan>State: {promotionUI.ToString()}</color>");
        
        yield break;
    }
    
    private void OnPieceSelected(char piece)
    {
        // Expected output: "Player selected: Q"
        Debug.Log($"<color=green>Player selected: {piece}</color>");
    }
    
    private void OnPromotionCancelled()
    {
        // Expected output: "Promotion was cancelled"
        Debug.Log("<color=yellow>Promotion was cancelled</color>");
    }
}
```

## Control flow / responsibilities & high-level algorithm summary /Side effects and I/O
Event-driven UI system: ShowPromotionDialog() → setup UI → start timeout coroutine → user clicks button or timeout → SelectPromotion() → fire OnPromotionSelected event → hide dialog. Manages UI state through isWaitingForSelection flag.

## Performance, allocations, and hotspots / Threading / async considerations
Minimal allocations. Uses Unity coroutines for timeout countdown (0.1s intervals). Main thread only.

## Security / safety / correctness concerns
Validates component references in Awake(). Protects against double-selection through isWaitingForSelection state.

## Tests, debugging & observability
Built-in comprehensive test suite via RunAllTests(). Color-coded Debug.Log statements throughout. ToString() override for state inspection.

## Cross-file references
Depends on ChessMove.cs for GetPromotionPieceName() and IsValidPromotionPiece() static methods.

<!-- ## TODO / Known limitations / Suggested improvements
* Add sound effects for button clicks and timeout warning
* Support for custom promotion piece sets beyond standard chess
* Keyboard shortcuts for piece selection
* Animation transitions for show/hide dialog
(only if I explicitly mentioned in the prompt) -->

## Appendix
Key private methods: ValidateComponents() checks UI references, SetupButtonListeners() wires button events, TimeoutCountdown() coroutine handles auto-selection, SelectPromotion() processes piece selection.

## General Note: important behaviors
Major functionality: Pawn Promotion selection with configurable timeout and visual feedback. Comprehensive test coverage through RunAllTests() method validates all public functionality.

`checksum: a7f3d82e (v0.3)`