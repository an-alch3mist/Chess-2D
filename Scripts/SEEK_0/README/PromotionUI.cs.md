# Source: `PromotionUI.cs` — Modal UI for chess pawn promotion piece selection with sprite-based buttons

Modal dialog component for handling pawn promotion in chess games. Provides mandatory piece selection interface with support for both human player interaction and automated engine moves. Features sprite-based buttons for visual piece representation and coroutine support for asynchronous operations.

## Short description (2–4 sentences)
Implements a Unity UI system for chess pawn promotion selection using sprite-based buttons. Handles both human player interactions through modal dialogs and automated engine selections. Provides comprehensive event system with detailed selection context and supports coroutine-based async operations. Designed for Unity 2020.3 compatibility with robust validation and testing capabilities.

## Metadata

* **Filename:** `PromotionUI.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `System, System.Collections, UnityEngine, UnityEngine.UI, TMPro`
* **Estimated lines:** 850
* **Estimated chars:** 28,000
* **Public types:** `PromotionUI (class), PromotionSelectionData (class)`
* **Unity version / Target framework:** `Unity 2020.3 / .NET Standard 2.0`
* **Dependencies:** `ChessMove.cs`, Unity UI system, TextMeshPro package

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| System.Action<char> (delegate) | OnPromotionSelected | public System.Action<char> OnPromotionSelected | Event fired when promotion piece selected | promotionUI.OnPromotionSelected += (piece) => {}; |
| System.Action<PromotionSelectionData> (delegate) | OnPromotionSelectedWithData | public System.Action<PromotionSelectionData> OnPromotionSelectedWithData | Event with detailed selection context | promotionUI.OnPromotionSelectedWithData += (data) => {}; |
| void | ShowPromotionDialog | public void ShowPromotionDialog(bool isWhite, string fromSquare = "", string toSquare = "") | Display promotion dialog for human | promotionUI.ShowPromotionDialog(true, "e7", "e8"); |
| IEnumerator | ShowPromotionDialogCoroutine | public IEnumerator ShowPromotionDialogCoroutine(bool isWhite, string fromSquare = "", string toSquare = "") | Show dialog with coroutine support | yield return promotionUI.ShowPromotionDialogCoroutine(true, "e7", "e8"); StartCoroutine(promotionUI.ShowPromotionDialogCoroutine(true, "e7", "e8")); |
| void | SelectPromotionAutomatically | public void SelectPromotionAutomatically(char promotionPiece, bool isWhite) | Auto-select for engine moves | promotionUI.SelectPromotionAutomatically('Q', true); |
| IEnumerator | SelectPromotionAutomaticallyCoroutine | public IEnumerator SelectPromotionAutomaticallyCoroutine(char promotionPiece, bool isWhite) | Auto-select with coroutine support | yield return promotionUI.SelectPromotionAutomaticallyCoroutine('Q', true); StartCoroutine(promotionUI.SelectPromotionAutomaticallyCoroutine('Q', true)); |
| void | HideDialog | public void HideDialog() | Hide promotion dialog | promotionUI.HideDialog(); |
| bool | IsWaitingForPromotion | public bool IsWaitingForPromotion() | Check if waiting for selection | bool waiting = promotionUI.IsWaitingForPromotion(); |
| bool | IsEngineSelection | public bool IsEngineSelection() | Check if engine is selecting | bool isEngine = promotionUI.IsEngineSelection(); |
| (bool, string, string) | GetPromotionContext | public (bool isWhite, string fromSquare, string toSquare) GetPromotionContext() | Get current promotion context | var context = promotionUI.GetPromotionContext(); |
| void | SelectDefaultPromotion | public void SelectDefaultPromotion() | Force default piece selection | promotionUI.SelectDefaultPromotion(); |
| void | RunAllTests | public void RunAllTests() | Run comprehensive test suite | promotionUI.RunAllTests(); |
| string | ToString | public override string ToString() | Debug representation | string debug = promotionUI.ToString(); |

## Important types — details

### `PromotionUI` (class)
* **Kind:** class inherits MonoBehaviour
* **Responsibility:** Manages pawn promotion UI with sprite-based button interface and event system
* **Note:** MonoBehaviour
* **Constructor(s):** Unity instantiated component - no public constructors
* **Public properties / fields:** 
  * OnPromotionSelected — System.Action<char> — Event fired when piece selected (get/set)
  * OnPromotionSelectedWithData — System.Action<PromotionSelectionData> — Event with detailed context (get/set)

* **Public methods:**
  * **Signature:** `public void ShowPromotionDialog(bool isWhite, string fromSquare = "", string toSquare = "")`
    * **Description:** Displays modal promotion dialog for human player selection
    * **Parameters:** 
      * isWhite : bool — true for white promotion, false for black
      * fromSquare : string — source square for context (optional)
      * toSquare : string — target square for context (optional)
    * **Returns:** void — promotionUI.ShowPromotionDialog(true, "e7", "e8")
    * **Side effects / state changes:** Shows UI panel, sets waiting state, configures button sprites
    * **Notes:** Mandatory selection, no timeout or cancel options

  * **Signature:** `public IEnumerator ShowPromotionDialogCoroutine(bool isWhite, string fromSquare = "", string toSquare = "")`
    * **Description:** Show promotion dialog with coroutine support for async operations
    * **Parameters:** 
      * isWhite : bool — true for white promotion
      * fromSquare : string — source square context  
      * toSquare : string — target square context
    * **Returns:** IEnumerator — yield return promotionUI.ShowPromotionDialogCoroutine(true); StartCoroutine(promotionUI.ShowPromotionDialogCoroutine(true)) (yield return new WaitUntil(() => !isWaitingForSelection))
    * **Side effects / state changes:** Same as ShowPromotionDialog but waits for completion

  * **Signature:** `public void SelectPromotionAutomatically(char promotionPiece, bool isWhite)`
    * **Description:** Auto-select promotion piece for engine moves
    * **Parameters:**
      * promotionPiece : char — piece to promote to (Q, R, B, N)
      * isWhite : bool — true if white is promoting
    * **Returns:** void — promotionUI.SelectPromotionAutomatically('Q', true)
    * **Side effects / state changes:** Sets engine selection flag, validates piece, triggers selection
    * **Notes:** Validates promotion piece, uses default if invalid

  * **Signature:** `public IEnumerator SelectPromotionAutomaticallyCoroutine(char promotionPiece, bool isWhite)`
    * **Description:** Auto-select with coroutine support
    * **Parameters:**
      * promotionPiece : char — piece to promote to
      * isWhite : bool — promotion color
    * **Returns:** IEnumerator — yield return promotionUI.SelectPromotionAutomaticallyCoroutine('Q', true); StartCoroutine(promotionUI.SelectPromotionAutomaticallyCoroutine('Q', true)) (yield return new WaitForSeconds(autoSelectionDelay))
    * **Side effects / state changes:** Same as SelectPromotionAutomatically with async completion

  * **Signature:** `public void HideDialog()`
    * **Description:** Hide promotion dialog and reset state
    * **Returns:** void — promotionUI.HideDialog()
    * **Side effects / state changes:** Deactivates panel, resets waiting flags

  * **Signature:** `public bool IsWaitingForPromotion()`
    * **Description:** Check if currently waiting for promotion selection
    * **Returns:** bool — bool waiting = promotionUI.IsWaitingForPromotion()

  * **Signature:** `public bool IsEngineSelection()`
    * **Description:** Check if current selection is from engine
    * **Returns:** bool — bool isEngine = promotionUI.IsEngineSelection()

  * **Signature:** `public (bool isWhite, string fromSquare, string toSquare) GetPromotionContext()`
    * **Description:** Get current promotion context information
    * **Returns:** tuple — var context = promotionUI.GetPromotionContext()

  * **Signature:** `public void SelectDefaultPromotion()`
    * **Description:** Force selection of default piece for fallback scenarios
    * **Returns:** void — promotionUI.SelectDefaultPromotion()
    * **Side effects / state changes:** Selects defaultPromotionPiece if waiting

  * **Signature:** `public void RunAllTests()`
    * **Description:** Run comprehensive validation and testing suite
    * **Returns:** void — promotionUI.RunAllTests()
    * **Side effects / state changes:** Logs test results to console

**Unity Lifecycle Methods:**
* `Awake()`
  - Called on component initialization. Validates UI components (ValidateComponents()), sets up button event listeners (SetupButtonListeners()), and hides dialog panel initially (HideDialog()).
  - Ensures all required UI elements are assigned and functional before use.

### `PromotionSelectionData` (class)
* **Kind:** class marked [System.Serializable]
* **Responsibility:** Data container for promotion selection events with enhanced context
* **Constructor(s):** `public PromotionSelectionData(char piece, bool isWhite, string from = "", string to = "")`
* **Public properties / fields:**
  * promotionPiece — char — selected promotion piece (get/set)
  * isWhitePromotion — bool — true if white promotion (get/set)
  * fromSquare — string — source square (get/set)
  * toSquare — string — target square (get/set)
  * selectionTime — float — Time.time when selected (get/set)
  * isEngineSelection — bool — true if engine selected (get/set)

* **Public methods:**
  * **Signature:** `public override string ToString()`
    * **Description:** Human-readable representation of selection data
    * **Returns:** string — string debug = selectionData.ToString()

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
        promotionUI.OnPromotionSelectedWithData += OnPieceSelectedWithData;
        
        // Test human promotion dialog
        Debug.Log("<color=cyan>Testing human promotion dialog...</color>");
        promotionUI.ShowPromotionDialog(true, "e7", "e8");
        
        // Wait for selection
        while (promotionUI.IsWaitingForPromotion())
        {
            yield return null;
        }
        // Expected output: "[PromotionUI] Selected promotion: Queen (Human)"
        
        yield return new WaitForSeconds(1f);
        
        // Test engine auto-selection
        Debug.Log("<color=cyan>Testing engine auto-selection...</color>");
        yield return promotionUI.SelectPromotionAutomaticallyCoroutine('R', false);
        // Expected output: "[PromotionUI] Selected promotion: Rook (Engine)"
        
        // Test context retrieval
        var context = promotionUI.GetPromotionContext();
        Debug.Log($"<color=green>Context: {context.isWhite}, {context.fromSquare}, {context.toSquare}</color>");
        // Expected output: "Context: False, , "
        
        // Test state queries
        bool waiting = promotionUI.IsWaitingForPromotion();
        bool isEngine = promotionUI.IsEngineSelection();
        Debug.Log($"<color=yellow>Waiting: {waiting}, Engine: {isEngine}</color>");
        // Expected output: "Waiting: False, Engine: False"
        
        // Test debug representation
        string debugInfo = promotionUI.ToString();
        Debug.Log($"<color=magenta>Debug: {debugInfo}</color>");
        // Expected output: "Debug: PromotionUI[Waiting:False, Side:Black, Selected:R, Default:Q, Engine:False]"
        
        // Run comprehensive tests
        promotionUI.RunAllTests();
        // Expected output: "[PromotionUI] ✓ All tests completed successfully"
        
        // Cleanup
        promotionUI.OnPromotionSelected -= OnPieceSelected;
        promotionUI.OnPromotionSelectedWithData -= OnPieceSelectedWithData;
    }
    
    private void OnPieceSelected(char piece)
    {
        Debug.Log($"<color=green>Piece selected: {piece}</color>");
    }
    
    private void OnPieceSelectedWithData(PromotionSelectionData data)
    {
        Debug.Log($"<color=blue>Selection data: {data.ToString()}</color>");
        // Expected output: "Selection data: White promotes to Queen (Human) (e7-e8)"
    }
}
```

## Control flow / responsibilities & high-level algorithm summary
Modal dialog shows sprite buttons for Q/R/B/N selection. Human clicks trigger SelectPromotion(), engine calls auto-select with delay. Events fire immediately on selection, dialog hides. Comprehensive validation and testing integrated.

## Performance, allocations, and hotspots / Threading / async considerations
Minimal allocations, UI-only operations. Main thread only, coroutine support for async patterns.

## Security / safety / correctness concerns
Validates promotion pieces, handles null UI components gracefully. Button validation prevents missing Image components.

## Tests, debugging & observability
Built-in RunAllTests() method with 11 comprehensive test categories. Extensive Debug.Log output with color coding. ToString() method for state inspection.

## Cross-file references
Dependencies on `ChessMove.cs` for piece validation (IsValidPromotionPiece, GetPromotionPieceName methods). Unity UI system and TextMeshPro package required.

## Appendix
Key private methods: ValidateComponents() ensures UI integrity, SetupPromotionUI() configures sprites per color, AutoSelectWithDelay() handles engine timing. State managed through isWaitingForSelection and isEngineSelection flags.

## General Note: important behaviors
Major functionality: Pawn Promotion UI with dual human/engine support. Sprite-based visual representation. Mandatory selection with no timeout. Comprehensive event system with detailed context data.

`checksum: a7b9c3e1 (v0.4)`