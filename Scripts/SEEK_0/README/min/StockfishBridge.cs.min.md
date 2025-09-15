# Source: `StockfishBridge.cs` — Unity Stockfish engine bridge with comprehensive chess analysis and promotion support

## Short description
A Unity MonoBehaviour that provides non-blocking communication with the Stockfish chess engine. Implements comprehensive position analysis, full UCI promotion support, game history management with undo/redo functionality, and robust engine crash detection/recovery.

## Metadata
* **Filename:** `StockfishBridge.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependencies:** `System`, `System.Collections.Concurrent`, `System.Diagnostics`, `UnityEngine`, `SPACE_UTIL`
* **Public types:** `StockfishBridge (MonoBehaviour class)`
* **Unity version:** Unity 2020.3+ compatible

## Public API Summary
| Type | Member | Signature | Short Purpose | OneLiner Call |
|------|---------|-----------|---------------|---------------|
| StockfishBridge | Constructor | N/A (MonoBehaviour) | Unity managed lifecycle | Component assigned in Inspector |
| void | Method | public void StartEngine() | Start Stockfish process | bridge.StartEngine(); |
| void | Method | public void StopEngine() | Stop and cleanup engine | bridge.StopEngine(); |
| IEnumerator | Coroutine | public IEnumerator AnalyzePositionCoroutine(string fen) | Analyze chess position with defaults | yield return bridge.AnalyzePositionCoroutine("startpos"); StartCoroutine(bridge.AnalyzePositionCoroutine("startpos")); |
| IEnumerator | Coroutine | public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel) | Comprehensive position analysis | yield return bridge.AnalyzePositionCoroutine("rnbq...", 5000, 15, 18, 2000, 10); StartCoroutine(bridge.AnalyzePositionCoroutine("rnbq...", 5000, 15, 18, 2000, 10)); |
| void | Method | public void SetHumanSide(char side) | Set human player side | bridge.SetHumanSide('w'); |
| void | Method | public void AddMoveToHistory(string fen, ChessMove move, string notation, float evaluation) | Add move to game history | bridge.AddMoveToHistory("rnbq...", move, "e4", 0.3f); |
| StockfishBridge.GameHistoryEntry | Method | public StockfishBridge.GameHistoryEntry UndoMove() | Undo last move | var entry = bridge.UndoMove(); |
| StockfishBridge.GameHistoryEntry | Method | public StockfishBridge.GameHistoryEntry RedoMove() | Redo next move | var entry = bridge.RedoMove(); |
| bool | Method | public bool CanUndo() | Check if undo possible | bool canUndo = bridge.CanUndo(); |
| bool | Method | public bool CanRedo() | Check if redo possible | bool canRedo = bridge.CanRedo(); |
| void | Method | public void ClearHistory() | Clear game history | bridge.ClearHistory(); |
| string | Method | public string GetGameHistoryPGN() | Get history as PGN notation | string pgn = bridge.GetGameHistoryPGN(); |
| IEnumerator | Coroutine | public IEnumerator RestartEngineCoroutine() | Restart crashed engine | yield return bridge.RestartEngineCoroutine(); StartCoroutine(bridge.RestartEngineCoroutine()); |
| bool | Method | public bool DetectAndHandleCrash() | Detect engine crash | bool crashed = bridge.DetectAndHandleCrash(); |
| void | Method | public void SendCommand(string command) | Send UCI command to engine | bridge.SendCommand("isready"); |
| IEnumerator | Coroutine | public IEnumerator InitializeEngineCoroutine() | Initialize and wait for ready | yield return bridge.InitializeEngineCoroutine(); StartCoroutine(bridge.InitializeEngineCoroutine()); |
| void | Method | public void RunAllTests() | Run comprehensive test suite | bridge.RunAllTests(); |
| string | Property | public string LastRawOutput { get; private set; } | Last engine output | string output = bridge.LastRawOutput; |
| StockfishBridge.ChessAnalysisResult | Property | public StockfishBridge.ChessAnalysisResult LastAnalysisResult { get; private set; } | Last analysis result | var result = bridge.LastAnalysisResult; |
| List<StockfishBridge.GameHistoryEntry> | Property | public List<StockfishBridge.GameHistoryEntry> GameHistory { get; private set; } | Move history list | var history = bridge.GameHistory; |
| int | Property | public int CurrentHistoryIndex { get; private set; } | Current history position | int index = bridge.CurrentHistoryIndex; |
| bool | Property | public bool IsEngineRunning { get; } | Engine process status | bool running = bridge.IsEngineRunning; |
| bool | Property | public bool IsReady { get; private set; } | Engine ready status | bool ready = bridge.IsReady; |
| char | Property | public char HumanSide { get; set; } | Human player side | char side = bridge.HumanSide; bridge.HumanSide = 'w'; |
| char | Property | public char EngineSide { get; } | Engine player side | char side = bridge.EngineSide; |
| UnityEvent<string> | Event | public UnityEvent<string> OnEngineLine | Engine output line received | bridge.OnEngineLine.AddListener(OnEngineOutput); |
| UnityEvent<StockfishBridge.ChessAnalysisResult> | Event | public UnityEvent<StockfishBridge.ChessAnalysisResult> OnAnalysisComplete | Analysis completed | bridge.OnAnalysisComplete.AddListener(OnAnalysisResult); |
| UnityEvent<char> | Event | public UnityEvent<char> OnSideToMoveChanged | Side selection changed | bridge.OnSideToMoveChanged.AddListener(OnSideChanged); |

## MonoBehaviour Integration
**MonoBehaviour Classes:** `StockfishBridge`
**Non-MonoBehaviour Classes:** None (all nested types)

**Unity Lifecycle Methods (per MonoBehaviour class):**
* **`StockfishBridge` (MonoBehaviour):**
  * `Awake()` - Initializes engine process, starts engine, begins initialization coroutine, sets static debug flag
  * `Update()` - Processes incoming engine output lines, fires events, tracks request completion, manages readiness state
  * `OnApplicationQuit()` - Calls StopEngine() for cleanup

**SerializeField Dependencies (per MonoBehaviour class):**
* **`StockfishBridge`:**
  * `[SerializeField] private int defaultTimeoutMs` - Inspector assignment required
  * `[SerializeField] private bool enableDebugLogging` - Inspector assignment required  
  * `[SerializeField] public bool enableEvaluation` - Inspector assignment required
  * `[SerializeField] private int defaultDepth` - Inspector assignment required
  * `[SerializeField] private int evalDepth` - Inspector assignment required
  * `[SerializeField] private int defaultElo` - Inspector assignment required
  * `[SerializeField] private int defaultSkillLevel` - Inspector assignment required
  * `[SerializeField] private bool allowPlayerSideSelection` - Inspector assignment required
  * `[SerializeField] private char humanSide` - Inspector assignment required
  * `[SerializeField] private int maxHistorySize` - Inspector assignment required

## Important Types

### `StockfishBridge`
* **Kind:** MonoBehaviour class
* **Responsibility:** Manages Stockfish engine process, provides chess analysis API, handles game history and promotion detection
* **Constructor(s):** N/A (MonoBehaviour - Unity managed)
* **Public Properties:**
  * `LastRawOutput` — `string` — Raw engine output from last analysis (`get`)
  * `LastAnalysisResult` — `StockfishBridge.ChessAnalysisResult` — Parsed analysis result (`get`)  
  * `GameHistory` — `List<StockfishBridge.GameHistoryEntry>` — Complete move history (`get`)
  * `CurrentHistoryIndex` — `int` — Current position in history for undo/redo (`get`)
  * `IsEngineRunning` — `bool` — Engine process health status (`get`)
  * `IsReady` — `bool` — Engine initialization and readiness (`get`)
  * `HumanSide` — `char` — Human player side 'w' or 'b' (`get/set`)
  * `EngineSide` — `char` — Engine player side opposite of human (`get`)
* **Public Methods:**
  * **`public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel)`**
    * Description: Comprehensive chess analysis with promotion detection and probability calculation
    * Parameters: `fen : string — Position FEN or "startpos"`, `movetimeMs : int — Search time limit`, `searchDepth : int — Search depth`, `evaluationDepth : int — Evaluation depth`, `elo : int — Engine strength`, `skillLevel : int — Skill limitation 0-20`
    * Returns: `IEnumerator — Unity coroutine for non-blocking analysis` + call example: `yield return bridge.AnalyzePositionCoroutine("startpos", 5000, 15, 18, 2000, 10);`
    * Notes: Thread-safe, handles promotion parsing, crash recovery, timeout protection

### `StockfishBridge.ChessAnalysisResult`
* **Kind:** class (nested in StockfishBridge)
* **Responsibility:** Comprehensive chess analysis data including promotion detection, evaluation scores, and game state
* **Constructor(s):** `public ChessAnalysisResult()` - parameterless constructor
* **Public Properties:**
  * `bestMove` — `string` — UCI move notation or special values ("check-mate", "stale-mate", "ERROR: message") (`get/set`)
  * `sideToMove` — `char` — Current player 'w' or 'b' (`get/set`)
  * `currentFen` — `string` — Position FEN string (`get/set`)
  * `whiteWinProbability` — `float` — White win probability 0-1 based on evaluation (`get/set`)
  * `sideToMoveWinProbability` — `float` — Current side win probability 0-1 (`get/set`)
  * `centipawnEvaluation` — `float` — Raw centipawn score from engine (`get/set`)
  * `isMateScore` — `bool` — True if evaluation represents mate (`get/set`)
  * `mateDistance` — `int` — Moves to mate (+ white mates, - black mates) (`get/set`)
  * `isGameEnd` — `bool` — True for checkmate or stalemate (`get/set`)
  * `isCheckmate` — `bool` — True for checkmate positions (`get/set`)
  * `isStalemate` — `bool` — True for stalemate positions (`get/set`)
  * `inCheck` — `bool` — True if current side in check (`get/set`)
  * `isPromotion` — `bool` — True if bestMove is promotion (`get/set`)
  * `promotionPiece` — `char` — Promotion piece ('q', 'r', 'b', 'n' or uppercase) (`get/set`)
  * `promotionFrom` — `v2` — Source square coordinates (`get/set`)
  * `promotionTo` — `v2` — Target square coordinates (`get/set`)
  * `isPromotionCapture` — `bool` — True if promotion captures (`get/set`)
* **Public Methods:**
  * **`public void ParsePromotionData()`**
    * Description: Parse UCI promotion move and validate promotion data
    * Parameters: None
    * Returns: `void — Populates promotion fields from bestMove` + call example: `result.ParsePromotionData();`
  * **`public string GetPromotionDescription()`**
    * Description: Human-readable promotion description
    * Returns: `string — Descriptive text of promotion` + call example: `string desc = result.GetPromotionDescription();`

### `StockfishBridge.GameHistoryEntry`  
* **Kind:** class (nested in StockfishBridge)
* **Responsibility:** Single move history record with position, move data, and metadata
* **Constructor(s):** `public GameHistoryEntry(string fen, ChessMove move, string notation, float evaluation)` - creates history entry
* **Public Properties:**
  * `fen` — `string` — Position before move (`get/set`)
  * `move` — `ChessMove` — Chess move object (`get/set`)
  * `moveNotation` — `string` — Human-readable notation (`get/set`)
  * `evaluationScore` — `float` — Position evaluation (`get/set`)
  * `timestamp` — `DateTime` — Move timestamp (`get/set`)

## Example Usage
**Required namespaces:**
```csharp
using System.Collections;
using UnityEngine;
using GPTDeepResearch;
using SPACE_UTIL;
```

**For files with MonoBehaviour classes:**
```csharp
public class ExampleUsage : MonoBehaviour 
{
    // MonoBehaviour class references (assigned in Inspector)
    [SerializeField] private StockfishBridge stockfishBridge; 
    
    private void Start()
    {
        // Engine is auto-started in Awake, wait for ready
        StartCoroutine(StockfishBridge_Check());
    }
    
    private IEnumerator StockfishBridge_Check() 
    {
        // Test MonoBehaviour class APIs
        yield return stockfishBridge.InitializeEngineCoroutine();
        
        yield return stockfishBridge.AnalyzePositionCoroutine("startpos");
        var analysisResult = stockfishBridge.LastAnalysisResult;
        
        stockfishBridge.SetHumanSide('w');
        var humanSide = stockfishBridge.HumanSide;
        var engineSide = stockfishBridge.EngineSide;
        
        var canUndo = stockfishBridge.CanUndo();
        var isRunning = stockfishBridge.IsEngineRunning;
        var isReady = stockfishBridge.IsReady;
        
        stockfishBridge.SendCommand("isready");
        var pgn = stockfishBridge.GetGameHistoryPGN();
        
        Debug.Log($"API Results: Move:{analysisResult.bestMove} Eval:{analysisResult.whiteWinProbability:F3} Sides:{humanSide}/{engineSide} Status:{isRunning}/{isReady} Undo:{canUndo} PGN:{pgn.Length}chars");
        yield break;
    }
}
```

## Control Flow & Responsibilities
Engine process management, UCI communication, chess analysis parsing, game history tracking, promotion detection.

## Performance & Threading  
Background reader thread, main-thread coroutines, crash detection, memory cleanup.

## Cross-file Dependencies
References ChessBoard.AlgebraicToCoord, ChessMove.FromUCI, v2 coordinate type from SPACE_UTIL namespace.

## Major Functionality
Full UCI promotion parsing, Elo-based strength control, undo/redo history, engine crash recovery.

`checksum: SF2E4A9B v0.6.min`