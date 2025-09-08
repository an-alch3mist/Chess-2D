# Source: `ChessMove.cs` — Comprehensive chess move representation with UCI/PGN parsing and Unity integration

## Short description
Implements a complete chess move system with parsing support for UCI and PGN notations, move validation, performance optimizations, and comprehensive analysis data integration for Unity-based chess applications.

## Metadata
* **Filename:** `ChessMove.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependencies:** System, System.Collections.Generic, System.Text, UnityEngine, SPACE_UTIL
* **Public types:** `ChessMove (struct), ChessMove.MoveType (enum), ChessMove.Annotations (static class)`
* **Unity version:** Unity 2020.3 Compatible

## Public API Summary
| Type | Member | Signature | Short Purpose | OneLiner Call |
|------|---------|-----------|---------------|---------------|
| v2 | Field | public v2 from | Source square coordinates | var pos = move.from; |
| v2 | Field | public v2 to | Target square coordinates | var pos = move.to; |
| char | Field | public char piece | Moving piece character | var p = move.piece; |
| char | Field | public char capturedPiece | Captured piece character | var cp = move.capturedPiece; |
| ChessMove.MoveType | Field | public ChessMove.MoveType moveType | Special move classification | var type = move.moveType; |
| char | Field | public char promotionPiece | Promotion target piece | var promo = move.promotionPiece; |
| v2 | Field | public v2 rookFrom | Castling rook source | var rf = move.rookFrom; |
| v2 | Field | public v2 rookTo | Castling rook target | var rt = move.rookTo; |
| float | Field | public float analysisTime | Analysis duration in ms | var time = move.analysisTime; |
| string | Field | public string annotation | Move annotation symbols | var ann = move.annotation; |
| int | Field | public int engineDepth | Engine search depth | var depth = move.engineDepth; |
| float | Field | public float engineEval | Engine position evaluation | var eval = move.engineEval; |
| void | Constructor | public ChessMove(v2 from, v2 to, char piece, char capturedPiece) | Normal move constructor | var move = new ChessMove(from, to, 'P'); |
| void | Constructor | public ChessMove(v2 from, v2 to, char piece, char promotionPiece, char capturedPiece) | Promotion move constructor | var move = new ChessMove(from, to, 'P', 'Q'); |
| void | Constructor | public ChessMove(v2 kingFrom, v2 kingTo, v2 rookFrom, v2 rookTo, char piece) | Castling move constructor | var move = new ChessMove(kf, kt, rf, rt, 'K'); |
| ChessMove | Method | public static ChessMove FromPGN(string pgnMove, ChessBoard board, List<ChessMove> legalMoves) | Parse PGN notation to move | var move = ChessMove.FromPGN("e4", board); |
| ChessMove | Method | public static ChessMove FromUCI(string uciMove, ChessBoard board) | Parse UCI notation to move | var move = ChessMove.FromUCI("e2e4", board); |
| ChessMove | Method | public static ChessMove CreatePromotionMove(v2 from, v2 to, char movingPiece, char promotionType, char capturedPiece) | Create validated promotion move | var move = ChessMove.CreatePromotionMove(from, to, 'P', 'Q'); |
| ChessMove | Method | public static ChessMove Invalid() | Create invalid move marker | var invalid = ChessMove.Invalid(); |
| string | Method | public string ToPGN(ChessBoard board, List<ChessMove> legalMoves) | Convert to PGN notation | var pgn = move.ToPGN(board); |
| string | Method | public string ToUCI() | Convert to UCI notation | var uci = move.ToUCI(); |
| ChessMove | Method | public ChessMove WithAnalysisData(float analysisTimeMs, int depth, float evaluation) | Add engine analysis data | var analyzed = move.WithAnalysisData(1000f, 12, 0.5f); |
| ChessMove | Method | public ChessMove WithAnnotation(string annotation) | Add move annotation | var annotated = move.WithAnnotation("!"); |
| string | Method | public string GetAnalysisSummary() | Generate analysis summary text | var summary = move.GetAnalysisSummary(); |
| bool | Method | public bool IsValid() | Check move validity | var valid = move.IsValid(); |
| bool | Method | public bool IsLegal(ChessBoard board) | Validate against board state | var legal = move.IsLegal(board); |
| bool | Method | public bool IsCapture() | Check if move captures | var captures = move.IsCapture(); |
| bool | Method | public bool IsQuiet() | Check if move is quiet | var quiet = move.IsQuiet(); |
| int | Method | public int GetDistance() | Calculate Manhattan distance | var dist = move.GetDistance(); |
| bool | Method | public static bool RequiresPromotion(v2 from, v2 to, char piece) | Check if move needs promotion | var needs = ChessMove.RequiresPromotion(from, to, 'P'); |
| bool | Method | public static bool IsValidPromotionPiece(char piece) | Validate promotion piece | var valid = ChessMove.IsValidPromotionPiece('Q'); |
| char | Method | public static char GetDefaultPromotionPiece(bool isWhite) | Get default promotion choice | var piece = ChessMove.GetDefaultPromotionPiece(true); |
| char[] | Method | public static char[] GetPromotionOptions(bool isWhite) | Get all promotion options | var options = ChessMove.GetPromotionOptions(true); |
| string | Method | public static string GetPromotionPieceName(char piece) | Get piece name string | var name = ChessMove.GetPromotionPieceName('Q'); |
| bool | Method | public bool Equals(ChessMove other) | Compare move equality | var equal = move1.Equals(move2); |
| void | Method | public static void RunAllTests() | Execute comprehensive test suite | ChessMove.RunAllTests(); |

## Important Types

### `ChessMove.MoveType`
* **Kind:** enum
* **Responsibility:** Classifies special move types for chess move processing
* **Values:**
  * `Normal` — Standard piece movement
  * `Castling` — King and rook castling maneuver
  * `EnPassant` — Pawn en passant capture
  * `Promotion` — Pawn promotion to other piece
  * `CastlingPromotion` — Combined castling and promotion (rare)

### `ChessMove.Annotations`
* **Kind:** static class
* **Responsibility:** Provides standard PGN annotation constants
* **Public Constants:**
  * `Check` — "+" check notation
  * `Checkmate` — "#" checkmate notation
  * `Brilliant` — "!!" brilliant move
  * `Good` — "!" good move
  * `Interesting` — "!?" interesting move
  * `Dubious` — "?!" dubious move
  * `Mistake` — "?" mistake
  * `Blunder` — "??" blunder

### `ChessMove`
* **Kind:** struct implementing IEquatable<ChessMove>
* **Responsibility:** Represents complete chess move with parsing, validation, and analysis capabilities
* **Constructor(s):** 
  * `public ChessMove(v2 from, v2 to, char piece, char capturedPiece = '\0')` — Normal move
  * `public ChessMove(v2 from, v2 to, char piece, char promotionPiece, char capturedPiece = '\0')` — Promotion move
  * `public ChessMove(v2 kingFrom, v2 kingTo, v2 rookFrom, v2 rookTo, char piece)` — Castling move
* **Public Properties:**
  * `from` — `v2` — Source square coordinates (`get/set`)
  * `to` — `v2` — Target square coordinates (`get/set`)
  * `piece` — `char` — Moving piece character (`get/set`)
  * `capturedPiece` — `char` — Captured piece character (`get/set`)
  * `moveType` — `ChessMove.MoveType` — Special move classification (`get/set`)
  * `promotionPiece` — `char` — Promotion target piece (`get/set`)
  * `rookFrom` — `v2` — Castling rook source coordinates (`get/set`)
  * `rookTo` — `v2` — Castling rook target coordinates (`get/set`)
  * `analysisTime` — `float` — Analysis duration in milliseconds (`get/set`)
  * `annotation` — `string` — Move annotation symbols (`get/set`)
  * `engineDepth` — `int` — Engine search depth (`get/set`)
  * `engineEval` — `float` — Engine position evaluation (`get/set`)
* **Public Methods:**
  * **`public static ChessMove FromPGN(string pgnMove, ChessBoard board, List<ChessMove> legalMoves = null)`**
    * Description: Parse PGN algebraic notation into ChessMove with disambiguation
    * Parameters: `pgnMove : string — PGN move string`, `board : ChessBoard — Current board state`, `legalMoves : List<ChessMove> — Optional legal moves list`
    * Returns: `ChessMove — Parsed move or Invalid()` + call example: `var move = ChessMove.FromPGN("Nf3", board);`
    * Notes: Handles castling, promotion, disambiguation, and annotations
  * **`public static ChessMove FromUCI(string uciMove, ChessBoard board)`**
    * Description: Parse UCI coordinate notation with caching optimization
    * Parameters: `uciMove : string — UCI format move`, `board : ChessBoard — Current board state`
    * Returns: `ChessMove — Parsed move or Invalid()` + call example: `var move = ChessMove.FromUCI("e2e4", board);`
    * Notes: Performance optimized with move caching, auto-detects promotion
  * **`public string ToPGN(ChessBoard board, List<ChessMove> legalMoves = null)`**
    * Description: Convert move to standard PGN algebraic notation
    * Parameters: `board : ChessBoard — Current board state`, `legalMoves : List<ChessMove> — Optional legal moves for disambiguation`
    * Returns: `string — PGN notation with proper disambiguation` + call example: `var pgn = move.ToPGN(board);`
    * Notes: Includes proper disambiguation logic and annotation preservation
  * **`public bool IsValid()`**
    * Description: Validate move coordinates and piece data
    * Returns: `bool — True if move has valid structure` + call example: `var valid = move.IsValid();`
  * **`public ChessMove WithAnalysisData(float analysisTimeMs, int depth, float evaluation)`**
    * Description: Create copy with engine analysis data attached
    * Parameters: `analysisTimeMs : float — Analysis time`, `depth : int — Search depth`, `evaluation : float — Position evaluation`
    * Returns: `ChessMove — Move copy with analysis data` + call example: `var analyzed = move.WithAnalysisData(1000f, 12, 0.5f);`

## Example Usage
**Required namespaces:**
```csharp
// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using GPTDeepResearch;
// using SPACE_UTIL;
```

**For Non-MonoBehaviour classes:**
```csharp
public class ExampleUsage : MonoBehaviour 
{
    private void ChessMove_Check()
    {
        // Test all major public APIs in minimal lines
        var board = new ChessBoard();
        var move1 = ChessMove.FromPGN("e4", board);
        var move2 = ChessMove.FromUCI("e2e4", board);
        var promotion = ChessMove.CreatePromotionMove(new v2(4,6), new v2(4,7), 'P', 'Q');
        var invalid = ChessMove.Invalid();
        var annotated = move1.WithAnnotation("!");
        var analyzed = move1.WithAnalysisData(1500f, 12, 0.25f);
        var pgn = move1.ToPGN(board);
        var uci = move1.ToUCI();
        var valid = move1.IsValid();
        var summary = analyzed.GetAnalysisSummary();
        var options = ChessMove.GetPromotionOptions(true);
        
        Debug.Log($"API Results: PGN={move1.ToUCI()}, UCI={move2.ToUCI()}, Promotion={promotion.IsValid()}, Invalid={invalid.IsValid()}, Annotated={annotated.annotation}, Analysis={summary}, Valid={valid}, Options={options.Length}");
    }
}
```

## Control Flow & Responsibilities
Parsing UCI/PGN notations, move validation, performance caching, analysis data integration, comprehensive testing framework.

## Performance & Threading
UCI caching system, optimized string operations, main-thread compatible, no async operations.

## Cross-file Dependencies
ChessBoard.cs for board state validation, SPACE_UTIL.v2 for coordinates, UnityEngine for logging.

## Major Functionality
PGN/UCI parsing, move disambiguation, promotion validation, castling detection, analysis integration, comprehensive testing suite.

`checksum: a7f8c2e1 v0.3.min`