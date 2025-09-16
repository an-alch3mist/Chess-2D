# Source: `StockfishBridge.cs` — Unity Stockfish chess engine bridge for position analysis and move evaluation

Unity MonoBehaviour bridge providing non-blocking chess engine communication focused purely on position analysis without game state management.

## Short description

Implements a Unity-compatible interface to the Stockfish chess engine for position analysis, move evaluation, and chess AI integration. Provides research-based evaluation metrics, comprehensive UCI promotion parsing, engine crash detection/recovery, and separation of analysis concerns from game state management. Designed for multi-board scenarios with clean API surface.

## Metadata

* **Filename:** `StockfishBridge.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `SPACE_UTIL` (external namespace)
* **Estimated lines:** 1800+
* **Estimated chars:** ~45,000
* **Public types:** `StockfishBridge (inherits MonoBehaviour), StockfishBridge.ChessAnalysisResult (class)`
* **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
* **Dependencies:** `SPACE_UTIL.v2` (SPACE_UTIL is external namespace), `ChessBoard.cs` (inferred from method calls)

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| int | defaultTimeoutMs | [SerializeField] private int defaultTimeoutMs | Analysis timeout in milliseconds | N/A (private field) |
| bool | enableDebugLogging | [SerializeField] private bool enableDebugLogging | Enable debug output logging | N/A (private field) |
| bool | enableEvaluation | [SerializeField] public bool enableEvaluation | Enable position evaluation | bridge.enableEvaluation = true; |
| int | defaultDepth | [SerializeField] private int defaultDepth | Default search depth | N/A (private field) |
| int | evalDepth | [SerializeField] private int evalDepth | Evaluation search depth | N/A (private field) |
| int | defaultElo | [SerializeField] private int defaultElo | Default engine Elo rating | N/A (private field) |
| int | defaultSkillLevel | [SerializeField] private int defaultSkillLevel | Default skill level | N/A (private field) |
| UnityEvent<string> | OnEngineLine | public UnityEvent<string> OnEngineLine | Engine output line event | bridge.OnEngineLine.AddListener(line => Debug.Log(line)); |
| UnityEvent<StockfishBridge.ChessAnalysisResult> | OnAnalysisComplete | public UnityEvent<StockfishBridge.ChessAnalysisResult> OnAnalysisComplete | Analysis completion event | bridge.OnAnalysisComplete.AddListener(result => ProcessResult(result)); |
| string | LastRawOutput { get; } | public string LastRawOutput { get; private set; } | Last engine raw output | var output = bridge.LastRawOutput; |
| StockfishBridge.ChessAnalysisResult | LastAnalysisResult { get; } | public StockfishBridge.ChessAnalysisResult LastAnalysisResult { get; private set; } | Last analysis result | var result = bridge.LastAnalysisResult; |
| bool | IsEngineRunning { get; } | public bool IsEngineRunning { get; } | Engine process status | var running = bridge.IsEngineRunning; |
| bool | IsReady { get; } | public bool IsReady { get; private set; } | Engine ready status | var ready = bridge.IsReady; |
| void | StartEngine() | public void StartEngine() | Start Stockfish process | bridge.StartEngine(); |
| void | StopEngine() | public void StopEngine() | Stop engine and cleanup | bridge.StopEngine(); |
| IEnumerator | AnalyzePositionCoroutine(string) | public IEnumerator AnalyzePositionCoroutine(string fen) | Analyze position with defaults | yield return bridge.AnalyzePositionCoroutine(fen); / StartCoroutine(bridge.AnalyzePositionCoroutine(fen)); |
| IEnumerator | AnalyzePositionCoroutine(string, int, int, int, int, int) | public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel) | Comprehensive position analysis | yield return bridge.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8); / StartCoroutine(bridge.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8)); |
| IEnumerator | RestartEngineCoroutine() | public IEnumerator RestartEngineCoroutine() | Restart crashed engine | yield return bridge.RestartEngineCoroutine(); / StartCoroutine(bridge.RestartEngineCoroutine()); |
| bool | DetectAndHandleCrash() | public bool DetectAndHandleCrash() | Check for engine crash | var crashed = bridge.DetectAndHandleCrash(); |
| void | SendCommand(string) | public void SendCommand(string command) | Send UCI command | bridge.SendCommand("uci"); |
| IEnumerator | InitializeEngineCoroutine() | public IEnumerator InitializeEngineCoroutine() | Initialize engine to ready state | yield return bridge.InitializeEngineCoroutine(); / StartCoroutine(bridge.InitializeEngineCoroutine()); |
| void | RunAllTests() | public void RunAllTests() | Run comprehensive test suite | bridge.RunAllTests(); |

## Important types — details

### `StockfishBridge` 
* **Kind:** class inherits MonoBehaviour
* **MonoBehaviour Status:** Inherits MonoBehaviour: Yes - contains Unity lifecycle methods Awake(), Update(), OnApplicationQuit()
* **Responsibility:** Unity Stockfish engine interface for chess position analysis with crash detection and research-based evaluation
* **Constructor(s):** Default Unity MonoBehaviour constructor (implicit)
* **Public properties / fields:**
  * `enableEvaluation` — bool — Enable/disable position evaluation (get/set)
  * `LastRawOutput` — string — Last raw engine output text (get)
  * `LastAnalysisResult` — StockfishBridge.ChessAnalysisResult — Last analysis result object (get)
  * `IsEngineRunning` — bool — Engine process active status (get)
  * `IsReady` — bool — Engine ready for commands status (get)
  * `OnEngineLine` — UnityEvent<string> — Event fired on engine output line (get/set)
  * `OnAnalysisComplete` — UnityEvent<StockfishBridge.ChessAnalysisResult> — Event fired on analysis completion (get/set)

* **Public methods:**
  * **Signature:** `public void StartEngine()`
    * **Description:** Starts Stockfish engine process and background reader thread
    * **Parameters:** None
    * **Returns:** void — StockfishBridge.StartEngine()
    * **Side effects / state changes:** Creates engine process, starts reader thread, sets crash detection
    * **Notes:** Thread-safe, handles already-running state

  * **Signature:** `public void StopEngine()`
    * **Description:** Gracefully stops engine process and cleans up resources
    * **Parameters:** None  
    * **Returns:** void — StockfishBridge.StopEngine()
    * **Side effects / state changes:** Terminates process, joins reader thread, cleanup temp files
    * **Notes:** 2-second graceful shutdown timeout before force kill

  * **Signature:** `public IEnumerator AnalyzePositionCoroutine(string fen)`
    * **Description:** Analyze chess position using inspector default settings
    * **Parameters:** fen : string — FEN position or "startpos"
    * **Returns:** IEnumerator — yield return StockfishBridge.AnalyzePositionCoroutine(fen) / StartCoroutine(StockfishBridge.AnalyzePositionCoroutine(fen)) — (~2-5 seconds typical, based on depth/timeout via WaitForSeconds equivalents)
    * **Side effects / state changes:** Updates LastAnalysisResult, fires OnAnalysisComplete event
    * **Notes:** Unity coroutine, handles engine crashes, FEN validation

  * **Signature:** `public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel)`
    * **Description:** Comprehensive position analysis with custom engine settings
    * **Parameters:** 
      * fen : string — FEN position string
      * movetimeMs : int — Move time in milliseconds (-1 for depth)
      * searchDepth : int — Search depth (12 default)
      * evaluationDepth : int — Evaluation depth (15 default)
      * elo : int — Engine Elo limit (1500 default)
      * skillLevel : int — Skill level 0-20 (8 default)
    * **Returns:** IEnumerator — yield return StockfishBridge.AnalyzePositionCoroutine(...) / StartCoroutine(StockfishBridge.AnalyzePositionCoroutine(...)) — (~1-10 seconds based on movetimeMs/depth via WaitForSeconds)
    * **Side effects / state changes:** Configures engine strength, updates analysis result, manages UCI communication
    * **Notes:** Research-based evaluation calculation, comprehensive UCI validation

  * **Signature:** `public IEnumerator RestartEngineCoroutine()`
    * **Description:** Restart engine after crash detection with initialization
    * **Parameters:** None
    * **Returns:** IEnumerator — yield return StockfishBridge.RestartEngineCoroutine() / StartCoroutine(StockfishBridge.RestartEngineCoroutine()) — (~2 seconds via WaitForSeconds(1f) + init time)
    * **Side effects / state changes:** Stops current engine, creates new process, reinitializes
    * **Notes:** Automatic crash recovery mechanism

  * **Signature:** `public bool DetectAndHandleCrash()`
    * **Description:** Check engine process health and detect crashes
    * **Parameters:** None
    * **Returns:** bool — var crashed = StockfishBridge.DetectAndHandleCrash()
    * **Side effects / state changes:** Sets engineCrashed flag if crash detected
    * **Notes:** Thread-safe, 30-second unresponsive timeout detection

  * **Signature:** `public void SendCommand(string command)`
    * **Description:** Send arbitrary UCI command to engine with crash detection
    * **Parameters:** command : string — UCI command string
    * **Returns:** void — StockfishBridge.SendCommand("uci")
    * **Throws:** Handles ObjectDisposedException, InvalidOperationException, IOException
    * **Side effects / state changes:** Writes to engine stdin, updates lastCommandTime
    * **Notes:** Thread-safe, automatic crash detection on send failure

  * **Signature:** `public IEnumerator InitializeEngineCoroutine()`
    * **Description:** Initialize engine with UCI protocol and wait for ready state
    * **Parameters:** None
    * **Returns:** IEnumerator — yield return StockfishBridge.InitializeEngineCoroutine() / StartCoroutine(StockfishBridge.InitializeEngineCoroutine()) — (~1-10 seconds until readyok via null yields)
    * **Side effects / state changes:** Sets IsReady flag, sends uci/isready commands
    * **Notes:** 10-second timeout for initialization

  * **Signature:** `public void RunAllTests()`
    * **Description:** Execute comprehensive test suite for all engine functionality
    * **Parameters:** None
    * **Returns:** void — StockfishBridge.RunAllTests()
    * **Side effects / state changes:** Logs test results, may restart engine
    * **Notes:** Debug/validation tool for development

**Unity Lifecycle Methods:**
* `Awake()` - Called on script load. Initializes engine process (calls StartEngine()), starts initialization coroutine (StartCoroutine(InitializeEngineOnAwake())), and sets static logging flag (enableDebugLogging_static = this.enableDebugLogging).
* `Update()` - Called every frame. Drains incoming engine lines from thread-safe queue (incomingLines.TryDequeue()), fires OnEngineLine events, and tracks analysis completion state for current requests.
* `OnApplicationQuit()` - Called on Unity application exit. Stops engine gracefully (calls StopEngine()) to clean up processes and temporary files.

### `StockfishBridge.ChessAnalysisResult`
* **Kind:** class does not inherit MonoBehaviour  
* **MonoBehaviour Status:** Inherits MonoBehaviour: No - standard C# serializable data class
* **Responsibility:** Contains comprehensive chess analysis results with research-based evaluation and UCI promotion data
* **Constructor(s):** Default parameterless constructor (implicit)
* **Public properties / fields:**
  * `bestMove` — string — Best move in UCI format or game state (get/set)
  * `sideToMove` — char — Side to move from FEN ('w'/'b') (get/set)  
  * `currentFen` — string — Current position FEN string (get/set)
  * `engineSideWinProbability` — float — Win probability for engine side (0-1) (get/set)
  * `sideToMoveWinProbability` — float — Win probability for side to move (0-1) (get/set)
  * `centipawnEvaluation` — float — Raw centipawn evaluation score (get/set)
  * `isMateScore` — bool — True if evaluation is mate score (get/set)
  * `mateDistance` — int — Distance to mate (+ white, - black) (get/set)
  * `isGameEnd` — bool — True if checkmate or stalemate (get/set)
  * `isCheckmate` — bool — True if position is checkmate (get/set)
  * `isStalemate` — bool — True if position is stalemate (get/set)
  * `inCheck` — bool — True if side to move in check (get/set)
  * `isPromotion` — bool — True if bestMove is promotion (get/set)
  * `promotionPiece` — char — Promotion piece UCI format (get/set)
  * `promotionFrom` — v2 — Promotion source square coordinates (get/set)
  * `promotionTo` — v2 — Promotion target square coordinates (get/set)
  * `isPromotionCapture` — bool — True if promotion includes capture (get/set)
  * `errorMessage` — string — Detailed error message if any (get/set)
  * `rawEngineOutput` — string — Full engine response for debugging (get/set)
  * `searchDepth` — int — Depth used for move search (get/set)
  * `evaluationDepth` — int — Depth used for position evaluation (get/set)
  * `skillLevel` — int — Skill level used (-1 if disabled) (get/set)
  * `approximateElo` — int — Approximate Elo based on settings (get/set)
  * `analysisTimeMs` — float — Time taken for analysis in ms (get/set)

* **Public methods:**
  * **Signature:** `public void ParsePromotionData()`
    * **Description:** Parse promotion data from UCI move string with enhanced validation
    * **Parameters:** None
    * **Returns:** void — StockfishBridge.ChessAnalysisResult.ParsePromotionData()
    * **Side effects / state changes:** Sets promotion-related fields based on bestMove UCI format
    * **Notes:** Validates UCI 5-character promotion format, rank transitions, side consistency

  * **Signature:** `public string GetPromotionDescription()`
    * **Description:** Get human-readable promotion description
    * **Parameters:** None
    * **Returns:** string — var desc = StockfishBridge.ChessAnalysisResult.GetPromotionDescription()
    * **Notes:** Returns empty string if not promotion, includes piece name and capture info

  * **Signature:** `public string GetEvaluationDisplay()`
    * **Description:** Get evaluation as percentage string for UI display with research formatting
    * **Parameters:** None
    * **Returns:** string — var display = StockfishBridge.ChessAnalysisResult.GetEvaluationDisplay()
    * **Notes:** Shows mate-in-X for mate scores, win percentages for normal evaluation

  * **Signature:** `public override string ToString()`
    * **Description:** Comprehensive string representation of analysis result
    * **Parameters:** None
    * **Returns:** string — var text = StockfishBridge.ChessAnalysisResult.ToString()
    * **Notes:** Multi-line formatted output with all analysis data

## Example Usage Coverage Requirements

### MonoBehaviour Integration:

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
        if (!stockfishBridge.IsEngineRunning)
        {
            stockfishBridge.StartEngine();
            yield return stockfishBridge.InitializeEngineCoroutine();
        }
        
        // Wait for engine ready
        while (!stockfishBridge.IsReady)
        {
            yield return null;
        }
        Debug.Log("<color=green>Engine initialized and ready</color>");
        
        // === Event Subscription ===
        stockfishBridge.OnEngineLine.AddListener(line => Debug.Log($"Engine: {line}"));
        stockfishBridge.OnAnalysisComplete.AddListener(result => {
            Debug.Log($"<color=cyan>Analysis complete: {result.bestMove}</color>");
        });
        
        // === Basic Position Analysis ===
        string startPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        yield return stockfishBridge.AnalyzePositionCoroutine(startPosition);
        
        var result = stockfishBridge.LastAnalysisResult;
        Debug.Log($"<color=green>Best move: {result.bestMove}</color>");
        Debug.Log($"<color=green>Evaluation: {result.GetEvaluationDisplay()}</color>");
        
        // === Advanced Analysis with Custom Settings ===
        yield return stockfishBridge.AnalyzePositionCoroutine(
            startPosition, 
            movetimeMs: 2000,    // 2 second analysis
            searchDepth: 15,     // Deep search
            evaluationDepth: 18, // Deeper evaluation
            elo: 2000,          // Strong engine
            skillLevel: 15      // High skill
        );
        
        var advancedResult = stockfishBridge.LastAnalysisResult;
        Debug.Log($"<color=green>Advanced analysis: {advancedResult.bestMove}</color>");
        Debug.Log($"<color=green>Win probability: {advancedResult.sideToMoveWinProbability:P1}</color>");
        
        // === Promotion Handling ===
        string promotionPosition = "4k3/4P3/8/8/8/8/8/4K3 w - - 0 1";
        yield return stockfishBridge.AnalyzePositionCoroutine(promotionPosition);
        
        var promoResult = stockfishBridge.LastAnalysisResult;
        if (promoResult.isPromotion)
        {
            Debug.Log($"<color=cyan>Promotion detected: {promoResult.GetPromotionDescription()}</color>");
            Debug.Log($"<color=cyan>Promotion piece: {promoResult.promotionPiece}</color>");
            Debug.Log($"<color=cyan>From: {promoResult.promotionFrom} To: {promoResult.promotionTo}</color>");
        }
        
        // === Nested Class Instantiation ===
        var customResult = new StockfishBridge.ChessAnalysisResult();
        customResult.bestMove = "e7e8q";
        customResult.sideToMove = 'w';
        customResult.ParsePromotionData();
        Debug.Log($"<color=green>Custom result: {customResult.GetPromotionDescription()}</color>");
        
        // === Engine Management ===
        var engineStatus = stockfishBridge.IsEngineRunning;
        Debug.Log($"<color=green>Engine running: {engineStatus}</color>");
        
        // === Crash Detection and Recovery ===
        if (stockfishBridge.DetectAndHandleCrash())
        {
            Debug.Log("<color=yellow>Engine crash detected, restarting...</color>");
            yield return stockfishBridge.RestartEngineCoroutine();
        }
        
        // === Direct UCI Commands ===
        stockfishBridge.SendCommand("position startpos");
        stockfishBridge.SendCommand("go depth 10");
        
        // === Testing and Validation ===
        stockfishBridge.RunAllTests();
        
        // Expected outputs:
        // "Engine initialized and ready"  
        // "Analysis complete: e2e4"
        // "Best move: e2e4"
        // "Evaluation: Engine: 52.3% | Oppo: 47.7%"
        // "Advanced analysis: Nf3"
        // "Win probability: 51.2%"
        // "Promotion detected: White promotes to Queen (e7-e8)"
        // "Engine running: True"
        
        yield break;
    }
}
```

## Control flow / responsibilities & high-level algorithm summary

Main thread handles Unity lifecycle and event firing; background reader thread processes engine stdout; UCI protocol communication with comprehensive analysis pipeline including FEN validation, engine configuration, move parsing, research-based evaluation conversion.

## Performance, allocations, and hotspots / Threading considerations 

Heavy string parsing allocations in analysis; background thread for engine I/O; main thread event processing.

## Security / safety / correctness concerns

Process management with crash detection; thread synchronization with locks; temp file cleanup on Windows builds.

## Tests, debugging & observability

Comprehensive built-in test suite via RunAllTests(); extensive debug logging with color coding; engine output monitoring through events.

## Cross-file references

Dependencies: `ChessBoard.cs` (AlgebraicToCoord, CoordToAlgebraic methods), `SPACE_UTIL.v2` (coordinate structure).

## General Note: important behaviors

Major functionalities: Position analysis with research-based win probability calculation, UCI promotion parsing with comprehensive validation, engine crash detection and recovery, FEN-based side management, comprehensive test suite for validation.

`checksum: a7f3b2c1 v0.5`