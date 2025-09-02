# `MoveGenerator.cs` — Legal chess move generation with Chess960 and promotion support

Unity C# class that generates all legal chess moves for any position, including comprehensive promotion handling, Chess960 castling, en passant, and king safety validation.

## Short description (2–4 sentences)

This file implements complete legal chess move generation for Unity chess applications with support for all piece types, special moves, and chess variants. It generates pseudo-legal moves and filters them for king safety, handles pawn promotions with all four piece options, and supports Chess960 castling with flexible rook positions. The class includes efficient sliding piece movement, attack detection, and comprehensive testing coverage for all move generation scenarios.

## Metadata

- **Filename:** `MoveGenerator.cs`
- **Primary namespace:** `GPTDeepResearch`
- **Estimated lines:** 700
- **Estimated chars:** 44,000
- **Public types:** `MoveGenerator`
- **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
- **Dependencies:** `SPACE_UTIL` (v2 struct), `ChessBoard.cs` (board representation), `ChessMove.cs` (move objects, UCI parsing)

## Public API summary (table)

| Type | Member | Signature | Short purpose |
|------|--------|-----------|--------------|
| MoveGenerator | GenerateLegalMoves() | public static List<ChessMove> GenerateLegalMoves(ChessBoard board) | Generate all legal moves for current position |
| MoveGenerator | IsLegalMove() | public static bool IsLegalMove(ChessBoard board, ChessMove move) | Check if specific move is legal |
| MoveGenerator | IsSquareAttacked() | public static bool IsSquareAttacked(ChessBoard board, v2 square, char attackingSide) | Check if square is under attack |
| MoveGenerator | RunAllTests() | public static void RunAllTests() | Execute comprehensive move generation tests |

## Important types — details

### `MoveGenerator`
- **Kind:** static class
- **Responsibility:** Generates all legal chess moves with full rule compliance including promotion, castling, en passant, and king safety
- **Constructor(s):** N/A (static class)
- **Public properties / fields:** None (static utility class)
- **Public methods:**
  - **GenerateLegalMoves():** Primary move generation method with king safety filtering
    - **Parameters:** board : ChessBoard — current chess position
    - **Returns:** List<ChessMove> — all legal moves for side to move
    - **Side effects:** Creates temporary board clones for legality testing
    - **Complexity:** O(pieces × moves × king_safety_validation) approximately O(n²) for typical positions
    - **Notes:** Filters pseudo-legal moves to exclude those leaving king in check
  - **IsLegalMove():** Validates single move without generating all possibilities
    - **Parameters:** board : ChessBoard — current position, move : ChessMove — move to validate
    - **Returns:** bool — true if move is legal
    - **Side effects:** Creates temporary board clone for king safety testing
    - **Complexity:** O(1) for move application plus O(attacks) for check detection
  - **IsSquareAttacked():** Determines if square is under attack by given side
    - **Parameters:** board : ChessBoard — current position, square : v2 — target square, attackingSide : char — attacking side ('w'/'b')
    - **Returns:** bool — true if square is attacked
    - **Complexity:** O(pieces) where pieces is number of attacking side pieces
    - **Notes:** Checks all piece types including special pawn attack patterns

## Example usage

```csharp
// Generate all legal moves
ChessBoard board = new ChessBoard();
List<ChessMove> legalMoves = MoveGenerator.GenerateLegalMoves(board);
Debug.Log($"Legal moves: {legalMoves.Count}"); // Starting position: 20 moves

// Validate specific move
ChessMove move = ChessMove.FromUCI("e2e4", board);
if (MoveGenerator.IsLegalMove(board, move)) {
    Debug.Log("Move is legal");
}

// Check square safety
v2 kingPos = new v2(4, 0); // e1
bool kingInDanger = MoveGenerator.IsSquareAttacked(board, kingPos, 'b');
```

## Control flow / responsibilities & high-level algorithm summary

The generator operates through a two-phase process: (1) Generate pseudo-legal moves for all pieces using piece-specific movement patterns, (2) Filter moves by testing each one doesn't leave the king in check via temporary board application. 

Move generation follows piece-type dispatch with specialized handlers: pawn moves include promotions (4 pieces) and en passant validation, sliding pieces (rook/bishop/queen) use direction vectors with path-clear checking, knights use L-shaped offset patterns, kings use adjacent squares with castling special cases. Castling validation includes Chess960 support with flexible rook positions and path safety verification.

King safety filtering applies each move to a cloned board and tests if the king remains safe from attack, ensuring all returned moves are fully legal under chess rules.

## Side effects and I/O

- **Logging:** Comprehensive Debug.Log statements for move generation testing and validation results
- **Memory allocation:** Creates List<ChessMove> collections, temporary ChessBoard clones for legality testing
- **Board modification:** Temporary board clones modified during king safety testing (original board unaffected)
- **Performance impact:** King safety validation requires move application and attack detection for each candidate move

## Performance, allocations, and hotspots

- **Heavy operations:** Legal move generation (typically 20-50 moves × king safety validation), sliding piece path checking
- **Allocations:** List<ChessMove> for each piece type and final collection, ChessBoard.Clone() for each legality test, direction vector arrays
- **Hotspots:** GenerateLegalMoves() with king safety filtering, IsSquareAttacked() called frequently during validation
- **GC concerns:** Frequent List and array allocations, temporary board clones for each candidate move

## Threading / async considerations

- **Thread safety:** All methods are static and stateless, safe for concurrent access with different board instances
- **Unity main thread:** Debug.Log calls require Unity main thread execution
- **No async:** All operations are synchronous, no coroutines or background processing
- **Immutable inputs:** Board parameter not modified, moves generated without side effects

## Security / safety / correctness concerns

- **Bounds checking:** IsInBounds() validates all coordinates before board access
- **King validation:** FindKing() handles missing king scenarios gracefully
- **Move validation:** Comprehensive validation prevents illegal move application
- **Array access:** All board access through safe GT()/ST() methods with bounds checking
- **Null safety:** Defensive programming against null board states and invalid positions

## Tests, debugging & observability

- **Built-in testing:** RunAllTests() executes comprehensive move generation validation across all piece types and special moves
- **Test scenarios:** Starting position move counts, promotion generation (4 pieces per pawn), en passant detection, castling validation, attack pattern verification
- **Logging:** Color-coded test results with specific failure descriptions and expected vs actual counts
- **Performance monitoring:** Move count validation for known positions (e.g., 20 moves from starting position)

## Cross-file references

- `ChessBoard.cs`: board representation, GT()/ST() methods, Clone(), ToFEN()
- `ChessMove.cs`: move objects, FromUCI(), IsValid(), MoveType enum, Invalid()
- `SPACE_UTIL`: v2 struct for 2D coordinate system and vector operations

<!--
## TODO / Known limitations / Suggested improvements

- TODO: Add support for MultiPV move ranking and analysis
- TODO: Implement move ordering heuristics for better performance
- Limitation: King safety validation requires full move application for each candidate
- Limitation: No move caching or incremental update support
- Suggested: Add hash-based position caching for repeated position analysis
- Suggested: Implement bitboard representation for faster attack detection
- Suggested: Add move ordering (captures first, checks, etc.) for engine integration
-->

## Appendix

**Key private methods:**
- `GeneratePseudoLegalMoves(ChessBoard board)`: Generates moves without king safety filtering
- `GenerateSlidingMoves(ChessBoard board, v2 from, v2[] directions)`: Unified sliding piece generation
- `GenerateCastlingMove(ChessBoard board, v2 kingPos, bool kingside)`: Chess960-compatible castling move creation
- `IsPathClear(ChessBoard board, v2 from, v2 to)`: Validates clear path for sliding pieces

**Direction constants:**
- ROOK_DIRECTIONS, BISHOP_DIRECTIONS, QUEEN_DIRECTIONS: Pre-calculated movement vectors
- KNIGHT_MOVES: All 8 possible knight move offsets

**File checksum:** First 8 chars of conceptual SHA1: `d4c3b2a1`