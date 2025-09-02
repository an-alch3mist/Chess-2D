# `StockfishBridge.cs` — Unity Stockfish chess engine bridge with promotion support and game management (Read-Only)

Unity MonoBehaviour that provides non-blocking chess engine communication with comprehensive promotion parsing, undo/redo functionality, and full game state management.

## Short description (2–4 sentences)
This file implements a Unity bridge to the Stockfish chess engine, handling UCI protocol communication through background threads. It provides comprehensive chess position analysis with enhanced promotion move parsing, Elo-based evaluation, and complete game history management. The class manages engine lifecycle, crash detection/recovery, and converts engine output into structured analysis results with win probability calculations.

## Metadata
- **Filename:** `StockfishBridge.cs`
- **Primary namespace:** `GPTDeepResearch`
- **Estimated lines:** 750
- **Estimated chars:** 47,000
- **Public types:** `StockfishBridge, ChessAnalysisResult, GameHistoryEntry`
- **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
- **Dependencies:** `SPACE_UTIL` (v2 struct), `ChessBoard.cs` (AlgebraicToCoord, CoordToAlgebraic), `ChessMove.cs` (FromUCI, Invalid)

## Public API summary (table)

| Type | Member | Signature | Short purpose |
|------|--------|-----------|--------------|
| StockfishBridge | StartEngine() | public void | Start Stockfish process |
| StockfishBridge | StopEngine() | public void | Stop engine and cleanup |
| StockfishBridge | AnalyzePositionCoroutine() | public IEnumerator AnalyzePositionCoroutine(string fen) | Analyze position with defaults |
| StockfishBridge | AnalyzePositionCoroutine() | public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel) | Full analysis with custom settings |
| StockfishBridge | SetHumanSide() | public void SetHumanSide(char side) | Set human player side ('w'/'b') |
| StockfishBridge | AddMoveToHistory() | public void AddMoveToHistory(string fen, ChessMove move, string notation, float evaluation) | Add move to game history |
| StockfishBridge | UndoMove() | public GameHistoryEntry UndoMove() | Undo last move |
| StockfishBridge | RedoMove() | public GameHistoryEntry RedoMove() | Redo next move |
| StockfishBridge | SendCommand() | public void SendCommand(string command) | Send UCI command to engine |
| ChessAnalysisResult | ParsePromotionData() | public void ParsePromotionData() | Parse promotion from bestMove |
| ChessAnalysisResult | ToChessMove() | public ChessMove ToChessMove(ChessBoard board) | Convert to ChessMove object |
| ChessAnalysisResult | GetEvaluationDisplay() | public string GetEvaluationDisplay() | Get UI-friendly evaluation text |

## Important types — details

### `StockfishBridge`
- **Kind:** class (MonoBehaviour)
- **Responsibility:** Manages Stockfish engine communication and provides chess analysis services
- **Constructor(s):** MonoBehaviour (Unity handles instantiation)
- **Public properties / fields:**
  - LastRawOutput — string — Raw engine output from last analysis
  - LastAnalysisResult — ChessAnalysisResult — Structured result from last analysis
  - GameHistory — List<GameHistoryEntry> — Complete move history with undo support
  - CurrentHistoryIndex — int — Current position in history (-1 if at start)
  - IsEngineRunning — bool — True if engine process is alive and responsive
  - IsReady — bool — True if engine has responded to UCI handshake
  - HumanSide — char — Human player side ('w' or 'b')
  - EngineSide — char — Engine side (opposite of HumanSide)
- **Public methods:**
  - **StartEngine():** Starts Stockfish process and background communication thread
    - **Parameters:** None
    - **Returns:** void
    - **Side effects:** Creates Process, Thread, temp file copy on Windows builds
    - **Notes:** Thread-safe, idempotent if already running
  - **AnalyzePositionCoroutine():** Analyzes chess position with comprehensive promotion detection
    - **Parameters:** fen : string — FEN position or "startpos", optional movetimeMs/depths/elo/skill
    - **Returns:** IEnumerator (Unity coroutine)
    - **Side effects:** Updates LastAnalysisResult, fires OnAnalysisComplete event
    - **Complexity:** O(depth^branching_factor) engine search
    - **Notes:** Non-blocking coroutine, handles timeouts, crash recovery
  - **SetHumanSide():** Sets which side human controls
    - **Parameters:** side : char — 'w' for white, 'b' for black
    - **Returns:** void
    - **Side effects:** Fires OnSideToMoveChanged event
  - **UndoMove()/RedoMove():** Navigate game history
    - **Returns:** GameHistoryEntry — Move data or null if no move available
    - **Side effects:** Updates CurrentHistoryIndex

### `ChessAnalysisResult`
- **Kind:** class (Serializable)
- **Responsibility:** Structured container for engine analysis with promotion and evaluation data
- **Constructor(s):** Default constructor initializes all fields
- **Public properties / fields:**
  - bestMove — string — UCI move ("e2e4", "e7e8q") or special values ("check-mate", "stale-mate", "ERROR: message")
  - sideToMove — char — Current player ('w'/'b')
  - whiteWinProbability — float — 0-1 probability white wins
  - sideToMoveWinProbability — float — 0-1 probability current player wins
  - centipawnEvaluation — float — Raw centipawn score
  - isPromotion — bool — True if bestMove contains promotion
  - promotionPiece — char — Promotion piece ('q','r','b','n' or uppercase)
  - isGameEnd/isCheckmate/isStalemate — bool — Game termination flags
- **Public methods:**
  - **ParsePromotionData():** Validates and extracts promotion data from UCI move
    - **Parameters:** None (uses internal bestMove)
    - **Returns:** void
    - **Side effects:** Sets promotion-related fields
    - **Notes:** Validates rank requirements, piece characters, coordinate bounds
  - **GetPromotionDescription():** Human-readable promotion description
    - **Returns:** string — "White promotes to Queen with capture (e7-e8)" or empty
  - **ToChessMove():** Converts to ChessMove object for game logic
    - **Parameters:** board : ChessBoard — Current board state for validation
    - **Returns:** ChessMove — Valid move or ChessMove.Invalid()

### `GameHistoryEntry`
- **Kind:** class (Serializable)
- **Responsibility:** Single move record with FEN, move data, and metadata
- **Constructor(s):** GameHistoryEntry(string fen, ChessMove move, string notation, float evaluation)
- **Public properties / fields:**
  - fen — string — Position before move
  - move — ChessMove — Move that was played
  - moveNotation — string — Human-readable notation
  - evaluationScore — float — Position evaluation after move
  - timestamp — DateTime — When move was made

## Example usage

```csharp
// Basic analysis
StockfishBridge bridge = GetComponent<StockfishBridge>();
yield return StartCoroutine(bridge.AnalyzePositionCoroutine("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"));

// Check result
ChessAnalysisResult result = bridge.LastAnalysisResult;
Debug.Log($"Best move: {result.bestMove}");
Debug.Log($"White win probability: {result.whiteWinProbability:P1}");

// Handle promotion
if (result.isPromotion) {
    Debug.Log($"Promotion: {result.GetPromotionDescription()}");
    ChessMove move = result.ToChessMove(chessBoard);
}

// Game management
bridge.SetHumanSide('w');
bridge.AddMoveToHistory(fenBeforeMove, move, "e4", 0.23f);
if (bridge.CanUndo()) {
    GameHistoryEntry undoEntry = bridge.UndoMove();
}
```

## Control flow / responsibilities & high-level algorithm summary

The bridge operates through background thread communication with the Stockfish process. Main workflow: (1) Unity Awake starts engine process and reader thread, (2) Analysis requests queue UCI commands via SendCommand, (3) Background thread reads engine output into ConcurrentQueue, (4) Main thread Update() drains queue and fires events, (5) AnalyzePositionCoroutine waits for bestmove response and parses structured results.

The analysis algorithm configures engine strength (UCI_Elo, skill level), sends position via UCI protocol, requests evaluation at specified depth, then parses the response for move and evaluation data. Promotion parsing validates UCI format (e7e8q) against chess rules (rank requirements, piece validation). Win probability calculation uses logistic function based on centipawn research.

Game history maintains undo/redo stack with FEN snapshots. Crash detection monitors process state and communication timeouts, triggering automatic restart sequences.

## Side effects and I/O

- **File I/O:** Copies sf-engine.exe from StreamingAssets to temp directory on Windows builds
- **Process management:** Creates/destroys Stockfish subprocess with redirected stdin/stdout
- **Threading:** Background reader thread continuously reads engine output
- **Unity lifecycle:** Awake starts engine, OnApplicationQuit ensures cleanup
- **Global state:** Updates static enableDebugLogging_static flag
- **Events:** Fires UnityEvents for engine lines, analysis completion, side changes

## Performance, allocations, and hotspots

- **Heavy operations:** Engine process startup (200-500ms), deep position analysis (100ms-5s depending on depth)
- **Allocations:** String parsing for each engine output line, StringBuilder for result formatting, List<string> for command tracking
- **Hotspots:** Update() drains ConcurrentQueue every frame, background thread continuously reads StandardOutput
- **GC concerns:** Frequent string allocations during parsing, temp collections for UCI command sequences
- **Memory:** Game history limited to maxHistorySize (default 100) entries to prevent unbounded growth

## Threading / async considerations

- **Background thread:** Reader thread runs continuously, must handle IOException/ObjectDisposedException on engine crash
- **Thread safety:** ConcurrentQueue for cross-thread communication, volatile flags for crash detection, lock objects for critical sections
- **Unity main thread:** All Unity API calls (Debug.Log, events) restricted to main thread via Update()
- **Coroutines:** Analysis uses Unity coroutines for non-blocking operation with timeout handling
- **Race conditions:** Request tracking uses locks to coordinate between background reader and analysis requests

## Security / safety / correctness concerns

- **Process security:** Engine executable copied to temp directory, process killed on shutdown
- **Input validation:** Comprehensive FEN validation prevents malformed position parsing
- **UCI injection:** No user input directly passed to UCI commands without validation
- **Null safety:** Extensive null checks for process, streams, and parsing results
- **Exception handling:** Try-catch blocks around all process operations with graceful degradation
- **Resource leaks:** Proper disposal of Process, Thread, and temp files in cleanup

## Tests, debugging & observability

- **Logging:** Comprehensive Debug.Log statements with color coding (green=success, red=error, yellow=warning, cyan=info)
- **Test methods:** TestPromotionParsing() validates UCI promotion format parsing, TestEloCalculation() verifies rating calculations
- **Debug flags:** enableDebugLogging controls verbose output, rawEngineOutput field preserves full engine response
- **Events:** OnEngineLine provides real-time engine communication monitoring
- **Metrics:** analysisTimeMs tracks performance, approximateElo estimates engine strength

## Cross-file references

- `ChessBoard.cs`: AlgebraicToCoord(), CoordToAlgebraic() for coordinate conversion
- `ChessMove.cs`: FromUCI(), Invalid() for move object creation
- `SPACE_UTIL`: v2 struct for 2D coordinates

<!--
## TODO / Known limitations / Suggested improvements (Only If explicitely mentioned in the prompt)

- TODO: Add support for MultiPV analysis for move ranking
- TODO: Implement opening book integration
- TODO: Add support for UCI engine options configuration
- Limitation: Windows-only temp file copying logic
- Limitation: Single engine instance per MonoBehaviour
- Suggested: Add chess variant support (Chess960, King of the Hill)
- Suggested: Implement pondering (background analysis during opponent's turn)
- Suggested: Add time control management for tournament play
-->

## Appendix

**Key private methods:**
- `ConvertCentipawnsToWinProbability(float cp)`: Uses logistic function P = 1/(1+exp(-0.004*cp))
- `DetectAndHandleCrash()`: Monitors process.HasExited and communication timeouts
- `ParseAnalysisResult(string output, int depth, int evalDepth)`: Main result parsing logic

**Constants:**
- `CENTIPAWN_SCALE = 0.004f`: Research-based probability conversion factor
- `MATE_BONUS = 950f`: Mate evaluation bonus scaling

**File checksum:** First 8 chars of conceptual SHA1: `a7b8c9d0`