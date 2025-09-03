# Source: `MoveGenerator.cs` — Legal chess move generation with Chess960 support

## Short description (2–4 sentences)
This file implements comprehensive legal chess move generation for all piece types including pawns, rooks, knights, bishops, queens, and kings. It supports standard chess rules plus Chess960 castling, en passant captures, pawn promotions, pin detection, and check evasion. The generator ensures all returned moves are legal by filtering out moves that would leave the king in check.

## Metadata

* **Filename:** `MoveGenerator.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `SPACE_UTIL` (for v2 vector type), `System`, `System.Collections.Generic`, `System.Linq`, `UnityEngine`
* **Estimated lines:** ~750
* **Estimated chars:** ~35,000
* **Public types:** `MoveGenerator (static class)`
* **Unity version / Target framework:** Unity 2020.3+ / .NET Standard 2.0
* **Dependencies:** `ChessBoard.cs`, `ChessMove.cs`, `SPACE_UTIL.v2` (SPACE_UTIL is namespace), `UnityEngine.Debug`

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| List<ChessMove> (class) | GenerateLegalMoves | `public static List<ChessMove> GenerateLegalMoves(ChessBoard board)` | Generate all legal moves for current position | `var moves = MoveGenerator.GenerateLegalMoves(board);` |
| bool (builtin-type) | IsLegalMove | `public static bool IsLegalMove(ChessBoard board, ChessMove move)` | Check if move is legal (doesn't leave king in check) | `bool legal = MoveGenerator.IsLegalMove(board, move);` |
| bool (builtin-type) | IsSquareAttacked | `public static bool IsSquareAttacked(ChessBoard board, v2 square, char attackingSide)` | Check if square is attacked by given side | `bool attacked = MoveGenerator.IsSquareAttacked(board, square, 'w');` |
| void (builtin-type) | RunAllTests | `public static void RunAllTests()` | Run comprehensive move generation tests | `MoveGenerator.RunAllTests();` |

## Important types — details

### `MoveGenerator`
* **Kind:** static class (GPTDeepResearch.MoveGenerator)
* **Responsibility:** Provides static methods for generating and validating chess moves with full rule compliance including Chess960 support.
* **Constructor(s):** N/A (static class)
* **Public properties / fields:** None
* **Public methods:**
  * **Signature:** `public static List<ChessMove> GenerateLegalMoves(ChessBoard board)`
    * **Description:** Generates all legal moves for the current position by filtering pseudo-legal moves.
    * **Parameters:** board : ChessBoard — current chess position
    * **Returns:** List<ChessMove> — all legal moves available, example: `var moves = MoveGenerator.GenerateLegalMoves(board);`
    * **Throws:** None explicitly
    * **Side effects / state changes:** None (pure function)
    * **Complexity / performance:** O(n²) where n is number of pieces, includes legality checking
    * **Notes:** Main entry point for move generation, ensures all moves are legal
  
  * **Signature:** `public static bool IsLegalMove(ChessBoard board, ChessMove move)`
    * **Description:** Validates if a move is legal by checking if it leaves own king in check.
    * **Parameters:** board : ChessBoard — current position, move : ChessMove — move to validate
    * **Returns:** bool — true if legal, example: `bool legal = MoveGenerator.IsLegalMove(board, move);`
    * **Throws:** None explicitly
    * **Side effects / state changes:** Creates temporary board clone for testing
    * **Complexity / performance:** O(n) for attack checking after move simulation
    * **Notes:** Uses board cloning and temporary move execution for validation
  
  * **Signature:** `public static bool IsSquareAttacked(ChessBoard board, v2 square, char attackingSide)`
    * **Description:** Determines if a square is under attack by pieces of the specified side.
    * **Parameters:** board : ChessBoard — current position, square : v2 — target square, attackingSide : char — 'w' or 'b'
    * **Returns:** bool — true if square is attacked, example: `bool attacked = MoveGenerator.IsSquareAttacked(board, square, 'w');`
    * **Throws:** None explicitly
    * **Side effects / state changes:** None (pure function)
    * **Complexity / performance:** O(n) where n is number of attacking pieces
    * **Notes:** Used for check detection and castling validation
  
  * **Signature:** `public static void RunAllTests()`
    * **Description:** Executes comprehensive test suite for all move generation functionality.
    * **Parameters:** None
    * **Returns:** void, example: `MoveGenerator.RunAllTests();`
    * **Throws:** None explicitly
    * **Side effects / state changes:** Outputs test results to Unity Debug.Log
    * **Complexity / performance:** O(1) — fixed test cases
    * **Notes:** Development/debugging tool for validating move generation correctness

## Example usage
```csharp
// namespace GPTDeepResearch required
using GPTDeepResearch;
using SPACE_UTIL;

// Generate all legal moves for current position
ChessBoard board = new ChessBoard();
List<ChessMove> legalMoves = MoveGenerator.GenerateLegalMoves(board);

// Check if specific move is legal
ChessMove move = ChessMove.FromUCI("e2e4", board);
bool isLegal = MoveGenerator.IsLegalMove(board, move);

// Check if square is under attack
bool kingInCheck = MoveGenerator.IsSquareAttacked(board, new v2(4, 0), 'b');

// Run test suite
MoveGenerator.RunAllTests();
```

## Control flow / responsibilities & high-level algorithm summary
Generates pseudo-legal moves for all pieces, then filters through legality checking by simulating moves and detecting check states.

## Performance, allocations, and hotspots
Heavy list allocations during move generation; O(n²) complexity for full legal move generation.

## Security / safety / correctness concerns
Relies on board cloning for move validation; potential null reference if board state is invalid.

## Tests, debugging & observability
Comprehensive built-in test suite with Unity Debug.Log output covering all move types and edge cases.

## Cross-file references
Depends on `ChessBoard.cs` for position representation, `ChessMove.cs` for move structure, `SPACE_UTIL.v2` for coordinates.

## General Note: important behaviors
Major functionalities include pawn promotion (all four pieces), en passant capture, Chess960 castling, pin detection, and check evasion.

`checksum: a7f3b8d2`