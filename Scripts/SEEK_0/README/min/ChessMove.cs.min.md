# Source: `ChessMove.cs` — Enhanced chess move representation with comprehensive parsing, PGN support, and engine integration

## Comprehensive chess move structure supporting UCI/PGN parsing, algebraic notation, move validation, engine analysis integration, and performance optimizations for high-frequency chess operations.

## Metadata
* **Filename:** `ChessMove.cs`
* **Primary namespace:** `GPTDeepResearch`
* **Dependencies:** `System`, `System.Collections.Generic`, `System.Text`, `UnityEngine`, `SPACE_UTIL`
* **Public types:** `ChessMove (struct), ChessMove.MoveType (enum), ChessMove.Annotations (static class)`
* **Unity version:** Unity-compatible (uses UnityEngine.Debug, [Header] attributes)

## Public API Summary
| Type | Member | Signature | Short Purpose | OneLiner Call |
|------|---------|-----------|---------------|---------------|
| v2 | Field | public v2 from | Source square coordinates | var pos = move.from; |
| v2 | Field | public v2 to | Target square coordinates | var pos = move.to; |
| char | Field | public char piece | Moving piece character | var p = move.piece; |
| char | Field | public char capturedPiece | Captured piece or '\0' | var cap = move.capturedPiece; |
| ChessMove.MoveType | Field | public MoveType moveType | Move classification type | var type = move.moveType; |
| char | Field | public char promotionPiece | Promotion target piece | var promo = move.promotionPiece; |
| v2 | Field | public v2 rookFrom | Castling rook source | var rookPos = move.rookFrom; |
| v2 | Field | public v2 rookTo | Castling rook target | var rookPos = move.rookTo; |
| float | Field | public float analysisTime | Engine analysis time (ms) | var time = move.analysisTime; |
| string | Field | public string annotation | PGN annotation (+, #, !) | var note = move.annotation; |
| int | Field | public int engineDepth | Search depth from engine | var depth = move.engineDepth; |
| float | Field | public float engineEval | Engine position evaluation | var eval = move.engineEval; |
| ChessMove | Constructor | public ChessMove(v2 from, v2 to, char piece, char capturedPiece = '\0') | Normal move creation | var move = new ChessMove(fromPos, toPos, 'P'); |
| ChessMove | Constructor | public ChessMove(v2 from, v2 to, char piece, char promotionPiece, char capturedPiece = '\0') | Promotion move creation | var move = new ChessMove(fromPos, toPos, 'P', 'Q'); |
| ChessMove | Constructor | public ChessMove(v2 kingFrom, v2 kingTo, v2 rookFrom, v2 rookTo, char piece) | Castling move creation | var move = new ChessMove(kingFrom, kingTo, rookFrom, rookTo, 'K'); |
| ChessMove | Method | public static ChessMove FromPGN(string pgnMove, ChessBoard board, List<ChessMove> legalMoves = null) | Parse from PGN notation | var move = ChessMove.FromPGN("e4", board); |
| ChessMove | Method | public static ChessMove FromUCI(string uciMove, ChessBoard board) | Parse from UCI notation | var move = ChessMove.FromUCI("e2e4", board); |
| string | Method | public string ToPGN(ChessBoard board, List<ChessMove> legalMoves = null) | Convert to PGN notation | var pgn = move.ToPGN(board); |
| string | Method | public string ToUCI() | Convert to UCI notation | var uci = move.ToUCI(); |
| ChessMove | Method | public ChessMove WithAnalysisData(float analysisTimeMs, int depth, float evaluation) | Add engine analysis data | var analyzed = move.WithAnalysisData(100f, 12, 0.5f); |
| ChessMove | Method | public ChessMove WithAnnotation(string annotation) | Add move annotation | var annotated = move.WithAnnotation("!"); |
| string | Method | public string GetAnalysisSummary() | Get analysis info string | var summary = move.GetAnalysisSummary(); |
| bool | Method | public bool IsValid() | Validate move coordinates | var valid = move.IsValid(); |
| bool | Method | public bool IsLegal(ChessBoard board) | Check if move is legal | var legal = move.IsLegal(board); |
| bool | Method | public bool IsCapture() | Check if move captures | var captures = move.IsCapture(); |
| bool | Method | public bool IsQuiet() | Check if move is quiet | var quiet = move.IsQuiet(); |
| int | Method | public int GetDistance() | Manhattan distance moved | var dist = move.GetDistance(); |
| bool | Method | public static bool RequiresPromotion(v2 from, v2 to, char piece) | Check if promotion needed | var needsPromo = ChessMove.RequiresPromotion(from, to, 'P'); |
| bool | Method | public static bool IsValidPromotionPiece(char piece) | Validate promotion piece | var validPromo = ChessMove.IsValidPromotionPiece('Q'); |
| char | Method | public static char GetDefaultPromotionPiece(bool isWhite) | Get default promotion | var defaultPiece = ChessMove.GetDefaultPromotionPiece(true); |
| char[] | Method | public static char[] GetPromotionOptions(bool isWhite) | Get promotion choices | var options = ChessMove.GetPromotionOptions(true); |
| string | Method | public static string GetPromotionPieceName(char piece) | Get promotion name | var name = ChessMove.GetPromotionPieceName('Q'); |
| ChessMove | Method | public static ChessMove CreatePromotionMove(v2 from, v2 to, char movingPiece, char promotionType, char capturedPiece = '\0') | Create promotion move | var move = ChessMove.CreatePromotionMove(from, to, 'P', 'Q'); |
| ChessMove | Method | public static ChessMove Invalid() | Create invalid move | var invalid = ChessMove.Invalid(); |
| bool | Method | public bool Equals(ChessMove other) | Compare moves | var same = move1.Equals(move2); |
| void | Method | public static void RunAllTests() | Execute test suite | ChessMove.RunAllTests(); |

## Important Types

### `ChessMove.MoveType`
* **Kind:** enum
* **Responsibility:** Classifies the type of chess move being represented
* **Values:**
  * `Normal` — Standard piece move
  * `Castling` — King-rook castling
  * `EnPassant` — En passant pawn capture
  * `Promotion` — Pawn promotion
  * `CastlingPromotion` — Combined castling and promotion

### `ChessMove.Annotations`
* **Kind:** static class
* **Responsibility:** Provides standard PGN move annotation constants
* **Constants:**
  * `Check` — "+" for check
  * `Checkmate` — "#" for checkmate
  * `Brilliant` — "!!" for brilliant move
  * `Good` — "!" for good move
  * `Interesting` — "!?" for interesting move
  * `Dubious` — "?!" for dubious move
  * `Mistake` — "?" for mistake
  * `Blunder` — "??" for blunder

### `ChessMove` (Main Struct)
* **Kind:** struct implementing IEquatable<ChessMove>
* **Responsibility:** Complete chess move representation with parsing, validation, and analysis support
* **Constructor(s):** Three overloads for normal moves, promotions, and castling
* **Public Properties:** All fields are public for direct access to move data
* **Public Methods:**
  * **`public static ChessMove FromPGN(string pgnMove, ChessBoard board, List<ChessMove> legalMoves = null)`**
    * Description: Parses algebraic notation (PGN) into ChessMove with disambiguation
    * Parameters: `pgnMove : string — PGN notation like "Nf3", "exd5", "O-O"`, `board : ChessBoard — Current board state`, `legalMoves : List<ChessMove> — Optional legal moves list`
    * Returns: `ChessMove — Parsed move or Invalid()` + call example: `var move = ChessMove.FromPGN("e4", board);`
    * Notes: Supports castling, promotion, captures, disambiguation, annotations
  * **`public static ChessMove FromUCI(string uciMove, ChessBoard board)`**
    * Description: Parses UCI coordinate notation with caching optimization
    * Parameters: `uciMove : string — UCI format like "e2e4", "e7e8q"`, `board : ChessBoard — Current board state`
    * Returns: `ChessMove — Parsed move or Invalid()` + call example: `var move = ChessMove.FromUCI("e2e4", board);`
    * Notes: Cached for performance, auto-detects promotions
  * **`public string ToPGN(ChessBoard board, List<ChessMove> legalMoves = null)`**
    * Description: Converts move to standard algebraic notation with disambiguation
    * Parameters: `board : ChessBoard — Board state for context`, `legalMoves : List<ChessMove> — Optional legal moves for disambiguation`
    * Returns: `string — PGN notation like "Nf3", "exd5+"` + call example: `var pgn = move.ToPGN(board);`
    * Notes: Includes annotations, handles castling special cases
  * **`public ChessMove WithAnalysisData(float analysisTimeMs, int depth, float evaluation)`**
    * Description: Creates copy with engine analysis metadata attached
    * Parameters: `analysisTimeMs : float — Analysis time in milliseconds`, `depth : int — Search depth`, `evaluation : float — Position evaluation`
    * Returns: `ChessMove — Copy with analysis data` + call example: `var analyzed = move.WithAnalysisData(150f, 12, 0.3f);`
    * Notes: Immutable operation, returns new instance

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
        var board = new ChessBoard();
        var fromPos = new v2(4, 1);
        var toPos = new v2(4, 3);
        
        // Test move creation and parsing
        var normalMove = new ChessMove(fromPos, toPos, 'P');
        var uciMove = ChessMove.FromUCI("e2e4", board);
        var pgnMove = ChessMove.FromPGN("e4", board);
        var promotion = ChessMove.CreatePromotionMove(new v2(4, 6), new v2(4, 7), 'P', 'Q');
        
        // Test conversions and validation
        var uci = normalMove.ToUCI();
        var pgn = normalMove.ToPGN(board);
        var isValid = normalMove.IsValid();
        var isCapture = normalMove.IsCapture();
        var distance = normalMove.GetDistance();
        
        // Test analysis integration
        var analyzed = normalMove.WithAnalysisData(100f, 12, 0.5f);
        var annotated = normalMove.WithAnnotation("!");
        var summary = analyzed.GetAnalysisSummary();
        
        // Test utility methods
        var needsPromo = ChessMove.RequiresPromotion(fromPos, toPos, 'P');
        var promoOptions = ChessMove.GetPromotionOptions(true);
        
        Debug.Log($"API Results: {uci}, {pgn}, Valid: {isValid}, Capture: {isCapture}, Distance: {distance}, Summary: {summary}, Promotion needed: {needsPromo}");
    }
}
```

## Control Flow & Responsibilities
Parse UCI/PGN notation, validate moves, handle special cases (castling, promotion, en passant), provide analysis integration.

## Performance & Threading  
UCI parsing cached, string operations optimized, main-thread safe operations.

## Cross-file Dependencies
References ChessBoard.cs for position state, SPACE_UTIL.v2 for coordinates, Unity Debug logging.

## Major Functionality
PGN/UCI parsing, move validation, promotion handling, castling support, engine analysis integration, move disambiguation.

`checksum: A7F3B2E1 v0.3.min`