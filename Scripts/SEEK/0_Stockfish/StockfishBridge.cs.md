# StockfishBridge

**Summary**  
`StockfishBridge` is a Unity `MonoBehaviour` that wraps a Stockfish (UCI) engine as a non-blocking, coroutine-first bridge. It runs the engine in a background process/thread, parses UCI output, exposes a coroutine API for analysis requests, and returns a structured `ChessAnalysisResult` containing best move, evaluation (as win probabilities), depth, and debug output.

**Key features**
- Starts/stops Stockfish process and manages reader thread.
- Coroutine-based analysis (`AnalyzePositionCoroutine`) that yields until engine returns a `bestmove`.
- Parses `info` lines to produce:
  - `bestMove` (e.g. `"e2e4"`, or `"check-mate"`, `"stale-mate"`, or error messages)
  - `evaluation` (0..1 = probability WHITE wins)
  - `stmEvaluation` (probability for side-to-move)
  - `isGameEnd`, depths, raw engine output, skill level / approx Elo
- Crash detection and automatic restart helper (`RestartEngineCoroutine`).
- Options to limit engine strength (Elo, Skill Level) and separate depths for move-search vs evaluation.
- Debug logging and `OnEngineLine` UnityEvent for live engine lines.

**Public API (most useful members)**
- `void StartEngine()`
- `void StopEngine()`
- `IEnumerator InitializeEngineCoroutine()`
- `IEnumerator RestartEngineCoroutine()`
- `IEnumerator AnalyzePositionCoroutine(string fen, int movetimeMs = 2000, int searchDepth = 1, int evaluationDepth = 5, int elo = 400, int skillLevel = 0)`
- `void SendCommand(string command)`
- `UnityEvent<string> OnEngineLine` — subscribe to raw engine lines.
- Properties: `LastAnalysisResult`, `LastRawOutput`, `IsEngineRunning`, `IsReady`

**Result structure**
`ChessAnalysisResult` (serializable)
- `bestMove` (string)
- `Side` (`'w'`/`'b'`)
- `evaluation` (float 0..1 for White)
- `stmEvaluation` (float 0..1 for side-to-move)
- `isGameEnd` (bool)
- `rawEngineOutput` (string)
- `searchDepth`, `evaluationDepth`, `skillLevel`, `approximateElo`, `errorMessage`

**Usage example**
```csharp
// Start engine (e.g. in Awake)
stockfishBridge.StartEngine();
StartCoroutine(stockfishBridge.InitializeEngineCoroutine());

// Run analysis (example)
StartCoroutine(RunAnalysis());

private IEnumerator RunAnalysis() {
  yield return StartCoroutine(stockfishBridge.AnalyzePositionCoroutine(
    "r1bqkbnr/pppppppp/n7/8/8/N7/PPPPPPPP/R1BQKBNR w KQkq - 0 1",
    movetimeMs: 1000,
    searchDepth: 10,
    evaluationDepth: 12,
    elo: 1500,
    skillLevel: 0
  ));
  var result = stockfishBridge.LastAnalysisResult;
  Debug.Log(result.ToString());
}
