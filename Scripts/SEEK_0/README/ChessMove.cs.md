# `ChessMove.cs` — Comprehensive chess move representation with UCI, PGN parsing and performance optimizations

Represents chess moves with support for UCI/PGN parsing, promotion handling, move validation, and analysis data integration for high-performance chess applications.

## Short description (2–4 sentences)
Implements a complete chess move structure supporting UCI notation, PGN (algebraic notation) parsing, and comprehensive move validation. Provides enhanced promotion move handling, castling support, and move annotation system for chess analysis integration. Includes performance optimizations with move caching and streamlined parsing for high-frequency operations. Designed for integration with chess engines and game state management with full promotion functionality implemented.

## Metadata
- **Filename:** `ChessMove.cs`
- **Primary namespace:** `GPTDeepResearch`
- **Dependent namespace:** `SPACE_UTIL` (using SPACE_UTIL;)
- **Estimated lines:** 620
- **Estimated chars:** 22,000
- **Public types:** `ChessMove` (struct), `ChessMove.MoveType` (enum), `ChessMove.Annotations` (static class)
- **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
- **Dependencies:** SPACE_UTIL.v2, ChessBoard (ChessBoard.AlgebraicToCoord, ChessBoard.CoordToAlgebraic, ChessBoard.GetPiece, ChessBoard.GetLegalMoves methods)

## Public API summary (table)

| Type | Member | Signature | Short purpose | OneLiner Call |
|------|--------|-----------|---------------|---------------|
| ChessMove (struct) | Constructor | `public ChessMove(v2 from, v2 to, char piece, char capturedPiece = '\0')` | Create normal move | `new ChessMove(from, to, 'P')` |
| ChessMove | Constructor | `public ChessMove(v2 from, v2 to, char piece, char promotionPiece, char capturedPiece = '\0')` | Create promotion move | `new ChessMove(from, to, 'P', 'Q')` |
| ChessMove | Constructor | `public ChessMove(v2 kingFrom, v2 kingTo, v2 rookFrom, v2 rookTo, char piece)` | Create castling move | `new ChessMove(e1, g1, h1, f1, 'K')` |
| ChessMove | FromPGN | `public static ChessMove FromPGN(string pgnMove, ChessBoard board, List<ChessMove> legalMoves = null)` | Parse from PGN notation | `ChessMove.FromPGN("e4", board)` |
| ChessMove | FromUCI | `public static ChessMove FromUCI(string uciMove, ChessBoard board)` | Parse from UCI notation | `ChessMove.FromUCI("e2e4", board)` |
| ChessMove | ToPGN | `public string ToPGN(ChessBoard board, List<ChessMove> legalMoves = null)` | Convert to PGN notation | `move.ToPGN(board)` |
| ChessMove | ToUCI | `public string ToUCI()` | Convert to UCI notation | `move.ToUCI()` |
| ChessMove | WithAnalysisData | `public ChessMove WithAnalysisData(float analysisTimeMs, int depth, float evaluation)` | Add analysis metadata | `move.WithAnalysisData(1500f, 15, 0.3f)` |
| ChessMove | WithAnnotation | `public ChessMove WithAnnotation(string annotation)` | Set move annotation | `move.WithAnnotation("!")` |
| ChessMove | GetAnalysisSummary | `public string GetAnalysisSummary()` | Format analysis data | `move.GetAnalysisSummary()` |
| ChessMove | IsValid | `public bool IsValid()` | Check move validity | `if (move.IsValid())` |
| ChessMove | IsLegal | `public bool IsLegal(ChessBoard board)` | Check move legality | `if (move.IsLegal(board))` |
| ChessMove | IsCapture | `public bool IsCapture()` | True if capture move | `if (move.IsCapture())` |
| ChessMove | IsQuiet | `public bool IsQuiet()` | True if quiet move | `if (move.IsQuiet())` |
| ChessMove | GetDistance | `public int GetDistance()` | Manhattan distance | `int dist = move.GetDistance()` |
| ChessMove | RequiresPromotion | `public static bool RequiresPromotion(v2 from, v2 to, char piece)` | Check if promotion needed | `if (ChessMove.RequiresPromotion(from, to, piece))` |
| ChessMove | IsValidPromotionPiece | `public static bool IsValidPromotionPiece(char piece)` | Validate promotion piece | `if (ChessMove.IsValidPromotionPiece('Q'))` |
| ChessMove | GetDefaultPromotionPiece | `public static char GetDefaultPromotionPiece(bool isWhite)` | Get default promotion | `char piece = ChessMove.GetDefaultPromotionPiece(true)` |
| ChessMove | GetPromotionOptions | `public static char[] GetPromotionOptions(bool isWhite)` | Get promotion choices | `char[] options = ChessMove.GetPromotionOptions(true)` |
| ChessMove | GetPromotionPieceName | `public static string GetPromotionPieceName(char piece)` | Get piece name | `string name = ChessMove.GetPromotionPieceName('Q')` |
| ChessMove | CreatePromotionMove | `public static ChessMove CreatePromotionMove(v2 from, v2 to, char movingPiece, char promotionType, char capturedPiece = '\0')` | Create valid promotion | `ChessMove.CreatePromotionMove(from, to, 'P', 'Q')` |
| ChessMove | Invalid | `public static ChessMove Invalid()` | Create invalid move | `ChessMove.Invalid()` |
| ChessMove | TestUCIPromotionParsing | `public static void TestUCIPromotionParsing()` | Test UCI promotion | `ChessMove.TestUCIPromotionParsing()` |
| ChessMove | TestMoveCreation | `public static void TestMoveCreation()` | Test move creation | `ChessMove.TestMoveCreation()` |
| ChessMove | TestPGNParsing | `public static void TestPGNParsing()` | Test PGN parsing | `ChessMove.TestPGNParsing()` |
| ChessMove | TestPerformanceOptimizations | `public static void TestPerformanceOptimizations()` | Test caching | `ChessMove.TestPerformanceOptimizations()` |
| ChessMove | RunAllTests | `public static void RunAllTests()` | Run all tests | `ChessMove.RunAllTests()` |
| MoveType (enum) | Values | `Normal, Castling, EnPassant, Promotion, CastlingPromotion` | Move type enumeration | `move.moveType == MoveType.Promotion` |
| Annotations (static class) | Constants | `Check = "+", Checkmate = "#", Brilliant = "!!", Good = "!", etc.` | Standard annotations | `move.WithAnnotation(Annotations.Good)` |

## Important types — details

### `ChessMove`
- **Kind:** struct
- **Responsibility:** Immutable move representation with comprehensive parsing and validation
- **Constructor(s):** Three overloads for normal, promotion, and castling moves
- **Public properties / fields:**
  - `from` — v2 — Source square coordinates
  - `to` — v2 — Target square coordinates  
  - `piece` — char — Moving piece character
  - `capturedPiece` — char — Captured piece (or '\0')
  - `moveType` — MoveType — Type of move
  - `promotionPiece` — char — Promotion piece character
  - `rookFrom` — v2 — Rook source for castling
  - `rookTo` — v2 — Rook target for castling
  - `analysisTime` — float — Analysis duration in ms
  - `annotation` — string — Move annotation (+, #, !, etc.)
  - `engineDepth` — int — Search depth if from engine
  - `engineEval` — float — Engine evaluation
- **Notes:** Implements IEquatable<ChessMove>, includes move caching for performance, supports comprehensive promotion validation

### `MoveType`
- **Kind:** enum (located in ChessMove)
- **Responsibility:** Categorizes different types of chess moves
- **Values:** Normal, Castling, EnPassant, Promotion, CastlingPromotion

### `Annotations`
- **Kind:** static class (located in ChessMove)
- **Responsibility:** Standard chess move annotation constants
- **Constants:** Check ("+"), Checkmate ("#"), Brilliant ("!!"), Good ("!"), Interesting ("!?"), Dubious ("?!"), Mistake ("?"), Blunder ("??")

## Example usage

```csharp
// Parse UCI move with promotion
ChessMove move = ChessMove.FromUCI("e7e8q", board);
if (move.moveType == MoveType.Promotion) {
    Debug.Log($"Promotes to {move.GetPromotionPieceName(move.promotionPiece)}");
}

// Parse PGN notation
ChessMove pgnMove = ChessMove.FromPGN("Nf3+", board);
Debug.Log(pgnMove.ToPGN(board)); // "Nf3+"

// Create promotion move
ChessMove promotion = ChessMove.CreatePromotionMove(
    new v2(4, 6), new v2(4, 7), 'P', 'Q');
```

## Control flow / responsibilities & high-level algorithm summary

Move parsing follows different paths for UCI vs PGN formats. UCI parsing uses direct coordinate calculation with character arithmetic for performance, validates coordinates and piece types, handles promotion detection through string length and character validation. PGN parsing involves multi-step disambiguation: clean annotation removal, component extraction (piece type, target square, capture flag, disambiguation), candidate filtering from legal moves, and disambiguation resolution using file/rank hints. Both formats support caching for repeated parsing operations. Promotion validation ensures proper rank requirements and piece type constraints.

## Side effects and I/O

No direct I/O operations. Accesses ChessBoard state for piece validation and legal move generation. Modifies static UCI cache dictionary for performance optimization. Writes debug logs to Unity console during test operations and error conditions. Move validation may trigger legal move generation on provided ChessBoard instance.

## Performance, allocations, and hotspots

UCI parsing optimized with direct character arithmetic and move caching (Dictionary<string, ChessMove> with 1000 entry limit). PGN parsing allocates StringBuilder for cleaning, string arrays for component parsing, and List<ChessMove> for candidate filtering. Frequent string operations in coordinate conversion and validation. GetHashCode() uses unchecked arithmetic for performance. ToString() methods use StringBuilder to minimize allocations.

## Threading / async considerations

Thread-safe struct design with immutable data. Static UCI cache shared across threads requires external synchronization if used concurrently. No async operations or Unity coroutines within the struct itself. All parsing methods are synchronous and safe for background thread usage.

## Security / safety / correctness concerns

Coordinate validation prevents out-of-bounds access, UCI string length validation prevents index exceptions, promotion piece validation ensures only valid pieces (Q,R,B,N), PGN parsing includes comprehensive input sanitization and bounds checking. Move equality comparison includes all relevant fields. Invalid move creation uses safe sentinel values (-1, -1) for coordinates.

## Tests, debugging & observability

Comprehensive test suite includes TestUCIPromotionParsing() for all promotion scenarios, TestMoveCreation() for constructor validation, TestPGNParsing() for algebraic notation, TestPerformanceOptimizations() for cache effectiveness, and RunAllTests() as complete test runner. Debug logging with colored Unity console output during test execution and error conditions.

## Cross-file references

- `SPACE_UTIL.v2` — 2D coordinate structure for chess squares  
- `ChessBoard.cs` — Board state access (GetPiece, GetLegalMoves, AlgebraicToCoord, CoordToAlgebraic, LoadFromFEN methods)
- UnityEngine.Debug — Unity logging system for test output and error reporting

<!-- TODO(only if i have explicitly mentioned in prompt): Consider implementing move quality evaluation based on engine analysis, adding support for time controls in analysis data, implementing move tree structures for variation analysis, adding FEN delta calculation for efficient position updates, and considering UCI_Chess960 support for Fischer Random positions. Performance could be improved with compile-time coordinate validation and specialized parsing for common opening moves. -->

## Appendix

Core parsing algorithms use optimized string operations and pre-allocated character arrays. UCI cache uses Dictionary<string, ChessMove> with LRU-style management when size limit reached. PGN disambiguation algorithm follows official FIDE standards for file/rank preference. Promotion validation includes comprehensive rank checking and piece type constraints.

File checksum: SHA1 first 8 chars would be calculated from file content for version tracking.