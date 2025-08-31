/*
CHANGELOG (New File):
- 3D chess board controller managing piece prefabs and board visualization
- Integration with existing ChessBoard logic for move validation
- Visual feedback system for valid/invalid moves and captures
- Coordinate conversion between 3D world space and board coordinates
- Piece spawning and management system with prefab references
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SPACE_UTIL;
using System;

namespace GPTDeepResearch
{
	/// <summary>
	/// 3D chess board controller that manages the visual representation
	/// and user interaction for a chess game in 3D space.
	/// </summary>
	public class ChessBoard3D : MonoBehaviour
	{
		[Header("Board Settings")]
		[SerializeField] private float tileSize = 1f;
		[SerializeField] private Vector3 boardOrigin = Vector3.zero;
		[SerializeField] private bool flipBoard = false; // Flip board for black perspective

		[Header("Piece Prefabs - Name Format: chess-piece-{TYPE}")]
		[SerializeField] private List<GameObject> piecePrefabs = new List<GameObject>();

		[Header("Visual Feedback Prefabs")]
		[SerializeField] private GameObject validMovePrefab;
		[SerializeField] private GameObject invalidMovePrefab;
		[SerializeField] private GameObject captureTargetPrefab;
		[SerializeField] private GameObject selectedTilePrefab;

		[Header("Game Integration")]
		[SerializeField] private MinimalChessUI chessUI;
		[SerializeField] private StockfishBridge stockfishBridge;

		// State
		private ChessBoard logicalBoard;
		private Dictionary<Vector2Int, ChessPieceController> pieceControllers = new Dictionary<Vector2Int, ChessPieceController>();
		private Dictionary<char, GameObject> piecePrefabMap = new Dictionary<char, GameObject>();
		private List<GameObject> feedbackObjects = new List<GameObject>();
		private ChessPieceController selectedPiece;

		void Awake()
		{
			BuildPrefabMap();
			logicalBoard = new ChessBoard();
		}

		void Start()
		{
			SetupBoard();
			SpawnInitialPieces();
		}

		#region Board Setup

		private void BuildPrefabMap()
		{
			piecePrefabMap.Clear();

			foreach (GameObject prefab in piecePrefabs)
			{
				if (prefab == null) continue;

				string name = prefab.name;
				if (name.StartsWith("chess-piece-") && name.Length >= 13)
				{
					char pieceChar = name[12]; // Extract piece character
					piecePrefabMap[pieceChar] = prefab;
				}
			}

			Debug.Log($"[ChessBoard3D] Mapped {piecePrefabMap.Count} piece prefabs");
		}

		private void SetupBoard()
		{
			// Board setup logic can be extended here
			// For now, just ensure we have a clean state
			ClearFeedbackObjects();
		}

		private void SpawnInitialPieces()
		{
			ClearAllPieces();

			// Spawn pieces based on current logical board
			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					Vector2Int boardPos = new Vector2Int(x, y);
					char piece = logicalBoard.board.GT(new v2(x, y));

					if (piece != '.')
					{
						SpawnPiece(piece, boardPos);
					}
				}
			}
		}

		#endregion

		#region Piece Management

		private void SpawnPiece(char pieceType, Vector2Int boardPosition)
		{
			if (!piecePrefabMap.ContainsKey(pieceType))
			{
				Debug.LogWarning($"[ChessBoard3D] No prefab found for piece '{pieceType}'");
				return;
			}

			GameObject prefab = piecePrefabMap[pieceType];
			Vector3 worldPos = BoardToWorld(boardPosition);

			GameObject pieceObj = Instantiate(prefab, worldPos, Quaternion.identity, transform);
			ChessPieceController controller = pieceObj.GetComponent<ChessPieceController>();

			if (controller == null)
			{
				controller = pieceObj.AddComponent<ChessPieceController>();
			}

			char color = char.IsUpper(pieceType) ? 'w' : 'b';
			controller.Initialize(pieceType, color, boardPosition, this);

			pieceControllers[boardPosition] = controller;
		}

		private void ClearAllPieces()
		{
			foreach (var kvp in pieceControllers)
			{
				if (kvp.Value != null && kvp.Value.gameObject != null)
				{
					DestroyImmediate(kvp.Value.gameObject);
				}
			}
			pieceControllers.Clear();
		}

		#endregion

		#region Move Handling

		public bool TryMakeMove(Vector2Int fromSquare, Vector2Int toSquare)
		{
			// Create ChessMove from coordinates
			v2 from = new v2(fromSquare.x, fromSquare.y);
			v2 to = new v2(toSquare.x, toSquare.y);

			char piece = logicalBoard.board.GT(from);
			char capturedPiece = logicalBoard.board.GT(to);
			if (capturedPiece == '.') capturedPiece = '\0';

			ChessMove move = new ChessMove(from, to, piece, capturedPiece);

			// Check for special moves
			if (char.ToUpper(piece) == 'K' && Math.Abs(toSquare.x - fromSquare.x) >= 2)
			{
				// Potential castling
				bool kingside = toSquare.x > fromSquare.x;
				move = CreateCastlingMove(fromSquare, kingside);
			}
			else if (char.ToUpper(piece) == 'P')
			{
				// Check for promotion
				int promotionRank = char.IsUpper(piece) ? 7 : 0;
				if (toSquare.y == promotionRank)
				{
					// Default to queen promotion - could be enhanced with UI selection
					char promotionPiece = char.IsUpper(piece) ? 'Q' : 'q';
					move = new ChessMove(from, to, piece, promotionPiece, capturedPiece);
				}
				// Check for en passant
				else if (capturedPiece == '\0' && fromSquare.x != toSquare.x)
				{
					move.moveType = ChessMove.MoveType.EnPassant;
					move.capturedPiece = char.IsUpper(piece) ? 'p' : 'P';
				}
			}

			// Validate move
			if (!ChessRules.ValidateMove(logicalBoard, move))
			{
				Debug.Log($"[ChessBoard3D] Invalid move: {move}");
				return false;
			}

			// Apply move to logical board
			if (!ChessRules.MakeMove(logicalBoard, move))
			{
				return false;
			}

			// Apply visual changes
			StartCoroutine(ExecuteVisualMove(move));

			// Update UI if connected
			if (chessUI != null)
			{
				chessUI.SetPosition(logicalBoard.ToFEN());
			}

			return true;
		}

		private ChessMove CreateCastlingMove(Vector2Int kingPos, bool kingside)
		{
			char king = logicalBoard.sideToMove == 'w' ? 'K' : 'k';
			char rook = logicalBoard.sideToMove == 'w' ? 'R' : 'r';
			int rank = kingPos.y;

			// Find rook position
			Vector2Int rookPos = new Vector2Int(-1, -1);
			if (kingside)
			{
				for (int file = 7; file > kingPos.x; file--)
				{
					if (logicalBoard.board.GT(new v2(file, rank)) == rook)
					{
						rookPos = new Vector2Int(file, rank);
						break;
					}
				}
			}
			else
			{
				for (int file = 0; file < kingPos.x; file++)
				{
					if (logicalBoard.board.GT(new v2(file, rank)) == rook)
					{
						rookPos = new Vector2Int(file, rank);
						break;
					}
				}
			}

			if (rookPos.x < 0) return new ChessMove(); // Invalid

			Vector2Int kingTarget = new Vector2Int(kingside ? 6 : 2, rank);
			Vector2Int rookTarget = new Vector2Int(kingside ? 5 : 3, rank);

			return new ChessMove(
				new v2(kingPos.x, kingPos.y),
				new v2(kingTarget.x, kingTarget.y),
				new v2(rookPos.x, rookPos.y),
				new v2(rookTarget.x, rookTarget.y),
				king
			);
		}

		private IEnumerator ExecuteVisualMove(ChessMove move)
		{
			// Handle different move types
			switch (move.moveType)
			{
				case ChessMove.MoveType.Castling:
					yield return StartCoroutine(ExecuteCastlingAnimation(move));
					break;

				case ChessMove.MoveType.EnPassant:
					yield return StartCoroutine(ExecuteEnPassantAnimation(move));
					break;

				case ChessMove.MoveType.Promotion:
					yield return StartCoroutine(ExecutePromotionAnimation(move));
					break;

				default:
					yield return StartCoroutine(ExecuteNormalMove(move));
					break;
			}

			// Update piece positions in dictionary
			UpdatePiecePositions();
		}

		private IEnumerator ExecuteNormalMove(ChessMove move)
		{
			Vector2Int from = new Vector2Int(move.from.x, move.from.y);
			Vector2Int to = new Vector2Int(move.to.x, move.to.y);

			if (!pieceControllers.ContainsKey(from))
				yield break;

			ChessPieceController movingPiece = pieceControllers[from];

			// Handle capture
			if (pieceControllers.ContainsKey(to))
			{
				ChessPieceController capturedPiece = pieceControllers[to];
				capturedPiece.SetCaptured();
				pieceControllers.Remove(to);
			}

			// Move piece
			Vector3 targetPos = BoardToWorld(to);
			yield return StartCoroutine(movingPiece.Move(targetPos, move.IsCapture()));

			// Update piece position
			movingPiece.UpdateBoardPosition(to);
			pieceControllers.Remove(from);
			pieceControllers[to] = movingPiece;
		}

		private IEnumerator ExecuteCastlingAnimation(ChessMove move)
		{
			Vector2Int kingFrom = new Vector2Int(move.from.x, move.from.y);
			Vector2Int kingTo = new Vector2Int(move.to.x, move.to.y);
			Vector2Int rookFrom = new Vector2Int(move.rookFrom.x, move.rookFrom.y);
			Vector2Int rookTo = new Vector2Int(move.rookTo.x, move.rookTo.y);

			ChessPieceController king = pieceControllers.ContainsKey(kingFrom) ? pieceControllers[kingFrom] : null;
			ChessPieceController rook = pieceControllers.ContainsKey(rookFrom) ? pieceControllers[rookFrom] : null;

			if (king == null || rook == null) yield break;

			// Move both pieces simultaneously
			Coroutine kingMove = StartCoroutine(king.Move(BoardToWorld(kingTo)));
			Coroutine rookMove = StartCoroutine(rook.Move(BoardToWorld(rookTo)));

			yield return kingMove;
			yield return rookMove;

			// Update positions
			king.UpdateBoardPosition(kingTo);
			rook.UpdateBoardPosition(rookTo);

			pieceControllers.Remove(kingFrom);
			pieceControllers.Remove(rookFrom);
			pieceControllers[kingTo] = king;
			pieceControllers[rookTo] = rook;
		}

		private IEnumerator ExecuteEnPassantAnimation(ChessMove move)
		{
			Vector2Int from = new Vector2Int(move.from.x, move.from.y);
			Vector2Int to = new Vector2Int(move.to.x, move.to.y);

			// Calculate captured pawn position
			int capturedPawnRank = logicalBoard.sideToMove == 'b' ? to.y + 1 : to.y - 1; // Note: sideToMove already switched
			Vector2Int capturedPawnPos = new Vector2Int(to.x, capturedPawnRank);

			ChessPieceController movingPawn = pieceControllers.ContainsKey(from) ? pieceControllers[from] : null;
			ChessPieceController capturedPawn = pieceControllers.ContainsKey(capturedPawnPos) ? pieceControllers[capturedPawnPos] : null;

			if (movingPawn == null) yield break;

			// Move pawn and capture simultaneously
			if (capturedPawn != null)
			{
				capturedPawn.SetCaptured();
				pieceControllers.Remove(capturedPawnPos);
			}

			yield return StartCoroutine(movingPawn.Move(BoardToWorld(to), true));

			movingPawn.UpdateBoardPosition(to);
			pieceControllers.Remove(from);
			pieceControllers[to] = movingPawn;
		}

		private IEnumerator ExecutePromotionAnimation(ChessMove move)
		{
			Vector2Int from = new Vector2Int(move.from.x, move.from.y);
			Vector2Int to = new Vector2Int(move.to.x, move.to.y);

			ChessPieceController pawn = pieceControllers.ContainsKey(from) ? pieceControllers[from] : null;
			if (pawn == null) yield break;

			// Handle capture if any
			if (pieceControllers.ContainsKey(to))
			{
				ChessPieceController capturedPiece = pieceControllers[to];
				capturedPiece.SetCaptured();
				pieceControllers.Remove(to);
			}

			// Remove pawn
			pawn.SetCaptured();
			pieceControllers.Remove(from);

			// Spawn promoted piece
			yield return new WaitForSeconds(0.2f);
			SpawnPiece(move.promotionPiece, to);
		}

		private void UpdatePiecePositions()
		{
			// Sync piece controllers with logical board state
			// This is called after moves to ensure consistency
		}

		#endregion

		#region Visual Feedback

		public void SelectPiece(ChessPieceController piece)
		{
			if (selectedPiece != null && selectedPiece != piece)
			{
				// Deselect previous piece
				HideValidMoves();
			}

			selectedPiece = piece;
			ShowSelectedTile(piece.GetBoardPosition());
		}

		public void ShowValidMoves(Vector2Int fromSquare)
		{
			ClearFeedbackObjects();

			// Generate legal moves for this piece
			List<ChessMove> legalMoves = MoveGenerator.GenerateLegalMoves(logicalBoard);

			foreach (ChessMove move in legalMoves)
			{
				if (move.from.x == fromSquare.x && move.from.y == fromSquare.y)
				{
					Vector2Int targetSquare = new Vector2Int(move.to.x, move.to.y);
					ShowMoveIndicator(targetSquare, move.IsCapture());
				}
			}
		}

		public void HideValidMoves()
		{
			ClearFeedbackObjects();
			selectedPiece = null;
		}

		private void ShowSelectedTile(Vector2Int tilePos)
		{
			if (selectedTilePrefab == null) return;

			Vector3 worldPos = BoardToWorld(tilePos);
			worldPos.y += 0.01f; // Slightly above board

			GameObject indicator = Instantiate(selectedTilePrefab, worldPos, Quaternion.identity, transform);
			feedbackObjects.Add(indicator);
		}

		private void ShowMoveIndicator(Vector2Int tilePos, bool isCapture)
		{
			GameObject prefab = isCapture ? captureTargetPrefab : validMovePrefab;
			if (prefab == null) return;

			Vector3 worldPos = BoardToWorld(tilePos);
			worldPos.y += 0.02f; // Above selected tile indicator

			GameObject indicator = Instantiate(prefab, worldPos, Quaternion.identity, transform);
			feedbackObjects.Add(indicator);
		}

		private void ClearFeedbackObjects()
		{
			foreach (GameObject obj in feedbackObjects)
			{
				if (obj != null)
					DestroyImmediate(obj);
			}
			feedbackObjects.Clear();
		}

		#endregion

		#region Coordinate Conversion

		public Vector3 BoardToWorld(Vector2Int boardPos)
		{
			float x = boardOrigin.x + (boardPos.x * tileSize);
			float z = boardOrigin.z + (boardPos.y * tileSize);

			if (flipBoard)
			{
				x = boardOrigin.x + ((7 - boardPos.x) * tileSize);
				z = boardOrigin.z + ((7 - boardPos.y) * tileSize);
			}

			return new Vector3(x, boardOrigin.y, z);
		}

		public Vector2Int WorldToBoard(Vector3 worldPos)
		{
			float relativeX = worldPos.x - boardOrigin.x;
			float relativeZ = worldPos.z - boardOrigin.z;

			int boardX = Mathf.RoundToInt(relativeX / tileSize);
			int boardY = Mathf.RoundToInt(relativeZ / tileSize);

			if (flipBoard)
			{
				boardX = 7 - boardX;
				boardY = 7 - boardY;
			}

			return new Vector2Int(boardX, boardY);
		}

		#endregion

		#region Public Interface

		public bool CanPlayerMovePiece(char pieceColor)
		{
			return logicalBoard.sideToMove == pieceColor;
		}

		public void SetPosition(string fen)
		{
			logicalBoard = new ChessBoard(fen);
			SpawnInitialPieces();
		}

		public string GetCurrentFEN()
		{
			return logicalBoard.ToFEN();
		}

		public void FlipBoard()
		{
			flipBoard = !flipBoard;
			SpawnInitialPieces(); // Respawn pieces in new orientation
		}

		#endregion
	}
}