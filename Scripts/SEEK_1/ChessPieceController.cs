/*
CHANGELOG (New File):
- 3D chess piece controller with drag and drop functionality
- Hover and selection visual feedback
- Animation state machine with customizable hooks
- Path preview using LineRenderer during drag
- Integration with existing ChessRules for move validation
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPTDeepResearch
{
	/// <summary>
	/// Controls individual chess pieces in 3D space with drag/drop functionality.
	/// Integrates with ChessBoard3D for board management and move validation.
	/// </summary>
	public class ChessPieceController : MonoBehaviour
	{
		[Header("Piece Data")]
		[SerializeField] private char pieceType = 'P'; // Will be set by ChessBoard3D
		[SerializeField] private char pieceColor = 'w'; // Will be set by ChessBoard3D
		[SerializeField] private Vector2Int boardPosition; // Current board coordinates

		[Header("Visual Settings")]
		[SerializeField] private Material normalMaterial;
		[SerializeField] private Material hoverMaterial;
		[SerializeField] private Material selectedMaterial;
		[SerializeField] private float hoverHeight = 0.1f;
		[SerializeField] private float selectedHeight = 0.2f;

		[Header("Animation Settings")]
		[SerializeField] private float moveSpeedPerTile = 0.2f;
		[SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
		[SerializeField] private Animator pieceAnimator;

		[Header("Drag Preview")]
		[SerializeField] private LineRenderer pathPreview;
		[SerializeField] private int pathResolution = 20;
		[SerializeField] private float pathHeight = 0.5f;

		// State
		private ChessBoard3D chessBoard3D;
		private Vector3 originalPosition;
		private Vector3 originalScale;
		private bool isDragging = false;
		private bool isSelected = false;
		private bool isAnimating = false;
		private Camera mainCamera;
		private Renderer pieceRenderer;

		// Animation state
		public enum PieceState { Idle, Moving, Attacking, Captured }
		[SerializeField] private PieceState currentState = PieceState.Idle;

		void Awake()
		{
			pieceRenderer = GetComponent<Renderer>();
			mainCamera = Camera.main;
			if (mainCamera == null)
				mainCamera = FindObjectOfType<Camera>();

			originalScale = transform.localScale;

			// Setup path preview
			if (pathPreview == null)
			{
				pathPreview = GetComponent<LineRenderer>();
				if (pathPreview == null)
				{
					GameObject pathObject = new GameObject("PathPreview");
					pathObject.transform.SetParent(transform);
					pathPreview = pathObject.AddComponent<LineRenderer>();
				}
			}

			SetupPathPreview();
		}

		void Start()
		{
			chessBoard3D = FindObjectOfType<ChessBoard3D>();
			originalPosition = transform.position;

			// Set initial state
			SetState(PieceState.Idle);
			SetVisualState(false, false);
		}

		void SetupPathPreview()
		{
			if (pathPreview == null) return;

			pathPreview.material = new Material(Shader.Find("Sprites/Default"));
			pathPreview.startColor = pathPreview.endColor = new Color(1f, 1f, 0f, 0.6f); // Yellow with transparency
			pathPreview.startWidth = 0.05f;
			pathPreview.endWidth = 0.05f;
			pathPreview.positionCount = 0;
			pathPreview.useWorldSpace = true;
			pathPreview.enabled = false;
		}

		#region Mouse Events

		void OnMouseEnter()
		{
			if (!CanInteract()) return;
			SetVisualState(true, isSelected);
		}

		void OnMouseExit()
		{
			if (!CanInteract() || isDragging) return;
			SetVisualState(false, isSelected);
		}

		void OnMouseDown()
		{
			if (!CanInteract()) return;

			if (chessBoard3D != null)
			{
				chessBoard3D.SelectPiece(this);
			}

			isSelected = true;
			SetVisualState(true, true);
			StartDrag();
		}

		void OnMouseDrag()
		{
			if (!isDragging || !CanInteract()) return;
			UpdateDrag();
		}

		void OnMouseUp()
		{
			if (!isDragging) return;
			EndDrag();
		}

		#endregion

		#region Drag and Drop

		private void StartDrag()
		{
			if (isAnimating) return;

			isDragging = true;
			originalPosition = transform.position;

			// Lift piece
			Vector3 liftedPos = originalPosition + Vector3.up * selectedHeight;
			transform.position = liftedPos;

			// Enable path preview
			if (pathPreview != null)
				pathPreview.enabled = true;

			// Show valid move indicators
			if (chessBoard3D != null)
				chessBoard3D.ShowValidMoves(boardPosition);

			// Animation hook
			StartMove();
		}

		private void UpdateDrag()
		{
			if (mainCamera == null) return;

			// Get mouse position in world space
			Vector3 mousePos = Input.mousePosition;
			mousePos.z = mainCamera.WorldToScreenPoint(transform.position).z;
			Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

			// Keep piece at hover height
			worldPos.y = originalPosition.y + selectedHeight;
			transform.position = worldPos;

			// Update path preview
			if (chessBoard3D != null)
			{
				Vector2Int targetTile = chessBoard3D.WorldToBoard(worldPos);
				UpdatePathPreview(targetTile);
			}
		}

		private void EndDrag()
		{
			isDragging = false;
			isSelected = false;

			// Hide path preview
			if (pathPreview != null)
				pathPreview.enabled = false;

			// Hide move indicators
			if (chessBoard3D != null)
				chessBoard3D.HideValidMoves();

			// Get target square
			Vector3 currentPos = transform.position;
			Vector2Int targetSquare = chessBoard3D != null ?
				chessBoard3D.WorldToBoard(currentPos) :
				new Vector2Int(-1, -1);

			// Try to make the move
			bool moveSuccessful = false;
			if (chessBoard3D != null && targetSquare.x >= 0 && targetSquare.y >= 0)
			{
				moveSuccessful = chessBoard3D.TryMakeMove(boardPosition, targetSquare);
			}

			if (!moveSuccessful)
			{
				// Snap back to original position
				StartCoroutine(AnimateToPosition(originalPosition, moveSpeedPerTile));
			}

			SetVisualState(false, false);
		}

		#endregion

		#region Path Preview

		private void UpdatePathPreview(Vector2Int targetTile)
		{
			if (pathPreview == null || chessBoard3D == null) return;

			// Generate path based on piece type
			List<Vector3> pathPoints = GeneratePathToTarget(targetTile);

			if (pathPoints.Count < 2)
			{
				pathPreview.positionCount = 0;
				return;
			}

			pathPreview.positionCount = pathPoints.Count;
			for (int i = 0; i < pathPoints.Count; i++)
			{
				pathPreview.SetPosition(i, pathPoints[i]);
			}
		}

		private List<Vector3> GeneratePathToTarget(Vector2Int targetTile)
		{
			List<Vector3> points = new List<Vector3>();

			if (targetTile.x < 0 || targetTile.x > 7 || targetTile.y < 0 || targetTile.y > 7)
				return points;

			Vector3 start = chessBoard3D.BoardToWorld(boardPosition);
			Vector3 end = chessBoard3D.BoardToWorld(targetTile);

			// Adjust heights
			start.y += pathHeight;
			end.y += pathHeight;

			char pieceUpper = char.ToUpper(pieceType);

			// Generate path based on piece type
			if (pieceUpper == 'N') // Knight - L-shaped path
			{
				Vector3 corner = new Vector3(
					Math.Abs(targetTile.x - boardPosition.x) > Math.Abs(targetTile.y - boardPosition.y) ? end.x : start.x,
					start.y + 0.5f, // Higher arc for knight
					Math.Abs(targetTile.x - boardPosition.x) > Math.Abs(targetTile.y - boardPosition.y) ? start.z : end.z
				);

				points.Add(start);
				points.Add(corner);
				points.Add(end);
			}
			else // Straight line for other pieces
			{
				// Simple arc for visual appeal
				Vector3 midPoint = Vector3.Lerp(start, end, 0.5f);
				midPoint.y += 0.3f;

				// Interpolate along curve
				for (int i = 0; i <= pathResolution; i++)
				{
					float t = (float)i / pathResolution;
					Vector3 point = CalculateBezierPoint(start, midPoint, end, t);
					points.Add(point);
				}
			}

			return points;
		}

		private Vector3 CalculateBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
		{
			float u = 1 - t;
			return u * u * p0 + 2 * u * t * p1 + t * t * p2;
		}

		#endregion

		#region Animation and State

		public void SetState(PieceState newState)
		{
			if (currentState == newState) return;

			currentState = newState;

			// Trigger animator if available
			if (pieceAnimator != null)
			{
				switch (currentState)
				{
					case PieceState.Idle:
						if (pieceAnimator.HasState(0, Animator.StringToHash("Idle")))
							pieceAnimator.SetTrigger("Idle");
						break;
					case PieceState.Moving:
						if (pieceAnimator.HasState(0, Animator.StringToHash("Move")))
							pieceAnimator.SetTrigger("Move");
						break;
					case PieceState.Attacking:
						if (pieceAnimator.HasState(0, Animator.StringToHash("Attack")))
							pieceAnimator.SetTrigger("Attack");
						break;
					case PieceState.Captured:
						if (pieceAnimator.HasState(0, Animator.StringToHash("Captured")))
							pieceAnimator.SetTrigger("Captured");
						break;
				}
			}
		}

		public void StartMove()
		{
			SetState(PieceState.Moving);
			// Animation hook for anticipation
		}

		public IEnumerator Move(Vector3 targetPosition, bool isCapture = false)
		{
			isAnimating = true;
			Vector3 startPos = transform.position;

			if (isCapture)
				SetState(PieceState.Attacking);
			else
				SetState(PieceState.Moving);

			float distance = Vector3.Distance(startPos, targetPosition);
			float duration = distance / (1f / moveSpeedPerTile);

			float elapsedTime = 0f;
			while (elapsedTime < duration)
			{
				float t = elapsedTime / duration;
				float curveValue = moveCurve.Evaluate(t);

				Vector3 currentPos = Vector3.Lerp(startPos, targetPosition, curveValue);
				transform.position = currentPos;

				elapsedTime += Time.deltaTime;
				yield return null;
			}

			transform.position = targetPosition;
			isAnimating = false;

			EndMove();
		}

		public void EndMove()
		{
			SetState(PieceState.Idle);
			// Animation hook for landing
		}

		public IEnumerator AnimateToPosition(Vector3 targetPosition, float duration)
		{
			yield return StartCoroutine(Move(targetPosition));
		}

		#endregion

		#region Visual State

		private void SetVisualState(bool hover, bool selected)
		{
			if (pieceRenderer == null) return;

			Material targetMaterial = normalMaterial;
			Vector3 targetScale = originalScale;

			if (selected && selectedMaterial != null)
			{
				targetMaterial = selectedMaterial;
				targetScale = originalScale * 1.1f;
			}
			else if (hover && hoverMaterial != null)
			{
				targetMaterial = hoverMaterial;
				targetScale = originalScale * 1.05f;
			}

			if (targetMaterial != null)
				pieceRenderer.material = targetMaterial;

			transform.localScale = targetScale;
		}

		#endregion

		#region Public Interface

		public void Initialize(char piece, char color, Vector2Int position, ChessBoard3D board)
		{
			pieceType = piece;
			pieceColor = color;
			boardPosition = position;
			chessBoard3D = board;

			originalPosition = transform.position;
		}

		public void UpdateBoardPosition(Vector2Int newPosition)
		{
			boardPosition = newPosition;
		}

		public Vector2Int GetBoardPosition()
		{
			return boardPosition;
		}

		public char GetPieceType()
		{
			return pieceType;
		}

		public char GetPieceColor()
		{
			return pieceColor;
		}

		public bool CanInteract()
		{
			return chessBoard3D != null &&
				   chessBoard3D.CanPlayerMovePiece(pieceColor) &&
				   !isAnimating;
		}

		public void SetCaptured()
		{
			SetState(PieceState.Captured);
			// Move off board or disable
			StartCoroutine(AnimateCaptured());
		}

		private IEnumerator AnimateCaptured()
		{
			Vector3 targetPos = transform.position + Vector3.down * 2f;
			yield return StartCoroutine(Move(targetPos));
			gameObject.SetActive(false);
		}

		#endregion
	}
}