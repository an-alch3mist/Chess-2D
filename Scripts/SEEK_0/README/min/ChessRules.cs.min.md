# Source: `ChessRules.cs` — Chess rules validation and game state evaluation engine

## Short description
Implements comprehensive chess rules validation, move legality checking, game state evaluation, and position analysis for Unity 2020.3 chess engines. Provides static methods for validating moves, detecting checkmate/stalemate, managing promotion rules, and tracking game-ending conditions like threefold repetition and insufficient material.

## Metadata
* **Filename:** `ChessRules.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependencies:** `System`, `System.Collections.Generic`, `System.Linq`, `UnityEngine`, `SPACE_UTIL`
* **Public types:** `ChessRules (static class), ChessRules.GameResult (enum), ChessRules.EvaluationInfo (struct)`
* **Unity version:** 2020.3 (mentioned in comments)

## Public API Summary
| Type | Member | Signature | Short Purpose | OneLiner Call |
|------|---------|-----------|---------------|---------------|
| ChessRules.GameResult | Method | public static GameResult EvaluatePosition(ChessBoard board, List<string> moveHistory = null) | Evaluates current game state | var result = ChessRules.EvaluatePosition(board); |
| ChessRules.EvaluationInfo | Method | public static EvaluationInfo GetEvaluationInfo(ChessBoard board, float centipawns = 0f, float winProbability = 0.5f, float mateDistance = 0f) | Gets evaluation info for UI | var info = ChessRules.GetEvaluationInfo(board, 50f); |
| bool | Method | public static bool IsInCheck(ChessBoard board, char side) | Checks if side is in check | var inCheck = ChessRules.IsInCheck(board, 'w'); |
| bool | Method | public static bool RequiresPromotion(ChessBoard board, ChessMove move) | Checks if move requires promotion | var needsPromotion = ChessRules.RequiresPromotion(board, move); |
| bool | Method | public static bool ValidateMove(ChessBoard board, ChessMove move) | Validates move legality | var isValid = ChessRules.ValidateMove(board, move); |
| bool | Method | public static bool ValidatePromotionMove(ChessBoard board, ChessMove move) | Validates promotion move requirements | var validPromo = ChessRules.ValidatePromotionMove(board, move); |
| bool | Method | public static bool MakeMove(ChessBoard board, ChessMove move) | Applies move to board | var success = ChessRules.MakeMove(board, move); |
| bool | Method | public static bool DoesMoveCauseCheck(ChessBoard board, ChessMove move) | Tests if move puts opponent in check | var causesCheck = ChessRules.DoesMoveCauseCheck(board, move); |
| bool | Method | public static bool IsCheckingMove(ChessBoard board, ChessMove move) | Checks if move is a checking move | var isCheck = ChessRules.IsCheckingMove(board, move); |
| List<v2> | Method | public static List<v2> GetAttackingPieces(ChessBoard board, v2 square, char attackingSide) | Gets pieces attacking square | var attackers = ChessRules.GetAttackingPieces(board, square, 'w'); |
| v2 | Method | public static v2 FindKing(ChessBoard board, char king) | Finds king position on board | var kingPos = ChessRules.FindKing(board, 'K'); |
| bool | Method | public static bool ValidatePosition(ChessBoard board) | Validates FEN position legality | var validPos = ChessRules.ValidatePosition(board); |
| void | Method | public static void ClearPositionHistory() | Clears threefold repetition history | ChessRules.ClearPositionHistory(); |
| void | Method | public static void RunAllTests() | Runs comprehensive rule tests | ChessRules.RunAllTests(); |

## Important Types

### `ChessRules.GameResult`
* **Kind:** enum
* **Responsibility:** Represents possible game termination states
* **Values:**
  * `InProgress` — Game continues
  * `WhiteWins` — White wins by checkmate
  * `BlackWins` — Black wins by checkmate
  * `Draw` — Generic draw
  * `Stalemate` — Draw by stalemate
  * `InsufficientMaterial` — Draw by insufficient material
  * `FiftyMoveRule` — Draw by fifty-move rule
  * `ThreefoldRepetition` — Draw by threefold repetition

### `ChessRules.EvaluationInfo`
* **Kind:** struct with private setters
* **Responsibility:** Encapsulates position evaluation data for UI display
* **Constructor:** `public EvaluationInfo(float centipawns, float winProbability, float mateDistance, bool isCheckmate, bool isStalemate, char sideToMove)`
* **Public Properties:**
  * `centipawns` — `float` — Position evaluation in centipawns (`get`)
  * `winProbability` — `float` — Win probability 0.0-1.0 (`get`)
  * `mateDistance` — `float` — Moves to mate if applicable (`get`)
  * `isCheckmate` — `bool` — True if position is checkmate (`get`)
  * `isStalemate` — `bool` — True if position is stalemate (`get`)
  * `sideToMove` — `char` — Side to move ('w'/'b') (`get`)
* **Public Methods:**
  * **`public string GetDisplayText()`**
    * Description: Returns formatted display text for UI
    * Returns: `string` — Formatted evaluation text
    * Example: `var displayText = evalInfo.GetDisplayText();`
  * **`public override string ToString()`**
    * Description: Returns detailed debug string representation
    * Returns: `string` — Debug format with all fields
    * Example: `var debugStr = evalInfo.ToString();`

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
        var board = new ChessBoard();
        var move = ChessMove.FromUCI("e2e4", board);
        
        var gameResult = ChessRules.EvaluatePosition(board);
        var evalInfo = ChessRules.GetEvaluationInfo(board, 25f, 0.55f);
        var inCheck = ChessRules.IsInCheck(board, 'w');
        var needsPromotion = ChessRules.RequiresPromotion(board, move);
        var validMove = ChessRules.ValidateMove(board, move);
        var moveSuccess = ChessRules.MakeMove(board, move);
        var kingPos = ChessRules.FindKing(board, 'K');
        var validPosition = ChessRules.ValidatePosition(board);
        var displayText = evalInfo.GetDisplayText();
        
        Debug.Log($"API Results: {gameResult}, {evalInfo}, {inCheck}, {needsPromotion}, {validMove}, {moveSuccess}, {kingPos}, {validPosition}, {displayText}");
    }
}
```

## Control Flow & Responsibilities
Static rule engine: validates moves, detects game endings, manages position history for repetitions.

## Performance & Threading  
Main-thread only, position cloning for lookahead, memory-limited history tracking.

## Cross-file Dependencies
References ChessBoard.cs, ChessMove.cs, MoveGenerator.cs for board state and move generation.

## Major Functionality
Promotion validation, threefold repetition tracking, insufficient material detection, comprehensive position validation.

`checksum: a7f9c2d1 v0.3.min`