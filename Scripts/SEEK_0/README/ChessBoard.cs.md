# Source: `ChessBoard.cs` — Enhanced chess board with comprehensive game management, PGN support, and analysis features

## Short description
Implements a complete chess board representation with enhanced features including game tree navigation, position caching, PGN import/export, variant support, and comprehensive analysis capabilities. Serves as the core game state manager with Zobrist hashing for position comparison and threefold repetition detection.

## Metadata

* **Filename:** `ChessBoard.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `System, System.Collections.Generic, System.Linq, System.Text, UnityEngine, SPACE_UTIL`
* **Estimated lines:** 1200
* **Estimated chars:** 48000
* **Public types:** `ChessBoard (class), ChessBoard.ChessVariant (enum), ChessBoard.PositionInfo (struct), ChessBoard.PGNMetadata (class), ChessBoard.GameTree (class), ChessBoard.GameNode (class), ChessBoard.BoardState (struct), ChessBoard.BoardDiff (struct), ChessBoard.PieceChange (struct), ChessBoard.GameStatistics (struct)`
* **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
* **Dependencies:** `SPACE_UTIL.v2` (SPACE_UTIL is external namespace), `SPACE_UTIL.Board<T>` (SPACE_UTIL is external namespace), `ChessRules.cs`, `MoveGenerator.cs`, `ChessMove.cs`

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| Board<char> | board | `public Board<char> board` | Main chess board representation | `var b = chessBoard.board` |
| char | sideToMove | `public char sideToMove` | Current player to move ('w'/'b') | `char side = chessBoard.sideToMove` |
| string | castlingRights | `public string castlingRights` | Available castling rights | `string rights = chessBoard.castlingRights` |
| string | enPassantSquare | `public string enPassantSquare` | En passant target square | `string ep = chessBoard.enPassantSquare` |
| int | halfmoveClock | `public int halfmoveClock` | Halfmove clock for 50-move rule | `int clock = chessBoard.halfmoveClock` |
| int | fullmoveNumber | `public int fullmoveNumber` | Full move counter | `int moves = chessBoard.fullmoveNumber` |
| char | humanSide | `public char humanSide` | Human player side | `char human = chessBoard.humanSide` |
| char | engineSide | `public char engineSide` | Engine player side | `char engine = chessBoard.engineSide` |
| bool | allowSideSwitching | `public bool allowSideSwitching` | Allow switching player sides | `bool canSwitch = chessBoard.allowSideSwitching` |
| ChessBoard.ChessVariant (enum) | variant | `public ChessVariant variant` | Current chess variant | `var variant = chessBoard.variant` |
| void | ChessBoard | `public ChessBoard()` | Default constructor with starting position | `var board = new ChessBoard()` |
| void | ChessBoard | `public ChessBoard(string fen, ChessVariant variant = ChessVariant.Standard)` | Constructor with FEN and variant | `var board = new ChessBoard("rnbq...", ChessVariant.Chess960)` |
| void | SetupStartingPosition | `public void SetupStartingPosition()` | Initialize board to starting position | `chessBoard.SetupStartingPosition()` |
| ulong | CalculatePositionHash | `public ulong CalculatePositionHash()` | Calculate Zobrist hash for position | `ulong hash = chessBoard.CalculatePositionHash()` |
| bool | MakeMove | `public bool MakeMove(ChessMove move, string comment = "")` | Make move and save to game tree | `bool success = chessBoard.MakeMove(move, "good move")` |
| bool | UndoMove | `public bool UndoMove()` | Navigate to previous position | `bool undone = chessBoard.UndoMove()` |
| bool | RedoMove | `public bool RedoMove()` | Navigate forward in main line | `bool redone = chessBoard.RedoMove()` |
| bool | GoToVariation | `public bool GoToVariation(int variationIndex)` | Navigate to specific variation | `bool switched = chessBoard.GoToVariation(1)` |
| ChessBoard.PositionInfo? | GetCachedPositionInfo | `public PositionInfo? GetCachedPositionInfo()` | Get cached evaluation for position | `var info = chessBoard.GetCachedPositionInfo()` |
| bool | IsThreefoldRepetition | `public bool IsThreefoldRepetition()` | Check for threefold repetition | `bool isRepeat = chessBoard.IsThreefoldRepetition()` |
| string | ToPGN | `public string ToPGN(bool includeComments = true, bool includeVariations = false)` | Export game as PGN | `string pgn = chessBoard.ToPGN()` |
| bool | LoadFromPGN | `public bool LoadFromPGN(string pgnString)` | Import PGN game | `bool loaded = chessBoard.LoadFromPGN(pgnText)` |
| ChessBoard.BoardDiff | CompareTo | `public BoardDiff CompareTo(ChessBoard other)` | Compare with another board | `var diff = chessBoard.CompareTo(otherBoard)` |
| List<ChessBoard.GameNode> | SearchPositions | `public List<GameNode> SearchPositions(System.Func<GameNode, bool> criteria)` | Search positions matching criteria | `var nodes = chessBoard.SearchPositions(n => n.move.IsCapture())` |
| List<ChessBoard.GameNode> | GetCapturePositions | `public List<GameNode> GetCapturePositions(char pieceType)` | Get positions where piece captured | `var captures = chessBoard.GetCapturePositions('Q')` |
| List<ChessBoard.GameNode> | GetTacticalPositions | `public List<GameNode> GetTacticalPositions()` | Get tactical positions | `var tactics = chessBoard.GetTacticalPositions()` |
| ChessRules.GameResult | GetGameResult | `public ChessRules.GameResult GetGameResult()` | Get enhanced game result | `var result = chessBoard.GetGameResult()` |
| ChessBoard.GameStatistics | GetGameStatistics | `public ChessBoard.GameStatistics GetGameStatistics()` | Get comprehensive game stats | `var stats = chessBoard.GetGameStatistics()` |
| bool | LoadFromFEN | `public bool LoadFromFEN(string fen)` | Load position from FEN | `bool loaded = chessBoard.LoadFromFEN("rnbqkbnr/...")` |
| string | ToFEN | `public string ToFEN()` | Export position as FEN | `string fen = chessBoard.ToFEN()` |
| void | SetHumanSide | `public void SetHumanSide(char side)` | Set human player side | `chessBoard.SetHumanSide('w')` |
| string | GetSideName | `public string GetSideName(char side)` | Get readable side name | `string name = chessBoard.GetSideName('w')` |
| bool | IsHumanTurn | `public bool IsHumanTurn()` | Check if human's turn | `bool humanTurn = chessBoard.IsHumanTurn()` |
| bool | IsEngineTurn | `public bool IsEngineTurn()` | Check if engine's turn | `bool engineTurn = chessBoard.IsEngineTurn()` |
| char | GetPiece | `public char GetPiece(string square)` | Get piece at algebraic square | `char piece = chessBoard.GetPiece("e4")` |
| char | GetPiece | `public char GetPiece(v2 coord)` | Get piece at coordinate | `char piece = chessBoard.GetPiece(new v2(4, 3))` |
| void | SetPiece | `public void SetPiece(v2 coord, char piece)` | Set piece at coordinate | `chessBoard.SetPiece(new v2(4, 3), 'Q')` |
| v2 (struct) | AlgebraicToCoord | `public static v2 AlgebraicToCoord(string square)` | Convert algebraic to coordinate | `v2 coord = ChessBoard.AlgebraicToCoord("e4")` |
| string | CoordToAlgebraic | `public static string CoordToAlgebraic(v2 coord)` | Convert coordinate to algebraic | `string square = ChessBoard.CoordToAlgebraic(new v2(4, 3))` |
| List<ChessMove> | GetLegalMoves | `public List<ChessMove> GetLegalMoves()` | Get all legal moves | `var moves = chessBoard.GetLegalMoves()` |
| void | UpdateEvaluation | `public void UpdateEvaluation(float centipawnScore, float winProbability, float mateDistance = 0f, int searchDepth = 0)` | Update position evaluation | `chessBoard.UpdateEvaluation(1.5f, 0.6f, 0f, 15)` |
| void | ResetEvaluation | `public void ResetEvaluation()` | Reset evaluation to neutral | `chessBoard.ResetEvaluation()` |
| ChessBoard | Clone | `public ChessBoard Clone()` | Create deep copy of board | `ChessBoard copy = chessBoard.Clone()` |
| void | TestEnhancedFeatures | `public static void TestEnhancedFeatures()` | Test enhanced board features | `ChessBoard.TestEnhancedFeatures()` |
| void | RunAllTests | `public static void RunAllTests()` | Run comprehensive test suite | `ChessBoard.RunAllTests()` |
| void | TestFENParsing | `public static void TestFENParsing()` | Test FEN parsing functionality | `ChessBoard.TestFENParsing()` |

## Important types — details

### `ChessBoard`
* **Kind:** class
* **Responsibility:** Main chess board representation with enhanced game management, analysis, and variant support
* **Constructor(s):** 
  - `ChessBoard()` — Creates board with starting position
  - `ChessBoard(string fen, ChessVariant variant = ChessVariant.Standard)` — Creates board from FEN with variant
* **Public properties / fields:**
  - `board` — Board<char> — Main 8x8 chess board (get/set)
  - `sideToMove` — char — Current player to move (get/set)
  - `castlingRights` — string — Available castling rights (get/set)
  - `enPassantSquare` — string — En passant target square (get/set)
  - `halfmoveClock` — int — Halfmove clock for 50-move rule (get/set)
  - `fullmoveNumber` — int — Full move counter (get/set)
  - `humanSide` — char — Human player side (get/set)
  - `engineSide` — char — Engine player side (get/set)
  - `allowSideSwitching` — bool — Allow switching sides (get/set)
  - `variant` — ChessBoard.ChessVariant — Current chess variant (get/set)
* **Public methods:**
  - **Signature:** `public bool MakeMove(ChessMove move, string comment = "")`
    - **Description:** Make move and save to game tree with optional comment
    - **Parameters:** move : ChessMove — move to make, comment : string — optional comment
    - **Returns:** bool — success/failure, `bool success = chessBoard.MakeMove(move, "brilliant")`
    - **Side effects / state changes:** Updates board position, switches sides, saves to game tree
  - **Signature:** `public ulong CalculatePositionHash()`
    - **Description:** Calculate Zobrist hash for current position
    - **Returns:** ulong — position hash, `ulong hash = chessBoard.CalculatePositionHash()`
    - **Complexity / performance:** O(64) board scan
  - **Signature:** `public string ToPGN(bool includeComments = true, bool includeVariations = false)`
    - **Description:** Export complete game as PGN string
    - **Parameters:** includeComments : bool — include comments, includeVariations : bool — include variations
    - **Returns:** string — PGN representation, `string pgn = chessBoard.ToPGN()`
  - **Signature:** `public bool LoadFromPGN(string pgnString)`
    - **Description:** Import PGN game and reconstruct position history
    - **Parameters:** pgnString : string — PGN text to parse
    - **Returns:** bool — parsing success, `bool loaded = chessBoard.LoadFromPGN(pgnText)`
    - **Throws:** Exception on invalid PGN format
  - **Signature:** `public ChessBoard.BoardDiff CompareTo(ChessBoard other)`
    - **Description:** Compare boards and return detailed differences
    - **Parameters:** other : ChessBoard — board to compare against
    - **Returns:** ChessBoard.BoardDiff — detailed differences, `var diff = board1.CompareTo(board2)`

### `ChessBoard.ChessVariant`
* **Kind:** enum (inside ChessBoard)
* **Responsibility:** Supported chess variants
* **Values:** Standard, Chess960, KingOfTheHill, Atomic, ThreeCheck, Horde, RacingKings

### `ChessBoard.PositionInfo`
* **Kind:** struct (inside ChessBoard)
* **Responsibility:** Cached position evaluation data
* **Constructor(s):** `PositionInfo(ulong hash, float eval, float winProb, int depth)`
* **Public properties / fields:**
  - `hash` — ulong — Position hash (get/set)
  - `evaluation` — float — Centipawn evaluation (get/set)
  - `winProbability` — float — Win probability 0-1 (get/set)
  - `depthSearched` — int — Search depth (get/set)
  - `timestamp` — float — Cache timestamp (get/set)
  - `legalMoves` — List<ChessMove> — Cached legal moves (get/set)
  - `gameResult` — ChessRules.GameResult — Game termination state (get/set)

### `ChessBoard.PGNMetadata`
* **Kind:** class (inside ChessBoard)
* **Responsibility:** PGN header information storage
* **Constructor(s):** `PGNMetadata()` — Initializes with default values
* **Public properties / fields:**
  - `Event` — string — Tournament/match name (get/set)
  - `Site` — string — Location (get/set)
  - `Date` — string — Game date (get/set)
  - `Round` — string — Round number (get/set)
  - `White` — string — White player name (get/set)
  - `Black` — string — Black player name (get/set)
  - `Result` — string — Game result (get/set)
  - `WhiteElo` — string — White player rating (get/set)
  - `BlackElo` — string — Black player rating (get/set)
  - `TimeControl` — string — Time control (get/set)
  - `ECO` — string — Opening code (get/set)
  - `Opening` — string — Opening name (get/set)

### `ChessBoard.GameTree`
* **Kind:** class (inside ChessBoard)
* **Responsibility:** Enhanced game history with branching support
* **Public properties / fields:**
  - `CurrentNodeIndex` — int — Current position index (get)
  - `NodeCount` — int — Total nodes in tree (get)
  - `CurrentNode` — ChessBoard.GameNode — Current position node (get)
* **Public methods:**
  - **Signature:** `public GameNode AddMove(BoardState state, ChessMove move, string san, float evaluation, string comment = "")`
    - **Description:** Add move to game tree with evaluation
    - **Parameters:** state : BoardState — board state, move : ChessMove — move made, san : string — algebraic notation, evaluation : float — position eval, comment : string — optional comment
    - **Returns:** ChessBoard.GameNode — created node, `var node = gameTree.AddMove(state, move, "e4", 0.3f)`
  - **Signature:** `public bool GoToNode(int nodeIndex)`
    - **Description:** Navigate to specific node in tree
    - **Parameters:** nodeIndex : int — target node index
    - **Returns:** bool — navigation success, `bool moved = gameTree.GoToNode(5)`

### `ChessBoard.GameNode`
* **Kind:** class (inside ChessBoard)
* **Responsibility:** Single position in game tree with move and analysis data
* **Constructor(s):** `GameNode()` — Initialize empty node
* **Public properties / fields:**
  - `state` — ChessBoard.BoardState — Complete board state (get/set)
  - `move` — ChessMove — Move that led to position (get/set)
  - `sanNotation` — string — Standard algebraic notation (get/set)
  - `evaluation` — float — Position evaluation (get/set)
  - `comment` — string — User/engine comment (get/set)
  - `parentIndex` — int — Parent node index (get/set)
  - `children` — List<int> — Child node indices (get/set)
  - `timestamp` — float — Creation time (get/set)
  - `annotations` — Dictionary<string, string> — Additional annotations (get/set)

### `ChessBoard.BoardState`
* **Kind:** struct (inside ChessBoard)
* **Responsibility:** Complete board state snapshot with evaluation
* **Constructor(s):** `BoardState(ChessBoard board)` — Create from current board
* **Public properties / fields:**
  - `fen` — string — FEN representation (get/set)
  - `sideToMove` — char — Current player (get/set)
  - `castlingRights` — string — Castling availability (get/set)
  - `enPassantSquare` — string — En passant target (get/set)
  - `halfmoveClock` — int — Halfmove counter (get/set)
  - `fullmoveNumber` — int — Fullmove number (get/set)
  - `timestamp` — float — State creation time (get/set)
  - `evaluation` — float — Position evaluation (get/set)
  - `winProbability` — float — Win probability (get/set)
  - `mateDistance` — float — Moves to mate (get/set)
  - `positionHash` — ulong — Zobrist hash (get/set)

### `ChessBoard.BoardDiff`
* **Kind:** struct (inside ChessBoard)
* **Responsibility:** Detailed comparison result between two boards
* **Constructor(s):** `BoardDiff(bool initialize = true)` — Initialize diff structure
* **Public properties / fields:**
  - `changedSquares` — List<v2> — Squares that changed (get/set)
  - `addedPieces` — List<ChessBoard.PieceChange> — Pieces added (get/set)
  - `removedPieces` — List<ChessBoard.PieceChange> — Pieces removed (get/set)
  - `changedPieces` — List<ChessBoard.PieceChange> — Pieces changed (get/set)
  - `sideToMoveChanged` — bool — Side to move different (get/set)
  - `castlingRightsChanged` — bool — Castling rights different (get/set)
  - `enPassantChanged` — bool — En passant square different (get/set)

### `ChessBoard.PieceChange`
* **Kind:** struct (inside ChessBoard)
* **Responsibility:** Single piece change information
* **Public properties / fields:**
  - `square` — v2 — Square coordinate (get/set)
  - `piece` — char — Piece character (get/set)

### `ChessBoard.GameStatistics`
* **Kind:** struct (inside ChessBoard)
* **Responsibility:** Comprehensive game statistics
* **Public properties / fields:**
  - `totalMoves` — int — Total moves played (get/set)
  - `captures` — int — Number of captures (get/set)
  - `checks` — int — Number of checks (get/set)
  - `castling` — int — Castling moves (get/set)
  - `promotions` — int — Promotion moves (get/set)
  - `averageThinkTime` — float — Average thinking time (get/set)
  - `longestThink` — float — Longest think time (get/set)

## Example usage

```csharp
// namespace GPTDeepResearch required
using GPTDeepResearch;
using SPACE_UTIL;

// Create new game
var board = new ChessBoard();

// Make moves with evaluation
var move = ChessMove.FromUCI("e2e4", board);
board.MakeMove(move, "King's pawn opening");
board.UpdateEvaluation(0.3f, 0.52f, 0f, 12);

// Export to PGN
string pgn = board.ToPGN();

// Navigate history
board.UndoMove();
board.RedoMove();

// Position analysis
ulong hash = board.CalculatePositionHash();
bool isRepeat = board.IsThreefoldRepetition();
var tactics = board.GetTacticalPositions();
```

## Control flow / responsibilities & high-level algorithm summary

Enhanced chess board manages complete game state with Zobrist hashing for position comparison, game tree for move history with branching, PGN import/export, position caching for performance, and comprehensive analysis tools.

## Performance, allocations, and hotspots / Threading / async considerations

Heavy operations: PGN parsing, position hashing O(64), cache management. Main-thread only, no async usage.

## Security / safety / correctness concerns

FEN parsing validates board state, null checks for move validation, exception handling in PGN import.

## Tests, debugging & observability

Comprehensive test suite with `RunAllTests()`, extensive debug logging for move validation and PGN processing, position validation methods.

## Cross-file references

Depends on `SPACE_UTIL.v2`, `SPACE_UTIL.Board<T>`, `ChessRules.cs`, `MoveGenerator.cs`, `ChessMove.cs` for complete chess engine functionality.

<!-- ## TODO / Known limitations / Suggested improvements

* Enhanced threefold repetition using complete position history
* Chess960 castling validation improvements  
* Performance optimization for large game trees
* Multi-threading support for analysis functions
* Memory management for position cache
* Extended variant support (Crazyhouse, Suicide) -->

## Appendix

Key private helpers: `ParseBoardPosition()`, `ValidateBoardState()`, `InitializeZobristKeys()`, `UpdatePositionCache()`, `GenerateChess960Position()`. Zobrist hashing uses fixed seed for consistency.

## General Note: important behaviors

Major functionalities: Complete PGN support, game tree navigation with variations, position caching with Zobrist hashing, comprehensive game statistics, chess variant support, threefold repetition detection.

`checksum: A7B3C4E2`