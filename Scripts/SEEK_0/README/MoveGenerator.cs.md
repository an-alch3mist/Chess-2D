# Source: `MoveGenerator.cs` — Complete legal chess move generation with Chess960 support

## Short description (2–4 sentences)
Implements comprehensive legal chess move generation for all piece types including pawns, rooks, knights, bishops, queens, and kings. Supports standard chess rules plus Chess960 castling, en passant captures, pawn promotion, check detection, and pin handling. Uses pseudo-legal move generation followed by legality filtering to ensure no moves leave the king in check. Includes extensive testing framework with 80+ test cases covering all move types and edge cases.

## Metadata

* **Filename:** `MoveGenerator.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `SPACE_UTIL` (external namespace), `System`, `System.Collections.Generic`, `System.Linq`, `UnityEngine`
* **Estimated lines:** 950
* **Estimated chars:** 38,000
* **Public types:** `MoveGenerator (static class)`
* **Unity version / Target framework:** Unity 2020.3+ / .NET Standard 2.0
* **Dependencies:** `ChessBoard.cs`, `ChessMove.cs`, `SPACE_UTIL.v2` (SPACE_UTIL is external namespace)

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| List<ChessMove> | GenerateLegalMoves | `public static List<ChessMove> GenerateLegalMoves(ChessBoard board)` | Generate all legal moves for current position | `var moves = MoveGenerator.GenerateLegalMoves(board);` |
| bool | IsLegalMove | `public static bool IsLegalMove(ChessBoard board, ChessMove move)` | Check if move is legal (doesn't leave king in check) | `bool isLegal = MoveGenerator.IsLegalMove(board, move);` |
| bool | IsSquareAttacked | `public static bool IsSquareAttacked(ChessBoard board, v2 square, char attackingSide)` | Check if square is under attack by given side | `bool attacked = MoveGenerator.IsSquareAttacked(board, pos, 'w');` |
| void | RunAllTests | `public static void RunAllTests()` | Execute comprehensive test suite for all methods | `MoveGenerator.RunAllTests();` |

## Important types — details

### `MoveGenerator` (static class)
* **Kind:** static class
* **Responsibility:** Provides chess move generation algorithms and validation for legal gameplay.
* **Constructor(s):** N/A (static class)
* **Public properties / fields:** None
* **Public methods:**
  * **Signature:** `public static List<ChessMove> GenerateLegalMoves(ChessBoard board)`
    * **Description:** Generates all legal moves for the current position by filtering pseudo-legal moves.
    * **Parameters:** 
      * board : ChessBoard — Chess position to analyze
    * **Returns:** `List<ChessMove> moves = MoveGenerator.GenerateLegalMoves(board);`
    * **Throws:** None explicitly
    * **Side effects / state changes:** None (pure function)
    * **Complexity / performance:** O(n*m) where n=pieces, m=avg moves per piece
    * **Notes:** Uses two-phase generation: pseudo-legal then legality filtering
  
  * **Signature:** `public static bool IsLegalMove(ChessBoard board, ChessMove move)`
    * **Description:** Validates if a move is legal by checking if it leaves own king in check.
    * **Parameters:**
      * board : ChessBoard — Current position
      * move : ChessMove — Move to validate
    * **Returns:** `bool isLegal = MoveGenerator.IsLegalMove(board, move);`
    * **Throws:** None explicitly
    * **Side effects / state changes:** Creates temporary board copy for testing
    * **Complexity / performance:** O(k) where k=enemy pieces that can attack king
    * **Notes:** Makes temporary move on cloned board to test legality
  
  * **Signature:** `public static bool IsSquareAttacked(ChessBoard board, v2 square, char attackingSide)`
    * **Description:** Determines if a square is under attack by pieces of the given side.
    * **Parameters:**
      * board : ChessBoard — Current position
      * square : v2 — Target square coordinates
      * attackingSide : char — 'w' or 'b' for attacking side
    * **Returns:** `bool attacked = MoveGenerator.IsSquareAttacked(board, pos, 'w');`
    * **Throws:** None explicitly
    * **Side effects / state changes:** None (pure function)
    * **Complexity / performance:** O(64) worst case checking all squares for attackers
    * **Notes:** Used for castling validation and check detection
  
  * **Signature:** `public static void RunAllTests()`
    * **Description:** Executes comprehensive test suite covering all move generation functionality.
    * **Parameters:** None
    * **Returns:** `MoveGenerator.RunAllTests();`
    * **Throws:** None explicitly
    * **Side effects / state changes:** Outputs test results via Debug.Log with color coding
    * **Complexity / performance:** O(1) constant test scenarios
    * **Notes:** Validates 80+ test cases including edge cases and special moves

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
    private void MoveGenerator_Check()
    {
        // Initialize chess board in starting position
        ChessBoard board = new ChessBoard();
        
        // Generate all legal moves for current position
        List<ChessMove> legalMoves = MoveGenerator.GenerateLegalMoves(board);
        Debug.Log($"<color=green>Legal moves in starting position: {legalMoves.Count}</color>");
        // Expected output: "Legal moves in starting position: 20"
        
        // Check specific move legality
        ChessMove testMove = ChessMove.FromUCI("e2e4", board);
        bool isLegal = MoveGenerator.IsLegalMove(board, testMove);
        Debug.Log($"<color=green>e2-e4 is legal: {isLegal}</color>");
        // Expected output: "e2-e4 is legal: True"
        
        // Test square attack detection
        v2 kingPos = new v2(4, 0); // e1 square
        bool kingAttacked = MoveGenerator.IsSquareAttacked(board, kingPos, 'b');
        Debug.Log($"<color=green>White king under attack: {kingAttacked}</color>");
        // Expected output: "White king under attack: False"
        
        // Test promotion position
        ChessBoard promoBoard = new ChessBoard("8/P7/8/8/8/8/8/k6K w - - 0 1");
        List<ChessMove> promoMoves = MoveGenerator.GenerateLegalMoves(promoBoard);
        int promotionCount = 0;
        foreach(ChessMove move in promoMoves)
        {
            if(move.moveType == ChessMove.MoveType.Promotion)
                promotionCount++;
        }
        Debug.Log($"<color=green>Promotion moves generated: {promotionCount}</color>");
        // Expected output: "Promotion moves generated: 4"
        
        // Test castling availability
        ChessBoard castlingBoard = new ChessBoard("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");
        List<ChessMove> castlingMoves = MoveGenerator.GenerateLegalMoves(castlingBoard);
        int castlingCount = 0;
        foreach(ChessMove move in castlingMoves)
        {
            if(move.moveType == ChessMove.MoveType.Castling)
                castlingCount++;
        }
        Debug.Log($"<color=green>Castling moves available: {castlingCount}</color>");
        // Expected output: "Castling moves available: 2"
        
        // Test en passant detection
        ChessBoard epBoard = new ChessBoard("8/8/8/pP6/8/8/8/k6K w - a6 0 1");
        List<ChessMove> epMoves = MoveGenerator.GenerateLegalMoves(epBoard);
        int enPassantCount = 0;
        foreach(ChessMove move in epMoves)
        {
            if(move.moveType == ChessMove.MoveType.EnPassant)
                enPassantCount++;
        }
        Debug.Log($"<color=green>En passant moves: {enPassantCount}</color>");
        // Expected output: "En passant moves: 1"
        
        // Run comprehensive test suite
        Debug.Log("<color=cyan>Running comprehensive move generation tests...</color>");
        MoveGenerator.RunAllTests();
        // Expected output: Multiple test result lines with pass/fail status
    }
}
```

## Control flow / responsibilities & high-level algorithm summary
Two-phase generation: creates pseudo-legal moves for all pieces, then filters by legality testing. Handles special moves (castling, en passant, promotion) with dedicated validation logic.

## Performance, allocations, and hotspots / Threading / async considerations
O(pieces × moves) complexity. Temporary board cloning for legality testing creates GC pressure. Main-thread only, no async support.

## Security / safety / correctness concerns
Relies on ChessBoard bounds checking. No null validation on input parameters.

## Tests, debugging & observability
Comprehensive built-in test suite with 14 test methods covering all functionality. Color-coded Debug.Log output for pass/fail results.

## Cross-file references
Dependencies: `ChessBoard.cs` (board representation), `ChessMove.cs` (move structure), `SPACE_UTIL.v2` (coordinate system from external namespace).

## TODO / Known limitations / Suggested improvements
<!-- 
* Consider move ordering optimization for better alpha-beta performance
* Add support for Chess960 Fischer Random starting positions
* Implement incremental move generation for performance
* Add move validation caching to reduce repeated calculations
-->

## Appendix
Key private helpers: `GeneratePseudoLegalMoves`, `GeneratePawnMoves`, `GenerateSlidingMoves`, `IsCastlingLegal`, `IsPathClear`. Contains 950+ lines with extensive piece-specific move generation logic.

## General Note: important behaviors
Major functionalities: Pawn promotion (all 4 pieces), En passant capture, Chess960 castling, Pin detection, Check evasion. Comprehensive test coverage with 80+ validation scenarios.

`checksum: a7f4b2c1 (v0.3)`