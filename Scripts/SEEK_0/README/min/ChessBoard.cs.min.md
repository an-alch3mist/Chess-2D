# Source: `ChessBoard.cs` — Enhanced chess board with comprehensive engine integration, validation, and testing

## Short description
Implements a complete chess board representation with advanced features including legal move generation, promotion handling, undo/redo functionality, evaluation metrics, game tree navigation, position caching, and comprehensive testing suite. Designed for integration with chess engines and supports multiple chess variants.

## Metadata
* **Filename:** `ChessBoard.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependencies:** `System`, `System.Collections.Generic`, `System.Linq`, `System.Text`, `UnityEngine`, `SPACE_UTIL`
* **Public types:** `ChessBoard (class)`, `ChessBoard.ChessVariant (enum)`, `ChessBoard.PositionInfo (struct)`, `ChessBoard.PGNMetadata (class)`, `ChessBoard.GameTree (class)`, `ChessBoard.GameNode (class)`, `ChessBoard.BoardState (struct)`
* **Unity version:** Unity-dependent (uses UnityEngine, SerializeField, Time.time)

## Public API Summary
| Type | Member | Signature | Short Purpose | OneLiner Call |
|------|---------|-----------|---------------|---------------|
| Board<char> | Field | public Board<char> board | 8x8 chess board grid | var piece = chessBoard.board.GT(coord); |
| char | Field | public char sideToMove | Current player turn | var turn = chessBoard.sideToMove; |
| string | Field | public string castlingRights | Castling availability | var castling = chessBoard.castlingRights; |
| string | Field | public string enPassantSquare | En passant target | var ep = chessBoard.enPassantSquare; |
| int | Field | public int halfmoveClock | Halfmove rule counter | var halfmove = chessBoard.halfmoveClock; |
| int | Field | public int fullmoveNumber | Game move number | var moveNum = chessBoard.fullmoveNumber; |
| char | Field | public char humanSide | Human player color | var human = chessBoard.humanSide; |
| char | Field | public char engineSide | Engine player color | var engine = chessBoard.engineSide; |
| bool | Field | public bool allowSideSwitching | Side switching enabled | var canSwitch = chessBoard.allowSideSwitching; |
| ChessBoard.ChessVariant | Field | public ChessBoard.ChessVariant variant | Chess variant type | var variant = chessBoard.variant; |
| float | Property | public float LastEvaluation | Latest position evaluation | var eval = chessBoard.LastEvaluation; |
| float | Property | public float LastWinProbability | Latest win probability | var winProb = chessBoard.LastWinProbability; |
| float | Property | public float LastMateDistance | Latest mate distance | var mate = chessBoard.LastMateDistance; |
| int | Property | public int LastEvaluationDepth | Latest search depth | var depth = chessBoard.LastEvaluationDepth; |
| int | Property | public int GameTreeNodeCount | Game tree size | var nodes = chessBoard.GameTreeNodeCount; |
| int | Property | public int CurrentHistoryIndex | Current position index | var index = chessBoard.CurrentHistoryIndex; |
| void | Constructor | public ChessBoard() | Default starting position | var board = new ChessBoard(); |
| void | Constructor | public ChessBoard(string fen, ChessBoard.ChessVariant variant) | Load from FEN | var board = new ChessBoard("rnbq...", variant); |
| void | Method | public void SetupStartingPosition() | Reset to start position | board.SetupStartingPosition(); |
| ulong | Method | public ulong CalculatePositionHash() | Get position hash | var hash = board.CalculatePositionHash(); |
| void | Method | public void SaveCurrentState() | Save to game tree | board.SaveCurrentState(); |
| bool | Method | public bool MakeMove(ChessMove move, string comment) | Execute chess move | var success = board.MakeMove(move, "comment"); |
| bool | Method | public bool UndoMove() | Navigate to previous | var undid = board.UndoMove(); |
| bool | Method | public bool RedoMove() | Navigate forward | var redid = board.RedoMove(); |
| bool | Method | public bool GoToVariation(int variationIndex) | Switch to variation | var switched = board.GoToVariation(0); |
| bool | Method | public bool CanUndo() | Check undo availability | var canUndo = board.CanUndo(); |
| bool | Method | public bool CanRedo() | Check redo availability | var canRedo = board.CanRedo(); |
| ChessBoard.PositionInfo? | Method | public ChessBoard.PositionInfo? GetCachedPositionInfo() | Get cached evaluation | var info = board.GetCachedPositionInfo(); |
| bool | Method | public bool IsThreefoldRepetition() | Check repetition rule | var isRep = board.IsThreefoldRepetition(); |
| bool | Method | public bool LoadFromFEN(string fen) | Parse FEN string | var loaded = board.LoadFromFEN(fenString); |
| string | Method | public string ToFEN() | Generate FEN string | var fen = board.ToFEN(); |
| void | Method | public void SetHumanSide(char side) | Set human player color | board.SetHumanSide('w'); |
| string | Method | public string GetSideName(char side) | Get readable side name | var name = board.GetSideName('w'); |
| bool | Method | public bool IsHumanTurn() | Check human turn | var isHuman = board.IsHumanTurn(); |
| bool | Method | public bool IsEngineTurn() | Check engine turn | var isEngine = board.IsEngineTurn(); |
| char | Method | public char GetPiece(string square) | Get piece by algebraic | var piece = board.GetPiece("e4"); |
| char | Method | public char GetPiece(v2 coord) | Get piece by coordinate | var piece = board.GetPiece(new v2(4,3)); |
| void | Method | public void SetPiece(v2 coord, char piece) | Set piece at position | board.SetPiece(coord, 'Q'); |
| List<ChessMove> | Method | public List<ChessMove> GetLegalMoves() | Generate legal moves | var moves = board.GetLegalMoves(); |
| void | Method | public void UpdateEvaluation(float centipawnScore, float winProbability, float mateDistance, int searchDepth) | Update position metrics | board.UpdateEvaluation(150f, 0.65f, 0f, 12); |
| void | Method | public void ResetEvaluation() | Reset to neutral | board.ResetEvaluation(); |
| ChessRules.GameResult | Method | public ChessRules.GameResult GetGameResult() | Check game status | var result = board.GetGameResult(); |
| ChessBoard | Method | public ChessBoard Clone() | Create deep copy | var copy = board.Clone(); |
| v2 | Method | public static v2 AlgebraicToCoord(string square) | Convert notation to coord | var coord = ChessBoard.AlgebraicToCoord("e4"); |
| string | Method | public static string CoordToAlgebraic(v2 coord) | Convert coord to notation | var square = ChessBoard.CoordToAlgebraic(coord); |
| void | Method | public static void RunAllTests() | Execute test suite | ChessBoard.RunAllTests(); |

## Important Types

### `ChessBoard.ChessVariant`
* **Kind:** enum
* **Responsibility:** Defines supported chess variants including standard chess, Chess960, King of the Hill, Atomic, etc.
* **Values:** `Standard`, `Chess960`, `KingOfTheHill`, `Atomic`, `ThreeCheck`, `Horde`, `RacingKings`

### `ChessBoard.PositionInfo`
* **Kind:** struct with validation
* **Responsibility:** Cached position data for performance optimization
* **Constructor(s):** `public PositionInfo(ulong hash, float eval, float winProb, int depth)`
* **Public Properties:**
  * `hash` — `ulong` — Position hash identifier (`get/set`)
  * `evaluation` — `float` — Centipawn evaluation (`get/set`)
  * `winProbability` — `float` — Win probability 0-1 (`get/set`)
  * `depthSearched` — `int` — Search depth used (`get/set`)
  * `timestamp` — `float` — Cache timestamp (`get/set`)
  * `legalMoves` — `List<ChessMove>` — Cached legal moves (`get/set`)
  * `gameResult` — `ChessRules.GameResult` — Position result (`get/set`)
* **Public Methods:**
  * **`public bool IsValid()`**
    * Description: Validates cache entry integrity
    * Returns: `bool — true if valid` + call example: `var valid = info.IsValid();`

### `ChessBoard.PGNMetadata`
* **Kind:** class for PGN standard compliance
* **Responsibility:** Stores complete game notation metadata including players, event, ratings, time control
* **Constructor(s):** `public PGNMetadata()` — initializes with defaults and current date
* **Public Properties:**
  * `Event` — `string` — Tournament/match name (`get/set`)
  * `Site` — `string` — Game location (`get/set`)
  * `Date` — `string` — Game date in YYYY.MM.DD (`get/set`)
  * `Round` — `string` — Round number (`get/set`)
  * `White` — `string` — White player name (`get/set`)
  * `Black` — `string` — Black player name (`get/set`)
  * `Result` — `string` — Game result notation (`get/set`)
  * `WhiteElo` — `string` — White player rating (`get/set`)
  * `BlackElo` — `string` — Black player rating (`get/set`)
  * `TimeControl` — `string` — Time control format (`get/set`)
  * `ECO` — `string` — Opening classification (`get/set`)
  * `Opening` — `string` — Opening name (`get/set`)
* **Public Methods:**
  * **`public bool IsValid()`**
    * Description: Validates required PGN metadata fields
    * Returns: `bool — metadata completeness` + call example: `var valid = metadata.IsValid();`

### `ChessBoard.GameTree`
* **Kind:** class for branching move history
* **Responsibility:** Manages game tree with variations, undo/redo, and position navigation
* **Public Properties:**
  * `CurrentNodeIndex` — `int` — Active position index (`get`)
  * `NodeCount` — `int` — Total tree nodes (`get`)
  * `CurrentNode` — `ChessBoard.GameNode` — Active tree node (`get`)
* **Public Methods:**
  * **`public ChessBoard.GameNode AddMove(ChessBoard.BoardState state, ChessMove move, string san, float evaluation, string comment)`**
    * Description: Adds move to current position creating new branch if necessary
    * Parameters: `state : ChessBoard.BoardState — board state`, `move : ChessMove — chess move`, `san : string — algebraic notation`, `evaluation : float — position score`, `comment : string — move annotation`
    * Returns: `ChessBoard.GameNode — new tree node` + call example: `var node = tree.AddMove(state, move, "e4", 0.2f, "");`
  * **`public bool GoToNode(int nodeIndex)`**
    * Description: Navigate to specific tree position
    * Parameters: `nodeIndex : int — target node index`
    * Returns: `bool — navigation success` + call example: `var moved = tree.GoToNode(5);`
  * **`public List<ChessBoard.GameNode> GetMainLine()`**
    * Description: Get path from root to current position
    * Returns: `List<ChessBoard.GameNode> — main variation` + call example: `var line = tree.GetMainLine();`
  * **`public List<List<ChessBoard.GameNode>> GetVariations(int fromNodeIndex)`**
    * Description: Get all variations from specified position
    * Parameters: `fromNodeIndex : int — starting node index`
    * Returns: `List<List<ChessBoard.GameNode>> — all branches` + call example: `var vars = tree.GetVariations(3);`
  * **`public void Clear()`**
    * Description: Reset tree to empty state
    * Call example: `tree.Clear();`
  * **`public int FindPosition(ulong positionHash)`**
    * Description: Locate position in tree by hash
    * Parameters: `positionHash : ulong — position identifier`
    * Returns: `int — node index or -1` + call example: `var index = tree.FindPosition(hash);`
  * **`public bool CanUndo()`**
    * Description: Check if undo operation available
    * Returns: `bool — undo availability` + call example: `var canUndo = tree.CanUndo();`
  * **`public bool CanRedo()`**
    * Description: Check if redo operation available
    * Returns: `bool — redo availability` + call example: `var canRedo = tree.CanRedo();`

### `ChessBoard.GameNode`
* **Kind:** class representing single tree position
* **Responsibility:** Stores complete position data including state, move, notation, evaluation and annotations
* **Constructor(s):** `public GameNode()` — initializes with empty collections and defaults
* **Public Properties:**
  * `state` — `ChessBoard.BoardState` — Complete position state (`get/set`)
  * `move` — `ChessMove` — Move that created position (`get/set`)
  * `sanNotation` — `string` — Standard algebraic notation (`get/set`)
  * `evaluation` — `float` — Position evaluation (`get/set`)
  * `comment` — `string` — Move annotation (`get/set`)
  * `parentIndex` — `int` — Parent node index (`get/set`)
  * `children` — `List<int>` — Child node indices (`get/set`)
  * `timestamp` — `float` — Creation time (`get/set`)
  * `annotations` — `Dictionary<string, string>` — Extended annotations (`get/set`)
* **Public Methods:**
  * **`public bool IsValid()`**
    * Description: Validates node data integrity
    * Returns: `bool — node validity` + call example: `var valid = node.IsValid();`

### `ChessBoard.BoardState`
* **Kind:** struct with position hashing
* **Responsibility:** Complete immutable board state with Zobrist hashing for fast position comparison
* **Constructor(s):** `public BoardState(ChessBoard board)` — captures complete board state
* **Public Properties:**
  * `fen` — `string` — FEN representation (`get/set`)
  * `sideToMove` — `char` — Active player (`get/set`)
  * `castlingRights` — `string` — Castling availability (`get/set`)
  * `enPassantSquare` — `string` — En passant target (`get/set`)
  * `halfmoveClock` — `int` — Halfmove counter (`get/set`)
  * `fullmoveNumber` — `int` — Move number (`get/set`)
  * `timestamp` — `float` — State creation time (`get/set`)
  * `evaluation` — `float` — Position score (`get/set`)
  * `winProbability` — `float` — Win probability (`get/set`)
  * `mateDistance` — `float` — Mate distance (`get/set`)
  * `positionHash` — `ulong` — Zobrist hash (`get/set`)
* **Public Methods:**
  * **`public bool IsValid()`**
    * Description: Validates complete state integrity
    * Returns: `bool — state validity` + call example: `var valid = state.IsValid();`

## Example Usage
**Required namespaces:**
```csharp
// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using GPTDeepResearch;
// using SPACE_UTIL;
```

**For Non-MonoBehaviour classes:**
```csharp
public class ExampleUsage : MonoBehaviour 
{
    private void ChessBoard_Check()
    {
        // Test all major public APIs in minimal lines
        var board = new ChessBoard();
        var startFEN = board.ToFEN();
        board.SetHumanSide('w');
        var moves = board.GetLegalMoves();
        var move = moves[0];
        var moveSuccess = board.MakeMove(move, "test");
        board.UpdateEvaluation(50f, 0.6f, 0f, 10);
        var canUndo = board.CanUndo();
        var undoSuccess = board.UndoMove();
        var piece = board.GetPiece("e2");
        var coord = ChessBoard.AlgebraicToCoord("e4");
        var square = ChessBoard.CoordToAlgebraic(coord);
        var hash = board.CalculatePositionHash();
        var gameResult = board.GetGameResult();
        var clone = board.Clone();
        var isRepetition = board.IsThreefoldRepetition();
        var cachedInfo = board.GetCachedPositionInfo();
        
        Debug.Log($"API Results: FEN={startFEN}, Moves={moves.Count}, MoveSuccess={moveSuccess}, CanUndo={canUndo}, UndoSuccess={undoSuccess}, Piece={piece}, Hash={hash}, Result={gameResult}");
    }
}
```

## Control Flow & Responsibilities
Manages complete chess game state with move validation, history tracking, evaluation caching, and variant support.

## Performance & Threading
Heavy Zobrist hashing, position caching, legal move generation. Main-thread only, no async operations.

## Cross-file Dependencies
References ChessRules.cs, ChessMove.cs, MoveGenerator.cs for validation and move generation functionality.

## Major Functionality
Comprehensive chess engine including FEN parsing, legal moves, undo/redo, evaluation, threefold repetition, variants.

`checksum: 7a4f9e2b v0.3.min`