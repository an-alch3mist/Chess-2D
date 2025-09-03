# Source: `StockfishBridge.cs` — Unity Stockfish chess engine bridge with comprehensive promotion support and game management

A Unity MonoBehaviour providing non-blocking communication with the Stockfish chess engine, including full promotion handling, Elo-based difficulty adjustment, and game history management with undo/redo functionality.

## Short description

Implements a complete chess engine integration system for Unity, managing Stockfish process communication, position analysis, move generation, and game state. Provides comprehensive promotion move parsing, research-based evaluation probability calculations, crash detection/recovery, and extensive game history tracking with undo/redo support.

## Metadata

* **Filename:** `StockfishBridge.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `using System;`, `using System.Text;`, `using System.IO;`, `using System.Collections;`, `using System.Collections.Concurrent;`, `using System.Collections.Generic;`, `using System.Diagnostics;`, `using System.Threading;`, `using UnityEngine;`, `using UnityEngine.Events;`, `using SPACE_UTIL;`
* **Estimated lines:** 1850
* **Estimated chars:** 75000
* **Public types:** `StockfishBridge (class)`, `StockfishBridge.ChessAnalysisResult (class)`, `StockfishBridge.GameHistoryEntry (class)`
* **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
* **Dependencies:** `SPACE_UTIL.v2` (SPACE_UTIL is namespace), `ChessBoard.cs`, `ChessMove.cs`

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| int | defaultTimeoutMs | `[SerializeField] private int defaultTimeoutMs` | Default analysis timeout | `var timeout = bridge.defaultTimeoutMs;` |
| bool | enableDebugLogging | `[SerializeField] private bool enableDebugLogging` | Enable debug console output | `bridge.enableDebugLogging = true;` |
| bool | enableEvaluation | `[SerializeField] public bool enableEvaluation` | Enable position evaluation | `bridge.enableEvaluation = true;` |
| int | defaultDepth | `[SerializeField] private int defaultDepth` | Default search depth | `var depth = bridge.defaultDepth;` |
| int | evalDepth | `[SerializeField] private int evalDepth` | Evaluation depth setting | `var evalDepth = bridge.evalDepth;` |
| int | defaultElo | `[SerializeField] private int defaultElo` | Default engine Elo rating | `var elo = bridge.defaultElo;` |
| int | defaultSkillLevel | `[SerializeField] private int defaultSkillLevel` | Default skill level (0-20) | `var skill = bridge.defaultSkillLevel;` |
| bool | allowPlayerSideSelection | `[SerializeField] private bool allowPlayerSideSelection` | Allow player to choose side | `bridge.allowPlayerSideSelection = true;` |
| char | humanSide | `[SerializeField] private char humanSide` | Human player side ('w'/'b') | `var side = bridge.humanSide;` |
| int | maxHistorySize | `[SerializeField] private int maxHistorySize` | Maximum history entries | `var maxHist = bridge.maxHistorySize;` |
| UnityEvent<string> | OnEngineLine | `public UnityEvent<string> OnEngineLine` | Engine output line event | `bridge.OnEngineLine.AddListener(handler);` |
| UnityEvent<StockfishBridge.ChessAnalysisResult> | OnAnalysisComplete | `public UnityEvent<StockfishBridge.ChessAnalysisResult> OnAnalysisComplete` | Analysis completion event | `bridge.OnAnalysisComplete.AddListener(handler);` |
| UnityEvent<char> | OnSideToMoveChanged | `public UnityEvent<char> OnSideToMoveChanged` | Side change event | `bridge.OnSideToMoveChanged.AddListener(handler);` |
| string | LastRawOutput | `public string LastRawOutput { get; }` | Last raw engine output | `string output = bridge.LastRawOutput;` |
| StockfishBridge.ChessAnalysisResult | LastAnalysisResult | `public StockfishBridge.ChessAnalysisResult LastAnalysisResult { get; }` | Last analysis result | `var result = bridge.LastAnalysisResult;` |
| List<StockfishBridge.GameHistoryEntry> | GameHistory | `public List<StockfishBridge.GameHistoryEntry> GameHistory { get; }` | Game move history | `var history = bridge.GameHistory;` |
| int | CurrentHistoryIndex | `public int CurrentHistoryIndex { get; }` | Current history position | `int index = bridge.CurrentHistoryIndex;` |
| bool | IsEngineRunning | `public bool IsEngineRunning { get; }` | Engine process status | `bool running = bridge.IsEngineRunning;` |
| bool | IsReady | `public bool IsReady { get; }` | Engine ready status | `bool ready = bridge.IsReady;` |
| char | HumanSide | `public char HumanSide { get; set; }` | Human player side property | `bridge.HumanSide = 'w';` |
| char | EngineSide | `public char EngineSide { get; }` | Engine side (opposite of human) | `char engineSide = bridge.EngineSide;` |
| void | StartEngine | `public void StartEngine()` | Start Stockfish process | `bridge.StartEngine();` |
| void | StopEngine | `public void StopEngine()` | Stop engine and cleanup | `bridge.StopEngine();` |
| IEnumerator | AnalyzePositionCoroutine | `public IEnumerator AnalyzePositionCoroutine(string fen)` | Analyze position with defaults | `yield return bridge.AnalyzePositionCoroutine(fen);` or `StartCoroutine(bridge.AnalyzePositionCoroutine(fen));` |
| IEnumerator | AnalyzePositionCoroutine | `public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs = -1, int searchDepth = 12, int evaluationDepth = 15, int elo = 1500, int skillLevel = 8)` | Comprehensive position analysis | `yield return bridge.AnalyzePositionCoroutine(fen, 5000, 15, 18, 2000, 12);` or `StartCoroutine(bridge.AnalyzePositionCoroutine(fen, 5000, 15, 18, 2000, 12));` |
| void | SetHumanSide | `public void SetHumanSide(char side)` | Set human player side | `bridge.SetHumanSide('w');` |
| void | AddMoveToHistory | `public void AddMoveToHistory(string fen, ChessMove move, string notation, float evaluation)` | Add move to game history | `bridge.AddMoveToHistory(fen, move, "e4", 0.2f);` |
| StockfishBridge.GameHistoryEntry | UndoMove | `public StockfishBridge.GameHistoryEntry UndoMove()` | Undo last move | `var entry = bridge.UndoMove();` |
| StockfishBridge.GameHistoryEntry | RedoMove | `public StockfishBridge.GameHistoryEntry RedoMove()` | Redo next move | `var entry = bridge.RedoMove();` |
| bool | CanUndo | `public bool CanUndo()` | Check if undo possible | `bool canUndo = bridge.CanUndo();` |
| bool | CanRedo | `public bool CanRedo()` | Check if redo possible | `bool canRedo = bridge.CanRedo();` |
| void | ClearHistory | `public void ClearHistory()` | Clear all game history | `bridge.ClearHistory();` |
| string | GetGameHistoryPGN | `public string GetGameHistoryPGN()` | Get history as PGN string | `string pgn = bridge.GetGameHistoryPGN();` |
| IEnumerator | RestartEngineCoroutine | `public IEnumerator RestartEngineCoroutine()` | Restart crashed engine | `yield return bridge.RestartEngineCoroutine();` or `StartCoroutine(bridge.RestartEngineCoroutine());` |
| bool | DetectAndHandleCrash | `public bool DetectAndHandleCrash()` | Detect and handle engine crash | `bool crashed = bridge.DetectAndHandleCrash();` |
| void | SendCommand | `public void SendCommand(string command)` | Send UCI command to engine | `bridge.SendCommand("position startpos");` |
| IEnumerator | InitializeEngineCoroutine | `public IEnumerator InitializeEngineCoroutine()` | Initialize engine and wait ready | `yield return bridge.InitializeEngineCoroutine();` or `StartCoroutine(bridge.InitializeEngineCoroutine());` |
| void | TestPromotionParsing | `public void TestPromotionParsing()` | Test promotion parsing functionality | `bridge.TestPromotionParsing();` |
| void | TestEloCalculation | `public void TestEloCalculation()` | Test Elo calculation logic | `bridge.TestEloCalculation();` |

## Important types — details

### `StockfishBridge`
* **Kind:** class (inherits MonoBehaviour)
* **Note:** MonoBehaviour
* **Responsibility:** Manages Stockfish chess engine communication and game state for Unity applications
* **Constructor(s):** Unity MonoBehaviour constructor (implicit)
* **Public properties / fields:**
  - `OnEngineLine — UnityEvent<string> — Event fired for each engine output line`
  - `OnAnalysisComplete — UnityEvent<StockfishBridge.ChessAnalysisResult> — Event fired when analysis completes`
  - `OnSideToMoveChanged — UnityEvent<char> — Event fired when human side changes`
  - `LastRawOutput — string — Last raw engine output (get)`
  - `LastAnalysisResult — StockfishBridge.ChessAnalysisResult — Most recent analysis result (get)`
  - `GameHistory — List<StockfishBridge.GameHistoryEntry> — Game move history list (get)`
  - `CurrentHistoryIndex — int — Current position in history (get)`
  - `IsEngineRunning — bool — Engine process running status (get)`
  - `IsReady — bool — Engine ready for commands status (get)`
  - `HumanSide — char — Human player side 'w' or 'b' (get/set)`
  - `EngineSide — char — Engine player side (get)`
* **MonoBehaviour Lifecycle Methods:**
  * `Awake()`
    - Called on script load. Initializes the engine by calling `StartEngine()` and `StartCoroutine(InitializeEngineOnAwake())`, sets static debug logging flag.
    - Sets `enableDebugLogging_static` for cross-instance logging control.
  * `Update()`
    - Called every frame. Processes incoming engine communication lines from background thread, fires `OnEngineLine` events, tracks request completion status, and updates `IsReady` flag.
    - Drains the `incomingLines` concurrent queue and manages request tracking with thread-safe locking.
  * `OnApplicationQuit()`
    - Called before application exit. Performs cleanup by calling `StopEngine()` to gracefully shut down the engine process.
    - Ensures proper resource cleanup and process termination.
* **Public methods:**
  - **Signature:** `public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs = -1, int searchDepth = 12, int evaluationDepth = 15, int elo = 1500, int skillLevel = 8)`
    - **Description:** Comprehensive chess position analysis with full promotion support and Elo-based evaluation
    - **Parameters:** fen : string — FEN position or "startpos", movetimeMs : int — Analysis time limit, searchDepth : int — Move search depth, evaluationDepth : int — Evaluation depth, elo : int — Engine Elo rating, skillLevel : int — Skill level 0-20
    - **Returns:** IEnumerator — Coroutine that yields until analysis completes: `yield return bridge.AnalyzePositionCoroutine(fen, 5000);` or `StartCoroutine(bridge.AnalyzePositionCoroutine(fen, 5000));` (yields for 1-30 seconds depending on depth and position complexity)
    - **Side effects / state changes:** Updates `LastAnalysisResult`, fires `OnAnalysisComplete` event, modifies engine state
    - **Complexity / performance:** O(depth^branches) engine search complexity, 1-30 second duration
    - **Notes:** Must be called as coroutine, thread-safe with crash detection and recovery
  - **Signature:** `public void SetHumanSide(char side)`
    - **Description:** Set which side the human player controls and fire side change event
    - **Parameters:** side : char — 'w' for white or 'b' for black
    - **Returns:** void — Side setting: `bridge.SetHumanSide('w');`
    - **Side effects / state changes:** Updates `humanSide` field, fires `OnSideToMoveChanged` event
  - **Signature:** `public void AddMoveToHistory(string fen, ChessMove move, string notation, float evaluation)`
    - **Description:** Add a move to the game history with undo/redo support
    - **Parameters:** fen : string — Position before move, move : ChessMove — The move made, notation : string — Human-readable notation, evaluation : float — Position evaluation
    - **Returns:** void — History addition: `bridge.AddMoveToHistory(fen, move, "e4", 0.2f);`
    - **Side effects / state changes:** Modifies `GameHistory` list, updates `CurrentHistoryIndex`, truncates future history if needed
    - **Complexity / performance:** O(1) insertion, O(k) removal where k is truncated entries
  - **Signature:** `public StockfishBridge.GameHistoryEntry UndoMove()`
    - **Description:** Undo the last move and return the history entry
    - **Returns:** StockfishBridge.GameHistoryEntry — History entry or null: `var entry = bridge.UndoMove();`
    - **Side effects / state changes:** Decrements `CurrentHistoryIndex`
  - **Signature:** `public bool DetectAndHandleCrash()`
    - **Description:** Detect if engine has crashed and mark for restart
    - **Returns:** bool — True if crash detected: `bool crashed = bridge.DetectAndHandleCrash();`
    - **Side effects / state changes:** Sets `engineCrashed` flag, logs crash information
    - **Notes:** Thread-safe crash detection with timeout monitoring

### `StockfishBridge.ChessAnalysisResult`
* **Kind:** class (nested under StockfishBridge)
* **Responsibility:** Comprehensive analysis result with promotion parsing and evaluation data
* **Public properties / fields:**
  - `bestMove — string — Best move in UCI format or special values`
  - `sideToMove — char — Side to move ('w' or 'b')`
  - `currentFen — string — Current position FEN`
  - `whiteWinProbability — float — Probability for white winning (0-1)`
  - `sideToMoveWinProbability — float — Probability for side-to-move winning (0-1)`
  - `centipawnEvaluation — float — Raw centipawn score`
  - `isMateScore — bool — True if evaluation is mate score`
  - `mateDistance — int — Distance to mate (+ = white mates)`
  - `isGameEnd — bool — True if checkmate or stalemate`
  - `isCheckmate — bool — True if position is checkmate`
  - `isStalemate — bool — True if position is stalemate`
  - `inCheck — bool — True if side to move is in check`
  - `isPromotion — bool — True if bestMove is a promotion`
  - `promotionPiece — char — The promotion piece ('q', 'r', 'b', 'n')`
  - `promotionFrom — v2 — Source square of promotion`
  - `promotionTo — v2 — Target square of promotion`
  - `isPromotionCapture — bool — True if promotion includes capture`
  - `errorMessage — string — Detailed error if any`
  - `rawEngineOutput — string — Full engine response for debugging`
  - `searchDepth — int — Depth used for move search`
  - `evaluationDepth — int — Depth used for position evaluation`
  - `skillLevel — int — Skill level used (-1 if disabled)`
  - `approximateElo — int — Approximate Elo based on settings`
  - `analysisTimeMs — float — Time taken for analysis`
* **Public methods:**
  - **Signature:** `public void ParsePromotionData()`
    - **Description:** Parse promotion data from UCI move string with comprehensive validation
    - **Returns:** void — Parses bestMove: `result.ParsePromotionData();`
    - **Side effects / state changes:** Updates promotion fields based on bestMove
  - **Signature:** `public string GetPromotionDescription()`
    - **Description:** Get human-readable promotion description
    - **Returns:** string — Description text: `string desc = result.GetPromotionDescription();`
  - **Signature:** `public ChessMove ToChessMove(ChessBoard board)`
    - **Description:** Convert to ChessMove object for game application
    - **Parameters:** board : ChessBoard — Current board state
    - **Returns:** ChessMove — Move object: `ChessMove move = result.ToChessMove(board);`
  - **Signature:** `public string GetEvaluationDisplay()`
    - **Description:** Get evaluation as percentage string for UI display
    - **Returns:** string — Evaluation display: `string eval = result.GetEvaluationDisplay();`

### `StockfishBridge.GameHistoryEntry`
* **Kind:** class (nested under StockfishBridge)
* **Responsibility:** Single game history entry for undo/redo functionality
* **Constructor(s):** `GameHistoryEntry(string fen, ChessMove move, string notation, float evaluation)` — Create history entry
* **Public properties / fields:**
  - `fen — string — Position before the move`
  - `move — ChessMove — The move that was made`
  - `moveNotation — string — Human-readable move notation`
  - `evaluationScore — float — Position evaluation after move`
  - `timestamp — DateTime — When the move was made`

## MonoBehaviour special rules (must follow)
* **Note:** MonoBehaviour
* Unity lifecycle methods present in this class:
  * `Awake()`
    - Called on script load. Initializes the engine by calling `StartEngine()` and `StartCoroutine(InitializeEngineOnAwake())`, sets static debug logging flag.
    - Sets `enableDebugLogging_static` for cross-instance logging control.
  * `Update()`
    - Called every frame. Processes incoming engine communication lines from background thread, fires `OnEngineLine` events, tracks request completion status, and updates `IsReady` flag.
    - Drains the `incomingLines` concurrent queue and manages request tracking with thread-safe locking.
  * `OnApplicationQuit()`
    - Called before application exit. Performs cleanup by calling `StopEngine()` to gracefully shut down the engine process.
    - Ensures proper resource cleanup and process termination.

## Example usage
```csharp
// using GPTDeepResearch;
// using UnityEngine;

// Get reference and start analysis
var bridge = GetComponent<StockfishBridge>();
bridge.SetHumanSide('w');

// Analyze position
yield return bridge.AnalyzePositionCoroutine("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

// Check for promotion
if (bridge.LastAnalysisResult.isPromotion) {
    Debug.Log(bridge.LastAnalysisResult.GetPromotionDescription());
}
```

## Control flow / responsibilities & high-level algorithm summary
Manages asynchronous Stockfish engine communication with thread-safe queues, performs position analysis with probability calculations, handles crash detection/recovery, maintains game history with undo/redo.

## Performance, allocations, and hotspots
Background thread for engine I/O; concurrent queues minimize main-thread blocking; string parsing optimizations; potential memory pressure from large history.

## Security / safety / correctness concerns
Process management with crash detection; thread synchronization with locks; FEN validation prevents malformed input; graceful engine shutdown.

## Tests, debugging & observability
Built-in test methods for promotion parsing and Elo calculation; extensive debug logging with color-coded output; engine communication monitoring; performance timing.

## Cross-file references
Depends on `ChessBoard.cs` for position validation, `ChessMove.cs` for move objects, `SPACE_UTIL.v2` for coordinates; integrates with Unity's coroutine system.

<!-- TODO improvements: Connection pooling for multiple engines, neural network integration, opening book support, endgame tablebase integration, distributed analysis across multiple cores, position caching with transposition tables (only if I explicitly mentioned in the prompt) -->

## Appendix
Key private methods include `ParseAnalysisResult()`, `ConvertCentipawnsToWinProbability()`, `CalculateApproximateElo()`, and `BackgroundReaderLoop()` for engine communication management.

## General Note: important behaviors
Major functionality includes Stockfish engine integration, promotion move parsing with validation, research-based Elo probability calculations, comprehensive crash detection/recovery, and full game history management with undo/redo support.

`checksum: f3d9c8a7`