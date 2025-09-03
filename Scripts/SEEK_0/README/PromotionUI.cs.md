# source: `PromotionUI.cs` — Modal UI component for chess pawn promotion selection with timeout

## Short description (2–4 sentences)
Implements a Unity MonoBehaviour-based UI system for handling pawn promotion in chess games. Provides a modal dialog with piece selection buttons (Queen, Rook, Bishop, Knight), configurable auto-timeout with default selection, and visual feedback. Integrates with the ChessBoard and ChessMove systems through events and supports both white and black piece promotions.

## Metadata

* **Filename:** `PromotionUI.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `System`, `System.Collections`, `UnityEngine`, `UnityEngine.UI`, `TMPro`
* **Estimated lines:** 345
* **Estimated chars:** 11,500
* **Public types:** `PromotionUI (class inherits MonoBehaviour)`, `PromotionSelectionData (class)`
* **Unity version / Target framework:** Unity 2020.3+ / .NET Standard 2.0
* **Dependencies:** `ChessMove.cs` (referenced via ChessMove.GetPromotionPieceName method)

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| System.Action<char> (event) | OnPromotionSelected | public System.Action<char> OnPromotionSelected | Event fired when promotion piece selected | promotionUI.OnPromotionSelected += (piece) => {...} |
| System.Action (event) | OnPromotionCancelled | public System.Action OnPromotionCancelled | Event fired when promotion cancelled | promotionUI.OnPromotionCancelled += () => {...} |
| void | ShowPromotionDialog | public void ShowPromotionDialog(bool isWhite, string fromSquare = "", string toSquare = "") | Shows promotion dialog for human player | promotionUI.ShowPromotionDialog(true, "e7", "e8") |
| void | HideDialog | public void HideDialog() | Hides promotion dialog | promotionUI.HideDialog() |
| bool | IsWaitingForPromotion | public bool IsWaitingForPromotion() | Check if waiting for promotion selection | bool waiting = promotionUI.IsWaitingForPromotion() |
| void | SelectDefaultPromotion | public void SelectDefaultPromotion() | Force selection of default piece | promotionUI.SelectDefaultPromotion() |
| void | TestPromotionUI | public void TestPromotionUI() | Test promotion UI functionality | promotionUI.TestPromotionUI() |
| void | TestTimeout | public void TestTimeout() | Test timeout functionality | promotionUI.TestTimeout() |

## Important types — details

### `PromotionUI`
* **Kind:** class (inherits MonoBehaviour)
* **Responsibility:** Handles pawn promotion UI with modal dialog, timeout, and visual feedback
* **Constructor(s):** Unity MonoBehaviour - no explicit constructor, uses Awake()
* **Public properties / fields:**
  * OnPromotionSelected — System.Action<char> — Event triggered when promotion piece selected
  * OnPromotionCancelled — System.Action — Event triggered when promotion cancelled
* **Public methods:**
  * **Signature:** `public void ShowPromotionDialog(bool isWhite, string fromSquare = "", string toSquare = "")`
  * **Description:** Shows promotion dialog for human player with configurable squares
  * **Parameters:** 
    * isWhite : bool — True if white promoting, false for black
    * fromSquare : string — Source square for context (optional)
    * toSquare : string — Target square for context (optional)
  * **Returns:** void — `promotionUI.ShowPromotionDialog(true, "e7", "e8")`
  * **Side effects / state changes:** Shows modal panel, starts timeout coroutine, sets internal state
  
  * **Signature:** `public void HideDialog()`
  * **Description:** Hides the promotion dialog and cleans up state
  * **Returns:** void — `promotionUI.HideDialog()`
  * **Side effects / state changes:** Deactivates panel, stops timeout coroutine, resets state
  
  * **Signature:** `public bool IsWaitingForPromotion()`
  * **Description:** Returns whether UI is currently waiting for promotion selection
  * **Returns:** bool — `bool waiting = promotionUI.IsWaitingForPromotion()`
  
  * **Signature:** `public void SelectDefaultPromotion()`
  * **Description:** Programmatically selects the default promotion piece
  * **Returns:** void — `promotionUI.SelectDefaultPromotion()`
  * **Side effects / state changes:** Triggers promotion selection with default piece
  
  * **Signature:** `public void TestPromotionUI()`
  * **Description:** Runs automated test sequence for UI functionality
  * **Returns:** void — `promotionUI.TestPromotionUI()`
  * **Side effects / state changes:** Starts test coroutine sequence
  
  * **Signature:** `public void TestTimeout()`
  * **Description:** Tests timeout functionality with shortened duration
  * **Returns:** void — `promotionUI.TestTimeout()`
  * **Side effects / state changes:** Temporarily modifies timeout setting

### `PromotionSelectionData`
* **Kind:** class (serializable data class)
* **Responsibility:** Data container for promotion selection events with metadata
* **Constructor(s):** 
  * `public PromotionSelectionData(char piece, bool isWhite, string from = "", string to = "")` — Creates selection data with piece and context
* **Public properties / fields:**
  * promotionPiece — char — The selected promotion piece ('Q', 'R', 'B', 'N')
  * isWhitePromotion — bool — Whether this was a white promotion
  * fromSquare — string — Source square of promotion move
  * toSquare — string — Target square of promotion move  
  * selectionTime — float — Time.time when selection was made
* **Public methods:**
  * **Signature:** `public override string ToString()`
  * **Description:** Returns formatted string describing the promotion
  * **Returns:** string — `string desc = data.ToString()`

## MonoBehaviour special rules
* **Note: MonoBehaviour**

**Awake()**
- Called on script load. Validates UI component references (calls ValidateComponents()), sets up button listeners (calls SetupButtonListeners()), and hides the dialog initially (calls HideDialog()).
- Does not access Unity scene objects that require Start().

## Example usage
```csharp
// Required namespace
using GPTDeepResearch;

// Basic usage - show promotion dialog
promotionUI.ShowPromotionDialog(true, "e7", "e8");

// Listen for selection
promotionUI.OnPromotionSelected += (piece) => {
    Debug.Log($"Player selected: {piece}");
};

// Check if waiting
if (promotionUI.IsWaitingForPromotion()) {
    // UI is active
}

// Force default selection
promotionUI.SelectDefaultPromotion();
```

## Control flow / responsibilities & high-level algorithm summary
Modal dialog workflow: ShowPromotionDialog() → user selection or timeout → event notification → HideDialog(). Handles UI setup, button color coding by piece color, timeout countdown with visual feedback.

## Performance, allocations, and hotspots / Threading / async considerations
Uses Unity coroutines for timeout countdown (0.1s intervals). UI updates on main thread only.

## Security / safety / correctness concerns
Validates UI components in Awake(), handles null references gracefully with Debug.Log warnings.

## Tests, debugging & observability
Built-in test methods (TestPromotionUI, TestTimeout), extensive Debug.Log throughout with color coding for status tracking.

## Cross-file references
Depends on `ChessMove.cs` for GetPromotionPieceName() method used in logging and UI display.

## TODO / Known limitations / Suggested improvements
<!-- No explicit TODO comments found in source code. (only if I explicitly mentioned in the prompt) -->

## Appendix
Key private methods: ValidateComponents(), SetupButtonListeners(), TimeoutCountdown() coroutine, SelectPromotion() for piece selection handling.

## General Note: important behaviors
Major functionality: PawnPromotion UI with timeout system, supports both manual selection and automatic fallback to Queen after configurable timeout period.


`checksum: a7f3e2b1`