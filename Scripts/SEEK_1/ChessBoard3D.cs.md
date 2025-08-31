# ChessBoard3D.cs.md

**Summary (1-3 sentences)**
ChessBoard3D is a Unity MonoBehaviour that provides 3D visualization and interaction for chess games, managing piece prefabs, coordinate conversion between board space and world space, and visual feedback for valid moves. It integrates with the existing ChessBoard logic system while providing drag-and-drop piece movement with path preview and animation support.

**Public Purpose / Intent**
- Manage 3D chess piece prefabs and their positions
- Provide coordinate conversion between 2D board coordinates and 3D world positions  
- Handle user interaction through drag-and-drop piece movement
- Display visual feedback for valid moves, captures, and selections
- Integrate with existing chess logic (ChessBoard, ChessRules, MoveGenerator)
- Support board flipping for different player perspectives

**Public API (most important members)**
- `Vector3 BoardToWorld(Vector2Int boardPos)` — convert board coordinates to world position
- `Vector2Int WorldToBoard(Vector3 worldPos)` — convert world position to board coordinates
- `bool TryMakeMove(Vector2Int fromSquare, Vector2Int toSquare)` — attempt to make a move with validation
- `void SelectPiece(ChessPieceController piece)` — handle piece selection
- `void ShowValidMoves(Vector2Int fromSquare)` — display valid move indicators
- `void HideValidMoves()` — hide all move indicators
- `bool CanPlayerMovePiece(char pieceColor)` — check if player can move pieces of given color
- `void SetPosition(string fen)` — set board position from FEN string
- `string GetCurrentFEN()` — get current position as FEN
- `void FlipBoard()` — flip board orientation

**Serialization / Inspector fields**
- `float tileSize = 1f` — size of each board tile in world units
- `Vector3 boardOrigin = Vector3.zero` — world position of a1 square
- `bool flipBoard = false` — whether to flip board for black perspective
- `List<GameObject> piecePrefabs` — piece prefabs named "chess-piece-{TYPE}"
- `GameObject validMovePrefab` — indicator for valid move squares
- `GameObject invalidMovePrefab` — indicator for invalid moves (reserved)
- `GameObject captureTargetPrefab` — indicator for capture squares
- `GameObject selectedTilePrefab` — indicator for selected piece tile
- `MinimalChessUI chessUI` — reference to chess UI controller
- `StockfishBridge stockfishBridge` — reference to engine bridge

**Key internals & flow**
1. Awake() → BuildPrefabMap() → map piece characters to prefab GameObjects
2. Start() → SetupBoard() → SpawnInitialPieces() → create initial piece GameObjects
3. User clicks piece → ChessPieceController.OnMouseDown() → SelectPiece() → ShowValidMoves()
4. User drags → UpdateDrag() → update piece position and path preview
5. User releases → TryMakeMove() → validate with ChessRules → ExecuteVisualMove()
6. ExecuteVisualMove() → handle special moves (castling, en passant, promotion) with animations
7. Coordinate conversion: BoardToWorld/WorldToBoard handle tileSize scaling and board flipping

**Dependencies**
- UnityEngine (MonoBehaviour, GameObject, Transform, Coroutines)
- SPACE_UTIL (v2 vector class from existing chess system)
- GPTDeepResearch.ChessBoard (logical board representation)
- GPTDeepResearch.ChessRules (move validation)
- GPTDeepResearch.MoveGenerator (legal move generation)
- GPTDeepResearch.ChessPieceController (individual piece control)
- GPTDeepResearch.MinimalChessUI (base chess UI integration)

**Side effects / threading / coroutines**
- GameObject instantiation/destruction for pieces and feedback objects
- Coroutine-based move animations (ExecuteVisualMove, ExecuteNormalMove, etc.)
- Transform manipulation for piece positioning
- Material changes for visual feedback
- No threading issues (all Unity main thread operations)

**Typical usage snippet**
```csharp
// Setup in scene
ChessBoard3D board3D = GetComponent<ChessBoard3D>();
board3D.SetPosition("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");

// Programmatic move
bool success = board3D.TryMakeMove(new Vector2Int(4, 1), new Vector2Int(4, 3)); // e2-e4

// Get world position for tile
Vector3 worldPos = board3D.BoardToWorld(new Vector2Int(4, 4)); // e5 square
```

**Important invariants & edge-cases**
- Piece prefabs must be named "chess-piece-{CHARACTER}" where CHARACTER matches piece type
- Board coordinates: (0,0) = a1, (7,7) = h8 regardless of board flipping
- World coordinates use tileSize scaling and boardOrigin offset
- Piece controllers must have ChessPieceController component
- Visual feedback objects are destroyed and recreated on each selection
- Board flipping affects world coordinate conversion but not logical coordinates
- Special moves (castling, en passant, promotion) require custom animation handling

**Suggested unit & integration tests**
- Test coordinate conversion: BoardToWorld(Vector2Int(0,0)) should equal boardOrigin
- Test piece spawning: verify all starting pieces are created correctly
- Test move validation: illegal moves should return false from TryMakeMove
- Test board flipping: coordinates should convert correctly in both orientations
- Test castling animation: king and rook should move simultaneously
- Test capture handling: captured pieces should be removed from board
- Test FEN synchronization: SetPosition should spawn correct pieces

**Scene & Prefab Setup Requirements**

**Camera Setup:**
- Use Perspective camera positioned at (4, 8, -6) looking down at board
- Layer mask should exclude "UI" layer to avoid raycast conflicts
- Consider adding OrbitControls for camera rotation around board

**Board Grid Mapping:**
- Tile (0,0) [a1] maps to world position (boardOrigin.x, boardOrigin.y, boardOrigin.z)  
- Tile (x,y) maps to world position (boardOrigin + Vector3(x * tileSize, 0, y * tileSize))
- Board flipping inverts x and y coordinates: (7-x, 7-y)

**Piece Prefab Requirements:**
- Name format: "chess-piece-K", "chess-piece-k", "chess-piece-Q", etc.
- Components required: MeshRenderer, BoxCollider (isTrigger=false), ChessPieceController script
- Collider should encompass piece bounds for accurate mouse detection
- Optional: Animator component for piece animations
- Child objects: optional LineRenderer for path preview (auto-created if missing)

**Visual Feedback Prefabs:**
- validMovePrefab: small cube/sphere indicating legal move squares
- captureTargetPrefab: different colored indicator for capture squares  
- selectedTilePrefab: highlight for currently selected piece square
- All feedback prefabs should be simple geometric shapes with distinct materials

**Layers/Tags:**
- Reserve "ChessPiece" layer for all chess pieces to control raycast interactions
- Use "Board" layer for board tiles/background
- Keep "UI" layer separate to avoid conflicts with 3D interaction

**Suggested commit message when changing this file**
`feat(chess3d): add 3D interactive board with drag-drop and visual feedback`

**Signature**
Generated by Claude 4