# Source: `StockfishBridge.cs` — Unity Stockfish chess engine bridge with comprehensive promotion support and game management

Unity MonoBehaviour that provides non-blocking communication with the Stockfish chess engine, including full promotion move parsing, undo/redo functionality, and enhanced evaluation with Elo support.

## Short description (2–4 sentences)
This file implements a comprehensive bridge between Unity and the Stockfish chess engine, providing asynchronous analysis capabilities with full promotion support (e7e8q, a2a1n, etc.). It handles engine lifecycle management, crash detection/recovery, game history tracking with undo/redo, and converts raw engine output into structured analysis results. The bridge supports configurable engine strength via Elo ratings and skill levels, with research-based probability calculations for win/loss evaluation.

## Metadata

* **Filename:** `StockfishBridge.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `using SPACE_UTIL;` (SPACE_UTIL is namespace)
* **Estimated lines:** 1800
* **Estimated chars:** 45,000
* **Public types:** `StockfishBridge (class inherits MonoBehaviour), StockfishBridge.ChessAnalysisResult (class), StockfishBridge.GameHistoryEntry (class)`
* **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
* **Dependencies:** `SPACE_UTIL.v2` (SPACE_UTIL is namespace), `ChessBoard.cs`, `ChessMove.cs`

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| UnityEvent<string> | OnEngineLine | public UnityEvent<string> OnEngineLine | Event fired for each engine output line | OnEngineLine.AddListener(line => Debug.Log(line)); |
| UnityEvent<StockfishBridge.ChessAnalysisResult> | OnAnalysisComplete | public UnityEvent<StockfishBridge.ChessAnalysisResult> OnAnalysisComplete | Event fired when analysis completes | OnAnalysisComplete.AddListener(result => Debug.Log(result)); |
| UnityEvent<char> | OnSideToMoveChanged | public UnityEvent<char> OnSideToMoveChanged | Event fired when human side changes | OnSideToMoveChanged.AddListener(side => Debug.Log(side)); |
| StockfishBridge.ChessAnalysisResult (class) | LastAnalysisResult | public StockfishBridge.ChessAnalysisResult LastAnalysisResult { get; } | Most recent analysis result | var result = bridge.LastAnalysisResult; |
| List<StockfishBridge.GameHistoryEntry> | GameHistory | public List<StockfishBridge.GameHistoryEntry> GameHistory { get; } | Complete game move history | var history = bridge.GameHistory; |
| int | CurrentHistoryIndex | public int CurrentHistoryIndex { get; } | Current position in game history | var index = bridge.CurrentHistoryIndex; |
| bool | IsEngineRunning | public bool IsEngineRunning { get; } | Whether engine process is active | bool running = bridge.IsEngineRunning; |
| bool | IsReady | public bool IsReady { get; } | Whether engine is initialized and ready | bool ready = bridge.IsReady; |
| char | HumanSide | public char HumanSide { get; set; } | Which side human plays ('w' or 'b') | bridge.HumanSide = 'w'; |
| char | EngineSide | public char EngineSide { get; } | Which side engine plays | char side = bridge.EngineSide; |
| string | LastRawOutput | public string LastRawOutput { get; } | Raw engine output from last analysis | string raw = bridge.LastRawOutput; |
| void | StartEngine | public void StartEngine() | Start the Stockfish engine process | bridge.StartEngine(); |
| void | StopEngine | public void StopEngine() | Stop engine and clean up resources | bridge.StopEngine(); |
| IEnumerator | AnalyzePositionCoroutine | public IEnumerator AnalyzePositionCoroutine(string fen) | Analyze position using defaults | yield return bridge.AnalyzePositionCoroutine("startpos"); StartCoroutine(bridge.AnalyzePositionCoroutine("startpos")); |
| IEnumerator | AnalyzePositionCoroutine | public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel) | Full analysis with all parameters | yield return bridge.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8); StartCoroutine(bridge.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8)); |
| void | SetHumanSide | public void SetHumanSide(char side) | Set which side human plays | bridge.SetHumanSide('w'); |
| void | AddMoveToHistory | public void AddMoveToHistory(string fen, ChessMove move, string notation, float evaluation) | Add move to game history | bridge.AddMoveToHistory(fen, move, "e4", 0.3f); |
| StockfishBridge.GameHistoryEntry | UndoMove | public StockfishBridge.GameHistoryEntry UndoMove() | Undo last move | var entry = bridge.UndoMove(); |
| StockfishBridge.GameHistoryEntry | RedoMove | public StockfishBridge.GameHistoryEntry RedoMove() | Redo next move | var entry = bridge.RedoMove(); |
| bool | CanUndo | public bool CanUndo() | Check if undo is possible | bool canUndo = bridge.CanUndo(); |
| bool | CanRedo | public bool CanRedo() | Check if redo is possible | bool canRedo = bridge.CanRedo(); |
| void | ClearHistory | public void ClearHistory() | Clear all game history | bridge.ClearHistory(); |
| string | GetGameHistoryPGN | public string GetGameHistoryPGN() | Get history as PGN notation | string pgn = bridge.GetGameHistoryPGN(); |
| IEnumerator | RestartEngineCoroutine | public IEnumerator RestartEngineCoroutine() | Restart engine after crash | yield return bridge.RestartEngineCoroutine(); StartCoroutine(bridge.RestartEngineCoroutine()); |
| bool | DetectAndHandleCrash | public bool DetectAndHandleCrash() | Detect and handle engine crash | bool crashed = bridge.DetectAndHandleCrash(); |
| void | SendCommand | public void SendCommand(string command) | Send arbitrary UCI command | bridge.SendCommand("isready"); |
| IEnumerator | InitializeEngineCoroutine | public IEnumerator InitializeEngineCoroutine() | Initialize engine and wait for ready | yield return bridge.InitializeEngineCoroutine(); StartCoroutine(bridge.InitializeEngineCoroutine()); |
| void | RunAllTests | public void RunAllTests() | Run comprehensive API tests | bridge.RunAllTests(); |

## Important types — details

### `StockfishBridge`
* **Kind:** class inherits MonoBehaviour
* **Responsibility:** Main bridge between Unity and Stockfish engine with complete game management
* **Constructor(s):** Unity creates via GameObject
* **Note:** MonoBehaviour

**Unity Lifecycle Methods:**
```
Awake()
- Called on script load. Initializes the Stockfish engine (calls StartEngine()), starts initialization coroutine (StartCoroutine(InitializeEngineOnAwake())), and sets static debug logging flag.
- Does not wait for engine readiness - that happens in InitializeEngineCoroutine().

Update()
- Called every frame. Drains incoming engine output lines from background thread (incomingLines.TryDequeue()), fires OnEngineLine events, tracks analysis completion, and updates readiness state.
- Handles thread-safe communication between background reader and main Unity thread.

OnApplicationQuit()
- Called when application closes. Stops engine process and cleans up resources (calls StopEngine()).
- Ensures proper cleanup to prevent process leaks.
```

**Public properties / fields:**
* OnEngineLine — UnityEvent<string> — Event fired for each engine output line (get/set)
* OnAnalysisComplete — UnityEvent<StockfishBridge.ChessAnalysisResult> — Event fired when analysis completes (get/set)  
* OnSideToMoveChanged — UnityEvent<char> — Event fired when human side changes (get/set)
* LastRawOutput — string — Raw engine output from last analysis (get)
* LastAnalysisResult — StockfishBridge.ChessAnalysisResult — Most recent analysis result (get)
* GameHistory — List<StockfishBridge.GameHistoryEntry> — Complete game move history (get)
* CurrentHistoryIndex — int — Current position in game history (get)
* IsEngineRunning — bool — Whether engine process is active and not crashed (get)
* IsReady — bool — Whether engine is initialized and ready for commands (get)
* HumanSide — char — Which side human plays ('w' or 'b') (get/set)
* EngineSide — char — Which side engine plays (opposite of HumanSide) (get)

**Public methods:**

* **Signature:** `public void StartEngine()`
  * **Description:** Start the Stockfish engine process with crash detection
  * **Parameters:** None
  * **Returns:** void — StartEngine()
  * **Side effects:** Creates background process, starts reader thread, sets up communication
  * **Notes:** Thread-safe, handles multiple calls gracefully

* **Signature:** `public void StopEngine()`
  * **Description:** Stop engine gracefully and clean up all resources
  * **Parameters:** None  
  * **Returns:** void — StopEngine()
  * **Side effects:** Terminates process, joins threads, cleans temp files
  * **Notes:** Attempts graceful shutdown before force termination

* **Signature:** `public IEnumerator AnalyzePositionCoroutine(string fen)`
  * **Description:** Analyze chess position using inspector default settings
  * **Parameters:** fen : string — FEN position or "startpos"
  * **Returns:** IEnumerator — yield return bridge.AnalyzePositionCoroutine("startpos") and StartCoroutine(bridge.AnalyzePositionCoroutine("startpos")) (~0.5-5 seconds via yield return new WaitForSeconds(timeout))
  * **Throws:** Sets error in result if engine crashes or times out
  * **Side effects:** Updates LastAnalysisResult, fires OnAnalysisComplete event
  * **Notes:** Coroutine, uses defaultDepth/evalDepth/Elo from inspector

* **Signature:** `public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel)`
  * **Description:** Comprehensive chess position analysis with full parameter control
  * **Parameters:**
    * fen : string — FEN position or "startpos"
    * movetimeMs : int — Time limit in milliseconds (-1 for depth-based)
    * searchDepth : int — Search depth (1-50, recommended 8-20)
    * evaluationDepth : int — Evaluation depth for win probability
    * elo : int — Engine Elo rating (100-3600, -1 to disable)
    * skillLevel : int — Skill level (0-20, -1 to disable)
  * **Returns:** IEnumerator — yield return bridge.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8) and StartCoroutine(bridge.AnalyzePositionCoroutine(...)) (~0.1-30 seconds via yield return new WaitForSeconds(calculated))
  * **Throws:** Updates result with ERROR prefix if validation fails or engine crashes
  * **Side effects:** Configures engine, sends UCI commands, parses results with promotion detection
  * **Complexity:** O(exponential) in search depth, can allocate significant memory for deep searches
  * **Notes:** Coroutine, includes comprehensive promotion parsing and probability calculation

* **Signature:** `public void SetHumanSide(char side)`
  * **Description:** Set which side the human player controls
  * **Parameters:** side : char — 'w' for white, 'b' for black
  * **Returns:** void — bridge.SetHumanSide('w')
  * **Side effects:** Updates HumanSide property, fires OnSideToMoveChanged event
  * **Notes:** Validates input, rejects invalid sides

* **Signature:** `public void AddMoveToHistory(string fen, ChessMove move, string notation, float evaluation)`
  * **Description:** Add move to game history with undo/redo support
  * **Parameters:**
    * fen : string — Position FEN before the move
    * move : ChessMove — The chess move object
    * notation : string — Human-readable notation (e.g., "e4", "Nf3")
    * evaluation : float — Position evaluation after move
  * **Returns:** void — bridge.AddMoveToHistory(fen, move, "e4", 0.3f)
  * **Side effects:** Updates GameHistory, truncates future history if in middle, enforces size limit
  * **Notes:** Thread-safe, maintains chronological order

* **Signature:** `public StockfishBridge.GameHistoryEntry UndoMove()`
  * **Description:** Undo the last move in history
  * **Parameters:** None
  * **Returns:** StockfishBridge.GameHistoryEntry — var entry = bridge.UndoMove() (null if cannot undo)
  * **Side effects:** Decrements CurrentHistoryIndex
  * **Notes:** Returns null if no moves to undo

* **Signature:** `public StockfishBridge.GameHistoryEntry RedoMove()`
  * **Description:** Redo the next move in history
  * **Parameters:** None
  * **Returns:** StockfishBridge.GameHistoryEntry — var entry = bridge.RedoMove() (null if cannot redo)
  * **Side effects:** Increments CurrentHistoryIndex  
  * **Notes:** Returns null if no moves to redo

* **Signature:** `public bool CanUndo()`
  * **Description:** Check if undo operation is possible
  * **Parameters:** None
  * **Returns:** bool — bool canUndo = bridge.CanUndo()
  * **Notes:** Read-only check, no side effects

* **Signature:** `public bool CanRedo()`
  * **Description:** Check if redo operation is possible  
  * **Parameters:** None
  * **Returns:** bool — bool canRedo = bridge.CanRedo()
  * **Notes:** Read-only check, no side effects

* **Signature:** `public void ClearHistory()`
  * **Description:** Clear all game history and reset indices
  * **Parameters:** None
  * **Returns:** void — bridge.ClearHistory()
  * **Side effects:** Empties GameHistory list, resets CurrentHistoryIndex to -1

* **Signature:** `public string GetGameHistoryPGN()`
  * **Description:** Get game history in PGN notation format
  * **Parameters:** None
  * **Returns:** string — string pgn = bridge.GetGameHistoryPGN()
  * **Notes:** Returns "No moves played" if history empty

* **Signature:** `public IEnumerator RestartEngineCoroutine()`
  * **Description:** Restart engine after crash with full reinitialization
  * **Parameters:** None
  * **Returns:** IEnumerator — yield return bridge.RestartEngineCoroutine() and StartCoroutine(bridge.RestartEngineCoroutine()) (~1-5 seconds via yield return new WaitForSeconds(1f))
  * **Side effects:** Stops current engine, waits, starts new engine, reinitializes
  * **Notes:** Coroutine, includes delay for process cleanup

* **Signature:** `public bool DetectAndHandleCrash()`
  * **Description:** Detect engine crash and mark as crashed
  * **Parameters:** None
  * **Returns:** bool — bool crashed = bridge.DetectAndHandleCrash()
  * **Side effects:** Sets internal crash flag if process has exited
  * **Notes:** Thread-safe, checks process status and responsiveness

* **Signature:** `public void SendCommand(string command)`
  * **Description:** Send arbitrary UCI command directly to engine
  * **Parameters:** command : string — UCI command (e.g., "isready", "quit")
  * **Returns:** void — bridge.SendCommand("isready")
  * **Throws:** Marks engine as crashed if communication fails
  * **Side effects:** Writes to engine stdin, updates last command time
  * **Notes:** Thread-safe, includes crash detection and recovery

* **Signature:** `public IEnumerator InitializeEngineCoroutine()`
  * **Description:** Initialize engine with UCI protocol and wait for readiness
  * **Parameters:** None
  * **Returns:** IEnumerator — yield return bridge.InitializeEngineCoroutine() and StartCoroutine(bridge.InitializeEngineCoroutine()) (~2-10 seconds via yield return null)
  * **Side effects:** Sends "uci" and "isready" commands, sets IsReady flag
  * **Notes:** Coroutine, 10-second timeout, required before analysis

* **Signature:** `public void RunAllTests()`
  * **Description:** Run comprehensive test suite for all API functionality
  * **Parameters:** None
  * **Returns:** void — bridge.RunAllTests()
  * **Side effects:** Executes multiple test scenarios, logs results with color coding
  * **Notes:** Includes promotion parsing, Elo calculation, FEN validation, crash recovery tests

### `StockfishBridge.ChessAnalysisResult`
* **Kind:** class
* **Responsibility:** Comprehensive container for chess analysis results with promotion support
* **Constructor(s):** `public ChessAnalysisResult()` — default constructor with neutral values

**Public properties / fields:**
* bestMove — string — Best move in UCI format ("e2e4", "e7e8q") or special values (get/set)
* sideToMove — char — Side to move ('w' or 'b') (get/set)
* currentFen — string — Current position FEN (get/set)
* whiteWinProbability — float — Probability (0-1) that white wins (get/set)
* sideToMoveWinProbability — float — Probability (0-1) that current side wins (get/set)
* centipawnEvaluation — float — Raw centipawn evaluation score (get/set)
* isMateScore — bool — True if evaluation represents mate (get/set)
* mateDistance — int — Moves to mate (+ for white, - for black) (get/set)
* isGameEnd — bool — True if checkmate or stalemate (get/set)
* isCheckmate — bool — True if position is checkmate (get/set)
* isStalemate — bool — True if position is stalemate (get/set)
* inCheck — bool — True if side to move is in check (get/set)
* isPromotion — bool — True if bestMove is a promotion (get/set)
* promotionPiece — char — Promotion piece ('q', 'r', 'b', 'n' or uppercase) (get/set)
* promotionFrom — v2 — Source square of promotion (get/set)
* promotionTo — v2 — Target square of promotion (get/set)
* isPromotionCapture — bool — True if promotion includes capture (get/set)
* errorMessage — string — Detailed error message if analysis failed (get/set)
* rawEngineOutput — string — Complete engine response for debugging (get/set)
* searchDepth — int — Depth used for move search (get/set)
* evaluationDepth — int — Depth used for evaluation (get/set)
* skillLevel — int — Skill level used (-1 if disabled) (get/set)
* approximateElo — int — Calculated Elo based on settings (get/set)
* analysisTimeMs — float — Time taken for analysis in milliseconds (get/set)

**Public methods:**

* **Signature:** `public void ParsePromotionData()`
  * **Description:** Parse promotion information from bestMove UCI string with validation
  * **Parameters:** None
  * **Returns:** void — result.ParsePromotionData()
  * **Side effects:** Sets isPromotion, promotionPiece, promotionFrom, promotionTo, isPromotionCapture
  * **Notes:** Validates UCI format, rank requirements, and piece types

* **Signature:** `public string GetPromotionDescription()`
  * **Description:** Get human-readable description of promotion move
  * **Parameters:** None
  * **Returns:** string — string desc = result.GetPromotionDescription()
  * **Notes:** Returns empty string if not a promotion

* **Signature:** `public ChessMove ToChessMove(ChessBoard board)`
  * **Description:** Convert result to ChessMove object for board application
  * **Parameters:** board : ChessBoard — Board context for move validation
  * **Returns:** ChessMove — var move = result.ToChessMove(board)
  * **Notes:** Returns ChessMove.Invalid() for errors or game end

* **Signature:** `public string GetEvaluationDisplay()`
  * **Description:** Get evaluation formatted for UI display
  * **Parameters:** None
  * **Returns:** string — string eval = result.GetEvaluationDisplay()
  * **Notes:** Handles both mate scores and percentage probabilities

### `StockfishBridge.GameHistoryEntry`
* **Kind:** class  
* **Responsibility:** Single entry in game history for undo/redo functionality
* **Constructor(s):** `public GameHistoryEntry(string fen, ChessMove move, string notation, float evaluation)` — creates entry with timestamp

**Public properties / fields:**
* fen — string — Position FEN before the move (get/set)
* move — ChessMove — The chess move that was made (get/set)
* moveNotation — string — Human-readable move notation (get/set)
* evaluationScore — float — Position evaluation after move (get/set)
* timestamp — DateTime — When the move was recorded (get/set)

## Example usage

```csharp
// Required namespaces:
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using GPTDeepResearch;
// using SPACE_UTIL;

public class ExampleUsage : MonoBehaviour 
{
    [SerializeField] private StockfishBridge stockfishBridge; // Assign in Inspector
    
    private IEnumerator StockfishBridge_Check()
    {
        // Engine lifecycle management
        stockfishBridge.StartEngine();
        yield return stockfishBridge.InitializeEngineCoroutine();
        
        if (stockfishBridge.IsReady && stockfishBridge.IsEngineRunning)
        {
            Debug.Log("<color=green>Engine started and ready</color>");
        }
        
        // Side management
        stockfishBridge.SetHumanSide('w');
        Debug.Log($"<color=cyan>Human plays: {(stockfishBridge.HumanSide == 'w' ? "White" : "Black")}</color>");
        Debug.Log($"<color=cyan>Engine plays: {(stockfishBridge.EngineSide == 'w' ? "White" : "Black")}</color>");
        
        // Basic position analysis
        yield return stockfishBridge.AnalyzePositionCoroutine("startpos");
        
        StockfishBridge.ChessAnalysisResult result = stockfishBridge.LastAnalysisResult;
        Debug.Log($"<color=green>Best move: {result.bestMove}</color>");
        Debug.Log($"<color=green>Evaluation: {result.GetEvaluationDisplay()}</color>");
        
        // Advanced analysis with custom parameters
        string testFen = "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1";
        yield return stockfishBridge.AnalyzePositionCoroutine(testFen, 2000, 15, 18, 1800, 12);
        
        result = stockfishBridge.LastAnalysisResult;
        if (result.isPromotion)
        {
            Debug.Log($"<color=yellow>Promotion found: {result.GetPromotionDescription()}</color>");
        }
        
        // Game history management
        ChessBoard board = new ChessBoard();
        ChessMove move = ChessMove.FromUCI("e2e4", board);
        stockfishBridge.AddMoveToHistory("startpos", move, "e4", 0.3f);
        
        Debug.Log($"<color=cyan>History size: {stockfishBridge.GameHistory.Count}</color>");
        Debug.Log($"<color=cyan>Can undo: {stockfishBridge.CanUndo()}</color>");
        Debug.Log($"<color=cyan>Can redo: {stockfishBridge.CanRedo()}</color>");
        
        // Undo/redo operations
        if (stockfishBridge.CanUndo())
        {
            StockfishBridge.GameHistoryEntry undoEntry = stockfishBridge.UndoMove();
            Debug.Log($"<color=green>Undid move: {undoEntry.moveNotation}</color>");
        }
        
        if (stockfishBridge.CanRedo())
        {
            StockfishBridge.GameHistoryEntry redoEntry = stockfishBridge.RedoMove();
            Debug.Log($"<color=green>Redid move: {redoEntry.moveNotation}</color>");
        }
        
        // PGN export
        string pgn = stockfishBridge.GetGameHistoryPGN();
        Debug.Log($"<color=cyan>Game PGN: {pgn}</color>");
        
        // Event subscription
        stockfishBridge.OnAnalysisComplete.AddListener(OnAnalysisCompleted);
        stockfishBridge.OnEngineLine.AddListener(line => Debug.Log($"<color=gray>Engine: {line}</color>"));
        stockfishBridge.OnSideToMoveChanged.AddListener(side => Debug.Log($"<color=blue>Side changed to: {side}</color>"));
        
        // Crash detection and recovery
        bool crashed = stockfishBridge.DetectAndHandleCrash();
        if (crashed)
        {
            Debug.Log("<color=red>Engine crashed, restarting...</color>");
            yield return stockfishBridge.RestartEngineCoroutine();
        }
        
        // Direct UCI commands
        stockfishBridge.SendCommand("isready");
        
        // Testing suite
        stockfishBridge.RunAllTests();
        
        // Cleanup
        stockfishBridge.ClearHistory();
        stockfishBridge.StopEngine();
        
        // Expected outputs:
        // "Engine started and ready"
        // "Human plays: White"  
        // "Engine plays: Black"
        // "Best move: e2e4"
        // "Evaluation: White: 52.3%"
        // "History size: 1"
        // "Can undo: True"
        // "Undid move: e4"
        // "Redid move: e4"
        // "Game PGN: 1. e4"
    }
    
    private void OnAnalysisCompleted(StockfishBridge.ChessAnalysisResult result)
    {
        Debug.Log($"<color=green>Analysis completed: {result.bestMove} | {result.GetEvaluationDisplay()}</color>");
        
        if (result.bestMove.StartsWith("ERROR"))
        {
            Debug.Log($"<color=red>Analysis error: {result.errorMessage}</color>");
        }
        else if (result.isGameEnd)
        {
            string endType = result.isCheckmate ? "Checkmate" : "Stalemate";
            Debug.Log($"<color=yellow>Game ended: {endType}</color>");
        }
    }
}
```

## Control flow / responsibilities & high-level algorithm summary

Main thread handles Unity lifecycle and events while background thread reads engine output. Analysis flow: validate FEN → configure engine strength → send position → parse bestmove/evaluation → convert to probabilities using logistic function. Crash detection monitors process health and restarts if needed.

## Performance, allocations, and hotspots / Threading / async considerations

Heavy operations: deep searches (exponential complexity), FEN parsing. Uses background thread for engine I/O, ConcurrentQueue for thread-safe communication. Main-thread-only for Unity API calls and coroutines.

## Security / safety / correctness concerns

Process execution for engine binary, potential crashes handled. Null checks for engine communication, validates FEN format. Thread-safe collections prevent race conditions.

## Tests, debugging & observability

Built-in comprehensive test suite (RunAllTests) with color-coded logging. Engine output events and crash detection for monitoring. Debug logging can be disabled via inspector.

## Cross-file references

Depends on `ChessBoard.cs` (ChessBoard class), `ChessMove.cs` (ChessMove class), `SPACE_UTIL.v2` (SPACE_UTIL is namespace) for coordinate handling.

## General Note: important behaviors

Major functionalities include pawn promotion detection with full UCI parsing, comprehensive undo/redo system, engine crash recovery, research-based Elo calculation, and probability conversion using logistic functions for realistic win/loss estimates.

`checksum: a7f3b891 v0.3`