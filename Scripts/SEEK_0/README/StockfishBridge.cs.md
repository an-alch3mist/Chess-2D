# Source: `StockfishBridge.cs` — Unity Stockfish chess engine bridge with research-based evaluation

Unity MonoBehaviour bridge providing non-blocking chess engine communication with comprehensive position analysis and FEN-driven evaluation using Lichess research-based probability calculations.

## Short description

Implements a Unity-integrated Stockfish chess engine bridge that provides asynchronous position analysis, move generation, and research-based win probability calculations. Handles engine process management, UCI protocol communication, game history tracking with undo/redo functionality, and comprehensive promotion parsing with strict UCI validation.

## Metadata

* **Filename:** `StockfishBridge.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `SPACE_UTIL`, `System`, `System.Collections`, `System.Collections.Concurrent`, `System.Collections.Generic`, `System.Diagnostics`, `System.Threading`, `UnityEngine`, `UnityEngine.Events`, `System.IO`, `System.Linq`, `System.Text`
* **Estimated lines:** 2500
* **Estimated chars:** 52000
* **Public types:** `StockfishBridge (inherits MonoBehaviour)`, `StockfishBridge.ChessAnalysisResult (does not inherit MonoBehaviour)`, `StockfishBridge.GameHistoryEntry (does not inherit MonoBehaviour)`
* **Unity version / Target framework:** Unity 2020.3+ / .NET Standard 2.0
* **Dependencies:** `SPACE_UTIL.v2`, `ChessBoard.cs`, `ChessMove.cs`

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| bool (field) | enableEvaluation | `public bool enableEvaluation` | Enable position evaluation analysis | `bridge.enableEvaluation = true;` |
| UnityEvent<string> (field) | OnEngineLine | `public UnityEvent<string> OnEngineLine` | Event fired for each engine output line | `bridge.OnEngineLine.AddListener((line) => {});` |
| UnityEvent<StockfishBridge.ChessAnalysisResult> (field) | OnAnalysisComplete | `public UnityEvent<StockfishBridge.ChessAnalysisResult> OnAnalysisComplete` | Event fired when analysis completes | `bridge.OnAnalysisComplete.AddListener((result) => {});` |
| string (property) | LastRawOutput | `public string LastRawOutput { get; }` | Last raw engine output | `var output = bridge.LastRawOutput;` |
| StockfishBridge.ChessAnalysisResult (property) | LastAnalysisResult | `public StockfishBridge.ChessAnalysisResult LastAnalysisResult { get; }` | Last analysis result | `var result = bridge.LastAnalysisResult;` |
| List<StockfishBridge.GameHistoryEntry> (property) | GameHistory | `public List<StockfishBridge.GameHistoryEntry> GameHistory { get; }` | Game move history | `var history = bridge.GameHistory;` |
| int (property) | CurrentHistoryIndex | `public int CurrentHistoryIndex { get; }` | Current position in history | `var index = bridge.CurrentHistoryIndex;` |
| bool (property) | IsEngineRunning | `public bool IsEngineRunning { get; }` | Engine process status | `var running = bridge.IsEngineRunning;` |
| bool (property) | IsReady | `public bool IsReady { get; }` | Engine readiness status | `var ready = bridge.IsReady;` |
| void (method) | StartEngine | `public void StartEngine()` | Start Stockfish process | `bridge.StartEngine();` |
| void (method) | StopEngine | `public void StopEngine()` | Stop engine and cleanup | `bridge.StopEngine();` |
| IEnumerator (method) | AnalyzePositionCoroutine | `public IEnumerator AnalyzePositionCoroutine(string fen)` | Analyze position with defaults | `yield return bridge.AnalyzePositionCoroutine(fen);` or `StartCoroutine(bridge.AnalyzePositionCoroutine(fen));` |
| IEnumerator (method) | AnalyzePositionCoroutine | `public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel)` | Comprehensive position analysis | `yield return bridge.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8);` or `StartCoroutine(bridge.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8));` |
| void (method) | AddMoveToHistory | `public void AddMoveToHistory(string fen, ChessMove move, string notation, float evaluation)` | Add move to game history | `bridge.AddMoveToHistory(fen, move, "e4", 0.3f);` |
| StockfishBridge.GameHistoryEntry (method) | UndoMove | `public StockfishBridge.GameHistoryEntry UndoMove()` | Undo last move | `var entry = bridge.UndoMove();` |
| StockfishBridge.GameHistoryEntry (method) | RedoMove | `public StockfishBridge.GameHistoryEntry RedoMove()` | Redo next move | `var entry = bridge.RedoMove();` |
| bool (method) | CanUndo | `public bool CanUndo()` | Check if undo possible | `var canUndo = bridge.CanUndo();` |
| bool (method) | CanRedo | `public bool CanRedo()` | Check if redo possible | `var canRedo = bridge.CanRedo();` |
| void (method) | ClearHistory | `public void ClearHistory()` | Clear game history | `bridge.ClearHistory();` |
| string (method) | GetGameHistoryPGN | `public string GetGameHistoryPGN()` | Get history as PGN notation | `var pgn = bridge.GetGameHistoryPGN();` |
| IEnumerator (method) | RestartEngineCoroutine | `public IEnumerator RestartEngineCoroutine()` | Restart crashed engine | `yield return bridge.RestartEngineCoroutine();` or `StartCoroutine(bridge.RestartEngineCoroutine());` |
| bool (method) | DetectAndHandleCrash | `public bool DetectAndHandleCrash()` | Detect engine crash | `var crashed = bridge.DetectAndHandleCrash();` |
| void (method) | SendCommand | `public void SendCommand(string command)` | Send UCI command to engine | `bridge.SendCommand("position startpos");` |
| IEnumerator (method) | InitializeEngineCoroutine | `public IEnumerator InitializeEngineCoroutine()` | Initialize engine and wait ready | `yield return bridge.InitializeEngineCoroutine();` or `StartCoroutine(bridge.InitializeEngineCoroutine());` |
| void (method) | RunAllTests | `public void RunAllTests()` | Run comprehensive test suite | `bridge.RunAllTests();` |

## Important types — details

### `StockfishBridge`
* **Kind:** class inherits MonoBehaviour with full path `GPTDeepResearch.StockfishBridge`
* **Responsibility:** Main engine bridge managing Stockfish process, UCI communication, and analysis coordination
* **MonoBehaviour Status:** Inherits MonoBehaviour: Yes - uses Unity lifecycle methods for initialization and update loops
* **Constructor(s):** Unity MonoBehaviour - no explicit constructor
* **Public properties / fields:**
  * `enableEvaluation` — bool — Enable position evaluation analysis
  * `OnEngineLine` — UnityEvent<string> — Event fired for each engine output line
  * `OnAnalysisComplete` — UnityEvent<StockfishBridge.ChessAnalysisResult> — Event fired when analysis completes
  * `LastRawOutput` — string — Last raw engine output (get)
  * `LastAnalysisResult` — StockfishBridge.ChessAnalysisResult — Last analysis result (get)
  * `GameHistory` — List<StockfishBridge.GameHistoryEntry> — Game move history (get)
  * `CurrentHistoryIndex` — int — Current position in history (get)
  * `IsEngineRunning` — bool — Engine process status (get)
  * `IsReady` — bool — Engine readiness status (get)

* **Public methods:**
  * **Signature:** `public void StartEngine()`
    * **Description:** Start the Stockfish engine process
    * **Parameters:** None
    * **Returns:** void — `bridge.StartEngine()`
    * **Side effects / state changes:** Creates engine process, starts background reader thread
    * **Notes:** Thread-safe, handles duplicate calls gracefully

  * **Signature:** `public void StopEngine()`
    * **Description:** Stop engine and clean up resources
    * **Parameters:** None
    * **Returns:** void — `bridge.StopEngine()`
    * **Side effects / state changes:** Terminates process, cleans temp files, stops threads
    * **Notes:** Graceful shutdown with forced termination fallback

  * **Signature:** `public IEnumerator AnalyzePositionCoroutine(string fen)`
    * **Description:** Analyze position using inspector defaults
    * **Parameters:** 
      * fen : string — Position in FEN notation or "startpos"
    * **Returns:** IEnumerator — `yield return bridge.AnalyzePositionCoroutine(fen);` and `StartCoroutine(bridge.AnalyzePositionCoroutine(fen));` expected duration (1-3 seconds based on depth, yield return new WaitForSeconds, yield return null)
    * **Side effects / state changes:** Updates LastAnalysisResult, fires OnAnalysisComplete event
    * **Notes:** Non-blocking coroutine, Unity main-thread required

  * **Signature:** `public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel)`
    * **Description:** Comprehensive position analysis with custom parameters
    * **Parameters:**
      * fen : string — Position in FEN notation
      * movetimeMs : int — Time limit in milliseconds (-1 for depth-based)
      * searchDepth : int — Search depth for move finding
      * evaluationDepth : int — Depth for position evaluation
      * elo : int — Engine strength rating
      * skillLevel : int — Skill level 0-20
    * **Returns:** IEnumerator — `yield return bridge.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8);` and `StartCoroutine(bridge.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8));` expected duration (variable based on time/depth, yield return null)
    * **Side effects / state changes:** Configures engine strength, updates analysis result
    * **Notes:** Research-based evaluation using Lichess probability equation

  * **Signature:** `public void AddMoveToHistory(string fen, ChessMove move, string notation, float evaluation)`
    * **Description:** Add move to game history with undo/redo support
    * **Parameters:**
      * fen : string — Position before move
      * move : ChessMove — Move object
      * notation : string — Human-readable notation
      * evaluation : float — Position evaluation
    * **Returns:** void — `bridge.AddMoveToHistory(fen, move, "e4", 0.3f)`
    * **Side effects / state changes:** Updates GameHistory, truncates redo paths
    * **Notes:** Maintains history size limits

### `StockfishBridge.ChessAnalysisResult`
* **Kind:** class does not inherit MonoBehaviour with full path `GPTDeepResearch.StockfishBridge.ChessAnalysisResult`
* **Responsibility:** Comprehensive analysis result with research-based evaluation and promotion data
* **MonoBehaviour Status:** Inherits MonoBehaviour: No - serializable data class
* **Constructor(s):** Default parameterless constructor
* **Public properties / fields:**
  * `bestMove` — string — Best move in UCI format or game end state
  * `sideToMove` — char — Side to move ('w' or 'b') extracted from FEN
  * `currentFen` — string — Current position FEN
  * `engineSideWinProbability` — float — Win probability for white (0-1, research-based)
  * `sideToMoveWinProbability` — float — Win probability for side to move (0-1)
  * `centipawnEvaluation` — float — Raw centipawn score from engine
  * `isMateScore` — bool — True if evaluation is mate score
  * `mateDistance` — int — Distance to mate (+ white mates, - black mates)
  * `isGameEnd` — bool — True if checkmate or stalemate
  * `isCheckmate` — bool — True if position is checkmate
  * `isStalemate` — bool — True if position is stalemate
  * `inCheck` — bool — True if side to move is in check
  * `isPromotion` — bool — True if bestMove is a promotion
  * `promotionPiece` — char — The promotion piece ('q', 'r', 'b', 'n')
  * `promotionFrom` — v2 — Source square of promotion
  * `promotionTo` — v2 — Target square of promotion
  * `isPromotionCapture` — bool — True if promotion includes capture
  * `errorMessage` — string — Detailed error if any
  * `rawEngineOutput` — string — Full engine response for debugging
  * `searchDepth` — int — Depth used for move search
  * `evaluationDepth` — int — Depth used for position evaluation
  * `skillLevel` — int — Skill level used (-1 if disabled)
  * `approximateElo` — int — Approximate Elo based on settings
  * `analysisTimeMs` — float — Time taken for analysis

* **Public methods:**
  * **Signature:** `public void ParsePromotionData()`
    * **Description:** Parse promotion data from UCI move string with enhanced validation
    * **Parameters:** None
    * **Returns:** void — `result.ParsePromotionData()`
    * **Side effects / state changes:** Sets promotion-related fields based on bestMove
    * **Notes:** Strict UCI protocol validation, rank and side consistency checks

  * **Signature:** `public string GetPromotionDescription()`
    * **Description:** Get human-readable promotion description
    * **Parameters:** None
    * **Returns:** string — `var desc = result.GetPromotionDescription()`
    * **Notes:** Returns empty string if not a promotion

  * **Signature:** `public ChessMove ToChessMove(ChessBoard board)`
    * **Description:** Convert to ChessMove object for game application
    * **Parameters:**
      * board : ChessBoard — Current board state
    * **Returns:** ChessMove — `var move = result.ToChessMove(board)`
    * **Notes:** Returns Invalid move for errors or game end states

  * **Signature:** `public string GetEvaluationDisplay()`
    * **Description:** Get evaluation as percentage string for UI display
    * **Parameters:** None
    * **Returns:** string — `var display = result.GetEvaluationDisplay()`
    * **Notes:** Research-based formatting with strength indicators

### `StockfishBridge.GameHistoryEntry`
* **Kind:** class does not inherit MonoBehaviour with full path `GPTDeepResearch.StockfishBridge.GameHistoryEntry`
* **Responsibility:** Game history entry for undo/redo functionality
* **MonoBehaviour Status:** Inherits MonoBehaviour: No - data storage class
* **Constructor(s):** `public GameHistoryEntry(string fen, ChessMove move, string notation, float evaluation)`
* **Public properties / fields:**
  * `fen` — string — Position before the move
  * `move` — ChessMove — The move that was made
  * `moveNotation` — string — Human-readable move notation
  * `evaluationScore` — float — Position evaluation after move
  * `timestamp` — DateTime — When the move was made

## MonoBehaviour Detection and Special Rules

**StockfishBridge inherits MonoBehaviour** and implements the following Unity lifecycle methods:

**Awake()**
- Called on script load. Initializes engine process (calls StartEngine()), starts engine initialization coroutine (StartCoroutine(InitializeEngineOnAwake())), and sets static debug logging flag.
- Prepares engine for immediate use after scene load.

**Update()**
- Called every frame. Drains incoming message queue from background thread (incomingLines.TryDequeue()), fires OnEngineLine events, and tracks analysis completion states.
- Processes engine communication on main thread, updates IsReady state based on "readyok" responses.

**OnApplicationQuit()**
- Called when application closes. Performs cleanup by calling StopEngine() to gracefully shutdown the engine process.
- Ensures proper resource cleanup and process termination.

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
    
    private IEnumerator StockfishBridge_Check() 
    {
        // === Engine Initialization ===
        stockfishBridge.StartEngine();
        yield return stockfishBridge.InitializeEngineCoroutine();
        
        if (!stockfishBridge.IsReady)
        {
            Debug.Log("<color=red>Engine failed to initialize</color>");
            yield break;
        }
        
        Debug.Log("<color=green>Engine initialized successfully</color>");
        
        // === Event Subscription ===
        stockfishBridge.OnAnalysisComplete.AddListener((result) => {
            Debug.Log($"<color=cyan>Analysis complete: {result.bestMove}</color>");
            Debug.Log($"<color=cyan>Evaluation: {result.GetEvaluationDisplay()}</color>");
        });
        
        stockfishBridge.OnEngineLine.AddListener((line) => {
            if (line.StartsWith("info") && line.Contains("score"))
            {
                Debug.Log($"<color=yellow>Engine info: {line}</color>");
            }
        });
        
        // === Position Analysis ===
        string startPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        
        // Analyze with default settings
        yield return stockfishBridge.AnalyzePositionCoroutine(startPosition);
        
        StockfishBridge.ChessAnalysisResult result = stockfishBridge.LastAnalysisResult;
        
        if (result.bestMove.StartsWith("ERROR"))
        {
            Debug.Log($"<color=red>Analysis error: {result.errorMessage}</color>");
        }
        else
        {
            Debug.Log($"<color=green>Best move: {result.bestMove}</color>");
            Debug.Log($"<color=green>Win probability: {result.sideToMoveWinProbability:P1}</color>");
            Debug.Log($"<color=green>Side to move: {(result.sideToMove == 'w' ? "White" : "Black")}</color>");
        }
        
        // === Custom Analysis Parameters ===
        yield return stockfishBridge.AnalyzePositionCoroutine(
            startPosition, 
            movetimeMs: 2000,  // 2 second time limit
            searchDepth: 15,   // Search depth
            evaluationDepth: 18, // Evaluation depth
            elo: 2000,         // Engine strength
            skillLevel: -1     // Use Elo instead of skill level
        );
        
        StockfishBridge.ChessAnalysisResult customResult = stockfishBridge.LastAnalysisResult;
        Debug.Log($"<color=green>Custom analysis time: {customResult.analysisTimeMs:F1}ms</color>");
        Debug.Log($"<color=green>Approximate Elo: {customResult.approximateElo}</color>");
        
        // === Promotion Detection ===
        string promotionPosition = "rnbqkb1r/ppppp2p/5np1/6B1/3P4/2N5/PPP1pPPP/R2QKB1R b KQkq - 0 1";
        yield return stockfishBridge.AnalyzePositionCoroutine(promotionPosition);
        
        StockfishBridge.ChessAnalysisResult promResult = stockfishBridge.LastAnalysisResult;
        if (promResult.isPromotion)
        {
            Debug.Log($"<color=magenta>Promotion detected: {promResult.GetPromotionDescription()}</color>");
            Debug.Log($"<color=magenta>Promotion piece: {promResult.promotionPiece}</color>");
            Debug.Log($"<color=magenta>From: {promResult.promotionFrom} To: {promResult.promotionTo}</color>");
        }
        
        // === Game History Management ===
        ChessBoard board = new ChessBoard();
        ChessMove move = ChessMove.FromUCI("e2e4", board);
        
        stockfishBridge.AddMoveToHistory(startPosition, move, "e4", 0.25f);
        Debug.Log($"<color=cyan>History size: {stockfishBridge.GameHistory.Count}</color>");
        Debug.Log($"<color=cyan>Can undo: {stockfishBridge.CanUndo()}</color>");
        
        // Undo/Redo operations
        if (stockfishBridge.CanUndo())
        {
            StockfishBridge.GameHistoryEntry undoEntry = stockfishBridge.UndoMove();
            Debug.Log($"<color=cyan>Undid move: {undoEntry.moveNotation}</color>");
        }
        
        if (stockfishBridge.CanRedo())
        {
            StockfishBridge.GameHistoryEntry redoEntry = stockfishBridge.RedoMove();
            Debug.Log($"<color=cyan>Redid move: {redoEntry.moveNotation}</color>");
        }
        
        // Get game history as PGN
        string pgn = stockfishBridge.GetGameHistoryPGN();
        Debug.Log($"<color=cyan>Game PGN: {pgn}</color>");
        
        // === Engine Commands ===
        stockfishBridge.SendCommand("setoption name Hash value 256");
        stockfishBridge.SendCommand("position startpos moves e2e4");
        
        // === Crash Recovery ===
        bool crashed = stockfishBridge.DetectAndHandleCrash();
        if (crashed)
        {
            Debug.Log("<color=yellow>Engine crash detected, restarting...</color>");
            yield return stockfishBridge.RestartEngineCoroutine();
        }
        
        // === Nested Type Usage ===
        StockfishBridge.ChessAnalysisResult newResult = new StockfishBridge.ChessAnalysisResult();
        newResult.bestMove = "e7e8q";
        newResult.sideToMove = 'w';
        newResult.ParsePromotionData();
        
        if (newResult.isPromotion)
        {
            Debug.Log($"<color=green>Manual promotion test: {newResult.GetPromotionDescription()}</color>");
        }
        
        StockfishBridge.GameHistoryEntry historyEntry = new StockfishBridge.GameHistoryEntry(
            startPosition, move, "e4", 0.15f
        );
        Debug.Log($"<color=green>Created history entry: {historyEntry.moveNotation} at {historyEntry.timestamp}</color>");
        
        // === Testing Framework ===
        stockfishBridge.RunAllTests();
        
        Debug.Log("<color=green>StockfishBridge demo completed successfully</color>");
        
        // Expected output: 
        // "Engine initialized successfully"
        // "Best move: e2e4" 
        // "Win probability: 52.3%"
        // "Custom analysis time: 2000.0ms"
        // "History size: 1"
        // "Game PGN: 1. e4"
        
        yield break;
    }
}
```

## Control flow / responsibilities & high-level algorithm summary

Main runtime flow: Unity Awake() starts engine process → background thread reads UCI output → Update() drains message queue on main thread → analysis coroutines send UCI commands and wait for "bestmove" responses → research-based evaluation converts centipawns to win probabilities using Lichess equation → results parsed and events fired.

## Performance, allocations, and hotspots / Threading / async considerations

Heavy operations: engine process spawn, UCI communication, FEN parsing. Background reader thread with ConcurrentQueue for thread-safe message passing. Main thread only for Unity API calls.

## Security / safety / correctness concerns

Process termination handling, temp file cleanup, thread synchronization, null checks on engine responses, FEN validation, UCI protocol compliance.

## Tests, debugging & observability

Built-in comprehensive test suite via RunAllTests(), extensive Debug.Log with color coding, crash detection and recovery mechanisms, raw engine output preservation for debugging.

## Cross-file references

Depends on `ChessBoard.cs` (ChessBoard class), `ChessMove.cs` (ChessMove class), `SPACE_UTIL.v2` (coordinate structure) for coordinate conversion and move representation.

## General Note: important behaviors

Major functionalities include: **Promotion Detection** with strict UCI validation and rank checking, **Undo/Redo System** with history management and PGN export, **Research-Based Evaluation** using Lichess probability equation for accurate win percentage calculations, **Engine Crash Recovery** with automatic restart capabilities.

`checksum: a7f8b2e1 (prompt v0.5)`