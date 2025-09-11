# Source: `ChessMove.cs` — Chess move representation with comprehensive UCI/PGN parsing and analysis support

Enhanced chess move struct with performance optimizations, caching, and Unity 2020.3 compatibility for game development.

## Short description (2–4 sentences)

Implements a comprehensive chess move representation system with support for UCI (Universal Chess Interface) and PGN (Portable Game Notation) parsing. Handles all chess move types including normal moves, castling, en passant, and promotions with disambiguation logic. Provides analysis integration for chess engines with timing, evaluation, and annotation support. Designed for Unity 2020.3 with performance optimizations including move caching and minimal allocation patterns.

## Metadata

* **Filename:** `ChessMove.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `using SPACE_UTIL;` (SPACE_UTIL is namespace), `using System;`, `using System.Collections.Generic;`, `using System.Text;`, `using UnityEngine;`
* **Estimated lines:** 1200
* **Estimated chars:** 45000
* **Public types:** `ChessMove (struct inherits IEquatable<ChessMove>), ChessMove.MoveType (enum), ChessMove.Annotations (static class)`
* **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
* **Dependencies:** `SPACE_UTIL.v2` (SPACE_UTIL is namespace), `ChessBoard.cs`, Unity Debug system

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| v2 (struct) | from | `public v2 from` | Source square coordinates | `var pos = move.from;` |
| v2 (struct) | to | `public v2 to` | Destination square coordinates | `var pos = move.to;` |
| char (basic-data-type) | piece | `public char piece` | Moving piece character | `var p = move.piece;` |
| char (basic-data-type) | capturedPiece | `public char capturedPiece` | Captured piece character | `var captured = move.capturedPiece;` |
| ChessMove.MoveType (enum) | moveType | `public ChessMove.MoveType moveType` | Type of chess move | `var type = move.moveType;` |
| char (basic-data-type) | promotionPiece | `public char promotionPiece` | Promotion piece character | `var promo = move.promotionPiece;` |
| v2 (struct) | rookFrom | `public v2 rookFrom` | Rook source in castling | `var rookPos = move.rookFrom;` |
| v2 (struct) | rookTo | `public v2 rookTo` | Rook destination in castling | `var rookDest = move.rookTo;` |
| float (basic-data-type) | analysisTime | `public float analysisTime { get; private set; }` | Engine analysis time in ms | `var time = move.analysisTime;` |
| string (basic-data-type) | annotation | `public string annotation { get; private set; }` | Move annotation (+, !, etc.) | `var note = move.annotation;` |
| int (basic-data-type) | engineDepth | `public int engineDepth { get; private set; }` | Engine search depth | `var depth = move.engineDepth;` |
| float (basic-data-type) | engineEval | `public float engineEval { get; private set; }` | Engine position evaluation | `var eval = move.engineEval;` |
| void (static void) | Constructor | `public ChessMove(v2 from, v2 to, char piece, char capturedPiece = '\0')` | Create normal move | `var move = new ChessMove(from, to, 'P');` |
| void (static void) | Constructor | `public ChessMove(v2 from, v2 to, char piece, char promotionPiece, char capturedPiece = '\0')` | Create promotion move | `var move = new ChessMove(from, to, 'P', 'Q');` |
| void (static void) | Constructor | `public ChessMove(v2 kingFrom, v2 kingTo, v2 rookFrom, v2 rookTo, char piece)` | Create castling move | `var move = new ChessMove(kingFrom, kingTo, rookFrom, rookTo, 'K');` |
| ChessMove (struct) | FromPGN | `public static ChessMove FromPGN(string pgnMove, ChessBoard board, List<ChessMove> legalMoves = null)` | Parse PGN notation | `var move = ChessMove.FromPGN("e4", board);` |
| ChessMove (struct) | FromUCI | `public static ChessMove FromUCI(string uciMove, ChessBoard board)` | Parse UCI notation | `var move = ChessMove.FromUCI("e2e4", board);` |
| ChessMove (struct) | CreatePromotionMove | `public static ChessMove CreatePromotionMove(v2 from, v2 to, char movingPiece, char promotionType, char capturedPiece = '\0')` | Create validated promotion | `var move = ChessMove.CreatePromotionMove(from, to, 'P', 'Q');` |
| ChessMove (struct) | Invalid | `public static ChessMove Invalid()` | Create invalid move marker | `var invalid = ChessMove.Invalid();` |
| string (basic-data-type) | ToPGN | `public string ToPGN(ChessBoard board, List<ChessMove> legalMoves = null)` | Convert to PGN notation | `string pgn = move.ToPGN(board);` |
| string (basic-data-type) | ToUCI | `public string ToUCI()` | Convert to UCI notation | `string uci = move.ToUCI();` |
| ChessMove (struct) | WithAnalysisData | `public ChessMove WithAnalysisData(float analysisTimeMs, int depth, float evaluation)` | Add engine analysis data | `var analyzed = move.WithAnalysisData(1500f, 12, 0.25f);` |
| ChessMove (struct) | WithAnnotation | `public ChessMove WithAnnotation(string annotation)` | Add move annotation | `var annotated = move.WithAnnotation("!");` |
| string (basic-data-type) | GetAnalysisSummary | `public string GetAnalysisSummary()` | Get analysis summary string | `string summary = move.GetAnalysisSummary();` |
| bool (basic-data-type) | IsValid | `public bool IsValid()` | Check move validity | `bool valid = move.IsValid();` |
| bool (basic-data-type) | IsLegal | `public bool IsLegal(ChessBoard board)` | Check if move is legal | `bool legal = move.IsLegal(board);` |
| bool (basic-data-type) | IsCapture | `public bool IsCapture()` | Check if move captures | `bool captures = move.IsCapture();` |
| bool (basic-data-type) | IsQuiet | `public bool IsQuiet()` | Check if move is quiet | `bool quiet = move.IsQuiet();` |
| int (basic-data-type) | GetDistance | `public int GetDistance()` | Get move distance | `int dist = move.GetDistance();` |
| bool (static bool) | RequiresPromotion | `public static bool RequiresPromotion(v2 from, v2 to, char piece)` | Check if promotion needed | `bool needsPromo = ChessMove.RequiresPromotion(from, to, 'P');` |
| bool (static bool) | IsValidPromotionPiece | `public static bool IsValidPromotionPiece(char piece)` | Validate promotion piece | `bool valid = ChessMove.IsValidPromotionPiece('Q');` |
| char (static char) | GetDefaultPromotionPiece | `public static char GetDefaultPromotionPiece(bool isWhite)` | Get default promotion | `char piece = ChessMove.GetDefaultPromotionPiece(true);` |
| char[] (static char[]) | GetPromotionOptions | `public static char[] GetPromotionOptions(bool isWhite)` | Get promotion options | `char[] options = ChessMove.GetPromotionOptions(true);` |
| string (static string) | GetPromotionPieceName | `public static string GetPromotionPieceName(char piece)` | Get piece name | `string name = ChessMove.GetPromotionPieceName('Q');` |
| void (static void) | RunAllTests | `public static void RunAllTests()` | Run comprehensive tests | `ChessMove.RunAllTests();` |

## Important types — details

### `ChessMove` (struct inherits IEquatable<ChessMove>)
* **Kind:** struct with full path (`ChessMove`)
* **Responsibility:** Represents chess moves with parsing, validation, and analysis capabilities
* **Constructor(s):** Three overloads for normal, promotion, and castling moves with validation
* **Public properties / fields:** 
  * `from — v2 — Source square coordinates`
  * `to — v2 — Destination square coordinates` 
  * `piece — char — Moving piece character`
  * `capturedPiece — char — Captured piece character`
  * `moveType — ChessMove.MoveType — Type of move (enum)`
  * `promotionPiece — char — Promotion piece character`
  * `rookFrom — v2 — Rook source position for castling`
  * `rookTo — v2 — Rook destination for castling`
  * `analysisTime — float — Engine analysis time in ms (get)`
  * `annotation — string — Move annotation like +, !, ? (get)`
  * `engineDepth — int — Engine search depth (get)`
  * `engineEval — float — Engine evaluation score (get)`

* **Public methods:**
  * **Signature:** `public static ChessMove FromPGN(string pgnMove, ChessBoard board, List<ChessMove> legalMoves = null)`
    * **Description:** Parses Standard Algebraic Notation with disambiguation support
    * **Parameters:** 
      * `pgnMove : string — PGN move string like "Nf3", "O-O", "e4"`
      * `board : ChessBoard — Current board position`
      * `legalMoves : List<ChessMove> — Optional legal moves for validation`
    * **Returns:** `ChessMove res = ChessMove.FromPGN("e4", board);`
    * **Throws:** None directly, returns Invalid() on parse failure
    * **Side effects / state changes:** Updates internal UCI cache
    * **Complexity / performance:** O(n) where n is legal moves count for disambiguation
    * **Notes:** Handles castling notation, captures, promotions, and annotations

  * **Signature:** `public static ChessMove FromUCI(string uciMove, ChessBoard board)`
    * **Description:** Parses Universal Chess Interface notation with caching
    * **Parameters:**
      * `uciMove : string — UCI format like "e2e4", "e7e8q"`
      * `board : ChessBoard — Current board position for piece lookup`
    * **Returns:** `ChessMove res = ChessMove.FromUCI("e2e4", board);`
    * **Throws:** None, returns Invalid() for malformed input
    * **Side effects / state changes:** Populates static UCI cache (max 1000 entries)
    * **Complexity / performance:** O(1) with cache hit, O(1) parsing without
    * **Notes:** Auto-detects promotions, validates coordinates, thread-safe cache

  * **Signature:** `public string ToPGN(ChessBoard board, List<ChessMove> legalMoves = null)`
    * **Description:** Converts move to Standard Algebraic Notation with disambiguation
    * **Parameters:**
      * `board : ChessBoard — Board position for disambiguation`
      * `legalMoves : List<ChessMove> — Legal moves for conflict resolution`
    * **Returns:** `string pgn = move.ToPGN(board);`
    * **Throws:** None, returns empty string for invalid moves
    * **Side effects / state changes:** None
    * **Complexity / performance:** O(n) for disambiguation where n is conflicting moves
    * **Notes:** Includes annotations, proper castling notation, capture symbols

  * **Signature:** `public string ToUCI()`
    * **Description:** Converts move to UCI format
    * **Parameters:** None
    * **Returns:** `string uci = move.ToUCI();`
    * **Throws:** None, returns empty string for invalid moves
    * **Side effects / state changes:** None
    * **Complexity / performance:** O(1)
    * **Notes:** Handles promotions, castling as king moves

  * **Signature:** `public bool IsValid()`
    * **Description:** Validates move coordinates and basic constraints
    * **Parameters:** None
    * **Returns:** `bool valid = move.IsValid();`
    * **Throws:** None
    * **Side effects / state changes:** None
    * **Complexity / performance:** O(1)
    * **Notes:** Checks coordinates bounds, non-null piece, from != to

  * **Signature:** `public ChessMove WithAnalysisData(float analysisTimeMs, int depth, float evaluation)`
    * **Description:** Creates copy with engine analysis metadata
    * **Parameters:**
      * `analysisTimeMs : float — Analysis time in milliseconds`
      * `depth : int — Engine search depth`
      * `evaluation : float — Position evaluation score`
    * **Returns:** `ChessMove analyzed = move.WithAnalysisData(1500f, 12, 0.25f);`
    * **Throws:** None
    * **Side effects / state changes:** None (returns new instance)
    * **Complexity / performance:** O(1)
    * **Notes:** Immutable operation, preserves original move

  * **Signature:** `public static void RunAllTests()`
    * **Description:** Executes comprehensive test suite with Unity logging
    * **Parameters:** None
    * **Returns:** `ChessMove.RunAllTests();`
    * **Throws:** None, logs failures via Debug.Log
    * **Side effects / state changes:** Outputs test results to Unity console
    * **Complexity / performance:** O(n) where n is number of test cases
    * **Notes:** Tests UCI/PGN parsing, performance, validation, equality

### `ChessMove.MoveType` (enum)
* **Kind:** enum with full path (`ChessMove.MoveType`)
* **Responsibility:** Categorizes different types of chess moves
* **Values:** `Normal, Castling, EnPassant, Promotion, CastlingPromotion`

### `ChessMove.Annotations` (static class)
* **Kind:** static class with full path (`ChessMove.Annotations`)
* **Responsibility:** Provides standard PGN annotation constants
* **Public properties / fields:**
  * `Check — const string — "+" check symbol`
  * `Checkmate — const string — "#" checkmate symbol`
  * `Brilliant — const string — "!!" brilliant move`
  * `Good — const string — "!" good move`
  * `Interesting — const string — "!?" interesting move`
  * `Dubious — const string — "?!" dubious move`
  * `Mistake — const string — "?" mistake`
  * `Blunder — const string — "??" blunder`

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
    private void ChessMove_Check()
    {
      // Initialize test board
      ChessBoard board = new ChessBoard();

      // Create moves using different constructors
      v2 from = new v2(4, 1); // e2
      v2 to = new v2(4, 3);   // e4
      ChessMove pawnMove = new ChessMove(from, to, 'P');

      // Expected output: "Move created: e2e4"
      Debug.Log($"<color=white>Move created: {pawnMove.ToUCI()}</color>");

      // Parse UCI notation with caching
      ChessMove parsed = ChessMove.FromUCI("e2e4", board);
      if (parsed.IsValid())
      {
        // Expected output: "UCI parsed successfully: e2e4"
        Debug.Log($"<color=white>UCI parsed successfully: {parsed.ToString()}, {parsed.ToUCI()}</color>");
      }

      // Parse PGN notation
      ChessMove pgnMove = ChessMove.FromPGN("Nf3", board);
      if (pgnMove.IsValid())
      {
        // Expected output: "PGN parsed: Nf3 -> g1f3"
        Debug.Log($"<color=white>PGN parsed: Nf3 -> {pgnMove.ToUCI()}</color>");
      }

      // Create promotion move
      v2 promoFrom = new v2(4, 6); // e7
      v2 promoTo = new v2(4, 7);   // e8
      ChessMove promotion = ChessMove.CreatePromotionMove(promoFrom, promoTo, 'P', 'N');

      // Expected output: "Promotion: e7e8q"
      Debug.Log($"<color=white>Promotion: {promotion.ToUCI()}</color>");

      // Add analysis data
      ChessMove analyzed = promotion.WithAnalysisData(1500f, 12, 0.25f);
      ChessMove annotated = analyzed.WithAnnotation(ChessMove.Annotations.Good);

      // Expected output: "Analysis: Eval: +0.25, Depth: 12, Time: 1500ms"
      Debug.Log($"<color=white>Analysis: {annotated.GetAnalysisSummary()}</color>");

      // Test move properties
      bool isCapture = pawnMove.IsCapture();
      bool isQuiet = pawnMove.IsQuiet();
      int distance = pawnMove.GetDistance();

      // Expected output: "Move properties: Capture=False, Quiet=True, Distance=2"
      Debug.Log($"<color=white>Move properties: Capture={isCapture}, Quiet={isQuiet}, Distance={distance}</color>");

      // Test promotion utilities
      bool needsPromotion = ChessMove.RequiresPromotion(promoFrom, promoTo, 'P');
      char[] options = ChessMove.GetPromotionOptions(true);
      string pieceName = ChessMove.GetPromotionPieceName('R');

      // Expected output: "Promotion check: Needs=True, Options=4, Queen name=Queen"
      Debug.Log($"<color=white>Promotion check: Needs={needsPromotion}, Options={options.Length}, Queen name={pieceName}</color>");

      // Test equality
      ChessMove move1 = new ChessMove(from, to, 'P');
      ChessMove move2 = new ChessMove(from, to, 'P');
      bool areEqual = move1.Equals(move2);

      // Expected output: "Equality test: True"
      Debug.Log($"<color=white>Equality test: {areEqual}</color>");

      // Create castling move
      v2 kingFrom = new v2(4, 0);
      v2 kingTo = new v2(6, 0);
      v2 rookFrom = new v2(7, 0);
      v2 rookTo = new v2(5, 0);
      ChessMove castling = new ChessMove(kingFrom, kingTo, rookFrom, rookTo, 'K');

      // Expected output: "Castling move: e1g1"
      Debug.Log($"<color=white>Castling move: {castling.ToUCI()}</color>");

      // Run comprehensive tests
      ChessMove.RunAllTests();

      // Expected output: "ChessMove example completed successfully"
      Debug.Log("<color=cyan>ChessMove example completed successfully</color>");
    }

    private void SimpleVerification_ChessMove_Check()
    {
      // After e4, Nf3 manually check the position
      ChessBoard testBoard = new ChessBoard();
      testBoard.MakeMove(ChessMove.FromUCI("e2e4", testBoard));
      Debug.Log($"Position after e2e4: {testBoard.ToFEN()}");

      testBoard.MakeMove(ChessMove.FromPGN("e5", testBoard)); // black turn
      Debug.Log($"Position after e2e4: {testBoard.ToFEN()}");

      testBoard.MakeMove(ChessMove.FromUCI("f1c4", testBoard));
      Debug.Log($"Position after f1c4: {testBoard.ToFEN()}");

      testBoard.MakeMove(ChessMove.FromPGN("Qh4", testBoard)); // black turn
      Debug.Log($"Position after Qh4: {testBoard.ToFEN()}");
      return;
    }
}
```

## Control flow / responsibilities & high-level algorithm summary

Parsing pipeline: UCI/PGN input → coordinate extraction → board validation → caching → move construction with analysis metadata integration.

## Performance, allocations, and hotspots

UCI caching (1000 entries), string operations optimized, minimal GC allocations.

## Threading / async considerations

Static cache thread-safe via Dictionary, no async operations, main-thread only.

## Security / safety / correctness concerns

Potential null reference on invalid board states, unhandled string parsing edge cases.

## Tests, debugging & observability

Built-in comprehensive test suite via `RunAllTests()`, color-coded Unity Debug.Log output, performance timing measurements.

## Cross-file references

Depends on `ChessBoard.cs` for position validation, `SPACE_UTIL.v2` for coordinate system.

## TODO / Known limitations / Suggested improvements

<!-- TODO items from code:
- Enhanced caching strategies with LRU eviction
- Thread-safe parsing for background analysis
- Extended PGN annotation support for variations
- Performance profiling for large game databases
- Memory optimization for mobile Unity targets
(only if I explicitly mentioned in the prompt) -->

## Appendix

Private helper `ParsePGNComponents` handles disambiguation logic, `CleanPGNMove` strips annotations, cached dictionary manages frequent UCI lookups.

## General Note: important behaviors

Major functionalities include PGN/UCI parsing with disambiguation, move validation, promotion handling, analysis integration, and comprehensive equality semantics.

`checksum: a7f2b1c8 (v0.3)`