# Source: `ChessMove.cs` — Enhanced chess move representation with comprehensive PGN/UCI parsing and analysis integration

* Comprehensive chess move data structure supporting UCI, PGN notation parsing with performance optimizations and engine analysis integration.

## Short description (2–4 sentences)

Implements a robust chess move representation supporting multiple notation formats (UCI, PGN), comprehensive move validation, and engine analysis integration. Handles all chess move types including normal moves, castling, en passant, and promotions with disambiguation logic. Enhanced with performance caching, timing analysis, and annotation support for chess engine integration.

## Metadata

* **Filename:** `ChessMove.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependent namespace:** `System, System.Collections.Generic, System.Text, UnityEngine, SPACE_UTIL`
* **Estimated lines:** 850
* **Estimated chars:** 32,000
* **Public types:** `ChessMove (struct)`
* **Unity version / Target framework:** Unity 2020.3 / .NET Standard 2.0
* **Dependencies:** `v2 (SPACE_UTIL namespace)`, `ChessBoard.cs`, `UnityEngine`

## Public API summary (table)

| **Type** | **Member** | **Signature** | **Short purpose** | **OneLiner Call** |
|----------|------------|---------------|-------------------|-------------------|
| v2 | from | `public v2 from` | Source square coordinates | `var f = move.from;` |
| v2 | to | `public v2 to` | Destination square coordinates | `var t = move.to;` |
| char | piece | `public char piece` | Moving piece character | `var p = move.piece;` |
| char | capturedPiece | `public char capturedPiece` | Captured piece character | `var c = move.capturedPiece;` |
| ChessMove.MoveType (enum) | moveType | `public ChessMove.MoveType moveType` | Special move type classification | `var mt = move.moveType;` |
| char | promotionPiece | `public char promotionPiece` | Promotion target piece | `var pp = move.promotionPiece;` |
| v2 | rookFrom | `public v2 rookFrom` | Rook source for castling | `var rf = move.rookFrom;` |
| v2 | rookTo | `public v2 rookTo` | Rook destination for castling | `var rt = move.rookTo;` |
| float | analysisTime | `public float analysisTime` | Analysis duration in milliseconds | `var at = move.analysisTime;` |
| string | annotation | `public string annotation` | Move annotation symbols | `var a = move.annotation;` |
| int | engineDepth | `public int engineDepth` | Engine search depth | `var ed = move.engineDepth;` |
| float | engineEval | `public float engineEval` | Engine position evaluation | `var ee = move.engineEval;` |
| void | ChessMove | `public ChessMove(v2 from, v2 to, char piece, char capturedPiece = '\0')` | Normal move constructor | `var move = new ChessMove(from, to, 'P');` |
| void | ChessMove | `public ChessMove(v2 from, v2 to, char piece, char promotionPiece, char capturedPiece = '\0')` | Promotion move constructor | `var move = new ChessMove(from, to, 'P', 'Q');` |
| void | ChessMove | `public ChessMove(v2 kingFrom, v2 kingTo, v2 rookFrom, v2 rookTo, char piece)` | Castling move constructor | `var move = new ChessMove(e1, g1, h1, f1, 'K');` |
| ChessMove (struct) | FromPGN | `public static ChessMove FromPGN(string pgnMove, ChessBoard board, List<ChessMove> legalMoves = null)` | Parse PGN notation to move | `ChessMove move = ChessMove.FromPGN("e4", board);` |
| ChessMove (struct) | FromUCI | `public static ChessMove FromUCI(string uciMove, ChessBoard board)` | Parse UCI notation to move | `ChessMove move = ChessMove.FromUCI("e2e4", board);` |
| string | ToPGN | `public string ToPGN(ChessBoard board, List<ChessMove> legalMoves = null)` | Convert to PGN notation | `string pgn = move.ToPGN(board);` |
| string | ToUCI | `public string ToUCI()` | Convert to UCI notation | `string uci = move.ToUCI();` |
| ChessMove (struct) | WithAnalysisData | `public ChessMove WithAnalysisData(float analysisTimeMs, int depth, float evaluation)` | Add analysis metadata | `ChessMove analyzed = move.WithAnalysisData(100f, 8, 0.5f);` |
| ChessMove (struct) | WithAnnotation | `public ChessMove WithAnnotation(string annotation)` | Add move annotation | `ChessMove annotated = move.WithAnnotation("!");` |
| string | GetAnalysisSummary | `public string GetAnalysisSummary()` | Get analysis summary text | `string summary = move.GetAnalysisSummary();` |
| bool | IsValid | `public bool IsValid()` | Check move validity | `bool valid = move.IsValid();` |
| bool | IsLegal | `public bool IsLegal(ChessBoard board)` | Check move legality on board | `bool legal = move.IsLegal(board);` |
| bool | IsCapture | `public bool IsCapture()` | Check if move captures piece | `bool capture = move.IsCapture();` |
| bool | IsQuiet | `public bool IsQuiet()` | Check if move is quiet | `bool quiet = move.IsQuiet();` |
| int | GetDistance | `public int GetDistance()` | Get Manhattan distance | `int dist = move.GetDistance();` |
| bool | RequiresPromotion | `public static bool RequiresPromotion(v2 from, v2 to, char piece)` | Check if move requires promotion | `bool promo = ChessMove.RequiresPromotion(from, to, 'P');` |
| bool | IsValidPromotionPiece | `public static bool IsValidPromotionPiece(char piece)` | Validate promotion piece | `bool valid = ChessMove.IsValidPromotionPiece('Q');` |
| char | GetDefaultPromotionPiece | `public static char GetDefaultPromotionPiece(bool isWhite)` | Get default promotion piece | `char piece = ChessMove.GetDefaultPromotionPiece(true);` |
| char[] | GetPromotionOptions | `public static char[] GetPromotionOptions(bool isWhite)` | Get promotion piece options | `char[] options = ChessMove.GetPromotionOptions(true);` |
| string | GetPromotionPieceName | `public static string GetPromotionPieceName(char piece)` | Get promotion piece name | `string name = ChessMove.GetPromotionPieceName('Q');` |
| ChessMove (struct) | CreatePromotionMove | `public static ChessMove CreatePromotionMove(v2 from, v2 to, char movingPiece, char promotionType, char capturedPiece = '\0')` | Create validated promotion move | `ChessMove promo = ChessMove.CreatePromotionMove(from, to, 'P', 'Q');` |
| ChessMove (struct) | Invalid | `public static ChessMove Invalid()` | Create invalid move instance | `ChessMove invalid = ChessMove.Invalid();` |
| bool | Equals | `public bool Equals(ChessMove other)` | Compare moves for equality | `bool equal = move1.Equals(move2);` |
| void | RunAllTests | `public static void RunAllTests()` | Execute comprehensive test suite | `ChessMove.RunAllTests();` |

## Important types — details

### `ChessMove` (struct)
* **Kind:** struct implementing IEquatable<ChessMove>
* **Responsibility:** Comprehensive chess move representation with parsing, validation, and analysis support
* **Constructor(s):** 
  - `ChessMove(v2 from, v2 to, char piece, char capturedPiece = '\0')` - Normal moves
  - `ChessMove(v2 from, v2 to, char piece, char promotionPiece, char capturedPiece = '\0')` - Promotion moves
  - `ChessMove(v2 kingFrom, v2 kingTo, v2 rookFrom, v2 rookTo, char piece)` - Castling moves
* **Public properties / fields:**
  * `from` — v2 — Source square coordinates (get/set)
  * `to` — v2 — Destination square coordinates (get/set)
  * `piece` — char — Moving piece character (get/set)
  * `capturedPiece` — char — Captured piece character (get/set)
  * `moveType` — ChessMove.MoveType — Move classification (get/set)
  * `promotionPiece` — char — Promotion target piece (get/set)
  * `rookFrom` — v2 — Rook source position for castling (get/set)
  * `rookTo` — v2 — Rook destination for castling (get/set)
  * `analysisTime` — float — Analysis duration in milliseconds (get/set)
  * `annotation` — string — Move annotation symbols (+, #, !, ?) (get/set)
  * `engineDepth` — int — Engine search depth (get/set)
  * `engineEval` — float — Engine position evaluation (get/set)

* **Public methods:**

  * **Signature:** `public static ChessMove FromPGN(string pgnMove, ChessBoard board, List<ChessMove> legalMoves = null)`
    * **Description:** Parse Standard Algebraic Notation to ChessMove with disambiguation
    * **Parameters:** 
      - pgnMove : string — PGN notation (e.g., "Nf3", "O-O", "exd5")
      - board : ChessBoard — Current board state for validation
      - legalMoves : List<ChessMove> — Pre-calculated legal moves for performance
    * **Returns:** ChessMove — `ChessMove move = ChessMove.FromPGN("e4", board);`
    * **Side effects / state changes:** Updates internal move cache for performance
    * **Notes:** Handles castling, captures, promotions, check/checkmate annotations

  * **Signature:** `public static ChessMove FromUCI(string uciMove, ChessBoard board)`
    * **Description:** Parse Universal Chess Interface notation to ChessMove with caching
    * **Parameters:**
      - uciMove : string — UCI notation (e.g., "e2e4", "e7e8q")
      - board : ChessBoard — Current board state for piece validation
    * **Returns:** ChessMove — `ChessMove move = ChessMove.FromUCI("e2e4", board);`
    * **Side effects / state changes:** Populates move cache for performance optimization
    * **Complexity / performance:** O(1) with cache hit, includes GC pressure reduction
    * **Notes:** Auto-detects promotion moves, validates coordinates

  * **Signature:** `public string ToPGN(ChessBoard board, List<ChessMove> legalMoves = null)`
    * **Description:** Convert move to Standard Algebraic Notation with proper disambiguation
    * **Parameters:**
      - board : ChessBoard — Current board state for disambiguation
      - legalMoves : List<ChessMove> — Legal moves for disambiguation context
    * **Returns:** string — `string pgn = move.ToPGN(board);`
    * **Notes:** Handles file/rank disambiguation, preserves annotations

  * **Signature:** `public string ToUCI()`
    * **Description:** Convert move to Universal Chess Interface notation
    * **Returns:** string — `string uci = move.ToUCI();`
    * **Complexity / performance:** O(1) with string builder optimization
    * **Notes:** Includes promotion piece suffix, handles castling as king moves

  * **Signature:** `public ChessMove WithAnalysisData(float analysisTimeMs, int depth, float evaluation)`
    * **Description:** Create copy with engine analysis metadata
    * **Parameters:**
      - analysisTimeMs : float — Time taken for analysis in milliseconds
      - depth : int — Search depth used by engine
      - evaluation : float — Position evaluation after move
    * **Returns:** ChessMove — `ChessMove analyzed = move.WithAnalysisData(100f, 8, 0.5f);`
    * **Notes:** Immutable operation, returns new instance

  * **Signature:** `public ChessMove WithAnnotation(string annotation)`
    * **Description:** Create copy with move annotation symbols
    * **Parameters:** annotation : string — Annotation symbols (+, #, !, ?, !!, ??)
    * **Returns:** ChessMove — `ChessMove annotated = move.WithAnnotation("!");`
    * **Notes:** Immutable operation, preserves original move data

  * **Signature:** `public string GetAnalysisSummary()`
    * **Description:** Generate formatted analysis summary string
    * **Returns:** string — `string summary = move.GetAnalysisSummary();`
    * **Notes:** Returns empty string if no analysis data present

  * **Signature:** `public bool IsValid()`
    * **Description:** Check basic move validity (coordinates, piece present)
    * **Returns:** bool — `bool valid = move.IsValid();`
    * **Complexity / performance:** O(1) coordinate validation
    * **Notes:** Does not check chess rules, only structural validity

  * **Signature:** `public bool IsLegal(ChessBoard board)`
    * **Description:** Check if move is legal on given board state
    * **Parameters:** board : ChessBoard — Board state for legal move validation
    * **Returns:** bool — `bool legal = move.IsLegal(board);`
    * **Complexity / performance:** O(n) where n is number of legal moves
    * **Notes:** Requires ChessBoard.GetLegalMoves() integration

  * **Signature:** `public bool IsCapture()`
    * **Description:** Check if move captures an opponent piece
    * **Returns:** bool — `bool capture = move.IsCapture();`
    * **Notes:** Includes en passant captures

  * **Signature:** `public bool IsQuiet()`
    * **Description:** Check if move is quiet (no capture, no special move)
    * **Returns:** bool — `bool quiet = move.IsQuiet();`
    * **Notes:** Opposite of tactical moves (captures, promotions)

  * **Signature:** `public int GetDistance()`
    * **Description:** Calculate Manhattan distance of move
    * **Returns:** int — `int dist = move.GetDistance();`
    * **Complexity / performance:** O(1) coordinate arithmetic
    * **Notes:** Sum of horizontal and vertical distance

  * **Signature:** `public static bool RequiresPromotion(v2 from, v2 to, char piece)`
    * **Description:** Check if pawn move requires promotion
    * **Parameters:**
      - from : v2 — Source square coordinates
      - to : v2 — Destination square coordinates  
      - piece : char — Moving piece character
    * **Returns:** bool — `bool promo = ChessMove.RequiresPromotion(from, to, 'P');`
    * **Notes:** Validates pawn reaching promotion rank

  * **Signature:** `public static bool IsValidPromotionPiece(char piece)`
    * **Description:** Validate promotion piece character
    * **Parameters:** piece : char — Promotion piece character (Q, R, B, N)
    * **Returns:** bool — `bool valid = ChessMove.IsValidPromotionPiece('Q');`
    * **Notes:** Accepts both upper and lower case

  * **Signature:** `public static char GetDefaultPromotionPiece(bool isWhite)`
    * **Description:** Get default promotion piece for color
    * **Parameters:** isWhite : bool — True for white pieces
    * **Returns:** char — `char piece = ChessMove.GetDefaultPromotionPiece(true);`
    * **Notes:** Returns 'Q' for white, 'q' for black

  * **Signature:** `public static char[] GetPromotionOptions(bool isWhite)`
    * **Description:** Get array of valid promotion pieces for color
    * **Parameters:** isWhite : bool — True for white pieces
    * **Returns:** char[] — `char[] options = ChessMove.GetPromotionOptions(true);`
    * **Notes:** Returns [Q,R,B,N] for white, [q,r,b,n] for black

  * **Signature:** `public static string GetPromotionPieceName(char piece)`
    * **Description:** Get human-readable name for promotion piece
    * **Parameters:** piece : char — Promotion piece character
    * **Returns:** string — `string name = ChessMove.GetPromotionPieceName('Q');`
    * **Notes:** Returns "Queen", "Rook", "Bishop", "Knight", or "Unknown"

  * **Signature:** `public static ChessMove CreatePromotionMove(v2 from, v2 to, char movingPiece, char promotionType, char capturedPiece = '\0')`
    * **Description:** Create validated promotion move with error checking
    * **Parameters:**
      - from : v2 — Source square coordinates
      - to : v2 — Destination square coordinates
      - movingPiece : char — Pawn character ('P' or 'p')
      - promotionType : char — Target promotion piece
      - capturedPiece : char — Captured piece (optional)
    * **Returns:** ChessMove — `ChessMove promo = ChessMove.CreatePromotionMove(from, to, 'P', 'Q');`
    * **Throws:** Returns Invalid() move if validation fails
    * **Notes:** Validates promotion requirements and piece colors

  * **Signature:** `public static ChessMove Invalid()`
    * **Description:** Create invalid move instance for error handling
    * **Returns:** ChessMove — `ChessMove invalid = ChessMove.Invalid();`
    * **Notes:** Uses (-1,-1) coordinates to indicate invalid state

  * **Signature:** `public bool Equals(ChessMove other)`
    * **Description:** Compare moves for structural equality
    * **Parameters:** other : ChessMove — Move to compare against
    * **Returns:** bool — `bool equal = move1.Equals(move2);`
    * **Notes:** Compares core move data, excludes analysis metadata

  * **Signature:** `public static void RunAllTests()`
    * **Description:** Execute comprehensive test suite for validation
    * **Returns:** void — `ChessMove.RunAllTests();`
    * **Side effects / state changes:** Outputs test results to Debug.Log
    * **Notes:** Tests UCI/PGN parsing, move creation, performance optimizations

### `ChessMove.MoveType` (enum)
* **Kind:** enum nested in ChessMove
* **Responsibility:** Classify special move types for chess rules
* **Values:**
  * `Normal` — Standard piece movement
  * `Castling` — King and rook castling move
  * `EnPassant` — Pawn en passant capture
  * `Promotion` — Pawn promotion to piece
  * `CastlingPromotion` — Reserved for future use

### `ChessMove.Annotations` (static class)
* **Kind:** static class nested in ChessMove  
* **Responsibility:** Provide standard chess move annotation constants
* **Public fields:**
  * `Check` — string — "+" symbol for check
  * `Checkmate` — string — "#" symbol for checkmate
  * `Brilliant` — string — "!!" for brilliant moves
  * `Good` — string — "!" for good moves
  * `Interesting` — string — "!?" for interesting moves
  * `Dubious` — string — "?!" for dubious moves
  * `Mistake` — string — "?" for mistakes
  * `Blunder` — string — "??" for blunders

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
        Debug.Log("<color=green>Chess board initialized</color>");
        
        // Create normal move - pawn from e2 to e4
        v2 from = new v2(4, 1); // e2
        v2 to = new v2(4, 3);   // e4
        ChessMove pawnMove = new ChessMove(from, to, 'P');
        Debug.Log($"<color=green>Normal move created: {pawnMove.ToUCI()}</color>");
        
        // Parse UCI notation
        ChessMove uciMove = ChessMove.FromUCI("e2e4", board);
        if (uciMove.IsValid())
        {
            Debug.Log($"<color=green>UCI parsed successfully: {uciMove.ToUCI()}</color>");
        }
        
        // Parse PGN notation
        var legalMoves = board.GetLegalMoves();
        ChessMove pgnMove = ChessMove.FromPGN("e4", board, legalMoves);
        if (pgnMove.IsValid())
        {
            Debug.Log($"<color=green>PGN parsed successfully: {pgnMove.ToPGN(board)}</color>");
        }
        
        // Create promotion move
        v2 promFrom = new v2(4, 6); // e7
        v2 promTo = new v2(4, 7);   // e8
        ChessMove promotion = ChessMove.CreatePromotionMove(promFrom, promTo, 'P', 'Q');
        if (promotion.IsValid())
        {
            Debug.Log($"<color=green>Promotion move: {promotion.ToUCI()}</color>");
        }
        
        // Add analysis data
        ChessMove analyzed = pawnMove.WithAnalysisData(150.0f, 12, 0.3f);
        string summary = analyzed.GetAnalysisSummary();
        Debug.Log($"<color=green>Analysis: {summary}</color>");
        
        // Add annotation
        ChessMove annotated = pawnMove.WithAnnotation(ChessMove.Annotations.Good);
        Debug.Log($"<color=green>Annotated move: {annotated.annotation}</color>");
        
        // Check move properties
        bool isCapture = pawnMove.IsCapture();
        bool isQuiet = pawnMove.IsQuiet();
        int distance = pawnMove.GetDistance();
        Debug.Log($"<color=green>Move properties - Capture: {isCapture}, Quiet: {isQuiet}, Distance: {distance}</color>");
        
        // Test promotion validation
        bool needsPromotion = ChessMove.RequiresPromotion(promFrom, promTo, 'P');
        char[] promoOptions = ChessMove.GetPromotionOptions(true);
        Debug.Log($"<color=green>Needs promotion: {needsPromotion}, Options: {string.Join(",", promoOptions)}</color>");
        
        // Create castling move
        v2 kingFrom = new v2(4, 0); // e1
        v2 kingTo = new v2(6, 0);   // g1
        v2 rookFrom = new v2(7, 0); // h1
        v2 rookTo = new v2(5, 0);   // f1
        ChessMove castling = new ChessMove(kingFrom, kingTo, rookFrom, rookTo, 'K');
        Debug.Log($"<color=green>Castling move: {castling.ToUCI()}</color>");
        
        // Test move equality
        ChessMove move1 = new ChessMove(from, to, 'P');
        ChessMove move2 = new ChessMove(from, to, 'P');
        bool areEqual = move1.Equals(move2);
        Debug.Log($"<color=green>Moves equal: {areEqual}</color>");
        
        // Run comprehensive tests
        ChessMove.RunAllTests();
        Debug.Log("<color=green>All tests completed</color>");
        
        // Expected output: 
        // "Chess board initialized"
        // "Normal move created: e2e4"  
        // "UCI parsed successfully: e2e4"
        // "PGN parsed successfully: e4"
        // "Promotion move: e7e8q"
        // "Analysis: Eval: +0.30, Depth: 12, Time: 150ms"
        // "Annotated move: !"
        // "Move properties - Capture: False, Quiet: True, Distance: 2"
        // "Needs promotion: True, Options: Q,R,B,N"
        // "Castling move: e1g1"
        // "Moves equal: True"
        // "All tests completed"
    }
}
```

## Control flow / responsibilities & high-level algorithm summary

Comprehensive chess move data structure with parsing pipeline: UCI/PGN input → validation → disambiguation → ChessMove struct. Performance-optimized with move caching and string builder usage.

## Performance, allocations, and hotspots

UCI cache reduces parsing overhead; StringBuilder minimizes string allocations; legal move list generation is O(n).

## Security / safety / correctness concerns

String parsing vulnerable to malformed input; coordinate bounds checking prevents array access errors.

## Tests, debugging & observability

Built-in test suite via RunAllTests(); Debug.Log with color coding; move validation and performance benchmarking included.

## Cross-file references

Depends on `ChessBoard.cs` for board state and legal move generation; uses `SPACE_UTIL.v2` for coordinate representation.

## General Note: important behaviors

Major functionality includes PGN disambiguation, UCI caching, promotion validation, castling detection, and engine analysis integration for chess AI systems.

`checksum: a7b3f9c2 (v0.3)`