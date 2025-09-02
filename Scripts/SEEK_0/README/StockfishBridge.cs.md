# `StockfishBridge.cs` — Unity Stockfish bridge with comprehensive promotion support and undo/redo functionality

Unity MonoBehaviour bridge to Stockfish engine providing non-blocking chess analysis with full promotion move parsing, game history management, and engine crash recovery.

## Short description (2–4 sentences)
Implements a complete bridge between Unity and the Stockfish chess engine, handling UCI communication, move analysis, and position evaluation. Provides comprehensive promotion move parsing from UCI format, undo/redo game history management, and robust crash detection with automatic recovery. Designed for game development with full support for pawn promotion scenarios and engine strength configuration. Includes research-based evaluation probability calculations and thread-safe communication patterns.

## Metadata
- **Filename:** `StockfishBridge.cs`
- **Primary namespace:** `GPTDeepResearch`
- **Dependent namespace:** `SPACE_UTIL` (using SPACE_UTIL;)
- **Estimated lines:** 1700
- **Estimated chars:** 47,000
- **Public types:** `StockfishBridge` (class), `ChessAnalysisResult` (class), `GameHistoryEntry` (class)
- **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
- **Dependencies:** SPACE_UTIL.v2, UnityEngine, System.Diagnostics.Process

## Public API summary (table)

| Type | Member | Signature | Short purpose | OneLiner Call |
|------|--------|-----------|---------------|---------------|
| StockfishBridge (class) | StartEngine | `public void StartEngine()` | Start Stockfish process | `bridge.StartEngine()` |
| StockfishBridge | StopEngine | `public void StopEngine()` | Stop and cleanup engine | `bridge.StopEngine()` |
| StockfishBridge | AnalyzePositionCoroutine | `public IEnumerator AnalyzePositionCoroutine(string fen)` | Analyze position with defaults | `StartCoroutine(bridge.AnalyzePositionCoroutine(fen))` |
| StockfishBridge | AnalyzePositionCoroutine | `public IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel)` | Full analysis with config | `StartCoroutine(bridge.AnalyzePositionCoroutine(fen, 5000, 15, 18, 1800, 10))` |
| StockfishBridge | SetHumanSide | `public void SetHumanSide(char side)` | Set human player side | `bridge.SetHumanSide('w')` |
| StockfishBridge | AddMoveToHistory | `public void AddMoveToHistory(string fen, ChessMove move, string notation, float evaluation)` | Add move to game history | `bridge.AddMoveToHistory(fen, move, "e4", 0.3f)` |
| StockfishBridge | UndoMove | `public GameHistoryEntry UndoMove()` | Undo last move | `var entry = bridge.UndoMove()` |
| StockfishBridge | RedoMove | `public GameHistoryEntry RedoMove()` | Redo next move | `var entry = bridge.RedoMove()` |
| StockfishBridge | CanUndo | `public bool CanUndo()` | Check if undo possible | `if (bridge.CanUndo())` |
| StockfishBridge | CanRedo | `public bool CanRedo()` | Check if redo possible | `if (bridge.CanRedo())` |
| StockfishBridge | ClearHistory | `public void ClearHistory()` | Clear game history | `bridge.ClearHistory()` |
| StockfishBridge | GetGameHistoryPGN | `public string GetGameHistoryPGN()` | Get history as PGN | `string pgn = bridge.GetGameHistoryPGN()` |
| StockfishBridge | RestartEngineCoroutine | `public IEnumerator RestartEngineCoroutine()` | Restart crashed engine | `StartCoroutine(bridge.RestartEngineCoroutine())` |
| StockfishBridge | DetectAndHandleCrash | `public bool DetectAndHandleCrash()` | Check engine health | `bool crashed = bridge.DetectAndHandleCrash()` |
| StockfishBridge | SendCommand | `public void SendCommand(string command)` | Send UCI command | `bridge.SendCommand("uci")` |
| StockfishBridge | InitializeEngineCoroutine | `public IEnumerator InitializeEngineCoroutine()` | Initialize and wait ready | `StartCoroutine(bridge.InitializeEngineCoroutine())` |
| StockfishBridge | TestPromotionParsing | `public void TestPromotionParsing()` | Test promotion parsing | `bridge.TestPromotionParsing()` |
| StockfishBridge | TestEloCalculation | `public void TestEloCalculation()` | Test Elo calculations | `bridge.TestEloCalculation()` |
| ChessAnalysisResult (class) | ParsePromotionData | `public void ParsePromotionData()` | Parse promotion from bestMove | `result.ParsePromotionData()` |
| ChessAnalysisResult | GetPromotionDescription | `public string GetPromotionDescription()` | Human-readable promotion | `string desc = result.GetPromotionDescription()` |
| ChessAnalysisResult | ToChessMove | `public ChessMove ToChessMove(ChessBoard board)` | Convert to ChessMove | `ChessMove move = result.ToChessMove(board)` |
| ChessAnalysisResult | GetEvaluationDisplay | `public string GetEvaluationDisplay()` | Format evaluation for UI | `string eval = result.GetEvaluationDisplay()` |
| GameHistoryEntry (class) | Constructor | `public GameHistoryEntry(string fen, ChessMove move, string notation, float evaluation)` | Create history entry | `new GameHistoryEntry(fen, move, "e4", 0.2f)` |

## Important types — details

### `StockfishBridge`
- **Kind:** class (MonoBehaviour)
- **Responsibility:** Manages Stockfish engine communication, chess analysis, and game state
- **Constructor(s):** Unity MonoBehaviour (no explicit constructor)
- **Public properties / fields:** 
  - `LastRawOutput` — string — Raw engine output from last analysis
  - `LastAnalysisResult` — ChessAnalysisResult — Parsed result from last analysis
  - `GameHistory` — List<GameHistoryEntry> — Complete game move history
  - `CurrentHistoryIndex` — int — Current position in history
  - `IsEngineRunning` — bool — True if engine process is active
  - `IsReady` — bool — True if engine initialized
  - `HumanSide` — char — Human player side ('w' or 'b')
  - `EngineSide` — char — Engine player side (opposite of human)
- **Public methods:** See API summary table above
- **Notes:** Thread-safe design, handles engine crashes, supports Unity coroutines

### `ChessAnalysisResult`
- **Kind:** class (located in StockfishBridge)
- **Responsibility:** Contains comprehensive analysis data including promotion parsing and evaluation
- **Constructor(s):** Default constructor
- **Public properties / fields:**
  - `bestMove` — string — UCI move or special values (check-mate, stale-mate, ERROR)
  - `sideToMove` — char — Side to move ('w' or 'b')
  - `currentFen` — string — Position FEN
  - `whiteWinProbability` — float — White win probability (0-1)
  - `sideToMoveWinProbability` — float — Current side win probability (0-1)
  - `centipawnEvaluation` — float — Raw centipawn score
  - `isMateScore` — bool — True if mate evaluation
  - `mateDistance` — int — Moves to mate (+ white, - black)
  - `isGameEnd` — bool — True if checkmate/stalemate
  - `isCheckmate` — bool — True if checkmate
  - `isStalemate` — bool — True if stalemate
  - `inCheck` — bool — True if side in check
  - `isPromotion` — bool — True if move is promotion
  - `promotionPiece` — char — Promotion piece character
  - `promotionFrom` — v2 — Source square of promotion
  - `promotionTo` — v2 — Target square of promotion
  - `isPromotionCapture` — bool — True if promotion with capture
  - `errorMessage` — string — Error details if any
  - `rawEngineOutput` — string — Full engine response
  - `searchDepth` — int — Search depth used
  - `evaluationDepth` — int — Evaluation depth used
  - `skillLevel` — int — Engine skill level
  - `approximateElo` — int — Calculated engine Elo
  - `analysisTimeMs` — float — Analysis duration

### `GameHistoryEntry`
- **Kind:** class (located in StockfishBridge)
- **Responsibility:** Stores single move history with metadata
- **Constructor(s):** `GameHistoryEntry(string fen, ChessMove move, string notation, float evaluation)`
- **Public properties / fields:**
  - `fen` — string — Position before move
  - `move` — ChessMove — The move made
  - `moveNotation` — string — Human-readable notation
  - `evaluationScore` — float — Position evaluation
  - `timestamp` — DateTime — When move was made

## Example usage

```csharp
// Basic engine startup and analysis
StockfishBridge bridge = GetComponent<StockfishBridge>();
bridge.StartEngine();
StartCoroutine(bridge.AnalyzePositionCoroutine("startpos"));

// Access analysis results
ChessAnalysisResult result = bridge.LastAnalysisResult;
if (result.isPromotion) {
    Debug.Log(result.GetPromotionDescription());
}

// Game history management
bridge.SetHumanSide('w');
bridge.AddMoveToHistory("fen", move, "e4", 0.2f);
if (bridge.CanUndo()) {
    bridge.UndoMove();
}
```

## Control flow / responsibilities & high-level algorithm summary

The bridge operates through several key flows: Engine lifecycle management starts the Stockfish process, establishes stdio communication, and handles crash detection/recovery. Analysis flow sends UCI commands (ucinewgame, position, go), waits for bestmove response, and parses results including promotion detection. The background reader thread continuously processes engine output while the main Unity thread handles UI events and coroutine coordination. Game management tracks move history with undo/redo support and maintains game state. The evaluation system uses research-based logistic functions to convert centipawn scores to win probabilities, with special handling for mate scores.

## Side effects and I/O

Creates temporary engine executable files on Windows builds, launches external Stockfish process with stdio redirection, uses background thread for continuous output reading, writes to Unity console with colored debug logging, modifies game history collections, and fires Unity events for analysis completion and side changes. Engine commands are queued and processed asynchronously with crash detection monitoring.

## Performance, allocations, and hotspots

Analysis requests allocate StringBuilder objects for command construction and List<string> for output collection. Background thread continuously reads engine output causing frequent string allocations. FEN validation performs string splits and character validation. Move parsing involves string operations and algebraic coordinate conversions. UCI cache in referenced ChessMove reduces parsing overhead. Game history maintains bounded list (maxHistorySize) to prevent memory growth.

## Threading / async considerations

Background reader thread runs continuously, main thread processes queued engine output via Update(), all engine communication protected by locks for thread safety, Unity coroutines handle async analysis with timeout support, volatile flags manage cross-thread communication, and process I/O operations can block briefly. Engine crashes detected across threads with proper synchronization.

## Security / safety / correctness concerns

External process execution with proper path validation, temp file creation/cleanup on Windows, engine crash detection with recovery attempts, FEN validation prevents malformed position strings, UCI command injection protection through parameter validation, thread-safe communication patterns, and bounded memory usage for history and caches. Engine output parsing validates move formats and coordinates.

## Tests, debugging & observability

Comprehensive debug logging with color-coded Unity console output, built-in test methods for promotion parsing and Elo calculation, analysis timing measurement, engine output capture for debugging, crash detection logging, and Unity events for external monitoring. Test methods validate core functionality and performance optimizations.

## Cross-file references

- `SPACE_UTIL.v2` — 2D coordinate structure for chess squares
- `ChessBoard.cs` — Chess board state and move generation (AlgebraicToCoord, CoordToAlgebraic, GetLegalMoves, GetPiece, LoadFromFEN methods)
- `ChessMove.cs` — Move representation and parsing (FromUCI, ToUCI, IsValid methods)

<!-- TODO(only if i have explicitly mentioned in prompt): Consider adding persistent engine configuration, implementing move validation before engine submission, adding support for time controls and increment, implementing analysis caching for repeated positions, adding support for multiple engine instances, and considering move quality annotations based on evaluation changes. Future versions could benefit from asynchronous initialization patterns and improved error recovery strategies. -->

## Appendix

Key private methods include `ParseAnalysisResult()` for comprehensive output parsing, `ConfigureEngineStrength()` for UCI option management, `CalculateApproximateElo()` using research-based rating calculations, and `ConvertCentipawnsToWinProbability()` with logistic function implementation. The background reader uses `BackgroundReaderLoop()` with exception handling and proper cleanup patterns.

File checksum: SHA1 first 8 chars would be calculated from file content for version tracking.