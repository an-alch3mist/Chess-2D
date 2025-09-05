# Source: `StockfishBridge.cs` — Unity Stockfish chess engine bridge with comprehensive promotion support and game management

Unity MonoBehaviour bridge providing non-blocking communication with Stockfish chess engine, featuring comprehensive UCI promotion parsing, evaluation analysis, and complete game history management with undo/redo functionality.

## Short description

Implements a Unity-compatible bridge to the Stockfish chess engine using UCI protocol. Provides comprehensive chess position analysis with full promotion move support (e7e8q, a2a1n), advanced evaluation calculations using research-based centipawn-to-probability conversion, game history management with undo/redo capabilities, and robust engine crash detection/recovery. Designed for Unity 2020.3+ with non-blocking coroutine-based analysis and thread-safe communication.

## Metadata

* **Filename:** `StockfishBridge.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `SPACE_UTIL` (v2 struct), `System`, `UnityEngine`
* **Estimated lines:** 1800
* **Estimated chars:** 45,000
* **Public types:** `StockfishBridge (class, MonoBehaviour)`, `StockfishBridge.ChessAnalysisResult (class)`, `StockfishBridge.GameHistoryEntry (class)`
* **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
* **Dependencies:** `SPACE_UTIL.v2` (struct), `ChessBoard.cs`, `ChessMove.cs`, Unity 2020.3+ threading, System.Diagnostics.Process

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|------------------|-------------------|
| void | Awake | private void Awake() | Initialize engine on component creation | N/A (Unity lifecycle) |
| void | Update | private void Update() | Process incoming engine messages | N/A (Unity lifecycle) |
| void | OnApplicationQuit | private void OnApplicationQuit() | Clean shutdown on app exit | N/A (Unity lifecycle) |
| void | StartEngine | public void StartEngine() | Start Stockfish process | bridge.StartEngine() |
| void | StopEngine | public void StopEngine() | Stop engine and cleanup | bridge.StopEngine() |
| IEnumerator | AnalyzePositionCoroutine | public IEnumerator AnalyzePositionCoroutine(string fen) | Analyze position with defaults | yield return bridge.AnalyzePositionCoroutine(fen) or StartCoroutine(bridge.AnalyzePositionCoroutine(fen)) |
| IEnumerator | AnalyzePositionCoroutine | public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel) | Comprehensive position analysis | yield return bridge.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8) or StartCoroutine(bridge.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8)) |
| void | SetHumanSide | public void SetHumanSide(char side) | Set human player side | bridge.SetHumanSide('w') |
| void | AddMoveToHistory | public void AddMoveToHistory(string fen, ChessMove move, string notation, float evaluation) | Add move to game history | bridge.AddMoveToHistory(fen, move, "e4", 0.3f) |
| StockfishBridge.GameHistoryEntry | UndoMove | public StockfishBridge.GameHistoryEntry UndoMove() | Undo last move | var entry = bridge.UndoMove() |
| StockfishBridge.GameHistoryEntry | RedoMove | public StockfishBridge.GameHistoryEntry RedoMove() | Redo next move | var entry = bridge.RedoMove() |
| bool | CanUndo | public bool CanUndo() | Check if undo possible | bool canUndo = bridge.CanUndo() |
| bool | CanRedo | public bool CanRedo() | Check if redo possible | bool canRedo = bridge.CanRedo() |
| void | ClearHistory | public void ClearHistory() | Clear game history | bridge.ClearHistory() |
| string | GetGameHistoryPGN | public string GetGameHistoryPGN() | Get history as PGN | string pgn = bridge.GetGameHistoryPGN() |
| IEnumerator | RestartEngineCoroutine | public IEnumerator RestartEngineCoroutine() | Restart crashed engine | yield return bridge.RestartEngineCoroutine() or StartCoroutine(bridge.RestartEngineCoroutine()) |
| bool | DetectAndHandleCrash | public bool DetectAndHandleCrash() | Check for engine crash | bool crashed = bridge.DetectAndHandleCrash() |
| void | SendCommand | public void SendCommand(string command) | Send UCI command | bridge.SendCommand("position startpos") |
| IEnumerator | InitializeEngineCoroutine | public IEnumerator InitializeEngineCoroutine() | Initialize and wait ready | yield return bridge.InitializeEngineCoroutine() or StartCoroutine(bridge.InitializeEngineCoroutine()) |
| void | RunAllTests | public void RunAllTests() | Run comprehensive tests | bridge.RunAllTests() |
| string | LastRawOutput | public string LastRawOutput { get; } | Last engine output | string output = bridge.LastRawOutput |
| StockfishBridge.ChessAnalysisResult | LastAnalysisResult | public StockfishBridge.ChessAnalysisResult LastAnalysisResult { get; } | Last analysis result | var result = bridge.LastAnalysisResult |
| List<StockfishBridge.GameHistoryEntry> | GameHistory | public List<StockfishBridge.GameHistoryEntry> GameHistory { get; } | Complete game history | var history = bridge.GameHistory |
| int | CurrentHistoryIndex | public int CurrentHistoryIndex { get; } | Current history position | int index = bridge.CurrentHistoryIndex |
| bool | IsEngineRunning | public bool IsEngineRunning { get; } | Engine process status | bool running = bridge.IsEngineRunning |
| bool | IsReady | public bool IsReady { get; } | Engine ready status | bool ready = bridge.IsReady |
| char | HumanSide | public char HumanSide { get; set; } | Human player side | char side = bridge.HumanSide |
| char | EngineSide | public char EngineSide { get; } | Engine player side | char side = bridge.EngineSide |
| UnityEvent<string> | OnEngineLine | public UnityEvent<string> OnEngineLine | Engine output event | bridge.OnEngineLine.AddListener(OnEngineOutput) |
| UnityEvent<StockfishBridge.ChessAnalysisResult> | OnAnalysisComplete | public UnityEvent<StockfishBridge.ChessAnalysisResult> OnAnalysisComplete | Analysis complete event | bridge.OnAnalysisComplete.AddListener(OnAnalysis) |
| UnityEvent<char> | OnSideToMoveChanged | public UnityEvent<char> OnSideToMoveChanged | Side change event | bridge.OnSideToMoveChanged.AddListener(OnSideChange) |

## Important types — details

### `StockfishBridge`
* **Kind:** class (MonoBehaviour)
* **Responsibility:** Unity bridge to Stockfish engine providing chess analysis and game management.
* **Constructor(s):** Unity MonoBehaviour constructor (automatic)
* **Public properties / fields:**
  * `LastRawOutput` — string — get — Last raw engine output
  * `LastAnalysisResult` — StockfishBridge.ChessAnalysisResult — get — Most recent analysis result
  * `GameHistory` — List<StockfishBridge.GameHistoryEntry> — get — Complete move history
  * `CurrentHistoryIndex` — int — get — Current position in history
  * `IsEngineRunning` — bool — get — Engine process running status
  * `IsReady` — bool — get — Engine initialized and ready
  * `HumanSide` — char — get/set — Human player side ('w' or 'b')
  * `EngineSide` — char — get — Engine player side (opposite of human)
  * `OnEngineLine` — UnityEvent<string> — get — Event fired for each engine output line
  * `OnAnalysisComplete` — UnityEvent<StockfishBridge.ChessAnalysisResult> — get — Event fired when analysis completes
  * `OnSideToMoveChanged` — UnityEvent<char> — get — Event fired when human side changes

* **Public methods:**
  * **Signature:** `public void StartEngine()`
  * **Description:** Starts the Stockfish engine process from StreamingAssets.
  * **Parameters:** None
  * **Returns:** void — bridge.StartEngine()
  * **Throws:** May log errors if engine executable not found
  * **Side effects / state changes:** Creates external process, starts background thread
  * **Notes:** Thread-safe, handles temp file creation on Windows builds

  * **Signature:** `public void StopEngine()`
  * **Description:** Gracefully stops engine with cleanup.
  * **Parameters:** None
  * **Returns:** void — bridge.StopEngine()
  * **Side effects / state changes:** Terminates process, joins threads, deletes temp files
  * **Notes:** 2-second graceful shutdown timeout before force kill

  * **Signature:** `public IEnumerator AnalyzePositionCoroutine(string fen)`
  * **Description:** Analyze position using default settings.
  * **Parameters:** fen : string — FEN string or "startpos"
  * **Returns:** IEnumerator — yield return bridge.AnalyzePositionCoroutine(fen) and StartCoroutine(bridge.AnalyzePositionCoroutine(fen)) — Duration varies by position complexity (2-10 seconds typical)
  * **Side effects / state changes:** Updates LastAnalysisResult, fires OnAnalysisComplete event
  * **Notes:** Non-blocking coroutine, handles engine crashes, timeout detection

  * **Signature:** `public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel)`
  * **Description:** Comprehensive position analysis with full configuration.
  * **Parameters:** 
    * fen : string — Position FEN or "startpos"
    * movetimeMs : int — Time limit in milliseconds (-1 for depth-based)
    * searchDepth : int — Search depth (1-30, typical 8-15)
    * evaluationDepth : int — Evaluation depth for probability calculation
    * elo : int — Engine Elo rating limit (100-3600, -1 for unlimited)
    * skillLevel : int — Skill level 0-20 (0=weakest, 20=strongest)
  * **Returns:** IEnumerator — yield return bridge.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8) and StartCoroutine(bridge.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8)) — Duration controlled by movetimeMs or position complexity
  * **Throws:** Sets error in result if FEN invalid or engine crashes
  * **Side effects / state changes:** Configures engine strength, updates analysis result with comprehensive data
  * **Complexity / performance:** O(exponential) in search depth, typical 0.1-30 seconds
  * **Notes:** Research-based evaluation conversion, full promotion parsing, crash recovery

  * **Signature:** `public void SetHumanSide(char side)`
  * **Description:** Set which side human controls.
  * **Parameters:** side : char — 'w' for white, 'b' for black
  * **Returns:** void — bridge.SetHumanSide('w')
  * **Side effects / state changes:** Updates HumanSide property, fires OnSideToMoveChanged event
  * **Notes:** Validates input, logs errors for invalid sides

  * **Signature:** `public void AddMoveToHistory(string fen, ChessMove move, string notation, float evaluation)`
  * **Description:** Add move to game history with automatic truncation.
  * **Parameters:**
    * fen : string — Position before move
    * move : ChessMove — Move object
    * notation : string — Human-readable notation
    * evaluation : float — Position evaluation
  * **Returns:** void — bridge.AddMoveToHistory(fen, move, "e4", 0.3f)
  * **Side effects / state changes:** Updates GameHistory list, manages history size limit
  * **Notes:** Truncates future moves if not at end, enforces maxHistorySize limit

  * **Signature:** `public StockfishBridge.GameHistoryEntry UndoMove()`
  * **Description:** Undo last move in history.
  * **Parameters:** None
  * **Returns:** StockfishBridge.GameHistoryEntry — var entry = bridge.UndoMove() — Returns move data or null if no moves to undo
  * **Side effects / state changes:** Decrements CurrentHistoryIndex
  * **Notes:** Does not apply move to board, only manages history pointer

  * **Signature:** `public StockfishBridge.GameHistoryEntry RedoMove()`
  * **Description:** Redo next move in history.
  * **Parameters:** None
  * **Returns:** StockfishBridge.GameHistoryEntry — var entry = bridge.RedoMove() — Returns move data or null if no moves to redo
  * **Side effects / state changes:** Increments CurrentHistoryIndex
  * **Notes:** Only works if previous undo operations exist

### `StockfishBridge.ChessAnalysisResult`
* **Kind:** class
* **Responsibility:** Comprehensive chess analysis result with promotion parsing and evaluation.
* **Constructor(s):** Default parameterless constructor
* **Public properties / fields:**
  * `bestMove` — string — get/set — Best move in UCI format or special values
  * `sideToMove` — char — get/set — Side to move ('w' or 'b')
  * `currentFen` — string — get/set — Current position FEN
  * `whiteWinProbability` — float — get/set — White win probability (0-1)
  * `sideToMoveWinProbability` — float — get/set — Current side win probability (0-1)
  * `centipawnEvaluation` — float — get/set — Raw centipawn evaluation score
  * `isMateScore` — bool — get/set — True if evaluation indicates mate
  * `mateDistance` — int — get/set — Moves to mate (+ white, - black)
  * `isGameEnd` — bool — get/set — True if checkmate or stalemate
  * `isCheckmate` — bool — get/set — True if position is checkmate
  * `isStalemate` — bool — get/set — True if position is stalemate
  * `inCheck` — bool — get/set — True if side to move in check
  * `isPromotion` — bool — get/set — True if bestMove is promotion
  * `promotionPiece` — char — get/set — Promotion piece character
  * `promotionFrom` — v2 — get/set — Source square of promotion
  * `promotionTo` — v2 — get/set — Target square of promotion
  * `isPromotionCapture` — bool — get/set — True if promotion captures
  * `errorMessage` — string — get/set — Error details if analysis failed
  * `rawEngineOutput` — string — get/set — Full engine response for debugging
  * `searchDepth` — int — get/set — Depth used for move search
  * `evaluationDepth` — int — get/set — Depth used for evaluation
  * `skillLevel` — int — get/set — Skill level used (-1 if disabled)
  * `approximateElo` — int — get/set — Estimated Elo based on settings
  * `analysisTimeMs` — float — get/set — Time taken for analysis

* **Public methods:**
  * **Signature:** `public void ParsePromotionData()`
  * **Description:** Parse promotion information from bestMove UCI string.
  * **Parameters:** None
  * **Returns:** void — result.ParsePromotionData()
  * **Side effects / state changes:** Updates all promotion-related fields based on bestMove
  * **Notes:** Validates UCI format, rank requirements, piece types

  * **Signature:** `public string GetPromotionDescription()`
  * **Description:** Get human-readable promotion description.
  * **Parameters:** None
  * **Returns:** string — string desc = result.GetPromotionDescription()
  * **Notes:** Returns empty string if not a promotion move

  * **Signature:** `public ChessMove ToChessMove(ChessBoard board)`
  * **Description:** Convert result to ChessMove object for board application.
  * **Parameters:** board : ChessBoard — Current board state
  * **Returns:** ChessMove — ChessMove move = result.ToChessMove(board)
  * **Notes:** Returns invalid move for errors or game-end states

  * **Signature:** `public string GetEvaluationDisplay()`
  * **Description:** Get evaluation as formatted percentage string.
  * **Parameters:** None
  * **Returns:** string — string eval = result.GetEvaluationDisplay()
  * **Notes:** Shows mate information or win probability percentage

### `StockfishBridge.GameHistoryEntry`
* **Kind:** class
* **Responsibility:** Single move entry in game history with metadata.
* **Constructor(s):** `GameHistoryEntry(string fen, ChessMove move, string notation, float evaluation)` — Creates entry with move data
* **Public properties / fields:**
  * `fen` — string — get/set — Position before the move
  * `move` — ChessMove — get/set — Move object that was played
  * `moveNotation` — string — get/set — Human-readable move notation
  * `evaluationScore` — float — get/set — Position evaluation after move
  * `timestamp` — DateTime — get/set — When move was made

## MonoBehaviour special rules

**Note: MonoBehaviour**

* **Awake()** - Called on script load. Initializes Stockfish engine (calls StartEngine()), starts initialization coroutine (StartCoroutine(InitializeEngineOnAwake())), and sets static debug logging flag. Does not access Unity scene objects.

* **Update()** - Called every frame. Processes incoming engine communication queue (drains incomingLines), fires OnEngineLine events for each message, tracks analysis completion state, and updates IsReady status. Critical for non-blocking engine communication.

* **OnApplicationQuit()** - Called before application termination. Ensures graceful engine shutdown (calls StopEngine()) to prevent orphaned processes and cleanup temporary files. Essential for proper resource cleanup.

## Example usage

```csharp
// namespace GPTDeepResearch required

// Basic engine startup and analysis
StockfishBridge bridge = gameObject.GetComponent<StockfishBridge>(); // or use from [SerilizedField]
bridge.StartEngine();
yield return bridge.InitializeEngineCoroutine();

// Analyze starting position with defaults
yield return bridge.AnalyzePositionCoroutine("startpos");
Debug.Log($"Best move: {bridge.LastAnalysisResult.bestMove}");
Debug.Log($"Evaluation: {bridge.LastAnalysisResult.GetEvaluationDisplay()}");

// Advanced analysis with custom settings
yield return bridge.AnalyzePositionCoroutine(
    "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", 
    -1,    // not based on time limit
    12,    // search depth
    12,    // evaluation depth  
    1800,  // 1800 Elo limit
    12     // skill level 12
);

// Check for promotion moves
if (bridge.LastAnalysisResult.isPromotion) {
    Debug.Log($"Promotion: {bridge.LastAnalysisResult.GetPromotionDescription()}");
}
```

## Control flow / responsibilities & high-level algorithm summary / Side effects and I/O

Non-blocking UCI communication via background threads with main-thread event processing. Manages external Stockfish process lifecycle, handles comprehensive position analysis with promotion parsing using research-based evaluation conversion, maintains game history with undo/redo, and provides crash detection/recovery. File I/O for engine executable, process creation/termination, thread synchronization via concurrent queues.

## Performance, allocations, and hotspots / Threading / async considerations

Heavy: position analysis O(exponential) in depth, string parsing allocations. Background reader thread, main-thread-only Unity API access, thread-safe communication queues, process I/O blocking operations.

## Security / safety / correctness concerns

External process execution, temp file creation, potential orphaned processes, unhandled engine crashes, thread synchronization issues, UCI command injection via user input.

## Tests, debugging & observability

Built-in comprehensive test suite (RunAllTests()), debug logging via enableDebugLogging flag, OnEngineLine events for raw output monitoring, rawEngineOutput field preservation, crash detection with detailed error reporting.

## Cross-file references

Dependencies: `ChessBoard.cs` (ChessBoard class), `ChessMove.cs` (ChessMove class), `SPACE_UTIL.v2` (coordinate struct). Uses Unity Process management, threading primitives, and MonoBehaviour lifecycle.

## TODO / Known limitations / Suggested improvements

<!-- TODO: Add UCI_Chess960 support for Fischer Random, implement pondering mode for background analysis, add opening book integration, optimize string parsing allocations, implement analysis cancellation tokens, add network engine support for remote analysis, improve Windows-specific file handling edge cases (only if I explicitly mentioned in the prompt) -->

## Appendix

Key private helpers: `ParseAnalysisResult()` - processes engine output into structured results, `ConvertCentipawnsToWinProbability()` - research-based probability conversion using logistic function, `DetectAndHandleCrash()` - monitors engine health with recovery, `BackgroundReaderLoop()` - thread-safe engine communication processor.

## General Note: important behaviors

Major functionality: **Comprehensive Promotion Support** (UCI parsing with validation), **Advanced Evaluation Analysis** (research-based probability conversion), **Complete Game History Management** (undo/redo with size limits), **Engine Crash Recovery** (automatic detection and restart), **Non-blocking Analysis** (coroutine-based with timeout handling). Inferred: Thread safety via concurrent queues and lock mechanisms for cross-thread communication.

`checksum: A7F9B2E1`