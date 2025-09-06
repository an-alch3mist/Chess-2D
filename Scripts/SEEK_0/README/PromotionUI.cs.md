# Source: `PromotionUI.cs` — Modal UI component for chess pawn promotion selection with timeout

* UI component that manages pawn promotion dialog with piece selection buttons and automatic timeout functionality.

## Short description (2–4 sentences)
This file implements a Unity UI component for handling pawn promotion selection in chess games. It provides a modal dialog with buttons for selecting promotion pieces (Queen, Rook, Bishop, Knight) and includes auto-timeout functionality with configurable default selection. The component integrates with Unity's UI system using TextMeshPro and Button components, supporting both white and black piece promotions with visual feedback.

## Metadata

* **Filename:** `PromotionUI.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `System`, `System.Collections`, `UnityEngine`, `UnityEngine.UI`, `TMPro`
* **Estimated lines:** 380
* **Estimated chars:** 14,500
* **Public types:** `PromotionUI (class), PromotionSelectionData (class)`
* **Unity version / Target framework:** Unity 2020.3+ / .NET Standard 2.0
* **Dependencies:** Unity UI package, TextMeshPro package, `ChessMove.cs` (for `GetPromotionPieceName` method)

## Public API summary (table)

**Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call**
--- | --- | --- | --- | ---
System.Action<char> | OnPromotionSelected | public System.Action<char> OnPromotionSelected | Event fired when promotion piece is selected | promotionUI.OnPromotionSelected += (piece) => {};
System.Action | OnPromotionCancelled | public System.Action OnPromotionCancelled | Event fired when promotion is cancelled | promotionUI.OnPromotionCancelled += () => {};
void | ShowPromotionDialog | public void ShowPromotionDialog(bool isWhite, string fromSquare = "", string toSquare = "") | Shows promotion dialog for human player | promotionUI.ShowPromotionDialog(true, "e7", "e8");
void | HideDialog | public void HideDialog() | Hides the promotion dialog | promotionUI.HideDialog();
bool | IsWaitingForPromotion | public bool IsWaitingForPromotion() | Checks if currently waiting for promotion selection | bool waiting = promotionUI.IsWaitingForPromotion();
void | SelectDefaultPromotion | public void SelectDefaultPromotion() | Force selection of default piece programmatically | promotionUI.SelectDefaultPromotion();
void | TestPromotionUI | public void TestPromotionUI() | Tests promotion UI functionality | promotionUI.TestPromotionUI();
void | TestTimeout | public void TestTimeout() | Tests timeout functionality | promotionUI.TestTimeout();

## Important types — details

### `PromotionUI` (class)
* **Kind:** class inheriting MonoBehaviour
* **Note:** MonoBehaviour
* **Responsibility:** Manages pawn promotion UI dialog with piece selection and timeout functionality.
* **Constructor(s):** Unity MonoBehaviour (no explicit constructor)
* **Public properties / fields:** 
  * `OnPromotionSelected — System.Action<char> — event fired when promotion piece selected`
  * `OnPromotionCancelled — System.Action — event fired when promotion cancelled`

* **Public methods:**
  * **Signature:** `public void ShowPromotionDialog(bool isWhite, string fromSquare = "", string toSquare = "")`
    * **Description:** Shows the promotion dialog for human player selection.
    * **Parameters:** 
      * `isWhite : bool — true if white is promoting, false for black`
      * `fromSquare : string — source square for context (optional)`
      * `toSquare : string — target square for context (optional)`
    * **Returns:** void — promotionUI.ShowPromotionDialog(true, "e7", "e8")
    * **Side effects / state changes:** Sets isWaitingForSelection=true, shows UI panel, starts timeout coroutine
    * **Notes:** Validates not already waiting before showing dialog

  * **Signature:** `public void HideDialog()`
    * **Description:** Hides the promotion dialog and resets state.
    * **Returns:** void — promotionUI.HideDialog()
    * **Side effects / state changes:** Sets isWaitingForSelection=false, hides UI panel, stops timeout coroutine

  * **Signature:** `public bool IsWaitingForPromotion()`
    * **Description:** Checks if currently waiting for promotion selection.
    * **Returns:** bool — bool waiting = promotionUI.IsWaitingForPromotion()
    * **Notes:** Used to prevent overlapping promotion dialogs

  * **Signature:** `public void SelectDefaultPromotion()`
    * **Description:** Forces selection of the configured default piece.
    * **Returns:** void — promotionUI.SelectDefaultPromotion()
    * **Side effects / state changes:** Triggers promotion selection with default piece

  * **Signature:** `public void TestPromotionUI()`
    * **Description:** Tests promotion UI functionality with automated sequence.
    * **Returns:** void — promotionUI.TestPromotionUI()
    * **Side effects / state changes:** Starts coroutine showing test promotion dialogs
    * **Notes:** Development/testing method

  * **Signature:** `public void TestTimeout()`
    * **Description:** Tests timeout functionality with shortened duration.
    * **Returns:** void — promotionUI.TestTimeout()
    * **Side effects / state changes:** Temporarily modifies timeout for testing
    * **Notes:** Development/testing method

**Unity Lifecycle Methods:**
* **Awake():**
  * Called on script load. Validates UI component references (calls ValidateComponents()), sets up button click listeners (calls SetupButtonListeners()), and hides dialog initially (calls HideDialog()).
  * Does not access Unity scene objects that require Start().

### `PromotionSelectionData` (class)
* **Kind:** class with [System.Serializable] attribute
* **Responsibility:** Data container for promotion selection events and logging.
* **Constructor(s):** `PromotionSelectionData(char piece, bool isWhite, string from = "", string to = "")`
* **Public properties / fields:**
  * `promotionPiece — char — the selected promotion piece character`
  * `isWhitePromotion — bool — true if white promotion, false for black`
  * `fromSquare — string — source square of promotion move`
  * `toSquare — string — target square of promotion move`
  * `selectionTime — float — Time.time when selection was made`

* **Public methods:**
  * **Signature:** `public override string ToString()`
    * **Description:** Returns formatted string representation of promotion data.
    * **Returns:** string — string result = selectionData.ToString()
    * **Notes:** Format: "{Color} promotes to {PieceName}"

## Example usage

```csharp
// Required namespaces:
// using System;
// using System.Collections;
// using UnityEngine;
// using GPTDeepResearch;
// using TMPro;
// using UnityEngine.UI;

public class ExampleUsage : MonoBehaviour 
{
    [SerializeField] private PromotionUI promotionUI; // Assign in Inspector
    
    private IEnumerator PromotionUI_Check()
    {
        // Test basic promotion dialog
        Debug.Log("<color=cyan>Testing promotion UI...</color>");
        
        // Setup event listeners
        promotionUI.OnPromotionSelected += OnPromotionPieceSelected;
        promotionUI.OnPromotionCancelled += OnPromotionCancelled;
        
        // Show white promotion dialog
        promotionUI.ShowPromotionDialog(true, "e7", "e8");
        Debug.Log("<color=green>White promotion dialog shown</color>");
        
        // Wait for user selection or timeout
        while (promotionUI.IsWaitingForPromotion())
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        yield return new WaitForSeconds(1f);
        
        // Show black promotion dialog
        promotionUI.ShowPromotionDialog(false, "d2", "d1");
        Debug.Log("<color=green>Black promotion dialog shown</color>");
        
        // Wait for selection
        while (promotionUI.IsWaitingForPromotion())
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        // Test programmatic selection
        promotionUI.ShowPromotionDialog(true);
        yield return new WaitForSeconds(0.5f);
        promotionUI.SelectDefaultPromotion();
        Debug.Log("<color=green>Default promotion selected programmatically</color>");
        
        // Test promotion data
        var promotionData = new PromotionSelectionData('Q', true, "a7", "a8");
        Debug.Log($"<color=blue>Promotion data: {promotionData.ToString()}</color>");
        
        // Expected output: "White promotes to Queen"
        Debug.Log("<color=green>✓ PromotionUI test completed successfully</color>");
        
        yield break;
    }
    
    private void OnPromotionPieceSelected(char piece)
    {
        string pieceName = ChessMove.GetPromotionPieceName(piece);
        Debug.Log($"<color=green>Promotion selected: {pieceName} ({piece})</color>");
        // Expected output: "Promotion selected: Queen (Q)"
    }
    
    private void OnPromotionCancelled()
    {
        Debug.Log("<color=yellow>Promotion was cancelled</color>");
        // Expected output: "Promotion was cancelled"
    }
}
```

## Control flow / responsibilities & high-level algorithm summary
ShowPromotionDialog() sets up UI, starts timeout coroutine; user clicks trigger SelectPromotion(); timeout auto-selects default piece; events notify listeners; dialog hides after selection.

## Performance, allocations, and hotspots
Minimal allocations; coroutines for timeout countdown; UI updates on main thread only.

## Security / safety / correctness concerns
Requires UI component validation; potential null references if components not assigned in Inspector.

## Tests, debugging & observability
Built-in test methods TestPromotionUI() and TestTimeout(); color-coded Debug.Log statements; event system for monitoring selections.

## Cross-file references
Depends on `ChessMove.cs` for `GetPromotionPieceName()` method; integrates with Unity UI and TextMeshPro systems.

<!-- TODO / Known limitations / Suggested improvements
* TODO comments in code suggest adding piece images/sprites to buttons
* Could add keyboard shortcuts for piece selection
* Timeout duration could be dynamic based on game time control
* Visual animations for button selection feedback could be enhanced
* Audio feedback for selection/timeout events
* Localization support for piece names and UI text
(only if I explicitly mentioned in the prompt) -->

## Appendix
Key private methods: SetupPromotionUI() configures colors/text, TimeoutCountdown() handles auto-selection, HighlightSelectedButton() provides visual feedback, SelectPromotion() processes selection and fires events.

## General Note: important behaviors
Major functionality: Pawn Promotion UI with timeout system, visual feedback, event-driven architecture for chess game integration, support for both white/black promotions with configurable defaults.

`checksum: a7f2b3c8 (v0.3)`