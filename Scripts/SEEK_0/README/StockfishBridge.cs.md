# Source: `StockfishBridge.cs` — Unity Stockfish chess engine bridge for position analysis and move evaluation

Unity MonoBehaviour bridge providing non-blocking communication with the Stockfish chess engine for position analysis, move evaluation, and research-based win probability calculations.

## Short description

This file implements a Unity-compatible bridge to the Stockfish chess engine, focusing purely on position analysis without game state management. It provides research-based evaluation using Lichess accuracy metrics, comprehensive UCI promotion parsing, engine crash detection/recovery, and analysis logging. The system is designed for multi-board scenarios with clean separation of concerns between analysis and game state.

## Metadata

* **Filename:** `StockfishBridge.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `System, System.Linq, System.Text, System.IO, System.Collections, System.Collections.Concurrent, System.Collections.Generic, System.Diagnostics, System.Threading, UnityEngine, UnityEngine.Events, SPACE_UTIL`
* **Estimated lines:** ~1,200
* **Estimated chars:** ~45,000
* **Public types:** `StockfishBridge (inherits MonoBehaviour), StockfishBridge.ChessAnalysisResult (class), StockfishBridge.AnalysisLogEntry (class)`
* **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
* **Dependencies:** `SPACE_UTIL.v2 (struct from external namespace SPACE_UTIL), ChessBoard (referenced for coordinate conversion)`

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| int | defaultTimeoutMs | `[SerializeField] private int defaultTimeoutMs = 20000` | Engine command timeout | N/A (private field) |
| bool | enableDebugLogging | `[SerializeField] private bool enableDebugLogging = true` | Debug output control | N/A (private field) |
| bool | enableEvaluation | `[SerializeField] public bool enableEvaluation = true` | Position evaluation toggle | `bridge.enableEvaluation = true;` |
| int | defaultDepth | `[SerializeField] private int defaultDepth = 12` | Default search depth | N/A (private field) |
| int | evalDepth | `[SerializeField] private int evalDepth = 15` | Evaluation depth | N/A (private field) |
| int | defaultElo | `[SerializeField] private int defaultElo = 1500` | Default engine Elo | N/A (private field) |
| int | defaultSkillLevel | `[SerializeField] private int defaultSkillLevel = 8` | Default skill level | N/A (private field) |
| UnityEvent<string> | OnEngineLine | `public UnityEvent<string> OnEngineLine` | Engine output event | `bridge.OnEngineLine.AddListener(HandleLine);` |
| UnityEvent<StockfishBridge.ChessAnalysisResult> | OnAnalysisComplete | `public UnityEvent<StockfishBridge.ChessAnalysisResult> OnAnalysisComplete` | Analysis completion event | `bridge.OnAnalysisComplete.AddListener(HandleResult);` |
| string | LastRawOutput | `public string LastRawOutput { get; }` | Last engine output | `string output = bridge.LastRawOutput;` |
| StockfishBridge.ChessAnalysisResult | LastAnalysisResult | `public StockfishBridge.ChessAnalysisResult LastAnalysisResult { get; }` | Last analysis result | `var result = bridge.LastAnalysisResult;` |
| List<StockfishBridge.AnalysisLogEntry> | AnalysisLog | `public List<StockfishBridge.AnalysisLogEntry> AnalysisLog { get; }` | Analysis history | `var log = bridge.AnalysisLog;` |
| bool | IsEngineRunning | `public bool IsEngineRunning { get; }` | Engine status check | `bool running = bridge.IsEngineRunning;` |
| bool | IsReady | `public bool IsReady { get; }` | Engine readiness | `bool ready = bridge.IsReady;` |
| void | StartEngine | `public void StartEngine()` | Start engine process | `bridge.StartEngine();` |
| void | StopEngine | `public void StopEngine()` | Stop engine process | `bridge.StopEngine();` |
| IEnumerator | AnalyzePositionCoroutine | `public IEnumerator AnalyzePositionCoroutine(string fen)` | Analyze position with defaults | `yield return bridge.AnalyzePositionCoroutine(fen);` or `StartCoroutine(bridge.AnalyzePositionCoroutine(fen));` |
| IEnumerator | AnalyzePositionCoroutine | `public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs = -1, int searchDepth = 12, int evaluationDepth = 15, int elo = 1500, int skillLevel = 8)` | Full analysis with parameters | `yield return bridge.AnalyzePositionCoroutine(fen, 1000, 10, 12, 1500, 5);` or `StartCoroutine(bridge.AnalyzePositionCoroutine(...));` |
| void | ClearAnalysisLog | `public void ClearAnalysisLog()` | Clear analysis history | `bridge.ClearAnalysisLog();` |
| string | ExportAnalysisLog | `public string ExportAnalysisLog()` | Export analysis log | `string export = bridge.ExportAnalysisLog();` |
| IEnumerator | RestartEngineCoroutine | `public IEnumerator RestartEngineCoroutine()` | Restart crashed engine | `yield return bridge.RestartEngineCoroutine();` or `StartCoroutine(bridge.RestartEngineCoroutine());` |
| bool | DetectAndHandleCrash | `public bool DetectAndHandleCrash()` | Check engine crash | `bool crashed = bridge.DetectAndHandleCrash();` |
| void | SendCommand | `public void SendCommand(string command)` | Send UCI command | `bridge.SendCommand("isready");` |
| IEnumerator | InitializeEngineCoroutine | `public IEnumerator InitializeEngineCoroutine()` | Initialize engine | `yield return bridge.InitializeEngineCoroutine();` or `StartCoroutine(bridge.InitializeEngineCoroutine());` |
| void | RunAllTests | `public void RunAllTests()` | Run test suite | `bridge.RunAllTests();` |

## Important types — details

### `StockfishBridge`
* **Kind:** class (inherits MonoBehaviour)
* **MonoBehaviour Status:** Inherits MonoBehaviour: Yes - Uses Unity lifecycle (Awake, Update, OnApplicationQuit) and SerializeField attributes
* **Responsibility:** Manages Stockfish engine process communication and provides position analysis API
* **Constructor(s):** Unity handles MonoBehaviour construction
* **Public properties / fields:**
  * `enableEvaluation — bool — get/set — Controls position evaluation during analysis`
  * `OnEngineLine — UnityEvent<string> — get — Event fired for each engine output line`
  * `OnAnalysisComplete — UnityEvent<StockfishBridge.ChessAnalysisResult> — get — Event fired when analysis completes`
  * `LastRawOutput — string — get — Most recent raw engine output`
  * `LastAnalysisResult — StockfishBridge.ChessAnalysisResult — get — Most recent analysis result`
  * `AnalysisLog — List<StockfishBridge.AnalysisLogEntry> — get — History of analysis operations`
  * `IsEngineRunning — bool — get — True if engine process is active and responsive`
  * `IsReady — bool — get — True if engine is initialized and ready for commands`

* **Public methods:**
  * **Signature:** `public void StartEngine()`
    * **Description:** Starts the Stockfish engine process and background reader thread
    * **Parameters:** None
    * **Returns:** void — `bridge.StartEngine()`
    * **Side effects / state changes:** Creates engine process, starts reader thread, sets up crash detection
    * **Notes:** Automatically called in Awake(), safe to call multiple times
    
  * **Signature:** `public void StopEngine()`
    * **Description:** Stops engine process and cleans up resources
    * **Parameters:** None
    * **Returns:** void — `bridge.StopEngine()`
    * **Side effects / state changes:** Terminates process, joins reader thread, cleans temp files
    * **Notes:** Called automatically in OnApplicationQuit()
    
  * **Signature:** `public IEnumerator AnalyzePositionCoroutine(string fen)`
    * **Description:** Analyzes chess position using inspector default settings
    * **Parameters:** 
      * `fen : string — FEN notation position or "startpos"`
    * **Returns:** IEnumerator — `yield return bridge.AnalyzePositionCoroutine(fen)` and `StartCoroutine(bridge.AnalyzePositionCoroutine(fen))` (~1-3 seconds based on depth, yield return new WaitForSeconds(0.1f) polling)
    * **Side effects / state changes:** Updates LastAnalysisResult, fires OnAnalysisComplete event, adds to AnalysisLog
    * **Notes:** Uses defaultDepth, evalDepth, defaultElo, defaultSkillLevel from inspector
    
  * **Signature:** `public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs = -1, int searchDepth = 12, int evaluationDepth = 15, int elo = 1500, int skillLevel = 8)`
    * **Description:** Comprehensive position analysis with full parameter control
    * **Parameters:** 
      * `fen : string — Position in FEN notation`
      * `movetimeMs : int — Time limit in milliseconds (-1 for depth-based)`
      * `searchDepth : int — Search depth for move finding`
      * `evaluationDepth : int — Depth for position evaluation`
      * `elo : int — Engine Elo limitation (1-3600)`
      * `skillLevel : int — Stockfish skill level (0-20)`
    * **Returns:** IEnumerator — `yield return bridge.AnalyzePositionCoroutine(fen, 1000, 10, 12, 1500, 5)` and `StartCoroutine(bridge.AnalyzePositionCoroutine(...))` (~0.5-10 seconds based on time/depth limits)
    * **Side effects / state changes:** Updates LastAnalysisResult with research-based evaluation, logs analysis
    * **Notes:** Research-based win probability calculation, comprehensive UCI promotion parsing
    
  * **Signature:** `public void ClearAnalysisLog()`
    * **Description:** Clears the analysis history log
    * **Parameters:** None
    * **Returns:** void — `bridge.ClearAnalysisLog()`
    * **Side effects / state changes:** Empties AnalysisLog list
    
  * **Signature:** `public string ExportAnalysisLog()`
    * **Description:** Exports analysis log as formatted string for debugging/PGN
    * **Parameters:** None
    * **Returns:** string — `string export = bridge.ExportAnalysisLog()`
    * **Notes:** Returns formatted log with timestamps and evaluation data
    
  * **Signature:** `public IEnumerator RestartEngineCoroutine()`
    * **Description:** Restarts engine after crash detection
    * **Parameters:** None
    * **Returns:** IEnumerator — `yield return bridge.RestartEngineCoroutine()` and `StartCoroutine(bridge.RestartEngineCoroutine())` (~2-3 seconds for process restart, yield return new WaitForSeconds(1f) delay)
    * **Side effects / state changes:** Stops and restarts engine process, reinitializes
    
  * **Signature:** `public bool DetectAndHandleCrash()`
    * **Description:** Checks for engine crash and logs status
    * **Parameters:** None
    * **Returns:** bool — `bool crashed = bridge.DetectAndHandleCrash()`
    * **Notes:** Returns true if crash detected, false if engine healthy
    
  * **Signature:** `public void SendCommand(string command)`
    * **Description:** Sends arbitrary UCI command to engine
    * **Parameters:** 
      * `command : string — UCI command string`
    * **Returns:** void — `bridge.SendCommand("isready")`
    * **Side effects / state changes:** Writes to engine stdin, updates lastCommandTime
    * **Notes:** Thread-safe, handles crash detection
    
  * **Signature:** `public IEnumerator InitializeEngineCoroutine()`
    * **Description:** Initializes engine with UCI protocol and waits for ready
    * **Parameters:** None
    * **Returns:** IEnumerator — `yield return bridge.InitializeEngineCoroutine()` and `StartCoroutine(bridge.InitializeEngineCoroutine())` (~1-2 seconds for initialization, yield return null polling)
    * **Side effects / state changes:** Sends UCI commands, sets IsReady when complete
    
  * **Signature:** `public void RunAllTests()`
    * **Description:** Runs comprehensive test suite for all functionality
    * **Parameters:** None
    * **Returns:** void — `bridge.RunAllTests()`
    * **Side effects / state changes:** Runs various tests, logs results to console
    * **Notes:** For debugging and validation of engine functionality

### `StockfishBridge.ChessAnalysisResult`
* **Kind:** class (does not inherit MonoBehaviour)
* **MonoBehaviour Status:** Inherits MonoBehaviour: No - Regular data class with analysis results
* **Responsibility:** Comprehensive chess analysis result with research-based evaluation and promotion data
* **Constructor(s):** Default parameterless constructor
* **Public properties / fields:**
  * `bestMove — string — get/set — Best move in UCI format or game state ("e2e4", "check-mate", "stale-mate", "ERROR: message")`
  * `sideToMove — char — get/set — Side to move ('w' or 'b') extracted from FEN`
  * `currentFen — string — get/set — Current position FEN that was analyzed`
  * `engineSideWinProbability — float — get/set — Win probability for engine side (0-1, Lichess research-based)`
  * `sideToMoveWinProbability — float — get/set — Win probability for side-to-move (0-1)`
  * `centipawnEvaluation — float — get/set — Raw centipawn evaluation from engine`
  * `isMateScore — bool — get/set — True if evaluation represents mate score`
  * `mateDistance — int — get/set — Distance to mate (+ white mates, - black mates)`
  * `isGameEnd — bool — get/set — True if position is checkmate or stalemate`
  * `isCheckmate — bool — get/set — True if position is checkmate`
  * `isStalemate — bool — get/set — True if position is stalemate`
  * `inCheck — bool — get/set — True if side to move is in check`
  * `isPromotion — bool — get/set — True if bestMove is a promotion`
  * `promotionPiece — char — get/set — Promotion piece ('q', 'r', 'b', 'n' in UCI lowercase)`
  * `promotionFrom — v2 — get/set — Source square of promotion`
  * `promotionTo — v2 — get/set — Target square of promotion`
  * `isPromotionCapture — bool — get/set — True if promotion includes capture`
  * `errorMessage — string — get/set — Detailed error message if analysis failed`
  * `rawEngineOutput — string — get/set — Full engine response for debugging`
  * `searchDepth — int — get/set — Depth used for move search`
  * `evaluationDepth — int — get/set — Depth used for position evaluation`
  * `skillLevel — int — get/set — Skill level used (-1 if disabled)`
  * `approximateElo — int — get/set — Calculated approximate Elo based on settings`
  * `analysisTimeMs — float — get/set — Time taken for analysis in milliseconds`

* **Public methods:**
  * **Signature:** `public void ParsePromotionData()`
    * **Description:** Parses UCI promotion data from bestMove with comprehensive validation
    * **Parameters:** None
    * **Returns:** void — `result.ParsePromotionData()`
    * **Side effects / state changes:** Updates promotion-related fields based on bestMove
    * **Notes:** Validates UCI format, rank transitions, side consistency
    
  * **Signature:** `public string GetPromotionDescription()`
    * **Description:** Returns human-readable promotion description
    * **Parameters:** None
    * **Returns:** string — `string desc = result.GetPromotionDescription()`
    * **Notes:** Returns empty string if not a promotion
    
  * **Signature:** `public string GetEvaluationDisplay()`
    * **Description:** Returns formatted evaluation string for UI display
    * **Parameters:** None
    * **Returns:** string — `string eval = result.GetEvaluationDisplay()`
    * **Notes:** Shows mate info or win percentages with strength indicators

### `StockfishBridge.AnalysisLogEntry`
* **Kind:** class (does not inherit MonoBehaviour)
* **MonoBehaviour Status:** Inherits MonoBehaviour: No - Simple data structure for logging
* **Responsibility:** Represents single analysis operation for debugging and PGN export
* **Constructor(s):** `AnalysisLogEntry(string fen, string bestMove, float evaluation, float analysisTime, int depth)`
* **Public properties / fields:**
  * `fen — string — get/set — Position that was analyzed`
  * `bestMove — string — get/set — Engine's best move result`
  * `evaluation — float — get/set — Position evaluation in centipawns`
  * `analysisTimeMs — float — get/set — Time taken for analysis`
  * `depth — int — get/set — Search depth used`
  * `timestamp — DateTime — get/set — When analysis was performed`

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
        // === Engine Startup and Initialization ===
        stockfishBridge.StartEngine();
        yield return stockfishBridge.InitializeEngineCoroutine();
        
        if (!stockfishBridge.IsReady)
        {
            Debug.Log("<color=red>Engine failed to initialize</color>");
            yield break;
        }
        
        // Expected output: "Engine ready for analysis"
        Debug.Log($"<color=green>Engine ready: {stockfishBridge.IsEngineRunning}</color>");
        
        // === Event Subscription ===
        stockfishBridge.OnAnalysisComplete.AddListener((result) => {
            Debug.Log($"<color=cyan>Analysis complete: {result.bestMove} | {result.GetEvaluationDisplay()}</color>");
        });
        
        stockfishBridge.OnEngineLine.AddListener((line) => {
            if (line.StartsWith("info") && line.Contains("score"))
                Debug.Log($"<color=yellow>Engine: {line}</color>");
        });
        
        // === Basic Position Analysis ===
        string startingPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        yield return stockfishBridge.AnalyzePositionCoroutine(startingPosition);
        
        StockfishBridge.ChessAnalysisResult result = stockfishBridge.LastAnalysisResult;
        
        // Expected output: "Starting position analysis: e2e4 | Engine: 52.1% | Oppo: 47.9%"
        Debug.Log($"<color=green>Starting position: {result.bestMove} | {result.GetEvaluationDisplay()}</color>");
        
        // === Advanced Analysis with Custom Parameters ===
        yield return stockfishBridge.AnalyzePositionCoroutine(
            startingPosition, 
            movetimeMs: 2000, 
            searchDepth: 15, 
            evaluationDepth: 18, 
            elo: 2000, 
            skillLevel: 12
        );
        
        result = stockfishBridge.LastAnalysisResult;
        
        // Expected output: "Advanced analysis: Nf3 | time: 2000ms | depth: 15"
        Debug.Log($"<color=green>Advanced: {result.bestMove} | time: {result.analysisTimeMs:F0}ms | depth: {result.searchDepth}</color>");
        
        // === Promotion Analysis ===
        string promotionPosition = "rnbqkbn1/pppppppr/8/8/8/8/PPPPPPP1/RNBQKBNR w KQkq - 0 1";
        yield return stockfishBridge.AnalyzePositionCoroutine(promotionPosition);
        
        result = stockfishBridge.LastAnalysisResult;
        if (result.isPromotion)
        {
            // Expected output: "Promotion detected: e7e8q - White promotes to Queen"
            Debug.Log($"<color=magenta>Promotion: {result.bestMove} - {result.GetPromotionDescription()}</color>");
        }
        
        // === Analysis Log Management ===
        var analysisLog = stockfishBridge.AnalysisLog;
        
        // Expected output: "Analysis log has 3 entries"
        Debug.Log($"<color=cyan>Analysis log: {analysisLog.Count} entries</color>");
        
        foreach (var entry in analysisLog)
        {
            Debug.Log($"<color=white>Log: {entry.bestMove} | eval: {entry.evaluation:F1}cp | time: {entry.analysisTimeMs:F0}ms</color>");
        }
        
        string exportedLog = stockfishBridge.ExportAnalysisLog();
        
        // Expected output: "Exported log: 847 characters"
        Debug.Log($"<color=green>Export size: {exportedLog.Length} chars</color>");
        
        // === Engine State Management ===
        bool isRunning = stockfishBridge.IsEngineRunning;
        bool isReady = stockfishBridge.IsReady;
        
        // Expected output: "Engine status: Running=True, Ready=True"
        Debug.Log($"<color=green>Status: Running={isRunning}, Ready={isReady}</color>");
        
        // === Manual UCI Commands ===
        stockfishBridge.SendCommand("position startpos moves e2e4");
        stockfishBridge.SendCommand("go depth 10");
        
        yield return new WaitForSeconds(2f);
        
        string rawOutput = stockfishBridge.LastRawOutput;
        
        // Expected output: "Raw output: 245 characters"
        Debug.Log($"<color=yellow>Raw output: {rawOutput.Length} chars</color>");
        
        // === Error Handling and Recovery ===
        bool crashDetected = stockfishBridge.DetectAndHandleCrash();
        if (crashDetected)
        {
            Debug.Log("<color=red>Engine crashed, restarting...</color>");
            yield return stockfishBridge.RestartEngineCoroutine();
        }
        
        // === Cleanup ===
        stockfishBridge.ClearAnalysisLog();
        stockfishBridge.StopEngine();
        
        // Expected output: "Analysis complete - log cleared, engine stopped"
        Debug.Log($"<color=green>Complete: log size={stockfishBridge.AnalysisLog.Count}, running={stockfishBridge.IsEngineRunning}</color>");
        
        yield break;
    }
}
```

## Control flow / responsibilities & high-level algorithm summary

Unity lifecycle drives engine startup (Awake), command processing (Update drains queue), cleanup (OnApplicationQuit). Background thread reads engine output, main thread processes results and fires events. Analysis flow: validate FEN → configure engine strength → send position/search commands → parse results → calculate research-based win probabilities → log and return structured result.

## Performance, allocations, and hotspots / Threading / async considerations

Background thread for engine I/O, ConcurrentQueue for thread-safe communication, string allocations in parsing, potential GC from frequent analysis logging. Main-thread-only for Unity events and coroutines.

## Security / safety / correctness concerns

External process execution, temp file creation/cleanup, thread synchronization, potential engine crashes requiring restart, UCI command validation.

## Tests, debugging & observability

Comprehensive test suite via `RunAllTests()`, detailed Debug.Log with color coding, analysis logging for debugging, raw engine output capture, crash detection with automatic recovery.

## Cross-file references

Dependencies on `ChessBoard.cs` for coordinate conversion (`AlgebraicToCoord`, `CoordToAlgebraic`), `SPACE_UTIL.v2` struct for coordinate representation, expects `sf-engine.exe` in StreamingAssets folder.

## TODO / Known limitations / Suggested improvements

<!-- No explicit TODOs mentioned in source code. Potential improvements: connection pooling for multiple engines, neural network evaluation integration, opening book support. (only if I explicitly mentioned in the prompt) -->

## Appendix

Key private helpers: `ParseAnalysisResult()` handles UCI output parsing, `ConvertCentipawnsToWinProbabilityResearch()` implements Lichess research equation, `DetectAndHandleCrash()` manages engine health monitoring, `BackgroundReaderLoop()` thread function.

## General Note: important behaviors

Major functionality includes research-based evaluation using Lichess accuracy metrics, comprehensive UCI promotion parsing with validation, engine crash detection and automatic recovery, analysis logging for debugging/PGN export, FEN-driven position analysis without game state coupling.

`checksum: a7b3c9f2 (v0.5)`