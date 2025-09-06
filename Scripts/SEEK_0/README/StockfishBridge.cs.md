# Source: `StockfishBridge.cs` — Unity Stockfish chess engine bridge with comprehensive promotion support and game management

Unity MonoBehaviour providing non-blocking chess engine communication with full promotion parsing, undo/redo functionality, and side-to-move evaluation.

## Short description (2–4 sentences)

This file implements a Unity bridge to the Stockfish chess engine, providing comprehensive chess position analysis with enhanced promotion move support. It manages engine processes, handles UCI communication, and offers game state management including move history with undo/redo capabilities. The bridge includes advanced evaluation conversion from centipawns to win probabilities and supports configurable engine strength via Elo ratings and skill levels.

## Metadata

* **Filename:** `StockfishBridge.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `System`, `System.Text`, `System.IO`, `System.Collections`, `System.Collections.Concurrent`, `System.Collections.Generic`, `System.Diagnostics`, `System.Threading`, `UnityEngine`, `UnityEngine.Events`, `SPACE_UTIL`
* **Estimated lines:** ~1800
* **Estimated chars:** ~45,000
* **Public types:** `StockfishBridge (class)`, `StockfishBridge.ChessAnalysisResult (class)`, `StockfishBridge.GameHistoryEntry (class)`
* **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
* **Dependencies:** `ChessBoard.cs`, `ChessMove.cs`, `v2` (SPACE_UTIL namespace)

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| int (field) | defaultTimeoutMs | `[SerializeField] private int defaultTimeoutMs` | Default analysis timeout | N/A (private field) |
| bool (field) | enableDebugLogging | `[SerializeField] private bool enableDebugLogging` | Debug logging toggle | N/A (private field) |
| bool (field) | enableEvaluation | `[SerializeField] public bool enableEvaluation` | Evaluation calculation toggle | `bridge.enableEvaluation = true;` |
| int (field) | defaultDepth | `[SerializeField] private int defaultDepth` | Default search depth | N/A (private field) |
| int (field) | evalDepth | `[SerializeField] private int evalDepth` | Default evaluation depth | N/A (private field) |
| int (field) | defaultElo | `[SerializeField] private int defaultElo` | Default engine Elo rating | N/A (private field) |
| int (field) | defaultSkillLevel | `[SerializeField] private int defaultSkillLevel` | Default skill level | N/A (private field) |
| bool (field) | allowPlayerSideSelection | `[SerializeField] private bool allowPlayerSideSelection` | Allow side selection | N/A (private field) |
| char (field) | humanSide | `[SerializeField] private char humanSide` | Human player side | N/A (private field) |
| int (field) | maxHistorySize | `[SerializeField] private int maxHistorySize` | Maximum history entries | N/A (private field) |
| UnityEvent<string> (field) | OnEngineLine | `public UnityEvent<string> OnEngineLine` | Engine output event | `bridge.OnEngineLine.AddListener(handler);` |
| UnityEvent<StockfishBridge.ChessAnalysisResult> (field) | OnAnalysisComplete | `public UnityEvent<ChessAnalysisResult> OnAnalysisComplete` | Analysis completion event | `bridge.OnAnalysisComplete.AddListener(handler);` |
| UnityEvent<char> (field) | OnSideToMoveChanged | `public UnityEvent<char> OnSideToMoveChanged` | Side change event | `bridge.OnSideToMoveChanged.AddListener(handler);` |
| string (property) | LastRawOutput | `public string LastRawOutput { get; }` | Last engine raw output | `string output = bridge.LastRawOutput;` |
| StockfishBridge.ChessAnalysisResult (property) | LastAnalysisResult | `public ChessAnalysisResult LastAnalysisResult { get; }` | Last analysis result | `var result = bridge.LastAnalysisResult;` |
| List<StockfishBridge.GameHistoryEntry> (property) | GameHistory | `public List<GameHistoryEntry> GameHistory { get; }` | Game move history | `var history = bridge.GameHistory;` |
| int (property) | CurrentHistoryIndex | `public int CurrentHistoryIndex { get; }` | Current history position | `int index = bridge.CurrentHistoryIndex;` |
| bool (property) | IsEngineRunning | `public bool IsEngineRunning { get; }` | Engine process status | `bool running = bridge.IsEngineRunning;` |
| bool (property) | IsReady | `public bool IsReady { get; }` | Engine ready status | `bool ready = bridge.IsReady;` |
| char (property) | HumanSide | `public char HumanSide { get; set; }` | Human player side | `char side = bridge.HumanSide;` |
| char (property) | EngineSide | `public char EngineSide { get; }` | Engine player side | `char side = bridge.EngineSide;` |
| void (method) | StartEngine | `public void StartEngine()` | Start Stockfish process | `bridge.StartEngine();` |
| void (method) | StopEngine | `public void StopEngine()` | Stop engine and cleanup | `bridge.StopEngine();` |
| IEnumerator (method) | AnalyzePositionCoroutine | `public IEnumerator AnalyzePositionCoroutine(string fen)` | Analyze position with defaults | `yield return bridge.AnalyzePositionCoroutine(fen);` or `StartCoroutine(bridge.AnalyzePositionCoroutine(fen));` |
| IEnumerator (method) | AnalyzePositionCoroutine | `public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel)` | Comprehensive position analysis | `yield return bridge.AnalyzePositionCoroutine(fen, 5000, 15, 15, 2000, 10);` or `StartCoroutine(bridge.AnalyzePositionCoroutine(fen, 5000, 15, 15, 2000, 10));` |
| void (method) | SetHumanSide | `public void SetHumanSide(char side)` | Set human player side | `bridge.SetHumanSide('w');` |
| void (method) | AddMoveToHistory | `public void AddMoveToHistory(string fen, ChessMove move, string notation, float evaluation)` | Add move to history | `bridge.AddMoveToHistory(fen, move, "e4", 0.3f);` |
| StockfishBridge.GameHistoryEntry (method) | UndoMove | `public GameHistoryEntry UndoMove()` | Undo last move | `var entry = bridge.UndoMove();` |
| StockfishBridge.GameHistoryEntry (method) | RedoMove | `public GameHistoryEntry RedoMove()` | Redo next move | `var entry = bridge.RedoMove();` |
| bool (method) | CanUndo | `public bool CanUndo()` | Check if undo possible | `bool canUndo = bridge.CanUndo();` |
| bool (method) | CanRedo | `public bool CanRedo()` | Check if redo possible | `bool canRedo = bridge.CanRedo();` |
| void (method) | ClearHistory | `public void ClearHistory()` | Clear game history | `bridge.ClearHistory();` |
| string (method) | GetGameHistoryPGN | `public string GetGameHistoryPGN()` | Get history as PGN | `string pgn = bridge.GetGameHistoryPGN();` |
| IEnumerator (method) | RestartEngineCoroutine | `public IEnumerator RestartEngineCoroutine()` | Restart crashed engine | `yield return bridge.RestartEngineCoroutine();` or `StartCoroutine(bridge.RestartEngineCoroutine());` |
| bool (method) | DetectAndHandleCrash | `public bool DetectAndHandleCrash()` | Detect engine crash | `bool crashed = bridge.DetectAndHandleCrash();` |
| void (method) | SendCommand | `public void SendCommand(string command)` | Send UCI command | `bridge.SendCommand("isready");` |
| IEnumerator (method) | InitializeEngineCoroutine | `public IEnumerator InitializeEngineCoroutine()` | Initialize and wait ready | `yield return bridge.InitializeEngineCoroutine();` or `StartCoroutine(bridge.InitializeEngineCoroutine());` |
| void (method) | RunAllTests | `public void RunAllTests()` | Run comprehensive tests | `bridge.RunAllTests();` |

## Important types — details

### `StockfishBridge`
* **Kind:** class inherits MonoBehaviour
* **Responsibility:** Main chess engine bridge managing UCI communication, analysis, and game state
* **Constructor(s):** Default MonoBehaviour constructor
* **Public properties / fields:**
  * `enableEvaluation` — bool — Enable/disable position evaluation
  * `OnEngineLine` — UnityEvent<string> — Fired for each engine output line
  * `OnAnalysisComplete` — UnityEvent<StockfishBridge.ChessAnalysisResult> — Fired when analysis completes
  * `OnSideToMoveChanged` — UnityEvent<char> — Fired when human side changes
  * `LastRawOutput` — string — Last complete engine response (get)
  * `LastAnalysisResult` — StockfishBridge.ChessAnalysisResult — Last analysis result object (get)
  * `GameHistory` — List<StockfishBridge.GameHistoryEntry> — Move history list (get)
  * `CurrentHistoryIndex` — int — Current position in history (get)
  * `IsEngineRunning` — bool — Engine process status (get)
  * `IsReady` — bool — Engine ready for commands (get)
  * `HumanSide` — char — Human player side 'w'/'b' (get/set)
  * `EngineSide` — char — Engine player side opposite of human (get)
* **Public methods:**
  * **Signature:** `public void StartEngine()`
  * **Description:** Starts Stockfish process and background reader thread
  * **Parameters:** None
  * **Returns:** void — `bridge.StartEngine()`
  * **Side effects / state changes:** Creates process, starts reader thread, sets IsEngineRunning
  * **Notes:** Automatically called in Awake(), safe to call multiple times
  * **Signature:** `public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs = -1, int searchDepth = 12, int evaluationDepth = 15, int elo = 1500, int skillLevel = 8)`
  * **Description:** Comprehensive chess position analysis with promotion detection
  * **Parameters:**
    * fen : string — FEN position or "startpos"
    * movetimeMs : int — Time limit in milliseconds (-1 for depth-based)
    * searchDepth : int — Search depth for move finding
    * evaluationDepth : int — Depth for position evaluation
    * elo : int — Engine strength Elo rating
    * skillLevel : int — Skill level 0-20 (overrides Elo)
  * **Returns:** IEnumerator — `yield return bridge.AnalyzePositionCoroutine(fen, 5000, 15, 15, 2000, 10)` and `StartCoroutine(bridge.AnalyzePositionCoroutine(fen, 5000, 15, 15, 2000, 10))` - Expected duration: 1-20 seconds (yield return new WaitForSeconds(variable))
  * **Side effects / state changes:** Updates LastAnalysisResult, fires OnAnalysisComplete event
  * **Complexity / performance:** O(exponential) in depth, can allocate significant memory
  * **Notes:** IEnumerator can be yielded from inside another IEnumerator OR started via StartCoroutine(...) on a MonoBehaviour; it cannot be invoked like a synchronous function for its side effects

### `StockfishBridge.ChessAnalysisResult`
* **Kind:** class (nested under StockfishBridge)
* **Responsibility:** Comprehensive analysis result with promotion detection and evaluation data
* **Constructor(s):** Default constructor initializes all fields to defaults
* **Public properties / fields:**
  * `bestMove` — string — Best move in UCI format or special states (get/set)
  * `sideToMove` — char — Side to move 'w'/'b' (get/set)
  * `currentFen` — string — Position FEN (get/set)
  * `whiteWinProbability` — float — White win probability 0-1 (get/set)
  * `sideToMoveWinProbability` — float — Side-to-move win probability 0-1 (get/set)
  * `centipawnEvaluation` — float — Raw centipawn score (get/set)
  * `isMateScore` — bool — True if mate detected (get/set)
  * `mateDistance` — int — Moves to mate (+ white, - black) (get/set)
  * `isGameEnd` — bool — True if checkmate/stalemate (get/set)
  * `isCheckmate` — bool — True if checkmate (get/set)
  * `isStalemate` — bool — True if stalemate (get/set)
  * `inCheck` — bool — True if side in check (get/set)
  * `isPromotion` — bool — True if bestMove is promotion (get/set)
  * `promotionPiece` — char — Promotion piece 'q'/'r'/'b'/'n' (get/set)
  * `promotionFrom` — v2 — Promotion source square (get/set)
  * `promotionTo` — v2 — Promotion target square (get/set)
  * `isPromotionCapture` — bool — True if promotion captures (get/set)
  * `errorMessage` — string — Error details if any (get/set)
  * `rawEngineOutput` — string — Full engine response (get/set)
  * `searchDepth` — int — Depth used for search (get/set)
  * `evaluationDepth` — int — Depth used for evaluation (get/set)
  * `skillLevel` — int — Skill level used (get/set)
  * `approximateElo` — int — Estimated Elo rating (get/set)
  * `analysisTimeMs` — float — Analysis duration (get/set)
* **Public methods:**
  * **Signature:** `public void ParsePromotionData()`
  * **Description:** Parse promotion data from bestMove UCI string
  * **Parameters:** None
  * **Returns:** void — `result.ParsePromotionData()`
  * **Side effects / state changes:** Sets isPromotion, promotionPiece, promotionFrom, promotionTo, isPromotionCapture
  * **Notes:** Validates UCI promotion format (e7e8q) and rank requirements
  * **Signature:** `public StockfishBridge.ChessMove ToChessMove(ChessBoard board)`
  * **Description:** Convert result to ChessMove object
  * **Parameters:** board : ChessBoard — Chess board for move validation
  * **Returns:** StockfishBridge.ChessMove — `var move = result.ToChessMove(board)`
  * **Notes:** Returns ChessMove.Invalid() for errors/game end
  * **Signature:** `public string GetEvaluationDisplay()`
  * **Description:** Format evaluation as percentage for UI
  * **Parameters:** None
  * **Returns:** string — `string display = result.GetEvaluationDisplay()`
  * **Notes:** Returns "Mate in X for Y" or "Side: XX.X%" format

### `StockfishBridge.GameHistoryEntry`
* **Kind:** class (nested under StockfishBridge)
* **Responsibility:** Single move history record for undo/redo functionality
* **Constructor(s):** `GameHistoryEntry(string fen, ChessMove move, string notation, float evaluation)` — Creates history entry with position, move, notation and evaluation
* **Public properties / fields:**
  * `fen` — string — Position before move (get/set)
  * `move` — ChessMove — The move made (get/set)
  * `moveNotation` — string — Human-readable notation (get/set)
  * `evaluationScore` — float — Position evaluation (get/set)
  * `timestamp` — DateTime — When move was made (get/set)

## MonoBehaviour special rules
**Note: MonoBehaviour**

* **Awake()** - Called on script load. Initializes engine by calling StartEngine(), starts initialization coroutine via StartCoroutine(InitializeEngineOnAwake()), and sets static logging flag enableDebugLogging_static.
* **Update()** - Called every frame. Drains incomingLines queue and fires OnEngineLine events, tracks engine responses for request completion, and updates IsReady status based on "readyok" responses.
* **OnApplicationQuit()** - Called on application quit. Calls StopEngine() to clean up engine process and resources.

## Example Usage Coverage Requirements
The example usage demonstrates initialization, core analysis functionality, event handling, game state management, error handling, nested type usage, history management, and coroutine patterns.

## Example usage
```csharp
// Required namespaces:
// using System;
// using System.Collections;
// using UnityEngine;
// using GPTDeepResearch;
// using SPACE_UTIL;

public class StockfishBridgeUsage : MonoBehaviour 
{
    [SerializeField] private StockfishBridge stockfishBridge; // Assign in Inspector
    
    private IEnumerator Start()
    {
        // Initialize engine
        if (!stockfishBridge.IsEngineRunning)
        {
            stockfishBridge.StartEngine();
            yield return stockfishBridge.InitializeEngineCoroutine();
        }
        
        // Expected output: [Stockfish] Engine started successfully
        Debug.Log("Engine ready: " + stockfishBridge.IsReady);
        
        // Set up event handlers
        stockfishBridge.OnAnalysisComplete.AddListener(result => {
            Debug.Log($"Analysis complete: {result.bestMove}");
        });
        stockfishBridge.OnEngineLine.AddListener(line => {
            if (line.StartsWith("info")) Debug.Log($"Engine info: {line}");
        });
        stockfishBridge.OnSideToMoveChanged.AddListener(side => {
            Debug.Log($"Human side changed to: {(side == 'w' ? "White" : "Black")}");
        });
        
        // Set human side
        stockfishBridge.SetHumanSide('w');
        // Expected output: Human side changed to: White
        
        // Basic position analysis
        yield return stockfishBridge.AnalyzePositionCoroutine("startpos");
        
        var result = stockfishBridge.LastAnalysisResult;
        Debug.Log($"Best move: {result.bestMove}");
        // Expected output: Best move: e2e4 (or similar opening move)
        
        Debug.Log($"Evaluation: {result.GetEvaluationDisplay()}");
        // Expected output: Evaluation: White: 50.2% (or similar)
        
        // Advanced analysis with custom parameters
        string testFen = "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1";
        yield return stockfishBridge.AnalyzePositionCoroutine(testFen, 5000, 15, 15, 2000, 10);
        
        result = stockfishBridge.LastAnalysisResult;
        Debug.Log($"Advanced analysis: {result.bestMove} | {result.GetEvaluationDisplay()}");
        // Expected output: Advanced analysis: e7e5 | Black: 49.8%
        
        // Test promotion detection
        string promotionFen = "8/7P/8/8/8/8/8/k6K w - - 0 1";
        yield return stockfishBridge.AnalyzePositionCoroutine(promotionFen);
        
        result = stockfishBridge.LastAnalysisResult;
        if (result.isPromotion)
        {
            Debug.Log($"Promotion detected: {result.GetPromotionDescription()}");
            // Expected output: Promotion detected: White promotes to Queen (h7-h8)
        }
        
        // Game history management
        ChessBoard board = new ChessBoard();
        ChessMove move = ChessMove.FromUCI("e2e4", board);
        stockfishBridge.AddMoveToHistory("startpos", move, "e4", 0.3f);
        
        Debug.Log($"History count: {stockfishBridge.GameHistory.Count}");
        // Expected output: History count: 1
        
        Debug.Log($"Can undo: {stockfishBridge.CanUndo()}");
        // Expected output: Can undo: True
        
        // Undo move
        var undoEntry = stockfishBridge.UndoMove();
        if (undoEntry != null)
        {
            Debug.Log($"Undid move: {undoEntry.moveNotation}");
            // Expected output: Undid move: e4
        }
        
        Debug.Log($"Can redo: {stockfishBridge.CanRedo()}");
        // Expected output: Can redo: True
        
        // Redo move
        var redoEntry = stockfishBridge.RedoMove();
        if (redoEntry != null)
        {
            Debug.Log($"Redid move: {redoEntry.moveNotation}");
            // Expected output: Redid move: e4
        }
        
        // Get PGN history
        string pgn = stockfishBridge.GetGameHistoryPGN();
        Debug.Log($"Game PGN: {pgn}");
        // Expected output: Game PGN: 1. e4
        
        // Error handling
        yield return stockfishBridge.AnalyzePositionCoroutine("invalid-fen");
        if (stockfishBridge.LastAnalysisResult.bestMove.StartsWith("ERROR"))
        {
            Debug.Log($"Error detected: {stockfishBridge.LastAnalysisResult.errorMessage}");
            // Expected output: Error detected: FEN missing required fields
        }
        
        // Nested type usage with full qualification
        var customResult = new StockfishBridge.ChessAnalysisResult();
        customResult.bestMove = "e7e8q";
        customResult.sideToMove = 'b';
        customResult.ParsePromotionData();
        
        if (customResult.isPromotion)
        {
            Debug.Log($"Custom promotion: {customResult.GetPromotionDescription()}");
            // Expected output: Custom promotion: Black promotes to queen (e7-e8)
        }
        
        // Create history entry
        var historyEntry = new StockfishBridge.GameHistoryEntry(
            "startpos", move, "e4", 0.3f
        );
        Debug.Log($"History entry: {historyEntry.moveNotation} at {historyEntry.timestamp}");
        // Expected output: History entry: e4 at [current timestamp]
        
        // Cleanup
        stockfishBridge.ClearHistory();
        Debug.Log($"History cleared. Count: {stockfishBridge.GameHistory.Count}");
        // Expected output: History cleared. Count: 0
        
        yield break;
    }
}
```

## Control flow / responsibilities & high-level algorithm summary /Side effects and I/O

Unity Update() drains thread-safe queues, UCI commands sent to engine stdin, background thread reads stdout. Analysis coordinates search+evaluation, parses UCI responses, converts centipawns to probabilities using logistic function. Game history maintains move stack with undo/redo. File I/O for engine executable, process management for Stockfish lifecycle.

## Performance, allocations, and hotspots / Threading / async considerations

Background reader thread, concurrent queues for thread communication. Analysis allocates StringBuilder, string arrays. UCI parsing creates temporary strings. Main thread processes events in Update().

## Security / safety / correctness concerns

Process.Start() with external executable, thread synchronization via locks, engine crash detection and recovery.

## Tests, debugging & observability

Built-in debug logging via enableDebugLogging flag, comprehensive test suite in RunAllTests(), OnEngineLine events for UCI monitoring. Engine state tracking via IsReady/IsEngineRunning.

## Cross-file references

ChessBoard.cs (ChessBoard, AlgebraicToCoord, CoordToAlgebraic), ChessMove.cs (ChessMove.FromUCI, ChessMove.Invalid), SPACE_UTIL.v2 (coordinate structure).

## TODO / Known limitations / Suggested improvements

<!-- TODO items and improvements (only if I explicitly mentioned in the prompt):
- Consider async/await pattern instead of coroutines for modern Unity versions
- Add support for MultiPV analysis for showing multiple candidate moves  
- Implement opening book integration for faster early game analysis
- Add time management for tournament-style time controls
- Consider caching frequent position evaluations
- Add support for chess variants (Chess960, King of the Hill, etc.)
-->

## Appendix

Key private helpers: ParseAnalysisResult() processes UCI output, ValidateFen() checks position validity, ConvertCentipawnsToWinProbability() uses logistic function, CalculateApproximateElo() estimates strength, BackgroundReaderLoop() handles engine communication.

## General Note: important behaviors

Major functionality includes PawnPromotion detection with full UCI parsing validation, Undo/Redo with comprehensive game history tracking, and Save/Load via FEN position management. Engine strength configuration supports both Elo ratings and skill levels with research-based mappings. Crash detection and automatic recovery ensures robust operation.

`checksum: 7a8f9b2e (v0.2)`