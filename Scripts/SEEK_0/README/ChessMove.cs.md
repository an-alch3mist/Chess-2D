# Source: `ChessMove.cs` — Comprehensive chess move representation with UCI, PGN parsing and analysis support

## Short description (2–4 sentences)
Implements a chess move data structure with comprehensive parsing support for UCI notation, PGN (Standard Algebraic Notation), and move annotations. Provides performance-optimized parsing with caching, validation, and analysis integration for chess engines. Enhanced for Unity 2020.3 compatibility with robust error handling and memory management.

## Metadata

* **Filename:** `ChessMove.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `System, System.Collections.Generic, System.Text, UnityEngine, SPACE_UTIL`
* **Estimated lines:** 1200
* **Estimated chars:** 45000
* **Public types:** `ChessMove (struct)`
* **Unity version / Target framework:** `Unity 2020.3 / .NET Standard 2.0`
* **Dependencies:** `v2` (SPACE_UTIL namespace), `ChessBoard.cs`, `UnityEngine.Debug`

## Public API summary (table)

| Type | Member | Signature | Short purpose | OneLiner Call |
|------|--------|-----------|---------------|---------------|
| v2 | from | `public v2 from` | Source square coordinates | `var from = move.from;` |
| v2 | to | `public v2 to` | Destination square coordinates | `var to = move.to;` |
| char | piece | `public char piece` | Moving piece character | `var piece = move.piece;` |
| char | capturedPiece | `public char capturedPiece` | Captured piece character | `var captured = move.capturedPiece;` |
| ChessMove.MoveType (enum) | moveType | `public ChessMove.MoveType moveType` | Type of chess move | `var type = move.moveType;` |
| char | promotionPiece | `public char promotionPiece` | Promoted piece for pawn promotion | `var promo = move.promotionPiece;` |
| v2 | rookFrom | `public v2 rookFrom` | Rook source for castling | `var rookFrom = move.rookFrom;` |
| v2 | rookTo | `public v2 rookTo` | Rook destination for castling | `var rookTo = move.rookTo;` |
| float | analysisTime | `public float analysisTime` | Engine analysis time in ms | `var time = move.analysisTime;` |
| string | annotation | `public string annotation` | Move annotation (+, #, !, ?) | `var annotation = move.annotation;` |
| int | engineDepth | `public int engineDepth` | Engine search depth | `var depth = move.engineDepth;` |
| float | engineEval | `public float engineEval` | Engine position evaluation | `var eval = move.engineEval;` |
| void | ChessMove (constructor) | `public ChessMove(v2 from, v2 to, char piece, char capturedPiece = '\0')` | Create normal move | `var move = new ChessMove(from, to, 'P');` |
| void | ChessMove (constructor) | `public ChessMove(v2 from, v2 to, char piece, char promotionPiece, char capturedPiece = '\0')` | Create promotion move | `var move = new ChessMove(from, to, 'P', 'Q');` |
| void | ChessMove (constructor) | `public ChessMove(v2 kingFrom, v2 kingTo, v2 rookFrom, v2 rookTo, char piece)` | Create castling move | `var move = new ChessMove(kingFrom, kingTo, rookFrom, rookTo, 'K');` |
| ChessMove (struct) | FromPGN | `public static ChessMove FromPGN(string pgnMove, ChessBoard board, List<ChessMove> legalMoves = null)` | Parse move from PGN notation | `var move = ChessMove.FromPGN("e4", board);` |
| ChessMove (struct) | FromUCI | `public static ChessMove FromUCI(string uciMove, ChessBoard board)` | Parse move from UCI notation | `var move = ChessMove.FromUCI("e2e4", board);` |
| ChessMove (struct) | CreatePromotionMove | `public static ChessMove CreatePromotionMove(v2 from, v2 to, char movingPiece, char promotionType, char capturedPiece = '\0')` | Create validated promotion move | `var move = ChessMove.CreatePromotionMove(from, to, 'P', 'Q');` |
| ChessMove (struct) | Invalid | `public static ChessMove Invalid()` | Create invalid move constant | `var invalid = ChessMove.Invalid();` |
| string | ToPGN | `public string ToPGN(ChessBoard board, List<ChessMove> legalMoves = null)` | Convert to PGN notation | `string pgn = move.ToPGN(board);` |
| string | ToUCI | `public string ToUCI()` | Convert to UCI notation | `string uci = move.ToUCI();` |
| ChessMove (struct) | WithAnalysisData | `public ChessMove WithAnalysisData(float analysisTimeMs, int depth, float evaluation)` | Set engine analysis data | `var analyzed = move.WithAnalysisData(1500f, 12, 0.25f);` |
| ChessMove (struct) | WithAnnotation | `public ChessMove WithAnnotation(string annotation)` | Set move annotation | `var annotated = move.WithAnnotation("!");` |
| string | GetAnalysisSummary | `public string GetAnalysisSummary()` | Get formatted analysis summary | `string summary = move.GetAnalysisSummary();` |
| bool | IsValid | `public bool IsValid()` | Check if move is valid | `bool valid = move.IsValid();` |
| bool | IsLegal | `public bool IsLegal(ChessBoard board)` | Validate move is legal on board | `bool legal = move.IsLegal(board);` |
| bool | IsCapture | `public bool IsCapture()` | Check if move captures piece | `bool captures = move.IsCapture();` |
| bool | IsQuiet | `public bool IsQuiet()` | Check if move is quiet (no capture) | `bool quiet = move.IsQuiet();` |
| int | GetDistance | `public int GetDistance()` | Get Manhattan distance | `int dist = move.GetDistance();` |
| bool | RequiresPromotion | `public static bool RequiresPromotion(v2 from, v2 to, char piece)` | Check if move requires promotion | `bool needsPromo = ChessMove.RequiresPromotion(from, to, 'P');` |
| bool | IsValidPromotionPiece | `public static bool IsValidPromotionPiece(char piece)` | Validate promotion piece | `bool valid = ChessMove.IsValidPromotionPiece('Q');` |
| char | GetDefaultPromotionPiece | `public static char GetDefaultPromotionPiece(bool isWhite)` | Get default promotion piece | `char piece = ChessMove.GetDefaultPromotionPiece(true);` |
| char[] | GetPromotionOptions | `public static char[] GetPromotionOptions(bool isWhite)` | Get valid promotion pieces | `char[] options = ChessMove.GetPromotionOptions(true);` |
| string | GetPromotionPieceName | `public static string GetPromotionPieceName(char piece)` | Get promotion piece display name | `string name = ChessMove.GetPromotionPieceName('Q');` |
| bool | Equals | `public bool Equals(ChessMove other)` | Compare moves for equality | `bool equal = move1.Equals(move2);` |
| int | GetHashCode | `public override int GetHashCode()` | Get move hash code | `int hash = move.GetHashCode();` |
| string | ToString | `public override string ToString()` | Get formatted move string | `string str = move.ToString();` |
| void | RunAllTests | `public static void RunAllTests()` | Run comprehensive test suite | `ChessMove.RunAllTests();` |

## Important types — details

### `ChessMove` (struct)
* **Kind:** struct implementing `IEquatable<ChessMove>`
* **Responsibility:** Represents a chess move with comprehensive parsing, validation, and analysis support
* **Constructor(s):** 
  - `ChessMove(v2 from, v2 to, char piece, char capturedPiece = '\0')` — normal move
  - `ChessMove(v2 from, v2 to, char piece, char promotionPiece, char capturedPiece = '\0')` — promotion move  
  - `ChessMove(v2 kingFrom, v2 kingTo, v2 rookFrom, v2 rookTo, char piece)` — castling move
* **Public properties / fields:**
  - `from — v2 — source square coordinates`
  - `to — v2 — destination square coordinates` 
  - `piece — char — moving piece character`
  - `capturedPiece — char — captured piece character`
  - `moveType — ChessMove.MoveType — type of chess move`
  - `promotionPiece — char — promoted piece for pawns`
  - `rookFrom — v2 — rook source for castling`
  - `rookTo — v2 — rook destination for castling`
  - `analysisTime — float — engine analysis time (ms)`
  - `annotation — string — move annotation (+, #, !, ?)`
  - `engineDepth — int — engine search depth`
  - `engineEval — float — engine evaluation`

* **Public methods:**
  * **Signature:** `public static ChessMove FromPGN(string pgnMove, ChessBoard board, List<ChessMove> legalMoves = null)`
    * **Description:** Parse move from PGN (Standard Algebraic Notation) with disambiguation support
    * **Parameters:** 
      - `pgnMove : string — PGN notation like "e4", "Nf3", "O-O"`
      - `board : ChessBoard — current board position`
      - `legalMoves : List<ChessMove> — optional legal moves list for performance`
    * **Returns:** `ChessMove — parsed move or invalid move` 
      - `ChessMove move = ChessMove.FromPGN("e4", board);`
    * **Throws:** None (returns invalid move on error)
    * **Side effects / state changes:** Updates internal UCI cache
    * **Complexity / performance:** O(n) where n is legal moves count
    * **Notes:** Supports castling notation, annotations, and complex disambiguation
    
  * **Signature:** `public static ChessMove FromUCI(string uciMove, ChessBoard board)`
    * **Description:** Parse move from UCI notation with performance caching
    * **Parameters:**
      - `uciMove : string — UCI notation like "e2e4", "e7e8q"`
      - `board : ChessBoard — current board position`
    * **Returns:** `ChessMove — parsed move or invalid move`
      - `ChessMove move = ChessMove.FromUCI("e2e4", board);`
    * **Throws:** None (returns invalid move on error)
    * **Side effects / state changes:** Populates internal cache up to MAX_CACHE_SIZE
    * **Complexity / performance:** O(1) with cache hit, O(1) parse time
    * **Notes:** Cache validated against current board state, optimized string operations
    
  * **Signature:** `public string ToPGN(ChessBoard board, List<ChessMove> legalMoves = null)`
    * **Description:** Convert move to PGN notation with proper disambiguation
    * **Parameters:**
      - `board : ChessBoard — current board position`
      - `legalMoves : List<ChessMove> — optional legal moves for disambiguation`
    * **Returns:** `string — PGN notation with annotations`
      - `string pgn = move.ToPGN(board);`
    * **Throws:** None (returns empty string for invalid moves)
    * **Side effects / state changes:** None
    * **Complexity / performance:** O(n) for disambiguation analysis
    * **Notes:** Handles castling, captures, promotions, and annotations
    
  * **Signature:** `public string ToUCI()`
    * **Description:** Convert move to UCI notation
    * **Parameters:** None
    * **Returns:** `string — UCI notation like "e2e4" or "e7e8q"`
      - `string uci = move.ToUCI();`
    * **Throws:** None (returns empty string for invalid moves)
    * **Side effects / state changes:** None
    * **Complexity / performance:** O(1) with StringBuilder optimization
    * **Notes:** Handles promotion suffix automatically
    
  * **Signature:** `public bool IsValid()`
    * **Description:** Validate move coordinates and piece requirements
    * **Parameters:** None
    * **Returns:** `bool — true if coordinates valid and piece not null`
      - `bool valid = move.IsValid();`
    * **Throws:** None
    * **Side effects / state changes:** None
    * **Complexity / performance:** O(1)
    * **Notes:** Checks coordinate bounds and basic move requirements
    
  * **Signature:** `public ChessMove WithAnalysisData(float analysisTimeMs, int depth, float evaluation)`
    * **Description:** Create copy with engine analysis data attached
    * **Parameters:**
      - `analysisTimeMs : float — time taken for analysis`
      - `depth : int — search depth used`
      - `evaluation : float — position evaluation`
    * **Returns:** `ChessMove — copy with analysis data`
      - `ChessMove analyzed = move.WithAnalysisData(1500f, 12, 0.25f);`
    * **Throws:** None
    * **Side effects / state changes:** None (returns new instance)
    * **Complexity / performance:** O(1) struct copy
    * **Notes:** Immutable operation, preserves original move

### `ChessMove.MoveType` (enum)
* **Kind:** enum nested in ChessMove
* **Responsibility:** Categorizes different types of chess moves
* **Values:**
  - `Normal — standard move`
  - `Castling — king and rook castling`
  - `EnPassant — en passant capture`
  - `Promotion — pawn promotion`
  - `CastlingPromotion — rare edge case`

### `ChessMove.Annotations` (static class)  
* **Kind:** static class nested in ChessMove
* **Responsibility:** Provides standard chess move annotation constants
* **Public fields:**
  - `Check — "+" constant`
  - `Checkmate — "#" constant`
  - `Brilliant — "!!" constant`
  - `Good — "!" constant`
  - `Interesting — "!?" constant`
  - `Dubious — "?!" constant`
  - `Mistake — "?" constant`
  - `Blunder — "??" constant`

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
        
        // Parse moves from different notations
        ChessMove pgnMove = ChessMove.FromPGN("e4", board);
        ChessMove uciMove = ChessMove.FromUCI("e2e4", board);
        
        // Expected output: "<color=green>PGN parsed: e4 -> e2e4</color>"
        Debug.Log($"<color=green>PGN parsed: e4 -> {pgnMove.ToUCI()}</color>");
        
        // Expected output: "<color=green>UCI parsed: e2e4 -> valid</color>"  
        Debug.Log($"<color=green>UCI parsed: e2e4 -> {uciMove.IsValid()}</color>");
        
        // Create promotion move
        v2 pawnFrom = new v2(4, 6); // e7
        v2 pawnTo = new v2(4, 7);   // e8
        ChessMove promotion = ChessMove.CreatePromotionMove(pawnFrom, pawnTo, 'P', 'Q');
        
        // Expected output: "<color=green>Promotion: e7e8q</color>"
        Debug.Log($"<color=green>Promotion: {promotion.ToUCI()}</color>");
        
        // Add analysis data and annotations
        ChessMove analyzed = promotion.WithAnalysisData(1500f, 12, 0.75f)
                                    .WithAnnotation(ChessMove.Annotations.Good);
        
        // Expected output: "<color=green>Analysis: Eval: +0.75, Depth: 12, Time: 1500ms</color>"
        Debug.Log($"<color=green>Analysis: {analyzed.GetAnalysisSummary()}</color>");
        
        // Test move properties
        bool isCapture = analyzed.IsCapture();
        bool isQuiet = analyzed.IsQuiet();
        int distance = analyzed.GetDistance();
        
        // Expected output: "<color=green>Move properties - Capture: False, Quiet: True, Distance: 1</color>"
        Debug.Log($"<color=green>Move properties - Capture: {isCapture}, Quiet: {isQuiet}, Distance: {distance}</color>");
        
        // Test promotion utilities
        bool needsPromotion = ChessMove.RequiresPromotion(pawnFrom, pawnTo, 'P');
        char[] whiteOptions = ChessMove.GetPromotionOptions(true);
        string queenName = ChessMove.GetPromotionPieceName('Q');
        
        // Expected output: "<color=green>Promotion check: True, Options: QRBN, Queen name: Queen</color>"
        Debug.Log($"<color=green>Promotion check: {needsPromotion}, Options: {new string(whiteOptions)}, Queen name: {queenName}</color>");
        
        // Test castling move creation
        v2 kingFrom = new v2(4, 0);
        v2 kingTo = new v2(6, 0);
        v2 rookFrom = new v2(7, 0);
        v2 rookTo = new v2(5, 0);
        ChessMove castling = new ChessMove(kingFrom, kingTo, rookFrom, rookTo, 'K');
        
        // Expected output: "<color=green>Castling: e1g1 (castling)</color>"
        Debug.Log($"<color=green>Castling: {castling.ToString()}</color>");
        
        // Test move comparison
        ChessMove move1 = new ChessMove(new v2(0, 1), new v2(0, 3), 'P');
        ChessMove move2 = new ChessMove(new v2(0, 1), new v2(0, 3), 'P');
        bool areEqual = move1.Equals(move2);
        
        // Expected output: "<color=green>Move equality: True</color>"
        Debug.Log($"<color=green>Move equality: {areEqual}</color>");
        
        // Test invalid move handling
        ChessMove invalid = ChessMove.Invalid();
        bool isValid = invalid.IsValid();
        
        // Expected output: "<color=green>Invalid move check: False</color>"
        Debug.Log($"<color=green>Invalid move check: {isValid}</color>");
        
        // Run comprehensive test suite
        ChessMove.RunAllTests();
        // Expected output: Various colored test results showing pass/fail status
    }
}
```

## Control flow / responsibilities & high-level algorithm summary / Side effects and I/O
Primary responsibilities: UCI/PGN parsing with caching, move validation, format conversion, and analysis integration. Parsing uses optimized string operations with disambiguation logic for PGN. Maintains UCI cache with board validation. No direct I/O, uses Unity Debug.Log for testing output.

## Performance, allocations, and hotspots / Threading / async considerations  
UCI cache reduces parsing overhead. String operations optimized with StringBuilder. No threading concerns, main-thread only.

## Security / safety / correctness concerns
Potential null reference exceptions on invalid board states. No unsafe operations present.

## Tests, debugging & observability
Comprehensive test suite via `RunAllTests()` with colored Debug.Log output. Tests cover UCI parsing, PGN parsing, move creation, performance optimization, and edge cases.

## Cross-file references  
Depends on `ChessBoard.cs` for position validation and legal move generation. Uses `SPACE_UTIL.v2` for coordinate representation.

## TODO / Known limitations / Suggested improvements
<!-- No explicit TODO comments found in source code. Potential improvements: (1) Add async UCI parsing for large move sets, (2) Implement move tree serialization, (3) Add support for Chess960 castling notation, (4) Consider memory pool for frequent move creation, (5) Add FEN integration for position-dependent validation. (only if I explicitly mentioned in the prompt) -->

## Appendix
Key private helpers: `ParsePGNComponents()`, `DisambiguateMove()`, `CleanPGNMove()` handle complex PGN parsing logic. Caching system uses Dictionary with size limits and validation.

## General Note: important behaviors
Major functionalities: Pawn promotion with comprehensive piece validation, UCI/PGN format conversion with caching optimization, move disambiguation for complex positions, engine analysis integration with timing data.

`checksum: a7f3b2c8 (v0.3)`