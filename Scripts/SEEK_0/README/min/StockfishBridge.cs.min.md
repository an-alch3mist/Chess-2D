# Source: `StockfishBridge.cs` — Unity Stockfish chess engine bridge with comprehensive promotion support and undo/redo functionality

## Short description
Provides non-blocking chess engine communication with full game management, comprehensive promotion move parsing from UCI format, enhanced evaluation calculation with side-to-move adjustment, and robust crash detection and recovery for Unity chess applications.

## Metadata
* **Filename:** `StockfishBridge.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependencies:** `System`, `System.Text`, `System.IO`, `System.Collections`, `System.Collections.Concurrent`, `System.Collections.Generic`, `System.Diagnostics`, `System.Threading`, `UnityEngine`, `UnityEngine.Events`, `SPACE_UTIL`
* **Public types:** `StockfishBridge (class)`, `StockfishBridge.ChessAnalysisResult (class)`, `StockfishBridge.GameHistoryEntry (class)`
* **Unity version:** Unity 2020.3 compatibility

## Public API Summary
| Type | Member | Signature | Short Purpose | OneLiner Call |
|------|---------|-----------|---------------|---------------|
| int | Field | [SerializeField] private int defaultTimeoutMs | Analysis timeout duration | N/A (Inspector) |
| bool | Field | [SerializeField] public bool enableEvaluation | Enable position evaluation | instance.enableEvaluation = true; |
| char | Property | public char HumanSide { get; set; } | Human player side control | instance.HumanSide = 'w'; |
| char | Property | public char EngineSide { get; } | Engine player side | var side = instance.EngineSide; |
| string | Property | public string LastRawOutput { get; } | Raw engine output | var output = instance.LastRawOutput; |
| StockfishBridge.ChessAnalysisResult | Property | public StockfishBridge.ChessAnalysisResult LastAnalysisResult { get; } | Last analysis result | var result = instance.LastAnalysisResult; |
| List<StockfishBridge.GameHistoryEntry> | Property | public List<StockfishBridge.GameHistoryEntry> GameHistory { get; } | Game move history | var history = instance.GameHistory; |
| int | Property | public int CurrentHistoryIndex { get; } | Current history position | var index = instance.CurrentHistoryIndex; |
| bool | Property | public bool IsEngineRunning { get; } | Engine process status | var running = instance.IsEngineRunning; |
| bool | Property | public bool IsReady { get; } | Engine initialization status | var ready = instance.IsReady; |
| UnityEvent<string> | Event | public UnityEvent<string> OnEngineLine | Engine output line received | instance.OnEngineLine.AddListener(handler); |
| UnityEvent<StockfishBridge.ChessAnalysisResult> | Event | public UnityEvent<StockfishBridge.ChessAnalysisResult> OnAnalysisComplete | Analysis completed | instance.OnAnalysisComplete.AddListener(handler); |
| UnityEvent<char> | Event | public UnityEvent<char> OnSideToMoveChanged | Side changed event | instance.OnSideToMoveChanged.AddListener(handler); |
| void | Method | public void StartEngine() | Start Stockfish process | instance.StartEngine(); |
| void | Method | public void StopEngine() | Stop engine and cleanup | instance.StopEngine(); |
| IEnumerator | Coroutine | public IEnumerator AnalyzePositionCoroutine(string fen) | Analyze chess position | yield return instance.AnalyzePositionCoroutine(fen); StartCoroutine(instance.AnalyzePositionCoroutine(fen)); |
| IEnumerator | Coroutine | public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel) | Full analysis with parameters | yield return instance.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8); StartCoroutine(instance.AnalyzePositionCoroutine(fen, 1000, 12, 15, 1500, 8)); |
| void | Method | public void SetHumanSide(char side) | Set human player side | instance.SetHumanSide('w'); |
| void | Method | public void AddMoveToHistory(string fen, ChessMove move, string notation, float evaluation) | Add move to history | instance.AddMoveToHistory(fen, move, "e4", 0.3f); |
| StockfishBridge.GameHistoryEntry | Method | public StockfishBridge.GameHistoryEntry UndoMove() | Undo last move | var entry = instance.UndoMove(); |
| StockfishBridge.GameHistoryEntry | Method | public StockfishBridge.GameHistoryEntry RedoMove() | Redo next move | var entry = instance.RedoMove(); |
| bool | Method | public bool CanUndo() | Check undo availability | var canUndo = instance.CanUndo(); |
| bool | Method | public bool CanRedo() | Check redo availability | var canRedo = instance.CanRedo(); |
| void | Method | public void ClearHistory() | Clear game history | instance.ClearHistory(); |
| string | Method | public string GetGameHistoryPGN() | Get PGN notation | var pgn = instance.GetGameHistoryPGN(); |
| IEnumerator | Coroutine | public IEnumerator RestartEngineCoroutine() | Restart after crash | yield return instance.RestartEngineCoroutine(); StartCoroutine(instance.RestartEngineCoroutine()); |
| bool | Method | public bool DetectAndHandleCrash() | Check engine health | var crashed = instance.DetectAndHandleCrash(); |
| void | Method | public void SendCommand(string command) | Send UCI command | instance.SendCommand("isready"); |
| IEnumerator | Coroutine | public IEnumerator InitializeEngineCoroutine() | Initialize and wait ready | yield return instance.InitializeEngineCoroutine(); StartCoroutine(instance.InitializeEngineCoroutine()); |
| void | Method | public void RunAllTests() | Run validation tests | instance.RunAllTests(); |

## MonoBehaviour Integration
**Note:** This class inherits MonoBehaviour.

**Unity Lifecycle Methods:**
* `Awake()` - Initializes engine, starts engine process, starts initialization coroutine, sets static debug flag
* `Update()` - Drains incoming engine lines queue, fires events, tracks request completion, monitors readiness
* `OnApplicationQuit()` - Stops engine and cleans up resources

**SerializeField Dependencies:**
* `[SerializeField] private int defaultTimeoutMs` - Inspector assignment for analysis timeout
* `[SerializeField] private bool enableDebugLogging` - Inspector toggle for debug output
* `[SerializeField] public bool enableEvaluation` - Inspector toggle for position evaluation
* `[SerializeField] private int defaultDepth` - Inspector setting for search depth
* `[SerializeField] private int evalDepth` - Inspector setting for evaluation depth
* `[SerializeField] private int defaultElo` - Inspector setting for engine Elo
* `[SerializeField] private int defaultSkillLevel` - Inspector setting for skill level
* `[SerializeField] private bool allowPlayerSideSelection` - Inspector toggle for side selection
* `[SerializeField] private char humanSide` - Inspector setting for human player side
* `[SerializeField] private int maxHistorySize` - Inspector setting for history limit

## Important Types

### `StockfishBridge.ChessAnalysisResult`
* **Kind:** class with comprehensive chess analysis data
* **Responsibility:** Enhanced analysis result with promotion and game state data
* **Constructor(s):** `public ChessAnalysisResult()` - default parameterless constructor
* **Public Properties:**
  * `bestMove` — `string` — Best move in UCI format or special values (`get/set`)
  * `sideToMove` — `char` — Current player ('w' or 'b') (`get/set`)
  * `currentFen` — `string` — Current position FEN (`get/set`)
  * `whiteWinProbability` — `float` — 0-1 probability for white winning (`get/set`)
  * `sideToMoveWinProbability` — `float` — 0-1 probability for side-to-move winning (`get/set`)
  * `centipawnEvaluation` — `float` — Raw centipawn score (`get/set`)
  * `isMateScore` — `bool` — True if evaluation is mate score (`get/set`)
  * `mateDistance` — `int` — Distance to mate (+ white, - black) (`get/set`)
  * `isGameEnd` — `bool` — True if checkmate or stalemate (`get/set`)
  * `isCheckmate` — `bool` — True if position is checkmate (`get/set`)
  * `isStalemate` — `bool` — True if position is stalemate (`get/set`)
  * `inCheck` — `bool` — True if side to move is in check (`get/set`)
  * `isPromotion` — `bool` — True if bestMove is promotion (`get/set`)
  * `promotionPiece` — `char` — Promotion piece character (`get/set`)
  * `promotionFrom` — `v2` — Source square of promotion (`get/set`)
  * `promotionTo` — `v2` — Target square of promotion (`get/set`)
  * `isPromotionCapture` — `bool` — True if promotion includes capture (`get/set`)
  * `errorMessage` — `string` — Detailed error if any (`get/set`)
  * `rawEngineOutput` — `string` — Full engine response for debugging (`get/set`)
  * `searchDepth` — `int` — Depth used for move search (`get/set`)
  * `evaluationDepth` — `int` — Depth used for position evaluation (`get/set`)
  * `skillLevel` — `int` — Skill level used (-1 if disabled) (`get/set`)
  * `approximateElo` — `int` — Approximate Elo based on settings (`get/set`)
  * `analysisTimeMs` — `float` — Time taken for analysis (`get/set`)
* **Public Methods:**
  * **`public void ParsePromotionData()`**
    * Description: Parse promotion data from UCI move string with comprehensive validation
    * Parameters: None
    * Returns: `void` + call example: `result.ParsePromotionData();`
    * Notes: Sets promotion flags and validates move format
  * **`public string GetPromotionDescription()`**
    * Description: Get human-readable promotion description
    * Parameters: None
    * Returns: `string — formatted promotion description` + call example: `var desc = result.GetPromotionDescription();`
  * **`public ChessMove ToChessMove(ChessBoard board)`**
    * Description: Convert to ChessMove object for game application
    * Parameters: `board : ChessBoard — current board state`
    * Returns: `ChessMove — move object or invalid` + call example: `var move = result.ToChessMove(board);`
  * **`public string GetEvaluationDisplay()`**
    * Description: Get evaluation as percentage string for UI display
    * Parameters: None
    * Returns: `string — formatted evaluation` + call example: `var eval = result.GetEvaluationDisplay();`

### `StockfishBridge.GameHistoryEntry`
* **Kind:** class for game history tracking with undo/redo support
* **Responsibility:** Game history entry for undo/redo functionality
* **Constructor(s):** `public GameHistoryEntry(string fen, ChessMove move, string notation, float evaluation)` - creates history entry with position data
* **Public Properties:**
  * `fen` — `string` — Position before the move (`get/set`)
  * `move` — `ChessMove` — The move that was made (`get/set`)
  * `moveNotation` — `string` — Human-readable move notation (`get/set`)
  * `evaluationScore` — `float` — Position evaluation after move (`get/set`)
  * `timestamp` — `DateTime` — When the move was made (`get/set`)

## Example Usage
**Required namespaces:**
```csharp
// using System.Collections;
// using UnityEngine;
// using GPTDeepResearch;
// using SPACE_UTIL;
```

**For MonoBehaviour classes:**
```csharp
public class ExampleUsage : MonoBehaviour 
{
    [SerializeField] private StockfishBridge stockfishBridge; // Assign in Inspector
    
    private IEnumerator StockfishBridge_Check()
    {
        // Test all major public APIs in minimal lines
        stockfishBridge.StartEngine();
        yield return stockfishBridge.InitializeEngineCoroutine();
        stockfishBridge.SetHumanSide('w');
        yield return stockfishBridge.AnalyzePositionCoroutine("startpos");
        var result = stockfishBridge.LastAnalysisResult;
        var canUndo = stockfishBridge.CanUndo();
        var pgn = stockfishBridge.GetGameHistoryPGN();
        var isRunning = stockfishBridge.IsEngineRunning;
        stockfishBridge.StopEngine();
        
        Debug.Log($"API Results: {result.bestMove}, {canUndo}, {pgn}, {isRunning}, Engine operations completed");
        yield break;
    }
}
```

## Control Flow & Responsibilities
Background thread reads engine output, main thread processes analysis requests, manages game history.

## Performance & Threading
Heavy analysis operations, background reader thread, main-thread coroutine processing, crash detection.

## Cross-file Dependencies
ChessBoard.cs, ChessMove.cs for move validation; SPACE_UTIL.v2 for coordinates.

## Major Functionality
PawnPromotion parsing, Undo/Redo history, Save/Load game state, Engine crash recovery, Elo calculation.

`checksum: a7b9c3d1 v0.3.min`