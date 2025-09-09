/*
 * CHANGE LOG:
 * v1.0.0 - [2025-01-09] Initial implementation
 *   - FEN parsing with board position extraction
 *   - GameObject instantiation from prefab lookup
 *   - Chess coordinate system (a1=0,0 to h8=7,7)
 *   - UI integration with TMP_InputField and Button
 *   - Comprehensive error handling and validation
 *   - Unity 2020.3 compatibility (.NET 2.0 subset)
 *   - WebGL build considerations
 * 
 * REQUIREMENTS:
 * - Parse FEN board position (ignore game state)
 * - Support all 6 piece types in both colors (12 total)
 * - Handle empty squares with numeric notation
 * - Validate FEN format with user feedback
 * - Clean prefab instantiation with proper cleanup
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SPACE_UTIL;

namespace GptDeepResearch
{
	[System.Serializable]
	public class ChessPieceConfig
	{
		[Header("Chess Piece Prefabs")]
		[Tooltip("Drag GameObject prefabs following naming: chess-piece-{type}-{color}")]
		[SerializeField] public List<GameObject> chessPiecePrefabs = new List<GameObject>(12);

		[Header("UI Components")]
		[SerializeField] public TMP_InputField fenInputField;
		[SerializeField] public Button generateBoardButton;
		[SerializeField] public TMP_InputField statusText;

		[Header("Board Configuration")]
		[SerializeField] public Transform boardParent;
		[SerializeField] public Vector3 squareSize = Vector3.one;
		[SerializeField] public Vector3 boardOrigin = Vector3.zero;
	}

	public class ChessFENViewer : MonoBehaviour
	{
		[SerializeField] private ChessPieceConfig config;

		// Private state management
		private readonly Dictionary<char, GameObject> piecePrefabLookup = new Dictionary<char, GameObject>();
		private readonly List<GameObject> activePieces = new List<GameObject>();
		private Board<char> chessBoard;
		private string currentStatus = "Ready";

		// FEN piece mapping
		private readonly Dictionary<char, string> pieceToKey = new Dictionary<char, string>
		{
			{'p', "chess-piece-p-b"}, {'P', "chess-piece-p-w"}, // Pawns
            {'r', "chess-piece-r-b"}, {'R', "chess-piece-r-w"}, // Rooks  
            {'n', "chess-piece-n-b"}, {'N', "chess-piece-n-w"}, // Knights
            {'b', "chess-piece-b-b"}, {'B', "chess-piece-b-w"}, // Bishops
            {'q', "chess-piece-q-b"}, {'Q', "chess-piece-q-w"}, // Queens
            {'k', "chess-piece-k-b"}, {'K', "chess-piece-k-w"}  // Kings
        };

		#region Unity Lifecycle

		private void Awake()
		{
			InitializeChessBoard();
			ValidateInspectorReferences();
			BuildPrefabLookup();
		}

		private void Start()
		{
			if (config.generateBoardButton != null)
			{
				config.generateBoardButton.onClick.AddListener(GenerateBoardFromFEN);
			}

			UpdateStatus("Chess FEN Viewer Ready");
			Debug.Log("<color=cyan>[ChessFENViewer] System initialized and ready</color>");

			#region frame_rate
			Application.targetFrameRate = 20;
			#endregion
		}

		private void OnDestroy()
		{
			ClearBoard();
		}

		#endregion

		#region Public API

		/// <summary>
		/// Main public method to generate chess board from FEN string
		/// </summary>
		public void GenerateBoardFromFEN()
		{
			string fenString = GetFENInput();

			if (string.IsNullOrEmpty(fenString))
			{
				UpdateStatus("Error: Empty FEN input");
				Debug.Log("<color=red>[ChessFENViewer] Empty FEN input provided</color>");
				return;
			}

			Debug.Log("<color=yellow>[ChessFENViewer] Processing FEN: " + fenString + "</color>");

			if (!ValidateFENFormat(fenString))
			{
				UpdateStatus("Error: Invalid FEN format");
				return;
			}

			ClearBoard();

			string boardPosition = ExtractBoardPosition(fenString);
			if (!ProcessBoardPosition(boardPosition))
			{
				UpdateStatus("Error: Failed to process board position");
				return;
			}

			UpdateStatus("Board generated successfully");
			Debug.Log("<color=green>[ChessFENViewer] Board generated with " + activePieces.Count + " pieces</color>");
		}

		/// <summary>
		/// Clear all pieces from the board
		/// </summary>
		public void ClearBoard()
		{
			foreach (GameObject piece in activePieces)
			{
				if (piece != null)
				{
#if UNITY_WEBGL && !UNITY_EDITOR
                    Destroy(piece);
#else
					DestroyImmediate(piece);
#endif
				}
			}

			activePieces.Clear();

			if (chessBoard != null)
			{
				// Reset board to empty
				for (int file = 0; file < 8; file++)
				{
					for (int rank = 0; rank < 8; rank++)
					{
						chessBoard.ST(new v2(file, rank), ' ');
					}
				}
			}

			Debug.Log("<color=yellow>[ChessFENViewer] Board cleared</color>");
		}

		/// <summary>
		/// Run comprehensive test suite
		/// </summary>
		public static void RunAllTests()
		{
			Debug.Log("<color=cyan>[ChessFENViewer] ======= STARTING TEST SUITE =======</color>");

			TestFENValidation();
			TestCoordinateMapping();
			TestPrefabLookup();
			TestEdgeCasesAndErrors();

			Debug.Log("<color=cyan>[ChessFENViewer] ======= TEST SUITE COMPLETED =======</color>");
		}

		#endregion

		#region Private Implementation

		private void InitializeChessBoard()
		{
			chessBoard = new Board<char>(new v2(8, 8), ' ');
			Debug.Log("<color=green>[ChessFENViewer] Chess board initialized (8x8)</color>");
		}

		private void ValidateInspectorReferences()
		{
			bool hasErrors = false;

			if (config.fenInputField == null)
			{
				Debug.Log("<color=red>[ChessFENViewer] Missing FEN Input Field reference</color>");
				hasErrors = true;
			}

			if (config.generateBoardButton == null)
			{
				Debug.Log("<color=red>[ChessFENViewer] Missing Generate Button reference</color>");
				hasErrors = true;
			}

			if (config.statusText == null)
			{
				Debug.Log("<color=yellow>[ChessFENViewer] Missing Status Text reference (optional)</color>");
			}

			if (config.boardParent == null)
			{
				Debug.Log("<color=yellow>[ChessFENViewer] Missing Board Parent reference, using this transform</color>");
				config.boardParent = this.transform;
			}

			if (config.chessPiecePrefabs == null || config.chessPiecePrefabs.Count == 0)
			{
				Debug.Log("<color=red>[ChessFENViewer] No chess piece prefabs assigned</color>");
				hasErrors = true;
			}

			if (hasErrors)
			{
				Debug.Log("<color=red>[ChessFENViewer] Critical references missing - functionality may be limited</color>");
			}
		}

		private void BuildPrefabLookup()
		{
			piecePrefabLookup.Clear();
			int foundPrefabs = 0;

			foreach (var prefab in config.chessPiecePrefabs)
			{
				if (prefab == null) continue;

				string prefabName = prefab.name;
				char? pieceChar = GetPieceCharFromName(prefabName);

				if (pieceChar.HasValue)
				{
					piecePrefabLookup[pieceChar.Value] = prefab;
					foundPrefabs++;
					Debug.Log("<color=green>[ChessFENViewer] Mapped prefab: " + prefabName + " → '" + pieceChar.Value + "'</color>");
				}
				else
				{
					Debug.Log("<color=yellow>[ChessFENViewer] Unknown prefab name format: " + prefabName + "</color>");
				}
			}

			Debug.Log("<color=cyan>[ChessFENViewer] Prefab lookup built: " + foundPrefabs + "/12 pieces mapped</color>");
		}

		private char? GetPieceCharFromName(string prefabName)
		{
			foreach (var kvp in pieceToKey)
			{
				if (prefabName == kvp.Value)
				{
					return kvp.Key;
				}
			}
			return null;
		}

		private string GetFENInput()
		{
			if (config.fenInputField != null)
			{
				return config.fenInputField.text.Trim();
			}

			Debug.Log("<color=red>[ChessFENViewer] FEN input field not assigned</color>");
			return string.Empty;
		}

		private void UpdateStatus(string message)
		{
			currentStatus = message;

			if (config.statusText != null)
			{
				config.statusText.text = message;
			}

			Debug.Log("<color=blue>[ChessFENViewer] Status: " + message + "</color>");
		}

		private bool ValidateFENFormat(string fen)
		{
			if (string.IsNullOrEmpty(fen))
			{
				Debug.Log("<color=red>[ChessFENViewer] FEN validation failed: null or empty</color>");
				return false;
			}

			string[] parts = fen.Split(' ');
			if (parts.Length < 1)
			{
				Debug.Log("<color=red>[ChessFENViewer] FEN validation failed: no board position</color>");
				return false;
			}

			string boardPosition = parts[0];
			string[] ranks = boardPosition.Split('/');

			if (ranks.Length != 8)
			{
				Debug.Log("<color=red>[ChessFENViewer] FEN validation failed: expected 8 ranks, got " + ranks.Length + "</color>");
				return false;
			}

			for (int i = 0; i < ranks.Length; i++)
			{
				if (!ValidateRank(ranks[i], i))
				{
					return false;
				}
			}

			Debug.Log("<color=green>[ChessFENViewer] FEN validation passed</color>");
			return true;
		}

		private bool ValidateRank(string rank, int rankIndex)
		{
			if (string.IsNullOrEmpty(rank))
			{
				Debug.Log("<color=red>[ChessFENViewer] Rank " + (rankIndex + 1) + " is empty</color>");
				return false;
			}

			int squareCount = 0;

			foreach (char c in rank)
			{
				if (char.IsDigit(c))
				{
					int emptySquares = c - '0';
					if (emptySquares < 1 || emptySquares > 8)
					{
						Debug.Log("<color=red>[ChessFENViewer] Invalid empty square count '" + c + "' in rank " + (rankIndex + 1) + "</color>");
						return false;
					}
					squareCount += emptySquares;
				}
				else if (pieceToKey.ContainsKey(c))
				{
					squareCount += 1;
				}
				else
				{
					Debug.Log("<color=red>[ChessFENViewer] Invalid character '" + c + "' in rank " + (rankIndex + 1) + "</color>");
					return false;
				}
			}

			if (squareCount != 8)
			{
				Debug.Log("<color=red>[ChessFENViewer] Rank " + (rankIndex + 1) + " has " + squareCount + " squares, expected 8</color>");
				return false;
			}

			return true;
		}

		private string ExtractBoardPosition(string fen)
		{
			string[] parts = fen.Split(' ');
			return parts[0]; // First part is always the board position
		}

		private bool ProcessBoardPosition(string boardPosition)
		{
			string[] ranks = boardPosition.Split('/');

			// Process ranks from top (8th rank) to bottom (1st rank)
			for (int rankIndex = 0; rankIndex < 8; rankIndex++)
			{
				int boardRank = 7 - rankIndex; // Convert to chess coordinate (rank 8 = board index 7)

				if (!ProcessRank(ranks[rankIndex], boardRank))
				{
					Debug.Log("<color=red>[ChessFENViewer] Failed to process rank " + (rankIndex + 1) + "</color>");
					return false;
				}
			}

			return true;
		}

		private bool ProcessRank(string rank, int boardRank)
		{
			int file = 0; // Start at 'a' file (index 0)

			foreach (char c in rank)
			{
				if (char.IsDigit(c))
				{
					// Skip empty squares
					int emptySquares = c - '0';
					file += emptySquares;
				}
				else
				{
					// Place piece
					if (!PlacePiece(c, file, boardRank))
					{
						return false;
					}
					file++;
				}

				if (file > 8)
				{
					Debug.Log("<color=red>[ChessFENViewer] File overflow in rank processing</color>");
					return false;
				}
			}

			return true;
		}

		private bool PlacePiece(char piece, int file, int rank)
		{
			if (!piecePrefabLookup.ContainsKey(piece))
			{
				Debug.Log("<color=red>[ChessFENViewer] No prefab found for piece '" + piece + "'</color>");
				return false;
			}

			GameObject prefab = piecePrefabLookup[piece];
			Vector3 worldPosition = ChessToWorldPosition(file, rank);

			GameObject pieceInstance = Instantiate(prefab, worldPosition, Quaternion.identity, config.boardParent);
			pieceInstance.name = prefab.name + "_" + GetSquareName(file, rank);

			activePieces.Add(pieceInstance);
			chessBoard.ST(new v2(file, rank), piece);

			Debug.Log("<color=green>[ChessFENViewer] Placed " + piece + " at " + GetSquareName(file, rank) + " (world: " + worldPosition + ")</color>");
			return true;
		}

		private Vector3 ChessToWorldPosition(int file, int rank)
		{
			return config.boardOrigin + new Vector3(
				file * config.squareSize.x,        // Files: a=0, b=1, ..., h=7
				rank * config.squareSize.y,        // Ranks: 1=0, 2=1, ..., 8=7
				0f                                 // 2D board (z=0)
			);
		}

		private string GetSquareName(int file, int rank)
		{
			char fileChar = (char)('a' + file);
			int rankNumber = rank + 1;
			return fileChar.ToString() + rankNumber.ToString();
		}

		#endregion

		#region Testing Framework

		private static void TestFENValidation()
		{
			Debug.Log("<color=cyan>[ChessFENViewer] --- Testing FEN Validation ---</color>");

			string[] validFENs = {
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
				"rnbqkb1r/pppp1ppp/5n2/4p3/2B1P3/8/PPPP1PPP/RNBQK1NR w KQkq - 4 4",
				"8/8/8/8/8/8/8/8 w - - 0 1"
			};

			string[] invalidFENs = {
				"",
				"invalid",
				"too/many/ranks/here/1/2/3/4/5",
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP", // Missing rank
                "rnbqkbnr/pppppppp/9/8/8/8/PPPPPPPP/RNBQKBNR", // Invalid empty square count
                "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNx" // Invalid piece
            };

			// Create temporary instance for testing
			GameObject tempGO = new GameObject("TestFENViewer");
			ChessFENViewer testViewer = tempGO.AddComponent<ChessFENViewer>();
			testViewer.config = new ChessPieceConfig();
			testViewer.InitializeChessBoard();

			foreach (string fen in validFENs)
			{
				bool result = testViewer.ValidateFENFormat(fen);
				Debug.Log(result ?
					"<color=green>[ChessFENViewer] ✓ Valid FEN accepted: " + fen.Substring(0, System.Math.Min(30, fen.Length)) + "...</color>" :
					"<color=red>[ChessFENViewer] ✗ Valid FEN rejected: " + fen.Substring(0, System.Math.Min(30, fen.Length)) + "...</color>");
			}

			foreach (string fen in invalidFENs)
			{
				bool result = testViewer.ValidateFENFormat(fen);
				Debug.Log(!result ?
					"<color=green>[ChessFENViewer] ✓ Invalid FEN rejected: " + (string.IsNullOrEmpty(fen) ? "(empty)" : fen.Substring(0, System.Math.Min(20, fen.Length))) + "</color>" :
					"<color=red>[ChessFENViewer] ✗ Invalid FEN accepted: " + (string.IsNullOrEmpty(fen) ? "(empty)" : fen.Substring(0, System.Math.Min(20, fen.Length))) + "</color>");
			}

			DestroyImmediate(tempGO);
		}

		private static void TestCoordinateMapping()
		{
			Debug.Log("<color=cyan>[ChessFENViewer] --- Testing Coordinate Mapping ---</color>");

			// Create temporary instance for testing
			GameObject tempGO = new GameObject("TestFENViewer");
			ChessFENViewer testViewer = tempGO.AddComponent<ChessFENViewer>();
			testViewer.config = new ChessPieceConfig();

			Vector3 a1 = testViewer.ChessToWorldPosition(0, 0); // a1 = (0,0)
			Vector3 h1 = testViewer.ChessToWorldPosition(7, 0); // h1 = (7,0)
			Vector3 a8 = testViewer.ChessToWorldPosition(0, 7); // a8 = (0,7)
			Vector3 h8 = testViewer.ChessToWorldPosition(7, 7); // h8 = (7,7)

			bool a1Test = a1 == new Vector3(0, 0, 0);
			bool h1Test = h1 == new Vector3(7, 0, 0);
			bool a8Test = a8 == new Vector3(0, 7, 0);
			bool h8Test = h8 == new Vector3(7, 7, 0);

			Debug.Log(a1Test ?
				"<color=green>[ChessFENViewer] ✓ a1 coordinate correct: " + a1 + "</color>" :
				"<color=red>[ChessFENViewer] ✗ a1 coordinate failed: " + a1 + "</color>");

			Debug.Log(h8Test ?
				"<color=green>[ChessFENViewer] ✓ h8 coordinate correct: " + h8 + "</color>" :
				"<color=red>[ChessFENViewer] ✗ h8 coordinate failed: " + h8 + "</color>");

			bool allCoordTests = a1Test && h1Test && a8Test && h8Test;
			Debug.Log(allCoordTests ?
				"<color=green>[ChessFENViewer] ✓ All coordinate mapping tests passed</color>" :
				"<color=red>[ChessFENViewer] ✗ Some coordinate mapping tests failed</color>");

			DestroyImmediate(tempGO);
		}

		private static void TestPrefabLookup()
		{
			Debug.Log("<color=cyan>[ChessFENViewer] --- Testing Prefab Lookup ---</color>");

			char[] allPieces = { 'p', 'r', 'n', 'b', 'q', 'k', 'P', 'R', 'N', 'B', 'Q', 'K' };

			// Create temporary instance for testing
			GameObject tempGO = new GameObject("TestFENViewer");
			ChessFENViewer testViewer = tempGO.AddComponent<ChessFENViewer>();
			testViewer.config = new ChessPieceConfig();

			// Test piece key mapping
			int mappedKeys = 0;
			foreach (char piece in allPieces)
			{
				if (testViewer.pieceToKey.ContainsKey(piece))
				{
					string expectedKey = testViewer.pieceToKey[piece];
					mappedKeys++;
					Debug.Log("<color=green>[ChessFENViewer] ✓ Piece '" + piece + "' maps to: " + expectedKey + "</color>");
				}
			}

			Debug.Log(mappedKeys == 12 ?
				"<color=green>[ChessFENViewer] ✓ All 12 piece keys mapped correctly</color>" :
				"<color=red>[ChessFENViewer] ✗ Missing piece key mappings: " + (12 - mappedKeys) + "</color>");

			DestroyImmediate(tempGO);
		}

		private static void TestEdgeCasesAndErrors()
		{
			Debug.Log("<color=cyan>[ChessFENViewer] --- Testing Edge Cases and Errors ---</color>");

			string[] edgeCases = { "", null, "invalid", "too/many/ranks/here/1/2/3/4/5" };

			// Create temporary instance for testing
			GameObject tempGO = new GameObject("TestFENViewer");
			ChessFENViewer testViewer = tempGO.AddComponent<ChessFENViewer>();
			testViewer.config = new ChessPieceConfig();
			testViewer.InitializeChessBoard();

			foreach (string testCase in edgeCases)
			{
				bool handledGracefully = !testViewer.ValidateFENFormat(testCase);
				string caseDesc = testCase ?? "(null)";
				if (string.IsNullOrEmpty(caseDesc)) caseDesc = "(empty)";

				Debug.Log(handledGracefully ?
					"<color=green>[ChessFENViewer] ✓ Edge case handled gracefully: " + caseDesc + "</color>" :
					"<color=red>[ChessFENViewer] ✗ Edge case not handled: " + caseDesc + "</color>");
			}

			DestroyImmediate(tempGO);
		}

		#endregion

		#region ToString Override

		public override string ToString()
		{
			return "[ChessFENViewer] Pieces: " + activePieces.Count + ", Status: " + currentStatus;
		}

		#endregion
	}
}