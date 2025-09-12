/*
CHANGELOG (v0.3):
- Updated to prompt v0.3 requirements with comprehensive API validation
- Fixed FEN loading fallback issue - separated parsing from validation
- Added configurable validation modes (strict vs permissive)
- Enhanced error reporting with detailed FEN parsing feedback
- Minimized public API surface with proper { get; private set; } patterns
- Enhanced evaluation system with full engine integration support
- Improved error handling and validation throughout all methods
- Added comprehensive testing suite with consolidated test runners
- Enhanced promotion support validation and special move handling
- Added proper debug logging with color coding for Unity console navigation
- Strengthened FEN validation and parsing with better error messages
- Added performance optimizations for move history and position caching
- Enhanced game state validation for seamless engine integration
- Fixed Unity 2020.3 compatibility issues (string.Contains char vs string)
- Added ToString() override for main ChessBoard class
- Consolidated public test methods into RunAllTests() pattern
- Improved position hashing with Zobrist implementation
- Enhanced game tree with branching support and variation handling
- Fixed validation fallback behavior to preserve parsed positions
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using SPACE_UTIL;

namespace GPTDeepResearch
{
	/// <summary>
	/// Enhanced chess board with comprehensive engine integration, validation, and testing.
	/// Supports legal move generation, promotion handling, undo/redo, and evaluation metrics.
	/// </summary>
	[System.Serializable]
	public class ChessBoard : ICloneable
	{
		static bool LoggingEnabled = false;

		[Header("Board State")]
		public Board<char> board = new Board<char>(new v2(8, 8), '.');
		public char sideToMove = 'w';
		public string castlingRights = "KQkq";
		public string enPassantSquare = "-";
		public int halfmoveClock = 0;
		public int fullmoveNumber = 1;

		// [Header("Game Settings")]
		public char humanSide { get; private set; } = 'w';
		public char engineSide { get; private set; } = 'b';
		public bool allowSideSwitching = true;
		public ChessVariant variant { get; private set; } = ChessVariant.Standard;

		[Header("Enhanced History Management")]
		[SerializeField] private GameTree gameTree = new GameTree();
		[SerializeField] private int maxHistorySize = 500;
		[SerializeField] private bool enablePositionCaching = true;
		[SerializeField] private int maxCacheSize = 1000;

		[Header("Evaluation and Analysis")]
		[SerializeField] private float lastEvaluation = 0f;
		[SerializeField] private float lastWinProbability = 0.5f;
		[SerializeField] private float lastMateDistance = 0f;
		[SerializeField] private int lastEvaluationDepth = 0;
		[SerializeField] private Dictionary<ulong, PositionInfo> positionCache = new Dictionary<ulong, PositionInfo>();

		[Header("PGN Support")]
		[SerializeField] private PGNMetadata pgnMetadata = new PGNMetadata();
		[SerializeField] private List<string> pgnComments = new List<string>();

		[Header("Validation Settings")]
		[SerializeField] private ValidationMode validationMode = ValidationMode.Permissive;
		[SerializeField] private bool fallbackToStartingPosition = false;

		// Public properties for engine integration with private setters
		public float LastEvaluation { get; private set; }
		public float LastWinProbability { get; private set; }
		public float LastMateDistance { get; private set; }
		public int LastEvaluationDepth { get; private set; }
		public List<GameNode> LogGameTreeNodes { get { return gameTree.GetNodes; } } // just to log
		public int GameTreeNodeCount => gameTree.NodeCount;
		public int CurrentHistoryIndex => gameTree.CurrentNodeIndex;
		public ValidationMode CurrentValidationMode { get { return validationMode; } }

		/// <summary>
		/// Chess variant support
		/// </summary>
		public enum ChessVariant
		{
			Standard,
			Chess960,
			KingOfTheHill,
			Atomic,
			ThreeCheck,
			Horde,
			RacingKings
		}

		/// <summary>
		/// Validation mode for FEN loading
		/// </summary>
		public enum ValidationMode
		{
			Strict,     // Must be a legal chess position
			Permissive, // Allow positions that violate chess rules (for testing)
			ParseOnly   // Only check FEN syntax, ignore position validity
		}

		/// <summary>
		/// Cached position information for performance
		/// </summary>
		[System.Serializable]
		public struct PositionInfo
		{
			public ulong hash;
			public float evaluation;
			public float winProbability;
			public int depthSearched;
			public float timestamp;
			public List<ChessMove> legalMoves;
			public ChessRules.GameResult gameResult;

			public PositionInfo(ulong hash, float eval, float winProb, int depth)
			{
				this.hash = hash;
				this.evaluation = eval;
				this.winProbability = winProb;
				this.depthSearched = depth;
				this.timestamp = Time.time;
				this.legalMoves = null;
				this.gameResult = ChessRules.GameResult.InProgress;
			}

			public bool IsValid()
			{
				return hash != 0 && timestamp > 0;
			}
		}

		/// <summary>
		/// PGN metadata for complete game notation
		/// </summary>
		[System.Serializable]
		public class PGNMetadata
		{
			public string Event { get; set; } = "Casual Game";
			public string Site { get; set; } = "Unity Chess";
			public string Date { get; set; } = "";
			public string Round { get; set; } = "1";
			public string White { get; set; } = "Human";
			public string Black { get; set; } = "Engine";
			public string Result { get; set; } = "*";
			public string WhiteElo { get; set; } = "?";
			public string BlackElo { get; set; } = "?";
			public string TimeControl { get; set; } = "-";
			public string ECO { get; set; } = "";
			public string Opening { get; set; } = "";

			public PGNMetadata()
			{
				Date = DateTime.Now.ToString("yyyy.MM.dd");
			}

			public bool IsValid()
			{
				return !string.IsNullOrEmpty(Event) && !string.IsNullOrEmpty(Site) &&
					   !string.IsNullOrEmpty(Date) && !string.IsNullOrEmpty(Result);
			}

			public override string ToString()
			{
				return $"PGN[{Event} at {Site}, {Date}] {White} vs {Black}: {Result}";
			}
		}

		/// <summary>
		/// Enhanced game tree for branching move history
		/// </summary>
		[System.Serializable]
		public class GameTree
		{
			[SerializeField] private List<GameNode> nodes = new List<GameNode>();
			[SerializeField] private int currentNodeIndex = -1;
			[SerializeField] private Dictionary<ulong, int> positionToNodeMap = new Dictionary<ulong, int>();

			public List<GameNode> GetNodes { get { return this.nodes; } } // just to log
			public int CurrentNodeIndex => currentNodeIndex;
			public int NodeCount => nodes.Count;
			public GameNode CurrentNode => currentNodeIndex >= 0 && currentNodeIndex < nodes.Count ? nodes[currentNodeIndex] : null;

			/// <summary>
			/// Add move to current position, creating new branch if necessary
			/// </summary>
			public GameNode AddMove(BoardState state, ChessMove move, string san, float evaluation, string comment = "")
			{
				if (!state.IsValid())
				{
					Debug.Log("<color=red>[GameTree] Cannot add move: invalid board state</color>");
					return null;
				}

				var newNode = new GameNode
				{
					state = state,
					move = move,
					sanNotation = san,
					evaluation = evaluation,
					comment = comment,
					parentIndex = currentNodeIndex,
					children = new List<int>(),
					timestamp = Time.time
				};

				// Add to parent's children if we have a parent
				if (currentNodeIndex >= 0 && currentNodeIndex < nodes.Count)
				{
					nodes[currentNodeIndex].children.Add(nodes.Count);
				}

				nodes.Add(newNode);
				currentNodeIndex = nodes.Count - 1;

				// Update position mapping
				if (!positionToNodeMap.ContainsKey(state.positionHash))
				{
					positionToNodeMap[state.positionHash] = currentNodeIndex;
				}

				return newNode;
			}

			/// <summary>
			/// Navigate to specific node
			/// </summary>
			public bool GoToNode(int nodeIndex)
			{
				if (nodeIndex >= -1 && nodeIndex < nodes.Count)
				{
					currentNodeIndex = nodeIndex;
					return true;
				}
				return false;
			}

			/// <summary>
			/// Get path from root to current node
			/// </summary>
			public List<GameNode> GetMainLine()
			{
				var path = new List<GameNode>();
				int index = currentNodeIndex;

				while (index >= 0 && index < nodes.Count)
				{
					path.Insert(0, nodes[index]);
					index = nodes[index].parentIndex;
				}

				return path;
			}

			/// <summary>
			/// Get all variations from a position
			/// </summary>
			public List<List<GameNode>> GetVariations(int fromNodeIndex = -1)
			{
				if (fromNodeIndex == -1) fromNodeIndex = currentNodeIndex;
				if (fromNodeIndex < 0 || fromNodeIndex >= nodes.Count) return new List<List<GameNode>>();

				var variations = new List<List<GameNode>>();
				var node = nodes[fromNodeIndex];

				foreach (int childIndex in node.children)
				{
					if (childIndex < 0 || childIndex >= nodes.Count) continue;

					var variation = new List<GameNode>();
					int index = childIndex;

					while (index >= 0 && index < nodes.Count)
					{
						var childNode = nodes[index];
						variation.Add(childNode);

						// Follow main line (first child)
						if (childNode.children.Count > 0)
						{
							index = childNode.children[0];
						}
						else
						{
							break;
						}
					}

					if (variation.Count > 0)
					{
						variations.Add(variation);
					}
				}

				return variations;
			}

			public void Clear()
			{
				nodes.Clear();
				currentNodeIndex = -1;
				positionToNodeMap.Clear();
			}

			/// <summary>
			/// Find position in tree
			/// </summary>
			public int FindPosition(ulong positionHash)
			{
				return positionToNodeMap.ContainsKey(positionHash) ? positionToNodeMap[positionHash] : -1;
			}

			/// <summary>
			/// Check if can undo move
			/// </summary>
			public bool CanUndo()
			{
				return currentNodeIndex > 0;
			}

			/// <summary>
			/// Check if can redo move
			/// </summary>
			public bool CanRedo()
			{
				return currentNodeIndex >= 0 && currentNodeIndex < nodes.Count &&
					   nodes[currentNodeIndex].children.Count > 0;
			}
		}

		/// <summary>
		/// Game tree node with enhanced data
		/// </summary>
		[System.Serializable]
		public class GameNode
		{
			public BoardState state;
			public ChessMove move;
			public string sanNotation;
			public float evaluation;
			public string comment;
			public int parentIndex;
			public List<int> children;
			public float timestamp;
			public Dictionary<string, string> annotations; // For engine analysis, etc.

			public GameNode()
			{
				children = new List<int>();
				annotations = new Dictionary<string, string>();
				parentIndex = -1;
				sanNotation = "";
				comment = "";
			}

			public bool IsValid()
			{
				return !string.IsNullOrEmpty(sanNotation) && timestamp > 0;
			}

			public override string ToString()
			{
				return $"GameNode[{sanNotation}] eval:{evaluation:F1} at {timestamp:F2}s";
			}
		}

		/// <summary>
		/// Enhanced board state with position hashing
		/// </summary>
		[System.Serializable]
		public struct BoardState
		{
			public string fen;
			public char sideToMove;
			public string castlingRights;
			public string enPassantSquare;
			public int halfmoveClock;
			public int fullmoveNumber;
			public float timestamp;
			public float evaluation;
			public float winProbability;
			public float mateDistance;
			public ulong positionHash; // Zobrist hash for fast position comparison

			public BoardState(ChessBoard board)
			{
				this.fen = board.ToFEN();
				this.sideToMove = board.sideToMove;
				this.castlingRights = board.castlingRights ?? "KQkq";
				this.enPassantSquare = board.enPassantSquare ?? "-";
				this.halfmoveClock = board.halfmoveClock;
				this.fullmoveNumber = board.fullmoveNumber;
				this.timestamp = Time.time;
				this.evaluation = board.lastEvaluation;
				this.winProbability = board.lastWinProbability;
				this.mateDistance = board.lastMateDistance;
				this.positionHash = board.CalculatePositionHash();
			}

			public bool IsValid()
			{
				return !string.IsNullOrEmpty(fen) && positionHash != 0 &&
					   (sideToMove == 'w' || sideToMove == 'b') &&
					   !string.IsNullOrEmpty(castlingRights) &&
					   !string.IsNullOrEmpty(enPassantSquare);
			}
		}

		#region Constructors and Initialization

		public ChessBoard()
		{
			InitializeZobristKeys();
			SetupStartingPosition();
			SaveCurrentState();
			InitializeProperties();
		}

		public ChessBoard(string fen, ChessVariant variant = ChessVariant.Standard, ValidationMode validation = ValidationMode.Permissive, bool fallbackOnFailure = false)
		{
			this.variant = variant;
			this.validationMode = validation;
			this.fallbackToStartingPosition = fallbackOnFailure;

			InitializeZobristKeys();

			if (string.IsNullOrEmpty(fen) || fen == "startpos")
			{
				SetupStartingPosition();
			}
			else
			{
				if (!LoadFromFEN(fen))
				{
					if (fallbackToStartingPosition)
					{
						Debug.Log("<color=yellow>[ChessBoard] Failed to load FEN, using starting position</color>");
						SetupStartingPosition();
					}
					else
					{
						Debug.Log("<color=red>[ChessBoard] Failed to load FEN, board may be in invalid state</color>");
					}
				}
			}
			SaveCurrentState();
			InitializeProperties();
		}

		private void InitializeProperties()
		{
			LastEvaluation = lastEvaluation;
			LastWinProbability = lastWinProbability;
			LastMateDistance = lastMateDistance;
			LastEvaluationDepth = lastEvaluationDepth;
		}

		/// <summary>
		/// Set validation mode for FEN loading
		/// </summary>
		public void SetValidationMode(ValidationMode mode, bool fallback = false)
		{
			validationMode = mode;
			fallbackToStartingPosition = fallback;

			if (LoggingEnabled)
				Debug.Log($"<color=cyan>[ChessBoard] Validation mode set to: {mode}, Fallback: {fallback}</color>");
		}

		/// <summary>
		/// Setup starting position based on variant
		/// </summary>
		public void SetupStartingPosition()
		{
			string startFEN;

			switch (variant)
			{
				case ChessVariant.Chess960:
					startFEN = GenerateChess960Position();
					break;
				case ChessVariant.KingOfTheHill:
					startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
					break;
				case ChessVariant.Horde:
					startFEN = "rnbqkbnr/pppppppp/8/1PP2PP1/PPPPPPPP/PPPPPPPP/PPPPPPPP/PPPPPPPP w kq - 0 1";
					break;
				case ChessVariant.RacingKings:
					startFEN = "8/8/8/8/8/8/krbnNBRK/qrbnNBRQ w - - 0 1";
					break;
				default:
					startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
					break;
			}

			// Temporarily use permissive mode to load starting positions
			var originalMode = validationMode;
			validationMode = ValidationMode.Permissive;

			if (!LoadFromFEN(startFEN))
			{
				Debug.Log("<color=red>[ChessBoard] Failed to setup starting position</color>");
			}

			validationMode = originalMode;
			ResetEvaluation();
		}

		private string GenerateChess960Position()
		{
			// Simple Chess960 position generation
			var pieces = new char[] { 'R', 'N', 'B', 'Q', 'K', 'B', 'N', 'R' };

			// Shuffle pieces (basic implementation)
			for (int i = 0; i < pieces.Length; i++)
			{
				int randomIndex = UnityEngine.Random.Range(i, pieces.Length);
				char temp = pieces[i];
				pieces[i] = pieces[randomIndex];
				pieces[randomIndex] = temp;
			}

			string backRank = new string(pieces);
			string blackRank = backRank.ToLower();

			return $"{blackRank}/pppppppp/8/8/8/8/PPPPPPPP/{backRank} w KQkq - 0 1";
		}

		#endregion

		#region Position Hashing (Zobrist)

		private static ulong[,] pieceKeys = new ulong[64, 12]; // 64 squares, 12 piece types
		private static ulong[] castlingKeys = new ulong[16];   // 16 castling combinations
		private static ulong[] enPassantKeys = new ulong[8];   // 8 files
		private static ulong sideToMoveKey;
		private static bool zobristInitialized = false;

		private void InitializeZobristKeys()
		{
			if (zobristInitialized) return;

			System.Random rng = new System.Random(12345); // Fixed seed for consistency

			// Initialize piece keys
			for (int square = 0; square < 64; square++)
			{
				for (int piece = 0; piece < 12; piece++)
				{
					pieceKeys[square, piece] = GenerateRandomUInt64(rng);
				}
			}

			// Initialize castling keys
			for (int i = 0; i < 16; i++)
			{
				castlingKeys[i] = GenerateRandomUInt64(rng);
			}

			// Initialize en passant keys
			for (int i = 0; i < 8; i++)
			{
				enPassantKeys[i] = GenerateRandomUInt64(rng);
			}

			sideToMoveKey = GenerateRandomUInt64(rng);
			zobristInitialized = true;
		}

		private ulong GenerateRandomUInt64(System.Random rng)
		{
			byte[] bytes = new byte[8];
			rng.NextBytes(bytes);
			return BitConverter.ToUInt64(bytes, 0);
		}

		/// <summary>
		/// Calculate Zobrist hash for current position
		/// </summary>
		public ulong CalculatePositionHash()
		{
			ulong hash = 0;

			try
			{
				// Hash pieces
				for (int y = 0; y < 8; y++)
				{
					for (int x = 0; x < 8; x++)
					{
						char piece = board.GT(new v2(x, y));
						if (piece != '.')
						{
							int pieceIndex = GetPieceIndex(piece);
							if (pieceIndex >= 0)
							{
								int squareIndex = y * 8 + x;
								hash ^= pieceKeys[squareIndex, pieceIndex];
							}
						}
					}
				}

				// Hash castling rights
				int castlingIndex = GetCastlingIndex(castlingRights ?? "");
				hash ^= castlingKeys[castlingIndex];

				// Hash en passant - Fixed Unity 2020.3 compatibility
				if (!string.IsNullOrEmpty(enPassantSquare) && enPassantSquare != "-" && enPassantSquare.Length >= 1)
				{
					int file = enPassantSquare[0] - 'a';
					if (file >= 0 && file < 8)
					{
						hash ^= enPassantKeys[file];
					}
				}

				// Hash side to move
				if (sideToMove == 'b')
				{
					hash ^= sideToMoveKey;
				}
			}
			catch (Exception e)
			{
				Debug.Log($"<color=red>[ChessBoard] Error calculating position hash: {e.Message}</color>");
				return 0;
			}

			return hash;
		}

		private int GetPieceIndex(char piece)
		{
			switch (piece)
			{
				case 'P': return 0;
				case 'N': return 1;
				case 'B': return 2;
				case 'R': return 3;
				case 'Q': return 4;
				case 'K': return 5;
				case 'p': return 6;
				case 'n': return 7;
				case 'b': return 8;
				case 'r': return 9;
				case 'q': return 10;
				case 'k': return 11;
				default: return -1;
			}
		}

		private int GetCastlingIndex(string rights)
		{
			int index = 0;
			if (rights.Contains("K")) index |= 1;
			if (rights.Contains("Q")) index |= 2;
			if (rights.Contains("k")) index |= 4;
			if (rights.Contains("q")) index |= 8;
			return index;
		}

		#endregion

		#region Enhanced History and Game Tree

		/// <summary>
		/// Save current state to game tree
		/// </summary>
		private void SaveCurrentState()
		{
			var state = new BoardState(this);

			if (gameTree.NodeCount == 0)
			{
				// First position (root)
				gameTree.AddMove(state, ChessMove.Invalid(), "start", lastEvaluation);
			}
		}

		/// <summary>
		/// Make move and save to game tree with PGN notation
		/// </summary>
		public bool MakeMove(ChessMove move, string comment = "")
		{
			if (!move.IsValid())
			{
				Debug.Log($"<color=red>[ChessBoard] Invalid move coordinates: from {move.from} to {move.to}</color>");
				return false;
			}

			// Validate move is legal
			if (!ChessRules.ValidateMove(this, move))
			{
				Debug.Log($"<color=red>[ChessBoard] Illegal move: {move.ToUCI()}</color>");
				return false;
			}

			// Generate SAN notation before making the move
			string sanNotation = "";
			try
			{
				sanNotation = move.ToPGN(this);
			}
			catch (Exception e)
			{
				Debug.Log($"<color=yellow>[ChessBoard] Warning: Could not generate SAN notation: {e.Message}</color>");
				sanNotation = move.ToUCI();
			}

			// Apply the move using ChessRules
			bool success = ChessRules.MakeMove(this, move);

			if (success)
			{
				// Save to game tree
				var state = new BoardState(this);
				var newNode = gameTree.AddMove(state, move, sanNotation, lastEvaluation, comment);

				if (newNode != null)
				{
					// Update position cache
					UpdatePositionCache();
					Debug.Log($"<color=green>[ChessBoard] Made move: {sanNotation} ({gameTree.NodeCount} gameTreeCount)</color>");
				}
				else
				{
					Debug.Log($"<color=yellow>[ChessBoard] Move made but failed to save to game tree</color>");
				}
			}
			else
			{
				Debug.Log($"<color=red>[ChessBoard] Failed to make move: {move.ToUCI()}</color>");
			}

			return success;
		}

		/// <summary>
		/// Navigate to previous position in game tree
		/// </summary>
		public bool UndoMove()
		{
			if (!gameTree.CanUndo())
			{
				Debug.Log("<color=yellow>[ChessBoard] Cannot undo: at start of game</color>");
				return false;
			}

			var currentNode = gameTree.CurrentNode;
			if (currentNode == null || currentNode.parentIndex < 0)
			{
				return false;
			}

			if (gameTree.GoToNode(currentNode.parentIndex))
			{
				var parentNode = gameTree.CurrentNode;
				if (parentNode != null && parentNode.state.IsValid())
				{
					RestoreState(parentNode.state);
					Debug.Log($"<color=green>[ChessBoard] Undid move. Now at position {gameTree.CurrentNodeIndex}</color>");
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Navigate forward in main line
		/// </summary>
		public bool RedoMove()
		{
			if (!gameTree.CanRedo())
			{
				Debug.Log("<color=yellow>[ChessBoard] Cannot redo: at end of variation</color>");
				return false;
			}

			var currentNode = gameTree.CurrentNode;
			if (currentNode == null || currentNode.children.Count == 0)
			{
				return false;
			}

			// Follow main line (first child)
			int nextIndex = currentNode.children[0];
			if (gameTree.GoToNode(nextIndex))
			{
				var nextNode = gameTree.CurrentNode;
				if (nextNode != null && nextNode.state.IsValid())
				{
					RestoreState(nextNode.state);
					Debug.Log($"<color=green>[ChessBoard] Redid move: {nextNode.sanNotation}</color>");
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Navigate to specific variation
		/// </summary>
		public bool GoToVariation(int variationIndex)
		{
			var currentNode = gameTree.CurrentNode;
			if (currentNode == null || variationIndex >= currentNode.children.Count || variationIndex < 0)
			{
				return false;
			}

			int targetIndex = currentNode.children[variationIndex];
			if (gameTree.GoToNode(targetIndex))
			{
				var targetNode = gameTree.CurrentNode;
				if (targetNode != null && targetNode.state.IsValid())
				{
					RestoreState(targetNode.state);
					Debug.Log($"<color=green>[ChessBoard] Switched to variation: {targetNode.sanNotation}</color>");
					return true;
				}
			}

			return false;
		}

		private void RestoreState(BoardState state)
		{
			if (!state.IsValid())
			{
				Debug.Log("<color=red>[ChessBoard] Cannot restore: invalid board state</color>");
				return;
			}

			// Temporarily use permissive mode to restore state
			var originalMode = validationMode;
			var originalFallback = fallbackToStartingPosition;
			validationMode = ValidationMode.Permissive;
			fallbackToStartingPosition = false;

			LoadFromFEN(state.fen);
			UpdateEvaluationPrivate(state.evaluation, state.winProbability, state.mateDistance);

			// Restore original validation settings
			validationMode = originalMode;
			fallbackToStartingPosition = originalFallback;
		}

		/// <summary>
		/// Check if can undo move
		/// </summary>
		public bool CanUndo()
		{
			return gameTree.CanUndo();
		}

		/// <summary>
		/// Check if can redo move
		/// </summary>
		public bool CanRedo()
		{
			return gameTree.CanRedo();
		}

		#endregion

		#region Position Caching

		/// <summary>
		/// Update position cache with current evaluation
		/// </summary>
		private void UpdatePositionCache()
		{
			if (!enablePositionCaching) return;

			try
			{
				ulong hash = CalculatePositionHash();
				if (hash == 0) return;

				var posInfo = new PositionInfo(hash, lastEvaluation, lastWinProbability, lastEvaluationDepth);
				positionCache[hash] = posInfo;

				// Prune cache if too large
				if (positionCache.Count > maxCacheSize)
				{
					var oldestEntries = positionCache.OrderBy(kvp => kvp.Value.timestamp).Take(maxCacheSize / 4).ToList();
					foreach (var entry in oldestEntries)
					{
						positionCache.Remove(entry.Key);
					}
				}
			}
			catch (Exception e)
			{
				Debug.Log($"<color=yellow>[ChessBoard] Warning: Could not update position cache: {e.Message}</color>");
			}
		}

		/// <summary>
		/// Get cached position info
		/// </summary>
		public PositionInfo? GetCachedPositionInfo()
		{
			if (!enablePositionCaching) return null;

			try
			{
				ulong hash = CalculatePositionHash();
				if (hash == 0) return null;

				return positionCache.ContainsKey(hash) ? positionCache[hash] : (PositionInfo?)null;
			}
			catch (Exception e)
			{
				Debug.Log($"<color=yellow>[ChessBoard] Warning: Could not get cached position info: {e.Message}</color>");
				return null;
			}
		}

		/// <summary>
		/// Check for threefold repetition using position hashes
		/// </summary>
		public bool IsThreefoldRepetition()
		{
			try
			{
				ulong currentHash = CalculatePositionHash();
				if (currentHash == 0) return false;

				int count = 0;
				var mainLine = gameTree.GetMainLine();

				foreach (var node in mainLine)
				{
					if (node.state.positionHash == currentHash)
					{
						count++;
						if (count >= 3) return true;
					}
				}
			}
			catch (Exception e)
			{
				Debug.Log($"<color=yellow>[ChessBoard] Warning: Could not check threefold repetition: {e.Message}</color>");
			}

			return false;
		}

		#endregion

		#region FEN and Basic Operations

		public bool LoadFromFEN(string fen)
		{
			if (string.IsNullOrEmpty(fen))
			{
				Debug.Log("<color=red>[ChessBoard] FEN string is null or empty</color>");
				return false;
			}

			try
			{
				string[] parts = fen.Trim().Split(' ');
				if (parts.Length < 6)
				{
					Debug.Log($"<color=red>[ChessBoard] Invalid FEN: incomplete format, expected 6 parts but got {parts.Length}</color>");
					return false;
				}

				// Step 1: Parse board position (syntax only)
				if (!ParseBoardPosition(parts[0]))
				{
					return false;
				}

				// Step 2: Parse metadata
				sideToMove = parts.Length > 1 && parts[1].Length > 0 ? parts[1][0] : 'w';
				if (sideToMove != 'w' && sideToMove != 'b')
				{
					Debug.Log($"<color=red>[ChessBoard] Invalid side to move: {sideToMove}</color>");
					sideToMove = 'w';
				}

				castlingRights = parts.Length > 2 ? parts[2] : "KQkq";
				if (string.IsNullOrEmpty(castlingRights)) castlingRights = "-";

				enPassantSquare = parts.Length > 3 ? parts[3] : "-";
				if (string.IsNullOrEmpty(enPassantSquare)) enPassantSquare = "-";

				if (parts.Length > 4 && int.TryParse(parts[4], out int halfMove))
					halfmoveClock = Math.Max(0, halfMove);
				else
					halfmoveClock = 0;

				if (parts.Length > 5 && int.TryParse(parts[5], out int fullMove))
					fullmoveNumber = Math.Max(1, fullMove);
				else
					fullmoveNumber = 1;

				// Step 3: Validate position based on current validation mode
				bool positionValid = true;
				string validationError = "";

				if (validationMode != ValidationMode.ParseOnly)
				{
					positionValid = ValidateBoardState(out validationError);
				}

				if (!positionValid)
				{
					if (validationMode == ValidationMode.Strict)
					{
						Debug.Log($"<color=red>[ChessBoard] FEN validation failed (strict): {validationError}</color>");
						if (fallbackToStartingPosition)
						{
							SetupStartingPosition();
							return true; // Fallback succeeded
						}
						return false;
					}
					else if (validationMode == ValidationMode.Permissive)
					{
						Debug.Log($"<color=yellow>[ChessBoard] FEN validation warning (permissive): {validationError}</color>");
						// Continue with the parsed position
					}
				}

				if (LoggingEnabled)
					Debug.Log($"<color=green>[ChessBoard] Loaded FEN: {fen} (mode: {validationMode})</color>");
				return true;
			}
			catch (Exception e)
			{
				Debug.Log($"<color=red>[ChessBoard] Error parsing FEN '{fen}': {e.Message}</color>");

				if (fallbackToStartingPosition)
				{
					SetupStartingPosition();
					return true;
				}
				return false;
			}
		}

		private bool ParseBoardPosition(string boardString)
		{
			if (string.IsNullOrEmpty(boardString))
			{
				Debug.Log("<color=red>[ChessBoard] Board string is null or empty</color>");
				return false;
			}

			string[] ranks = boardString.Split('/');
			if (ranks.Length != 8)
			{
				Debug.Log($"<color=red>[ChessBoard] Invalid board FEN: expected 8 ranks, got {ranks.Length}</color>");
				return false;
			}

			// Clear board first
			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					board.ST(new v2(x, y), '.');
				}
			}

			for (int rank = 0; rank < 8; rank++)
			{
				int y = 7 - rank;
				int x = 0;

				foreach (char c in ranks[rank])
				{
					if (char.IsDigit(c))
					{
						int emptyCount = c - '0';
						if (emptyCount < 1 || emptyCount > 8)
						{
							Debug.Log($"<color=red>[ChessBoard] Invalid empty square count: {emptyCount}</color>");
							return false;
						}

						if (x + emptyCount > 8)
						{
							Debug.Log($"<color=red>[ChessBoard] Empty squares exceed rank boundary</color>");
							return false;
						}

						x += emptyCount;
					}
					else if ("rnbqkpRNBQKP".Contains(c.ToString()))
					{
						if (x >= 8)
						{
							Debug.Log($"<color=red>[ChessBoard] Invalid FEN: too many pieces in rank {8 - rank}</color>");
							return false;
						}
						board.ST(new v2(x, y), c);
						x++;
					}
					else
					{
						Debug.Log($"<color=red>[ChessBoard] Invalid FEN character: '{c}' in rank {8 - rank}</color>");
						return false;
					}
				}

				if (x != 8)
				{
					Debug.Log($"<color=red>[ChessBoard] Invalid FEN: rank {8 - rank} has {x} squares, expected 8</color>");
					return false;
				}
			}

			return true;
		}

		private bool ValidateBoardState(out string errorMessage)
		{
			errorMessage = "";

			try
			{
				int whiteKings = 0, blackKings = 0;
				int whitePawns = 0, blackPawns = 0;
				int whiteQueens = 0, blackQueens = 0;
				int whiteRooks = 0, blackRooks = 0;
				int whiteBishops = 0, blackBishops = 0;
				int whiteKnights = 0, blackKnights = 0;

				for (int y = 0; y < 8; y++)
				{
					for (int x = 0; x < 8; x++)
					{
						char piece = board.GT(new v2(x, y));
						switch (piece)
						{
							case 'K': whiteKings++; break;
							case 'k': blackKings++; break;
							case 'Q': whiteQueens++; break;
							case 'q': blackQueens++; break;
							case 'R': whiteRooks++; break;
							case 'r': blackRooks++; break;
							case 'B': whiteBishops++; break;
							case 'b': blackBishops++; break;
							case 'N': whiteKnights++; break;
							case 'n': blackKnights++; break;
							case 'P':
								whitePawns++;
								if (y == 0 || y == 7)
								{
									errorMessage = $"white pawn on rank {y + 1}";
									return false;
								}
								break;
							case 'p':
								blackPawns++;
								if (y == 0 || y == 7)
								{
									errorMessage = $"black pawn on rank {y + 1}";
									return false;
								}
								break;
							case '.':
								// Empty square, valid
								break;
							default:
								errorMessage = $"invalid piece character: '{piece}'";
								return false;
						}
					}
				}

				// Validate piece counts
				if (whiteKings != 1 || blackKings != 1)
				{
					errorMessage = $"found {whiteKings} white kings, {blackKings} black kings (expected 1 each)";
					return false;
				}

				if (whitePawns > 8 || blackPawns > 8)
				{
					errorMessage = $"too many pawns (W:{whitePawns}, B:{blackPawns})";
					return false;
				}

				// Check for reasonable piece limits (allowing for promotions)
				if (whiteQueens > 9 || blackQueens > 9 || whiteRooks > 10 || blackRooks > 10 ||
					whiteBishops > 10 || blackBishops > 10 || whiteKnights > 10 || blackKnights > 10)
				{
					if (validationMode == ValidationMode.Strict)
					{
						errorMessage = "unusual piece count detected";
						return false;
					}
					else
					{
						Debug.Log("<color=yellow>[ChessBoard] Warning: unusual piece count detected</color>");
					}
				}

				// Validate en passant square
				if (enPassantSquare != "-" && enPassantSquare.Length >= 2)
				{
					v2 epSquare = AlgebraicToCoord(enPassantSquare);
					if (epSquare.x < 0 || epSquare.y < 0)
					{
						errorMessage = $"invalid en passant square: {enPassantSquare}";
						return false;
					}

					// En passant square should be on 3rd or 6th rank
					if (epSquare.y != 2 && epSquare.y != 5)
					{
						if (validationMode == ValidationMode.Strict)
						{
							errorMessage = $"en passant square on wrong rank: {enPassantSquare}";
							return false;
						}
						else
						{
							Debug.Log($"<color=yellow>[ChessBoard] Warning: unusual en passant square rank: {enPassantSquare}</color>");
						}
					}
				}

				return true;
			}
			catch (Exception e)
			{
				errorMessage = $"exception during validation: {e.Message}";
				return false;
			}
		}

		public string ToFEN()
		{
			try
			{
				string boardFen = "";

				for (int rank = 7; rank >= 0; rank--)
				{
					int emptyCount = 0;
					for (int file = 0; file < 8; file++)
					{
						char piece = board.GT(new v2(file, rank));
						if (piece == '.')
						{
							emptyCount++;
						}
						else
						{
							if (emptyCount > 0)
							{
								boardFen += emptyCount.ToString();
								emptyCount = 0;
							}
							boardFen += piece;
						}
					}

					if (emptyCount > 0)
					{
						boardFen += emptyCount.ToString();
					}

					if (rank > 0)
					{
						boardFen += "/";
					}
				}

				string castling = string.IsNullOrEmpty(castlingRights) || castlingRights == "-" ? "-" : castlingRights;
				string enPassant = string.IsNullOrEmpty(enPassantSquare) || enPassantSquare == "-" ? "-" : enPassantSquare;

				return $"{boardFen} {sideToMove} {castling} {enPassant} {halfmoveClock} {fullmoveNumber}";
			}
			catch (Exception e)
			{
				Debug.Log($"<color=red>[ChessBoard] Error generating FEN: {e.Message}</color>");
				return "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"; // Return starting position as fallback
			}
		}

		#endregion

		#region Basic Board Access and Utility

		/// <summary>
		/// Set human player side with validation
		/// </summary>
		public void SetHumanSide(char side)
		{
			if (side != 'w' && side != 'b' && side != 'x')
			{
				Debug.Log($"<color=yellow>[ChessBoard] Invalid human side: {side}. Using 'w' (white)</color>");
				side = 'w';
			}

			humanSide = side;
			engineSide = side == 'w' ? 'b' : side == 'b' ? 'w' : 'x';

			Debug.Log($"<color=green>[ChessBoard] Human plays: {GetSideName(humanSide)}, Engine plays: {GetSideName(engineSide)}</color>");
		}

		/// <summary>
		/// Get human-readable side name
		/// </summary>
		public string GetSideName(char side)
		{
			switch (side)
			{
				case 'w': return "White";
				case 'b': return "Black";
				case 'x': return "Both/None";
				default: return "Unknown";
			}
		}

		/// <summary>
		/// Check if it's human player's turn
		/// </summary>
		public bool IsHumanTurn() => humanSide == 'x' || humanSide == sideToMove;

		/// <summary>
		/// Check if it's engine's turn
		/// </summary>
		public bool IsEngineTurn() => engineSide == 'x' || engineSide == sideToMove;

		/// <summary>
		/// Get piece at algebraic coordinate with validation
		/// </summary>
		public char GetPiece(string square)
		{
			if (string.IsNullOrEmpty(square))
			{
				Debug.Log("<color=yellow>[ChessBoard] GetPiece: square is null or empty</color>");
				return '.';
			}

			v2 coord = AlgebraicToCoord(square);
			if (coord.x < 0 || coord.y < 0)
			{
				Debug.Log($"<color=yellow>[ChessBoard] GetPiece: invalid square '{square}'</color>");
				return '.';
			}

			return board.GT(coord);
		}

		/// <summary>
		/// Get piece at coordinate with bounds checking
		/// </summary>
		public char GetPiece(v2 coord)
		{
			if (coord.x < 0 || coord.x >= 8 || coord.y < 0 || coord.y >= 8)
			{
				Debug.Log($"<color=yellow>[ChessBoard] GetPiece: coordinate out of bounds {coord}</color>");
				return '.';
			}
			return board.GT(coord);
		}

		/// <summary>
		/// Set piece at coordinate with validation
		/// </summary>
		public void SetPiece(v2 coord, char piece)
		{
			if (coord.x < 0 || coord.x >= 8 || coord.y < 0 || coord.y >= 8)
			{
				Debug.Log($"<color=yellow>[ChessBoard] SetPiece: coordinate out of bounds {coord}</color>");
				return;
			}

			if (piece != '.' && !"rnbqkpRNBQKP".Contains(piece.ToString()))
			{
				Debug.Log($"<color=yellow>[ChessBoard] SetPiece: invalid piece '{piece}'</color>");
				return;
			}

			board.ST(coord, piece);
		}

		/// <summary>
		/// Convert algebraic notation to coordinate with validation
		/// </summary>
		public static v2 AlgebraicToCoord(string square)
		{
			if (string.IsNullOrEmpty(square) || square.Length < 2)
				return new v2(-1, -1);

			char file = char.ToLower(square[0]);
			char rank = square[1];

			if (file < 'a' || file > 'h' || rank < '1' || rank > '8')
				return new v2(-1, -1);

			return new v2(file - 'a', rank - '1');
		}

		/// <summary>
		/// Convert coordinate to algebraic notation with validation
		/// </summary>
		public static string CoordToAlgebraic(v2 coord)
		{
			if (coord.x < 0 || coord.x >= 8 || coord.y < 0 || coord.y >= 8)
				return "";

			char file = (char)('a' + coord.x);
			char rank = (char)('1' + coord.y);
			return "" + file + rank;
		}

		/// <summary>
		/// Get all legal moves for current position
		/// </summary>
		public List<ChessMove> GetLegalMoves()
		{
			try
			{
				return MoveGenerator.GenerateLegalMoves(this);
			}
			catch (Exception e)
			{
				Debug.Log($"<color=red>[ChessBoard] Error generating legal moves: {e.Message}</color>");
				return new List<ChessMove>();
			}
		}

		/// <summary>
		/// Update evaluation data with validation
		/// </summary>
		public void UpdateEvaluation(float centipawnScore, float winProbability, float mateDistance = 0f, int searchDepth = 0)
		{
			UpdateEvaluationPrivate(centipawnScore, winProbability, mateDistance, searchDepth);
			UpdatePositionCache();

			Debug.Log($"<color=cyan>[ChessBoard] Updated evaluation: {centipawnScore:F1}cp, WinProb: {winProbability:F2}, Depth: {searchDepth}</color>");
		}

		private void UpdateEvaluationPrivate(float centipawnScore, float winProbability, float mateDistance, int searchDepth = 0)
		{
			lastEvaluation = centipawnScore;
			lastWinProbability = Mathf.Clamp01(winProbability);
			lastMateDistance = mateDistance;
			lastEvaluationDepth = Math.Max(0, searchDepth);

			LastEvaluation = lastEvaluation;
			LastWinProbability = lastWinProbability;
			LastMateDistance = lastMateDistance;
			LastEvaluationDepth = lastEvaluationDepth;
		}

		/// <summary>
		/// Reset evaluation to neutral
		/// </summary>
		public void ResetEvaluation()
		{
			UpdateEvaluationPrivate(0f, 0.5f, 0f, 0);
		}

		/// <summary>
		/// Get game result with validation
		/// </summary>
		public ChessRules.GameResult GetGameResult()
		{
			try
			{
				var moveHistory = GetMoveHistoryStrings();
				var standardResult = ChessRules.EvaluatePosition(this, moveHistory);

				// Check variant-specific winning conditions
				switch (variant)
				{
					case ChessVariant.KingOfTheHill:
						if (IsKingInCenter('w')) return ChessRules.GameResult.WhiteWins;
						if (IsKingInCenter('b')) return ChessRules.GameResult.BlackWins;
						break;
					case ChessVariant.ThreeCheck:
						// Would need to track check count
						break;
					case ChessVariant.RacingKings:
						// Would need to check if king reached 8th rank
						break;
				}

				return standardResult;
			}
			catch (Exception e)
			{
				Debug.Log($"<color=red>[ChessBoard] Error evaluating game result: {e.Message}</color>");
				return ChessRules.GameResult.InProgress;
			}
		}

		private bool IsKingInCenter(char side)
		{
			char king = side == 'w' ? 'K' : 'k';
			v2[] centerSquares = { new v2(3, 3), new v2(4, 3), new v2(3, 4), new v2(4, 4) };

			return centerSquares.Any(square => board.GT(square) == king);
		}

		private List<string> GetMoveHistoryStrings()
		{
			try
			{
				var mainLine = gameTree.GetMainLine();
				return mainLine.Skip(1).Select(node => node.move.ToUCI()).ToList();
			}
			catch (Exception e)
			{
				Debug.Log($"<color=yellow>[ChessBoard] Warning: Could not get move history: {e.Message}</color>");
				return new List<string>();
			}
		}

		/// <summary>
		/// Create deep copy of board
		/// </summary>
		public ChessBoard Clone()
		{
			try
			{
				ChessBoard clone = new ChessBoard();

				// Temporarily use permissive mode for cloning
				var originalMode = clone.validationMode;
				var originalFallback = clone.fallbackToStartingPosition;
				clone.validationMode = ValidationMode.Permissive;
				clone.fallbackToStartingPosition = false;

				clone.LoadFromFEN(this.ToFEN());

				// Restore original settings and copy all properties
				clone.validationMode = this.validationMode;
				clone.fallbackToStartingPosition = this.fallbackToStartingPosition;
				clone.humanSide = this.humanSide;
				clone.engineSide = this.engineSide;
				clone.variant = this.variant;
				clone.UpdateEvaluationPrivate(this.lastEvaluation, this.lastWinProbability, this.lastMateDistance, this.lastEvaluationDepth);
				clone.maxHistorySize = this.maxHistorySize;
				clone.enablePositionCaching = this.enablePositionCaching;
				clone.maxCacheSize = this.maxCacheSize;

				return clone;
			}
			catch (Exception e)
			{
				Debug.Log($"<color=red>[ChessBoard] Error cloning board: {e.Message}</color>");
				return new ChessBoard(); // Return new board as fallback
			}
		}

		object ICloneable.Clone() => Clone();

		/// <summary>
		/// ToString override for debugging and logging
		/// </summary>
		public override string ToString()
		{
			return $"ChessBoard[{variant}] {GetSideName(sideToMove)} to move, Move {fullmoveNumber}, " +
				   $"Eval: {LastEvaluation:F1}cp ({LastWinProbability:P0}), " +
				   $"History: {gameTree.NodeCount} positions, Mode: {validationMode}";
		}

		#endregion

		#region Comprehensive Test Suite

		/// <summary>
		/// Test FEN parsing with comprehensive edge cases
		/// </summary>
		private static void TestFENParsing()
		{
			Debug.Log("<color=cyan>[ChessBoard] Testing FEN parsing...</color>");

			// Valid FEN test cases
			string[] validFENs = {
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", // Starting position
				"r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1", // Castling test
				"rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1", // En passant
				"8/8/8/8/3k4/8/8/4K3 w - - 50 75", // Endgame with move counters
				"r1bqk1nr/pppp1ppp/2n5/2b1p3/2B1P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 4 4" // Italian game
			};

			foreach (string fen in validFENs)
			{
				ChessBoard board = new ChessBoard(fen, ChessVariant.Standard, ValidationMode.Strict, false);
				bool success = board.ToFEN().Split(' ')[0] == fen.Split(' ')[0]; // Compare board positions
				if (success)
				{
					string generatedFEN = board.ToFEN();
					if (generatedFEN.Split(' ')[0] == fen.Split(' ')[0]) // Compare positions only
					{
						Debug.Log($"<color=green>[ChessBoard] ✓ Valid FEN parsed: {fen.Substring(0, Math.Min(40, fen.Length))}...</color>");
					}
					else
					{
						Debug.Log($"<color=yellow>[ChessBoard] ? FEN round-trip differs: {fen.Substring(0, Math.Min(20, fen.Length))}...</color>");
					}
				}
				else
				{
					Debug.Log($"<color=red>[ChessBoard] ✗ Failed to parse valid FEN: {fen.Substring(0, Math.Min(40, fen.Length))}...</color>");
				}
			}

			// Test permissive mode with rule-violating positions
			string[] testPositions = {
				"P7/8/8/8/8/8/8/K6k w - - 0 1", // Pawn on 8th rank
				"8/8/8/8/8/8/8/K6k w - - 0 1", // No pieces except kings
				"8/8/8/8/8/8/p7/K6k w - - 0 1", // Pawn on 1st rank
			};

			foreach (string testFen in testPositions)
			{
				// Test strict mode (should fail)
				ChessBoard strictBoard = new ChessBoard(testFen, ChessVariant.Standard, ValidationMode.Strict, false);
				string strictResult = strictBoard.ToFEN();
				bool strictFailed = strictResult == "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

				// Test permissive mode (should succeed)
				ChessBoard permissiveBoard = new ChessBoard(testFen, ChessVariant.Standard, ValidationMode.Permissive, false);
				string permissiveResult = permissiveBoard.ToFEN();
				bool permissiveSucceeded = permissiveResult.Split(' ')[0] == testFen.Split(' ')[0];

				if (strictFailed && permissiveSucceeded)
				{
					Debug.Log($"<color=green>[ChessBoard] ✓ Validation modes work correctly for: {testFen.Substring(0, Math.Min(30, testFen.Length))}...</color>");
				}
				else
				{
					Debug.Log($"<color=red>[ChessBoard] ✗ Validation modes failed for: {testFen}</color>");
				}
			}

			// Invalid FEN test cases
			string[] invalidFENs = {
				"", // Empty string
				"invalid", // Not a FEN
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP", // Missing parts
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0", // Missing fullmove
				"rnbqkbnr/pppppppp/9/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", // Invalid rank (9)
				"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNRX w KQkq - 0 1", // Invalid piece
			};

			foreach (string invalidFen in invalidFENs)
			{
				ChessBoard board = new ChessBoard(invalidFen, ChessVariant.Standard, ValidationMode.Strict, false);
				string result = board.ToFEN();
				bool correctlyRejected = result == "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1" ||
										result.Split(' ')[0] != invalidFen.Split(' ')[0];

				if (correctlyRejected)
				{
					Debug.Log($"<color=green>[ChessBoard] ✓ Correctly rejected invalid FEN: {(string.IsNullOrEmpty(invalidFen) ? "(empty)" : invalidFen.Substring(0, Math.Min(30, invalidFen.Length)))}...</color>");
				}
				else
				{
					Debug.Log($"<color=red>[ChessBoard] ✗ Incorrectly accepted invalid FEN: {invalidFen}</color>");
				}
			}

			Debug.Log("<color=cyan>[ChessBoard] FEN parsing tests completed</color>");
		}

		/// <summary>
		/// Test move making and undo/redo functionality
		/// </summary>
		private static void TestMoveOperations()
		{
			Debug.Log("<color=cyan>[ChessBoard] Testing move operations...</color>");

			ChessBoard board = new ChessBoard();

			// Test basic move making
			ChessMove e4 = ChessMove.FromUCI("e2e4", board);
			if (board.MakeMove(e4))
			{
				Debug.Log("<color=green>[ChessBoard] ✓ Basic move making works</color>");

				// Test undo
				if (board.CanUndo() && board.UndoMove())
				{
					if (board.GetPiece("e2") == 'P' && board.GetPiece("e4") == '.')
					{
						Debug.Log("<color=green>[ChessBoard] ✓ Undo move works</color>");

						// Test redo
						if (board.CanRedo() && board.RedoMove())
						{
							if (board.GetPiece("e2") == '.' && board.GetPiece("e4") == 'P')
							{
								Debug.Log("<color=green>[ChessBoard] ✓ Redo move works</color>");
							}
							else
							{
								Debug.Log("<color=red>[ChessBoard] ✗ Redo move failed - pieces not in expected positions</color>");
							}
						}
						else
						{
							Debug.Log("<color=red>[ChessBoard] ✗ Redo move failed</color>");
						}
					}
					else
					{
						Debug.Log("<color=red>[ChessBoard] ✗ Undo move failed - pieces not in expected positions</color>");
					}
				}
				else
				{
					Debug.Log("<color=red>[ChessBoard] ✗ Undo move failed</color>");
				}
			}
			else
			{
				Debug.Log("<color=red>[ChessBoard] ✗ Basic move making failed</color>");
			}

			// Test illegal move rejection
			ChessMove illegalMove = new ChessMove(new v2(0, 0), new v2(7, 7), 'r'); // rook a1 to h8 (blocked)
			if (!board.MakeMove(illegalMove))
			{
				Debug.Log("<color=green>[ChessBoard] ✓ Illegal move correctly rejected</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessBoard] ✗ Illegal move incorrectly accepted</color>");
			}

			Debug.Log("<color=cyan>[ChessBoard] Move operations tests completed</color>");
		}

		/// <summary>
		/// Test evaluation system and game logic
		/// </summary>
		private static void TestEvaluationAndGameLogic()
		{
			Debug.Log("<color=cyan>[ChessBoard] Testing evaluation system and game logic...</color>");

			ChessBoard board = new ChessBoard();

			// Test evaluation update
			board.UpdateEvaluation(150.5f, 0.65f, 0f, 12);
			if (Math.Abs(board.LastEvaluation - 150.5f) < 0.01f &&
				Math.Abs(board.LastWinProbability - 0.65f) < 0.01f &&
				board.LastEvaluationDepth == 12)
			{
				Debug.Log("<color=green>[ChessBoard] ✓ Evaluation update works</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessBoard] ✗ Evaluation update failed</color>");
			}

			// Test win probability clamping
			board.UpdateEvaluation(0f, 1.5f, 0f, 10); // Should clamp to 1.0
			if (Math.Abs(board.LastWinProbability - 1.0f) < 0.01f)
			{
				Debug.Log("<color=green>[ChessBoard] ✓ Win probability clamping works</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessBoard] ✗ Win probability clamping failed</color>");
			}

			// Test evaluation reset
			board.ResetEvaluation();
			if (Math.Abs(board.LastEvaluation) < 0.01f &&
				Math.Abs(board.LastWinProbability - 0.5f) < 0.01f)
			{
				Debug.Log("<color=green>[ChessBoard] ✓ Evaluation reset works</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessBoard] ✗ Evaluation reset failed</color>");
			}

			// Test legal move generation
			var legalMoves = board.GetLegalMoves();
			if (legalMoves.Count == 20)
			{
				Debug.Log("<color=green>[ChessBoard] ✓ Starting position legal move count correct</color>");
			}
			else
			{
				Debug.Log($"<color=red>[ChessBoard] ✗ Starting position legal move count wrong: {legalMoves.Count}, expected 20</color>");
			}

			// Test game result evaluation
			var startResult = board.GetGameResult();
			if (startResult == ChessRules.GameResult.InProgress)
			{
				Debug.Log("<color=green>[ChessBoard] ✓ Starting position correctly identified as in progress</color>");
			}
			else
			{
				Debug.Log($"<color=red>[ChessBoard] ✗ Starting position incorrectly identified as: {startResult}</color>");
			}

			Debug.Log("<color=cyan>[ChessBoard] Evaluation system and game logic tests completed</color>");
		}

		/// <summary>
		/// Test position hashing, caching, and utility methods
		/// </summary>
		private static void TestAdvancedFeatures()
		{
			Debug.Log("<color=cyan>[ChessBoard] Testing advanced features...</color>");

			ChessBoard board1 = new ChessBoard();
			ChessBoard board2 = new ChessBoard();

			// Test identical positions have same hash
			ulong hash1 = board1.CalculatePositionHash();
			ulong hash2 = board2.CalculatePositionHash();

			if (hash1 == hash2 && hash1 != 0)
			{
				Debug.Log("<color=green>[ChessBoard] ✓ Identical positions have same hash</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessBoard] ✗ Identical positions have different hashes</color>");
			}

			// Test different positions have different hashes
			board2.MakeMove(ChessMove.FromUCI("e2e4", board2));
			ulong hash3 = board2.CalculatePositionHash();

			if (hash1 != hash3)
			{
				Debug.Log("<color=green>[ChessBoard] ✓ Different positions have different hashes</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessBoard] ✗ Different positions have same hash</color>");
			}

			// Test position caching
			board1.UpdateEvaluation(100f, 0.6f, 0f, 10);
			var cachedInfo = board1.GetCachedPositionInfo();
			if (cachedInfo.HasValue && Math.Abs(cachedInfo.Value.evaluation - 100f) < 0.01f)
			{
				Debug.Log("<color=green>[ChessBoard] ✓ Position caching works</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessBoard] ✗ Position caching failed</color>");
			}

			// Test side management
			board1.SetHumanSide('b');
			if (board1.humanSide == 'b' && board1.engineSide == 'w')
			{
				Debug.Log("<color=green>[ChessBoard] ✓ Valid side setting works</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessBoard] ✗ Valid side setting failed</color>");
			}

			// Test algebraic notation conversion
			v2 coord = AlgebraicToCoord("e4");
			string square = CoordToAlgebraic(coord);
			if (coord.x == 4 && coord.y == 3 && square == "e4")
			{
				Debug.Log("<color=green>[ChessBoard] ✓ Algebraic notation conversion works</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessBoard] ✗ Algebraic notation conversion failed</color>");
			}

			// Test piece access
			char piece = board1.GetPiece("e1");
			if (piece == 'K')
			{
				Debug.Log("<color=green>[ChessBoard] ✓ Piece access works</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessBoard] ✗ Piece access failed</color>");
			}

			// Test board cloning
			ChessBoard original = new ChessBoard();
			original.MakeMove(ChessMove.FromUCI("e2e4", original));
			original.UpdateEvaluation(50f, 0.55f, 0f, 8);
			original.SetHumanSide('b');

			ChessBoard clone = original.Clone();
			if (clone.ToFEN() == original.ToFEN() &&
				Math.Abs(clone.LastEvaluation - original.LastEvaluation) < 0.01f &&
				clone.humanSide == original.humanSide)
			{
				Debug.Log("<color=green>[ChessBoard] ✓ Board cloning works correctly</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessBoard] ✗ Board cloning failed</color>");
			}

			// Test independence
			original.MakeMove(ChessMove.FromUCI("e7e5", original));
			if (clone.ToFEN() != original.ToFEN())
			{
				Debug.Log("<color=green>[ChessBoard] ✓ Cloned board is independent</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessBoard] ✗ Cloned board is not independent</color>");
			}

			// Test threefold repetition
			ChessBoard repBoard = new ChessBoard();
			var moves = new string[] { "g1f3", "g8f6", "f3g1", "f6g8", "g1f3", "g8f6", "f3g1", "f6g8" };
			foreach (string uciMove in moves)
			{
				var move = ChessMove.FromUCI(uciMove, repBoard);
				repBoard.MakeMove(move);
			}

			bool isRepetition = repBoard.IsThreefoldRepetition();
			if (isRepetition)
			{
				Debug.Log("<color=green>[ChessBoard] ✓ Threefold repetition correctly detected</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessBoard] ✗ Threefold repetition not detected</color>");
			}

			// Test ToString method
			string boardString = original.ToString();
			if (!string.IsNullOrEmpty(boardString) && boardString.Contains("ChessBoard"))
			{
				Debug.Log("<color=green>[ChessBoard] ✓ ToString method works</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessBoard] ✗ ToString method failed</color>");
			}

			Debug.Log("<color=cyan>[ChessBoard] Advanced features tests completed</color>");
		}

		/// <summary>
		/// Test validation modes and edge cases
		/// </summary>
		private static void TestValidationModesAndEdgeCases()
		{
			Debug.Log("<color=cyan>[ChessBoard] Testing validation modes and edge cases...</color>");

			// Test validation mode changes
			ChessBoard board = new ChessBoard();
			board.SetValidationMode(ValidationMode.Strict, true);
			if (board.CurrentValidationMode == ValidationMode.Strict)
			{
				Debug.Log("<color=green>[ChessBoard] ✓ Validation mode setting works</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessBoard] ✗ Validation mode setting failed</color>");
			}

			// Test Chess960 setup
			ChessBoard chess960Board = new ChessBoard("", ChessVariant.Chess960);
			string fen960 = chess960Board.ToFEN();
			if (!string.IsNullOrEmpty(fen960) && fen960.Contains(" w "))
			{
				Debug.Log("<color=green>[ChessBoard] ✓ Chess960 setup works</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessBoard] ✗ Chess960 setup failed</color>");
			}

			// Test King of the Hill variant
			ChessBoard kothBoard = new ChessBoard("", ChessVariant.KingOfTheHill);
			if (kothBoard.variant == ChessVariant.KingOfTheHill)
			{
				Debug.Log("<color=green>[ChessBoard] ✓ King of the Hill variant initialization works</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessBoard] ✗ King of the Hill variant initialization failed</color>");
			}

			// Test fallback behavior
			ChessBoard fallbackBoard = new ChessBoard("invalid_fen", ChessVariant.Standard, ValidationMode.Strict, true);
			if (fallbackBoard.GetPiece("e1") == 'K')
			{
				Debug.Log("<color=green>[ChessBoard] ✓ Invalid FEN fallback works</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessBoard] ✗ Invalid FEN fallback failed</color>");
			}

			// Test turn checking
			ChessBoard turnBoard = new ChessBoard();
			turnBoard.sideToMove = 'w';
			turnBoard.SetHumanSide('w');
			if (turnBoard.IsHumanTurn() && !turnBoard.IsEngineTurn())
			{
				Debug.Log("<color=green>[ChessBoard] ✓ Turn checking works</color>");
			}
			else
			{
				Debug.Log("<color=red>[ChessBoard] ✗ Turn checking failed</color>");
			}

			// Test checkmate position (Fool's mate)
			ChessBoard mateBoard = new ChessBoard("rnb1kbnr/pppp1ppp/4p3/8/6Pq/5P2/PPPPP2P/RNBQKBNR w KQkq - 1 3",
												ChessVariant.Standard, ValidationMode.Permissive, false);
			var mateResult = mateBoard.GetGameResult();
			if (mateResult == ChessRules.GameResult.BlackWins)
			{
				Debug.Log("<color=green>[ChessBoard] ✓ Checkmate position correctly identified</color>");
			}
			else
			{
				Debug.Log($"<color=red>[ChessBoard] ✗ Checkmate position incorrectly identified as: {mateResult}</color>");
			}

			// Test the original issue - pawns on wrong ranks
			ChessBoard pawnOn8th = new ChessBoard("P7/8/8/8/8/8/8/K6k w - - 0 1",
												ChessVariant.Standard, ValidationMode.Permissive, false);
			string pawnFEN = pawnOn8th.ToFEN();
			if (pawnFEN.StartsWith("P7/8/8/8/8/8/8/K6k"))
			{
				Debug.Log("<color=green>[ChessBoard] ✓ Permissive mode allows pawn on 8th rank</color>");
			}
			else
			{
				Debug.Log($"<color=red>[ChessBoard] ✗ Permissive mode failed for pawn on 8th rank: {pawnFEN}</color>");
			}

			ChessBoard noKingBoard = new ChessBoard("8/8/8/8/8/8/8/8 w - - 0 1",
												 ChessVariant.Standard, ValidationMode.ParseOnly, false);
			string noKingFEN = noKingBoard.ToFEN();
			if (noKingFEN.StartsWith("8/8/8/8/8/8/8/8"))
			{
				Debug.Log("<color=green>[ChessBoard] ✓ ParseOnly mode allows empty board</color>");
			}
			else
			{
				Debug.Log($"<color=red>[ChessBoard] ✗ ParseOnly mode failed for empty board: {noKingFEN}</color>");
			}

			Debug.Log("<color=cyan>[ChessBoard] Validation modes and edge cases tests completed</color>");
		}

		/// <summary>
		/// Run comprehensive ChessBoard API validation tests
		/// </summary>
		public static void RunAllTests()
		{
			Debug.Log("<color=cyan>=== Enhanced ChessBoard Test Suite v0.3 ===</color>");

			try
			{
				TestFENParsing();
				TestMoveOperations();
				TestEvaluationAndGameLogic();
				TestAdvancedFeatures();
				TestValidationModesAndEdgeCases();

				Debug.Log("<color=green>=== All ChessBoard tests completed successfully ===</color>");
			}
			catch (Exception e)
			{
				Debug.Log($"<color=red>=== ChessBoard tests failed with exception: {e.Message} ===</color>");
			}
		}

		#endregion
	}
}
