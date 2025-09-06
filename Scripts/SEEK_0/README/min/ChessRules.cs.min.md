# Source: `ChessRules.cs` — Chess rules validation and game state evaluation

## Static class providing comprehensive chess rules validation, game termination detection, and move application with enhanced promotion support.

Handles game state evaluation including checkmate, stalemate, draws, and position validation. Supports move application with special handling for castling, en passant, and promotions. Includes extensive testing framework for rule validation.

## Metadata
* **Filename:** `ChessRules.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependencies:** `System`, `System.Collections.Generic`, `System.Linq`, `UnityEngine`, `SPACE_UTIL`
* **Public types:** `ChessRules (static class), ChessRules.GameResult (enum), ChessRules.EvaluationInfo (struct)`
* **Unity version:** Unity 2020.3+ (UnityEngine.Debug usage)

## Public API Summary
| Type | Member | Signature | Short Purpose | OneLiner Call |
|------|---------|-----------|---------------|---------------|
| ChessRules.GameResult (enum) | Method | public static ChessRules.GameResult EvaluatePosition(ChessBoard board, List<string> moveHistory = null) | Evaluate current game state for termination | var result = ChessRules.EvaluatePosition(board); |
| ChessRules.EvaluationInfo (struct) | Method | public static ChessRules.EvaluationInfo GetEvaluationInfo(ChessBoard board, float centipawns = 0f, float winProbability = 0.5f, float mateDistance = 0f) | Get evaluation info for UI display | var info = ChessRules.GetEvaluationInfo(board, 50.0f); |
| bool | Method | public static bool IsInCheck(ChessBoard board, char side) | Check if given side's king is in check | bool inCheck = ChessRules.IsInCheck(board, 'w'); |
| bool | Method | public static bool RequiresPromotion(ChessBoard board, ChessMove move) | Check if move requires pawn promotion | bool needsPromo = ChessRules.RequiresPromotion(board, move); |
| bool | Method | public static bool ValidateMove(ChessBoard board, ChessMove move) | Validate if move is legal according to chess rules | bool valid = ChessRules.ValidateMove(board, move); |
| bool | Method | public static bool ValidatePromotionMove(ChessBoard board, ChessMove move) | Validate promotion move requirements | bool validPromo = ChessRules.ValidatePromotionMove(board, move); |
| bool | Method | public static bool MakeMove(ChessBoard board, ChessMove move) | Apply move to board and update game state | bool applied = ChessRules.MakeMove(board, move); |
| bool | Method | public static bool DoesMoveCauseCheck(ChessBoard board, ChessMove move) | Check if move puts opponent in check | bool causesCheck = ChessRules.DoesMoveCauseCheck(board, move); |
| bool | Method | public static bool IsCheckingMove(ChessBoard board, ChessMove move) | Check if move is a checking move | bool isChecking = ChessRules.IsCheckingMove(board, move); |
| List<v2> | Method | public static List<v2> GetAttackingPieces(ChessBoard board, v2 square, char attackingSide) | Get all pieces attacking a square | var attackers = ChessRules.GetAttackingPieces(board, pos, 'b'); |
| v2 | Method | public static v2 FindKing(ChessBoard board, char king) | Find king position for given side | v2 kingPos = ChessRules.FindKing(board, 'K'); |
| bool | Method | public static bool ValidatePosition(ChessBoard board) | Comprehensive FEN validation for any chess position | bool valid = ChessRules.ValidatePosition(board); |
| void | Method | public static void RunAllTests() | Execute comprehensive rule validation tests | ChessRules.RunAllTests(); |

## Important Types

### `ChessRules`
* **Kind:** static class
* **Responsibility:** Provides chess rules validation and game state management functionality
* **Constructor(s):** None (static class)
* **Public Properties:** None
* **Public Methods:**
  * **`public static ChessRules.GameResult EvaluatePosition(ChessBoard board, List<string> moveHistory = null)`**
    * Description: Evaluates current position for game termination conditions
    * Parameters: `board : ChessBoard — position to evaluate`, `moveHistory : List<string> — optional move history for repetition detection`
    * Returns: `ChessRules.GameResult — game state enum value` + call example: `var gameState = ChessRules.EvaluatePosition(currentBoard);`
    * Notes: Detects checkmate, stalemate, draws, insufficient material

  * **`public static bool MakeMove(ChessBoard board, ChessMove move)`**
    * Description: Applies move to board with full rule compliance and state updates
    * Parameters: `board : ChessBoard — board to modify`, `move : ChessMove — move to apply`
    * Returns: `bool — true if move was successfully applied` + call example: `bool success = ChessRules.MakeMove(gameBoard, playerMove);`
    * Notes: Updates castling rights, en passant, move counters, handles special moves

  * **`public static bool ValidateMove(ChessBoard board, ChessMove move)`**
    * Description: Validates move legality according to all chess rules
    * Parameters: `board : ChessBoard — current position`, `move : ChessMove — move to validate`
    * Returns: `bool — true if move is legal` + call example: `bool canPlay = ChessRules.ValidateMove(board, inputMove);`
    * Notes: Comprehensive validation including piece ownership, legal moves, check avoidance

### `ChessRules.GameResult`
* **Kind:** enum
* **Responsibility:** Represents possible game termination states
* **Values:** `InProgress, WhiteWins, BlackWins, Draw, Stalemate, InsufficientMaterial, FiftyMoveRule, ThreefoldRepetition`

### `ChessRules.EvaluationInfo`
* **Kind:** struct
* **Responsibility:** Encapsulates position evaluation data for UI display
* **Constructor(s):** Default struct constructor
* **Public Properties:**
  * `centipawns` — `float` — position evaluation in centipawns (`get/set`)
  * `winProbability` — `float` — probability of winning (0-1) (`get/set`)
  * `mateDistance` — `float` — moves to mate if applicable (`get/set`)
  * `isCheckmate` — `bool` — true if position is checkmate (`get/set`)
  * `isStalemate` — `bool` — true if position is stalemate (`get/set`)
  * `sideToMove` — `char` — current side to move ('w' or 'b') (`get/set`)
* **Public Methods:**
  * **`public string GetDisplayText()`**
    * Description: Formats evaluation info for UI display
    * Parameters: None
    * Returns: `string — formatted evaluation text` + call example: `string display = info.GetDisplayText();`
    * Notes: Handles mate announcements, stalemate, and centipawn display

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
    private void ChessRules_Check()
    {
        // Test all major public APIs in minimal lines
        var board = new ChessBoard();
        var gameResult = ChessRules.EvaluatePosition(board);
        var evalInfo = ChessRules.GetEvaluationInfo(board, 25.0f, 0.55f);
        var testMove = ChessMove.FromUCI("e2e4", board);
        bool inCheck = ChessRules.IsInCheck(board, 'w');
        bool needsPromo = ChessRules.RequiresPromotion(board, testMove);
        bool validMove = ChessRules.ValidateMove(board, testMove);
        bool moveApplied = ChessRules.MakeMove(board, testMove);
        var kingPos = ChessRules.FindKing(board, 'K');
        bool validPosition = ChessRules.ValidatePosition(board);
        ChessRules.RunAllTests();
        
        Debug.Log($"API Results: {gameResult}, eval: {evalInfo.GetDisplayText()}, check: {inCheck}, promotion: {needsPromo}, valid: {validMove}, applied: {moveApplied}, king: {kingPos}, position valid: {validPosition}, tests completed");
    }
}
```

## Control Flow & Responsibilities
Evaluates game states, validates moves, applies rules, updates board state with comprehensive error checking.

## Performance & Threading
Board cloning for move validation, position scanning for checks, extensive rule verification processing.

## Cross-file Dependencies
ChessBoard.cs ChessMove.cs MoveGenerator.cs for position, moves, and legal move generation.

## Major Functionality
Game termination detection, move validation, position validation, castling rights management, promotion handling, check detection.

`checksum: B8D4E1F7 v0.3.min`