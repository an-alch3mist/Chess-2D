# Source: `ChessBoard.cs` — Enhanced chess board implementation with comprehensive engine integration, validation, and testing

## Short description
This file implements a comprehensive chess board system with advanced features including move generation, undo/redo functionality, position validation, game tree management, evaluation caching, and support for multiple chess variants. The system provides extensive engine integration capabilities with Zobrist hashing for position comparison and threefold repetition detection.

## Metadata
* **Filename:** `ChessBoard.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependencies:** `System`, `System.Collections.Generic`, `System.Linq`, `System.Text`, `UnityEngine`, `SPACE_UTIL`
* **Public types:** `ChessBoard (class)`, `ChessBoard.ChessVariant (enum)`, `ChessBoard.ValidationMode (enum)`, `ChessBoard.PositionInfo (struct)`, `ChessBoard.PGNMetadata (class)`, `ChessBoard.GameTree (class)`, `ChessBoard.GameNode (class)`, `ChessBoard.BoardState (struct)`
* **Unity version:** Compatible with Unity 2020.3+

## Public API Summary
| Type | Member | Signature | Short Purpose | OneLiner Call |
|------|---------|-----------|---------------|---------------|
| Board<char> | Field | public Board<char> board | 8x8 chess board grid | var piece = chessBoard.board.GT(new v2(0,0)); |
| char | Field | public char sideToMove | Current player turn | var turn = chessBoard.sideToMove; |
| string | Field | public string castlingRights | Castling availability | var castling = chessBoard.castlingRights; |
| string | Field | public string enPassantSquare | En passant target | var epSquare = chessBoard.enPassantSquare; |
| int | Field | public int halfmoveClock | Half-move counter | var halfMoves = chessBoard.halfmoveClock; |
| int | Field | public int fullmoveNumber | Full move number | var moveNum = chessBoard.fullmoveNumber; |
| char | Property | public char humanSide { get; private set; } | Human player side | var humanColor = chessBoard.humanSide; |
| char | Property | public char engineSide { get; private set; } | Engine player side | var engineColor = chessBoard.engineSide; |
| bool | Field | public bool allowSideSwitching | Allow side changes | var canSwitch = chessBoard.allowSideSwitching; |
| ChessBoard.ChessVariant | Property | public ChessBoard.ChessVariant variant { get; private set; } | Chess variant type | var gameVariant = chessBoard.variant; |
| float | Property | public float LastEvaluation { get; private set; } | Latest position evaluation | var eval = chessBoard.LastEvaluation; |
| float | Property | public float LastWinProbability { get; private set; } | Win probability estimate | var winProb = chessBoard.LastWinProbability; |
| float | Property | public float LastMateDistance { get; private set; } | Distance to mate | var mateDist = chessBoard.LastMateDistance; |
| int | Property | public int LastEvaluationDepth { get; private set; } | Search depth used | var depth = chessBoard.LastEvaluationDepth; |
| List<ChessBoard.GameNode> | Property | public List<ChessBoard.GameNode> LogGameTreeNodes { get; } | Game tree nodes for logging | var nodes = chessBoard.LogGameTreeNodes; |
| int | Property | public int GameTreeNodeCount | Count of game tree nodes | var nodeCount = chessBoard.GameTreeNodeCount; |
| int | Property | public int CurrentHistoryIndex | Current position index | var currentIdx = chessBoard.CurrentHistoryIndex; |
| ChessBoard.ValidationMode | Property | public ChessBoard.ValidationMode CurrentValidationMode { get; } | Current validation mode | var mode = chessBoard.CurrentValidationMode; |
| void | Constructor | public ChessBoard() | Creates default starting position | var board = new ChessBoard(); |
| void | Constructor | public ChessBoard(string fen, ChessBoard.ChessVariant variant = ChessBoard.ChessVariant.Standard, ChessBoard.ValidationMode validation = ChessBoard.ValidationMode.Permissive, bool fallbackOnFailure = false) | Creates from FEN string | var board = new ChessBoard("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"); |
| void | Method | public void SetValidationMode(ChessBoard.ValidationMode mode, bool fallback = false) | Sets validation behavior | chessBoard.SetValidationMode(ChessBoard.ValidationMode.Strict, true); |
| void | Method | public void SetupStartingPosition() | Initializes starting position | chessBoard.SetupStartingPosition(); |
| ulong | Method | public ulong CalculatePositionHash() | Calculates Zobrist hash | var hash = chessBoard.CalculatePositionHash(); |
| bool | Method | public bool MakeMove(ChessMove move, string comment = "") | Makes a chess move | var success = chessBoard.MakeMove(move, "Good move"); |
| bool | Method | public bool UndoMove() | Undoes last move | var undone = chessBoard.UndoMove(); |
| bool | Method | public bool RedoMove() | Redoes undone move | var redone = chessBoard.RedoMove(); |
| bool | Method | public bool GoToVariation(int variationIndex) | Switches to variation | var switched = chessBoard.GoToVariation(0); |
| bool | Method | public bool CanUndo() | Checks if undo possible | var canUndo = chessBoard.CanUndo(); |
| bool | Method | public bool CanRedo() | Checks if redo possible | var canRedo = chessBoard.CanRedo(); |
| ChessBoard.PositionInfo? | Method | public ChessBoard.PositionInfo? GetCachedPositionInfo() | Gets cached evaluation | var cached = chessBoard.GetCachedPositionInfo(); |
| bool | Method | public bool IsThreefoldRepetition() | Checks for repetition | var isRep = chessBoard.IsThreefoldRepetition(); |
| bool | Method | public bool LoadFromFEN(string fen) | Loads position from FEN | var loaded = chessBoard.LoadFromFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"); |
| string | Method | public string ToFEN() | Exports to FEN format | var fen = chessBoard.ToFEN(); |
| void | Method | public void SetHumanSide(char side) | Sets human player side | chessBoard.SetHumanSide('w'); |
| string | Method | public string GetSideName(char side) | Gets side display name | var name = chessBoard.GetSideName('w'); |
| bool | Method | public bool IsHumanTurn() | Checks if human turn | var isHumanTurn = chessBoard.IsHumanTurn(); |
| bool | Method | public bool IsEngineTurn() | Checks if engine turn | var isEngineTurn = chessBoard.IsEngineTurn(); |
| char | Method | public char GetPiece(string square) | Gets piece at square | var piece = chessBoard.GetPiece("e4"); |
| char | Method | public char GetPiece(v2 coord) | Gets piece at coordinate | var piece = chessBoard.GetPiece(new v2(4, 3)); |
| void | Method | public void SetPiece(v2 coord, char piece) | Sets piece at coordinate | chessBoard.SetPiece(new v2(4, 3), 'P'); |
| v2 | Method | public static v2 AlgebraicToCoord(string square) | Converts square to coordinate | var coord = ChessBoard.AlgebraicToCoord("e4"); |
| string | Method | public static string CoordToAlgebraic(v2 coord) | Converts coordinate to square | var square = ChessBoard.CoordToAlgebraic(new v2(4, 3)); |
| List<ChessMove> | Method | public List<ChessMove> GetLegalMoves() | Gets all legal moves | var moves = chessBoard.GetLegalMoves(); |
| void | Method | public void UpdateEvaluation(float centipawnScore, float winProbability, float mateDistance = 0f, int searchDepth = 0) | Updates position evaluation | chessBoard.UpdateEvaluation(150.5f, 0.65f, 0f, 12); |
| void | Method | public void ResetEvaluation() | Resets evaluation to neutral | chessBoard.ResetEvaluation(); |
| ChessRules.GameResult | Method | public ChessRules.GameResult GetGameResult() | Gets current game result | var result = chessBoard.GetGameResult(); |
| ChessBoard | Method | public ChessBoard Clone() | Creates deep copy | var clone = chessBoard.Clone(); |
| string | Method | public override string ToString() | Gets string representation | var str = chessBoard.ToString(); |
| void | Method | public static void RunAllTests() | Runs comprehensive test suite | ChessBoard.RunAllTests(); |

## Important Types

### `ChessBoard.ChessVariant`
* **Kind:** enum
* **Responsibility:** Defines supported chess variants for different game rules
* **Values:**
  * `Standard` — Regular chess rules
  * `Chess960` — Fischer Random Chess with shuffled starting positions
  * `KingOfTheHill` — Win by moving king to center squares
  * `Atomic` — Captures cause explosions
  * `ThreeCheck` — Win by giving three checks
  * `Horde` — White starts with many pawns
  * `RacingKings` — Race kings to 8th rank

### `ChessBoard.ValidationMode`
* **Kind:** enum
* **Responsibility:** Controls how strictly FEN positions are validated
* **Values:**
  * `Strict` — Must be legal chess position
  * `Permissive` — Allow rule-violating positions for testing
  * `ParseOnly` — Only check FEN syntax

### `ChessBoard.PositionInfo`
* **Kind:** struct
* **Responsibility:** Cached evaluation data for performance optimization
* **Constructor:** `public PositionInfo(ulong hash, float eval, float winProb, int depth)`
* **Public Properties:**
  * `hash` — `ulong` — Position Zobrist hash (`get/set`)
  * `evaluation` — `float` — Centipawn evaluation (`get/set`)
  * `winProbability` — `float` — Win probability 0-1 (`get/set`)
  * `depthSearched` — `int` — Search depth used (`get/set`)
  * `timestamp` — `float` — Cache timestamp (`get/set`)
  * `legalMoves` — `List<ChessMove>` — Legal moves cache (`get/set`)
  * `gameResult` — `ChessRules.GameResult` — Position result (`get/set`)
* **Public Methods:**
  * **`public bool IsValid()`**
    * Description: Validates cached position data
    * Returns: `bool` — true if cache entry valid + call example: `var isValid = posInfo.IsValid();`

### `ChessBoard.PGNMetadata`
* **Kind:** class
* **Responsibility:** Stores PGN game metadata and headers for complete game notation
* **Constructor:** `public PGNMetadata()` - initializes with default values
* **Public Properties:**
  * `Event` — `string` — Tournament/match name (`get/set`)
  * `Site` — `string` — Playing location (`get/set`)
  * `Date` — `string` — Game date (`get/set`)
  * `Round` — `string` — Round number (`get/set`)
  * `White` — `string` — White player name (`get/set`)
  * `Black` — `string` — Black player name (`get/set`)
  * `Result` — `string` — Game result (`get/set`)
  * `WhiteElo` — `string` — White player rating (`get/set`)
  * `BlackElo` — `string` — Black player rating (`get/set`)
  * `TimeControl` — `string` — Time control format (`get/set`)
  * `ECO` — `string` — Opening classification (`get/set`)
  * `Opening` — `string` — Opening name (`get/set`)
* **Public Methods:**
  * **`public bool IsValid()`**
    * Description: Validates required PGN metadata fields
    * Returns: `bool` — true if metadata complete + call example: `var valid = metadata.IsValid();`
  * **`public override string ToString()`**
    * Description: Formats metadata for display
    * Returns: `string` — formatted metadata string + call example: `var str = metadata.ToString();`

### `ChessBoard.GameTree`
* **Kind:** class
* **Responsibility:** Manages branching move history with variations and navigation
* **Public Properties:**
  * `GetNodes` — `List<ChessBoard.GameNode>` — All tree nodes for logging (`get`)
  * `CurrentNodeIndex` — `int` — Current position index (`get`)
  * `NodeCount` — `int` — Total nodes in tree (`get`)
  * `CurrentNode` — `ChessBoard.GameNode` — Current position node (`get`)
* **Public Methods:**
  * **`public ChessBoard.GameNode AddMove(ChessBoard.BoardState state, ChessMove move, string san, float evaluation, string comment = "")`**
    * Description: Adds move to current position creating new branch if needed
    * Parameters: `state : ChessBoard.BoardState — board state`, `move : ChessMove — chess move`, `san : string — algebraic notation`, `evaluation : float — position score`, `comment : string — move comment`
    * Returns: `ChessBoard.GameNode` — new tree node or null + call example: `var node = gameTree.AddMove(state, move, "e4", 0.5f);`
  * **`public bool GoToNode(int nodeIndex)`**
    * Description: Navigates to specific node in tree
    * Parameters: `nodeIndex : int — target node index`
    * Returns: `bool` — true if navigation successful + call example: `var success = gameTree.GoToNode(5);`
  * **`public List<ChessBoard.GameNode> GetMainLine()`**
    * Description: Gets path from root to current node
    * Returns: `List<ChessBoard.GameNode>` — main line moves + call example: `var mainLine = gameTree.GetMainLine();`
  * **`public List<List<ChessBoard.GameNode>> GetVariations(int fromNodeIndex = -1)`**
    * Description: Gets all variations from specified position
    * Parameters: `fromNodeIndex : int — starting node (-1 for current)`
    * Returns: `List<List<ChessBoard.GameNode>>` — list of variations + call example: `var variations = gameTree.GetVariations();`
  * **`public void Clear()`**
    * Description: Clears entire game tree
    * Returns: `void` + call example: `gameTree.Clear();`
  * **`public int FindPosition(ulong positionHash)`**
    * Description: Finds node with matching position hash
    * Parameters: `positionHash : ulong — Zobrist hash to find`
    * Returns: `int` — node index or -1 if not found + call example: `var index = gameTree.FindPosition(hash);`
  * **`public bool CanUndo()`**
    * Description: Checks if undo move is possible
    * Returns: `bool` — true if can undo + call example: `var canUndo = gameTree.CanUndo();`
  * **`public bool CanRedo()`**
    * Description: Checks if redo move is possible  
    * Returns: `bool` — true if can redo + call example: `var canRedo = gameTree.CanRedo();`

### `ChessBoard.GameNode`
* **Kind:** class
* **Responsibility:** Individual node in game tree containing move and position data
* **Constructor:** `public GameNode()` - initializes with default values
* **Public Properties:**
  * `state` — `ChessBoard.BoardState` — Board state at this position (`get/set`)
  * `move` — `ChessMove` — Move that led to this position (`get/set`)
  * `sanNotation` — `string` — Standard algebraic notation (`get/set`)
  * `evaluation` — `float` — Position evaluation (`get/set`)
  * `comment` — `string` — Move comment (`get/set`)
  * `parentIndex` — `int` — Parent node index (`get/set`)
  * `children` — `List<int>` — Child node indices (`get/set`)
  * `timestamp` — `float` — Creation timestamp (`get/set`)
  * `annotations` — `Dictionary<string, string>` — Additional annotations (`get/set`)
* **Public Methods:**
  * **`public bool IsValid()`**
    * Description: Validates node data integrity
    * Returns: `bool` — true if node data valid + call example: `var valid = node.IsValid();`
  * **`public override string ToString()`**
    * Description: Formats node for display
    * Returns: `string` — formatted node info + call example: `var str = node.ToString();`

### `ChessBoard.BoardState`
* **Kind:** struct
* **Responsibility:** Immutable snapshot of complete board position for history storage
* **Constructor:** `public BoardState(ChessBoard board)` - captures current board state
* **Public Properties:**
  * `fen` — `string` — FEN representation (`get/set`)
  * `sideToMove` — `char` — Current player (`get/set`)
  * `castlingRights` — `string` — Castling availability (`get/set`)
  * `enPassantSquare` — `string` — En passant target (`get/set`)
  * `halfmoveClock` — `int` — Half-move counter (`get/set`)
  * `fullmoveNumber` — `int` — Full move number (`get/set`)
  * `timestamp` — `float` — State timestamp (`get/set`)
  * `evaluation` — `float` — Position evaluation (`get/set`)
  * `winProbability` — `float` — Win probability (`get/set`)
  * `mateDistance` — `float` — Mate distance (`get/set`)
  * `positionHash` — `ulong` — Zobrist hash (`get/set`)
* **Public Methods:**
  * **`public bool IsValid()`**
    * Description: Validates board state completeness
    * Returns: `bool` — true if state data valid + call example: `var valid = state.IsValid();`

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
public class ChessBoardExample : MonoBehaviour 
{
    private void ChessBoard_Check()
    {
        // Test all major public APIs in minimal lines
        var board = new ChessBoard();
        var fenLoaded = board.LoadFromFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        var fen = board.ToFEN();
        var piece = board.GetPiece("e2");
        var moves = board.GetLegalMoves();
        var hash = board.CalculatePositionHash();
        board.UpdateEvaluation(150.5f, 0.65f, 0f, 12);
        var eval = board.LastEvaluation;
        var canUndo = board.CanUndo();
        var gameResult = board.GetGameResult();
        var clone = board.Clone();
        var boardString = board.ToString();
        ChessBoard.RunAllTests();
        
        Debug.Log($"API Results: FEN={fenLoaded}, piece={piece}, moves={moves.Count}, hash={hash}, eval={eval}, undo={canUndo}, result={gameResult}, clone created, tests run");
    }
}
```

## Control Flow & Responsibilities
Manages complete chess game state with move validation, history tracking, position caching, and engine integration.

## Performance & Threading
Heavy Zobrist hashing, move generation, position caching; all main-thread operations.

## Cross-file Dependencies
References ChessRules, MoveGenerator, ChessMove classes; requires SPACE_UTIL.v2 and Board<T>.

## Major Functionality
FEN parsing, move validation, undo/redo, game tree, position hashing, evaluation caching, variant support.

`checksum: A7B9C2D1 v0.3.min`