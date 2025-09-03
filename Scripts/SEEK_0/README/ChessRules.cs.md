# Source: `ChessRules.cs` — Chess rules validation and game state evaluation with comprehensive promotion and evaluation support

## Short description

Implements comprehensive chess rule validation, move application, and game state evaluation. Handles all chess rules including castling, en passant, promotion, check detection, and game termination conditions. Provides extensive testing and validation for any chess position.

## Metadata

* **Filename:** `ChessRules.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `System, System.Collections.Generic, System.Linq, UnityEngine, SPACE_UTIL`
* **Estimated lines:** 1500
* **Estimated chars:** 60000
* **Public types:** `ChessRules (static class), ChessRules.GameResult (enum), ChessRules.EvaluationInfo (struct)`
* **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
* **Dependencies:** `SPACE_UTIL.v2` (SPACE_UTIL is external namespace), `ChessBoard.cs`, `MoveGenerator.cs`, `ChessMove.cs`

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| ChessRules.GameResult (enum) | EvaluatePosition | `public static GameResult EvaluatePosition(ChessBoard board, List<string> moveHistory = null)` | Evaluate current game state | `var result = ChessRules.EvaluatePosition(board, history)` |
| ChessRules.EvaluationInfo (struct) | GetEvaluationInfo | `public static EvaluationInfo GetEvaluationInfo(ChessBoard board, float centipawns = 0f, float winProbability = 0.5f, float mateDistance = 0f)` | Get evaluation info for UI | `var info = ChessRules.GetEvaluationInfo(board, 50f, 0.6f)` |
| bool | IsInCheck | `public static bool IsInCheck(ChessBoard board, char side)` | Check if given side is in check | `bool inCheck = ChessRules.IsInCheck(board, 'w')` |
| bool | RequiresPromotion | `public static bool RequiresPromotion(ChessBoard board, ChessMove move)` | Check if move requires promotion | `bool needsPromotion = ChessRules.RequiresPromotion(board, move)` |
| bool | ValidateMove | `public static bool ValidateMove(ChessBoard board, ChessMove move)` | Validate if move is legal | `bool isLegal = ChessRules.ValidateMove(board, move)` |
| bool | ValidatePromotionMove | `public static bool ValidatePromotionMove(ChessBoard board, ChessMove move)` | Validate promotion move requirements | `bool validPromotion = ChessRules.ValidatePromotionMove(board, move)` |
| bool | MakeMove | `public static bool MakeMove(ChessBoard board, ChessMove move)` | Apply move to board and update state | `bool success = ChessRules.MakeMove(board, move)` |
| bool | DoesMoveCauseCheck | `public static bool DoesMoveCauseCheck(ChessBoard board, ChessMove move)` | Check if move puts opponent in check | `bool causesCheck = ChessRules.DoesMoveCauseCheck(board, move)` |
| bool | IsCheckingMove | `public static bool IsCheckingMove(ChessBoard board, ChessMove move)` | Check if move is a checking move | `bool isChecking = ChessRules.IsCheckingMove(board, move)` |
| List<v2> | GetAttackingPieces | `public static List<v2> GetAttackingPieces(ChessBoard board, v2 square, char attackingSide)` | Get all pieces attacking a square | `var attackers = ChessRules.GetAttackingPieces(board, square, 'w')` |
| v2 (struct) | FindKing | `public static v2 FindKing(ChessBoard board, char king)` | Find king position on board | `v2 kingPos = ChessRules.FindKing(board, 'K')` |
| bool | ValidatePosition | `public static bool ValidatePosition(ChessBoard board)` | Comprehensive position validation | `bool isValid = ChessRules.ValidatePosition(board)` |
| void | RunAllTests | `public static void RunAllTests()` | Run comprehensive rule tests | `ChessRules.RunAllTests()` |

## Important types — details

### `ChessRules`
* **Kind:** static class
* **Responsibility:** Chess rules validation, move application, and game state evaluation
* **Public methods:**
  - **Signature:** `public static GameResult EvaluatePosition(ChessBoard board, List<string> moveHistory = null)`
    - **Description:** Evaluate current game state for checkmate, stalemate, draws
    - **Parameters:** board : ChessBoard — current position, moveHistory : List<string> — move history for repetition check
    - **Returns:** ChessRules.GameResult — game termination state, `var result = ChessRules.EvaluatePosition(board)`
    - **Complexity / performance:** O(64) board scan for each piece type
  - **Signature:** `public static v2 FindKing(ChessBoard board, char king)`
    - **Description:** Locate king position on the board
    - **Parameters:** board : ChessBoard — current position, king : char — king piece ('K' or 'k')
    - **Returns:** v2 — king position or (-1,-1) if not found, `v2 pos = ChessRules.FindKing(board, 'K')`
    - **Complexity / performance:** O(64) worst case board scan
  - **Signature:** `public static bool ValidatePosition(ChessBoard board)`
    - **Description:** Comprehensive validation for any chess position
    - **Parameters:** board : ChessBoard — position to validate
    - **Returns:** bool — position validity, `bool valid = ChessRules.ValidatePosition(board)`
    - **Notes:** Validates king count, pawn placement, piece limits
  - **Signature:** `public static void RunAllTests()`
    - **Description:** Execute comprehensive rule validation test suite
    - **Side effects / state changes:** Debug logging of test results
    - **Notes:** Tests all public methods with various positions

### `ChessRules.GameResult`
* **Kind:** enum (inside ChessRules static class)
* **Responsibility:** Game termination states
* **Values:** InProgress, WhiteWins, BlackWins, Draw, Stalemate, InsufficientMaterial, FiftyMoveRule, ThreefoldRepetition

### `ChessRules.EvaluationInfo`
* **Kind:** struct (inside ChessRules static class)
* **Responsibility:** UI-ready evaluation information with display formatting
* **Public properties / fields:**
  - `centipawns` — float — Position evaluation in centipawns (get/set)
  - `winProbability` — float — Win probability 0-1 (get/set)
  - `mateDistance` — float — Moves to mate if applicable (get/set)
  - `isCheckmate` — bool — Position is checkmate (get/set)
  - `isStalemate` — bool — Position is stalemate (get/set)
  - `sideToMove` — char — Current side to move (get/set)
* **Public methods:**
  - **Signature:** `public string GetDisplayText()`
    - **Description:** Format evaluation for UI display
    - **Returns:** string — formatted evaluation text, `string display = info.GetDisplayText()`

## Example usage

```csharp
// namespace GPTDeepResearch required
using GPTDeepResearch;
using SPACE_UTIL;

// Validate a move
var board = new ChessBoard();
var move = ChessMove.FromUCI("e2e4", board);
bool isLegal = ChessRules.ValidateMove(board, move);

// Make move and check game state
if (ChessRules.MakeMove(board, move)) {
    var result = ChessRules.EvaluatePosition(board);
    bool inCheck = ChessRules.IsInCheck(board, board.sideToMove);
}

// Check for promotion
if (ChessRules.RequiresPromotion(board, move)) {
    move.promotionPiece = 'Q';
    bool validPromotion = ChessRules.ValidatePromotionMove(board, move);
}

// Get evaluation info for UI
var evalInfo = ChessRules.GetEvaluationInfo(board, 150f, 0.65f, 0f);
string displayText = evalInfo.GetDisplayText();
```

## Control flow / responsibilities & high-level algorithm summary

Static class validates chess moves through legal move generation, applies moves with state updates, detects game termination via material/position analysis, comprehensive position validation.

## Performance, allocations, and hotspots / Threading / async considerations

Heavy operations: legal move generation O(moves), position validation O(64), attack detection. Main-thread only, no async.

## Security / safety / correctness concerns

Comprehensive validation prevents illegal moves, null checks for board access, exception handling in complex rule validation.

## Tests, debugging & observability

Extensive test suite with `RunAllTests()` covering all public methods, debug logging for rule validation, position analysis verification.

## Cross-file references

Depends on `SPACE_UTIL.v2`, `ChessBoard.cs`, `MoveGenerator.cs`, `ChessMove.cs` for complete rule validation and move application.

<!-- ## TODO / Known limitations / Suggested improvements

* Enhanced threefold repetition with complete position tracking
* Performance optimization for attack detection
* Chess960 castling validation improvements
* Extended insufficient material detection
* Multi-threading support for complex validation -->

## Appendix

Key private helpers: `UpdateCastlingRights()`, `UpdateEnPassantSquare()`, `HasInsufficientMaterial()`, `IsPathClear()`, `DoesPieceAttackSquare()`. Comprehensive piece attack pattern validation.

## General Note: important behaviors

Major functionalities: Complete chess rule enforcement, promotion validation with color/piece checks, comprehensive game state evaluation, position validation for any FEN input, extensive testing framework.

`checksum: B8C5D6F1` / performance:** O(legal moves generation) + O(position validation)
  - **Signature:** `public static bool IsInCheck(ChessBoard board, char side)`
    - **Description:** Check if the given side's king is under attack
    - **Parameters:** board : ChessBoard — current position, side : char — side to check ('w'/'b')
    - **Returns:** bool — whether king is in check, `bool inCheck = ChessRules.IsInCheck(board, 'w')`
    - **Complexity / performance:** O(64) square scan for attacks
  - **Signature:** `public static bool ValidateMove(ChessBoard board, ChessMove move)`
    - **Description:** Comprehensive move validation according to chess rules
    - **Parameters:** board : ChessBoard — current position, move : ChessMove — move to validate
    - **Returns:** bool — move legality, `bool isLegal = ChessRules.ValidateMove(board, move)`
    - **Side effects / state changes:** None, read-only validation
    - **Complexity / performance:** O(legal moves generation) for full validation
  - **Signature:** `public static bool MakeMove(ChessBoard board, ChessMove move)`
    - **Description:** Apply move to board with full rule enforcement
    - **Parameters:** board : ChessBoard — board to modify, move : ChessMove — move to apply
    - **Returns:** bool — application success, `bool success = ChessRules.MakeMove(board, move)`
    - **Side effects / state changes:** Modifies board state, updates castling rights, en passant, move counters
    - **Throws:** Invalid moves are rejected, no exceptions thrown
  - **Signature:** `public static bool ValidatePromotionMove(ChessBoard board, ChessMove move)`
    - **Description:** Enhanced validation for promotion move requirements
    - **Parameters:** board : ChessBoard — current position, move : ChessMove — promotion move
    - **Returns:** bool — promotion validity, `bool valid = ChessRules.ValidatePromotionMove(board, move)`
    - **Notes:** Validates pawn on correct rank, valid promotion piece, color matching
  - **Signature:** `public static List<v2> GetAttackingPieces(ChessBoard board, v2 square, char attackingSide)`
    - **Description:** Find all pieces of given side attacking target square
    - **Parameters:** board : ChessBoard — current position, square : v2 — target square, attackingSide : char — attacking side
    - **Returns:** List<v2> — positions of attacking pieces, `var attackers = ChessRules.GetAttackingPieces(board, pos, 'w')`
    - **Complexity