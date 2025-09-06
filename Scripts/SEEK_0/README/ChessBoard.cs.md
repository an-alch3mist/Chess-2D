# Source: `ChessBoard.cs` — Enhanced chess board with comprehensive engine integration, validation, and testing

Enhanced chess board implementation supporting legal move generation, promotion handling, undo/redo functionality, position caching, game tree navigation, and comprehensive evaluation metrics for chess engine integration.

## Short description (2–4 sentences)

This file implements a comprehensive chess board system with advanced features for chess engine integration. It handles board state management, move validation, game tree navigation with branching variations, position caching using Zobrist hashing, and extensive evaluation metrics. The system supports multiple chess variants, PGN notation, threefold repetition detection, and includes comprehensive API validation testing.

## Metadata

* **Filename:** `ChessBoard.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `using SPACE_UTIL;` (external namespace), `using System;`, `using System.Collections.Generic;`, `using System.Linq;`, `using System.Text;`, `using UnityEngine;`
* **Estimated lines:** 2500
* **Estimated chars:** 95000
* **Public types:** `ChessBoard (class)`, `ChessBoard.ChessVariant (enum)`, `ChessBoard.PositionInfo (struct)`, `ChessBoard.PGNMetadata (class)`, `ChessBoard.GameTree (class)`, `ChessBoard.GameNode (class)`, `ChessBoard.BoardState (struct)`
* **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
* **Dependencies:** `SPACE_UTIL.v2` (SPACE_UTIL is external namespace), `SPACE_UTIL.Board<T>` (SPACE_UTIL is external namespace), `ChessRules`, `MoveGenerator`, `ChessMove`

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|------------------|-------------------|
| Board<char> | board | public Board<char> board | Chess board representation | var piece = chessBoard.board.GT(new v2(x, y)); |
| char | sideToMove | public char sideToMove | Current side to move ('w' or 'b') | char side = chessBoard.sideToMove; |
| string | castlingRights | public string castlingRights | Castling availability | string rights = chessBoard.castlingRights; |
| string | enPassantSquare | public string enPassantSquare | En passant target square | string epSquare = chessBoard.enPassantSquare; |
| int | halfmoveClock | public int halfmoveClock | Halfmove clock for 50-move rule | int clock = chessBoard.halfmoveClock; |
| int | fullmoveNumber | public int fullmoveNumber | Full move number | int moveNum = chessBoard.fullmoveNumber; |
| char | humanSide | public char humanSide | Human player side | char side = chessBoard.humanSide; |
| char | engineSide | public char engineSide | Engine player side | char side = chessBoard.engineSide; |
| bool | allowSideSwitching | public bool allowSideSwitching | Allow switching sides | bool canSwitch = chessBoard.allowSideSwitching; |
| ChessBoard.ChessVariant (enum) | variant | public ChessBoard.ChessVariant variant | Chess variant type | var variant = chessBoard.variant; |
| float | LastEvaluation | public float LastEvaluation { get; } | Last position evaluation | float eval = chessBoard.LastEvaluation; |
| float | LastWinProbability | public float LastWinProbability { get; } | Last win probability | float winProb = chessBoard.LastWinProbability; |
| float | LastMateDistance | public float LastMateDistance { get; } | Last mate distance | float mateDist = chessBoard.LastMateDistance; |
| int | LastEvaluationDepth | public int LastEvaluationDepth { get; } | Last evaluation depth | int depth = chessBoard.LastEvaluationDepth; |
| int | GameTreeNodeCount | public int GameTreeNodeCount { get; } | Game tree node count | int nodes = chessBoard.GameTreeNodeCount; |
| int | CurrentHistoryIndex | public int CurrentHistoryIndex { get; } | Current history index | int index = chessBoard.CurrentHistoryIndex; |
| void | ChessBoard() | public ChessBoard() | Default constructor | var board = new ChessBoard(); |
| void | ChessBoard(string, ChessBoard.ChessVariant) | public ChessBoard(string fen, ChessBoard.ChessVariant variant = ChessBoard.ChessVariant.Standard) | Constructor with FEN | var board = new ChessBoard("fen", ChessBoard.ChessVariant.Standard); |
| void | SetupStartingPosition() | public void SetupStartingPosition() | Setup starting position | chessBoard.SetupStartingPosition(); |
| ulong | CalculatePositionHash() | public ulong CalculatePositionHash() | Calculate Zobrist hash | ulong hash = chessBoard.CalculatePositionHash(); |
| void | SaveCurrentState() | public void SaveCurrentState() | Save current state to game tree | chessBoard.SaveCurrentState(); |
| bool | MakeMove(ChessMove, string) | public bool MakeMove(ChessMove move, string comment = "") | Make and validate move | bool success = chessBoard.MakeMove(move, "comment"); |
| bool | UndoMove() | public bool UndoMove() | Undo last move | bool success = chessBoard.UndoMove(); |
| bool | RedoMove() | public bool RedoMove() | Redo next move | bool success = chessBoard.RedoMove(); |
| bool | GoToVariation(int) | public bool GoToVariation(int variationIndex) | Navigate to variation | bool success = chessBoard.GoToVariation(0); |
| bool | CanUndo() | public bool CanUndo() | Check if can undo | bool canUndo = chessBoard.CanUndo(); |
| bool | CanRedo() | public bool CanRedo() | Check if can redo | bool canRedo = chessBoard.CanRedo(); |
| ChessBoard.PositionInfo? | GetCachedPositionInfo() | public ChessBoard.PositionInfo? GetCachedPositionInfo() | Get cached position info | var info = chessBoard.GetCachedPositionInfo(); |
| bool | IsThreefoldRepetition() | public bool IsThreefoldRepetition() | Check threefold repetition | bool isRep = chessBoard.IsThreefoldRepetition(); |
| bool | LoadFromFEN(string) | public bool LoadFromFEN(string fen) | Load position from FEN | bool success = chessBoard.LoadFromFEN("fen"); |
| string | ToFEN() | public string ToFEN() | Convert to FEN string | string fen = chessBoard.ToFEN(); |
| void | SetHumanSide(char) | public void SetHumanSide(char side) | Set human player side | chessBoard.SetHumanSide('w'); |
| string | GetSideName(char) | public string GetSideName(char side) | Get human-readable side name | string name = chessBoard.GetSideName('w'); |
| bool | IsHumanTurn() | public bool IsHumanTurn() | Check if human's turn | bool isHuman = chessBoard.IsHumanTurn(); |
| bool | IsEngineTurn() | public bool IsEngineTurn() | Check if engine's turn | bool isEngine = chessBoard.IsEngineTurn(); |
| char | GetPiece(string) | public char GetPiece(string square) | Get piece at algebraic square | char piece = chessBoard.GetPiece("e4"); |
| char | GetPiece(v2) | public char GetPiece(v2 coord) | Get piece at coordinate | char piece = chessBoard.GetPiece(new v2(4, 3)); |
| void | SetPiece(v2, char) | public void SetPiece(v2 coord, char piece) | Set piece at coordinate | chessBoard.SetPiece(new v2(4, 3), 'Q'); |
| v2 | AlgebraicToCoord(string) | public static v2 AlgebraicToCoord(string square) | Convert algebraic to coordinate | v2 coord = ChessBoard.AlgebraicToCoord("e4"); |
| string | CoordToAlgebraic(v2) | public static string CoordToAlgebraic(v2 coord) | Convert coordinate to algebraic | string square = ChessBoard.CoordToAlgebraic(new v2(4, 3)); |
| List<ChessMove> | GetLegalMoves() | public List<ChessMove> GetLegalMoves() | Get all legal moves | var moves = chessBoard.GetLegalMoves(); |
| void | UpdateEvaluation(float, float, float, int) | public void UpdateEvaluation(float centipawnScore, float winProbability, float mateDistance = 0f, int searchDepth = 0) | Update evaluation data | chessBoard.UpdateEvaluation(150f, 0.65f, 0f, 12); |
| void | ResetEvaluation() | public void ResetEvaluation() | Reset evaluation to neutral | chessBoard.ResetEvaluation(); |
| ChessRules.GameResult | GetGameResult() | public ChessRules.GameResult GetGameResult() | Get current game result | var result = chessBoard.GetGameResult(); |
| ChessBoard | Clone() | public ChessBoard Clone() | Create deep copy | var clone = chessBoard.Clone(); |
| void | RunAllTests() | public static void RunAllTests() | Run comprehensive test suite | ChessBoard.RunAllTests(); |

## Important types — details

### `ChessBoard` (class)
* **Kind:** class implementing ICloneable
* **Responsibility:** Main chess board implementation with engine integration, move validation, game tree management, and comprehensive testing
* **Constructor(s):** 
  - `public ChessBoard()` — Creates board with starting position
  - `public ChessBoard(string fen, ChessBoard.ChessVariant variant = ChessBoard.ChessVariant.Standard)` — Creates board from FEN string
* **Public properties / fields:**
  - `board` — Board<char> — 8x8 chess board representation (get/set)
  - `sideToMove` — char — Current side to move ('w' or 'b') (get/set)
  - `castlingRights` — string — Castling availability (KQkq format) (get/set)
  - `enPassantSquare` — string — En passant target square (get/set)
  - `halfmoveClock` — int — Halfmove clock for 50-move rule (get/set)
  - `fullmoveNumber` — int — Full move number (get/set)
  - `humanSide` — char — Human player side (get/set)
  - `engineSide` — char — Engine player side (get/set)
  - `allowSideSwitching` — bool — Allow switching sides during game (get/set)
  - `variant` — ChessBoard.ChessVariant — Chess variant type (get/set)
  - `LastEvaluation` — float — Last position evaluation in centipawns (get)
  - `LastWinProbability` — float — Last win probability (0.0-1.0) (get)
  - `LastMateDistance` — float — Last mate distance (get)
  - `LastEvaluationDepth` — int — Last evaluation search depth (get)
  - `GameTreeNodeCount` — int — Number of nodes in game tree (get)
  - `CurrentHistoryIndex` — int — Current position index in game tree (get)

* **Public methods:**
  - **Signature:** `public void SetupStartingPosition()`
    **Description:** Initialize board to starting position based on variant
    **Parameters:** none
    **Returns:** void — ChessBoard.SetupStartingPosition()
    **Side effects:** Resets board state, clears evaluation
    
  - **Signature:** `public ulong CalculatePositionHash()`
    **Description:** Calculate Zobrist hash for current position
    **Parameters:** none
    **Returns:** ulong hash = ChessBoard.CalculatePositionHash()
    **Complexity:** O(64) — iterates through all squares
    
  - **Signature:** `public bool MakeMove(ChessMove move, string comment = "")`
    **Description:** Validate and execute chess move with game tree update
    **Parameters:** 
      - move : ChessMove — Move to execute
      - comment : string — Optional move comment
    **Returns:** bool success = ChessBoard.MakeMove(move, "comment")
    **Side effects:** Updates board state, game tree, position cache
    **Throws:** Logs validation errors to Debug
    
  - **Signature:** `public bool UndoMove()`
    **Description:** Navigate to previous position in game tree
    **Parameters:** none
    **Returns:** bool success = ChessBoard.UndoMove()
    **Side effects:** Restores previous board state
    
  - **Signature:** `public bool RedoMove()`
    **Description:** Navigate forward in main line
    **Parameters:** none
    **Returns:** bool success = ChessBoard.RedoMove()
    **Side effects:** Advances to next position
    
  - **Signature:** `public bool LoadFromFEN(string fen)`
    **Description:** Load board position from FEN notation with validation
    **Parameters:** fen : string — FEN string to parse
    **Returns:** bool success = ChessBoard.LoadFromFEN("fen")
    **Throws:** Logs parsing errors to Debug
    **Side effects:** Replaces current board state
    
  - **Signature:** `public string ToFEN()`
    **Description:** Convert current position to FEN notation
    **Parameters:** none
    **Returns:** string fen = ChessBoard.ToFEN()
    
  - **Signature:** `public void SetHumanSide(char side)`
    **Description:** Set human player side with validation
    **Parameters:** side : char — Player side ('w', 'b', or 'x')
    **Returns:** void — ChessBoard.SetHumanSide('w')
    **Side effects:** Updates humanSide and engineSide properties
    
  - **Signature:** `public char GetPiece(string square)`
    **Description:** Get piece at algebraic coordinate with bounds checking
    **Parameters:** square : string — Algebraic notation (e.g., "e4")
    **Returns:** char piece = ChessBoard.GetPiece("e4")
    
  - **Signature:** `public char GetPiece(v2 coord)`
    **Description:** Get piece at coordinate with bounds checking
    **Parameters:** coord : v2 — Board coordinate
    **Returns:** char piece = ChessBoard.GetPiece(new v2(4, 3))
    
  - **Signature:** `public void SetPiece(v2 coord, char piece)`
    **Description:** Set piece at coordinate with validation
    **Parameters:** 
      - coord : v2 — Board coordinate
      - piece : char — Piece character or '.' for empty
    **Returns:** void — ChessBoard.SetPiece(new v2(4, 3), 'Q')
    **Side effects:** Modifies board state
    
  - **Signature:** `public static v2 AlgebraicToCoord(string square)`
    **Description:** Convert algebraic notation to board coordinate
    **Parameters:** square : string — Algebraic notation
    **Returns:** v2 coord = ChessBoard.AlgebraicToCoord("e4")
    
  - **Signature:** `public static string CoordToAlgebraic(v2 coord)`
    **Description:** Convert board coordinate to algebraic notation
    **Parameters:** coord : v2 — Board coordinate
    **Returns:** string square = ChessBoard.CoordToAlgebraic(new v2(4, 3))
    
  - **Signature:** `public List<ChessMove> GetLegalMoves()`
    **Description:** Generate all legal moves for current position
    **Parameters:** none
    **Returns:** List<ChessMove> moves = ChessBoard.GetLegalMoves()
    **Complexity:** O(n²) — depends on position complexity
    
  - **Signature:** `public void UpdateEvaluation(float centipawnScore, float winProbability, float mateDistance = 0f, int searchDepth = 0)`
    **Description:** Update position evaluation metrics with validation
    **Parameters:** 
      - centipawnScore : float — Evaluation in centipawns
      - winProbability : float — Win probability (0.0-1.0)
      - mateDistance : float — Distance to mate
      - searchDepth : int — Search depth used
    **Returns:** void — ChessBoard.UpdateEvaluation(150f, 0.65f, 0f, 12)
    **Side effects:** Updates evaluation cache, logs to Debug
    
  - **Signature:** `public ChessRules.GameResult GetGameResult()`
    **Description:** Evaluate current position for game termination
    **Parameters:** none
    **Returns:** ChessRules.GameResult result = ChessBoard.GetGameResult()
    
  - **Signature:** `public ChessBoard Clone()`
    **Description:** Create deep copy of board including all state
    **Parameters:** none
    **Returns:** ChessBoard clone = ChessBoard.Clone()
    **Complexity:** O(n) — copies all board data
    
  - **Signature:** `public static void RunAllTests()`
    **Description:** Execute comprehensive API validation test suite
    **Parameters:** none
    **Returns:** void — ChessBoard.RunAllTests()
    **Side effects:** Logs test results to Debug console

### `ChessBoard.ChessVariant` (enum)
* **Kind:** enum
* **Responsibility:** Define supported chess variants
* **Values:** Standard, Chess960, KingOfTheHill, Atomic, ThreeCheck, Horde, RacingKings

### `ChessBoard.PositionInfo` (struct)
* **Kind:** struct
* **Responsibility:** Cached position information for performance optimization
* **Constructor(s):** `public PositionInfo(ulong hash, float eval, float winProb, int depth)`
* **Public properties / fields:**
  - `hash` — ulong — Position Zobrist hash (get/set)
  - `evaluation` — float — Cached evaluation score (get/set)
  - `winProbability` — float — Cached win probability (get/set)
  - `depthSearched` — int — Search depth used (get/set)
  - `timestamp` — float — Cache timestamp (get/set)
  - `legalMoves` — List<ChessMove> — Cached legal moves (get/set)
  - `gameResult` — ChessRules.GameResult — Cached game result (get/set)
* **Public methods:**
  - **Signature:** `public bool IsValid()`
    **Description:** Check if cached data is valid
    **Returns:** bool valid = posInfo.IsValid()

### `ChessBoard.PGNMetadata` (class)
* **Kind:** class
* **Responsibility:** Store PGN game metadata and headers
* **Constructor(s):** `public PGNMetadata()` — Initializes with default values
* **Public properties / fields:**
  - `Event` — string — Tournament/match name (get/set)
  - `Site` — string — Game location (get/set)
  - `Date` — string — Game date in yyyy.MM.dd format (get/set)
  - `Round` — string — Round number (get/set)
  - `White` — string — White player name (get/set)
  - `Black` — string — Black player name (get/set)
  - `Result` — string — Game result (* for ongoing) (get/set)
  - `WhiteElo` — string — White player rating (get/set)
  - `BlackElo` — string — Black player rating (get/set)
  - `TimeControl` — string — Time control format (get/set)
  - `ECO` — string — Opening ECO code (get/set)
  - `Opening` — string — Opening name (get/set)
* **Public methods:**
  - **Signature:** `public bool IsValid()`
    **Description:** Validate required PGN metadata fields
    **Returns:** bool valid = metadata.IsValid()

### `ChessBoard.GameTree` (class)
* **Kind:** class
* **Responsibility:** Manage game tree with branching variations and navigation
* **Constructor(s):** Default parameterless constructor
* **Public properties / fields:**
  - `CurrentNodeIndex` — int — Index of current position (get)
  - `NodeCount` — int — Total number of nodes (get)
  - `CurrentNode` — ChessBoard.GameNode — Current game node (get)
* **Public methods:**
  - **Signature:** `public ChessBoard.GameNode AddMove(ChessBoard.BoardState state, ChessMove move, string san, float evaluation, string comment = "")`
    **Description:** Add new move to current position, creating branch if needed
    **Parameters:** 
      - state : ChessBoard.BoardState — New board state
      - move : ChessMove — Move that led to state
      - san : string — Standard algebraic notation
      - evaluation : float — Position evaluation
      - comment : string — Move comment
    **Returns:** ChessBoard.GameNode node = gameTree.AddMove(state, move, san, eval, comment)
    **Side effects:** Updates game tree structure, position mapping
    
  - **Signature:** `public bool GoToNode(int nodeIndex)`
    **Description:** Navigate to specific node in game tree
    **Parameters:** nodeIndex : int — Target node index
    **Returns:** bool success = gameTree.GoToNode(5)
    
  - **Signature:** `public List<ChessBoard.GameNode> GetMainLine()`
    **Description:** Get path from root to current node
    **Returns:** List<ChessBoard.GameNode> path = gameTree.GetMainLine()
    
  - **Signature:** `public List<List<ChessBoard.GameNode>> GetVariations(int fromNodeIndex = -1)`
    **Description:** Get all variations from specified position
    **Parameters:** fromNodeIndex : int — Starting node (-1 for current)
    **Returns:** List<List<ChessBoard.GameNode>> variations = gameTree.GetVariations()

### `ChessBoard.GameNode` (class)
* **Kind:** class
* **Responsibility:** Individual node in game tree with move and state data
* **Constructor(s):** Default parameterless constructor
* **Public properties / fields:**
  - `state` — ChessBoard.BoardState — Board state at this position (get/set)
  - `move` — ChessMove — Move that led to this position (get/set)
  - `sanNotation` — string — Standard algebraic notation (get/set)
  - `evaluation` — float — Position evaluation (get/set)
  - `comment` — string — Move/position comment (get/set)
  - `parentIndex` — int — Index of parent node (get/set)
  - `children` — List<int> — Indices of child nodes (get/set)
  - `timestamp` — float — Creation timestamp (get/set)
  - `annotations` — Dictionary<string, string> — Engine annotations (get/set)
* **Public methods:**
  - **Signature:** `public bool IsValid()`
    **Description:** Check if node data is valid
    **Returns:** bool valid = node.IsValid()

### `ChessBoard.BoardState` (struct)
* **Kind:** struct
* **Responsibility:** Complete board state snapshot with evaluation data
* **Constructor(s):** `public BoardState(ChessBoard board)` — Creates state from board
* **Public properties / fields:**
  - `fen` — string — FEN representation (get/set)
  - `sideToMove` — char — Current side to move (get/set)
  - `castlingRights` — string — Castling availability (get/set)
  - `enPassantSquare` — string — En passant target (get/set)
  - `halfmoveClock` — int — Halfmove clock (get/set)
  - `fullmoveNumber` — int — Full move number (get/set)
  - `timestamp` — float — State timestamp (get/set)
  - `evaluation` — float — Position evaluation (get/set)
  - `winProbability` — float — Win probability (get/set)
  - `mateDistance` — float — Mate distance (get/set)
  - `positionHash` — ulong — Zobrist hash (get/set)
* **Public methods:**
  - **Signature:** `public bool IsValid()`
    **Description:** Validate board state completeness
    **Returns:** bool valid = state.IsValid()

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
        // Basic board creation and setup
        var board = new ChessBoard();
        Debug.Log($"<color=green>Created board with FEN: {board.ToFEN()}</color>");
        // Expected output: "Created board with FEN: rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        
        // Load custom position
        bool loadSuccess = board.LoadFromFEN("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");
        Debug.Log($"<color=green>Load custom FEN success: {loadSuccess}</color>");
        // Expected output: "Load custom FEN success: True"
        
        // Set player sides
        board.SetHumanSide('w');
        Debug.Log($"<color=green>Human: {board.GetSideName(board.humanSide)}, Engine: {board.GetSideName(board.engineSide)}</color>");
        // Expected output: "Human: White, Engine: Black"
        
        // Get piece information
        char piece = board.GetPiece("e1");
        Debug.Log($"<color=green>Piece at e1: {piece}</color>");
        // Expected output: "Piece at e1: K"
        
        // Coordinate conversion
        v2 coord = ChessBoard.AlgebraicToCoord("e4");
        string square = ChessBoard.CoordToAlgebraic(new v2(4, 3));
        Debug.Log($"<color=green>e4 -> {coord}, (4,3) -> {square}</color>");
        // Expected output: "e4 -> (4, 3), (4,3) -> e4"
        
        // Generate legal moves
        var legalMoves = board.GetLegalMoves();
        Debug.Log($"<color=green>Legal moves available: {legalMoves.Count}</color>");
        // Expected output: "Legal moves available: [varies by position]"
        
        // Make a move
        if (legalMoves.Count > 0)
        {
            bool moveSuccess = board.MakeMove(legalMoves[0], "Opening move");
            Debug.Log($"<color=green>Move made successfully: {moveSuccess}</color>");
            // Expected output: "Move made successfully: True"
        }
        
        // Update evaluation
        board.UpdateEvaluation(25.5f, 0.55f, 0f, 10);
        Debug.Log($"<color=green>Evaluation: {board.LastEvaluation}cp, Win prob: {board.LastWinProbability:F2}</color>");
        // Expected output: "Evaluation: 25.5cp, Win prob: 0.55"
        
        // Test undo/redo functionality
        bool canUndo = board.CanUndo();
        if (canUndo)
        {
            bool undoSuccess = board.UndoMove();
            Debug.Log($"<color=green>Undo successful: {undoSuccess}</color>");
            // Expected output: "Undo successful: True"
            
            bool canRedo = board.CanRedo();
            if (canRedo)
            {
                bool redoSuccess = board.RedoMove();
                Debug.Log($"<color=green>Redo successful: {redoSuccess}</color>");
                // Expected output: "Redo successful: True"
            }
        }
        
        // Position hashing
        ulong posHash = board.CalculatePositionHash();
        Debug.Log($"<color=green>Position hash: {posHash:X}</color>");
        // Expected output: "Position hash: [hexadecimal hash]"
        
        // Cache information
        var cachedInfo = board.GetCachedPositionInfo();
        if (cachedInfo.HasValue)
        {
            Debug.Log($"<color=green>Cached evaluation: {cachedInfo.Value.evaluation}</color>");
            // Expected output: "Cached evaluation: 25.5"
        }
        
        // Game tree information
        Debug.Log($"<color=green>Game tree nodes: {board.GameTreeNodeCount}, Current index: {board.CurrentHistoryIndex}</color>");
        // Expected output: "Game tree nodes: [count], Current index: [index]"
        
        // Check game result
        var gameResult = board.GetGameResult();
        Debug.Log($"<color=green>Game result: {gameResult}</color>");
        // Expected output: "Game result: InProgress"
        
        // Board cloning
        var clonedBoard = board.Clone();
        bool samePosition = clonedBoard.ToFEN() == board.ToFEN();
        Debug.Log($"<color=green>Clone has same position: {samePosition}</color>");
        // Expected output: "Clone has same position: True"
        
        // Variant support
        var chess960Board = new ChessBoard("", ChessBoard.ChessVariant.Chess960);
        Debug.Log($"<color=green>Chess960 variant created with FEN: {chess960Board.ToFEN().Substring(0, 40)}...</color>");
        // Expected output: "Chess960 variant created with FEN: [shuffled back rank]..."
        
        // Nested class usage - PGN metadata
        var metadata = new ChessBoard.PGNMetadata();
        metadata.Event = "Test Game";
        metadata.White = "Human";
        metadata.Black = "Engine";
        bool metadataValid = metadata.IsValid();
        Debug.Log($"<color=green>PGN metadata valid: {metadataValid}</color>");
        // Expected output: "PGN metadata valid: True"
        
        // Position validation
        var posInfo = new ChessBoard.PositionInfo(12345UL, 50f, 0.6f, 8);
        bool posValid = posInfo.IsValid();
        Debug.Log($"<color=green>Position info valid: {posValid}</color>");
        // Expected output: "Position info valid: True"
        
        // Run comprehensive tests
        Debug.Log("<color=cyan>Running comprehensive API tests...</color>");
        ChessBoard.RunAllTests();
        // Expected output: Multiple test result logs with color-coded pass/fail indicators
    }
}
```

## Control flow / responsibilities & high-level algorithm summary

Main runtime flow: Initialize Zobrist keys → Setup starting position → Save initial state. Move execution: Validate via ChessRules → Apply move → Generate SAN notation → Update game tree → Cache position. Navigation uses game tree with branching support. Zobrist hashing enables fast position comparison and threefold repetition detection.

## Performance, allocations, and hotspots

Heavy operations: Legal move generation O(n²), Zobrist hash calculation O(64), position cache pruning. Game tree can grow large. Move validation involves rule checking. Debug logging on all operations.

## Threading / async considerations

Unity main-thread only. No async operations. Coroutines not used. Position caching uses timestamps for pruning.

## Security / safety / correctness concerns

Null reference risks on invalid FEN parsing, array bounds exceptions on coordinate access. Heavy use of try-catch with Debug logging may mask critical errors. Zobrist key initialization uses fixed seed - could be predictable. Position cache unbounded growth potential despite pruning logic.

## Tests, debugging & observability

Comprehensive test suite via RunAllTests() with color-coded Debug logging. FEN validation, move operations, evaluation system, position hashing, side management, piece access, notation conversion, legal moves, game results, cloning, and repetition detection. Built-in validation throughout all methods.

## Cross-file references

Dependencies: `ChessRules` for move validation and game result evaluation, `MoveGenerator` for legal move generation, `ChessMove` for move representation, `SPACE_UTIL.v2` for coordinates, `SPACE_UTIL.Board<T>` for board storage.

## TODO / Known limitations / Suggested improvements

<!-- TODO items from code comments and practical improvements:
- Enhanced evaluation system with engine integration support mentioned in changelog
- Performance optimizations for move history mentioned but not fully detailed  
- Chess960 position generation uses basic shuffle, could use proper algorithm
- PGN export functionality not not implemented despite metadata support
- Variant-specific rules (King of Hill, Atomic, etc.) only partially implemented
- Position cache size management could be more sophisticated
- Threading considerations for engine integration not addressed -->

## Appendix

Key private helpers: `ParseBoardPosition()` validates FEN board section, `ValidateBoardState()` ensures legal piece counts and positions, `InitializeZobristKeys()` sets up position hashing, `UpdatePositionCache()` manages cached evaluations. Zobrist hashing uses 64×12 piece keys + castling/en passant keys for collision-resistant position comparison.

## General Note: important behaviors

Major functionalities: PawnPromotion support via ChessMove, comprehensive Undo/Redo with game tree navigation and branching variations, position caching with Zobrist hashing, threefold repetition detection, FEN Save/Load with validation, chess variant support, comprehensive API testing suite, evaluation metrics for engine integration.

`checksum: A7B8C9D2 v0.3`