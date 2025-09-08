# Source: `ChessBoard.cs` — Enhanced chess board with comprehensive engine integration, validation, and testing

## Short description
Implements a comprehensive chess board system with engine integration support, legal move generation, position caching, game tree management, and multiple chess variants. Provides complete FEN parsing/generation, move validation, undo/redo functionality, and evaluation metrics for chess engine integration.

## Metadata
* **Filename:** `ChessBoard.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependencies:** System, System.Collections.Generic, System.Linq, System.Text, UnityEngine, SPACE_UTIL
* **Public types:** `ChessBoard (class), ChessBoard.ChessVariant (enum), ChessBoard.PositionInfo (struct), ChessBoard.PGNMetadata (class), ChessBoard.GameTree (class), ChessBoard.GameNode (class), ChessBoard.BoardState (struct)`
* **Unity version:** Unity 2020.3+ (based on compatibility notes)

## Public API Summary
| Type | Member | Signature | Short Purpose | OneLiner Call |
|------|---------|-----------|---------------|---------------|
| Board<char> | Field | public Board<char> board | 8x8 chess board grid | var piece = chessBoard.board.GT(new v2(0, 0)); |
| char | Field | public char sideToMove | Current player turn | var turn = chessBoard.sideToMove; |
| string | Field | public string castlingRights | Available castling options | var rights = chessBoard.castlingRights; |
| string | Field | public string enPassantSquare | En passant target square | var epSquare = chessBoard.enPassantSquare; |
| int | Field | public int halfmoveClock | 50-move rule counter | var halfMoves = chessBoard.halfmoveClock; |
| int | Field | public int fullmoveNumber | Game move counter | var moveNum = chessBoard.fullmoveNumber; |
| char | Field | public char humanSide | Human player color | var human = chessBoard.humanSide; |
| char | Field | public char engineSide | Engine player color | var engine = chessBoard.engineSide; |
| bool | Field | public bool allowSideSwitching | Side switching permission | var canSwitch = chessBoard.allowSideSwitching; |
| ChessBoard.ChessVariant | Field | public ChessBoard.ChessVariant variant | Chess variant type | var variant = chessBoard.variant; |
| float | Property | public float LastEvaluation | Latest position evaluation | var eval = chessBoard.LastEvaluation; |
| float | Property | public float LastWinProbability | Win probability metric | var winProb = chessBoard.LastWinProbability; |
| float | Property | public float LastMateDistance | Distance to mate | var mateDist = chessBoard.LastMateDistance; |
| int | Property | public int LastEvaluationDepth | Search depth used | var depth = chessBoard.LastEvaluationDepth; |
| int | Property | public int GameTreeNodeCount | Total game positions | var nodeCount = chessBoard.GameTreeNodeCount; |
| int | Property | public int CurrentHistoryIndex | Current position index | var currentIdx = chessBoard.CurrentHistoryIndex; |
| void | Constructor | public ChessBoard() | Creates starting position | var board = new ChessBoard(); |
| void | Constructor | public ChessBoard(string fen, ChessBoard.ChessVariant variant) | Creates from FEN string | var board = new ChessBoard("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"); |
| void | Method | public void SetupStartingPosition() | Resets to starting position | chessBoard.SetupStartingPosition(); |
| ulong | Method | public ulong CalculatePositionHash() | Zobrist position hash | var hash = chessBoard.CalculatePositionHash(); |
| bool | Method | public bool MakeMove(ChessMove move, string comment) | Execute chess move | var success = chessBoard.MakeMove(move, "good move"); |
| bool | Method | public bool UndoMove() | Revert last move | var undone = chessBoard.UndoMove(); |
| bool | Method | public bool RedoMove() | Replay next move | var redone = chessBoard.RedoMove(); |
| bool | Method | public bool GoToVariation(int variationIndex) | Switch to move variation | var switched = chessBoard.GoToVariation(0); |
| bool | Method | public bool CanUndo() | Check undo availability | var canUndo = chessBoard.CanUndo(); |
| bool | Method | public bool CanRedo() | Check redo availability | var canRedo = chessBoard.CanRedo(); |
| ChessBoard.PositionInfo? | Method | public ChessBoard.PositionInfo? GetCachedPositionInfo() | Get cached evaluation | var cached = chessBoard.GetCachedPositionInfo(); |
| bool | Method | public bool IsThreefoldRepetition() | Check repetition draw | var isRep = chessBoard.IsThreefoldRepetition(); |
| bool | Method | public bool LoadFromFEN(string fen) | Parse FEN notation | var loaded = chessBoard.LoadFromFEN(fenString); |
| string | Method | public string ToFEN() | Generate FEN string | var fen = chessBoard.ToFEN(); |
| void | Method | public void SetHumanSide(char side) | Set human player color | chessBoard.SetHumanSide('w'); |
| string | Method | public string GetSideName(char side) | Get color name | var name = chessBoard.GetSideName('w'); |
| bool | Method | public bool IsHumanTurn() | Check human turn | var humanTurn = chessBoard.IsHumanTurn(); |
| bool | Method | public bool IsEngineTurn() | Check engine turn | var engineTurn = chessBoard.IsEngineTurn(); |
| char | Method | public char GetPiece(string square) | Get piece at square | var piece = chessBoard.GetPiece("e4"); |
| char | Method | public char GetPiece(v2 coord) | Get piece at coordinate | var piece = chessBoard.GetPiece(new v2(4, 3)); |
| void | Method | public void SetPiece(v2 coord, char piece) | Place piece on board | chessBoard.SetPiece(new v2(4, 3), 'K'); |
| v2 | Method | public static v2 AlgebraicToCoord(string square) | Convert notation to coord | var coord = ChessBoard.AlgebraicToCoord("e4"); |
| string | Method | public static string CoordToAlgebraic(v2 coord) | Convert coord to notation | var square = ChessBoard.CoordToAlgebraic(new v2(4, 3)); |
| List<ChessMove> | Method | public List<ChessMove> GetLegalMoves() | Generate legal moves | var moves = chessBoard.GetLegalMoves(); |
| void | Method | public void UpdateEvaluation(float centipawnScore, float winProbability, float mateDistance, int searchDepth) | Update position metrics | chessBoard.UpdateEvaluation(150.5f, 0.65f, 0f, 12); |
| void | Method | public void ResetEvaluation() | Clear evaluation data | chessBoard.ResetEvaluation(); |
| ChessRules.GameResult | Method | public ChessRules.GameResult GetGameResult() | Check game outcome | var result = chessBoard.GetGameResult(); |
| ChessBoard | Method | public ChessBoard Clone() | Deep copy board | var copy = chessBoard.Clone(); |
| string | Method | public override string ToString() | Board debug string | var info = chessBoard.ToString(); |
| void | Method | public static void RunAllTests() | Execute test suite | ChessBoard.RunAllTests(); |

## Important Types

### `ChessBoard.ChessVariant`
* **Kind:** enum
* **Responsibility:** Defines supported chess variant types including standard, Chess960, King of the Hill, and others
* **Values:**
  * `Standard` — Traditional chess rules
  * `Chess960` — Fischer Random starting positions
  * `KingOfTheHill` — Win by moving king to center
  * `Atomic` — Pieces explode when captured
  * `ThreeCheck` — Win by checking three times
  * `Horde` — Asymmetric pawn vs pieces
  * `RacingKings` — Race kings to back rank

### `ChessBoard.PositionInfo`
* **Kind:** struct
* **Responsibility:** Cached position evaluation data for performance optimization
* **Constructor:** `public PositionInfo(ulong hash, float eval, float winProb, int depth)`
* **Public Properties:**
  * `hash` — `ulong` — Zobrist position hash (`get/set`)
  * `evaluation` — `float` — Centipawn evaluation (`get/set`)
  * `winProbability` — `float` — Win probability [0-1] (`get/set`)
  * `depthSearched` — `int` — Search depth used (`get/set`)
  * `timestamp` — `float` — Cache entry time (`get/set`)
  * `legalMoves` — `List<ChessMove>` — Cached legal moves (`get/set`)
  * `gameResult` — `ChessRules.GameResult` — Position outcome (`get/set`)
* **Public Methods:**
  * **`public bool IsValid()`**
    * Description: Validates cache entry integrity
    * Returns: `bool — true if hash and timestamp are valid` + call example: `var valid = posInfo.IsValid();`

### `ChessBoard.PGNMetadata`
* **Kind:** class
* **Responsibility:** Stores complete PGN game metadata for notation export
* **Constructor:** `public PGNMetadata()` — initializes with default values and current date
* **Public Properties:**
  * `Event` — `string` — Tournament/game name (`get/set`)
  * `Site` — `string` — Playing location (`get/set`)
  * `Date` — `string` — Game date (yyyy.MM.dd) (`get/set`)
  * `Round` — `string` — Tournament round (`get/set`)
  * `White` — `string` — White player name (`get/set`)
  * `Black` — `string` — Black player name (`get/set`)
  * `Result` — `string` — Game result (*,1-0,0-1,1/2-1/2) (`get/set`)
  * `WhiteElo` — `string` — White player rating (`get/set`)
  * `BlackElo` — `string` — Black player rating (`get/set`)
  * `TimeControl` — `string` — Time control format (`get/set`)
  * `ECO` — `string` — Opening classification (`get/set`)
  * `Opening` — `string` — Opening name (`get/set`)
* **Public Methods:**
  * **`public bool IsValid()`**
    * Description: Validates required PGN fields
    * Returns: `bool — true if essential fields are present` + call example: `var valid = metadata.IsValid();`
  * **`public override string ToString()`**
    * Description: Summary string representation
    * Returns: `string — formatted metadata summary` + call example: `var summary = metadata.ToString();`

### `ChessBoard.GameTree`
* **Kind:** class
* **Responsibility:** Manages branching game history with variations and navigation
* **Constructor:** `public GameTree()` — initializes empty game tree
* **Public Properties:**
  * `CurrentNodeIndex` — `int` — Active position index (`get`)
  * `NodeCount` — `int` — Total positions stored (`get`)
  * `CurrentNode` — `ChessBoard.GameNode` — Active game node (`get`)
* **Public Methods:**
  * **`public ChessBoard.GameNode AddMove(ChessBoard.BoardState state, ChessMove move, string san, float evaluation, string comment)`**
    * Description: Adds new move to current position
    * Parameters: `state : ChessBoard.BoardState — board position`, `move : ChessMove — chess move`, `san : string — algebraic notation`, `evaluation : float — position score`, `comment : string — move annotation`
    * Returns: `ChessBoard.GameNode — created game node` + call example: `var node = gameTree.AddMove(state, move, "e4", 0.2f, "good");`
  * **`public bool GoToNode(int nodeIndex)`**
    * Description: Navigate to specific position
    * Parameters: `nodeIndex : int — target position index`
    * Returns: `bool — true if navigation successful` + call example: `var moved = gameTree.GoToNode(5);`
  * **`public List<ChessBoard.GameNode> GetMainLine()`**
    * Description: Gets path from root to current position
    * Returns: `List<ChessBoard.GameNode> — ordered move sequence` + call example: `var mainLine = gameTree.GetMainLine();`
  * **`public List<List<ChessBoard.GameNode>> GetVariations(int fromNodeIndex)`**
    * Description: Gets all move alternatives from position
    * Parameters: `fromNodeIndex : int — starting position (-1 for current)`
    * Returns: `List<List<ChessBoard.GameNode>> — nested variation lists` + call example: `var variations = gameTree.GetVariations(-1);`
  * **`public void Clear()`**
    * Description: Resets game tree to empty state
    * Returns: `void` + call example: `gameTree.Clear();`
  * **`public int FindPosition(ulong positionHash)`**
    * Description: Locates position by hash
    * Parameters: `positionHash : ulong — Zobrist hash to find`
    * Returns: `int — node index or -1 if not found` + call example: `var nodeIdx = gameTree.FindPosition(hash);`
  * **`public bool CanUndo()`**
    * Description: Checks undo availability
    * Returns: `bool — true if can go back` + call example: `var canUndo = gameTree.CanUndo();`
  * **`public bool CanRedo()`**
    * Description: Checks redo availability
    * Returns: `bool — true if can go forward` + call example: `var canRedo = gameTree.CanRedo();`

### `ChessBoard.GameNode`
* **Kind:** class
* **Responsibility:** Single position in game tree with move and evaluation data
* **Constructor:** `public GameNode()` — initializes empty game node
* **Public Properties:**
  * `state` — `ChessBoard.BoardState` — Position data (`get/set`)
  * `move` — `ChessMove` — Move that led here (`get/set`)
  * `sanNotation` — `string` — Algebraic move notation (`get/set`)
  * `evaluation` — `float` — Position evaluation (`get/set`)
  * `comment` — `string` — Move annotation (`get/set`)
  * `parentIndex` — `int` — Previous position index (`get/set`)
  * `children` — `List<int>` — Next position indices (`get/set`)
  * `timestamp` — `float` — Creation time (`get/set`)
  * `annotations` — `Dictionary<string, string>` — Additional metadata (`get/set`)
* **Public Methods:**
  * **`public bool IsValid()`**
    * Description: Validates node data integrity
    * Returns: `bool — true if notation and timestamp are valid` + call example: `var valid = node.IsValid();`
  * **`public override string ToString()`**
    * Description: Debug representation
    * Returns: `string — formatted node summary` + call example: `var info = node.ToString();`

### `ChessBoard.BoardState`
* **Kind:** struct
* **Responsibility:** Complete position snapshot with evaluation and hash for caching
* **Constructor:** `public BoardState(ChessBoard board)` — captures current board state
* **Public Properties:**
  * `fen` — `string` — FEN position notation (`get/set`)
  * `sideToMove` — `char` — Current player ('w'/'b') (`get/set`)
  * `castlingRights` — `string` — Available castling (`get/set`)
  * `enPassantSquare` — `string` — En passant target (`get/set`)
  * `halfmoveClock` — `int` — 50-move counter (`get/set`)
  * `fullmoveNumber` — `int` — Move number (`get/set`)
  * `timestamp` — `float` — Snapshot time (`get/set`)
  * `evaluation` — `float` — Position score (`get/set`)
  * `winProbability` — `float` — Win chance [0-1] (`get/set`)
  * `mateDistance` — `float` — Moves to mate (`get/set`)
  * `positionHash` — `ulong` — Zobrist hash (`get/set`)
* **Public Methods:**
  * **`public bool IsValid()`**
    * Description: Validates state data completeness
    * Returns: `bool — true if all required fields are valid` + call example: `var valid = state.IsValid();`

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
        var fen = board.ToFEN();
        var loaded = board.LoadFromFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        var piece = board.GetPiece("e1");
        var hash = board.CalculatePositionHash();
        var moves = board.GetLegalMoves();
        board.UpdateEvaluation(50.0f, 0.6f, 0f, 10);
        var evaluation = board.LastEvaluation;
        var humanTurn = board.IsHumanTurn();
        var result = board.GetGameResult();
        var clone = board.Clone();
        var canUndo = board.CanUndo();
        
        Debug.Log($"API Results: FEN={fen.Length>0}, Loaded={loaded}, Piece={piece}, Hash={hash}, Moves={moves.Count}, Eval={evaluation}, HumanTurn={humanTurn}, Result={result}, Clone={clone!=null}, CanUndo={canUndo}");
    }
}
```

## Control Flow & Responsibilities
Zobrist hashing, FEN parsing/generation, legal move validation, game tree navigation, position caching, evaluation tracking, variant-specific rules.

## Performance & Threading
Heavy: move generation, position hashing. Main-thread only. Position caching optimized.

## Cross-file Dependencies
Board<char> from SPACE_UTIL, ChessRules validation, MoveGenerator, ChessMove structures referenced.

## Major Functionality
FEN import/export, undo/redo system, position caching, game tree with variations, chess variants, comprehensive test suite.

`checksum: 4A7B9C2E v0.3.min`