# StockfishBridge.cs.md

**Summary (1-3 sentences)**
StockfishBridge is a Unity MonoBehaviour that manages a Stockfish chess engine process, providing coroutine-based position analysis with configurable strength settings (Elo, skill level, depth). It handles engine crashes gracefully with automatic restart, converts engine evaluations to win probabilities, and detects game-ending conditions (checkmate/stalemate).

**Public Purpose / Intent**
- Manage Stockfish engine lifecycle (start/stop/restart)
- Provide non-blocking chess position analysis via coroutines
- Convert UCI engine output to structured analysis results with win probabilities
- Handle engine crashes and automatic recovery
- Support configurable engine strength for different difficulty levels

**Public API (most important members)**
- `class ChessAnalysisResult` — nested class containing analysis results
  - `string bestMove` — best move in UCI notation or "check-mate"/"stale-mate"
  - `char Side` — 'w' or 'b' for side to move
  - `float evaluation` — white win probability (0.0-1.0)
  - `float stmEvaluation` — side-to-move win probability (0.0-1.0)
  - `bool isGameEnd` — true if checkmate or stalemate
  - `string errorMessage` — detailed error if any
  - `int searchDepth` — depth used for move search
  - `int approximateElo` — estimated playing strength
- `void StartEngine()` — starts the Stockfish process
- `void StopEngine()` — gracefully shuts down the engine
- `IEnumerator AnalyzePositionCoroutine(string fen)` — analyze with default settings
- `IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs, int searchDepth, int evaluationDepth, int elo, int skillLevel)` — full analysis control
- `IEnumerator RestartEngineCoroutine()` — restart after crash
- `IEnumerator InitializeEngineCoroutine()` — wait for engine ready state
- `bool DetectAndHandleCrash()` — check if engine crashed
- `void SendCommand(string command)` — send raw UCI command
- `bool IsEngineRunning` — property checking engine status
- `bool IsReady` — property for engine readiness
- `ChessAnalysisResult LastAnalysisResult` — last analysis result
- `string LastRawOutput` — raw engine output for debugging
- `UnityEvent<string> OnEngineLine` — event fired for each engine output line

**Serialization / Inspector fields**
- `int defaultTimeoutMs = 20000` — default timeout for engine requests
- `bool enableDebugLogging = true` — toggle debug console output
- `bool enableEvaluation = false` — whether to compute position evaluation
- `int defaultDepth = 1` — default search depth for moves
- `int evalDepth = 5` — separate depth for evaluation (when enabled)
- `int defaultElo = 400` — default engine Elo rating
- `int defaultSkillLevel = 0` — default skill level (0-20)

**Key internals & flow**
1. Awake() → StartEngine() → extract engine from StreamingAssets, start Process
2. StartBackgroundReader() → spawn thread reading stdout, enqueue lines to ConcurrentQueue
3. Update() → drain incomingLines queue, fire OnEngineLine events, track "bestmove" responses
4. AnalyzePositionCoroutine() → validate FEN, send UCI commands (position/go), wait for bestmove
5. ParseAnalysisResult() → extract best move, parse evaluation from info lines, convert to probabilities
6. Crash detection → monitor process.HasExited, auto-restart via RestartEngineCoroutine()
7. Evaluation mapping → centipawns to probability via logistic function, mate distance to probability

**Dependencies**
- UnityEngine (MonoBehaviour, Coroutines, Events, Debug)
- System.Diagnostics (Process management)
- System.Threading (Thread for reader)
- System.Collections.Concurrent (ConcurrentQueue)
- GPTDeepResearch namespace (assumed parent namespace)
- sf-engine.exe (Stockfish executable in StreamingAssets)

**Side effects / threading / coroutines**
- Spawns background thread for reading engine stdout (BackgroundReaderLoop)
- Creates external process (Stockfish engine)
- Uses coroutines for non-blocking analysis (must be called from MonoBehaviour)
- Main thread only for Unity API calls (Update, Debug.Log)
- Temporary file creation on Windows builds (copies engine to temp)
- File I/O when extracting engine from StreamingAssets

**Typical usage snippet**
```csharp
// In a MonoBehaviour
StockfishBridge bridge = GetComponent<StockfishBridge>();
string fenPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

StartCoroutine(AnalyzePosition());

IEnumerator AnalyzePosition() {
    yield return StartCoroutine(bridge.AnalyzePositionCoroutine(fenPosition, searchDepth: 10));
    var result = bridge.LastAnalysisResult;
    Debug.Log($"Best move: {result.bestMove}, White win prob: {result.evaluation:P}");
}
```

**Important invariants & edge-cases**
- Must call StartEngine() before any analysis (automatic in Awake)
- FEN validation may reject malformed positions with detailed error messages
- Engine process may crash mid-analysis (handled with auto-restart)
- "bestmove (none)" indicates checkmate or stalemate
- Evaluation defaults to 0.5 when enableEvaluation=false
- Thread-safe communication via ConcurrentQueue
- Cleanup on OnApplicationQuit only (persists through focus changes)
- Skill level and Elo are mutually configurable for weakness

**Suggested unit & integration tests**
- Test StartEngine() → verify IsEngineRunning returns true
- Test mate-in-1 position → verify bestMove delivers checkmate
- Test invalid FEN → verify errorMessage populated correctly
- Test engine crash recovery → kill process, verify auto-restart
- Test evaluation mapping → known position returns expected probability
- Test different skill levels → verify approximate Elo calculation

**Possible refactors / TODOs**
- Add cancellation token support for aborting analysis mid-flight
- Implement connection pooling for multiple simultaneous analyses
- Add support for MultiPV analysis (multiple best moves)
- Cache analysis results by FEN to avoid redundant calculations
- Implement UCI option discovery (setoption enumeration)
- Add performance metrics tracking (nodes/second, time per move)
- Support for other UCI engines beyond Stockfish
- Implement opening book and endgame tablebase support

**Changelog / existing notes**
- Added separate evalDepth configuration for evaluation vs move search
- Enhanced ParseAnalysisResult() to prefer depth-matching info lines with multipv=1
- Added robust mate score parsing with isMate flag and mateDistance tracking
- Replaced CentipawnsToWinProbability() with configurable logistic mapping (PROB_K = 0.004f)
- Added skill level and approximate Elo tracking in results

**Suggested commit message when changing this file**
`fix(stockfish): [component] - [specific change description]`

**Signature**
Generated by Claude 4