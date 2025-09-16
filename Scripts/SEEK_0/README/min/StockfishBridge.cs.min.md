# Source: `StockfishBridge.cs` — Unity Stockfish chess engine integration for position analysis and move evaluation

## Short description
A comprehensive Unity MonoBehaviour that bridges Stockfish chess engine for position analysis, move evaluation, and chess AI functionality. Provides research-based evaluation calculations, comprehensive UCI promotion handling, engine crash detection/recovery, and analysis logging without game state management.

## Metadata
* **Filename:** `StockfishBridge.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependencies:** `System`, `System.IO`, `System.Collections.Generic`, `System.Collections.Concurrent`, `System.Diagnostics`, `System.Threading`, `UnityEngine`, `UnityEngine.Events`, `SPACE_UTIL`
* **Public types:** `StockfishBridge (MonoBehaviour class)`
* **Unity version:** Compatible with Unity 2019.4+

## Public API Summary
| Type | Member | Signature | Short Purpose | OneLiner Call |
|------|---------|-----------|---------------|---------------|
| StockfishBridge | Field | public bool enableEvaluation | Toggle position evaluation | bridge.enableEvaluation = true; |
| StockfishBridge | Property | public string LastRawOutput { get; private set; } | Raw engine output | var output = bridge.LastRawOutput; |
| StockfishBridge | Property | public StockfishBridge.ChessAnalysisResult LastAnalysisResult { get; private set; } | Last analysis result | var result = bridge.LastAnalysisResult; |
| StockfishBridge | Property | public List<StockfishBridge.AnalysisLogEntry> AnalysisLog { get; private set; } | Analysis history log | var log = bridge.AnalysisLog; |
| StockfishBridge | Property | public bool IsEngineRunning { get; } | Engine process status | if (bridge.IsEngineRunning) |
| StockfishBridge | Property | public bool IsReady { get; private set; } | Engine ready status | if (bridge.IsReady) |
| StockfishBridge | Event | public UnityEvent<string> OnEngineLine | Engine output line event | bridge.OnEngineLine.AddListener(handler); |
| StockfishBridge | Event | public UnityEvent<StockfishBridge.ChessAnalysisResult> OnAnalysisComplete | Analysis completion event | bridge.OnAnalysisComplete.AddListener(handler); |
| StockfishBridge | Method | public void StartEngine() | Start Stockfish process | bridge.StartEngine(); |
| StockfishBridge | Method | public void StopEngine() | Stop engine and cleanup | bridge.StopEngine(); |
| StockfishBridge | Coroutine | public IEnumerator AnalyzePositionCoroutine(string fen) | Analyze chess position | yield return bridge.AnalyzePositionCoroutine(fen); StartCoroutine(bridge.AnalyzePositionCoroutine(fen)); |
| StockfishBridge | Coroutine | public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel) | Full analysis with settings | yield return bridge.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8); StartCoroutine(bridge.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8)); |
| StockfishBridge | Method | public void ClearAnalysisLog() | Clear analysis history | bridge.ClearAnalysisLog(); |
| StockfishBridge | Method | public string ExportAnalysisLog() | Export log as string | var exported = bridge.ExportAnalysisLog(); |
| StockfishBridge | Coroutine | public IEnumerator RestartEngineCoroutine() | Restart after crash | yield return bridge.RestartEngineCoroutine(); StartCoroutine(bridge.RestartEngineCoroutine()); |
| StockfishBridge | Method | public bool DetectAndHandleCrash() | Check engine health | bool crashed = bridge.DetectAndHandleCrash(); |
| StockfishBridge | Method | public void SendCommand(string command) | Send UCI command | bridge.SendCommand("isready"); |
| StockfishBridge | Coroutine | public IEnumerator InitializeEngineCoroutine() | Initialize and wait ready | yield return bridge.InitializeEngineCoroutine(); StartCoroutine(bridge.InitializeEngineCoroutine()); |
| StockfishBridge | Method | public void RunAllTests() | Run comprehensive tests | bridge.RunAllTests(); |
| StockfishBridge.ChessAnalysisResult | Field | public string bestMove | Best move from engine | var move = result.bestMove; |
| StockfishBridge.ChessAnalysisResult | Field | public char sideToMove | Side to move (w/b) | var side = result.sideToMove; |
| StockfishBridge.ChessAnalysisResult | Field | public string currentFen | Position FEN string | var fen = result.currentFen; |
| StockfishBridge.ChessAnalysisResult | Field | public float engineSideWinProbability | Engine side win probability | var prob = result.engineSideWinProbability; |
| StockfishBridge.ChessAnalysisResult | Field | public float sideToMoveWinProbability | Side-to-move win probability | var prob = result.sideToMoveWinProbability; |
| StockfishBridge.ChessAnalysisResult | Field | public float centipawnEvaluation | Raw centipawn score | var eval = result.centipawnEvaluation; |
| StockfishBridge.ChessAnalysisResult | Field | public bool isMateScore | Is mate evaluation | if (result.isMateScore) |
| StockfishBridge.ChessAnalysisResult | Field | public int mateDistance | Distance to mate | var mate = result.mateDistance; |
| StockfishBridge.ChessAnalysisResult | Field | public bool isGameEnd | Is checkmate/stalemate | if (result.isGameEnd) |
| StockfishBridge.ChessAnalysisResult | Field | public bool isCheckmate | Is checkmate | if (result.isCheckmate) |
| StockfishBridge.ChessAnalysisResult | Field | public bool isStalemate | Is stalemate | if (result.isStalemate) |
| StockfishBridge.ChessAnalysisResult | Field | public bool isPromotion | Is promotion move | if (result.isPromotion) |
| StockfishBridge.ChessAnalysisResult | Field | public char promotionPiece | Promotion piece (qrbn) | var piece = result.promotionPiece; |
| StockfishBridge.ChessAnalysisResult | Field | public v2 promotionFrom | Promotion source square | var from = result.promotionFrom; |
| StockfishBridge.ChessAnalysisResult | Field | public v2 promotionTo | Promotion target square | var to = result.promotionTo; |
| StockfishBridge.ChessAnalysisResult | Field | public bool isPromotionCapture | Is promotion with capture | if (result.isPromotionCapture) |
| StockfishBridge.ChessAnalysisResult | Field | public string errorMessage | Error details if any | var error = result.errorMessage; |
| StockfishBridge.ChessAnalysisResult | Field | public float analysisTimeMs | Analysis duration | var time = result.analysisTimeMs; |
| StockfishBridge.ChessAnalysisResult | Method | public void ParsePromotionData() | Parse UCI promotion move | result.ParsePromotionData(); |
| StockfishBridge.ChessAnalysisResult | Method | public string GetPromotionDescription() | Human-readable promotion | var desc = result.GetPromotionDescription(); |
| StockfishBridge.ChessAnalysisResult | Method | public string GetEvaluationDisplay() | Formatted evaluation | var display = result.GetEvaluationDisplay(); |
| StockfishBridge.AnalysisLogEntry | Field | public string fen | Position analyzed | var fen = entry.fen; |
| StockfishBridge.AnalysisLogEntry | Field | public string bestMove | Engine best move | var move = entry.bestMove; |
| StockfishBridge.AnalysisLogEntry | Field | public float evaluation | Position evaluation | var eval = entry.evaluation; |
| StockfishBridge.AnalysisLogEntry | Field | public float analysisTimeMs | Analysis duration | var time = entry.analysisTimeMs; |
| StockfishBridge.AnalysisLogEntry | Field | public int depth | Search depth used | var depth = entry.depth; |
| StockfishBridge.AnalysisLogEntry | Field | public DateTime timestamp | Analysis timestamp | var time = entry.timestamp; |

## MonoBehaviour Integration
**MonoBehaviour Classes:** StockfishBridge
**Non-MonoBehaviour Root Classes:** None (all other public types are nested classes within StockfishBridge)

**Unity Lifecycle Methods (per MonoBehaviour root class):**
* **`StockfishBridge` (MonoBehaviour):**
  * `Awake()` - Initializes analysis engine, starts engine process, begins initialization coroutine, sets static debug logging flag
  * `Update()` - Drains incoming engine lines from background thread, fires OnEngineLine events, tracks request completion, manages readiness state
  * `OnApplicationQuit()` - Stops engine process and cleans up resources

**SerializeField Dependencies (per MonoBehaviour root class):**
* **`StockfishBridge`:**
  * `[SerializeField] private int defaultTimeoutMs` - Analysis timeout in milliseconds
  * `[SerializeField] private bool enableDebugLogging` - Console debug output toggle
  * `[SerializeField] public bool enableEvaluation` - Position evaluation toggle
  * `[SerializeField] private int defaultDepth` - Default search depth
  * `[SerializeField] private int evalDepth` - Evaluation depth
  * `[SerializeField] private int defaultElo` - Default Elo rating
  * `[SerializeField] private int defaultSkillLevel` - Default skill level

## Important Types

### `StockfishBridge`
* **Kind:** MonoBehaviour class
* **Responsibility:** Manages Stockfish chess engine integration for position analysis, move evaluation, and chess AI functionality with crash recovery and comprehensive logging
* **Constructor(s):** N/A (MonoBehaviour class)
* **Public Properties:**
  * `LastRawOutput` — `string` — Raw engine output from last analysis (`get`)
  * `LastAnalysisResult` — `StockfishBridge.ChessAnalysisResult` — Complete analysis result with evaluation and move data (`get`)
  * `AnalysisLog` — `List<StockfishBridge.AnalysisLogEntry>` — History of all analyses performed (`get`)
  * `IsEngineRunning` — `bool` — Whether Stockfish process is active and responsive (`get`)
  * `IsReady` — `bool` — Whether engine is initialized and ready for commands (`get`)
* **Public Methods:**
  * **`public void StartEngine()`**
    * Description: Starts Stockfish process and initializes communication
    * Returns: `void` + call example: `bridge.StartEngine();`
    * Notes: Automatically handles executable location and platform differences
  * **`public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel)`**
    * Description: Comprehensive chess position analysis with research-based evaluation
    * Parameters: `fen : string — FEN position or "startpos"`, `movetimeMs : int — Time limit (-1 for depth)`, `searchDepth : int — Search depth`, `evaluationDepth : int — Evaluation depth`, `elo : int — Engine Elo limit`, `skillLevel : int — Skill level 0-20`
    * Returns: `IEnumerator — Coroutine for async analysis` + call example: `yield return bridge.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8);`
    * Notes: Thread-safe, handles engine crashes, fires OnAnalysisComplete event

### `StockfishBridge.ChessAnalysisResult`
* **Kind:** Nested class with comprehensive chess analysis data
* **Responsibility:** Contains move evaluation, position assessment, promotion data, and game state information from engine analysis
* **Constructor(s):** `public ChessAnalysisResult()` — default constructor initializes all fields to safe defaults
* **Public Properties:**
  * `bestMove` — `string` — Engine's best move in UCI format or error message (`get/set`)
  * `sideToMove` — `char` — Side to move extracted from FEN ('w' or 'b') (`get/set`)
  * `engineSideWinProbability` — `float` — Research-based win probability for engine side (0-1) (`get/set`)
  * `isPromotion` — `bool` — Whether best move is a pawn promotion (`get/set`)
  * `promotionPiece` — `char` — Promotion piece in UCI format (qrbn) (`get/set`)
  * `isMateScore` — `bool` — Whether evaluation is mate score vs centipawn (`get/set`)
  * `mateDistance` — `int` — Distance to mate (positive=white mates, negative=black mates) (`get/set`)
* **Public Methods:**
  * **`public void ParsePromotionData()`**
    * Description: Validates and parses UCI promotion move with comprehensive error checking
    * Returns: `void` + call example: `result.ParsePromotionData();`
    * Notes: Handles rank validation, side consistency, and UCI format compliance
  * **`public string GetEvaluationDisplay()`**
    * Description: Formats evaluation for UI display with research-based percentages
    * Returns: `string — Formatted evaluation text` + call example: `var display = result.GetEvaluationDisplay();`
    * Notes: Shows mate information or win probabilities with strength indicators

### `StockfishBridge.AnalysisLogEntry`
* **Kind:** Nested class for analysis history tracking
* **Responsibility:** Records analysis session data for debugging and PGN export functionality
* **Constructor(s):** `public AnalysisLogEntry(string fen, string bestMove, float evaluation, float analysisTime, int depth)` — creates log entry with analysis data
* **Public Properties:**
  * `fen` — `string` — Position that was analyzed (`get/set`)
  * `bestMove` — `string` — Engine's recommended move (`get/set`)
  * `evaluation` — `float` — Position evaluation in centipawns (`get/set`)
  * `analysisTimeMs` — `float` — Time taken for analysis (`get/set`)
  * `depth` — `int` — Search depth used (`get/set`)
  * `timestamp` — `DateTime` — When analysis was performed (`get/set`)

## Example Usage
**Required namespaces:**
```csharp
// using System;
// using System.Collections;
// using UnityEngine;
// using GPTDeepResearch;
// using SPACE_UTIL;
```

**For files with MonoBehaviour root classes:**
```csharp
public class ExampleUsage : MonoBehaviour 
{
    // MonoBehaviour root class references (assigned in Inspector)
    [SerializeField] private StockfishBridge stockfishBridge; 
    
    private void Start()
    {
        // Engine starts automatically in Awake, but can be manually controlled
        if (!stockfishBridge.IsEngineRunning)
        {
            stockfishBridge.StartEngine();
        }
    }
    
    private IEnumerator StockfishBridge_Check()
    {
        // Wait for engine to be ready
        yield return stockfishBridge.InitializeEngineCoroutine();
        
        // Test basic analysis
        yield return stockfishBridge.AnalyzePositionCoroutine("startpos");
        var basicResult = stockfishBridge.LastAnalysisResult;
        
        // Test advanced analysis with custom settings
        yield return stockfishBridge.AnalyzePositionCoroutine(
            "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1", 
            1000, 12, 15, 1500, 8
        );
        var advancedResult = stockfishBridge.LastAnalysisResult;
        
        // Test analysis logging
        var logCount = stockfishBridge.AnalysisLog.Count;
        var exportedLog = stockfishBridge.ExportAnalysisLog();
        stockfishBridge.ClearAnalysisLog();
        
        // Test engine management
        var engineRunning = stockfishBridge.IsEngineRunning;
        var engineReady = stockfishBridge.IsReady;
        var crashed = stockfishBridge.DetectAndHandleCrash();
        
        // Test nested class APIs
        var promotionDesc = advancedResult.GetPromotionDescription();
        var evalDisplay = advancedResult.GetEvaluationDisplay();
        advancedResult.ParsePromotionData();
        
        Debug.Log($"API Results: BasicMove:{basicResult.bestMove} AdvancedMove:{advancedResult.bestMove} LogCount:{logCount} EngineStatus:{engineRunning},{engineReady},{crashed} Promotion:{promotionDesc} Eval:{evalDisplay} Methods called, Coroutines completed");
        yield break;
    }
}
```

## Control Flow & Responsibilities
Background thread reads engine output, main thread processes analysis requests, coroutines handle async position analysis with timeout and crash recovery.

## Performance & Threading
Heavy UCI communication on background thread, main thread processes results, engine restarts cause 1s delay, analysis logging limited to 1000 entries.

## Cross-file Dependencies
References ChessBoard.AlgebraicToCoord and ChessBoard.CoordToAlgebraic for coordinate conversion, uses SPACE_UTIL.v2 for chess square representation.

## Major Functionality
Position analysis with research-based evaluation, comprehensive UCI promotion parsing and validation, engine crash detection and recovery, analysis logging for debugging and PGN export.

`checksum: A7F9B2E1 v0.7.min`