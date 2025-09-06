# Source: `StockfishBridge.cs` — Unity chess engine bridge with comprehensive promotion support and game management

* Unity MonoBehaviour that provides non-blocking Stockfish chess engine integration with full promotion handling, undo/redo functionality, and side selection.

## Short description (2–4 sentences)

StockfishBridge implements a complete chess engine interface for Unity games, handling UCI communication, position analysis, and game state management. It provides comprehensive promotion move parsing (e7e8q, a2a1n), evaluation probability calculations using research-based logistic functions, and robust crash detection with automatic recovery. The bridge supports configurable engine strength via Elo ratings and skill levels, maintains complete game history with undo/redo capabilities, and offers side selection for human vs engine gameplay.

## Metadata

* **Filename:** `StockfishBridge.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `System, System.Text, System.IO, System.Collections, System.Collections.Concurrent, System.Collections.Generic, System.Diagnostics, System.Threading, UnityEngine, UnityEngine.Events, SPACE_UTIL`
* **Estimated lines:** 1800
* **Estimated chars:** 45,000
* **Public types:** `StockfishBridge (class, inherits MonoBehaviour), StockfishBridge.ChessAnalysisResult (class), StockfishBridge.GameHistoryEntry (class)`
* **Unity version / Target framework (if detectable):** Unity 2020.3 / .NET Standard 2.0
* **Dependencies:** `SPACE_UTIL.v2` (SPACE_UTIL is namespace), ChessBoard.cs, ChessMove.cs

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| int (basic-data-type) | defaultTimeoutMs | [SerializeField] private int defaultTimeoutMs = 20000 | Default analysis timeout in milliseconds | var timeout = bridge.defaultTimeoutMs; |
| bool (basic-data-type) | enableDebugLogging | [SerializeField] private bool enableDebugLogging = true | Enable console debug output | var debug = bridge.enableDebugLogging; |
| bool (basic-data-type) | enableEvaluation | [SerializeField] public bool enableEvaluation = true | Enable position evaluation | var evalEnabled = bridge.enableEvaluation; |
| int (basic-data-type) | defaultDepth | [SerializeField] private int defaultDepth = 12 | Default search depth | var depth = bridge.defaultDepth; |
| int (basic-data-type) | evalDepth | [SerializeField] private int evalDepth = 15 | Default evaluation depth | var evalDepth = bridge.evalDepth; |
| int (basic-data-type) | defaultElo | [SerializeField] private int defaultElo = 1500 | Default engine Elo rating | var elo = bridge.defaultElo; |
| int (basic-data-type) | defaultSkillLevel | [SerializeField] private int defaultSkillLevel = 8 | Default engine skill level (0-20) | var skill = bridge.defaultSkillLevel; |
| bool (basic-data-type) | allowPlayerSideSelection | [SerializeField] private bool allowPlayerSideSelection = true | Allow human side selection | var allowSelection = bridge.allowPlayerSideSelection; |
| char (basic-data-type) | humanSide | [SerializeField] private char humanSide = 'w' | Human player side ('w' or 'b') | var side = bridge.humanSide; |
| int (basic-data-type) | maxHistorySize | [SerializeField] private int maxHistorySize = 100 | Maximum game history entries | var maxHistory = bridge.maxHistorySize; |
| UnityEvent<string> (class) | OnEngineLine | public UnityEvent<string> OnEngineLine = new UnityEvent<string>() | Event fired for each engine output line | bridge.OnEngineLine.AddListener(handler); |
| UnityEvent<StockfishBridge.ChessAnalysisResult> (class) | OnAnalysisComplete | public UnityEvent<ChessAnalysisResult> OnAnalysisComplete | Event fired when analysis completes | bridge.OnAnalysisComplete.AddListener(handler); |
| UnityEvent<char> (class) | OnSideToMoveChanged | public UnityEvent<char> OnSideToMoveChanged | Event fired when side to move changes | bridge.OnSideToMoveChanged.AddListener(handler); |
| string (basic-data-type) | LastRawOutput | public string LastRawOutput { get; } | Last raw engine output | var output = bridge.LastRawOutput; |
| StockfishBridge.ChessAnalysisResult (class) | LastAnalysisResult | public ChessAnalysisResult LastAnalysisResult { get; } | Last analysis result | var result = bridge.LastAnalysisResult; |
| List<StockfishBridge.GameHistoryEntry> (class) | GameHistory | public List<GameHistoryEntry> GameHistory { get; } | Complete game move history | var history = bridge.GameHistory; |
| int (basic-data-type) | CurrentHistoryIndex | public int CurrentHistoryIndex { get; } | Current position in history | var index = bridge.CurrentHistoryIndex; |
| bool (basic-data-type) | IsEngineRunning | public bool IsEngineRunning { get; } | True if engine process is active | var running = bridge.IsEngineRunning; |
| bool (basic-data-type) | IsReady | public bool IsReady { get; } | True if engine is ready for commands | var ready = bridge.IsReady; |
| char (basic-data-type) | HumanSide | public char HumanSide { get; set; } | Human player side with event firing | bridge.HumanSide = 'w'; |
| char (basic-data-type) | EngineSide | public char EngineSide { get; } | Engine player side (opposite of human) | var engineSide = bridge.EngineSide; |
| void (void) | StartEngine | public void StartEngine() | Start Stockfish engine process | bridge.StartEngine(); |
| void (void) | StopEngine | public void StopEngine() | Stop engine and cleanup resources | bridge.StopEngine(); |
| IEnumerator (IEnumerator) | AnalyzePositionCoroutine | public IEnumerator AnalyzePositionCoroutine(string fen) | Analyze position with default settings | yield return bridge.AnalyzePositionCoroutine(fen); // StartCoroutine(bridge.AnalyzePositionCoroutine(fen)); |
| IEnumerator (IEnumerator) | AnalyzePositionCoroutine | public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel) | Comprehensive position analysis with custom parameters | yield return bridge.AnalyzePositionCoroutine(fen, 5000, 15, 20, 2000, 10); // StartCoroutine(bridge.AnalyzePositionCoroutine(fen, 5000, 15, 20, 2000, 10)); |
| void (void) | SetHumanSide | public void SetHumanSide(char side) | Set human player side ('w' or 'b') | bridge.SetHumanSide('w'); |
| void (void) | AddMoveToHistory | public void AddMoveToHistory(string fen, ChessMove move, string notation, float evaluation) | Add move to game history | bridge.AddMoveToHistory(fen, move, "e4", 0.3f); |
| StockfishBridge.GameHistoryEntry (class) | UndoMove | public GameHistoryEntry UndoMove() | Undo last move and return entry | var entry = bridge.UndoMove(); |
| StockfishBridge.GameHistoryEntry (class) | RedoMove | public GameHistoryEntry RedoMove() | Redo next move and return entry | var entry = bridge.RedoMove(); |
| bool (basic-data-type) | CanUndo | public bool CanUndo() | Check if undo is possible | var canUndo = bridge.CanUndo(); |
| bool (basic-data-type) | CanRedo | public bool CanRedo() | Check if redo is possible | var canRedo = bridge.CanRedo(); |
| void (void) | ClearHistory | public void ClearHistory() | Clear all game history | bridge.ClearHistory(); |
| string (basic-data-type) | GetGameHistoryPGN | public string GetGameHistoryPGN() | Get history as PGN notation | var pgn = bridge.GetGameHistoryPGN(); |
| IEnumerator (IEnumerator) | RestartEngineCoroutine | public IEnumerator RestartEngineCoroutine() | Restart engine after crash | yield return bridge.RestartEngineCoroutine(); // StartCoroutine(bridge.RestartEngineCoroutine()); |
| bool (basic-data-type) | DetectAndHandleCrash | public bool DetectAndHandleCrash() | Detect and handle engine crashes | var crashed = bridge.DetectAndHandleCrash(); |
| void (void) | SendCommand | public void SendCommand(string command) | Send UCI command to engine | bridge.SendCommand("position startpos"); |
| IEnumerator (IEnumerator) | InitializeEngineCoroutine | public IEnumerator InitializeEngineCoroutine() | Initialize engine and wait for ready | yield return bridge.InitializeEngineCoroutine(); // StartCoroutine(bridge.InitializeEngineCoroutine()); |
| void (void) | RunAllTests | public void RunAllTests() | Run comprehensive API validation tests | bridge.RunAllTests(); |

## Important types — details

### `StockfishBridge`
* **Kind:** class (inherits MonoBehaviour)
* **Responsibility:** Main bridge class managing Stockfish engine communication, analysis, and game state.
* **Constructor(s):** Default MonoBehaviour constructor
* **Public properties / fields:**
  * enableEvaluation — bool — Enable/disable position evaluation (get/set)
  * LastRawOutput — string — Last raw engine output text (get)
  * LastAnalysisResult — ChessAnalysisResult — Complete analysis result with promotion data (get)
  * GameHistory — List<GameHistoryEntry> — Full move history list (get)
  * CurrentHistoryIndex — int — Current position in history for undo/redo (get)
  * IsEngineRunning — bool — True if engine process is active and responsive (get)
  * IsReady — bool — True if engine responded to isready command (get)
  * HumanSide — char — Human player side 'w' or 'b' with event firing (get/set)
  * EngineSide — char — Engine side opposite of human (get)
  * OnEngineLine — UnityEvent<string> — Event for each engine output line (get)
  * OnAnalysisComplete — UnityEvent<ChessAnalysisResult> — Event when analysis completes (get)
  * OnSideToMoveChanged — UnityEvent<char> — Event when human side changes (get)

* **Public methods:**
  * **Signature:** `public void StartEngine()`
  * **Description:** Starts Stockfish process and background reader thread.
  * **Parameters:** None
  * **Returns:** void — StockfishBridge.StartEngine()
  * **Side effects / state changes:** Creates process, starts reader thread, sets up crash detection.
  * **Complexity / performance:** O(1) process startup
  * **Notes:** Handles executable copying on Windows builds, initializes thread-safe communication.

  * **Signature:** `public void StopEngine()`
  * **Description:** Gracefully stops engine with fallback to force kill.
  * **Parameters:** None
  * **Returns:** void — StockfishBridge.StopEngine()
  * **Side effects / state changes:** Stops process, joins reader thread, cleans temp files.
  * **Complexity / performance:** O(1) with 2s timeout for graceful shutdown
  * **Notes:** Thread-safe cleanup, handles disposed process exceptions.

  * **Signature:** `public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs = -1, int searchDepth = 12, int evaluationDepth = 15, int elo = 1500, int skillLevel = 8)`
  * **Description:** Comprehensive chess analysis with promotion detection and evaluation.
  * **Parameters:** 
    - fen : string — FEN position or "startpos"
    - movetimeMs : int — Analysis time limit (-1 for depth)
    - searchDepth : int — Search depth for best move
    - evaluationDepth : int — Depth for position evaluation
    - elo : int — Engine strength Elo rating
    - skillLevel : int — Skill level 0-20 (overrides Elo)
  * **Returns:** IEnumerator — yield return StockfishBridge.AnalyzePositionCoroutine(fen, 5000, 15, 20, 2000, 10) or StartCoroutine(StockfishBridge.AnalyzePositionCoroutine(fen, 5000, 15, 20, 2000, 10))
  * **Throws:** No exceptions thrown, errors stored in ChessAnalysisResult.errorMessage
  * **Side effects / state changes:** Updates LastAnalysisResult, fires OnAnalysisComplete event, sends UCI commands.
  * **Complexity / performance:** O(depth^branching) engine search, timeout protection
  * **Notes:** Detects promotions (e7e8q format), calculates win probabilities, handles mate scores, crash recovery.

  * **Signature:** `public void AddMoveToHistory(string fen, ChessMove move, string notation, float evaluation)`
  * **Description:** Adds move to game history with automatic truncation for redo branches.
  * **Parameters:**
    - fen : string — Position before the move
    - move : ChessMove — The move that was played
    - notation : string — Human-readable notation (e.g., "e4", "Nf3")
    - evaluation : float — Position evaluation after move
  * **Returns:** void — StockfishBridge.AddMoveToHistory(fen, move, "e4", 0.3f)
  * **Side effects / state changes:** Modifies GameHistory list, updates CurrentHistoryIndex, enforces maxHistorySize limit.
  * **Complexity / performance:** O(1) for append, O(n) when history limit exceeded
  * **Notes:** Truncates future moves when adding to middle of history, thread-safe access.

### `StockfishBridge.ChessAnalysisResult`
* **Kind:** class (nested in StockfishBridge)
* **Responsibility:** Complete analysis result containing move data, evaluation, promotion details, and game state.
* **Constructor(s):** Default constructor initializes all fields to safe defaults
* **Public properties / fields:**
  * bestMove — string — Best move in UCI format or special values ("check-mate", "stale-mate", "ERROR: message") (get/set)
  * sideToMove — char — Current side to move 'w' or 'b' (get/set)
  * currentFen — string — Position FEN that was analyzed (get/set)
  * whiteWinProbability — float — White's win probability 0.0-1.0 based on logistic evaluation (get/set)
  * sideToMoveWinProbability — float — Current side's win probability 0.0-1.0 (get/set)
  * centipawnEvaluation — float — Raw centipawn score (positive = white advantage) (get/set)
  * isMateScore — bool — True if evaluation represents forced mate (get/set)
  * mateDistance — int — Moves to mate (+ white mates, - black mates) (get/set)
  * isGameEnd — bool — True for checkmate or stalemate positions (get/set)
  * isCheckmate — bool — True if position is checkmate (get/set)
  * isStalemate — bool — True if position is stalemate (get/set)
  * inCheck — bool — True if side to move is in check (get/set)
  * isPromotion — bool — True if bestMove is a promotion move (get/set)
  * promotionPiece — char — Promotion piece ('q','r','b','n' or uppercase) (get/set)
  * promotionFrom — v2 — Source square of promotion move (get/set)
  * promotionTo — v2 — Target square of promotion move (get/set)
  * isPromotionCapture — bool — True if promotion includes capture (diagonal move) (get/set)
  * errorMessage — string — Detailed error description if analysis failed (get/set)
  * analysisTimeMs — float — Time taken for complete analysis (get/set)

* **Public methods:**
  * **Signature:** `public void ParsePromotionData()`
  * **Description:** Parses UCI promotion moves with comprehensive validation.
  * **Parameters:** None
  * **Returns:** void — result.ParsePromotionData()
  * **Side effects / state changes:** Updates all promotion-related fields based on bestMove format.
  * **Complexity / performance:** O(1) string parsing and validation
  * **Notes:** Validates rank requirements, piece types, and coordinate bounds for promotion moves.

  * **Signature:** `public ChessMove ToChessMove(ChessBoard board)`
  * **Description:** Converts analysis result to ChessMove for game application.
  * **Parameters:** board : ChessBoard — Current board state for move validation
  * **Returns:** ChessMove — ChessMove.ToChessMove(board) where board is ChessBoard instance
  * **Notes:** Returns ChessMove.Invalid() for errors or game end states.

### `StockfishBridge.GameHistoryEntry`
* **Kind:** class (nested in StockfishBridge)
* **Responsibility:** Single move entry in game history for undo/redo functionality.
* **Constructor(s):** `GameHistoryEntry(string fen, ChessMove move, string notation, float evaluation)` — Creates history entry with timestamp
* **Public properties / fields:**
  * fen — string — Position before the move (get/set)
  * move — ChessMove — The move that was made (get/set)
  * moveNotation — string — Human-readable move notation (get/set)
  * evaluationScore — float — Position evaluation after move (get/set)
  * timestamp — DateTime — When the move was made (get/set)

## MonoBehaviour special rules
**Note: MonoBehaviour**

* **Awake()** - Called on script load. Initializes engine by calling StartEngine(), starts initialization coroutine InitializeEngineOnAwake(), and sets static debug logging flag enableDebugLogging_static.

* **Update()** - Called every frame. Drains incomingLines queue and fires OnEngineLine events, tracks engine readiness from "readyok" responses, manages request completion detection for waitingForBestMove state, and updates LastRawOutput when bestmove received.

* **OnApplicationQuit()** - Called when application exits. Performs cleanup by calling StopEngine() to gracefully shutdown the engine process and clean up resources.

## Example usage

```csharp
// Required namespaces
using GPTDeepResearch;
using SPACE_UTIL;

// Basic engine startup and analysis
StockfishBridge bridge = GetComponent<StockfishBridge>();
bridge.StartEngine();
yield return bridge.InitializeEngineCoroutine();

// Analyze starting position
yield return bridge.AnalyzePositionCoroutine("startpos");
ChessAnalysisResult result = bridge.LastAnalysisResult;
Debug.Log($"Best move: {result.bestMove}, Win probability: {result.whiteWinProbability:P1}");

// Handle promotion moves
if (result.isPromotion) {
    Debug.Log($"Promotion: {result.GetPromotionDescription()}");
    ChessMove move = result.ToChessMove(currentBoard);
}
```

## Control flow / responsibilities & high-level algorithm summary
Engine manages UCI communication via background thread, processes analysis requests with timeout protection, parses promotion moves with validation, calculates win probabilities using logistic function.

## Side effects and I/O
File I/O for executable copying, process creation/termination, thread management, Unity event system integration, temporary file cleanup, console logging.

## Performance, allocations, and hotspots
Background reader thread, string allocations in parsing, ConcurrentQueue operations.

## Threading / async considerations
Thread-safe communication queues, background reader thread, volatile crash detection, lock-based request tracking.

## Security / safety / correctness concerns
Process execution, file system access, thread synchronization, engine crash recovery, UCI command injection prevention.

## Tests, debugging & observability
Comprehensive test suite with RunAllTests(), debug logging via enableDebugLogging, engine output events, crash detection with restart capability.

## Cross-file references
Depends on SPACE_UTIL.v2 for coordinate representation, ChessBoard.cs for board state and coordinate conversion, ChessMove.cs for move representation and UCI parsing.

<!-- ## TODO / Known limitations / Suggested improvements
- Add support for chess960 starting positions
- Implement engine options configuration UI
- Add position evaluation caching
- Support multiple concurrent analysis requests
- Add opening book integration
(only if I explicitly mentioned in the prompt) -->

## Appendix
Core analysis flow: StartEngine() → InitializeEngineCoroutine() → AnalyzePositionCoroutine() → ParseAnalysisResult() → OnAnalysisComplete event. Background reader continuously processes engine output through thread-safe queues.

## General Note: important behaviors
Major functionality includes promotion move parsing (e7e8q format), undo/redo game history management, engine crash detection with automatic restart, and research-based evaluation probability calculation.

`checksum: a1b2c3d4 (v0.1)`