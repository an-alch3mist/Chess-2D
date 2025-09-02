# `ChessRules.cs` — Chess rules validation and game state evaluation with comprehensive promotion support

Unity C# class providing complete chess rule validation, move legality checking, and game state evaluation including checkmate, stalemate, and draw conditions.

## Short description (2–4 sentences)

This file implements comprehensive chess rules validation and game state evaluation for Unity chess applications. It provides methods for validating moves, detecting check/checkmate/stalemate, handling promotions, and evaluating game termination conditions including threefold repetition and insufficient material. The class includes extensive position validation for FEN inputs and supports Chess960 castling scenarios with enhanced testing coverage.

## Metadata

- **Filename:** `ChessRules.cs`
- **Primary namespace:** `GPTDeepResearch`
- **Estimated lines:** 750
- **Estimated chars:** 47,000
- **Public types:** `ChessRules, GameResult, EvaluationInfo`
- **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
- **Dependencies:** `SPACE_UTIL` (v2 struct), `ChessBoard.cs` (board representation, FEN parsing), `ChessMove.cs` (move objects), `MoveGenerator.cs` (legal move generation)

## Public API summary (table)

| Type | Member | Signature | Short purpose |
|------|--------|-----------|--------------|
| ChessRules | EvaluatePosition() | public static GameResult EvaluatePosition(ChessBoard board, List<string> moveHistory = null) | Determine game result (checkmate, stalemate, draw) |
| ChessRules | GetEvaluationInfo() | public static EvaluationInfo GetEvaluationInfo(ChessBoard board, float centipawns = 0f, float winProbability = 0.5f, float mateDistance = 0f) | Get UI display evaluation data |
| ChessRules | IsInCheck() | public static bool IsInCheck(ChessBoard board, char side) | Check if given side is in check |
| ChessRules | RequiresPromotion() | public static bool RequiresPromotion(ChessBoard board, ChessMove move) | Check if move requires promotion |
| ChessRules | ValidateMove() | public static bool ValidateMove(ChessBoard board, ChessMove move) | Validate move legality |
| ChessRules | ValidatePromotionMove() | public static bool ValidatePromotionMove(ChessBoard board, ChessMove move) | Enhanced promotion validation |
| ChessRules | MakeMove() | public static bool MakeMove(ChessBoard board, ChessMove move) | Apply move to board with state updates |
| ChessRules | DoesMoveCauseCheck() | public static bool DoesMoveCauseCheck(ChessBoard board, ChessMove move) | Check if move puts opponent in check |
| ChessRules | IsCheckingMove() | public static bool IsCheckingMove(ChessBoard board, ChessMove move) | Alias for DoesMoveCauseCheck |
| ChessRules | GetAttackingPieces() | public static List<v2> GetAttackingPieces(ChessBoard board, v2 square, char attackingSide) | Get all pieces attacking a square |
| ChessRules | FindKing() | public static v2 FindKing(ChessBoard board, char king) | Find king position on board |
| ChessRules | ValidatePosition() | public static bool ValidatePosition(ChessBoard board) | Comprehensive FEN position validation |
| ChessRules | RunAllTests() | public static void RunAllTests() | Execute comprehensive rule validation tests |
| GameResult | enum | InProgress, WhiteWins, BlackWins, Draw, Stalemate, InsufficientMaterial, FiftyMoveRule, ThreefoldRepetition | Game termination conditions |
| EvaluationInfo | GetDisplayText() | public string GetDisplayText() | Get formatted evaluation string for UI |

## Important types — details

### `ChessRules`
- **Kind:** static class
- **Responsibility:** Central authority for chess rule validation, move legality, and game state evaluation
- **Constructor(s):** N/A (static class)
- **Public properties / fields:** None (static utility class)
- **Public methods:**
  - **EvaluatePosition():** Determines complete game state with draw conditions
    - **Parameters:** board : ChessBoard — current position, moveHistory : List<string> — optional move sequence for repetition detection
    - **Returns:** GameResult — game termination state or InProgress
    - **Side effects:** Calls MoveGenerator.GenerateLegalMoves() for move validation
    - **Complexity:** O(n) where n is number of legal moves for checkmate/stalemate detection
  - **ValidateMove():** Complete move validation against chess rules
    - **Parameters:** board : ChessBoard — current position, move : ChessMove — move to validate
    - **Returns:** bool — true if move is legal
    - **Side effects:** Generates all legal moves for comparison, logs validation failures
    - **Notes:** Includes promotion validation, side-to-move verification, piece existence checks
  - **MakeMove():** Applies move to board with full state management
    - **Parameters:** board : ChessBoard — position to modify, move : ChessMove — validated move
    - **Returns:** bool — true if successfully applied
    - **Side effects:** Updates board state, castling rights, en passant, move counters, side to move
    - **Notes:** Handles castling, en passant, promotion moves with proper game state transitions

### `GameResult`
- **Kind:** enum
- **Responsibility:** Represents all possible chess game termination conditions
- **Values:** InProgress, WhiteWins, BlackWins, Draw, Stalemate, InsufficientMaterial, FiftyMoveRule, ThreefoldRepetition

### `EvaluationInfo`
- **Kind:** struct
- **Responsibility:** UI-friendly evaluation data container with formatted display support
- **Constructor(s):** Struct initialization with all fields
- **Public properties / fields:**
  - centipawns — float — position evaluation in centipawns
  - winProbability — float — 0-1 probability of current side winning
  - mateDistance — float — moves to mate (positive/negative for side advantage)
  - isCheckmate/isStalemate — bool — game termination flags
  - sideToMove — char — current player ('w'/'b')
- **Public methods:**
  - **GetDisplayText():** Returns formatted evaluation string for UI display
    - **Returns:** string — "Mate in 3", "+1.50", "Draw by stalemate", etc.

## Example usage

```csharp
// Evaluate game state
ChessBoard board = new ChessBoard("8/8/8/8/8/8/8/K6k w - - 100 1");
GameResult result = ChessRules.EvaluatePosition(board);
if (result == GameResult.FiftyMoveRule) {
    Debug.Log("Game drawn by fifty-move rule");
}

// Validate and apply moves with promotion
ChessMove move = ChessMove.FromUCI("e7e8q", board);
if (ChessRules.ValidateMove(board, move)) {
    if (ChessRules.RequiresPromotion(board, move)) {
        Debug.Log("Promotion move validated");
    }
    ChessRules.MakeMove(board, move);
}

// Check for threats
if (ChessRules.IsInCheck(board, 'w')) {
    List<v2> attackers = ChessRules.GetAttackingPieces(board, kingPos, 'b');
    Debug.Log($"White king in check from {attackers.Count} pieces");
}
```

## Control flow / responsibilities & high-level algorithm summary

The class operates as a stateless rule authority that validates chess positions and moves against FIDE rules. Main workflow: (1) EvaluatePosition generates legal moves and checks termination conditions, (2) ValidateMove verifies move legality by comparing against generated legal moves, (3) MakeMove applies validated moves with proper state transitions including castling rights and en passant updates.

Key algorithms include insufficient material detection (piece counting with special cases for opposite-color bishops), check detection via attack pattern validation, and comprehensive position validation for FEN inputs. Promotion validation ensures rank requirements and piece color matching. The class integrates with MoveGenerator for legal move computation and provides extensive logging for debugging.

## Side effects and I/O

- **Logging:** Extensive Debug.Log statements with color coding for validation results and move application
- **State modification:** MakeMove() permanently modifies ChessBoard state including piece positions, castling rights, en passant square, and move counters
- **Move generation:** Calls MoveGenerator.GenerateLegalMoves() which creates temporary move collections
- **Testing:** RunAllTests() executes comprehensive validation across multiple test scenarios

## Performance, allocations, and hotspots

- **Heavy operations:** Legal move generation for validation (O(pieces × moves)), insufficient material detection (O(64) board scan)
- **Allocations:** List<ChessMove> creation for legal moves, temporary ChessBoard.Clone() for move validation, string allocations for logging
- **Hotspots:** ValidateMove() generates all legal moves for comparison, EvaluatePosition() calls multiple validation methods
- **GC concerns:** Frequent List allocations during move validation, string concatenation in logging statements

## Threading / async considerations

- **Thread safety:** All methods are static and stateless, safe for concurrent access with different board instances
- **Unity main thread:** Debug.Log calls require Unity main thread execution
- **No async:** All operations are synchronous, no coroutines or Task usage
- **Race conditions:** None due to stateless design, board modifications are caller responsibility

## Security / safety / correctness concerns

- **Null safety:** Checks for null moveHistory parameter, validates board state before operations
- **Input validation:** Comprehensive FEN validation prevents malformed position processing
- **Bounds checking:** Position validation ensures pieces are within board boundaries
- **King safety:** FindKing() method prevents missing king errors that could crash validation
- **Exception handling:** Graceful handling of invalid coordinates and missing pieces

## Tests, debugging & observability

- **Built-in testing:** RunAllTests() provides comprehensive validation of all public methods
- **Logging:** Color-coded Debug.Log statements (green=success, red=error, cyan=info) throughout all operations
- **Test coverage:** Individual test methods for each major function with multiple scenarios
- **Validation:** Position validation tests include edge cases like missing kings, invalid pawn positions

## Cross-file references

- `ChessBoard.cs`: board representation, FEN parsing, CoordToAlgebraic(), ToFEN(), Clone()
- `ChessMove.cs`: move objects, FromUCI(), IsValid(), MoveType enum
- `MoveGenerator.cs`: GenerateLegalMoves(), IsSquareAttacked() for legal move computation
- `SPACE_UTIL`: v2 struct for 2D board coordinates

<!--
## TODO / Known limitations / Suggested improvements

- TODO: Complete threefold repetition implementation (currently simplified placeholder)
- TODO: Add support for Chess960 castling rights validation beyond standard positions
- Limitation: Threefold repetition requires position history tracking not fully implemented
- Limitation: Some logging may impact performance in release builds
- Suggested: Add caching for expensive legal move generation during validation
- Suggested: Implement position hashing for faster repetition detection
-->

## Appendix

**Key private methods:**
- `MovesAreEqual(ChessMove a, ChessMove b)`: Comprehensive move comparison including promotion pieces
- `UpdateCastlingRights(ChessBoard board, ChessMove move)`: Chess960-compatible castling right management
- `HasInsufficientMaterial(ChessBoard board)`: Detects K vs K, KB vs K, KN vs K, and opposite-color bishops
- `IsPathClear(ChessBoard board, v2 from, v2 to)`: Sliding piece path validation

**File checksum:** First 8 chars of conceptual SHA1: `e8f7a6b5`