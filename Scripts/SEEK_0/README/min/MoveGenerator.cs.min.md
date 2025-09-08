# Source: `MoveGenerator.cs` — Static chess move generator with full rule compliance

## Short description

Static class providing comprehensive legal chess move generation for any position. Supports standard chess and Chess960 with complete rule validation including castling, en passant, promotions, check detection, and pin handling.

## Metadata

* **Filename:** `MoveGenerator.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependencies:** `System`, `System.Collections.Generic`, `System.Linq`, `UnityEngine`, `SPACE_UTIL`
* **Public types:** `MoveGenerator (static class)`
* **Unity version:** Compatible with Unity 2020.3+

## Public API Summary

| Type | Member | Signature | Short Purpose | OneLiner Call |
|------|---------|-----------|---------------|---------------|
| List<ChessMove> | Method | public static List<ChessMove> GenerateLegalMoves(ChessBoard board) | Generates all legal moves for current position | var moves = MoveGenerator.GenerateLegalMoves(board); |
| bool | Method | public static bool IsLegalMove(ChessBoard board, ChessMove move) | Validates if move is legal (doesn't leave king in check) | bool valid = MoveGenerator.IsLegalMove(board, move); |
| bool | Method | public static bool IsSquareAttacked(ChessBoard board, v2 square, char attackingSide) | Checks if square is under attack by given side | bool attacked = MoveGenerator.IsSquareAttacked(board, pos, 'w'); |
| void | Method | public static void RunAllTests() | Executes comprehensive test suite for all methods | MoveGenerator.RunAllTests(); |

## Important Types

### `MoveGenerator`

* **Kind:** static class
* **Responsibility:** Provides comprehensive chess move generation with full rule compliance and validation
* **Constructor(s):** N/A (static class)
* **Public Properties:** None
* **Public Methods:**
  * **`public static List<ChessMove> GenerateLegalMoves(ChessBoard board)`**
    * Description: Generates all legal moves for the current position
    * Parameters: `board : ChessBoard — chess position to analyze`
    * Returns: `List<ChessMove> — all legal moves available` + call example: `var legalMoves = MoveGenerator.GenerateLegalMoves(chessBoard);`
    * Notes: Filters pseudo-legal moves to ensure king safety, handles all piece types and special moves
  * **`public static bool IsLegalMove(ChessBoard board, ChessMove move)`**
    * Description: Validates if a move is legal by checking it doesn't leave own king in check
    * Parameters: `board : ChessBoard — current position`, `move : ChessMove — move to validate`
    * Returns: `bool — true if move is legal` + call example: `bool isValid = MoveGenerator.IsLegalMove(board, candidateMove);`
    * Notes: Makes temporary move and checks for king safety
  * **`public static bool IsSquareAttacked(ChessBoard board, v2 square, char attackingSide)`**
    * Description: Determines if a square is under attack by pieces of the given side
    * Parameters: `board : ChessBoard — current position`, `square : v2 — target square coordinates`, `attackingSide : char — 'w' for white or 'b' for black`
    * Returns: `bool — true if square is attacked` + call example: `bool underAttack = MoveGenerator.IsSquareAttacked(board, kingPos, 'b');`
    * Notes: Checks all piece attack patterns including pawns, sliding pieces, and knights
  * **`public static void RunAllTests()`**
    * Description: Executes comprehensive test suite covering all move generation functionality
    * Parameters: None
    * Returns: `void — outputs test results to Debug.Log` + call example: `MoveGenerator.RunAllTests();`
    * Notes: Tests include performance benchmarks, edge cases, and rule compliance validation

## Example Usage

**Required namespaces:**
```csharp
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
        var board = new ChessBoard(); // Starting position
        var legalMoves = MoveGenerator.GenerateLegalMoves(board);
        var testMove = ChessMove.FromUCI("e2e4", board);
        bool isValid = MoveGenerator.IsLegalMove(board, testMove);
        bool e4Attacked = MoveGenerator.IsSquareAttacked(board, new v2(4, 3), 'b');
        MoveGenerator.RunAllTests();
        
        Debug.Log($"API Results: {legalMoves.Count} moves, e2e4 valid: {isValid}, e4 attacked: {e4Attacked}, Tests executed");
    }
}
```

## Control Flow & Responsibilities

Pseudo-legal generation → legality filtering → complete rule compliance validation for all chess positions.

## Performance & Threading

Optimized sliding piece generation, efficient pin detection, main-thread only operations.

## Cross-file Dependencies

References `ChessBoard.cs`, `ChessMove.cs` for board state and move representation.

## Major Functionality

Comprehensive move generation, Chess960 castling, en passant, promotion handling, attack detection.

`checksum: a7f3d9e2 v0.3.min`

