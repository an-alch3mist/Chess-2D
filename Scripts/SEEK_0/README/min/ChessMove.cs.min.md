# Source: `ChessMove.cs` — Comprehensive chess move representation with UCI/PGN parsing and performance optimizations

## Short description
Implements a comprehensive chess move struct with support for UCI and PGN parsing, move validation, castling, en passant, promotions, and performance-optimized caching. Includes extensive analysis data support and Unity 2020.3 compatibility features.

## Metadata
* **Filename:** `ChessMove.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependencies:** `System`, `System.Collections.Generic`, `System.Text`, `UnityEngine`, `SPACE_UTIL`
* **Public types:** `ChessMove (struct), ChessMove.MoveType (enum), ChessMove.Annotations (static class)`
* **Unity version:** Unity 2020.3 compatible

## Public API Summary
| Type | Member | Signature | Short Purpose | OneLiner Call |
|------|---------|-----------|---------------|---------------|
| v2 | Field | public v2 from; | Source square coordinates | var pos = move.from; |
| v2 | Field | public v2 to; | Target square coordinates | var pos = move.to; |
| char | Field | public char piece; | Moving piece character | var p = move.piece; |
| char | Field | public char capturedPiece; | Captured piece character | var cap = move.capturedPiece; |
| ChessMove.MoveType | Field | public MoveType moveType; | Type of move being made | var type = move.moveType; |
| char | Field | public char promotionPiece; | Promotion piece character | var promo = move.promotionPiece; |
| v2 | Field | public v2 rookFrom; | Castling rook source | var rookPos = move.rookFrom; |
| v2 | Field | public v2 rookTo; | Castling rook target | var rookTarget = move.rookTo; |
| float | Property | public float analysisTime { get; private set; } | Analysis time in ms | var time = move.analysisTime; |
| string | Property | public string annotation { get; private set; } | Move annotation (+, #, !, ?) | var note = move.annotation; |
| int | Property | public int engineDepth { get; private set; } | Engine search depth | var depth = move.engineDepth; |
| float | Property | public float engineEval { get; private set; } | Engine position evaluation | var eval = move.engineEval; |
| ChessMove | Constructor | public ChessMove(v2 from, v2 to, char piece, char capturedPiece = '\0') | Creates normal move | var move = new ChessMove(from, to, 'P'); |
| ChessMove | Constructor | public ChessMove(v2 from, v2 to, char piece, char promotionPiece, char capturedPiece = '\0') | Creates promotion move | var move = new ChessMove(from, to, 'P', 'Q'); |
| ChessMove | Constructor | public ChessMove(v2 kingFrom, v2 kingTo, v2 rookFrom, v2 rookTo, char piece) | Creates castling move | var move = new ChessMove(kFrom, kTo, rFrom, rTo, 'K'); |
| ChessMove | Method | public static ChessMove FromPGN(string pgnMove, ChessBoard board, List<ChessMove> legalMoves = null) | Parse PGN notation | var move = ChessMove.FromPGN("e4", board); |
| ChessMove | Method | public static ChessMove FromUCI(string uciMove, ChessBoard board) | Parse UCI notation | var move = ChessMove.FromUCI("e2e4", board); |
| ChessMove | Method | public static ChessMove CreatePromotionMove(v2 from, v2 to, char movingPiece, char promotionType, char capturedPiece = '\0') | Create validated promotion | var move = ChessMove.CreatePromotionMove(from, to, 'P', 'Q'); |
| ChessMove | Method | public static ChessMove Invalid() | Returns invalid move | var invalid = ChessMove.Invalid(); |
| string | Method | public string ToPGN(ChessBoard board, List<ChessMove> legalMoves = null) | Convert to PGN notation | var pgn = move.ToPGN(board); |
| string | Method | public string ToUCI() | Convert to UCI notation | var uci = move.ToUCI(); |
| ChessMove | Method | public ChessMove WithAnalysisData(float analysisTimeMs, int depth, float evaluation) | Add analysis metadata | var analyzed = move.WithAnalysisData(1500f, 12, 0.25f); |
| ChessMove | Method | public ChessMove WithAnnotation(string annotation) | Add move annotation | var annotated = move.WithAnnotation("!"); |
| string | Method | public string GetAnalysisSummary() | Get analysis info string | var summary = move.GetAnalysisSummary(); |
| bool | Method | public bool IsValid() | Check move validity | var valid = move.IsValid(); |
| bool | Method | public bool IsLegal(ChessBoard board) | Check if legal on board | var legal = move.IsLegal(board); |
| bool | Method | public bool IsCapture() | Check if capture move | var capture = move.IsCapture(); |
| bool | Method | public bool IsQuiet() | Check if quiet move | var quiet = move.IsQuiet(); |
| int | Method | public int GetDistance() | Get Manhattan distance | var dist = move.GetDistance(); |
| bool | Method | public static bool RequiresPromotion(v2 from, v2 to, char piece) | Check promotion requirement | var needs = ChessMove.RequiresPromotion(from, to, 'P'); |
| bool | Method | public static bool IsValidPromotionPiece(char piece) | Validate promotion piece | var valid = ChessMove.IsValidPromotionPiece('Q'); |
| char | Method | public static char GetDefaultPromotionPiece(bool isWhite) | Get default promotion | var piece = ChessMove.GetDefaultPromotionPiece(true); |
| char[] | Method | public static char[] GetPromotionOptions(bool isWhite) | Get promotion choices | var options = ChessMove.GetPromotionOptions(true); |
| string | Method | public static string GetPromotionPieceName(char piece) | Get piece display name | var name = ChessMove.GetPromotionPieceName('Q'); |
| bool | Method | public bool Equals(ChessMove other) | Compare moves | var equal = move1.Equals(move2); |
| string | Method | public override string ToString() | Debug string representation | var str = move.ToString(); |
| void | Method | public static void RunAllTests() | Execute test suite | ChessMove.RunAllTests(); |

## Important Types

### `ChessMove.MoveType`
* **Kind:** enum
* **Responsibility:** Categorizes different types of chess moves
* **Values:**
  * `Normal` — Standard piece movement
  * `Castling` — King and rook castling maneuver
  * `EnPassant` — Pawn en passant capture
  * `Promotion` — Pawn promotion to another piece
  * `CastlingPromotion` — Combined castling with promotion (unused)

### `ChessMove.Annotations`
* **Kind:** static class
* **Responsibility:** Provides standard PGN move annotation constants
* **Public Fields:**
  * `Check` — "+" for check notation
  * `Checkmate` — "#" for checkmate notation  
  * `Brilliant` — "!!" for brilliant move
  * `Good` — "!" for good move
  * `Interesting` — "!?" for interesting move
  * `Dubious` — "?!" for dubious move
  * `Mistake` — "?" for mistake
  * `Blunder` — "??" for blunder

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
        // Create test board and moves
        var board = new ChessBoard();
        var from = new v2(4, 1);
        var to = new v2(4, 3);
        
        // Test major public APIs
        var normalMove = new ChessMove(from, to, 'P');
        var promotionMove = ChessMove.CreatePromotionMove(new v2(4, 6), new v2(4, 7), 'P', 'Q');
        var fromUCI = ChessMove.FromUCI("e2e4", board);
        var fromPGN = ChessMove.FromPGN("e4", board);
        var analyzed = normalMove.WithAnalysisData(1500f, 12, 0.25f).WithAnnotation("!");
        var uci = normalMove.ToUCI();
        var pgn = normalMove.ToPGN(board);
        var valid = normalMove.IsValid();
        var capture = normalMove.IsCapture();
        var distance = normalMove.GetDistance();
        var summary = analyzed.GetAnalysisSummary();
        var needsPromo = ChessMove.RequiresPromotion(from, to, 'P');
        var options = ChessMove.GetPromotionOptions(true);
        
        Debug.Log($"API Results: {uci}, {pgn}, Valid:{valid}, Capture:{capture}, Distance:{distance}, Summary:{summary}, NeedsPromo:{needsPromo}, Options:{options.Length}");
    }

    private void SimpleVerification_ChessMove_Check()
    {
      // After e4, Nf3 manually check the position
      ChessBoard testBoard = new ChessBoard();
      testBoard.MakeMove(ChessMove.FromUCI("e2e4", testBoard));
      Debug.Log($"Position after e2e4: {testBoard.ToFEN()}");

      testBoard.MakeMove(ChessMove.FromPGN("e5", testBoard)); // black turn
      Debug.Log($"Position after e7e5: {testBoard.ToFEN()}");

      testBoard.MakeMove(ChessMove.FromUCI("f1c4", testBoard));
      Debug.Log($"Position after f1c4: {testBoard.ToFEN()}");

      testBoard.MakeMove(ChessMove.FromPGN("Qh4", testBoard)); // black turn
      Debug.Log($"Position after Qh4: {testBoard.ToFEN()}");
      return;
    }
}
```

## Control Flow & Responsibilities
UCI/PGN parsing with caching, move validation, analysis integration, comprehensive chess move operations

## Performance & Threading  
Move caching, optimized string operations, main-thread only

## Cross-file Dependencies
ChessBoard.cs for board state validation, SPACE_UTIL.v2 coordinate system

## Major Functionality
PGN/UCI parsing, move disambiguation, promotion handling, castling support, caching optimization

`checksum: 4A7B9C2E v0.3.min`