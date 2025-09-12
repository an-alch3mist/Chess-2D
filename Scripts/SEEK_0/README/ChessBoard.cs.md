# Source: `ChessBoard.cs` — Enhanced chess board with comprehensive engine integration, validation, and testing

Enhanced chess board implementation supporting legal move generation, promotion handling, undo/redo functionality, evaluation metrics, and multiple chess variants with comprehensive validation modes.

## Short description (2–4 sentences)

This file implements a comprehensive chess board system with engine integration support, featuring FEN parsing/generation, move validation, game tree management with branching history, position caching using Zobrist hashing, and support for multiple chess variants. It provides extensive validation modes from strict rule enforcement to permissive testing scenarios, comprehensive evaluation tracking, and a full testing suite for API validation.

## Metadata

* **Filename:** `ChessBoard.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `using SPACE_UTIL;` (external namespace), `using System;`, `using System.Collections.Generic;`, `using System.Linq;`, `using System.Text;`, `using UnityEngine;`
* **Estimated lines:** 1800
* **Estimated chars:** 90000
* **Public types:** `ChessBoard (class)`, `ChessBoard.ChessVariant (enum)`, `ChessBoard.ValidationMode (enum)`, `ChessBoard.PositionInfo (struct)`, `ChessBoard.PGNMetadata (class)`, `ChessBoard.GameTree (class)`, `ChessBoard.GameNode (class)`, `ChessBoard.BoardState (struct)`
* **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
* **Dependencies:** SPACE_UTIL.v2 (SPACE_UTIL is namespace), SPACE_UTIL.Board<T> (SPACE_UTIL is namespace), ChessRules (referenced but not defined), MoveGenerator (referenced but not defined), ChessMove (referenced but not defined)

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| Board<char> | board | `public Board<char> board` | 8x8 chess board representation | `var piece = chessBoard.board.GT(coord);` |
| char | sideToMove | `public char sideToMove` | Current player turn ('w' or 'b') | `var turn = chessBoard.sideToMove;` |
| string | castlingRights | `public string castlingRights` | Castling availability (KQkq format) | `var rights = chessBoard.castlingRights;` |
| string | enPassantSquare | `public string enPassantSquare` | En passant target square | `var ep = chessBoard.enPassantSquare;` |
| int | halfmoveClock | `public int halfmoveClock` | Halfmove clock for 50-move rule | `var half = chessBoard.halfmoveClock;` |
| int | fullmoveNumber | `public int fullmoveNumber` | Full move counter | `var full = chessBoard.fullmoveNumber;` |
| char | humanSide | `public char humanSide { get; private set; }` | Human player side | `var human = chessBoard.humanSide;` |
| char | engineSide | `public char engineSide { get; private set; }` | Engine player side | `var engine = chessBoard.engineSide;` |
| bool | allowSideSwitching | `public bool allowSideSwitching` | Allow changing player sides | `var allow = chessBoard.allowSideSwitching;` |
| ChessBoard.ChessVariant | variant | `public ChessBoard.ChessVariant variant { get; private set; }` | Chess variant being played | `var var = chessBoard.variant;` |
| float | LastEvaluation | `public float LastEvaluation { get; private set; }` | Last position evaluation score | `var eval = chessBoard.LastEvaluation;` |
| float | LastWinProbability | `public float LastWinProbability { get; private set; }` | Last win probability (0-1) | `var prob = chessBoard.LastWinProbability;` |
| float | LastMateDistance | `public float LastMateDistance { get; private set; }` | Distance to mate in plies | `var mate = chessBoard.LastMateDistance;` |
| int | LastEvaluationDepth | `public int LastEvaluationDepth { get; private set; }` | Search depth of last evaluation | `var depth = chessBoard.LastEvaluationDepth;` |
| List<ChessBoard.GameNode> | LogGameTreeNodes | `public List<ChessBoard.GameNode> LogGameTreeNodes { get; }` | Game tree nodes for logging | `var nodes = chessBoard.LogGameTreeNodes;` |
| int | GameTreeNodeCount | `public int GameTreeNodeCount { get; }` | Total nodes in game tree | `var count = chessBoard.GameTreeNodeCount;` |
| int | CurrentHistoryIndex | `public int CurrentHistoryIndex { get; }` | Current position in game tree | `var index = chessBoard.CurrentHistoryIndex;` |
| ChessBoard.ValidationMode | CurrentValidationMode | `public ChessBoard.ValidationMode CurrentValidationMode { get; }` | Current FEN validation mode | `var mode = chessBoard.CurrentValidationMode;` |
| void | ChessBoard() | `public ChessBoard()` | Default constructor with starting position | `var board = new ChessBoard();` |
| void | ChessBoard(string, ChessBoard.ChessVariant, ChessBoard.ValidationMode, bool) | `public ChessBoard(string fen, ChessBoard.ChessVariant variant = ChessBoard.ChessVariant.Standard, ChessBoard.ValidationMode validation = ChessBoard.ValidationMode.Permissive, bool fallbackOnFailure = false)` | Constructor with FEN and options | `var board = new ChessBoard(fen, ChessBoard.ChessVariant.Standard, ChessBoard.ValidationMode.Strict, true);` |
| void | SetValidationMode | `public void SetValidationMode(ChessBoard.ValidationMode mode, bool fallback = false)` | Set FEN validation mode | `chessBoard.SetValidationMode(ChessBoard.ValidationMode.Strict, true);` |
| void | SetupStartingPosition | `public void SetupStartingPosition()` | Reset to variant starting position | `chessBoard.SetupStartingPosition();` |
| ulong | CalculatePositionHash | `public ulong CalculatePositionHash()` | Calculate Zobrist position hash | `ulong hash = chessBoard.CalculatePositionHash();` |
| bool | MakeMove | `public bool MakeMove(ChessMove move, string comment = "")` | Make a move on the board | `bool success = chessBoard.MakeMove(move, "good move");` |
| bool | UndoMove | `public bool UndoMove()` | Undo last move | `bool undone = chessBoard.UndoMove();` |
| bool | RedoMove | `public bool RedoMove()` | Redo next move in history | `bool redone = chessBoard.RedoMove();` |
| bool | GoToVariation | `public bool GoToVariation(int variationIndex)` | Navigate to variation branch | `bool moved = chessBoard.GoToVariation(0);` |
| bool | CanUndo | `public bool CanUndo()` | Check if undo is possible | `bool canUndo = chessBoard.CanUndo();` |
| bool | CanRedo | `public bool CanRedo()` | Check if redo is possible | `bool canRedo = chessBoard.CanRedo();` |
| ChessBoard.PositionInfo? | GetCachedPositionInfo | `public ChessBoard.PositionInfo? GetCachedPositionInfo()` | Get cached evaluation data | `var cached = chessBoard.GetCachedPositionInfo();` |
| bool | IsThreefoldRepetition | `public bool IsThreefoldRepetition()` | Check for threefold repetition | `bool rep = chessBoard.IsThreefoldRepetition();` |
| bool | LoadFromFEN | `public bool LoadFromFEN(string fen)` | Load position from FEN string | `bool loaded = chessBoard.LoadFromFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");` |
| string | ToFEN | `public string ToFEN()` | Generate FEN string from position | `string fen = chessBoard.ToFEN();` |
| void | SetHumanSide | `public void SetHumanSide(char side)` | Set human player side | `chessBoard.SetHumanSide('w');` |
| string | GetSideName | `public string GetSideName(char side)` | Get readable side name | `string name = chessBoard.GetSideName('w');` |
| bool | IsHumanTurn | `public bool IsHumanTurn()` | Check if human's turn | `bool humanTurn = chessBoard.IsHumanTurn();` |
| bool | IsEngineTurn | `public bool IsEngineTurn()` | Check if engine's turn | `bool engineTurn = chessBoard.IsEngineTurn();` |
| char | GetPiece(string) | `public char GetPiece(string square)` | Get piece at algebraic square | `char piece = chessBoard.GetPiece("e4");` |
| char | GetPiece(v2) | `public char GetPiece(v2 coord)` | Get piece at coordinate | `char piece = chessBoard.GetPiece(coord);` |
| void | SetPiece | `public void SetPiece(v2 coord, char piece)` | Set piece at coordinate | `chessBoard.SetPiece(coord, 'Q');` |
| v2 | AlgebraicToCoord | `public static v2 AlgebraicToCoord(string square)` | Convert algebraic to coordinate | `v2 coord = ChessBoard.AlgebraicToCoord("e4");` |
| string | CoordToAlgebraic | `public static string CoordToAlgebraic(v2 coord)` | Convert coordinate to algebraic | `string square = ChessBoard.CoordToAlgebraic(coord);` |
| List<ChessMove> | GetLegalMoves | `public List<ChessMove> GetLegalMoves()` | Get all legal moves | `var moves = chessBoard.GetLegalMoves();` |
| void | UpdateEvaluation | `public void UpdateEvaluation(float centipawnScore, float winProbability, float mateDistance = 0f, int searchDepth = 0)` | Update position evaluation | `chessBoard.UpdateEvaluation(150.5f, 0.65f, 0f, 12);` |
| void | ResetEvaluation | `public void ResetEvaluation()` | Reset evaluation to neutral | `chessBoard.ResetEvaluation();` |
| ChessRules.GameResult | GetGameResult | `public ChessRules.GameResult GetGameResult()` | Get current game result | `var result = chessBoard.GetGameResult();` |
| ChessBoard | Clone | `public ChessBoard Clone()` | Create deep copy of board | `var copy = chessBoard.Clone();` |
| object | Clone | `object ICloneable.Clone()` | ICloneable implementation | `var copy = ((ICloneable)chessBoard).Clone();` |
| string | ToString | `public override string ToString()` | String representation for debugging | `string info = chessBoard.ToString();` |
| void | RunAllTests | `public static void RunAllTests()` | Run comprehensive test suite | `ChessBoard.RunAllTests();` |

## Important types — details

### `ChessBoard` (class)
* **Kind:** class implementing ICloneable
* **Responsibility:** Main chess board with comprehensive game state management, move validation, evaluation tracking, and engine integration support.
* **Constructor(s):** 
  * `ChessBoard()` - Creates board with starting position
  * `ChessBoard(string fen, ChessBoard.ChessVariant variant, ChessBoard.ValidationMode validation, bool fallbackOnFailure)` - Creates board from FEN with validation options
* **Public properties / fields:**
  * `board — Board<char> — 8x8 chess board grid`
  * `sideToMove — char — Current player ('w' or 'b')`
  * `castlingRights — string — Castling availability (KQkq format)`
  * `enPassantSquare — string — En passant target square`
  * `halfmoveClock — int — Halfmove clock for 50-move rule`
  * `fullmoveNumber — int — Full move number`
  * `humanSide — char { get; private set; } — Human player side`
  * `engineSide — char { get; private set; } — Engine player side`
  * `allowSideSwitching — bool — Allow changing player sides`
  * `variant — ChessBoard.ChessVariant { get; private set; } — Chess variant`
  * `LastEvaluation — float { get; private set; } — Last position evaluation`
  * `LastWinProbability — float { get; private set; } — Last win probability`
  * `LastMateDistance — float { get; private set; } — Distance to mate`
  * `LastEvaluationDepth — int { get; private set; } — Evaluation search depth`
  * `LogGameTreeNodes — List<ChessBoard.GameNode> { get; } — Game tree nodes`
  * `GameTreeNodeCount — int { get; } — Total game tree nodes`
  * `CurrentHistoryIndex — int { get; } — Current history position`
  * `CurrentValidationMode — ChessBoard.ValidationMode { get; } — Current validation mode`

* **Public methods:**
  * **Signature:** `public bool LoadFromFEN(string fen)`
    * **Description:** Load chess position from FEN string with validation.
    * **Parameters:** 
      * fen : string — FEN notation string
    * **Returns:** bool — True if loaded successfully: `bool success = chessBoard.LoadFromFEN(fenString);`
    * **Throws:** No exceptions, returns false on invalid FEN
    * **Side effects / state changes:** Updates entire board state, resets evaluation
    * **Complexity / performance:** O(1) with string parsing overhead
    * **Notes:** Respects current validation mode settings

  * **Signature:** `public string ToFEN()`
    * **Description:** Generate FEN string from current position.
    * **Parameters:** None
    * **Returns:** string — FEN notation: `string fen = chessBoard.ToFEN();`
    * **Throws:** No exceptions, returns starting position FEN on error
    * **Side effects / state changes:** None
    * **Complexity / performance:** O(1) string construction
    * **Notes:** Always generates valid FEN format

  * **Signature:** `public bool MakeMove(ChessMove move, string comment = "")`
    * **Description:** Make a legal move and save to game tree with PGN notation.
    * **Parameters:** 
      * move : ChessMove — Chess move to execute
      * comment : string — Optional move annotation
    * **Returns:** bool — True if move was legal and made: `bool success = chessBoard.MakeMove(move, "excellent");`
    * **Throws:** No exceptions, validates move legality first
    * **Side effects / state changes:** Updates board state, adds to game tree, advances turn
    * **Complexity / performance:** O(1) move execution, O(k) legal move validation
    * **Notes:** Automatically generates SAN notation for move history

  * **Signature:** `public bool UndoMove()`
    * **Description:** Navigate to previous position in game tree.
    * **Parameters:** None
    * **Returns:** bool — True if undo successful: `bool undone = chessBoard.UndoMove();`
    * **Throws:** No exceptions, returns false if at start
    * **Side effects / state changes:** Restores previous board state
    * **Complexity / performance:** O(1) state restoration
    * **Notes:** Uses game tree for efficient undo

  * **Signature:** `public ulong CalculatePositionHash()`
    * **Description:** Calculate Zobrist hash for current position.
    * **Parameters:** None
    * **Returns:** ulong — Position hash for caching: `ulong hash = chessBoard.CalculatePositionHash();`
    * **Throws:** No exceptions, returns 0 on error
    * **Side effects / state changes:** None
    * **Complexity / performance:** O(1) hash calculation
    * **Notes:** Uses Zobrist hashing for fast position comparison

  * **Signature:** `public void UpdateEvaluation(float centipawnScore, float winProbability, float mateDistance = 0f, int searchDepth = 0)`
    * **Description:** Update position evaluation data with validation.
    * **Parameters:** 
      * centipawnScore : float — Evaluation in centipawns
      * winProbability : float — Win probability (0-1)
      * mateDistance : float — Distance to mate in plies
      * searchDepth : int — Engine search depth
    * **Returns:** void: `chessBoard.UpdateEvaluation(150.5f, 0.65f, 0f, 12);`
    * **Throws:** No exceptions, clamps values to valid ranges
    * **Side effects / state changes:** Updates evaluation cache, logs evaluation
    * **Complexity / performance:** O(1) with cache update
    * **Notes:** Clamps win probability to [0,1] range

  * **Signature:** `public ChessBoard Clone()`
    * **Description:** Create deep copy of chess board with all state.
    * **Parameters:** None
    * **Returns:** ChessBoard — Independent copy: `ChessBoard copy = chessBoard.Clone();`
    * **Throws:** No exceptions, returns new board on error
    * **Side effects / state changes:** None
    * **Complexity / performance:** O(1) with FEN conversion overhead
    * **Notes:** Preserves all settings and evaluation state

  * **Signature:** `public static void RunAllTests()`
    * **Description:** Run comprehensive test suite for all ChessBoard functionality.
    * **Parameters:** None
    * **Returns:** void: `ChessBoard.RunAllTests();`
    * **Throws:** No exceptions, logs test results to Debug
    * **Side effects / state changes:** Creates temporary test objects, extensive logging
    * **Complexity / performance:** O(n) where n is number of test cases
    * **Notes:** Tests FEN parsing, moves, evaluation, validation modes

### `ChessBoard.ChessVariant` (enum)
* **Kind:** enum
* **Responsibility:** Defines supported chess variants with different rules and starting positions.
* **Values:**
  * `Standard — Standard chess rules`
  * `Chess960 — Fischer Random Chess with random starting positions`
  * `KingOfTheHill — Win by moving king to center squares`
  * `Atomic — Captures cause explosions`
  * `ThreeCheck — Win by giving check three times`
  * `Horde — Asymmetric variant with pawn army`
  * `RacingKings — Race kings to eighth rank`

### `ChessBoard.ValidationMode` (enum)
* **Kind:** enum
* **Responsibility:** Controls FEN validation strictness for testing and engine integration.
* **Values:**
  * `Strict — Must be legal chess position`
  * `Permissive — Allow rule violations for testing`
  * `ParseOnly — Only check FEN syntax`

### `ChessBoard.PositionInfo` (struct)
* **Kind:** struct
* **Responsibility:** Cached position information for performance optimization.
* **Constructor(s):** `PositionInfo(ulong hash, float eval, float winProb, int depth)`
* **Public properties / fields:**
  * `hash — ulong — Position Zobrist hash`
  * `evaluation — float — Cached evaluation score`
  * `winProbability — float — Cached win probability`
  * `depthSearched — int — Search depth used`
  * `timestamp — float — When cached (Unity time)`
  * `legalMoves — List<ChessMove> — Cached legal moves`
  * `gameResult — ChessRules.GameResult — Cached game result`

* **Public methods:**
  * **Signature:** `public bool IsValid()`
    * **Description:** Check if cached data is valid and not expired.
    * **Parameters:** None
    * **Returns:** bool — True if cache entry valid: `bool valid = posInfo.IsValid();`

### `ChessBoard.PGNMetadata` (class)
* **Kind:** class
* **Responsibility:** PGN metadata for complete game notation and export.
* **Constructor(s):** `PGNMetadata()` - Initialize with default values
* **Public properties / fields:**
  * `Event — string { get; set; } — Tournament or event name`
  * `Site — string { get; set; } — Location of game`
  * `Date — string { get; set; } — Game date in YYYY.MM.DD format`
  * `Round — string { get; set; } — Round number`
  * `White — string { get; set; } — White player name`
  * `Black — string { get; set; } — Black player name`
  * `Result — string { get; set; } — Game result (* for ongoing)`
  * `WhiteElo — string { get; set; } — White player rating`
  * `BlackElo — string { get; set; } — Black player rating`
  * `TimeControl — string { get; set; } — Time control settings`
  * `ECO — string { get; set; } — Encyclopedia of Chess Openings code`
  * `Opening — string { get; set; } — Opening name`

### `ChessBoard.GameTree` (class)
* **Kind:** class  
* **Responsibility:** Enhanced game tree for branching move history with variations.
* **Constructor(s):** Default constructor
* **Public properties / fields:**
  * `GetNodes — List<ChessBoard.GameNode> { get; } — All nodes for logging`
  * `CurrentNodeIndex — int { get; } — Current position index`
  * `NodeCount — int { get; } — Total nodes in tree`
  * `CurrentNode — ChessBoard.GameNode { get; } — Current node or null`

* **Public methods:**
  * **Signature:** `public ChessBoard.GameNode AddMove(ChessBoard.BoardState state, ChessMove move, string san, float evaluation, string comment = "")`
    * **Description:** Add move to current position, creating new branch if necessary.
    * **Parameters:** 
      * state : ChessBoard.BoardState — Board state after move
      * move : ChessMove — Chess move made
      * san : string — Standard algebraic notation
      * evaluation : float — Position evaluation
      * comment : string — Move comment
    * **Returns:** ChessBoard.GameNode — Created node: `var node = gameTree.AddMove(state, move, "Nf3", 0.2f, "book");`

### `ChessBoard.GameNode` (class)
* **Kind:** class
* **Responsibility:** Game tree node with enhanced data for analysis.
* **Constructor(s):** Default constructor
* **Public properties / fields:**
  * `state — ChessBoard.BoardState — Board state at this position`
  * `move — ChessMove — Move that led to this position`
  * `sanNotation — string — Standard algebraic notation`
  * `evaluation — float — Position evaluation`
  * `comment — string — Move annotation`
  * `parentIndex — int — Parent node index`
  * `children — List<int> — Child node indices`
  * `timestamp — float — When node created`
  * `annotations — Dictionary<string, string> — Analysis annotations`

### `ChessBoard.BoardState` (struct)
* **Kind:** struct
* **Responsibility:** Enhanced board state with position hashing for history.
* **Constructor(s):** `BoardState(ChessBoard board)` - Create from ChessBoard
* **Public properties / fields:**
  * `fen — string — FEN representation`
  * `sideToMove — char — Current player turn`
  * `castlingRights — string — Castling availability`
  * `enPassantSquare — string — En passant target`
  * `halfmoveClock — int — Halfmove counter`
  * `fullmoveNumber — int — Full move number`
  * `timestamp — float — When state created`
  * `evaluation — float — Position evaluation`
  * `winProbability — float — Win probability`
  * `mateDistance — float — Distance to mate`
  * `positionHash — ulong — Zobrist position hash`

## MonoBehaviour Detection and Special Rules

The `ChessBoard` class is **NOT** a MonoBehaviour - it's a standard C# class that implements `ICloneable`. It uses standard instantiation patterns with `new ChessBoard()` constructors and does not inherit from `UnityEngine.MonoBehaviour`.

## Example Usage Coverage Requirements

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
        // Standard instantiation and basic setup
        var board = new ChessBoard();
        Debug.Log($"<color=green>Created board: {board.ToString()}</color>");
        
        // Load position from FEN with validation
        string testFEN = "r1bqkbnr/pppppppp/2n5/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 2";
        bool loaded = board.LoadFromFEN(testFEN);
        Debug.Log($"<color=green>FEN loaded: {loaded}</color>");
        
        // Get and set pieces
        char piece = board.GetPiece("e4");
        Debug.Log($"<color=green>Piece at e4: {piece}</color>");
        
        // Coordinate conversion
        v2 coord = ChessBoard.AlgebraicToCoord("e4");
        string square = ChessBoard.CoordToAlgebraic(coord);
        Debug.Log($"<color=green>e4 converts to {coord} and back to {square}</color>");
        
        // Generate legal moves
        var legalMoves = board.GetLegalMoves();
        Debug.Log($"<color=green>Legal moves: {legalMoves.Count}</color>");
        
        // Make a move (assuming valid ChessMove available)
        // bool moveMade = board.MakeMove(someMove, "test move");
        
        // Evaluation system
        board.UpdateEvaluation(125.5f, 0.6f, 0f, 10);
        float eval = board.LastEvaluation;
        float winProb = board.LastWinProbability;
        Debug.Log($"<color=green>Evaluation: {eval}cp, WinProb: {winProb:P0}</color>");
        
        // Position hashing for caching
        ulong hash = board.CalculatePositionHash();
        Debug.Log($"<color=green>Position hash: {hash}</color>");
        
        // Game tree navigation
        bool canUndo = board.CanUndo();
        bool canRedo = board.CanRedo();
        int nodeCount = board.GameTreeNodeCount;
        Debug.Log($"<color=green>History: nodes={nodeCount}, canUndo={canUndo}, canRedo={canRedo}</color>");
        
        // Player side management
        board.SetHumanSide('w');
        bool isHumanTurn = board.IsHumanTurn();
        bool isEngineTurn = board.IsEngineTurn();
        string humanSideName = board.GetSideName(board.humanSide);
        Debug.Log($"<color=green>Human plays {humanSideName}, human turn: {isHumanTurn}</color>");
        
        // Chess variants
        var chess960Board = new ChessBoard("", ChessBoard.ChessVariant.Chess960);
        var variant = chess960Board.variant;
        Debug.Log($"<color=green>Created {variant} board</color>");
        
        // Validation modes
        var strictBoard = new ChessBoard("invalid", ChessBoard.ChessVariant.Standard, 
                                        ChessBoard.ValidationMode.Strict, true);
        var mode = strictBoard.CurrentValidationMode;
        Debug.Log($"<color=green>Validation mode: {mode}</color>");
        
        // Position caching
        var cachedInfo = board.GetCachedPositionInfo();
        if (cachedInfo.HasValue)
        {
            Debug.Log($"<color=green>Cached eval: {cachedInfo.Value.evaluation}</color>");
        }
        
        // Threefold repetition check
        bool isRepetition = board.IsThreefoldRepetition();
        Debug.Log($"<color=green>Threefold repetition: {isRepetition}</color>");
        
        // Board cloning
        ChessBoard clone = board.Clone();
        string originalFEN = board.ToFEN();
        string cloneFEN = clone.ToFEN();
        bool identical = originalFEN == cloneFEN;
        Debug.Log($"<color=green>Clone identical: {identical}</color>");
        
        // Nested type usage
        var posInfo = new ChessBoard.PositionInfo(hash, eval, winProb, 10);
        bool validCache = posInfo.IsValid();
        Debug.Log($"<color=green>Position info valid: {validCache}</color>");
        
        var pgnMeta = new ChessBoard.PGNMetadata();
        pgnMeta.Event = "Test Game";
        pgnMeta.White = "Human";
        pgnMeta.Black = "Engine";
        bool validPGN = pgnMeta.IsValid();
        Debug.Log($"<color=green>PGN metadata: {pgnMeta.ToString()}, valid: {validPGN}</color>");
        
        // Game result evaluation
        var gameResult = board.GetGameResult();
        Debug.Log($"<color=green>Game result: {gameResult}</color>");
        
        // Reset evaluation to neutral
        board.ResetEvaluation();
        Debug.Log($"<color=green>Reset evaluation: {board.LastEvaluation}</color>");
        
        // Expected outputs:
        // "Created board: ChessBoard[Standard] White to move, Move 1, Eval: 0.0cp (50%), History: 1 positions, Mode: Permissive"
        // "FEN loaded: True"
        // "Piece at e4: P" 
        // "e4 converts to (4, 3) and back to e4"
        // "Legal moves: 20"
        // "Evaluation: 125.5cp, WinProb: 60%"
        // "Position hash: [large number]"
        // "History: nodes=1, canUndo=False, canRedo=False"
        // "Human plays White, human turn: True"
        // "Created Chess960 board"
        // "Validation mode: Strict"
        // "Cached eval: 125.5"
        // "Threefold repetition: False"
        // "Clone identical: True"
        // "Position info valid: True"
        // "PGN metadata: PGN[Test Game at Unity Chess, [date]] Human vs Engine: *, valid: True"
        // "Game result: InProgress"
        // "Reset evaluation: 0"
    }
    
    private void TestingSuite_Check()
    {
        // Run comprehensive tests
        ChessBoard.RunAllTests();
        
        // Expected output: Extensive test logging with colored pass/fail results
        // Tests FEN parsing, move operations, evaluation system, advanced features, validation modes
    }
}
```

## Control flow / responsibilities & high-level algorithm summary / Side effects and I/O

Main runtime flow: Initialize Zobrist keys → Load FEN/setup position → Make moves with validation → Save to game tree → Update evaluation cache. Key algorithms include Zobrist hashing for position comparison, game tree navigation with branching support, and comprehensive FEN parsing with multiple validation modes. Side effects: Unity Debug logging with color coding, position caching to Dictionary, game state persistence in GameTree structure.

## Performance, allocations, and hotspots / Threading / async considerations

Heavy allocations during: FEN parsing string operations, legal move generation, game tree node creation. GC pressure from string manipulations and temporary collections. Single-threaded only, no async/await usage. Main thread Unity constraints for Debug logging.

## Security / safety / correctness concerns

Potential nulls in FEN parsing, unvalidated array access in board coordinates, Unity 2020.3 string compatibility issues (contains char vs string), reflection-free but extensive string parsing with exception handling.

## Tests, debugging & observability

Built-in comprehensive test suite via `RunAllTests()` with color-coded Debug logging. Extensive validation logging for FEN parsing, move validation, and state transitions. Observable game tree state and evaluation history.

## Cross-file references

Dependencies: `ChessRules.cs` (validation, move execution), `MoveGenerator.cs` (legal move generation), `ChessMove.cs` (move representation), `SPACE_UTIL.Board<T>` and `SPACE_UTIL.v2` (grid and coordinate utilities).

## TODO / Known limitations / Suggested improvements

<!-- TODO items from code comments:
- Enhanced Chess960 position generation beyond basic shuffling
- Complete variant-specific win condition implementations for ThreeCheck, RacingKings
- Performance optimization for position cache pruning algorithm
- Enhanced PGN export with full game notation and comments
- Improved error recovery for malformed FEN strings in strict validation mode
- Thread-safe position caching for multi-threaded engine integration
(only if I explicitly mentioned in the prompt) -->

## Appendix

Key private helpers: `ParseBoardPosition()` FEN board parsing, `ValidateBoardState()` position validation, `UpdatePositionCache()` evaluation caching, `RestoreState()` game tree navigation. Zobrist key initialization uses fixed seed for consistency across sessions.

## General Note: important behaviors

Major functionalities: Comprehensive FEN parsing with validation modes, Undo/Redo via game tree with branching variations, Position caching using Zobrist hashing, Multi-variant chess support with different starting positions, Evaluation tracking with engine integration support, Comprehensive test suite with API validation.

`checksum: 3f7a2b1c (v0.3)`