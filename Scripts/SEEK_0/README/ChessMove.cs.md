# Source: `ChessMove.cs` — Enhanced chess move representation with comprehensive PGN and UCI parsing support

A serializable chess move struct with optimized parsing, validation, and analysis integration for Unity chess applications.

## Short description

Implements a comprehensive chess move system supporting UCI and PGN notation parsing, move validation, promotion handling, and engine analysis integration. Provides performance-optimized parsing with caching, extensive move validation, and detailed move metadata including timing and annotations for chess engine integration.

## Metadata

* **Filename:** `ChessMove.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `using System;`, `using System.Collections.Generic;`, `using System.Text;`, `using UnityEngine;`, `using SPACE_UTIL;`
* **Estimated lines:** 1180
* **Estimated chars:** 47000
* **Public types:** `ChessMove (struct)`, `ChessMove.MoveType (enum)`, `ChessMove.Annotations (static class)`
* **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
* **Dependencies:** `SPACE_UTIL.v2` (SPACE_UTIL is namespace), `ChessBoard.cs`, `ChessRules.cs`

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| v2 | from | `public v2 from` | Source square coordinates | `var f = move.from;` |
| v2 | to | `public v2 to` | Target square coordinates | `var t = move.to;` |
| char | piece | `public char piece` | Moving piece character | `var p = move.piece;` |
| char | capturedPiece | `public char capturedPiece` | Captured piece character | `var c = move.capturedPiece;` |
| ChessMove.MoveType (enum) | moveType | `public ChessMove.MoveType moveType` | Type of move being made | `var mt = move.moveType;` |
| char | promotionPiece | `public char promotionPiece` | Piece to promote to | `var pp = move.promotionPiece;` |
| v2 | rookFrom | `public v2 rookFrom` | Rook source for castling | `var rf = move.rookFrom;` |
| v2 | rookTo | `public v2 rookTo` | Rook target for castling | `var rt = move.rookTo;` |
| float | analysisTime | `public float analysisTime` | Analysis time in milliseconds | `var at = move.analysisTime;` |
| string | annotation | `public string annotation` | Move annotation (+, #, !, ?) | `var ann = move.annotation;` |
| int | engineDepth | `public int engineDepth` | Search depth from engine | `var depth = move.engineDepth;` |
| float | engineEval | `public float engineEval` | Engine evaluation score | `var eval = move.engineEval;` |
| void | ChessMove | `public ChessMove(v2 from, v2 to, char piece, char capturedPiece = '\0')` | Normal move constructor | `var move = new ChessMove(from, to, 'P');` |
| void | ChessMove | `public ChessMove(v2 from, v2 to, char piece, char promotionPiece, char capturedPiece = '\0')` | Promotion move constructor | `var move = new ChessMove(from, to, 'P', 'Q');` |
| void | ChessMove | `public ChessMove(v2 kingFrom, v2 kingTo, v2 rookFrom, v2 rookTo, char piece)` | Castling move constructor | `var move = new ChessMove(kingFrom, kingTo, rookFrom, rookTo, 'K');` |
| ChessMove (struct) | FromPGN | `public static ChessMove FromPGN(string pgnMove, ChessBoard board, List<ChessMove> legalMoves = null)` | Parse move from PGN notation | `var move = ChessMove.FromPGN("e4", board);` |
| ChessMove (struct) | FromUCI | `public static ChessMove FromUCI(string uciMove, ChessBoard board)` | Parse move from UCI notation | `var move = ChessMove.FromUCI("e2e4", board);` |
| string | ToPGN | `public string ToPGN(ChessBoard board, List<ChessMove> legalMoves = null)` | Convert to PGN notation | `string pgn = move.ToPGN(board);` |
| string | ToUCI | `public string ToUCI()` | Convert to UCI notation | `string uci = move.ToUCI();` |
| ChessMove (struct) | WithAnalysisData | `public ChessMove WithAnalysisData(float analysisTimeMs, int depth, float evaluation)` | Set analysis metadata | `var analyzed = move.WithAnalysisData(150f, 12, 0.5f);` |
| ChessMove (struct) | WithAnnotation | `public ChessMove WithAnnotation(string annotation)` | Set move annotation | `var annotated = move.WithAnnotation("!");` |
| string | GetAnalysisSummary | `public string GetAnalysisSummary()` | Get analysis summary string | `string summary = move.GetAnalysisSummary();` |
| bool | IsValid | `public bool IsValid()` | Check if move is valid | `bool valid = move.IsValid();` |
| bool | IsLegal | `public bool IsLegal(ChessBoard board)` | Check if move is legal on board | `bool legal = move.IsLegal(board);` |
| bool | IsCapture | `public bool IsCapture()` | Check if move captures piece | `bool capture = move.IsCapture();` |
| bool | IsQuiet | `public bool IsQuiet()` | Check if move is quiet | `bool quiet = move.IsQuiet();` |
| int | GetDistance | `public int GetDistance()` | Get Manhattan distance | `int dist = move.GetDistance();` |
| bool | RequiresPromotion | `public static bool RequiresPromotion(v2 from, v2 to, char piece)` | Check if move requires promotion | `bool req = ChessMove.RequiresPromotion(from, to, 'P');` |
| bool | IsValidPromotionPiece | `public static bool IsValidPromotionPiece(char piece)` | Check if piece valid for promotion | `bool valid = ChessMove.IsValidPromotionPiece('Q');` |
| char | GetDefaultPromotionPiece | `public static char GetDefaultPromotionPiece(bool isWhite)` | Get default promotion piece | `char def = ChessMove.GetDefaultPromotionPiece(true);` |
| char[] | GetPromotionOptions | `public static char[] GetPromotionOptions(bool isWhite)` | Get available promotion pieces | `char[] options = ChessMove.GetPromotionOptions(true);` |
| string | GetPromotionPieceName | `public static string GetPromotionPieceName(char piece)` | Get promotion piece name | `string name = ChessMove.GetPromotionPieceName('Q');` |
| ChessMove (struct) | CreatePromotionMove | `public static ChessMove CreatePromotionMove(v2 from, v2 to, char movingPiece, char promotionType, char capturedPiece = '\0')` | Create promotion move safely | `var prom = ChessMove.CreatePromotionMove(from, to, 'P', 'Q');` |
| ChessMove (struct) | Invalid | `public static ChessMove Invalid()` | Create invalid move marker | `var invalid = ChessMove.Invalid();` |
| bool | Equals | `public bool Equals(ChessMove other)` | Compare moves for equality | `bool eq = move1.Equals(move2);` |
| void | TestUCIPromotionParsing | `public static void TestUCIPromotionParsing()` | Test UCI promotion parsing | `ChessMove.TestUCIPromotionParsing();` |
| void | TestMoveCreation | `public static void TestMoveCreation()` | Test move creation validation | `ChessMove.TestMoveCreation();` |
| void | TestPGNParsing | `public static void TestPGNParsing()` | Test PGN parsing functionality | `ChessMove.TestPGNParsing();` |
| void | TestPerformanceOptimizations | `public static void TestPerformanceOptimizations()` | Test performance optimizations | `ChessMove.TestPerformanceOptimizations();` |
| void | RunAllTests | `public static void RunAllTests()` | Run complete test suite | `ChessMove.RunAllTests();` |

## Important types — details

### `ChessMove`
* **Kind:** struct (implements IEquatable<ChessMove>)
* **Responsibility:** Represents a chess move with comprehensive parsing and validation support
* **Constructor(s):** 
  - `ChessMove(v2 from, v2 to, char piece, char capturedPiece = '\0')` — Normal move
  - `ChessMove(v2 from, v2 to, char piece, char promotionPiece, char capturedPiece = '\0')` — Promotion move
  - `ChessMove(v2 kingFrom, v2 kingTo, v2 rookFrom, v2 rookTo, char piece)` — Castling move
* **Public properties / fields:**
  - `from — v2 — Source square coordinates`
  - `to — v2 — Target square coordinates` 
  - `piece — char — Moving piece character`
  - `capturedPiece — char — Captured piece character`
  - `moveType — ChessMove.MoveType — Type of move (Normal, Castling, EnPassant, Promotion, CastlingPromotion)`
  - `promotionPiece — char — Piece to promote to`
  - `rookFrom — v2 — Rook source square for castling`
  - `rookTo — v2 — Rook target square for castling`
  - `analysisTime — float — Analysis time in milliseconds`
  - `annotation — string — Move annotation (+, #, !, ?)`
  - `engineDepth — int — Search depth if from engine`
  - `engineEval — float — Engine evaluation of position`
* **Public methods:**
  - **Signature:** `public static ChessMove FromPGN(string pgnMove, ChessBoard board, List<ChessMove> legalMoves = null)`
    - **Description:** Parse move from PGN (Standard Algebraic Notation) with comprehensive disambiguation
    - **Parameters:** pgnMove : string — PGN move string, board : ChessBoard — Current board state, legalMoves : List<ChessMove> — Optional legal moves list
    - **Returns:** ChessMove — Parsed move: `var move = ChessMove.FromPGN("e4", board);`
    - **Side effects / state changes:** Uses static move cache for performance
    - **Complexity / performance:** O(n) where n is number of legal moves for disambiguation
  - **Signature:** `public static ChessMove FromUCI(string uciMove, ChessBoard board)`
    - **Description:** Parse move from UCI format with performance optimizations and caching
    - **Parameters:** uciMove : string — UCI move string, board : ChessBoard — Current board state
    - **Returns:** ChessMove — Parsed move: `var move = ChessMove.FromUCI("e2e4", board);`
    - **Side effects / state changes:** Updates static UCI cache for repeated calls
    - **Complexity / performance:** O(1) with cache hit, O(1) without cache
  - **Signature:** `public string ToPGN(ChessBoard board, List<ChessMove> legalMoves = null)`
    - **Description:** Convert to PGN notation with proper disambiguation
    - **Parameters:** board : ChessBoard — Current board state, legalMoves : List<ChessMove> — Optional legal moves
    - **Returns:** string — PGN notation: `string pgn = move.ToPGN(board);`
  - **Signature:** `public string ToUCI()`
    - **Description:** Convert to UCI notation format
    - **Returns:** string — UCI notation: `string uci = move.ToUCI();`
  - **Signature:** `public bool IsValid()`
    - **Description:** Check if move coordinates and piece are valid
    - **Returns:** bool — Validity status: `bool valid = move.IsValid();`
  - **Signature:** `public bool IsLegal(ChessBoard board)`
    - **Description:** Validate move is legal on given board
    - **Parameters:** board : ChessBoard — Board to validate against
    - **Returns:** bool — Legality status: `bool legal = move.IsLegal(board);`
    - **Complexity / performance:** O(n) where n is number of legal moves

### `ChessMove.MoveType`
* **Kind:** enum
* **Responsibility:** Categorize different types of chess moves
* **Values:**
  - `Normal` — Standard piece move
  - `Castling` — King and rook castling
  - `EnPassant` — En passant capture
  - `Promotion` — Pawn promotion
  - `CastlingPromotion` — Combined castling and promotion

### `ChessMove.Annotations`
* **Kind:** static class
* **Responsibility:** Standard chess move annotations constants
* **Public fields:**
  - `Check — string — "+" check annotation`
  - `Checkmate — string — "#" checkmate annotation`
  - `Brilliant — string — "!!" brilliant move`
  - `Good — string — "!" good move`
  - `Interesting — string — "!?" interesting move`
  - `Dubious — string — "?!" dubious move`
  - `Mistake — string — "?" mistake`
  - `Blunder — string — "??" blunder`

## Example usage
```csharp
// using GPTDeepResearch;
// using SPACE_UTIL;

// Create a normal move
var move = new ChessMove(new v2(4, 1), new v2(4, 3), 'P');

// Parse from PGN
var pgnMove = ChessMove.FromPGN("e4", board);

// Parse from UCI with promotion
var uciMove = ChessMove.FromUCI("e7e8q", board);

// Add analysis data
var analyzed = move.WithAnalysisData(150.0f, 12, 0.75f)
                   .WithAnnotation("!");
```

## Control flow / responsibilities & high-level algorithm summary
Handles chess move parsing, validation, and conversion between notations. Uses cached parsing for performance with disambiguation logic for ambiguous PGN moves.

## Performance, allocations, and hotspots
Static caching reduces GC pressure; string operations optimized with StringBuilder; pre-allocated character arrays for parsing.

## Security / safety / correctness concerns
Extensive validation prevents invalid coordinates; null checks for board state; exception handling for malformed input.

## Tests, debugging & observability
Built-in test suite with `RunAllTests()`, comprehensive logging, performance benchmarks, and validation of parsing accuracy.

## Cross-file references
Depends on `ChessBoard.cs` for board state validation, `SPACE_UTIL.v2` for coordinates, and integrates with `ChessRules.cs` for legal move validation.

<!-- TODO improvements: Enhanced caching strategy, async parsing for large move lists, integration with transposition tables, multi-threaded move validation, memory pool for move objects, SIMD optimizations for coordinate calculations (only if I explicitly mentioned in the prompt) -->

## Appendix
Key private helper methods include `CleanPGNMove()`, `ParsePGNComponents()`, `DisambiguateMove()`, and `RequiresPromotion()` for move parsing and validation logic.

## General Note: important behaviors
Major functionality includes PGN/UCI parsing with disambiguation, promotion handling, move validation, performance caching, and comprehensive testing suite for chess engine integration.

`checksum: a7f8e2b1`