# `ChessBoard.cs` — Enhanced chess board with comprehensive game management, PGN support, and analysis features

Unity chess board implementation with game tree navigation, position caching, PGN export/import, and chess variant support.

## Short description (2–4 sentences)

Implements a complete chess board with enhanced history management through game trees, comprehensive PGN notation support, and position caching for performance. Supports multiple chess variants including Chess960, King of the Hill, and Racing Kings with Zobrist hashing for efficient position comparison. Provides undo/redo functionality, threefold repetition detection, and board comparison utilities for analysis. Designed for Unity game development with serializable components and extensive testing capabilities.

## Metadata

- **Filename:** `ChessBoard.cs`
- **Primary namespace:** `GPTDeepResearch`
- **Dependent namespace:** `SPACE_UTIL` (using SPACE_UTIL;)
- **Estimated lines:** 800
- **Estimated chars:** 25,000
- **Public types:** `ChessBoard` (class), `ChessVariant` (enum), `PositionInfo` (struct), `PGNMetadata` (class), `GameTree` (class), `GameNode` (class), `BoardState` (struct), `BoardDiff` (struct), `PieceChange` (struct), `GameStatistics` (struct)
- **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
- **Dependencies:** SPACE_UTIL.v2, SPACE_UTIL.Board<T>, UnityEngine, System.Collections.Generic

## Public API summary (table)

| Type | Member | Signature | Short purpose | OneLiner Call |
|------|--------|-----------|---------------|---------------|
| ChessBoard (class) | Constructor | `public ChessBoard()` | Initialize starting position | `new ChessBoard()` |
| ChessBoard | Constructor | `public ChessBoard(string fen, ChessVariant variant = ChessVariant.Standard)` | Initialize from FEN | `new ChessBoard("startpos")` |
| ChessBoard | LoadFromFEN | `public bool LoadFromFEN(string fen)` | Load position from FEN | `board.LoadFromFEN(fen)` |
| ChessBoard | ToFEN | `public string ToFEN()` | Export position as FEN | `string fen = board.ToFEN()` |
| ChessBoard | MakeMove | `public bool MakeMove(ChessMove move, string comment = "")` | Make move and save to history | `board.MakeMove(move, "good move")` |
| ChessBoard | UndoMove | `public bool UndoMove()` | Navigate to previous position | `bool success = board.UndoMove()` |
| ChessBoard | RedoMove | `public bool RedoMove()` | Navigate forward in game tree | `bool success = board.RedoMove()` |
| ChessBoard | GoToVariation | `public bool GoToVariation(int variationIndex)` | Switch to specific variation | `board.GoToVariation(1)` |
| ChessBoard | ToPGN | `public string ToPGN(bool includeComments = true, bool includeVariations = false)` | Export game as PGN | `string pgn = board.ToPGN()` |
| ChessBoard | LoadFromPGN | `public bool LoadFromPGN(string pgnString)` | Import PGN game | `board.LoadFromPGN(pgnText)` |
| ChessBoard | CompareTo | `public BoardDiff CompareTo(ChessBoard other)` | Compare with another board | `var diff = board.CompareTo(other)` |
| ChessBoard | SearchPositions | `public List<GameNode> SearchPositions(System.Func<GameNode, bool> criteria)` | Search game tree | `var nodes = board.SearchPositions(n => n.move.IsCapture())` |
| ChessBoard | GetCapturePositions | `public List<GameNode> GetCapturePositions(char pieceType)` | Find capture positions | `var captures = board.GetCapturePositions('Q')` |
| ChessBoard | GetTacticalPositions | `public List<GameNode> GetTacticalPositions()` | Find tactical positions | `var tactical = board.GetTacticalPositions()` |
| ChessBoard | GetGameResult | `public ChessRules.GameResult GetGameResult()` | Get current game result | `var result = board.GetGameResult()` |
| ChessBoard | GetGameStatistics | `public GameStatistics GetGameStatistics()` | Get comprehensive stats | `var stats = board.GetGameStatistics()` |
| ChessBoard | CalculatePositionHash | `public ulong CalculatePositionHash()` | Get Zobrist hash | `ulong hash = board.CalculatePositionHash()` |
| ChessBoard | GetCachedPositionInfo | `public PositionInfo? GetCachedPositionInfo()` | Get cached evaluation | `var cached = board.GetCachedPositionInfo()` |
| ChessBoard | IsThreefoldRepetition | `public bool IsThreefoldRepetition()` | Check repetition rule | `bool repeated = board.IsThreefoldRepetition()` |
| ChessBoard | SetHumanSide | `public void SetHumanSide(char side)` | Set human player side | `board.SetHumanSide('w')` |
| ChessBoard | GetPiece | `public char GetPiece(string square)` | Get piece at square | `char piece = board.GetPiece("e4")` |
| ChessBoard | GetPiece | `public char GetPiece(v2 coord)` | Get piece at coordinates | `char piece = board.GetPiece(coord)` |
| ChessBoard | SetPiece | `public void SetPiece(v2 coord, char piece)` | Set piece at position | `board.SetPiece(coord, 'Q')` |
| ChessBoard | GetLegalMoves | `public List<ChessMove> GetLegalMoves()` | Generate legal moves | `var moves = board.GetLegalMoves()` |
| ChessBoard | UpdateEvaluation | `public void UpdateEvaluation(float centipawnScore, float winProbability, float mateDistance = 0f, int searchDepth = 0)` | Update position evaluation | `board.UpdateEvaluation(1.5f, 0.6f)` |
| ChessBoard | Clone | `public ChessBoard Clone()` | Create deep copy | `var copy = board.Clone()` |
| ChessBoard (static) | AlgebraicToCoord | `public static v2 AlgebraicToCoord(string square)` | Convert algebraic to coords | `v2 coord = ChessBoard.AlgebraicToCoord("e4")` |
| ChessBoard (static) | CoordToAlgebraic | `public static string CoordToAlgebraic(v2 coord)` | Convert coords to algebraic | `string square = ChessBoard.CoordToAlgebraic(coord)` |
| ChessBoard (static) | TestEnhancedFeatures | `public static void TestEnhancedFeatures()` | Test enhanced functionality | `ChessBoard.TestEnhancedFeatures()` |
| ChessBoard (static) | RunAllTests | `public static void RunAllTests()` | Run complete test suite | `ChessBoard.RunAllTests()` |
| ChessVariant (enum) | Standard | `Standard` | Standard chess rules | `ChessVariant.Standard` |
| ChessVariant | Chess960 | `Chess960` | Fischer Random variant | `ChessVariant.Chess960` |
| ChessVariant | KingOfTheHill | `KingOfTheHill` | King to center variant | `ChessVariant.KingOfTheHill` |
| ChessVariant | Atomic | `Atomic` | Atomic chess variant | `ChessVariant.Atomic` |
| ChessVariant | ThreeCheck | `ThreeCheck` | Three check variant | `ChessVariant.ThreeCheck` |
| ChessVariant | Horde | `Horde` | Horde chess variant | `ChessVariant.Horde` |
| ChessVariant | RacingKings | `RacingKings` | Racing Kings variant | `ChessVariant.RacingKings` |

## Important types — details

### `ChessBoard`
- **Kind:** class (MonoBehaviour)
- **Responsibility:** Manages chess board state, game history, and position analysis with comprehensive variant support
- **Constructor(s):** `ChessBoard()` - sets up starting position; `ChessBoard(string fen, ChessVariant variant)` - initializes from FEN
- **Public properties / fields:**
  - `board` — Board<char> — 8x8 piece representation
  - `sideToMove` — char — Current player ('w' or 'b')
  - `castlingRights` — string — Available castling (KQkq format)
  - `enPassantSquare` — string — En passant target square
  - `halfmoveClock` — int — Moves since pawn/capture
  - `fullmoveNumber` — int — Full move counter
  - `humanSide` — char — Human player side
  - `engineSide` — char — Engine player side
  - `variant` — ChessVariant — Chess variant being played

### `ChessBoard.GameTree`
- **Kind:** class (nested in ChessBoard)
- **Responsibility:** Manages branching game history with tree navigation
- **Constructor(s):** Default constructor
- **Public properties / fields:**
  - `CurrentNodeIndex` — int — Current position index
  - `NodeCount` — int — Total nodes in tree
  - `CurrentNode` — GameNode — Current game node

### `ChessBoard.GameNode`
- **Kind:** class (nested in ChessBoard)
- **Responsibility:** Represents single move in game tree with metadata
- **Constructor(s):** Default constructor
- **Public properties / fields:**
  - `state` — BoardState — Board state after move
  - `move` — ChessMove — The move made
  - `sanNotation` — string — Standard algebraic notation
  - `evaluation` — float — Position evaluation
  - `comment` — string — Move annotation

### `ChessBoard.PositionInfo`
- **Kind:** struct (nested in ChessBoard)
- **Responsibility:** Cached position data for performance optimization
- **Constructor(s):** `PositionInfo(ulong hash, float eval, float winProb, int depth)`
- **Public properties / fields:**
  - `hash` — ulong — Zobrist position hash
  - `evaluation` — float — Cached evaluation
  - `winProbability` — float — Win probability (0-1)
  - `depthSearched` — int — Analysis depth

### `ChessBoard.PGNMetadata`
- **Kind:** class (nested in ChessBoard)
- **Responsibility:** Stores PGN header information for complete game notation
- **Constructor(s):** Default constructor
- **Public properties / fields:**
  - `Event` — string — Tournament/game event
  - `Site` — string — Game location
  - `Date` — string — Game date (yyyy.MM.dd)
  - `White` — string — White player name
  - `Black` — string — Black player name
  - `Result` — string — Game result (1-0, 0-1, 1/2-1/2, *)

### `ChessBoard.BoardDiff`
- **Kind:** struct (nested in ChessBoard)
- **Responsibility:** Represents differences between two board positions
- **Constructor(s):** `BoardDiff(bool initialize = true)`
- **Public properties / fields:**
  - `changedSquares` — List<v2> — All modified squares
  - `addedPieces` — List<PieceChange> — Newly placed pieces
  - `removedPieces` — List<PieceChange> — Removed pieces
  - `sideToMoveChanged` — bool — Side to move difference

### `ChessBoard.GameStatistics`
- **Kind:** struct (nested in ChessBoard)
- **Responsibility:** Comprehensive game statistics and metrics
- **Public properties / fields:**
  - `totalMoves` — int — Total moves played
  - `captures` — int — Number of captures
  - `checks` — int — Number of checks
  - `promotions` — int — Number of promotions

## Example usage

```csharp
// Create and setup board
var board = new ChessBoard();
board.SetHumanSide('w');

// Make moves with history
var move = ChessMove.FromUCI("e2e4", board);
board.MakeMove(move, "Opening move");

// Navigate history
if (board.UndoMove()) {
    Debug.Log("Undid last move");
}

// Export/import PGN
string pgn = board.ToPGN(includeComments: true);
board.LoadFromPGN(pgnString);

// Position analysis
var stats = board.GetGameStatistics();
bool isRepetition = board.IsThreefoldRepetition();
```

## Control flow / responsibilities & high-level algorithm summary

The ChessBoard manages chess game state through a sophisticated game tree structure supporting branching variations and comprehensive history. Position management uses Zobrist hashing for efficient comparison and caching, while FEN parsing handles board setup and validation. Move execution integrates with ChessRules for validation and applies moves to both board state and game tree. PGN functionality provides complete import/export with metadata and comment preservation. The caching system optimizes repeated position analysis using LRU eviction, and variant support modifies starting positions and win conditions.

## Side effects and I/O

Modifies internal board state and game tree during move operations, updates Unity console with colored debug logging, creates temporary cache entries with automatic cleanup, modifies PGN metadata timestamps, and generates random Chess960 positions using Unity's random system. Game tree navigation triggers state restoration affecting multiple board properties.

## Performance, allocations, and hotspots

Game tree operations allocate GameNode objects for each move, PGN parsing creates StringBuilder and string arrays, position hashing uses fixed Zobrist tables with minimal allocation, cache management triggers periodic cleanup causing GC pressure, and FEN parsing involves string splitting and character validation. Game statistics calculation iterates entire move history potentially causing frame drops in long games.

## Threading / async considerations

Designed for Unity main thread usage with no explicit threading, position cache uses Dictionary without locks assuming single-threaded access, game tree modifications not thread-safe, and Zobrist key initialization uses static variables requiring careful timing. All Unity API calls (Debug.Log, Time.time) assume main thread execution.

## Security / safety / correctness concerns

FEN validation prevents malformed position loading, game tree navigation bounds-checks node indices, position cache size limits prevent memory exhaustion, PGN parsing includes input sanitization, and Zobrist hash collisions possible but statistically unlikely. Move validation depends on external ChessRules and MoveGenerator classes for correctness.

## Tests, debugging & observability

Extensive test suite via `RunAllTests()` and `TestEnhancedFeatures()` methods, comprehensive debug logging with color-coded Unity console output, position hash validation for debugging, game statistics for analysis, and built-in FEN validation testing. Performance timing available through game statistics and cache hit analysis.

## Cross-file references

- `SPACE_UTIL.v2` — 2D coordinate structure for chess squares
- `SPACE_UTIL.Board<T>` — Generic 2D board container
- `ChessMove.cs` — Move representation and UCI parsing (FromUCI, ToUCI, ToPGN, IsValid, IsCapture methods)
- `ChessRules.cs` — Chess rule validation and move application (MakeMove, EvaluatePosition methods)
- `MoveGenerator.cs` — Legal move generation (GenerateLegalMoves method)

<!-- TODO (only if i have explicitly mentioned in prompt): Consider adding persistent engine configuration, implementing move validation before engine submission, adding support for time controls and increment, implementing analysis caching for repeated positions, adding support for multiple engine instances, and considering move quality annotations based on evaluation changes. Future versions could benefit from asynchronous initialization patterns and improved error recovery strategies. -->