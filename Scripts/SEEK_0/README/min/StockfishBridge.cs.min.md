# Source: `StockfishBridge.cs` — Unity Stockfish chess engine bridge with research-based evaluation

## Short description
Provides comprehensive Unity integration for the Stockfish chess engine with non-blocking analysis, FEN-based position management, and research-based evaluation calculations. Implements UCI protocol communication, game history management, and enhanced promotion handling for chess applications.

## Metadata
* **Filename:** `StockfishBridge.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependencies:** System, System.Collections, System.Collections.Concurrent, System.Diagnostics, System.IO, System.Linq, System.Text, System.Threading, UnityEngine, UnityEngine.Events, SPACE_UTIL
* **Public types:** `StockfishBridge (MonoBehaviour class)`
* **Unity version:** Unity 2020.3+ (inferred from UnityEngine.Events usage)

## MonoBehaviour Integration
**MonoBehaviour Classes:** `StockfishBridge`
**Non-MonoBehaviour Root Classes:** None (all other public types are nested classes within StockfishBridge)

**Unity Lifecycle Methods (per MonoBehaviour root class):**
* **`StockfishBridge` (MonoBehaviour):**
  * `Awake()` - Initializes engine process, starts InitializeEngineOnAwake coroutine, sets static debug logging flag
  * `Update()` - Processes incoming engine lines queue, fires events, tracks request completion and engine readiness
  * `OnApplicationQuit()` - Cleanly stops engine process and resources

**SerializeField Dependencies (per MonoBehaviour root class):**
* **`StockfishBridge`:**
  * `[SerializeField] private int defaultTimeoutMs` - Inspector assignment for analysis timeout
  * `[SerializeField] private bool enableDebugLogging` - Inspector toggle for debug output
  * `[SerializeField] public bool enableEvaluation` - Inspector toggle for position evaluation
  * `[SerializeField] private int defaultDepth` - Inspector assignment for search depth
  * `[SerializeField] private int evalDepth` - Inspector assignment for evaluation depth
  * `[SerializeField] private int defaultElo` - Inspector assignment for engine Elo
  * `[SerializeField] private int defaultSkillLevel` - Inspector assignment for skill level
  * `[SerializeField] private int maxHistorySize` - Inspector assignment for history limit

**Non-MonoBehaviour Root Classes:**
* None (all other public types are nested classes within StockfishBridge)

## Public API Summary
| Type | Member | Signature | Short Purpose | OneLiner Call |
|------|---------|-----------|---------------|---------------|
| StockfishBridge | Property | public string LastRawOutput { get; private set; } | Last engine output | var output = bridge.LastRawOutput; |
| StockfishBridge | Property | public StockfishBridge.ChessAnalysisResult LastAnalysisResult { get; private set; } | Last analysis result | var result = bridge.LastAnalysisResult; |
| StockfishBridge | Property | public List<StockfishBridge.GameHistoryEntry> GameHistory { get; private set; } | Game move history | var history = bridge.GameHistory; |
| StockfishBridge | Property | public int CurrentHistoryIndex { get; private set; } | Current history position | var index = bridge.CurrentHistoryIndex; |
| StockfishBridge | Property | public bool IsEngineRunning { get; } | Engine process status | var running = bridge.IsEngineRunning; |
| StockfishBridge | Property | public bool IsReady { get; private set; } | Engine ready status | var ready = bridge.IsReady; |
| StockfishBridge | Event | public UnityEvent<string> OnEngineLine | Engine output line event | bridge.OnEngineLine.AddListener(handler); |
| StockfishBridge | Event | public UnityEvent<StockfishBridge.ChessAnalysisResult> OnAnalysisComplete | Analysis completion event | bridge.OnAnalysisComplete.AddListener(handler); |
| StockfishBridge | Method | public void StartEngine() | Start engine process | bridge.StartEngine(); |
| StockfishBridge | Method | public void StopEngine() | Stop engine process | bridge.StopEngine(); |
| StockfishBridge | Coroutine | public IEnumerator AnalyzePositionCoroutine(string fen) | Analyze position with defaults | yield return bridge.AnalyzePositionCoroutine("startpos"); StartCoroutine(bridge.AnalyzePositionCoroutine("startpos")); |
| StockfishBridge | Coroutine | public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel) | Full position analysis | yield return bridge.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8); StartCoroutine(bridge.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8)); |
| StockfishBridge | Method | public void AddMoveToHistory(string fen, ChessMove move, string notation, float evaluation) | Add move to history | bridge.AddMoveToHistory(fen, move, "e4", 0.3f); |
| StockfishBridge | Method | public StockfishBridge.GameHistoryEntry UndoMove() | Undo last move | var entry = bridge.UndoMove(); |
| StockfishBridge | Method | public StockfishBridge.GameHistoryEntry RedoMove() | Redo next move | var entry = bridge.RedoMove(); |
| StockfishBridge | Method | public bool CanUndo() | Check undo availability | var canUndo = bridge.CanUndo(); |
| StockfishBridge | Method | public bool CanRedo() | Check redo availability | var canRedo = bridge.CanRedo(); |
| StockfishBridge | Method | public void ClearHistory() | Clear game history | bridge.ClearHistory(); |
| StockfishBridge | Method | public string GetGameHistoryPGN() | Get history as PGN | var pgn = bridge.GetGameHistoryPGN(); |
| StockfishBridge | Coroutine | public IEnumerator RestartEngineCoroutine() | Restart crashed engine | yield return bridge.RestartEngineCoroutine(); StartCoroutine(bridge.RestartEngineCoroutine()); |
| StockfishBridge | Method | public bool DetectAndHandleCrash() | Detect engine crash | var crashed = bridge.DetectAndHandleCrash(); |
| StockfishBridge | Method | public void SendCommand(string command) | Send UCI command | bridge.SendCommand("isready"); |
| StockfishBridge | Coroutine | public IEnumerator InitializeEngineCoroutine() | Initialize engine | yield return bridge.InitializeEngineCoroutine(); StartCoroutine(bridge.InitializeEngineCoroutine()); |
| StockfishBridge | Method | public void RunAllTests() | Run comprehensive tests | bridge.RunAllTests(); |
| StockfishBridge.ChessAnalysisResult | Field | public string bestMove | Best move in UCI format | var move = result.bestMove; |
| StockfishBridge.ChessAnalysisResult | Field | public char sideToMove | Side to move from FEN | var side = result.sideToMove; |
| StockfishBridge.ChessAnalysisResult | Field | public string currentFen | Current position FEN | var fen = result.currentFen; |
| StockfishBridge.ChessAnalysisResult | Field | public float engineSideWinProbability | Engine side win probability | var prob = result.engineSideWinProbability; |
| StockfishBridge.ChessAnalysisResult | Field | public float sideToMoveWinProbability | Side-to-move win probability | var prob = result.sideToMoveWinProbability; |
| StockfishBridge.ChessAnalysisResult | Field | public float centipawnEvaluation | Raw centipawn score | var score = result.centipawnEvaluation; |
| StockfishBridge.ChessAnalysisResult | Field | public bool isMateScore | True if mate evaluation | var isMate = result.isMateScore; |
| StockfishBridge.ChessAnalysisResult | Field | public int mateDistance | Distance to mate | var distance = result.mateDistance; |
| StockfishBridge.ChessAnalysisResult | Field | public bool isGameEnd | True if game ended | var ended = result.isGameEnd; |
| StockfishBridge.ChessAnalysisResult | Field | public bool isCheckmate | True if checkmate | var checkmate = result.isCheckmate; |
| StockfishBridge.ChessAnalysisResult | Field | public bool isStalemate | True if stalemate | var stalemate = result.isStalemate; |
| StockfishBridge.ChessAnalysisResult | Field | public bool inCheck | True if in check | var check = result.inCheck; |
| StockfishBridge.ChessAnalysisResult | Field | public bool isPromotion | True if promotion move | var promotion = result.isPromotion; |
| StockfishBridge.ChessAnalysisResult | Field | public char promotionPiece | Promotion piece character | var piece = result.promotionPiece; |
| StockfishBridge.ChessAnalysisResult | Field | public v2 promotionFrom | Promotion source square | var from = result.promotionFrom; |
| StockfishBridge.ChessAnalysisResult | Field | public v2 promotionTo | Promotion target square | var to = result.promotionTo; |
| StockfishBridge.ChessAnalysisResult | Field | public bool isPromotionCapture | True if promotion with capture | var capture = result.isPromotionCapture; |
| StockfishBridge.ChessAnalysisResult | Method | public void ParsePromotionData() | Parse UCI promotion data | result.ParsePromotionData(); |
| StockfishBridge.ChessAnalysisResult | Method | public string GetPromotionDescription() | Get human-readable promotion | var desc = result.GetPromotionDescription(); |
| StockfishBridge.ChessAnalysisResult | Method | public ChessMove ToChessMove(ChessBoard board) | Convert to ChessMove object | var move = result.ToChessMove(board); |
| StockfishBridge.ChessAnalysisResult | Method | public string GetEvaluationDisplay() | Get evaluation display string | var display = result.GetEvaluationDisplay(); |
| StockfishBridge.GameHistoryEntry | Constructor | public GameHistoryEntry(string fen, ChessMove move, string notation, float evaluation) | Create history entry | var entry = new StockfishBridge.GameHistoryEntry(fen, move, "e4", 0.3f); |
| StockfishBridge.GameHistoryEntry | Field | public string fen | Position before move | var fen = entry.fen; |
| StockfishBridge.GameHistoryEntry | Field | public ChessMove move | The move made | var move = entry.move; |
| StockfishBridge.GameHistoryEntry | Field | public string moveNotation | Human-readable notation | var notation = entry.moveNotation; |
| StockfishBridge.GameHistoryEntry | Field | public float evaluationScore | Position evaluation | var score = entry.evaluationScore; |
| StockfishBridge.GameHistoryEntry | Field | public DateTime timestamp | Move timestamp | var time = entry.timestamp; |

## Important Types

### `StockfishBridge`
* **Kind:** MonoBehaviour class
* **Responsibility:** Manages Stockfish chess engine communication and provides comprehensive chess analysis functionality with research-based evaluation
* **Constructor(s):** N/A (MonoBehaviour class)
* **Public Properties:**
  * `LastRawOutput` — `string` — Last raw engine output (`get`)
  * `LastAnalysisResult` — `StockfishBridge.ChessAnalysisResult` — Most recent analysis result (`get`)
  * `GameHistory` — `List<StockfishBridge.GameHistoryEntry>` — Game move history list (`get`)
  * `CurrentHistoryIndex` — `int` — Current position in history (`get`)
  * `IsEngineRunning` — `bool` — Engine process status (`get`)
  * `IsReady` — `bool` — Engine ready for commands (`get`)
* **Public Methods:**
  * **`public void StartEngine()`**
    * Description: Starts the Stockfish engine process and initializes communication
    * Returns: `void` + call example: `bridge.StartEngine();`
  * **`public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel)`**
    * Description: Comprehensive position analysis with research-based evaluation and FEN-driven side management
    * Parameters: `fen : string — Position in FEN notation or "startpos"`, `movetimeMs : int — Time limit in milliseconds (-1 for depth-based)`, `searchDepth : int — Search depth`, `evaluationDepth : int — Evaluation depth`, `elo : int — Engine Elo rating`, `skillLevel : int — Skill level 0-20`
    * Returns: `IEnumerator — Coroutine for async analysis` + call example: `yield return bridge.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8);`
    * Notes: Non-blocking coroutine that fires OnAnalysisComplete event upon completion

### `StockfishBridge.ChessAnalysisResult`
* **Kind:** class with comprehensive analysis data
* **Responsibility:** Contains complete chess position analysis results with research-based evaluation and enhanced promotion support
* **Constructor(s):** `public ChessAnalysisResult()` + notes
* **Public Properties:**
  * `bestMove` — `string` — Best move in UCI format, "check-mate", "stale-mate", or error message (`get/set`)
  * `sideToMove` — `char` — Side to move from FEN ('w' or 'b') (`get/set`)
  * `engineSideWinProbability` — `float` — Research-based win probability for engine side (`get/set`)
  * `sideToMoveWinProbability` — `float` — Win probability for side to move (`get/set`)
  * `isPromotion` — `bool` — True if best move is a promotion (`get/set`)
  * `promotionPiece` — `char` — UCI promotion piece character (`get/set`)
* **Public Methods:**
  * **`public void ParsePromotionData()`**
    * Description: Parses UCI promotion data with enhanced validation
    * Returns: `void` + call example: `result.ParsePromotionData();`
  * **`public string GetEvaluationDisplay()`**
    * Description: Returns formatted evaluation string for UI display
    * Returns: `string — Formatted evaluation text` + call example: `var display = result.GetEvaluationDisplay();`
  * **`public ChessMove ToChessMove(ChessBoard board)`**
    * Description: Converts analysis result to ChessMove object for game application
    * Parameters: `board : ChessBoard — Current chess board state`
    * Returns: `ChessMove — Converted move object` + call example: `var move = result.ToChessMove(board);`

### `StockfishBridge.GameHistoryEntry`
* **Kind:** class for game history tracking
* **Responsibility:** Stores individual move history data for undo/redo functionality
* **Constructor(s):** `public GameHistoryEntry(string fen, ChessMove move, string notation, float evaluation)` + notes
* **Public Properties:**
  * `fen` — `string` — Position before the move (`get/set`)
  * `move` — `ChessMove` — The move that was made (`get/set`)
  * `moveNotation` — `string` — Human-readable move notation (`get/set`)
  * `evaluationScore` — `float` — Position evaluation after move (`get/set`)
  * `timestamp` — `DateTime` — When the move was made (`get/set`)

## Example Usage
**Required namespaces:**
```csharp
using System;
using System.Collections;
using UnityEngine;
using GPTDeepResearch;
using SPACE_UTIL;
```

**For files with MonoBehaviour root classes:**
```csharp
public class ExampleUsage : MonoBehaviour 
{
    // MonoBehaviour root class references (assigned in Inspector)
    [SerializeField] private StockfishBridge stockfishBridge; 
    
    private void Start()
    {
        // Set up event listeners
        stockfishBridge.OnAnalysisComplete.AddListener(OnAnalysisComplete);
        
        // Initialize engine
        stockfishBridge.StartEngine();
        StartCoroutine(stockfishBridge.InitializeEngineCoroutine());
    }
    
    private IEnumerator StockfishBridge_Check() 
    {
        // Test analysis
        yield return stockfishBridge.AnalyzePositionCoroutine("startpos");
        var analysisResult = stockfishBridge.LastAnalysisResult;
        
        // Test game history
        var testMove = new ChessMove(); // Assuming ChessMove exists
        stockfishBridge.AddMoveToHistory("startpos", testMove, "e4", 0.3f);
        var canUndo = stockfishBridge.CanUndo();
        var undoEntry = stockfishBridge.UndoMove();
        var canRedo = stockfishBridge.CanRedo();
        
        // Test engine management
        var isRunning = stockfishBridge.IsEngineRunning;
        var isReady = stockfishBridge.IsReady;
        stockfishBridge.SendCommand("isready");
        
        // Test nested class functionality
        analysisResult.ParsePromotionData();
        var evalDisplay = analysisResult.GetEvaluationDisplay();
        var promotionDesc = analysisResult.GetPromotionDescription();
        
        var historyEntry = new StockfishBridge.GameHistoryEntry("startpos", testMove, "e4", 0.3f);
        var historyPgn = stockfishBridge.GetGameHistoryPGN();
        
        Debug.Log($"API Results: Analysis:{analysisResult.bestMove} History:{canUndo},{canRedo} Engine:{isRunning},{isReady} Eval:{evalDisplay} Promotion:{promotionDesc} PGN:{historyPgn} Methods called, Coroutine completed");
        yield break;
    }
    
    private void OnAnalysisComplete(StockfishBridge.ChessAnalysisResult result)
    {
        Debug.Log($"Analysis completed: {result.bestMove} with evaluation {result.GetEvaluationDisplay()}");
    }
}
```

## Control Flow & Responsibilities
Engine process management, UCI communication, FEN-based analysis, research-based evaluation calculations, game history tracking

## Performance & Threading  
Background thread for engine I/O, main thread event processing

## Cross-file Dependencies
ChessBoard.cs for coordinate conversion, ChessMove.cs for move representation, v2 from SPACE_UTIL

## Major Functionality
Position analysis, promotion parsing, undo/redo, crash detection, research-based evaluation

`checksum: A7F9E2B1 v0.7.min`