# Source: `StockfishBridge.cs` — Unity-Stockfish chess engine bridge with research-based evaluation

Unity MonoBehaviour bridge for Stockfish chess engine providing non-blocking position analysis, move generation, and game management with Lichess research-based win probability calculations.

## Short description

Implements a comprehensive Unity-Stockfish communication layer with FEN-based position analysis, UCI protocol handling, and research-based evaluation metrics. Manages engine process lifecycle, provides coroutine-based analysis, handles promotion parsing with strict UCI validation, and maintains game history with undo/redo functionality. Features crash detection, engine restart capabilities, and configurable strength settings.

## Metadata

* **Filename:** `StockfishBridge.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `SPACE_UTIL` (provides `v2` struct)
* **Estimated lines:** 1,500+
* **Estimated chars:** ~52,000
* **Public types:** `StockfishBridge (inherits MonoBehaviour), StockfishBridge.ChessAnalysisResult (class), StockfishBridge.GameHistoryEntry (class)`
* **Unity version / Target framework:** Unity 2020.3+ / .NET Standard 2.0
* **Dependencies:** `SPACE_UTIL.v2` (coordinate struct), `ChessBoard.cs`, `ChessMove.cs`

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| UnityEvent<string> | OnEngineLine | `public UnityEvent<string> OnEngineLine` | Engine output line event | `bridge.OnEngineLine.AddListener(line => Debug.Log(line));` |
| UnityEvent<StockfishBridge.ChessAnalysisResult> | OnAnalysisComplete | `public UnityEvent<StockfishBridge.ChessAnalysisResult> OnAnalysisComplete` | Analysis completion event | `bridge.OnAnalysisComplete.AddListener(result => ProcessResult(result));` |
| string | LastRawOutput | `public string LastRawOutput { get; }` | Last engine raw output | `var output = bridge.LastRawOutput;` |
| StockfishBridge.ChessAnalysisResult | LastAnalysisResult | `public StockfishBridge.ChessAnalysisResult LastAnalysisResult { get; }` | Last analysis result | `var result = bridge.LastAnalysisResult;` |
| List<StockfishBridge.GameHistoryEntry> | GameHistory | `public List<StockfishBridge.GameHistoryEntry> GameHistory { get; }` | Game move history | `var history = bridge.GameHistory;` |
| int | CurrentHistoryIndex | `public int CurrentHistoryIndex { get; }` | Current history position | `var index = bridge.CurrentHistoryIndex;` |
| bool | IsEngineRunning | `public bool IsEngineRunning { get; }` | Engine process status | `var running = bridge.IsEngineRunning;` |
| bool | IsReady | `public bool IsReady { get; }` | Engine ready status | `var ready = bridge.IsReady;` |
| void | StartEngine | `public void StartEngine()` | Start Stockfish process | `bridge.StartEngine();` |
| void | StopEngine | `public void StopEngine()` | Stop engine and cleanup | `bridge.StopEngine();` |
| IEnumerator | AnalyzePositionCoroutine | `public IEnumerator AnalyzePositionCoroutine(string fen)` | Analyze position with defaults | `yield return bridge.AnalyzePositionCoroutine("startpos");` or `StartCoroutine(bridge.AnalyzePositionCoroutine("startpos"));` |
| IEnumerator | AnalyzePositionCoroutine | `public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel)` | Full analysis with settings | `yield return bridge.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8);` or `StartCoroutine(bridge.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8));` |
| void | AddMoveToHistory | `public void AddMoveToHistory(string fen, ChessMove move, string notation, float evaluation)` | Add move to history | `bridge.AddMoveToHistory(fen, move, "e4", 0.3f);` |
| StockfishBridge.GameHistoryEntry | UndoMove | `public StockfishBridge.GameHistoryEntry UndoMove()` | Undo last move | `var entry = bridge.UndoMove();` |
| StockfishBridge.GameHistoryEntry | RedoMove | `public StockfishBridge.GameHistoryEntry RedoMove()` | Redo next move | `var entry = bridge.RedoMove();` |
| bool | CanUndo | `public bool CanUndo()` | Check undo availability | `var canUndo = bridge.CanUndo();` |
| bool | CanRedo | `public bool CanRedo()` | Check redo availability | `var canRedo = bridge.CanRedo();` |
| void | ClearHistory | `public void ClearHistory()` | Clear game history | `bridge.ClearHistory();` |
| string | GetGameHistoryPGN | `public string GetGameHistoryPGN()` | Get history as PGN | `var pgn = bridge.GetGameHistoryPGN();` |
| IEnumerator | RestartEngineCoroutine | `public IEnumerator RestartEngineCoroutine()` | Restart crashed engine | `yield return bridge.RestartEngineCoroutine();` or `StartCoroutine(bridge.RestartEngineCoroutine());` |
| bool | DetectAndHandleCrash | `public bool DetectAndHandleCrash()` | Detect engine crash | `var crashed = bridge.DetectAndHandleCrash();` |
| void | SendCommand | `public void SendCommand(string command)` | Send UCI command | `bridge.SendCommand("isready");` |
| IEnumerator | InitializeEngineCoroutine | `public IEnumerator InitializeEngineCoroutine()` | Initialize and wait ready | `yield return bridge.InitializeEngineCoroutine();` or `StartCoroutine(bridge.InitializeEngineCoroutine());` |
| void | RunAllTests | `public void RunAllTests()` | Run comprehensive tests | `bridge.RunAllTests();` |

## Important types — details

### `StockfishBridge` (Main MonoBehaviour class)

* **Kind:** class (inherits MonoBehaviour)
* **Responsibility:** Unity-Stockfish engine bridge with process management, UCI communication, and analysis coordination
* **MonoBehaviour Status:** Inherits MonoBehaviour: Yes - manages engine process lifecycle and provides Unity integration
* **Constructor(s):** Unity-managed (no explicit constructors)
* **Public properties / fields:**
  * `OnEngineLine — UnityEvent<string> — get/set — Engine output line event`
  * `OnAnalysisComplete — UnityEvent<StockfishBridge.ChessAnalysisResult> — get/set — Analysis completion event`  
  * `LastRawOutput — string — get — Last raw engine output`
  * `LastAnalysisResult — StockfishBridge.ChessAnalysisResult — get — Last analysis result`
  * `GameHistory — List<StockfishBridge.GameHistoryEntry> — get — Game move history`
  * `CurrentHistoryIndex — int — get — Current history position`
  * `IsEngineRunning — bool — get — Engine process status`
  * `IsReady — bool — get — Engine ready status`

* **Public methods:**

  * **Signature:** `public void StartEngine()`
    * **Description:** Starts Stockfish process and background reader thread
    * **Parameters:** None
    * **Returns:** `void — StockfishBridge.StartEngine()`
    * **Side effects:** Creates process, starts thread, sets engine state
    * **Notes:** Thread-safe, handles already-running state

  * **Signature:** `public void StopEngine()`
    * **Description:** Stops engine gracefully with cleanup and resource disposal
    * **Parameters:** None
    * **Returns:** `void — StockfishBridge.StopEngine()`
    * **Side effects:** Terminates process, joins thread, cleans temp files
    * **Notes:** Graceful shutdown with 2s timeout, force kill if needed

  * **Signature:** `public IEnumerator AnalyzePositionCoroutine(string fen)`
    * **Description:** Analyzes position using inspector default settings
    * **Parameters:** 
      * `fen : string — FEN notation or "startpos"`
    * **Returns:** `IEnumerator — yield return StockfishBridge.AnalyzePositionCoroutine(fen) or StartCoroutine(StockfishBridge.AnalyzePositionCoroutine(fen)) — duration: ~0.5-2 seconds based on depth`
    * **Side effects:** Updates LastAnalysisResult, fires OnAnalysisComplete
    * **Notes:** Uses defaultDepth (12), defaultElo (1500), defaultSkillLevel (8)

  * **Signature:** `public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel)`
    * **Description:** Full chess position analysis with research-based evaluation and configurable strength
    * **Parameters:**
      * `fen : string — FEN notation or "startpos"`
      * `movetimeMs : int — Search time limit (-1 for depth-based)`
      * `searchDepth : int — Search depth (12 recommended)`
      * `evaluationDepth : int — Evaluation depth (15 recommended)`
      * `elo : int — Engine Elo limit (1500 default)`
      * `skillLevel : int — Skill level 0-20 (8 default)`
    * **Returns:** `IEnumerator — yield return StockfishBridge.AnalyzePositionCoroutine(...) or StartCoroutine(StockfishBridge.AnalyzePositionCoroutine(...)) — duration: movetimeMs + 5s timeout`
    * **Side effects:** Engine configuration, UCI commands, result parsing, event firing
    * **Complexity:** O(search_depth) exponential with configurable time bounds
    * **Notes:** FEN-based side management, research-based win probability calculation, promotion parsing

  * **Signature:** `public void AddMoveToHistory(string fen, ChessMove move, string notation, float evaluation)`
    * **Description:** Adds move to game history with evaluation score
    * **Parameters:**
      * `fen : string — Position before move`
      * `move : ChessMove — The move made`
      * `notation : string — Human-readable notation`
      * `evaluation : float — Position evaluation`
    * **Returns:** `void — StockfishBridge.AddMoveToHistory(fen, move, "e4", 0.3f)`
    * **Side effects:** Updates GameHistory, CurrentHistoryIndex, truncates redo path
    * **Notes:** Respects maxHistorySize limit (100)

  * **Signature:** `public StockfishBridge.GameHistoryEntry UndoMove()`
    * **Description:** Undoes last move and returns history entry
    * **Parameters:** None
    * **Returns:** `StockfishBridge.GameHistoryEntry — var entry = StockfishBridge.UndoMove() — Returns entry or null if no moves`
    * **Side effects:** Decrements CurrentHistoryIndex
    * **Notes:** Thread-safe, bounds-checked

  * **Signature:** `public StockfishBridge.GameHistoryEntry RedoMove()`
    * **Description:** Redoes next move in history
    * **Parameters:** None
    * **Returns:** `StockfishBridge.GameHistoryEntry — var entry = StockfishBridge.RedoMove() — Returns entry or null if no redo available`
    * **Side effects:** Increments CurrentHistoryIndex
    * **Notes:** Thread-safe, bounds-checked

### `StockfishBridge.ChessAnalysisResult` (Nested analysis result class)

* **Kind:** class (nested in StockfishBridge)
* **Responsibility:** Comprehensive chess position analysis result with research-based evaluation and promotion data
* **MonoBehaviour Status:** Inherits MonoBehaviour: No - serializable data container
* **Constructor(s):** Default parameterless constructor
* **Public properties / fields:**
  * `bestMove — string — get/set — Best move in UCI format or game state`
  * `sideToMove — char — get/set — 'w' or 'b' extracted from FEN`
  * `currentFen — string — get/set — Current position FEN`
  * `whiteWinProbability — float — get/set — 0-1 probability for white winning (Lichess research-based)`
  * `sideToMoveWinProbability — float — get/set — 0-1 probability for side-to-move winning`
  * `centipawnEvaluation — float — get/set — Raw centipawn score from engine`
  * `isMateScore — bool — get/set — True if evaluation is mate score`
  * `mateDistance — int — get/set — Distance to mate (+ white, - black)`
  * `isGameEnd — bool — get/set — True if checkmate or stalemate`
  * `isCheckmate — bool — get/set — True if position is checkmate`
  * `isStalemate — bool — get/set — True if position is stalemate`
  * `inCheck — bool — get/set — True if side to move is in check`
  * `isPromotion — bool — get/set — True if bestMove is promotion`
  * `promotionPiece — char — get/set — Promotion piece ('q','r','b','n' - UCI lowercase)`
  * `promotionFrom — v2 — get/set — Source square of promotion`
  * `promotionTo — v2 — get/set — Target square of promotion`
  * `isPromotionCapture — bool — get/set — True if promotion includes capture`
  * `errorMessage — string — get/set — Detailed error if any`
  * `rawEngineOutput — string — get/set — Full engine response for debugging`
  * `searchDepth — int — get/set — Depth used for move search`
  * `evaluationDepth — int — get/set — Depth used for position evaluation`
  * `skillLevel — int — get/set — Skill level used (-1 if disabled)`
  * `approximateElo — int — get/set — Approximate Elo based on settings`
  * `analysisTimeMs — float — get/set — Time taken for analysis`

* **Public methods:**

  * **Signature:** `public void ParsePromotionData()`
    * **Description:** Parses promotion data from UCI move string with enhanced validation
    * **Parameters:** None
    * **Returns:** `void — StockfishBridge.ChessAnalysisResult.ParsePromotionData()`
    * **Side effects:** Updates isPromotion, promotionPiece, promotionFrom, promotionTo, isPromotionCapture
    * **Notes:** Strict UCI format validation (e7e8q), rank validation, side consistency checks

  * **Signature:** `public string GetPromotionDescription()`
    * **Description:** Gets human-readable promotion description
    * **Parameters:** None
    * **Returns:** `string — var desc = StockfishBridge.ChessAnalysisResult.GetPromotionDescription()`
    * **Notes:** Converts UCI lowercase to display names with capture indication

  * **Signature:** `public ChessMove ToChessMove(ChessBoard board)`
    * **Description:** Converts to ChessMove object for game application
    * **Parameters:**
      * `board : ChessBoard — Current board state`
    * **Returns:** `ChessMove — var move = StockfishBridge.ChessAnalysisResult.ToChessMove(board)`
    * **Notes:** Handles invalid moves, game end states

  * **Signature:** `public string GetEvaluationDisplay()`
    * **Description:** Gets evaluation as percentage string for UI with research-based formatting
    * **Parameters:** None
    * **Returns:** `string — var display = StockfishBridge.ChessAnalysisResult.GetEvaluationDisplay()`
    * **Notes:** Shows mate info or win percentage with strength indicators

### `StockfishBridge.GameHistoryEntry` (Nested history entry class)

* **Kind:** class (nested in StockfishBridge) 
* **Responsibility:** Game history entry for undo/redo functionality with move and evaluation data
* **MonoBehaviour Status:** Inherits MonoBehaviour: No - serializable data container
* **Constructor(s):** `GameHistoryEntry(string fen, ChessMove move, string notation, float evaluation)`
* **Public properties / fields:**
  * `fen — string — get/set — Position before the move`
  * `move — ChessMove — get/set — The move that was made`
  * `moveNotation — string — get/set — Human-readable move notation`
  * `evaluationScore — float — get/set — Position evaluation after move`
  * `timestamp — DateTime — get/set — When the move was made`

## Example usage

```csharp
// Required namespaces:
// using System;
// using System.Collections;
// using UnityEngine;
// using GPTDeepResearch;
// using SPACE_UTIL;

public class ExampleUsage : MonoBehaviour 
{
    [SerializeField] private StockfishBridge stockfishBridge; // Assign in Inspector
    
    private void Awake()
    {
        // Subscribe to engine events
        stockfishBridge.OnEngineLine.AddListener(line => 
        {
            Debug.Log($"<color=yellow>Engine: {line}</color>");
        });
        
        stockfishBridge.OnAnalysisComplete.AddListener(result => 
        {
            Debug.Log($"<color=green>Analysis: {result.bestMove} | {result.GetEvaluationDisplay()}</color>");
        });
    }
    
    private IEnumerator StockfishBridge_Check()
    {
        // === Engine Management ===
        // Start engine (automatically called in Awake)
        stockfishBridge.StartEngine();
        
        // Wait for engine initialization
        yield return stockfishBridge.InitializeEngineCoroutine();
        
        if (stockfishBridge.IsReady && stockfishBridge.IsEngineRunning)
        {
            Debug.Log("<color=green>Engine initialized successfully</color>");
        }
        
        // === Position Analysis ===
        // Analyze starting position with defaults
        yield return stockfishBridge.AnalyzePositionCoroutine("startpos");
        
        var startingResult = stockfishBridge.LastAnalysisResult;
        Debug.Log($"<color=cyan>Starting position: {startingResult.bestMove}</color>");
        // Expected output: "Starting position: e2e4" or similar opening move
        
        // Analyze position with custom settings
        string testFen = "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1";
        yield return stockfishBridge.AnalyzePositionCoroutine(testFen, 1000, 10, 12, 1200, 5);
        
        var customResult = stockfishBridge.LastAnalysisResult;
        Debug.Log($"<color=cyan>Custom analysis: {customResult.bestMove} | Win probability: {customResult.sideToMoveWinProbability:P1}</color>");
        // Expected output: "Custom analysis: e7e5 | Win probability: 45.2%" or similar
        
        // === Promotion Handling ===
        // Test promotion detection
        var promotionResult = new StockfishBridge.ChessAnalysisResult
        {
            bestMove = "e7e8q",
            sideToMove = 'w'
        };
        promotionResult.ParsePromotionData();
        
        if (promotionResult.isPromotion)
        {
            Debug.Log($"<color=magenta>Promotion detected: {promotionResult.GetPromotionDescription()}</color>");
            // Expected output: "Promotion detected: White promotes to Queen (e7-e8)"
        }
        
        // === Game History Management ===
        // Create test moves for history
        ChessBoard testBoard = new ChessBoard();
        ChessMove move1 = ChessMove.FromUCI("e2e4", testBoard);
        ChessMove move2 = ChessMove.FromUCI("e7e5", testBoard);
        
        // Add moves to history
        stockfishBridge.AddMoveToHistory("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", move1, "e4", 0.3f);
        stockfishBridge.AddMoveToHistory("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1", move2, "e5", 0.1f);
        
        Debug.Log($"<color=green>History size: {stockfishBridge.GameHistory.Count}</color>");
        // Expected output: "History size: 2"
        
        // Test undo/redo
        if (stockfishBridge.CanUndo())
        {
            var undoEntry = stockfishBridge.UndoMove();
            Debug.Log($"<color=yellow>Undid move: {undoEntry.moveNotation}</color>");
            // Expected output: "Undid move: e5"
        }
        
        if (stockfishBridge.CanRedo())
        {
            var redoEntry = stockfishBridge.RedoMove();
            Debug.Log($"<color=yellow>Redid move: {redoEntry.moveNotation}</color>");
            // Expected output: "Redid move: e5"
        }
        
        // Get game history as PGN
        string pgn = stockfishBridge.GetGameHistoryPGN();
        Debug.Log($"<color=cyan>Game PGN: {pgn}</color>");
        // Expected output: "Game PGN: 1. e4 e5"
        
        // === Engine Commands ===
        // Send custom UCI command
        stockfishBridge.SendCommand("isready");
        
        // === Error Handling ===
        // Test crash detection
        bool crashDetected = stockfishBridge.DetectAndHandleCrash();
        if (!crashDetected)
        {
            Debug.Log("<color=green>Engine running normally</color>");
        }
        
        // === Analysis Result Properties ===
        var result = stockfishBridge.LastAnalysisResult;
        Debug.Log($"<color=cyan>Side to move: {result.sideToMove}</color>");
        Debug.Log($"<color=cyan>Centipawn evaluation: {result.centipawnEvaluation:F1}</color>");
        Debug.Log($"<color=cyan>Is mate: {result.isMateScore}</color>");
        Debug.Log($"<color=cyan>Game end: {result.isGameEnd}</color>");
        
        // Expected outputs:
        // "Side to move: b"
        // "Centipawn evaluation: 15.0"
        // "Is mate: False"
        // "Game end: False"
        
        // === Testing ===
        // Run comprehensive tests
        stockfishBridge.RunAllTests();
        Debug.Log("<color=green>Comprehensive tests completed</color>");
        
        yield break;
    }
    
    private void OnDestroy()
    {
        // Cleanup engine on destroy
        if (stockfishBridge != null)
        {
            stockfishBridge.StopEngine();
        }
    }
}
```

## MonoBehaviour Lifecycle Methods

### `StockfishBridge` MonoBehaviour Methods:

**Awake()**
- Called on script load. Initializes engine process (calls StartEngine()), starts engine initialization coroutine (StartCoroutine(InitializeEngineOnAwake())), and sets enableDebugLogging_static flag.
- Sets up engine process and background communication thread before scene objects are active.

**Update()**
- Called every frame. Drains incoming engine lines from background thread (incomingLines.TryDequeue()), fires OnEngineLine events, tracks analysis completion, and updates IsReady status.
- Processes engine communication on main thread and manages request tracking state.

**OnApplicationQuit()**
- Called when application is quitting. Ensures clean engine shutdown by calling StopEngine().
- Prevents engine process from remaining active after Unity closes.

## Control flow / responsibilities & high-level algorithm summary

Engine process management with UCI protocol communication via background thread. Analysis flow: FEN validation → engine configuration → UCI commands → response parsing → research-based evaluation conversion → event firing. Background reader thread feeds main thread queue for Unity integration.

## Performance, allocations, and hotspots / Threading considerations

Heavy allocations in string parsing, potential GC from concurrent queues. Background reader thread with main-thread event processing.

## Security / safety / correctness concerns

Process crash detection, temp file cleanup, thread synchronization via locks and volatile fields.

## Tests, debugging & observability

Built-in comprehensive test suite via RunAllTests(). Colored debug logging with enableDebugLogging flag. Engine output events for monitoring.

## Cross-file references

`SPACE_UTIL.v2` (coordinate struct), `ChessBoard.cs` (AlgebraicToCoord, CoordToAlgebraic methods), `ChessMove.cs` (FromUCI, Invalid methods)

## General Note: important behaviors

Major functionalities include Stockfish engine integration, research-based win probability calculation using Lichess equation, UCI promotion parsing with strict validation, comprehensive game history with undo/redo, engine crash detection and recovery, and configurable engine strength via Elo/skill settings.

`checksum: af3d2e1b (v0.5)`