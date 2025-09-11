# Source: `ChessBoard.cs` — Enhanced chess board with comprehensive engine integration, validation, and testing

A comprehensive chess board implementation supporting legal move generation, promotion handling, undo/redo operations, evaluation metrics, position caching, game tree management with variations, and multiple chess variants.

## Short description (2–4 sentences)

This file implements a feature-rich chess board system designed for chess engine integration and game analysis. It provides complete game state management with FEN parsing/generation, legal move validation, position hashing using Zobrist keys, threefold repetition detection, and comprehensive move history with branching variations support. The system includes position caching for performance optimization and supports multiple chess variants including Chess960, King of the Hill, and Racing Kings.

## Metadata

* **Filename:** `ChessBoard.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `SPACE_UTIL` (v2 coordinate system, Board<T> generic board), `System`, `System.Collections.Generic`, `System.Linq`, `System.Text`, `UnityEngine`
* **Estimated lines:** 1450
* **Estimated chars:** 52000
* **Public types:** `ChessBoard (class)`, `ChessBoard.ChessVariant (enum)`, `ChessBoard.PositionInfo (struct)`, `ChessBoard.PGNMetadata (class)`, `ChessBoard.GameTree (class)`, `ChessBoard.GameNode (class)`, `ChessBoard.BoardState (struct)`
* **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
* **Dependencies:** `SPACE_UTIL.v2` (SPACE_UTIL is namespace), `SPACE_UTIL.Board<T>` (SPACE_UTIL is namespace), `ChessRules.cs`, `MoveGenerator.cs`, `ChessMove.cs`

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| Board<char> | board | public Board<char> board | 8x8 chess board state | var piece = chessBoard.board.GT(coord) |
| char | sideToMove | public char sideToMove | Current side to move (w/b) | var side = chessBoard.sideToMove |
| string | castlingRights | public string castlingRights | Castling availability string | var rights = chessBoard.castlingRights |
| string | enPassantSquare | public string enPassantSquare | En passant target square | var ep = chessBoard.enPassantSquare |
| int | halfmoveClock | public int halfmoveClock | 50-move rule counter | var half = chessBoard.halfmoveClock |
| int | fullmoveNumber | public int fullmoveNumber | Full move number | var move = chessBoard.fullmoveNumber |
| char | humanSide | public char humanSide { get; private set; } | Human player side | var human = chessBoard.humanSide |
| char | engineSide | public char engineSide { get; private set; } | Engine player side | var engine = chessBoard.engineSide |
| bool | allowSideSwitching | public bool allowSideSwitching | Allow changing sides | var allow = chessBoard.allowSideSwitching |
| ChessBoard.ChessVariant (enum) | variant | public ChessBoard.ChessVariant variant { get; private set; } | Chess variant type | var var = chessBoard.variant |
| float | LastEvaluation | public float LastEvaluation { get; private set; } | Last position evaluation | var eval = chessBoard.LastEvaluation |
| float | LastWinProbability | public float LastWinProbability { get; private set; } | Last win probability | var prob = chessBoard.LastWinProbability |
| float | LastMateDistance | public float LastMateDistance { get; private set; } | Distance to mate | var mate = chessBoard.LastMateDistance |
| int | LastEvaluationDepth | public int LastEvaluationDepth { get; private set; } | Evaluation search depth | var depth = chessBoard.LastEvaluationDepth |
| int | GameTreeNodeCount | public int GameTreeNodeCount { get; } | Game tree node count | var count = chessBoard.GameTreeNodeCount |
| int | CurrentHistoryIndex | public int CurrentHistoryIndex { get; } | Current history position | var index = chessBoard.CurrentHistoryIndex |
| void | constructor | public ChessBoard() | Default constructor with starting position | var board = new ChessBoard() |
| void | constructor | public ChessBoard(string fen, ChessBoard.ChessVariant variant = ChessBoard.ChessVariant.Standard) | Constructor with FEN and variant | var board = new ChessBoard("fen", ChessBoard.ChessVariant.Standard) |
| void | SetupStartingPosition | public void SetupStartingPosition() | Setup starting position for variant | chessBoard.SetupStartingPosition() |
| bool | LoadFromFEN | public bool LoadFromFEN(string fen) | Load position from FEN string | bool success = chessBoard.LoadFromFEN("fen") |
| string | ToFEN | public string ToFEN() | Generate FEN string from position | string fen = chessBoard.ToFEN() |
| void | SetHumanSide | public void SetHumanSide(char side) | Set human player side | chessBoard.SetHumanSide('w') |
| string | GetSideName | public string GetSideName(char side) | Get readable side name | string name = chessBoard.GetSideName('w') |
| bool | IsHumanTurn | public bool IsHumanTurn() | Check if human's turn | bool turn = chessBoard.IsHumanTurn() |
| bool | IsEngineTurn | public bool IsEngineTurn() | Check if engine's turn | bool turn = chessBoard.IsEngineTurn() |
| char | GetPiece | public char GetPiece(string square) | Get piece at algebraic square | char piece = chessBoard.GetPiece("e4") |
| char | GetPiece | public char GetPiece(v2 coord) | Get piece at coordinate | char piece = chessBoard.GetPiece(coord) |
| void | SetPiece | public void SetPiece(v2 coord, char piece) | Set piece at coordinate | chessBoard.SetPiece(coord, 'Q') |
| v2 | AlgebraicToCoord | public static v2 AlgebraicToCoord(string square) | Convert algebraic to coordinate | v2 coord = ChessBoard.AlgebraicToCoord("e4") |
| string | CoordToAlgebraic | public static string CoordToAlgebraic(v2 coord) | Convert coordinate to algebraic | string square = ChessBoard.CoordToAlgebraic(coord) |
| List<ChessMove> | GetLegalMoves | public List<ChessMove> GetLegalMoves() | Get all legal moves | var moves = chessBoard.GetLegalMoves() |
| bool | MakeMove | public bool MakeMove(ChessMove move, string comment = "") | Make move with validation | bool success = chessBoard.MakeMove(move) |
| bool | UndoMove | public bool UndoMove() | Undo last move | bool success = chessBoard.UndoMove() |
| bool | RedoMove | public bool RedoMove() | Redo next move | bool success = chessBoard.RedoMove() |
| bool | GoToVariation | public bool GoToVariation(int variationIndex) | Navigate to variation | bool success = chessBoard.GoToVariation(0) |
| bool | CanUndo | public bool CanUndo() | Check if can undo | bool can = chessBoard.CanUndo() |
| bool | CanRedo | public bool CanRedo() | Check if can redo | bool can = chessBoard.CanRedo() |
| void | UpdateEvaluation | public void UpdateEvaluation(float centipawnScore, float winProbability, float mateDistance = 0f, int searchDepth = 0) | Update position evaluation | chessBoard.UpdateEvaluation(150f, 0.65f) |
| void | ResetEvaluation | public void ResetEvaluation() | Reset evaluation to neutral | chessBoard.ResetEvaluation() |
| ChessRules.GameResult | GetGameResult | public ChessRules.GameResult GetGameResult() | Get current game result | var result = chessBoard.GetGameResult() |
| ulong | CalculatePositionHash | public ulong CalculatePositionHash() | Calculate Zobrist position hash | ulong hash = chessBoard.CalculatePositionHash() |
| ChessBoard.PositionInfo? | GetCachedPositionInfo | public ChessBoard.PositionInfo? GetCachedPositionInfo() | Get cached position data | var info = chessBoard.GetCachedPositionInfo() |
| bool | IsThreefoldRepetition | public bool IsThreefoldRepetition() | Check threefold repetition | bool rep = chessBoard.IsThreefoldRepetition() |
| ChessBoard | Clone | public ChessBoard Clone() | Create deep copy | ChessBoard clone = chessBoard.Clone() |
| void | RunAllTests | public static void RunAllTests() | Run comprehensive test suite | ChessBoard.RunAllTests() |

## Important types — details

### `ChessBoard` (class)
* **Kind:** class implementing ICloneable
* **Responsibility:** Main chess board with engine integration, move validation, history management, and position analysis
* **Constructor(s):** 
  * `public ChessBoard()` — Creates board with starting position
  * `public ChessBoard(string fen, ChessBoard.ChessVariant variant = ChessBoard.ChessVariant.Standard)` — Creates board from FEN with variant
* **Public properties / fields:** 
  * `board` — `Board<char>` — 8x8 board representation (get/set)
  * `sideToMove` — `char` — Current player ('w'/'b') (get/set)
  * `castlingRights` — `string` — Castling availability (get/set)
  * `enPassantSquare` — `string` — En passant target (get/set)
  * `halfmoveClock` — `int` — 50-move rule counter (get/set)
  * `fullmoveNumber` — `int` — Full move number (get/set)
  * `humanSide` — `char` — Human player side (get)
  * `engineSide` — `char` — Engine player side (get)
  * `allowSideSwitching` — `bool` — Allow side changes (get/set)
  * `variant` — `ChessBoard.ChessVariant` — Chess variant type (get)
  * `LastEvaluation` — `float` — Last position evaluation (get)
  * `LastWinProbability` — `float` — Last win probability (get)
  * `LastMateDistance` — `float` — Distance to mate (get)
  * `LastEvaluationDepth` — `int` — Evaluation search depth (get)
  * `GameTreeNodeCount` — `int` — Game tree node count (get)
  * `CurrentHistoryIndex` — `int` — Current history position (get)

* **Public methods:**
  * **Signature:** `public void SetupStartingPosition()`
    * **Description:** Sets up starting position based on current variant
    * **Parameters:** None
    * **Returns:** `void` — `ChessBoard.SetupStartingPosition()`
    * **Side effects / state changes:** Resets board to starting position, clears history
    * **Notes:** Handles different variants like Chess960, King of the Hill

  * **Signature:** `public bool LoadFromFEN(string fen)`
    * **Description:** Loads position from FEN notation with validation
    * **Parameters:** 
      * `fen` : `string` — FEN notation string
    * **Returns:** `bool success = ChessBoard.LoadFromFEN("fen")` — true if successful
    * **Throws:** Returns false for invalid FEN instead of throwing
    * **Side effects / state changes:** Updates all board state fields, validates piece counts
    * **Complexity / performance:** O(64) for board parsing plus validation

  * **Signature:** `public string ToFEN()`
    * **Description:** Generates FEN notation string from current position
    * **Parameters:** None
    * **Returns:** `string fen = ChessBoard.ToFEN()` — FEN notation string
    * **Side effects / state changes:** None - pure function
    * **Complexity / performance:** O(64) for board serialization

  * **Signature:** `public bool MakeMove(ChessMove move, string comment = "")`
    * **Description:** Makes move with legal validation and history storage
    * **Parameters:** 
      * `move` : `ChessMove` — Move to make
      * `comment` : `string` — Optional move comment
    * **Returns:** `bool success = ChessBoard.MakeMove(move)` — true if move was legal and made
    * **Side effects / state changes:** Updates position, switches sides, saves to game tree
    * **Complexity / performance:** O(1) move application plus legal move validation
    * **Notes:** Validates move legality before applying, generates PGN notation

  * **Signature:** `public bool UndoMove()`
    * **Description:** Reverts to previous position in game tree
    * **Parameters:** None
    * **Returns:** `bool success = ChessBoard.UndoMove()` — true if undo was possible
    * **Side effects / state changes:** Restores previous board state and evaluation
    * **Notes:** Uses game tree navigation, preserves variations

  * **Signature:** `public bool RedoMove()`
    * **Description:** Advances to next position in current variation
    * **Parameters:** None
    * **Returns:** `bool success = ChessBoard.RedoMove()` — true if redo was possible
    * **Side effects / state changes:** Advances to next position, updates state
    * **Notes:** Follows main line (first child) when multiple variations exist

  * **Signature:** `public List<ChessMove> GetLegalMoves()`
    * **Description:** Generates all legal moves for current position
    * **Parameters:** None
    * **Returns:** `List<ChessMove> moves = ChessBoard.GetLegalMoves()` — list of legal moves
    * **Complexity / performance:** O(n) where n is number of pieces, with legal validation
    * **Notes:** Uses MoveGenerator for generation, includes special moves

  * **Signature:** `public void UpdateEvaluation(float centipawnScore, float winProbability, float mateDistance = 0f, int searchDepth = 0)`
    * **Description:** Updates position evaluation metrics from engine
    * **Parameters:** 
      * `centipawnScore` : `float` — Evaluation in centipawns
      * `winProbability` : `float` — Win probability (0.0-1.0)
      * `mateDistance` : `float` — Moves to mate (0 if not mate)
      * `searchDepth` : `int` — Search depth used
    * **Returns:** `void` — `ChessBoard.UpdateEvaluation(150f, 0.65f)`
    * **Side effects / state changes:** Updates cached evaluation, triggers position caching
    * **Notes:** Clamps win probability to [0,1] range

  * **Signature:** `public ChessRules.GameResult GetGameResult()`
    * **Description:** Evaluates current position for game termination
    * **Parameters:** None
    * **Returns:** `ChessRules.GameResult result = ChessBoard.GetGameResult()` — game result enum
    * **Side effects / state changes:** None - analysis only
    * **Notes:** Includes variant-specific win conditions (King of the Hill, etc)

  * **Signature:** `public ulong CalculatePositionHash()`
    * **Description:** Calculates Zobrist hash for current position
    * **Parameters:** None
    * **Returns:** `ulong hash = ChessBoard.CalculatePositionHash()` — 64-bit position hash
    * **Complexity / performance:** O(64) for board scan plus hash calculation
    * **Notes:** Used for position caching and repetition detection

  * **Signature:** `public ChessBoard Clone()`
    * **Description:** Creates independent deep copy of board
    * **Parameters:** None
    * **Returns:** `ChessBoard clone = ChessBoard.Clone()` — independent copy
    * **Side effects / state changes:** None - creates new instance
    * **Complexity / performance:** O(1) for state copy, history not cloned

  * **Signature:** `public static void RunAllTests()`
    * **Description:** Executes comprehensive test suite for validation
    * **Parameters:** None
    * **Returns:** `void` — `ChessBoard.RunAllTests()`
    * **Side effects / state changes:** Logs test results to Unity console with color coding
    * **Notes:** Tests FEN parsing, move operations, evaluation system, advanced features

### `ChessBoard.ChessVariant` (enum)
* **Kind:** enum nested in ChessBoard
* **Responsibility:** Defines supported chess variants
* **Values:** `Standard`, `Chess960`, `KingOfTheHill`, `Atomic`, `ThreeCheck`, `Horde`, `RacingKings`

### `ChessBoard.PositionInfo` (struct)
* **Kind:** struct for position caching
* **Responsibility:** Caches position evaluation and analysis data
* **Constructor(s):** `public PositionInfo(ulong hash, float eval, float winProb, int depth)`
* **Public properties / fields:**
  * `hash` — `ulong` — Position hash (get/set)
  * `evaluation` — `float` — Cached evaluation (get/set)
  * `winProbability` — `float` — Cached win probability (get/set)
  * `depthSearched` — `int` — Search depth used (get/set)
  * `timestamp` — `float` — Cache timestamp (get/set)
  * `legalMoves` — `List<ChessMove>` — Cached legal moves (get/set)
  * `gameResult` — `ChessRules.GameResult` — Cached game result (get/set)

* **Public methods:**
  * **Signature:** `public bool IsValid()`
    * **Description:** Validates cache entry integrity
    * **Parameters:** None
    * **Returns:** `bool valid = posInfo.IsValid()` — true if valid cache entry
    * **Notes:** Checks hash and timestamp validity

### `ChessBoard.PGNMetadata` (class)
* **Kind:** class for PGN game metadata
* **Responsibility:** Stores complete PGN header information
* **Constructor(s):** `public PGNMetadata()` — Initializes with defaults
* **Public properties / fields:**
  * `Event` — `string` — Tournament/match name (get/set)
  * `Site` — `string` — Playing location (get/set)
  * `Date` — `string` — Game date (get/set)
  * `Round` — `string` — Round number (get/set)
  * `White` — `string` — White player name (get/set)
  * `Black` — `string` — Black player name (get/set)
  * `Result` — `string` — Game result (get/set)
  * `WhiteElo` — `string` — White player rating (get/set)
  * `BlackElo` — `string` — Black player rating (get/set)
  * `TimeControl` — `string` — Time control used (get/set)
  * `ECO` — `string` — Opening classification (get/set)
  * `Opening` — `string` — Opening name (get/set)

### `ChessBoard.GameTree` (class)
* **Kind:** class for game tree management
* **Responsibility:** Manages branching move history with variations
* **Constructor(s):** Implicit default constructor
* **Public properties / fields:**
  * `CurrentNodeIndex` — `int` — Current position index (get)
  * `NodeCount` — `int` — Total nodes in tree (get)
  * `CurrentNode` — `ChessBoard.GameNode` — Current tree node (get)

* **Public methods:**
  * **Signature:** `public ChessBoard.GameNode AddMove(ChessBoard.BoardState state, ChessMove move, string san, float evaluation, string comment = "")`
    * **Description:** Adds move to game tree with branching support
    * **Parameters:** 
      * `state` : `ChessBoard.BoardState` — Board state after move
      * `move` : `ChessMove` — Move made
      * `san` : `string` — Standard algebraic notation
      * `evaluation` : `float` — Position evaluation
      * `comment` : `string` — Optional move comment
    * **Returns:** `ChessBoard.GameNode node = gameTree.AddMove(state, move, san, eval)` — created node
    * **Side effects / state changes:** Adds node to tree, updates current position

  * **Signature:** `public bool GoToNode(int nodeIndex)`
    * **Description:** Navigates to specific node in tree
    * **Parameters:** 
      * `nodeIndex` : `int` — Target node index
    * **Returns:** `bool success = gameTree.GoToNode(index)` — true if navigation successful
    * **Side effects / state changes:** Changes current position in tree

### `ChessBoard.GameNode` (class)
* **Kind:** class representing single move in game tree
* **Responsibility:** Stores move data and tree navigation links
* **Constructor(s):** `public GameNode()` — Initializes empty node
* **Public properties / fields:**
  * `state` — `ChessBoard.BoardState` — Board state (get/set)
  * `move` — `ChessMove` — Move made (get/set)
  * `sanNotation` — `string` — Standard algebraic notation (get/set)
  * `evaluation` — `float` — Position evaluation (get/set)
  * `comment` — `string` — Move comment (get/set)
  * `parentIndex` — `int` — Parent node index (get/set)
  * `children` — `List<int>` — Child node indices (get/set)
  * `timestamp` — `float` — Creation timestamp (get/set)
  * `annotations` — `Dictionary<string, string>` — Additional annotations (get/set)

### `ChessBoard.BoardState` (struct)
* **Kind:** struct for complete board state snapshot
* **Responsibility:** Immutable position state for history and caching
* **Constructor(s):** `public BoardState(ChessBoard board)` — Creates from board
* **Public properties / fields:**
  * `fen` — `string` — FEN notation (get/set)
  * `sideToMove` — `char` — Current player (get/set)
  * `castlingRights` — `string` — Castling availability (get/set)
  * `enPassantSquare` — `string` — En passant target (get/set)
  * `halfmoveClock` — `int` — 50-move counter (get/set)
  * `fullmoveNumber` — `int` — Move number (get/set)
  * `timestamp` — `float` — State timestamp (get/set)
  * `evaluation` — `float` — Position evaluation (get/set)
  * `winProbability` — `float` — Win probability (get/set)
  * `mateDistance` — `float` — Mate distance (get/set)
  * `positionHash` — `ulong` — Zobrist hash (get/set)

## Example usage

```csharp
// Required namespaces:
// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using GPTDeepResearch;
// using SPACE_UTIL;

public class ExampleUsage : MonoBehaviour 
{
private void ChessBoard_Check()
    {
      // Initialize board with starting position
      var chessBoard = new ChessBoard();
      Debug.Log($"<color=white>Created board: {chessBoard.ToFEN()}</color>");

      // Load position from FEN
      var customBoard = new ChessBoard("r1bqkb1r/pppp1ppp/2n2n2/4p3/2B1P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 4 4");
      Debug.Log($"<color=white>Loaded custom position: {customBoard.ToFEN()}</color>");

      // Make moves with validation
      var e4Move = ChessMove.FromUCI("e2e4", chessBoard);
      bool moveSuccess = chessBoard.MakeMove(e4Move);
      Debug.Log($"<color=white>Move e2-e4 success: {moveSuccess}</color>");
      // Expected output: "Move e2-e4 success: True"

      // Get legal moves
      var legalMoves = chessBoard.GetLegalMoves();
      Debug.Log($"<color=white>Legal moves available: {legalMoves.Count}</color>");
      // Expected output: "Legal moves available: 20"

      // Update evaluation from engine
      chessBoard.UpdateEvaluation(15.0f, 0.52f, 0f, 12);
      Debug.Log($"<color=white>Evaluation: {chessBoard.LastEvaluation:F1}cp, WinProb: {chessBoard.LastWinProbability:F2}</color>");
      // Expected output: "Evaluation: 15.0cp, WinProb: 0.52"

      // Test undo/redo functionality
      if (chessBoard.CanUndo())
      {
        bool undoSuccess = chessBoard.UndoMove();
        Debug.Log($"<color=white>Undo success: {undoSuccess}</color>");
        // Expected output: "Undo success: True"

        if (chessBoard.CanRedo())
        {
          bool redoSuccess = chessBoard.RedoMove();
          Debug.Log($"<color=white>Redo success: {redoSuccess}</color>");
          // Expected output: "Redo success: True"
        }
      }

      // Position hashing and caching
      ulong posHash = chessBoard.CalculatePositionHash();
      Debug.Log($"<color=white>Position hash: {posHash:X}</color>");
      // Expected output: "Position hash: A1B2C3D4E5F6G7H8" (example hex)

      var cachedInfo = chessBoard.GetCachedPositionInfo();
      if (cachedInfo.HasValue)
      {
        Debug.Log($"<color=white>Cached evaluation: {cachedInfo.Value.evaluation:F1}</color>");
        // Expected output: "Cached evaluation: 15.0"
      }

      // Side management
      chessBoard.SetHumanSide('b');
      Debug.Log($"<color=white>Human side: {chessBoard.GetSideName(chessBoard.humanSide)}</color>");
      // Expected output: "Human side: Black"

      bool isHumanTurn = chessBoard.IsHumanTurn();
      bool isEngineTurn = chessBoard.IsEngineTurn();
      Debug.Log($"<color=white>Human turn: {isHumanTurn}, Engine turn: {isEngineTurn}</color>");
      // Expected output: "Human turn: False, Engine turn: True"

      // Piece access
      char piece = chessBoard.GetPiece("e4");
      Debug.Log($"<color=white>Piece at e4: {piece}</color>");
      // Expected output: "Piece at e4: P"

      // Algebraic notation conversion
      v2 coord = ChessBoard.AlgebraicToCoord("e4");
      string square = ChessBoard.CoordToAlgebraic(coord);
      Debug.Log($"<color=white>e4 coordinate: {coord}, back to algebraic: {square}</color>");
      // Expected output: "e4 coordinate: (4, 3), back to algebraic: e4"

      // Game result evaluation
      var gameResult = chessBoard.GetGameResult();
      Debug.Log($"<color=white>Game result: {gameResult}</color>");
      // Expected output: "Game result: InProgress"

      // Board cloning
      var clonedBoard = chessBoard.Clone();
      Debug.Log($"<color=white>Cloned board FEN: {clonedBoard.ToFEN()}</color>");
      // Expected output: "Cloned board FEN: rnbqkbnr/pppp1ppp/8/4p3/4P3/8/PPPP1PPP/RNBQKBNR w KQkq e6 0 2"

      // Test threefold repetition
      var repBoard = new ChessBoard();
      var moves = new string[] { "g1f3", "g8f6", "f3g1", "f6g8", "g1f3", "g8f6", "f3g1", "f6g8" };
      foreach (string uciMove in moves)
      {
        var move = ChessMove.FromUCI(uciMove, repBoard);
        repBoard.MakeMove(move);
      }
      bool isRepetition = repBoard.IsThreefoldRepetition();
      Debug.Log($"<color=white>Threefold repetition detected: {isRepetition}</color>");
      // Expected output: "Threefold repetition detected: True"

      // Chess960 variant
      var chess960Board = new ChessBoard("", ChessBoard.ChessVariant.Chess960);
      Debug.Log($"<color=white>Chess960 starting position: {chess960Board.ToFEN()}</color>");
      // Expected output: "Chess960 starting position: rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1" (or shuffled)

      // Nested type usage - PositionInfo
      var posInfo = new ChessBoard.PositionInfo(posHash, 25.5f, 0.6f, 15);
      bool isValidCache = posInfo.IsValid();
      Debug.Log($"<color=white>Position cache valid: {isValidCache}</color>");
      // Expected output: "Position cache valid: True"

      // Nested type usage - PGNMetadata
      var pgnMeta = new ChessBoard.PGNMetadata();
      pgnMeta.Event = "Test Game";
      pgnMeta.White = "Player1";
      pgnMeta.Black = "Player2";
      Debug.Log($"<color=white>PGN metadata: {pgnMeta.ToString()}</color>");
      // Expected output: "PGN metadata: PGN[Test Game at Unity Chess, 2024.01.15] Player1 vs Player2: *"

      // Run comprehensive tests
      ChessBoard.RunAllTests();
      Debug.Log("<color=white>ChessBoard test suite completed</color>");
      // Expected output: Multiple test result lines with color-coded pass/fail status
    }
}
```

## Control flow / responsibilities & high-level algorithm summary

ChessBoard orchestrates chess game state through FEN parsing, move validation via ChessRules, position hashing with Zobrist keys, and game tree management with branching support. Core algorithm: parse FEN → validate moves → update state → save to tree → calculate hash → cache evaluation.

## Side effects and I/O

File I/O: None. Unity logging via Debug.Log with color coding. Global state: position cache dictionary, Zobrist key arrays (static). Memory allocation: game tree nodes, position cache entries, move lists.

## Performance, allocations, and hotspots

Heavy operations: legal move generation O(n), position hashing O(64). Main allocations: List<ChessMove> generation, game tree nodes, dictionary entries for caching.

## Threading / async considerations

Main-thread only Unity operations. No async/await patterns. Position caching uses Dictionary without locking - single-threaded access assumed.

## Security / safety / correctness concerns

Potential nulls in FEN parsing, unhandled exceptions in move validation, unsafe array access in board operations. Zobrist keys use fixed seed for reproducibility.

## Tests, debugging & observability

Built-in comprehensive test suite via RunAllTests(). Color-coded Unity console logging throughout. Position validation, move history verification, FEN round-trip testing.

## Cross-file references

Dependencies: `ChessRules.cs` (move validation, game result evaluation), `MoveGenerator.cs` (legal move generation), `ChessMove.cs` (move representation), `SPACE_UTIL.v2` (coordinate system), `SPACE_UTIL.Board<T>` (board data structure).

## TODO / Known limitations / Suggested improvements

<!-- 
* TODO: Add support for remaining chess variants (Atomic, ThreeCheck completion)
* TODO: Implement PGN export functionality with full game tree
* TODO: Add position evaluation caching with LRU eviction policy
* TODO: Optimize move generation with piece-specific generators
* TODO: Add UCI protocol integration for engine communication
* TODO: Implement opening book lookup and ECO classification
(only if I explicitly mentioned in the prompt)
-->

## Appendix

Key private helper signatures: `ParseBoardPosition(string boardString)`, `ValidateBoardState()`, `UpdatePositionCache()`, `RestoreState(BoardState state)`. Zobrist initialization uses System.Random with fixed seed 12345 for deterministic hashing across sessions.

## General Note: important behaviors

Major functionalities: **Move Validation** (legal move checking with ChessRules integration), **Undo/Redo System** (game tree navigation with variation support), **Position Caching** (Zobrist hashing with evaluation storage), **Multi-Variant Support** (Chess960, King of the Hill, Racing Kings), **FEN Import/Export** (complete position serialization). Inferred: Game tree automatically manages branching variations when multiple moves exist from same position.

`checksum: A7F8B2E1 (v0.3)`