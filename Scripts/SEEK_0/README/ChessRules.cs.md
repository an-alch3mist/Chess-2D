# Source: `ChessRules.cs` — Chess rules validation and game state evaluation for Unity chess engines

Implements comprehensive chess rules validation, move verification, game state evaluation, and position analysis with support for all special moves and termination conditions.

## Short description (2–4 sentences)

The file provides a complete chess rules engine as a static class handling move validation, game state evaluation, promotion validation, check detection, and game termination analysis. It supports all chess rules including castling, en passant, promotion, insufficient material, threefold repetition, and the fifty-move rule. The implementation is designed for Unity 2020.3 chess engines with comprehensive logging and debugging support.

## Metadata

* **Filename:** `ChessRules.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `SPACE_UTIL` (provides v2 vector type), `System`, `System.Collections.Generic`, `System.Linq`, `UnityEngine`
* **Estimated lines:** 1650
* **Estimated chars:** 65000
* **Public types:** `ChessRules (static class), ChessRules.GameResult (enum), ChessRules.EvaluationInfo (struct)`
* **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
* **Dependencies:** `ChessBoard.cs`, `MoveGenerator.cs`, `ChessMove.cs`, `SPACE_UTIL.v2` (SPACE_UTIL is namespace)

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|------------------|------------------|
| ChessRules.GameResult (enum) | InProgress | `InProgress` | Game continues | `var result = ChessRules.GameResult.InProgress;` |
| ChessRules.GameResult (enum) | WhiteWins | `WhiteWins` | White wins by checkmate | `var result = ChessRules.GameResult.WhiteWins;` |
| ChessRules.GameResult (enum) | BlackWins | `BlackWins` | Black wins by checkmate | `var result = ChessRules.GameResult.BlackWins;` |
| ChessRules.GameResult (enum) | Draw | `Draw` | Generic draw result | `var result = ChessRules.GameResult.Draw;` |
| ChessRules.GameResult (enum) | Stalemate | `Stalemate` | Draw by stalemate | `var result = ChessRules.GameResult.Stalemate;` |
| ChessRules.GameResult (enum) | InsufficientMaterial | `InsufficientMaterial` | Draw by insufficient material | `var result = ChessRules.GameResult.InsufficientMaterial;` |
| ChessRules.GameResult (enum) | FiftyMoveRule | `FiftyMoveRule` | Draw by fifty-move rule | `var result = ChessRules.GameResult.FiftyMoveRule;` |
| ChessRules.GameResult (enum) | ThreefoldRepetition | `ThreefoldRepetition` | Draw by threefold repetition | `var result = ChessRules.GameResult.ThreefoldRepetition;` |
| float (basic-data-type) | centipawns | `public float centipawns { get; }` | Position evaluation in centipawns | `float cp = info.centipawns;` |
| float (basic-data-type) | winProbability | `public float winProbability { get; }` | Win probability (0.0-1.0) | `float wp = info.winProbability;` |
| float (basic-data-type) | mateDistance | `public float mateDistance { get; }` | Distance to mate in moves | `float mate = info.mateDistance;` |
| bool (basic-data-type) | isCheckmate | `public bool isCheckmate { get; }` | True if position is checkmate | `bool checkmate = info.isCheckmate;` |
| bool (basic-data-type) | isStalemate | `public bool isStalemate { get; }` | True if position is stalemate | `bool stalemate = info.isStalemate;` |
| char (basic-data-type) | sideToMove | `public char sideToMove { get; }` | Side to move ('w' or 'b') | `char side = info.sideToMove;` |
| ChessRules.GameResult (enum) | EvaluatePosition | `public static ChessRules.GameResult EvaluatePosition(ChessBoard board, List<string> moveHistory = null)` | Evaluate current game state | `var result = ChessRules.EvaluatePosition(board);` |
| ChessRules.EvaluationInfo (struct) | GetEvaluationInfo | `public static ChessRules.EvaluationInfo GetEvaluationInfo(ChessBoard board, float centipawns = 0f, float winProbability = 0.5f, float mateDistance = 0f)` | Get evaluation info for UI | `var info = ChessRules.GetEvaluationInfo(board, 50.0f);` |
| bool (basic-data-type) | IsInCheck | `public static bool IsInCheck(ChessBoard board, char side)` | Check if side is in check | `bool inCheck = ChessRules.IsInCheck(board, 'w');` |
| bool (basic-data-type) | RequiresPromotion | `public static bool RequiresPromotion(ChessBoard board, ChessMove move)` | Check if move requires promotion | `bool needsPromotion = ChessRules.RequiresPromotion(board, move);` |
| bool (basic-data-type) | ValidateMove | `public static bool ValidateMove(ChessBoard board, ChessMove move)` | Validate if move is legal | `bool isLegal = ChessRules.ValidateMove(board, move);` |
| bool (basic-data-type) | ValidatePromotionMove | `public static bool ValidatePromotionMove(ChessBoard board, ChessMove move)` | Validate promotion move requirements | `bool validPromotion = ChessRules.ValidatePromotionMove(board, move);` |
| bool (basic-data-type) | MakeMove | `public static bool MakeMove(ChessBoard board, ChessMove move)` | Apply move to board | `bool success = ChessRules.MakeMove(board, move);` |
| bool (basic-data-type) | DoesMoveCauseCheck | `public static bool DoesMoveCauseCheck(ChessBoard board, ChessMove move)` | Check if move causes check | `bool causesCheck = ChessRules.DoesMoveCauseCheck(board, move);` |
| bool (basic-data-type) | IsCheckingMove | `public static bool IsCheckingMove(ChessBoard board, ChessMove move)` | Check if move is checking | `bool isChecking = ChessRules.IsCheckingMove(board, move);` |
| List<v2> (List<T>) | GetAttackingPieces | `public static List<v2> GetAttackingPieces(ChessBoard board, v2 square, char attackingSide)` | Get pieces attacking a square | `var attackers = ChessRules.GetAttackingPieces(board, square, 'w');` |
| v2 (struct) | FindKing | `public static v2 FindKing(ChessBoard board, char king)` | Find king position | `v2 kingPos = ChessRules.FindKing(board, 'K');` |
| bool (basic-data-type) | ValidatePosition | `public static bool ValidatePosition(ChessBoard board)` | Validate chess position | `bool isValid = ChessRules.ValidatePosition(board);` |
| void (void) | ClearPositionHistory | `public static void ClearPositionHistory()` | Clear position history | `ChessRules.ClearPositionHistory();` |
| void (static void) | RunAllTests | `public static void RunAllTests()` | Run comprehensive tests | `ChessRules.RunAllTests();` |

## Important types — details

### `ChessRules` (static class)
* **Kind:** static class with full path `GPTDeepResearch.ChessRules`
* **Responsibility:** Provides comprehensive chess rules validation, game state evaluation, and position analysis.
* **Constructor(s):** N/A (static class)
* **Public properties / fields:** None
* **Public methods:**
  * **Signature:** `public static ChessRules.GameResult EvaluatePosition(ChessBoard board, List<string> moveHistory = null)`
    * **Description:** Evaluates current game state for termination conditions.
    * **Parameters:** 
      * board : ChessBoard — board position to evaluate
      * moveHistory : List<string> — optional move history for repetition detection
    * **Returns:** ChessRules.GameResult — game termination state, example call: `var result = ChessRules.EvaluatePosition(board);`
    * **Side effects / state changes:** None (read-only evaluation)
    * **Complexity / performance:** O(n) where n is number of pieces for move generation
    * **Notes:** Checks checkmate, stalemate, insufficient material, fifty-move rule, threefold repetition

  * **Signature:** `public static ChessRules.EvaluationInfo GetEvaluationInfo(ChessBoard board, float centipawns = 0f, float winProbability = 0.5f, float mateDistance = 0f)`
    * **Description:** Creates evaluation info structure for UI display.
    * **Parameters:**
      * board : ChessBoard — board position
      * centipawns : float — position evaluation
      * winProbability : float — win probability (0.0-1.0)
      * mateDistance : float — moves to mate
    * **Returns:** ChessRules.EvaluationInfo — evaluation data, example call: `var info = ChessRules.GetEvaluationInfo(board, 50.0f);`
    * **Side effects / state changes:** None
    * **Notes:** Combines position analysis with provided evaluation metrics

  * **Signature:** `public static bool ValidateMove(ChessBoard board, ChessMove move)`
    * **Description:** Validates if a move is legal according to chess rules.
    * **Parameters:**
      * board : ChessBoard — current board state
      * move : ChessMove — move to validate
    * **Returns:** bool — true if move is legal, example call: `bool isLegal = ChessRules.ValidateMove(board, move);`
    * **Side effects / state changes:** None (validation only)
    * **Complexity / performance:** O(n) for legal move generation and lookup
    * **Notes:** Comprehensive validation including special moves and check prevention

  * **Signature:** `public static bool MakeMove(ChessBoard board, ChessMove move)`
    * **Description:** Applies a validated move to the board and updates game state.
    * **Parameters:**
      * board : ChessBoard — board to modify
      * move : ChessMove — move to apply
    * **Returns:** bool — true if move was successfully applied, example call: `bool success = ChessRules.MakeMove(board, move);`
    * **Side effects / state changes:** Modifies board state, updates castling rights, en passant, move counters
    * **Complexity / performance:** O(1) for move application
    * **Notes:** Handles all special moves including castling, en passant, promotion

  * **Signature:** `public static void RunAllTests()`
    * **Description:** Runs comprehensive test suite for all chess rules functionality.
    * **Parameters:** None
    * **Returns:** void, example call: `ChessRules.RunAllTests();`
    * **Side effects / state changes:** Outputs test results to Unity console with colored logging
    * **Notes:** Includes tests for all major functionality, useful for debugging

### `ChessRules.GameResult` (enum)
* **Kind:** enum with full path `GPTDeepResearch.ChessRules.GameResult`
* **Responsibility:** Represents possible game termination states.
* **Constructor(s):** N/A (enum)
* **Public properties / fields:**
  * InProgress — enum — game is still in progress
  * WhiteWins — enum — white wins by checkmate
  * BlackWins — enum — black wins by checkmate
  * Draw — enum — generic draw result
  * Stalemate — enum — draw by stalemate
  * InsufficientMaterial — enum — draw by insufficient material
  * FiftyMoveRule — enum — draw by fifty-move rule
  * ThreefoldRepetition — enum — draw by threefold repetition

### `ChessRules.EvaluationInfo` (struct)
* **Kind:** struct with full path `GPTDeepResearch.ChessRules.EvaluationInfo`
* **Responsibility:** Contains position evaluation data for UI display.
* **Constructor(s):** `public EvaluationInfo(float centipawns, float winProbability, float mateDistance, bool isCheckmate, bool isStalemate, char sideToMove)`
* **Public properties / fields:**
  * centipawns — float — position evaluation in centipawns (get)
  * winProbability — float — win probability 0.0-1.0 (get)
  * mateDistance — float — distance to mate in moves (get)
  * isCheckmate — bool — true if position is checkmate (get)
  * isStalemate — bool — true if position is stalemate (get)
  * sideToMove — char — side to move 'w' or 'b' (get)
* **Public methods:**
  * **Signature:** `public string GetDisplayText()`
    * **Description:** Returns formatted text for UI display.
    * **Returns:** string — formatted evaluation text, example call: `string text = info.GetDisplayText();`
    * **Notes:** Handles checkmate, stalemate, mate-in-N, and centipawn display formats

  * **Signature:** `public override string ToString()`
    * **Description:** Returns detailed string representation for debugging.
    * **Returns:** string — debug format string, example call: `string debug = info.ToString();`

## Example usage

```csharp
// Required namespaces:
// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using GPTDeepResearch;
// using SPACE_UTIL;

public class ChessRulesExampleUsage : MonoBehaviour 
{
    private void ChessRules_Check()
    {
        // Initialize chess board with starting position
        var board = new ChessBoard();
        Debug.Log("<color=green>Board initialized: " + board.ToFEN() + "</color>");
        
        // Validate starting position
        bool isValidPosition = ChessRules.ValidatePosition(board);
        Debug.Log("<color=green>Starting position valid: " + isValidPosition + "</color>");
        // Expected output: "Starting position valid: True"
        
        // Check if white king is in check (should be false at start)
        bool whiteInCheck = ChessRules.IsInCheck(board, 'w');
        Debug.Log("<color=green>White in check: " + whiteInCheck + "</color>");
        // Expected output: "White in check: False"
        
        // Create and validate a legal opening move (e2-e4)
        var pawnMove = ChessMove.FromUCI("e2e4", board);
        bool isLegalMove = ChessRules.ValidateMove(board, pawnMove);
        Debug.Log("<color=green>e2-e4 is legal: " + isLegalMove + "</color>");
        // Expected output: "e2-e4 is legal: True"
        
        // Apply the move
        bool moveApplied = ChessRules.MakeMove(board, pawnMove);
        Debug.Log("<color=green>Move applied: " + moveApplied + "</color>");
        // Expected output: "Move applied: True"
        
        // Evaluate position after move
        var gameResult = ChessRules.EvaluatePosition(board);
        Debug.Log("<color=green>Game result: " + gameResult + "</color>");
        // Expected output: "Game result: InProgress"
        
        // Get evaluation info for UI
        var evalInfo = ChessRules.GetEvaluationInfo(board, 25.0f, 0.55f, 0f);
        Debug.Log("<color=green>Evaluation: " + evalInfo.GetDisplayText() + "</color>");
        // Expected output: "Evaluation: +0.25"
        
        // Test promotion requirement (create position with pawn on 7th rank)
        var promotionBoard = new ChessBoard("8/4P3/8/8/8/8/8/K6k w - - 0 1");
        var promotionMove = new ChessMove(new v2(4, 6), new v2(4, 7), 'P', 'Q', '\0');
        promotionMove.moveType = ChessMove.MoveType.Promotion;
        
        bool needsPromotion = ChessRules.RequiresPromotion(promotionBoard, promotionMove);
        Debug.Log("<color=green>Move requires promotion: " + needsPromotion + "</color>");
        // Expected output: "Move requires promotion: True"
        
        bool validPromotion = ChessRules.ValidatePromotionMove(promotionBoard, promotionMove);
        Debug.Log("<color=green>Valid promotion move: " + validPromotion + "</color>");
        // Expected output: "Valid promotion move: True"
        
        // Test checkmate detection
        var checkmateBoard = new ChessBoard("7k/5Q2/6K1/8/8/8/8/8 b - - 0 1");
        var checkmateResult = ChessRules.EvaluatePosition(checkmateBoard);
        Debug.Log("<color=green>Checkmate position result: " + checkmateResult + "</color>");
        // Expected output: "Checkmate position result: WhiteWins"
        
        // Test insufficient material
        var insufficientBoard = new ChessBoard("8/8/8/8/8/8/8/K6k w - - 0 1");
        var insufficientResult = ChessRules.EvaluatePosition(insufficientBoard);
        Debug.Log("<color=green>Insufficient material result: " + insufficientResult + "</color>");
        // Expected output: "Insufficient material result: InsufficientMaterial"
        
        // Find king positions
        v2 whiteKing = ChessRules.FindKing(board, 'K');
        v2 blackKing = ChessRules.FindKing(board, 'k');
        Debug.Log("<color=green>White king at: " + whiteKing + ", Black king at: " + blackKing + "</color>");
        // Expected output: "White king at: (4, 0), Black king at: (4, 7)"
        
        // Test attacking pieces
        var attackers = ChessRules.GetAttackingPieces(board, new v2(4, 4), 'w');
        Debug.Log("<color=green>White pieces attacking e5: " + attackers.Count + "</color>");
        // Expected output: "White pieces attacking e5: 1"
        
        // Clear position history for new game
        ChessRules.ClearPositionHistory();
        Debug.Log("<color=green>Position history cleared</color>");
        // Expected output: "Position history cleared"
    }
}
```

## Control flow / responsibilities & high-level algorithm summary

Static validation engine using legal move generation for rule compliance, position history tracking for repetition detection, comprehensive game state analysis with material evaluation.

## Side effects and I/O

Modifies board state in MakeMove(), updates position history list, extensive Unity Debug.Log output with color-coded messages, no file/network I/O.

## Performance, allocations, and hotspots

Move generation O(n) complexity, temporary board cloning for validation, string allocations in FEN processing.

## Threading / async considerations

Main-thread only Unity operations, no async/Task usage, no thread safety mechanisms.

## Security / safety / correctness concerns

Null board parameter checks throughout, extensive validation prevents illegal moves, comprehensive test coverage for edge cases.

## Tests, debugging & observability

Built-in comprehensive test suite (RunAllTests()), extensive color-coded Unity logging, detailed error reporting with position information.

## Cross-file references

Depends on `ChessBoard.cs` for board representation, `ChessMove.cs` for move structure, `MoveGenerator.cs` for legal move generation, `SPACE_UTIL.v2` for coordinates.

## TODO / Known limitations / Suggested improvements

<!-- TODO items from code comments:
- Enhanced Chess960 castling validation support
- Performance optimization for attack detection in complex positions
- Memory usage optimization for position history tracking
- Additional draw conditions (dead position detection)
- Integration with external chess engines for validation
- Thread-safe version for multi-threaded analysis
(only if I explicitly mentioned in the prompt) -->

## Appendix

Key private helpers: `HasInsufficientMaterial()`, `HasThreefoldRepetition()`, `UpdateCastlingRights()`, `IsPathClear()`, `DoesPieceAttackSquare()` - core validation logic with O(1)-O(n) complexity patterns.

## General Note: important behaviors

Major functionalities: Comprehensive promotion validation with board state verification, threefold repetition detection with position tracking, complete special move support (castling, en passant), extensive test coverage with 18 test methods.

`checksum: a7b9c8d2 (v0.3)`