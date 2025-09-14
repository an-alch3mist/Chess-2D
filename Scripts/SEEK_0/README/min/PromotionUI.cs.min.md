# Source: `PromotionUI.cs` — Modal pawn promotion dialog with sprite-based UI and auto-selection

## Short description
Implements a Unity MonoBehaviour component for handling chess pawn promotion selection through a modal dialog interface. Provides both human player interaction via sprite-based buttons and automated selection for engine moves with comprehensive event handling and coroutine support.

## Metadata
* **Filename:** `PromotionUI.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependencies:** System, System.Collections, UnityEngine, UnityEngine.UI, TMPro
* **Public types:** `PromotionUI (MonoBehaviour class), PromotionSelectionData (class)`
* **Unity version:** 2020.3 compatible

## Public API Summary
| Type | Member | Signature | Short Purpose | OneLiner Call |
|------|---------|-----------|---------------|---------------|
| System.Action<char> | Event | public System.Action<char> OnPromotionSelected | Basic promotion piece selection event | promotionUI.OnPromotionSelected += (piece) => HandlePromotion(piece); |
| System.Action<PromotionSelectionData> | Event | public System.Action<PromotionSelectionData> OnPromotionSelectedWithData | Enhanced promotion event with context data | promotionUI.OnPromotionSelectedWithData += HandlePromotionData; |
| void | Method | public void ShowPromotionDialog(bool isWhite, string fromSquare = "", string toSquare = "") | Display promotion dialog for human selection | promotionUI.ShowPromotionDialog(true, "e7", "e8"); |
| IEnumerator | Coroutine | public IEnumerator ShowPromotionDialogCoroutine(bool isWhite, string fromSquare = "", string toSquare = "") | Async promotion dialog display | yield return promotionUI.ShowPromotionDialogCoroutine(true, "e7", "e8"); StartCoroutine(promotionUI.ShowPromotionDialogCoroutine(true, "e7", "e8")); |
| void | Method | public void SelectPromotionAutomatically(char promotionPiece, bool isWhite) | Auto-select piece for engine moves | promotionUI.SelectPromotionAutomatically('Q', true); |
| IEnumerator | Coroutine | public IEnumerator SelectPromotionAutomaticallyCoroutine(char promotionPiece, bool isWhite) | Async auto-selection | yield return promotionUI.SelectPromotionAutomaticallyCoroutine('Q', true); StartCoroutine(promotionUI.SelectPromotionAutomaticallyCoroutine('Q', true)); |
| void | Method | public void HideDialog() | Hide promotion dialog panel | promotionUI.HideDialog(); |
| bool | Method | public bool IsWaitingForPromotion() | Check if awaiting selection | bool waiting = promotionUI.IsWaitingForPromotion(); |
| bool | Method | public bool IsEngineSelection() | Check if current selection is engine-driven | bool isEngine = promotionUI.IsEngineSelection(); |
| (bool, string, string) | Method | public (bool isWhite, string fromSquare, string toSquare) GetPromotionContext() | Get current promotion context | var context = promotionUI.GetPromotionContext(); |
| void | Method | public void SelectDefaultPromotion() | Force default piece selection | promotionUI.SelectDefaultPromotion(); |
| void | Method | public void RunAllTests() | Execute comprehensive test suite | promotionUI.RunAllTests(); |
| string | Method | public override string ToString() | Debug string representation | Debug.Log(promotionUI.ToString()); |
| PromotionSelectionData | Constructor | public PromotionSelectionData(char piece, bool isWhite, string from = "", string to = "") | Create selection data object | var data = new PromotionSelectionData('Q', true, "e7", "e8"); |
| char | Property | public char promotionPiece | Selected promotion piece | char piece = data.promotionPiece; |
| bool | Property | public bool isWhitePromotion | White promotion flag | bool isWhite = data.isWhitePromotion; |
| string | Property | public string fromSquare | Source square identifier | string from = data.fromSquare; |
| string | Property | public string toSquare | Target square identifier | string to = data.toSquare; |
| float | Property | public float selectionTime | Selection timestamp | float time = data.selectionTime; |
| bool | Property | public bool isEngineSelection | Engine selection flag | bool engine = data.isEngineSelection; |
| string | Method | public override string ToString() | Human-readable selection description | Debug.Log(data.ToString()); |

## MonoBehaviour Integration
**Note:** This class inherits MonoBehaviour.

**Unity Lifecycle Methods:**
* `Awake()` - Validates UI components, sets up button listeners, hides dialog initially
* No other Unity lifecycle methods implemented

**SerializeField Dependencies:**
* `[SerializeField] private GameObject promotionPanel` - Main promotion dialog panel
* `[SerializeField] private TextMeshProUGUI titleText` - Dialog title text component
* `[SerializeField] private Button queenButton` - Queen promotion button
* `[SerializeField] private Button rookButton` - Rook promotion button  
* `[SerializeField] private Button bishopButton` - Bishop promotion button
* `[SerializeField] private Button knightButton` - Knight promotion button
* `[SerializeField] private Sprite blackQueenSprite` - Black queen piece sprite
* `[SerializeField] private Sprite blackRookSprite` - Black rook piece sprite
* `[SerializeField] private Sprite blackBishopSprite` - Black bishop piece sprite
* `[SerializeField] private Sprite blackKnightSprite` - Black knight piece sprite
* `[SerializeField] private Sprite whiteQueenSprite` - White queen piece sprite
* `[SerializeField] private Sprite whiteRookSprite` - White rook piece sprite
* `[SerializeField] private Sprite whiteBishopSprite` - White bishop piece sprite
* `[SerializeField] private Sprite whiteKnightSprite` - White knight piece sprite
* `[SerializeField] private char defaultPromotionPiece` - Default promotion piece ('Q')
* `[SerializeField] private float autoSelectionDelay` - Auto-selection delay (0.1s)
* `[SerializeField] private Color selectedColor` - Selected button color
* `[SerializeField] private Color normalColor` - Normal button color
* `[SerializeField] private Color highlightColor` - Highlighted button color

## Important Types

### `PromotionUI`
* **Kind:** MonoBehaviour class
* **Responsibility:** Manages pawn promotion selection UI with sprite-based buttons and handles both human and engine selections
* **Constructor(s):** Unity MonoBehaviour - no explicit constructor
* **Public Events:**
  * `OnPromotionSelected` — `System.Action<char>` — Fires when piece selected (`event subscription`)
  * `OnPromotionSelectedWithData` — `System.Action<PromotionSelectionData>` — Enhanced selection event (`event subscription`)
* **Public Methods:**
  * **`public void ShowPromotionDialog(bool isWhite, string fromSquare = "", string toSquare = "")`**
    * Description: Display modal promotion dialog for human player selection
    * Parameters: `isWhite : bool — true for white promotion`, `fromSquare : string — source square (optional)`, `toSquare : string — target square (optional)`
    * Returns: `void` + call example: `promotionUI.ShowPromotionDialog(true, "e7", "e8");`
    * Notes: Sets up sprite-based buttons, shows dialog, enables selection
  * **`public IEnumerator ShowPromotionDialogCoroutine(bool isWhite, string fromSquare = "", string toSquare = "")`**
    * Description: Coroutine version that waits for selection completion
    * Parameters: Same as ShowPromotionDialog
    * Returns: `IEnumerator` + call example: `yield return promotionUI.ShowPromotionDialogCoroutine(true);`
    * Notes: Async operation, completes when user makes selection
  * **`public void SelectPromotionAutomatically(char promotionPiece, bool isWhite)`**
    * Description: Auto-select promotion piece for engine moves
    * Parameters: `promotionPiece : char — piece to promote to (Q/R/B/N)`, `isWhite : bool — promotion color`
    * Returns: `void` + call example: `promotionUI.SelectPromotionAutomatically('Q', true);`
    * Notes: Validates piece, applies delay, triggers selection events

### `PromotionSelectionData`
* **Kind:** Serializable data class  
* **Responsibility:** Contains comprehensive promotion selection information including context and metadata
* **Constructor(s):** `public PromotionSelectionData(char piece, bool isWhite, string from = "", string to = "")`
* **Public Properties:**
  * `promotionPiece` — `char` — Selected piece character (Q/R/B/N) (`get/set`)
  * `isWhitePromotion` — `bool` — White promotion flag (`get/set`)
  * `fromSquare` — `string` — Source square identifier (`get/set`)
  * `toSquare` — `string` — Target square identifier (`get/set`) 
  * `selectionTime` — `float` — Unity Time.time when selected (`get/set`)
  * `isEngineSelection` — `bool` — Engine vs human selection flag (`get/set`)
* **Public Methods:**
  * **`public override string ToString()`**
    * Description: Generate human-readable selection description
    * Returns: `string — formatted description` + call example: `Debug.Log(data.ToString());`
    * Notes: Includes color, piece name, source type, and move notation

## Example Usage
**Required namespaces:**
```csharp
using System.Collections;
using UnityEngine;
using GPTDeepResearch;
```

**For MonoBehaviour classes:**
```csharp
public class ExampleUsage : MonoBehaviour 
{
    [SerializeField] private PromotionUI promotionUI; // Assign in Inspector
    
    private IEnumerator PromotionUI_Check()
    {
        // Setup event handlers
        promotionUI.OnPromotionSelected += HandlePromotion;
        promotionUI.OnPromotionSelectedWithData += HandlePromotionData;
        
        // Test human selection dialog
        promotionUI.ShowPromotionDialog(true, "e7", "e8");
        yield return new WaitUntil(() => !promotionUI.IsWaitingForPromotion());
        
        // Test engine auto-selection
        promotionUI.SelectPromotionAutomatically('Q', false);
        
        // Test context and state
        var context = promotionUI.GetPromotionContext();
        bool waiting = promotionUI.IsWaitingForPromotion();
        bool engine = promotionUI.IsEngineSelection();
        
        // Test data object
        var data = new PromotionSelectionData('R', true, "a7", "a8");
        string description = data.ToString();
        
        Debug.Log($"API Results: Context={context}, Waiting={waiting}, Engine={engine}, Data={description}");
        yield break;
    }
    
    private void HandlePromotion(char piece) { }
    private void HandlePromotionData(PromotionSelectionData data) { }
}
```

## Control Flow & Responsibilities
Modal dialog workflow: setup sprites → show UI → await selection → fire events → hide dialog.

## Performance & Threading  
Main thread UI operations, minimal allocations, coroutine-based async support.

## Cross-file Dependencies
References ChessMove.IsValidPromotionPiece() and ChessMove.GetPromotionPieceName() for validation and display.

## Major Functionality
Pawn promotion selection with sprite-based UI, human/engine selection modes, comprehensive event system, test suite.

`checksum: A7B9C3D1 v0.5.min`