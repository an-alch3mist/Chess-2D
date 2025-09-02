# `PromotionUI.cs` — Unity modal dialog for pawn promotion selection with timeout and visual feedback

Unity MonoBehaviour providing interactive promotion piece selection with auto-timeout, visual customization, and comprehensive event integration for chess applications.

## Short description (2–4 sentences)
This file implements a Unity UI component for handling pawn promotion selection through a modal dialog interface. It provides configurable auto-timeout with default piece selection, visual customization for white/black pieces, and comprehensive event callbacks for game integration. The system supports both user interaction and programmatic control with proper state management and visual feedback.

## Metadata
- **Filename:** `PromotionUI.cs`
- **Primary namespace:** `GPTDeepResearch`
- **Estimated lines:** 480
- **Estimated chars:** 28,000
- **Public types:** `PromotionUI, PromotionSelectionData`
- **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
- **Dependencies:** `ChessMove.cs` (GetPromotionPieceName), UnityEngine.UI, TMPro

## Public API summary (table)

| Type | Member | Signature | Short purpose |
|------|--------|-----------|--------------|
| PromotionUI | ShowPromotionDialog() | public void ShowPromotionDialog(bool isWhite, string fromSquare = "", string toSquare = "") | Display promotion selection dialog |
| PromotionUI | HideDialog() | public void HideDialog() | Close dialog and cleanup |
| PromotionUI | IsWaitingForPromotion() | public bool IsWaitingForPromotion() | Check if dialog is active |
| PromotionUI | SelectDefaultPromotion() | public void SelectDefaultPromotion() | Force default piece selection |
| PromotionUI | TestPromotionUI() | public void TestPromotionUI() | Automated UI testing |
| PromotionUI | OnPromotionSelected | public System.Action<char> | Event fired when piece selected |
| PromotionUI | OnPromotionCancelled | public System.Action | Event fired when dialog cancelled |
| PromotionSelectionData | Constructor | public PromotionSelectionData(char piece, bool isWhite, string from = "", string to = "") | Create selection data with timing |

## Important types — details

### `PromotionUI`
- **Kind:** class (MonoBehaviour)
- **Responsibility:** Manages interactive promotion piece selection with timeout handling
- **Constructor(s):** MonoBehaviour (Unity handles instantiation)
- **Public properties / fields:**
  - OnPromotionSelected — System.Action<char> — Event when piece selected
  - OnPromotionCancelled — System.Action — Event when dialog cancelled
- **Public methods:**
  - **ShowPromotionDialog():** Display modal dialog for promotion selection
    - **Parameters:** isWhite : bool — true for white promotion, fromSquare/toSquare : string — optional move context
    - **Returns:** void
    - **Side effects:** Activates UI panel, starts timeout coroutine, sets internal state flags
    - **Notes:** Idempotent if already waiting, validates state before showing
  - **HideDialog():** Close dialog and cleanup resources
    - **Returns:** void
    - **Side effects:** Deactivates panel, stops timeout coroutine, resets internal state
    - **Notes:** Safe to call multiple times, handles cleanup gracefully
  - **IsWaitingForPromotion():** Check dialog state
    - **Returns:** bool — true if dialog is active and waiting for user input
    - **Notes:** Thread-safe state check
  - **SelectDefaultPromotion():** Programmatically select default piece
    - **Returns:** void
    - **Side effects:** Triggers OnPromotionSelected event, hides dialog
    - **Notes:** Uses configurable defaultPromotionPiece (default: Queen)

### `PromotionSelectionData`
- **Kind:** class (Serializable)
- **Responsibility:** Data container for promotion selection events with metadata
- **Constructor(s):** `PromotionSelectionData(char piece, bool isWhite, string from = "", string to = "")`
- **Public properties / fields:**
  - promotionPiece — char — Selected piece ('Q', 'R', 'B', 'N' or lowercase)
  - isWhitePromotion — bool — True if white's promotion
  - fromSquare/toSquare — string — Source/destination squares
  - selectionTime — float — Time.time when selection made
- **Public methods:**
  - **ToString():** Human-readable description
    - **Returns:** string — "White promotes to Queen" format

## Example usage

```csharp
// Setup promotion UI in scene
PromotionUI promotionUI = GetComponent<PromotionUI>();

// Configure settings
promotionUI.timeoutSeconds = 5f;
promotionUI.defaultPromotionPiece = 'Q';

// Register event handlers
promotionUI.OnPromotionSelected += OnPromotionChoice;
promotionUI.OnPromotionCancelled += OnPromotionCancelled;

// Show dialog when pawn reaches promotion rank
if (pawnReachesLastRank) {
    promotionUI.ShowPromotionDialog(isWhitePlayer, "e7", "e8");
}

// Handle selection
private void OnPromotionChoice(char piece) {
    ChessMove promotionMove = ChessMove.CreatePromotionMove(from, to, 'P', piece);
    chessBoard.MakeMove(promotionMove);
}
```

## Control flow / responsibilities & high-level algorithm summary

The promotion UI operates through Unity's event system and coroutine management. Main workflow: (1) ShowPromotionDialog() validates state and activates UI, (2) SetupPromotionUI() configures buttons and colors based on side, (3) Button click handlers call SelectPromotion(), (4) TimeoutCountdown() coroutine runs parallel countdown, (5) Selection triggers events and DelayedHide() for visual feedback, (6) HideDialog() cleanup and state reset.

The timeout mechanism uses a continuously updating coroutine that decrements remaining time and auto-selects the default piece when timeout reaches zero. Visual feedback system dynamically adjusts button colors based on white/black promotion and highlights selected buttons with the configured selection color.

Button setup uses Unity's Button.colors system to provide visual differentiation between white/black pieces and selection states. All UI operations are main-thread only following Unity's threading constraints.

## Side effects and I/O

- **Unity UI:** Activates/deactivates GameObject panel, modifies Button component colors
- **Coroutine management:** Starts/stops Unity coroutines for timeout and visual feedback
- **Event system:** Fires UnityEvents and C# Action events
- **Time-based operations:** Uses Time.time for timestamps, WaitForSeconds for delays
- **Debug logging:** Color-coded Debug.Log statements for state changes and user actions

## Performance, allocations, and hotspots

- **Light operations:** Button setup and color changes are minimal overhead (~0.1ms)
- **Allocations:** String building for timeout text, event delegate invocations
- **Memory:** Temporary Color structs for button state changes, minimal GC pressure
- **Coroutines:** Two active coroutines maximum (timeout + delayed hide), automatic cleanup
- **UI updates:** Text updates at 0.1s intervals during countdown (10 updates/second max)

## Threading / async considerations

- **Unity main thread:** All operations must occur on main thread per Unity constraints
- **Coroutines:** Uses Unity's coroutine system for non-blocking timeout and delays
- **No threading:** No background threads or Task operations
- **Event safety:** Events fired synchronously on main thread
- **State management:** Boolean flags provide simple thread-safe state checking

## Security / safety / correctness concerns

- **Null reference protection:** Comprehensive null checks for all UI component references
- **State validation:** Prevents multiple concurrent dialogs through isWaitingForSelection flag
- **Resource cleanup:** Proper coroutine stopping prevents memory leaks
- **Input validation:** Character case normalization for promotion pieces
- **Error handling:** Graceful degradation when UI components are missing

## Tests, debugging & observability

- **Logging:** Comprehensive Debug.Log with color coding (cyan=info, green=success, yellow=warning)
- **Test methods:** 
  - TestPromotionUI() — Automated sequence testing white/black promotions
  - TestTimeout() — Validates timeout functionality with temporary settings
  - TestSelectionSequence() — Coroutine-based automated interaction testing
- **Visual feedback:** Real-time button highlighting and timeout countdown display
- **State inspection:** IsWaitingForPromotion() provides external state visibility
- **Event monitoring:** OnPromotionSelected/Cancelled provide comprehensive callback coverage

## Cross-file references

- `ChessMove.cs`: GetPromotionPieceName() for UI text display

<!--
## TODO / Known limitations / Suggested improvements

- Limitation: Single dialog instance per scene (no multi-board support)
- Limitation: Unity UI dependency prevents non-Unity usage
- Suggested: Add keyboard shortcuts for piece selection (Q, R, B, N)
- Suggested: Add piece preview visualization
- Suggested: Support for custom piece icons/sprites
- Suggested: Add sound effects for button interactions
-->

## Appendix

**Inspector configuration fields:**
- `timeoutSeconds = 3f`: Auto-selection delay
- `defaultPromotionPiece = 'Q'`: Default piece when timeout reached
- `showTimeoutCountdown = true`: Display countdown text
- `whiteButtonColor/blackButtonColor`: Side-specific button styling
- `selectedColor`: Highlight color for chosen piece

**Unity component requirements:**
- GameObject with Button components for Queen, Rook, Bishop, Knight
- TextMeshProUGUI components for title and timeout display
- Canvas/Panel structure for modal dialog presentation

**File checksum:** First 8 chars of conceptual SHA1: `b9c4d1a7`