# Source: `StockfishBridge.cs` — Unity Stockfish chess engine bridge for position analysis

## Short description
Provides a Unity MonoBehaviour bridge to Stockfish chess engine focused purely on position analysis and move evaluation. Handles UCI protocol communication, engine process management, and research-based evaluation calculations without game state management.

## Metadata
* **Filename:** `StockfishBridge.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependencies:** System, System.Collections.Concurrent, System.Diagnostics, System.IO, System.Linq, System.Text, System.Threading, UnityEngine, UnityEngine.Events, SPACE_UTIL
* **Public types:** `StockfishBridge (MonoBehaviour class)`
* **Unity version:** Compatible with modern Unity (uses MonoBehaviour, Coroutines, SerializeField)

## Public API Summary
| Class | Member Type | Member | Signature | Short Purpose | OneLiner Call |
|-------|-------------|---------|-----------|---------------|---------------|
| StockfishBridge | Field | defaultTimeoutMs | [SerializeField] private int defaultTimeoutMs | Analysis timeout | N/A (private) |
| StockfishBridge | Field | enableDebugLogging | [SerializeField] private bool enableDebugLogging | Debug output control | N/A (private) |
| StockfishBridge | Field | enableEvaluation | [SerializeField] public bool enableEvaluation | Position evaluation toggle | bridge.enableEvaluation = true; |
| StockfishBridge | Field | defaultDepth | [SerializeField] private int defaultDepth | Search depth | N/A (private) |
| StockfishBridge | Field | defaultElo | [SerializeField] private int defaultElo | Engine strength | N/A (private) |
| StockfishBridge | Field | defaultSkillLevel | [SerializeField] private int defaultSkillLevel | Skill level | N/A (private) |
| StockfishBridge | Event | OnEngineLine | public UnityEvent<string> OnEngineLine | Raw engine output | bridge.OnEngineLine.AddListener(handler); |
| StockfishBridge | Event | OnAnalysisComplete | public UnityEvent<StockfishBridge.ChessAnalysisResult> OnAnalysisComplete | Analysis completion | bridge.OnAnalysisComplete.AddListener(handler); |
| StockfishBridge | Property | LastRawOutput | public string LastRawOutput { get; private set; } | Recent engine output | var output = bridge.LastRawOutput; |
| StockfishBridge | Property | LastAnalysisResult | public StockfishBridge.ChessAnalysisResult LastAnalysisResult { get; private set; } | Recent analysis data | var result = bridge.LastAnalysisResult; |
| StockfishBridge | Property | IsEngineRunning | public bool IsEngineRunning { get; } | Engine process status | var running = bridge.IsEngineRunning; |
| StockfishBridge | Property | IsReady | public bool IsReady { get; private set; } | Engine readiness | var ready = bridge.IsReady; |
| StockfishBridge | Method | StartEngine | public void StartEngine() | Start engine process | bridge.StartEngine(); |
| StockfishBridge | Method | StopEngine | public void StopEngine() | Stop engine process | bridge.StopEngine(); |
| StockfishBridge | Method | AnalyzePositionCoroutine | public IEnumerator AnalyzePositionCoroutine(string fen) | Analyze position | yield return bridge.AnalyzePositionCoroutine(fen); StartCoroutine(bridge.AnalyzePositionCoroutine(fen)); |
| StockfishBridge | Method | AnalyzePositionCoroutine | public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel) | Full analysis config | yield return bridge.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8); StartCoroutine(bridge.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8)); |
| StockfishBridge | Method | RestartEngineCoroutine | public IEnumerator RestartEngineCoroutine() | Restart after crash | yield return bridge.RestartEngineCoroutine(); StartCoroutine(bridge.RestartEngineCoroutine()); |
| StockfishBridge | Method | DetectAndHandleCrash | public bool DetectAndHandleCrash() | Check engine health | var crashed = bridge.DetectAndHandleCrash(); |
| StockfishBridge | Method | SendCommand | public void SendCommand(string command) | Send UCI command | bridge.SendCommand("isready"); |
| StockfishBridge | Method | InitializeEngineCoroutine | public IEnumerator InitializeEngineCoroutine() | Initialize engine | yield return bridge.InitializeEngineCoroutine(); StartCoroutine(bridge.InitializeEngineCoroutine()); |
| StockfishBridge | Method | RunAllTests | public void RunAllTests() | Run test suite | bridge.RunAllTests(); |
| StockfishBridge | Method | ToString | public override string ToString() | Debug representation | var info = bridge.ToString(); |
| StockfishBridge.ChessAnalysisResult | Field | bestMove | public string bestMove | Best move in UCI | result.bestMove = "e2e4"; |
| StockfishBridge.ChessAnalysisResult | Field | sideToMove | public char sideToMove | Active side | result.sideToMove = 'w'; |
| StockfishBridge.ChessAnalysisResult | Field | currentFen | public string currentFen | Position FEN | result.currentFen = "rnbqkbnr/..."; |
| StockfishBridge.ChessAnalysisResult | Field | engineSideWinProbability | public float engineSideWinProbability | White win chance | var prob = result.engineSideWinProbability; |
| StockfishBridge.ChessAnalysisResult | Field | sideToMoveWinProbability | public float sideToMoveWinProbability | STM win chance | var prob = result.sideToMoveWinProbability; |
| StockfishBridge.ChessAnalysisResult | Field | centipawnEvaluation | public float centipawnEvaluation | Raw engine score | var cp = result.centipawnEvaluation; |
| StockfishBridge.ChessAnalysisResult | Field | isMateScore | public bool isMateScore | Mate evaluation | var mate = result.isMateScore; |
| StockfishBridge.ChessAnalysisResult | Field | mateDistance | public int mateDistance | Moves to mate | var dist = result.mateDistance; |
| StockfishBridge.ChessAnalysisResult | Field | isGameEnd | public bool isGameEnd | Terminal position | var end = result.isGameEnd; |
| StockfishBridge.ChessAnalysisResult | Field | isCheckmate | public bool isCheckmate | Checkmate state | var mate = result.isCheckmate; |
| StockfishBridge.ChessAnalysisResult | Field | isStalemate | public bool isStalemate | Stalemate state | var stale = result.isStalemate; |
| StockfishBridge.ChessAnalysisResult | Field | inCheck | public bool inCheck | Check status | var check = result.inCheck; |
| StockfishBridge.ChessAnalysisResult | Field | isPromotion | public bool isPromotion | Promotion move | var promo = result.isPromotion; |
| StockfishBridge.ChessAnalysisResult | Field | promotionPiece | public char promotionPiece | Piece promoted to | var piece = result.promotionPiece; |
| StockfishBridge.ChessAnalysisResult | Field | promotionFrom | public v2 promotionFrom | Source square | var from = result.promotionFrom; |
| StockfishBridge.ChessAnalysisResult | Field | promotionTo | public v2 promotionTo | Target square | var to = result.promotionTo; |
| StockfishBridge.ChessAnalysisResult | Field | isPromotionCapture | public bool isPromotionCapture | Capturing promotion | var capture = result.isPromotionCapture; |
| StockfishBridge.ChessAnalysisResult | Field | errorMessage | public string errorMessage | Error details | var error = result.errorMessage; |
| StockfishBridge.ChessAnalysisResult | Field | rawEngineOutput | public string rawEngineOutput | Full engine response | var raw = result.rawEngineOutput; |
| StockfishBridge.ChessAnalysisResult | Field | searchDepth | public int searchDepth | Search depth used | var depth = result.searchDepth; |
| StockfishBridge.ChessAnalysisResult | Field | evaluationDepth | public int evaluationDepth | Eval depth used | var depth = result.evaluationDepth; |
| StockfishBridge.ChessAnalysisResult | Field | skillLevel | public int skillLevel | Skill level used | var skill = result.skillLevel; |
| StockfishBridge.ChessAnalysisResult | Field | approximateElo | public int approximateElo | Estimated Elo | var elo = result.approximateElo; |
| StockfishBridge.ChessAnalysisResult | Field | analysisTimeMs | public float analysisTimeMs | Analysis duration | var time = result.analysisTimeMs; |
| StockfishBridge.ChessAnalysisResult | Method | ParsePromotionData | public void ParsePromotionData() | Parse promotion info | result.ParsePromotionData(); |
| StockfishBridge.ChessAnalysisResult | Method | GetPromotionDescription | public string GetPromotionDescription() | Human-readable promo | var desc = result.GetPromotionDescription(); |
| StockfishBridge.ChessAnalysisResult | Method | GetEvaluationDisplay | public string GetEvaluationDisplay() | UI-friendly eval | var display = result.GetEvaluationDisplay(); |
| StockfishBridge.ChessAnalysisResult | Method | ToString | public override string ToString() | Debug representation | var info = result.ToString(); |

## MonoBehaviour Integration
**MonoBehaviour Classes:** StockfishBridge
**Non-MonoBehaviour Root Classes:** None (all other public types are nested classes within StockfishBridge)

**Unity Lifecycle Methods (per MonoBehaviour root class):**
* **`StockfishBridge` (MonoBehaviour):**
  * `Awake()` - Initializes engine and starts coroutine for engine setup, sets static debug flag
  * `Update()` - Drains incoming engine lines from background thread, fires events, tracks request completion
  * `OnApplicationQuit()` - Ensures clean engine shutdown and resource cleanup

**SerializeField Dependencies (per MonoBehaviour root class):**
* **`StockfishBridge`:**
  * `[SerializeField] private int defaultTimeoutMs` - Inspector assignment for analysis timeout
  * `[SerializeField] private bool enableDebugLogging` - Inspector assignment for debug output
  * `[SerializeField] public bool enableEvaluation` - Inspector assignment for evaluation toggle
  * `[SerializeField] private int defaultDepth` - Inspector assignment for search depth
  * `[SerializeField] private int evalDepth` - Inspector assignment for evaluation depth
  * `[SerializeField] private int defaultElo` - Inspector assignment for engine strength
  * `[SerializeField] private int defaultSkillLevel` - Inspector assignment for skill level

**Non-MonoBehaviour Root Classes:**
* None (all other public types are nested classes within StockfishBridge)

## Important Types

### `StockfishBridge`
* **Kind:** MonoBehaviour class inheriting from MonoBehaviour
* **Responsibility:** Manages Stockfish engine process, UCI communication, and provides chess position analysis
* **Constructor(s):** N/A (MonoBehaviour instantiated by Unity)
* **Public Properties:**
  * `LastRawOutput` — `string` — Most recent raw engine output (`get`)
  * `LastAnalysisResult` — `StockfishBridge.ChessAnalysisResult` — Most recent analysis result (`get`)
  * `IsEngineRunning` — `bool` — Engine process status with crash detection (`get`)
  * `IsReady` — `bool` — Engine readiness for commands (`get`)
* **Public Methods:**
  * **`public void StartEngine()`**
    * Description: Starts Stockfish engine process and background reader thread
    * Returns: `void` + call example: `bridge.StartEngine();`
    * Notes: Handles engine executable copying on Windows builds
  * **`public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel)`**
    * Description: Comprehensive position analysis with research-based evaluation
    * Parameters: `fen : string — FEN position or "startpos"`, `movetimeMs : int — time limit (-1 for depth)`, `searchDepth : int — search depth`, `evaluationDepth : int — eval depth`, `elo : int — strength limit`, `skillLevel : int — skill level 0-20`
    * Returns: `IEnumerator — Unity coroutine` + call example: `yield return bridge.AnalyzePositionCoroutine("startpos", -1, 12, 15, 1500, 8);`
    * Notes: Thread-safe with timeout handling and crash recovery
  * **`public bool DetectAndHandleCrash()`**
    * Description: Checks engine health and marks crash state
    * Returns: `bool — true if crash detected` + call example: `var crashed = bridge.DetectAndHandleCrash();`

### `StockfishBridge.ChessAnalysisResult`
* **Kind:** Serializable nested class with comprehensive analysis data
* **Responsibility:** Contains all analysis results including moves, evaluation, promotion data, and technical info
* **Constructor(s):** `public ChessAnalysisResult()` — Default constructor initializes fields to defaults
* **Public Properties:**
  * `bestMove` — `string` — UCI move notation or special values ("check-mate", "stale-mate", "ERROR: message") (`get/set`)
  * `engineSideWinProbability` — `float` — Win probability 0-1 using Lichess research equation (`get/set`)
  * `isPromotion` — `bool` — True if bestMove is a promotion move (`get/set`)
  * `promotionPiece` — `char` — UCI promotion piece ('q', 'r', 'b', 'n') (`get/set`)
  * `mateDistance` — `int` — Moves to mate (positive = white mates, negative = black mates) (`get/set`)
* **Public Methods:**
  * **`public void ParsePromotionData()`**
    * Description: Validates and parses UCI promotion move with comprehensive checks
    * Returns: `void` + call example: `result.ParsePromotionData();`
    * Notes: Strict UCI format validation and chess rule enforcement
  * **`public string GetEvaluationDisplay()`**
    * Description: Formats evaluation as percentage string for UI display
    * Returns: `string — formatted evaluation` + call example: `var display = result.GetEvaluationDisplay();`
  * **`public string GetPromotionDescription()`**
    * Description: Human-readable promotion description
    * Returns: `string — promotion details` + call example: `var desc = result.GetPromotionDescription();`

## Example Usage
**Required namespaces:**
```csharp
// using System.Collections;
// using UnityEngine;
// using GPTDeepResearch;
// using SPACE_UTIL;
```

**For files with MonoBehaviour root classes:**
```csharp
public class ExampleUsage : MonoBehaviour 
{
    // MonoBehaviour root class references (assigned reference in Inspector)
    [SerializeField] private StockfishBridge stockfishBridge;
    
    private void Start()
    {
        // Set up event listeners
        stockfishBridge.OnAnalysisComplete.AddListener(OnAnalysisComplete);
        
        // Start analysis
        StartCoroutine(StockfishBridge_Check());
    }
    
    private IEnumerator StockfishBridge_Check()
	{
		// Wait for engine to be ready
		yield return stockfishBridge.InitializeEngineCoroutine();

		// Test basic analysis
		yield return stockfishBridge.AnalyzePositionCoroutine("startpos");
		var basicResult = stockfishBridge.LastAnalysisResult;

		// Test advanced analysis with custom settings
		string FEN = "rnbqkbnr/8/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1";
		yield return stockfishBridge.AnalyzePositionCoroutine(
			FEN,
			movetimeMs: -1, searchDepth: 12, evaluationDepth: 12, elo: 700, skillLevel: 10);
		var advancedResult = stockfishBridge.LastAnalysisResult;

		// Test engine management
		var engineRunning = stockfishBridge.IsEngineRunning;
		var engineReady = stockfishBridge.IsReady;
		var crashed = stockfishBridge.DetectAndHandleCrash();

		// Test nested class APIs
		var promotionDesc = advancedResult.GetPromotionDescription();
		var evalDisplay = advancedResult.GetEvaluationDisplay();
		advancedResult.ParsePromotionData();

		Debug.Log($@" API Results: 
BasicMove on Chess960 :{basicResult.bestMove} 
Move for {FEN} :{advancedResult.bestMove} 
EngineStatus/Ready/Crashed:{engineRunning},{engineReady}, {crashed} 
Promotion:{promotionDesc} 
Eval:{evalDisplay} 
==== Methods called, Coroutines completed ==== ");
		yield break;
	}
    
    private void OnAnalysisComplete(StockfishBridge.ChessAnalysisResult result)
    {
        Debug.Log($"Analysis complete");
    }
}
```

## Control Flow & Responsibilities
UCI engine process management, thread-safe communication, research-based evaluation calculations, FEN analysis.

## Performance & Threading
Background reader thread, main-thread event firing, engine crash detection, timeout handling.

## Cross-file Dependencies
ChessBoard.cs for coordinate conversion, SPACE_UTIL.v2 for position data.

## Major Functionality
Position analysis, promotion parsing, research-based evaluation, engine strength configuration, crash recovery.

`checksum: 4f8a9b2c v0.8.min`