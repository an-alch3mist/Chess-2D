# Source: `ChessFENViewer.cs` — Unity MonoBehaviour for parsing FEN strings and instantiating chess piece GameObjects on a board

Parses Forsyth-Edwards Notation (FEN) strings to generate visual chess board representations using Unity GameObjects and prefab instantiation.

## Short description

This file implements a Unity-based chess FEN (Forsyth-Edwards Notation) parser that converts chess position strings into 3D GameObject representations. It handles FEN validation, piece prefab lookup, coordinate mapping from chess notation to Unity world space, and provides comprehensive error handling with UI integration. The system supports all 12 chess piece types (6 pieces × 2 colors) and includes a built-in testing framework for validation.

## Metadata

* **Filename:** `ChessFENViewer.cs`
* **Primary namespace:** `GptDeepResearch`
* **Dependent namespace:** `SPACE_UTIL` (external namespace), `System.Collections.Generic`, `UnityEngine`, `UnityEngine.UI`, `TMPro`
* **Estimated lines:** 617
* **Estimated chars:** 14,000
* **Public types:** `ChessPieceConfig (class)`, `ChessFENViewer (class, inherits MonoBehaviour)`
* **Unity version / Target framework:** Unity 2020.3 / .NET 2.0 subset (WebGL compatible)
* **Dependencies:** `SPACE_UTIL.v2` (external namespace), `Board<T>` (external namespace)

## Public API summary

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| `ChessPieceConfig (class)` | Constructor | `public ChessPieceConfig()` | Initialize chess piece configuration | `var config = new ChessPieceConfig();` |
| `List<GameObject>` | chessPiecePrefabs | `public List<GameObject> chessPiecePrefabs` | Chess piece prefab references | `config.chessPiecePrefabs.Add(prefab);` |
| `TMP_InputField` | fenInputField | `public TMP_InputField fenInputField` | FEN input UI field | `config.fenInputField.text = "rnbq...";` |
| `Button` | generateBoardButton | `public Button generateBoardButton` | Board generation trigger button | `config.generateBoardButton.onClick.AddListener(...);` |
| `TMP_InputField` | statusText | `public TMP_InputField statusText` | Status display field | `config.statusText.text = "Ready";` |
| `Transform` | boardParent | `public Transform boardParent` | Board parent transform | `config.boardParent = transform;` |
| `Vector3` | squareSize | `public Vector3 squareSize` | Board square dimensions | `config.squareSize = Vector3.one;` |
| `Vector3` | boardOrigin | `public Vector3 boardOrigin` | Board origin position | `config.boardOrigin = Vector3.zero;` |
| `void` | GenerateBoardFromFEN | `public void GenerateBoardFromFEN()` | Generate board from FEN input | `chessFENViewer.GenerateBoardFromFEN();` |
| `void` | ClearBoard | `public void ClearBoard()` | Clear all pieces from board | `chessFENViewer.ClearBoard();` |
| `void` | RunAllTests | `public static void RunAllTests()` | Run comprehensive test suite | `ChessFENViewer.RunAllTests();` |

## Important types — details

### `ChessPieceConfig` 
* **Kind:** class
* **Responsibility:** Configuration container for chess piece prefabs and UI components with serializable inspector fields
* **Constructor(s):** `public ChessPieceConfig()` — default parameterless constructor
* **Public properties / fields:**
  * `chessPiecePrefabs — List<GameObject> — get/set` — List of 12 chess piece prefab references
  * `fenInputField — TMP_InputField — get/set` — UI input field for FEN strings  
  * `generateBoardButton — Button — get/set` — UI button to trigger board generation
  * `statusText — TMP_InputField — get/set` — UI field for status messages
  * `boardParent — Transform — get/set` — Parent transform for instantiated pieces
  * `squareSize — Vector3 — get/set` — Dimensions of each chess square
  * `boardOrigin — Vector3 — get/set` — World position origin for the board
* **Public methods:** None (configuration data class)

### `ChessFENViewer`
* **Kind:** class (inherits MonoBehaviour)
* **Note:** MonoBehaviour
* **Responsibility:** Main FEN parsing and board generation logic with Unity lifecycle integration
* **Constructor(s):** Unity handles MonoBehaviour instantiation
* **Public properties / fields:** None exposed publicly
* **Public methods:**
  * **Signature:** `public void GenerateBoardFromFEN()`
    * **Description:** Main method to parse FEN input and generate board representation
    * **Parameters:** None
    * **Returns:** void — `chessFENViewer.GenerateBoardFromFEN();`
    * **Side effects / state changes:** Clears existing board, instantiates new GameObjects, updates UI status
    * **Complexity / performance:** O(64) for board parsing, GameObject instantiation per piece
    * **Notes:** Validates FEN format, handles prefab lookup, manages GameObject lifecycle
  
  * **Signature:** `public void ClearBoard()`
    * **Description:** Removes all instantiated chess pieces from the board
    * **Parameters:** None  
    * **Returns:** void — `chessFENViewer.ClearBoard();`
    * **Side effects / state changes:** Destroys all active GameObjects, resets internal board state
    * **Complexity / performance:** O(n) where n is number of active pieces
    * **Notes:** Uses DestroyImmediate in editor, Destroy in WebGL builds
    
  * **Signature:** `public static void RunAllTests()`
    * **Description:** Executes comprehensive test suite for FEN validation and coordinate mapping
    * **Parameters:** None
    * **Returns:** void — `ChessFENViewer.RunAllTests();`
    * **Side effects / state changes:** Creates temporary test objects, outputs Debug.Log results
    * **Complexity / performance:** O(1) test execution with temporary GameObject creation
    * **Notes:** Static method, creates and destroys temporary instances for testing

**Unity Lifecycle Methods:**
* `Awake()` - Called on script load. Initializes chess board (calls InitializeChessBoard()), validates inspector references (calls ValidateInspectorReferences()), and builds prefab lookup dictionary (calls BuildPrefabLookup())
* `Start()` - Called before first frame. Registers button click listener for generateBoardButton, updates status to "Chess FEN Viewer Ready", sets target frame rate to 20, and outputs initialization debug log
* `OnDestroy()` - Called on object destruction. Cleans up all instantiated pieces by calling ClearBoard()

## Example usage

```csharp
// Required namespaces:
// using System.Collections;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using GptDeepResearch;
// using SPACE_UTIL;

public class ExampleUsage : MonoBehaviour 
{
    [SerializeField] private ChessFENViewer chessFENViewer; // Assign in Inspector
    [SerializeField] private TMP_InputField fenInput; // Assign in Inspector
    [SerializeField] private Button generateButton; // Assign in Inspector
    [SerializeField] private TMP_InputField statusDisplay; // Assign in Inspector
    
    private IEnumerator ChessFENViewer_Check()
    {
        // Setup configuration
        var config = new ChessPieceConfig();
        config.fenInputField = fenInput;
        config.generateBoardButton = generateButton;
        config.statusText = statusDisplay;
        config.boardParent = transform;
        config.squareSize = Vector3.one;
        config.boardOrigin = Vector3.zero;
        
        // Load standard starting position FEN
        fenInput.text = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        
        // Generate board from FEN
        chessFENViewer.GenerateBoardFromFEN();
        // Expected output: "<color=green>[ChessFENViewer] Board generated with 32 pieces</color>"
        Debug.Log("<color=green>Board generated successfully</color>");
        
        yield return new WaitForSeconds(2f);
        
        // Clear the board
        chessFENViewer.ClearBoard();
        // Expected output: "<color=yellow>[ChessFENViewer] Board cleared</color>"
        Debug.Log("<color=yellow>Board cleared</color>");
        
        // Test with custom position
        fenInput.text = "rnbqkb1r/pppp1ppp/5n2/4p3/2B1P3/8/PPPP1PPP/RNBQK1NR w KQkq - 4 4";
        chessFENViewer.GenerateBoardFromFEN();
        // Expected output: "<color=green>[ChessFENViewer] Board generated with 30 pieces</color>"
        Debug.Log("<color=green>Custom position loaded</color>");
        
        // Run comprehensive tests
        ChessFENViewer.RunAllTests();
        // Expected output: "<color=cyan>[ChessFENViewer] ======= TEST SUITE COMPLETED =======</color>"
        Debug.Log("<color=cyan>All tests completed</color>");
        
        yield break;
    }
}
```

## Control flow / responsibilities & high-level algorithm summary
FEN string validation → board position extraction → rank-by-rank parsing → piece prefab lookup → GameObject instantiation at calculated world positions. Unity lifecycle manages initialization, UI event binding, and cleanup with comprehensive error handling throughout.

## Performance, allocations, and hotspots / Threading / async considerations
GameObject instantiation per piece, Dictionary lookups for prefab mapping, main-thread only operations, WebGL-compatible destruction patterns.

## Security / safety / correctness concerns
FEN validation prevents invalid input processing, null reference checks for UI components, proper GameObject lifecycle management prevents memory leaks.

## Tests, debugging & observability
Built-in static test suite (RunAllTests()) validates FEN parsing, coordinate mapping, and prefab lookup with colored Debug.Log output for pass/fail states.

## Cross-file references
Depends on `SPACE_UTIL.v2` for 2D vector coordinates and `Board<T>` for chess board data structure representation.

<!-- ## TODO / Known limitations / Suggested improvements
* Add support for FEN castling rights and en passant validation
* Implement piece animation for smoother board transitions  
* Add support for different board orientations (white/black perspective)
* Include FEN export functionality for current board state
* Optimize prefab lookup with cached dictionary initialization
* Add support for custom piece sets beyond standard chess pieces -->

## Appendix
Key private helpers: ValidateFENFormat() for input validation, ChessToWorldPosition() for coordinate conversion, PlacePiece() for GameObject instantiation with proper naming conventions.

## General Note: important behaviors
Major functionality includes FEN validation with comprehensive error reporting, chess coordinate system mapping (a1=0,0 to h8=7,7), and WebGL-compatible GameObject lifecycle management with frame rate optimization.

`checksum: a7b3f912 (v0.3)`