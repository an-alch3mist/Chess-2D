# Source: `MoveGenerator.cs` — Chess move generation with full rule compliance

## Static class providing legal chess move generation for all piece types with Chess960 support and comprehensive validation.

Implements pseudo-legal move generation followed by legality filtering to ensure moves don't leave the king in check. Supports standard chess rules including castling, en passant, promotions, and pin detection.

## Metadata
* **Filename:** `MoveGenerator.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependencies:** `System`, `System.Collections.Generic`, `System.Linq`, `UnityEngine`, `SPACE_UTIL`
* **Public types:** `MoveGenerator (static class)`
* **Unity version:** Unity 2020.3+ (UnityEngine.Debug usage)

## Public API Summary
| Type | Member | Signature | Short Purpose | OneLiner Call |
|------|---------|-----------|---------------|---------------|
| List<ChessMove> | Method | public static List<ChessMove> GenerateLegalMoves(ChessBoard board) | Generate all legal moves for current position | var moves = MoveGenerator.GenerateLegalMoves(board); |
| bool | Method | public static bool IsLegalMove(ChessBoard board, ChessMove move) | Check if move doesn't leave king in check | bool legal = MoveGenerator.IsLegalMove(board, move); |
| bool | Method | public static bool IsSquareAttacked(ChessBoard board, v2 square, char attackingSide) | Check if square is under attack by given side | bool attacked = MoveGenerator.IsSquareAttacked(board, pos, 'w'); |
| void | Method | public static void RunAllTests() | Execute comprehensive test suite for all methods | MoveGenerator.RunAllTests(); |

## Important Types

### `MoveGenerator`
* **Kind:** static class
* **Responsibility:** Provides chess move generation and validation functionality with full rule compliance
* **Constructor(s):** None (static class)
* **Public Properties:** None
* **Public Methods:**
  * **`public static List<ChessMove> GenerateLegalMoves(ChessBoard board)`**
    * Description: Generates all legal moves for the current position
    * Parameters: `board : ChessBoard — chess position to analyze`
    * Returns: `List<ChessMove> — all legal moves available` + call example: `var legalMoves = MoveGenerator.GenerateLegalMoves(currentBoard);`
    * Notes: Filters out moves that would leave own king in check

  * **`public static bool IsLegalMove(ChessBoard board, ChessMove move)`**
    * Description: Validates if a move is legal by testing it doesn't expose king
    * Parameters: `board : ChessBoard — current position`, `move : ChessMove — move to validate`
    * Returns: `bool — true if move is legal` + call example: `bool canPlay = MoveGenerator.IsLegalMove(board, proposedMove);`
    * Notes: Creates temporary board copy to test move safety

  * **`public static bool IsSquareAttacked(ChessBoard board, v2 square, char attackingSide)`**
    * Description: Determines if a square is under attack by specified side
    * Parameters: `board : ChessBoard — position to analyze`, `square : v2 — target square coordinates`, `attackingSide : char — 'w' or 'b' for attacking side`
    * Returns: `bool — true if square is attacked` + call example: `bool inDanger = MoveGenerator.IsSquareAttacked(board, kingPos, 'b');`
    * Notes: Used for check detection and castling validation

  * **`public static void RunAllTests()`**
    * Description: Executes comprehensive test suite covering all public methods
    * Parameters: None
    * Returns: `void — logs test results to Debug.Log` + call example: `MoveGenerator.RunAllTests();`
    * Notes: Tests move generation, legality checking, attack detection, promotions

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
    private void MoveGenerator_Check()
    {
        // Test all major public APIs in minimal lines
        var board = new ChessBoard();
        var moves = MoveGenerator.GenerateLegalMoves(board);
        var testMove = ChessMove.FromUCI("e2e4", board);
        bool isLegal = MoveGenerator.IsLegalMove(board, testMove);
        bool attacked = MoveGenerator.IsSquareAttacked(board, new v2(4, 4), 'w');
        MoveGenerator.RunAllTests();
        
        Debug.Log($"API Results: {moves.Count} moves, move legal: {isLegal}, square attacked: {attacked}, tests completed");
    }
}
```

## Control Flow & Responsibilities
Generates pseudo-legal moves then filters for king safety using temporary board simulation.

## Performance & Threading
Heavy computation for complex positions, creates board clones for validation.

## Cross-file Dependencies
ChessBoard.cs ChessMove.cs for position and move representations.

## Major Functionality
Complete move generation, castling rights, en passant, promotion handling, pin detection.

`checksum: A7B3C9F2 v0.3.min`