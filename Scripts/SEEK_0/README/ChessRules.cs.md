# Source: `ChessRules.cs` — Chess rules validation and game state evaluation engine

Comprehensive chess rules validation system with enhanced promotion support, position validation, and game termination detection for Unity chess applications.

## Short description

This file implements complete chess rules validation including move legality checking, game state evaluation (checkmate, stalemate, draws), position validation for any FEN input, and enhanced promotion handling. It serves as the core rules engine for chess applications, providing both validation and game state analysis capabilities with comprehensive testing support.

## Metadata

* **Filename:** `ChessRules.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `using SPACE_UTIL;` (v2 struct), `using System;`, `using System.Collections.Generic;`, `using System.Linq;`, `using UnityEngine;`
* **Estimated lines:** 800+
* **Estimated chars:** 35,000+
* **Public types:** `ChessRules (static class), ChessRules.GameResult (enum), ChessRules.EvaluationInfo (struct)`
* **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
* **Dependencies:** `ChessBoard.cs`, `MoveGenerator.cs`, `ChessMove.cs`, `SPACE_UTIL.v2` (SPACE_UTIL is external namespace)

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| ChessRules.GameResult (enum) | InProgress | enum value | Game is ongoing | var result = ChessRules.GameResult.InProgress; |
| ChessRules.GameResult (enum) | WhiteWins | enum value | White wins by checkmate | var result = ChessRules.GameResult.WhiteWins; |
| ChessRules.GameResult (enum) | BlackWins | enum value | Black wins by checkmate | var result = ChessRules.GameResult.BlackWins; |
| ChessRules.GameResult (enum) | Draw | enum value | General draw condition | var result = ChessRules.GameResult.Draw; |
| ChessRules.GameResult (enum) | Stalemate | enum value | Stalemate draw | var result = ChessRules.GameResult.Stalemate; |
| ChessRules.GameResult (enum) | InsufficientMaterial | enum value | Insufficient material draw | var result = ChessRules.GameResult.InsufficientMaterial; |
| ChessRules.GameResult (enum) | FiftyMoveRule | enum value | Fifty-move rule draw | var result = ChessRules.GameResult.FiftyMoveRule; |
| ChessRules.GameResult (enum) | ThreefoldRepetition | enum value | Threefold repetition draw | var result = ChessRules.GameResult.ThreefoldRepetition; |
| ChessRules.EvaluationInfo (struct) | centipawns | public float centipawns; | Position evaluation in centipawns | var eval = info.centipawns; |
| ChessRules.EvaluationInfo (struct) | winProbability | public float winProbability; | Win probability (0.0-1.0) | var prob = info.winProbability; |
| ChessRules.EvaluationInfo (struct) | mateDistance | public float mateDistance; | Moves to mate (if applicable) | var mate = info.mateDistance; |
| ChessRules.EvaluationInfo (struct) | isCheckmate | public bool isCheckmate; | Position is checkmate | var checkmate = info.isCheckmate; |
| ChessRules.EvaluationInfo (struct) | isStalemate | public bool isStalemate; | Position is stalemate | var stalemate = info.isStalemate; |
| ChessRules.EvaluationInfo (struct) | sideToMove | public char sideToMove; | Side to move ('w'/'b') | var side = info.sideToMove; |
| string (builtin-type) | GetDisplayText | public string GetDisplayText() | Get formatted evaluation display | string text = info.GetDisplayText(); |
| ChessRules.GameResult (enum) | EvaluatePosition | public static ChessRules.GameResult EvaluatePosition(ChessBoard board, List<string> moveHistory = null) | Evaluate current game state | var result = ChessRules.EvaluatePosition(board, history); |
| ChessRules.EvaluationInfo (struct) | GetEvaluationInfo | public static ChessRules.EvaluationInfo GetEvaluationInfo(ChessBoard board, float centipawns = 0f, float winProbability = 0.5f, float mateDistance = 0f) | Get evaluation info for UI | var info = ChessRules.GetEvaluationInfo(board, 50f, 0.6f); |
| bool (builtin-type) | IsInCheck | public static bool IsInCheck(ChessBoard board, char side) | Check if side is in check | bool inCheck = ChessRules.IsInCheck(board, 'w'); |
| bool (builtin-type) | RequiresPromotion | public static bool RequiresPromotion(ChessBoard board, ChessMove move) | Check if move requires promotion | bool needsPromotion = ChessRules.RequiresPromotion(board, move); |
| bool (builtin-type) | ValidateMove | public static bool ValidateMove(ChessBoard board, ChessMove move) | Validate move legality | bool isValid = ChessRules.ValidateMove(board, move); |
| bool (builtin-type) | ValidatePromotionMove | public static bool ValidatePromotionMove(ChessBoard board, ChessMove move) | Validate promotion move requirements | bool validPromotion = ChessRules.ValidatePromotionMove(board, move); |
| bool (builtin-type) | MakeMove | public static bool MakeMove(ChessBoard board, ChessMove move) | Apply move to board | bool success = ChessRules.MakeMove(board, move); |
| bool (builtin-type) | DoesMoveCauseCheck | public static bool DoesMoveCauseCheck(ChessBoard board, ChessMove move) | Check if move puts opponent in check | bool causesCheck = ChessRules.DoesMoveCauseCheck(board, move); |
| bool (builtin-type) | IsCheckingMove | public static bool IsCheckingMove(ChessBoard board, ChessMove move) | Check if move is a checking move | bool isChecking = ChessRules.IsCheckingMove(board, move); |
| List<v2> (class) | GetAttackingPieces | public static List<v2> GetAttackingPieces(ChessBoard board, v2 square, char attackingSide) | Get pieces attacking a square | var attackers = ChessRules.GetAttackingPieces(board, pos, 'w'); |
| v2 (struct) | FindKing | public static v2 FindKing(ChessBoard board, char king) | Find king position on board | v2 kingPos = ChessRules.FindKing(board, 'K'); |
| bool (builtin-type) | ValidatePosition | public static bool ValidatePosition(ChessBoard board) | Comprehensive FEN position validation | bool isValid = ChessRules.ValidatePosition(board); |
| void (builtin-type) | RunAllTests | public static void RunAllTests() | Run comprehensive rule tests | ChessRules.RunAllTests(); |

## Important types — details

### `ChessRules` (static class)
* **Kind:** static class - `GPTDeepResearch.ChessRules`
* **Responsibility:** Provides comprehensive chess rules validation, game state evaluation, and position analysis functionality.
* **Constructor(s):** N/A (static class)
* **Public properties / fields:** None
* **Public methods:**
  * **Signature:** `public static ChessRules.GameResult EvaluatePosition(ChessBoard board, List<string> moveHistory = null)`
    * **Description:** Evaluates current game state for termination conditions.
    * **Parameters:** 
      * board : ChessBoard — current board position
      * moveHistory : List<string> — move history for repetition detection (optional)
    * **Returns:** ChessRules.GameResult — `var result = ChessRules.EvaluatePosition(board, history);`
    * **Throws:** None
    * **Side effects / state changes:** None (pure evaluation)
    * **Complexity / performance:** O(n) where n is number of legal moves
    * **Notes:** Checks checkmate, stalemate, insufficient material, fifty-move rule, threefold repetition

  * **Signature:** `public static ChessRules.EvaluationInfo GetEvaluationInfo(ChessBoard board, float centipawns = 0f, float winProbability = 0.5f, float mateDistance = 0f)`
    * **Description:** Creates evaluation info structure for UI display.
    * **Parameters:**
      * board : ChessBoard — current position
      * centipawns : float — evaluation in centipawns (default 0)
      * winProbability : float — win probability 0-1 (default 0.5)  
      * mateDistance : float — moves to mate (default 0)
    * **Returns:** ChessRules.EvaluationInfo — `var info = ChessRules.GetEvaluationInfo(board, 50f);`
    * **Throws:** None
    * **Side effects / state changes:** None
    * **Complexity / performance:** O(1)
    * **Notes:** Combines evaluation data with game state analysis

  * **Signature:** `public static bool IsInCheck(ChessBoard board, char side)`
    * **Description:** Determines if specified side is in check.
    * **Parameters:**
      * board : ChessBoard — current position
      * side : char — side to check ('w' or 'b')
    * **Returns:** bool — `bool inCheck = ChessRules.IsInCheck(board, 'w');`
    * **Throws:** None
    * **Side effects / state changes:** None
    * **Complexity / performance:** O(n) where n is number of pieces
    * **Notes:** Uses MoveGenerator.IsSquareAttacked for efficient detection

  * **Signature:** `public static bool ValidateMove(ChessBoard board, ChessMove move)`
    * **Description:** Validates if a move is legal according to chess rules.
    * **Parameters:**
      * board : ChessBoard — current position
      * move : ChessMove — move to validate
    * **Returns:** bool — `bool isValid = ChessRules.ValidateMove(board, move);`
    * **Throws:** None
    * **Side effects / state changes:** Debug logging
    * **Complexity / performance:** O(n) generates legal moves for validation
    * **Notes:** Comprehensive validation including piece presence, turn, and legality

  * **Signature:** `public static bool MakeMove(ChessBoard board, ChessMove move)`
    * **Description:** Applies a validated move to the board state.
    * **Parameters:**
      * board : ChessBoard — board to modify
      * move : ChessMove — move to apply
    * **Returns:** bool — `bool success = ChessRules.MakeMove(board, move);`
    * **Throws:** None
    * **Side effects / state changes:** Modifies board state, castling rights, en passant, counters
    * **Complexity / performance:** O(1) for most moves
    * **Notes:** Handles castling, en passant, promotion; updates game state

### `ChessRules.GameResult` (enum)
* **Kind:** enum - `GPTDeepResearch.ChessRules.GameResult`
* **Responsibility:** Represents possible game termination states and ongoing status.
* **Constructor(s):** N/A (enum)
* **Public properties / fields:**
  * InProgress — ChessRules.GameResult — game is still ongoing
  * WhiteWins — ChessRules.GameResult — white wins by checkmate
  * BlackWins — ChessRules.GameResult — black wins by checkmate  
  * Draw — ChessRules.GameResult — general draw condition
  * Stalemate — ChessRules.GameResult — stalemate draw
  * InsufficientMaterial — ChessRules.GameResult — insufficient material draw
  * FiftyMoveRule — ChessRules.GameResult — fifty-move rule draw
  * ThreefoldRepetition — ChessRules.GameResult — threefold repetition draw

### `ChessRules.EvaluationInfo` (struct)
* **Kind:** struct - `GPTDeepResearch.ChessRules.EvaluationInfo`
* **Responsibility:** Contains position evaluation data for UI display and analysis.
* **Constructor(s):** Default struct constructor
* **Public properties / fields:**
  * centipawns — float — position evaluation in centipawns
  * winProbability — float — win probability from 0.0 to 1.0
  * mateDistance — float — moves to mate if applicable
  * isCheckmate — bool — position is checkmate
  * isStalemate — bool — position is stalemate
  * sideToMove — char — side to move ('w' or 'b')
* **Public methods:**
  * **Signature:** `public string GetDisplayText()`
    * **Description:** Formats evaluation data for display.
    * **Parameters:** None
    * **Returns:** string — `string display = info.GetDisplayText();`
    * **Throws:** None
    * **Side effects / state changes:** None
    * **Complexity / performance:** O(1)
    * **Notes:** Returns formatted strings like "Mate in 3" or "+1.50"

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
    private void ChessRules_Check()
    {
        // Create test board
        ChessBoard board = new ChessBoard();
        
        // Test position evaluation
        ChessRules.GameResult result = ChessRules.EvaluatePosition(board);
        Debug.Log($"<color=green>Game state: {result}</color>");
        // Expected output: "Game state: InProgress"
        
        // Test check detection
        bool whiteInCheck = ChessRules.IsInCheck(board, 'w');
        bool blackInCheck = ChessRules.IsInCheck(board, 'b');
        Debug.Log($"<color=green>White in check: {whiteInCheck}, Black in check: {blackInCheck}</color>");
        // Expected output: "White in check: False, Black in check: False"
        
        // Test move validation
        ChessMove pawnMove = ChessMove.FromUCI("e2e4", board);
        bool isValid = ChessRules.ValidateMove(board, pawnMove);
        Debug.Log($"<color=green>Move e2e4 valid: {isValid}</color>");
        // Expected output: "Move e2e4 valid: True"
        
        // Test promotion requirement
        ChessBoard promotionBoard = new ChessBoard("8/P7/8/8/8/8/8/K6k w - - 0 1");
        ChessMove promotionMove = ChessMove.FromUCI("a7a8q", promotionBoard);
        bool needsPromotion = ChessRules.RequiresPromotion(promotionBoard, promotionMove);
        Debug.Log($"<color=green>Needs promotion: {needsPromotion}</color>");
        // Expected output: "Needs promotion: True"
        
        // Test move application
        ChessBoard testBoard = board.Clone();
        bool moveApplied = ChessRules.MakeMove(testBoard, pawnMove);
        Debug.Log($"<color=green>Move applied: {moveApplied}</color>");
        // Expected output: "Move applied: True"
        
        // Test evaluation info
        ChessRules.EvaluationInfo evalInfo = ChessRules.GetEvaluationInfo(board, 25.5f, 0.6f, 0f);
        string displayText = evalInfo.GetDisplayText();
        Debug.Log($"<color=green>Evaluation: {displayText}</color>");
        // Expected output: "Evaluation: +25.50"
        
        // Test position validation
        bool positionValid = ChessRules.ValidatePosition(board);
        Debug.Log($"<color=green>Position valid: {positionValid}</color>");
        // Expected output: "Position valid: True"
        
        // Test king finding
        v2 whiteKing = ChessRules.FindKing(board, 'K');
        v2 blackKing = ChessRules.FindKing(board, 'k');
        Debug.Log($"<color=green>White king at: {whiteKing}, Black king at: {blackKing}</color>");
        // Expected output: "White king at: (4, 0), Black king at: (4, 7)"
        
        // Test attacking pieces
        List<v2> attackers = ChessRules.GetAttackingPieces(board, new v2(4, 4), 'w');
        Debug.Log($"<color=green>White pieces attacking e4: {attackers.Count}</color>");
        // Expected output: "White pieces attacking e4: 0"
        
        // Test stalemate detection
        ChessBoard stalemateBoard = new ChessBoard("7k/5Q2/6K1/8/8/8/8/8 b - - 0 1");
        ChessRules.GameResult stalemateResult = ChessRules.EvaluatePosition(stalemateBoard);
        Debug.Log($"<color=green>Stalemate result: {stalemateResult}</color>");
        // Expected output: "Stalemate result: Stalemate"
        
        // Test insufficient material
        ChessBoard insufficientBoard = new ChessBoard("8/8/8/8/8/8/8/K6k w - - 0 1");
        ChessRules.GameResult insufficientResult = ChessRules.EvaluatePosition(insufficientBoard);
        Debug.Log($"<color=green>Insufficient material result: {insufficientResult}</color>");
        // Expected output: "Insufficient material result: InsufficientMaterial"
        
        // Run comprehensive tests
        ChessRules.RunAllTests();
        // Expected output: Multiple test results with colored pass/fail indicators
    }
}
```

## Control flow / responsibilities & high-level algorithm summary

Static rule engine processes moves through validation pipeline: coordinate validation → piece existence → turn verification → legal move generation → rule compliance checking. Game state evaluation generates legal moves then applies termination detection algorithms.

## Performance, allocations, and hotspots / Threading considerations

Heavy operations: legal move generation (O(n)), attack detection (O(n²)). Main-thread only, no async.

## Security / safety / correctness concerns

Potential NullReferenceException if board/move parameters null; relies on MoveGenerator accuracy for validation.

## Tests, debugging & observability

Comprehensive built-in test suite via RunAllTests(); extensive Debug.Log with color coding for pass/fail validation results.

## Cross-file references

Depends on `ChessBoard.cs` (board state), `MoveGenerator.cs` (legal moves), `ChessMove.cs` (move representation), `SPACE_UTIL.v2` (coordinates).

## TODO / Known limitations / Suggested improvements

<!-- TODO items from code:
- Threefold repetition detection simplified (needs full position history tracking)
- Chess960 castling validation could be enhanced
- Performance optimization for repeated position evaluations
- Add draw by agreement and resignation support
(only if I explicitly mentioned in the prompt) -->

## Appendix

Key private helpers: UpdateCastlingRights, UpdateEnPassantSquare, HasInsufficientMaterial, IsPathClear. Core validation flow: ValidateMove → generate legal moves → check membership.

## General Note: important behaviors

Major functionalities: comprehensive promotion validation with piece type checking, enhanced game state evaluation including all draw conditions, position validation for any valid chess FEN, complete move application with state updates.

`checksum: a7f3d2e1 v0.3`