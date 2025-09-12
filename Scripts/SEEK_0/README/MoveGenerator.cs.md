# Source: `MoveGenerator.cs` — Generates legal chess moves with Chess960 and special rules support

## Short description (2–4 sentences)
The file implements a comprehensive chess move generator that produces legal moves for all piece types in any chess position. It supports standard chess rules including castling, en passant, pawn promotion, and Chess960 variant with flexible rook positions. The generator filters pseudo-legal moves to ensure king safety and handles complex scenarios like pinned pieces and check evasion.

## Metadata

* **Filename:** `MoveGenerator.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `SPACE_UTIL` (provides v2 coordinate type)
* **Estimated lines:** 1400
* **Estimated chars:** 58000
* **Public types:** `MoveGenerator (static class)`
* **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
* **Dependencies:** `ChessBoard.cs`, `ChessMove.cs`, `SPACE_UTIL.v2` (SPACE_UTIL is external namespace), `UnityEngine.Debug`

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| List&lt;ChessMove&gt; | GenerateLegalMoves | `public static List<ChessMove> GenerateLegalMoves(ChessBoard board)` | Generate all legal moves for current position | `List<ChessMove> moves = MoveGenerator.GenerateLegalMoves(board);` |
| bool | IsLegalMove | `public static bool IsLegalMove(ChessBoard board, ChessMove move)` | Check if move is legal (doesn't leave king in check) | `bool legal = MoveGenerator.IsLegalMove(board, move);` |
| bool | IsSquareAttacked | `public static bool IsSquareAttacked(ChessBoard board, v2 square, char attackingSide)` | Check if square is attacked by given side | `bool attacked = MoveGenerator.IsSquareAttacked(board, square, 'w');` |
| void | RunAllTests | `public static void RunAllTests()` | Run comprehensive move generation tests | `MoveGenerator.RunAllTests();` |

## Important types — details

### `MoveGenerator` (static class)
* **Kind:** static class
* **Responsibility:** Provides chess move generation functionality with full rule compliance including special moves and Chess960 support.
* **Constructor(s):** N/A (static class)
* **Public properties / fields:** None
* **Public methods:** 
  * **Signature:** `public static List<ChessMove> GenerateLegalMoves(ChessBoard board)`
    * **Description:** Generates all legal moves for the current position by filtering pseudo-legal moves.
    * **Parameters:** 
      * board : ChessBoard — the chess position to analyze
    * **Returns:** List&lt;ChessMove&gt; — list of legal moves, example: `List<ChessMove> moves = MoveGenerator.GenerateLegalMoves(board);`
    * **Throws:** Returns empty list if board is null
    * **Side effects / state changes:** None (pure function)
    * **Complexity / performance:** O(n²) where n is number of pieces, includes legality checking
    * **Notes:** Handles all piece types, special moves, and king safety validation

  * **Signature:** `public static bool IsLegalMove(ChessBoard board, ChessMove move)`
    * **Description:** Validates if a move is legal by temporarily making it and checking for king safety.
    * **Parameters:**
      * board : ChessBoard — current position
      * move : ChessMove — move to validate
    * **Returns:** bool — true if legal, example: `bool legal = MoveGenerator.IsLegalMove(board, move);`
    * **Throws:** Returns false for null board or invalid move
    * **Side effects / state changes:** Temporarily modifies board clone for validation
    * **Complexity / performance:** O(n) for attack detection after move simulation
    * **Notes:** Uses board cloning and temporary move execution

  * **Signature:** `public static bool IsSquareAttacked(ChessBoard board, v2 square, char attackingSide)`
    * **Description:** Determines if a square is under attack by pieces of the specified side.
    * **Parameters:**
      * board : ChessBoard — current position
      * square : v2 — square coordinates to check
      * attackingSide : char — 'w' or 'b' for attacking side
    * **Returns:** bool — true if attacked, example: `bool attacked = MoveGenerator.IsSquareAttacked(board, square, 'w');`
    * **Throws:** Returns false for null board or out-of-bounds square
    * **Side effects / state changes:** None (pure function)
    * **Complexity / performance:** O(n) where n is number of attacking pieces
    * **Notes:** Checks all piece attack patterns including pawns, sliding pieces, knights

  * **Signature:** `public static void RunAllTests()`
    * **Description:** Executes comprehensive test suite for all move generation functionality.
    * **Parameters:** None
    * **Returns:** void — outputs test results to Unity console, example: `MoveGenerator.RunAllTests();`
    * **Throws:** None
    * **Side effects / state changes:** Logs test results to Unity Debug console with color-coded output
    * **Complexity / performance:** O(k) where k is number of test cases, includes performance benchmarks
    * **Notes:** Tests all public methods, edge cases, and performance characteristics

## Example usage

```csharp
// Required namespaces:
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
// using GPTDeepResearch;
// using SPACE_UTIL;

public class ExampleUsage : MonoBehaviour 
{
    private void MoveGenerator_Check()
    {
      // Initialize starting position
      ChessBoard board = new ChessBoard();

      // Generate all legal moves
      List<ChessMove> moves = MoveGenerator.GenerateLegalMoves(board);

      LOG.SaveLog(moves.ToTable(name: "LIST<moves>", toString: true));
      // Expected output: "Generated 20 legal moves from starting position"
      Debug.Log($"<color=white>Generated {moves.Count} legal moves from starting position</color>");


      // Check specific move legality
      ChessMove testMove = ChessMove.FromUCI("e2e4", board);
      bool isLegal = MoveGenerator.IsLegalMove(board, testMove);
      // Expected output: "Move e2-e4 is legal: True"
      Debug.Log($"<color=white>Move e2-e4 is legal: {isLegal}</color>");

      board.MakeMove(testMove);
      board.MakeMove(ChessMove.FromPGN("d5", board));   // black turn
      board.MakeMove(ChessMove.FromUCI("e4d5", board));
      board.MakeMove(ChessMove.FromPGN("Qxd5", board)); // black turn
      board.MakeMove(ChessMove.FromPGN("a3", board));
      board.MakeMove(ChessMove.FromPGN("Qe5", board));  // black turn
      // board.MakeMove(ChessMove.FromPGN("Qe2", board));
      Debug.Log(board.ToFEN());

      // Test square attack detection
      v2 kingSquare = new v2(4, 0); // e1
      bool kingAttacked = MoveGenerator.IsSquareAttacked(board, kingSquare, 'b');

      // Expected output: "King square e1 under attack: False"
      Debug.Log($"<color=white>King square e1 under attack: {kingAttacked}</color>");

      // Test promotion scenario
      ChessBoard promoBoard = new ChessBoard("8/P7/8/8/8/8/8/k6K w - - 0 1");
      List<ChessMove> promoMoves = MoveGenerator.GenerateLegalMoves(promoBoard);
      // LOG.SaveLog(promoMoves.ToTable(name: "LIST<promoMoves>"));
      int promotionCount = promoMoves.Count(m => m.moveType == ChessMove.MoveType.Promotion);

      // Expected output: "Promotion moves available: 4"
      Debug.Log($"<color=white>Promotion moves available: {promotionCount}</color>");

      // Test castling availability
      ChessBoard castleBoard = new ChessBoard("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");
      List<ChessMove> castleMoves = MoveGenerator.GenerateLegalMoves(castleBoard);
      // LOG.SaveLog(castleMoves.ToTable(name: "LIST<castleMoves>"));
      int castlingCount = castleMoves.Count(m => m.moveType == ChessMove.MoveType.Castling);

      // Expected output: "Castling moves available: 2"
      Debug.Log($"<color=white>Castling moves available: {castlingCount}</color>");

      // Test en passant
      ChessBoard epBoard = new ChessBoard("8/8/8/pP6/8/8/8/k6K w - a6 0 1");
      List<ChessMove> epMoves = MoveGenerator.GenerateLegalMoves(epBoard);
      LOG.SaveLog(epMoves.ToTable(name: "LIST<enPassantMoves>"));
      int enPassantCount = epMoves.Count(m => m.moveType == ChessMove.MoveType.EnPassant);

      // Expected output: "En passant moves available: 1"
      Debug.Log($"<color=white>En passant moves available: {enPassantCount}</color>");
      // Expected output: Various colored test results showing pass/fail status
      Debug.Log("<color=white>==== Move generation testing completed ====</color>");

      // Run comprehensive tests
      Debug.Log("MoveGenerator.RunAllTests() for MoveGenerator static class");
      MoveGenerator.RunAllTests();
    }
}
```

## Control flow / responsibilities & high-level algorithm summary /Side effects and I/O

Generates pseudo-legal moves for all pieces, then filters through legality checking by simulating moves and detecting check states. Uses directional vectors for sliding pieces, lookup tables for knights, special handling for pawns, castling, and en passant. Core algorithm: enumerate pieces → generate candidate moves → validate king safety.

## Performance, allocations, and hotspots / Threading / async considerations

Heavy list allocations during move generation, O(n²) complexity for legal filtering. Main-thread only, no threading.

## Security / safety / correctness concerns

Null board handling, out-of-bounds coordinate validation, temporary board cloning for move validation.

## Tests, debugging & observability

Comprehensive built-in test suite with `RunAllTests()` method covering all functionality, performance benchmarks, and edge cases with color-coded Unity Debug logging.

## Cross-file references

Depends on `ChessBoard.cs` for position representation, `ChessMove.cs` for move structure, `SPACE_UTIL.v2` for coordinates.

<!-- ## TODO / Known limitations / Suggested improvements

* No known TODOs in source code
* Could optimize with bitboards for performance
* Move ordering for alpha-beta integration
* Threaded move generation for complex positions -->

## Appendix

Key private helpers: `GeneratePseudoLegalMoves()`, `MakeMove()`, `FindKing()`, `IsPathClear()`, `GenerateSlidingMoves()` handle core generation logic and board manipulation for legality testing.

## General Note: important behaviors

Major functionalities: Pawn promotion (all four pieces), castling (Chess960 compatible), en passant capture, pin detection, check evasion, comprehensive legal move filtering with king safety validation.

`checksum: a7f2b9e1 v0.3`