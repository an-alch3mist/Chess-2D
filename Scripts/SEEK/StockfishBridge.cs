using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace GPTDeepResearch
{
	/// <summary>
	/// Unity Stockfish bridge with coroutine-first API.
	/// Provides non-blocking chess engine communication via background threads.
	/// </summary>
	public class StockfishBridge : MonoBehaviour
	{
		[Header("Engine Configuration")]
		[SerializeField] private int defaultTimeoutMs = 30000;
		[SerializeField] private bool enableDebugLogging = true;

		// Events
		public UnityEvent<string> OnEngineLine = new UnityEvent<string>();

		// Public properties
		public string LastRawOutput { get; private set; } = "";
		public bool IsEngineRunning => engineProcess != null && !engineProcess.HasExited;
		public bool IsReady { get; private set; } = false;

		// Private fields
		private Process engineProcess;
		private Thread readerThread;
		private volatile bool shouldStop = false;
		private string tempEnginePath;

		// Thread-safe communication
		private readonly ConcurrentQueue<string> incomingLines = new ConcurrentQueue<string>();
		private readonly ConcurrentQueue<string> pendingCommands = new ConcurrentQueue<string>();

		// Request tracking
		private volatile bool waitingForBestMove = false;
		private volatile bool currentRequestCompleted = false;
		private readonly List<string> currentRequestOutput = new List<string>();
		private readonly object requestLock = new object();

		#region Unity Lifecycle

		private void Update()
		{
			// Main thread: drain incoming lines and fire events
			while (incomingLines.TryDequeue(out string line))
			{
				OnEngineLine?.Invoke(line);

				if (enableDebugLogging)
					UnityEngine.Debug.Log($"[Stockfish] < {line}");

				// Track lines for current request
				lock (requestLock)
				{
					if (waitingForBestMove)
					{
						currentRequestOutput.Add(line);

						if (line.StartsWith("bestmove"))
						{
							waitingForBestMove = false;
							currentRequestCompleted = true;
							LastRawOutput = string.Join("\n", currentRequestOutput);
						}
					}
				}

				// Track readiness
				if (line == "readyok")
				{
					IsReady = true;
				}
			}
		}

		// MODIFY: Remove OnDestroy, OnDisable, OnApplicationPause, OnApplicationFocus
		// since we want engine to persist through focus changes

		// REPLACE: Only stop engine on application quit
		private void OnApplicationQuit()
		{
			StopEngine();
		}

		#endregion

		#region Public API

		/// <summary>
		/// Start the Stockfish engine process
		/// </summary>
		public void StartEngine()
		{
			if (IsEngineRunning)
			{
				if (enableDebugLogging)
					UnityEngine.Debug.Log("[Stockfish] Engine already running");
				return;
			}

			try
			{
				string enginePath = GetEngineExecutablePath();
				if (string.IsNullOrEmpty(enginePath))
				{
					UnityEngine.Debug.LogError("[Stockfish] Engine executable not found in StreamingAssets/sf-engine.exe");
					return;
				}

				StartEngineProcess(enginePath);
				StartBackgroundReader();

				if (enableDebugLogging)
					UnityEngine.Debug.Log("[Stockfish] Engine started successfully");
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogError($"[Stockfish] Failed to start engine: {e.Message}");
			}
		}

		/// <summary>
		/// Stop the engine and clean up resources
		/// </summary>
		public void StopEngine()
		{
			if (!IsEngineRunning && readerThread == null)
				return;

			shouldStop = true;

			try
			{
				// Try graceful shutdown first
				if (IsEngineRunning)
				{
					SendCommand("quit");

					if (!engineProcess.WaitForExit(2000))
					{
						if (enableDebugLogging)
							UnityEngine.Debug.Log("[Stockfish] Forcing engine termination");
						engineProcess.Kill();
					}
				}
			}
			catch (Exception e)
			{
				if (enableDebugLogging)
					UnityEngine.Debug.LogWarning($"[Stockfish] Exception during engine shutdown: {e.Message}");
			}
			finally
			{
				// Cleanup
				if (readerThread != null)
				{
					readerThread.Join(1000);
					readerThread = null;
				}

				if (engineProcess != null)
				{
					engineProcess.Dispose();
					engineProcess = null;
				}

				CleanupTempFile();

				IsReady = false;
				shouldStop = false;

				if (enableDebugLogging)
					UnityEngine.Debug.Log("[Stockfish] Engine stopped and cleaned up");
			}
		}

		/// <summary>
		/// Get next move from Stockfish using coroutine pattern.
		/// After completion, check LastRawOutput for full engine response.
		/// </summary>
		/// <param name="fen">Position in FEN notation (or "startpos")</param>
		/// <param name="movetimeMs">Time limit in milliseconds (-1 to use depth instead)</param>
		/// <param name="depth">Search depth (-1 to use movetime instead)</param>
		/// <param name="elo">Engine strength (-1 for maximum strength)</param>
		public IEnumerator GetNextMoveCoroutine(string fen, int movetimeMs = 2000, int depth = -1, int elo = -1)
		{
			if (!IsEngineRunning)
			{
				UnityEngine.Debug.LogError("[Stockfish] Engine not running. Call StartEngine() first.");
				LastRawOutput = "ERROR: Engine not running";
				yield break;
			}

			bool setupSuccessful = false;
			bool requestTimedOut = false;
			bool requestAborted = false;
			float startTime = Time.time;
			float timeoutSeconds = (movetimeMs > 0 ? movetimeMs + 5000 : defaultTimeoutMs) / 1000f;

			// Setup request - NO YIELDS INSIDE TRY/CATCH
			try
			{
				// Prepare request tracking
				lock (requestLock)
				{
					currentRequestOutput.Clear();
					waitingForBestMove = true;
					currentRequestCompleted = false;
				}

				// Configure engine strength if specified
				if (elo > 0)
				{
					SendCommand($"setoption name UCI_LimitStrength value true");
					SendCommand($"setoption name UCI_Elo value {elo}");
				}
				else
				{
					SendCommand($"setoption name UCI_LimitStrength value false");
				}

				// Set position
				if (fen == "startpos" || string.IsNullOrEmpty(fen))
				{
					SendCommand("position startpos");
				}
				else
				{
					SendCommand($"position fen {fen}");
				}

				// Start search
				string goCommand = "go";
				if (depth > 0)
				{
					goCommand += $" depth {depth}";
				}
				else
				{
					goCommand += $" movetime {movetimeMs}";
				}

				SendCommand(goCommand);
				setupSuccessful = true;
			}
			catch (Exception e)
			{
				requestAborted = true;
				setupSuccessful = false;
				UnityEngine.Debug.LogError($"[Stockfish] Exception in GetNextMoveCoroutine setup: {e.Message}");
			}

			// If setup failed, exit early
			if (!setupSuccessful)
			{
				LastRawOutput = "ERROR: Failed to setup request";
				yield break;
			}

			// Wait for completion or timeout - YIELDS OUTSIDE TRY/CATCH
			while (!requestTimedOut && !requestAborted)
			{
				yield return null;

				// Check timeout
				if (Time.time - startTime > timeoutSeconds)
				{
					requestTimedOut = true;
					break;
				}

				// Check completion
				lock (requestLock)
				{
					if (currentRequestCompleted)
						break;
				}

				// Check if engine died
				if (!IsEngineRunning)
				{
					requestAborted = true;
					break;
				}
			}

			// Handle error cases
			if (requestTimedOut)
			{
				SendCommand("stop");  // Try to stop the search
				LastRawOutput = $"ERROR: Request timed out after {timeoutSeconds}s";
				if (enableDebugLogging)
					UnityEngine.Debug.LogWarning("[Stockfish] Request timed out");
			}
			else if (requestAborted)
			{
				LastRawOutput = "ERROR: Request aborted (engine died or exception)";
				if (enableDebugLogging)
					UnityEngine.Debug.LogError("[Stockfish] Request aborted");
			}
		}

		/// <summary>
		/// Send arbitrary UCI command to engine
		/// </summary>
		public void SendCommand(string command)
		{
			if (!IsEngineRunning)
			{
				if (enableDebugLogging)
					UnityEngine.Debug.LogWarning("[Stockfish] Cannot send command - engine not running");
				return;
			}

			try
			{
				engineProcess.StandardInput.WriteLine(command);
				engineProcess.StandardInput.Flush();

				if (enableDebugLogging)
					UnityEngine.Debug.Log($"[Stockfish] > {command}");
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogError($"[Stockfish] Failed to send command '{command}': {e.Message}");
			}
		}

		/// <summary>
		/// Initialize engine and wait until ready
		/// </summary>
		public IEnumerator InitializeEngineCoroutine()
		{
			if (!IsEngineRunning)
			{
				yield break;
			}

			IsReady = false;
			SendCommand("uci");
			SendCommand("isready");

			// Wait for readyok response
			float startTime = Time.time;
			while (!IsReady && Time.time - startTime < 10f)
			{
				yield return null;
			}

			if (!IsReady)
			{
				UnityEngine.Debug.LogError("[Stockfish] Engine failed to initialize within 10 seconds");
			}
		}

		#endregion

		#region Private Methods

		private string GetEngineExecutablePath()
		{
			string streamingAssetsPath = Path.Combine(Application.streamingAssetsPath, "sf-engine.exe");

			if (!File.Exists(streamingAssetsPath))
			{
				return null;
			}

			// On some platforms, we may need to copy to a writable location
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        try
        {
            // Try to execute from StreamingAssets first
            return streamingAssetsPath;
        }
        catch
        {
            // If that fails, copy to temp directory
            return CopyToTempLocation(streamingAssetsPath);
        }
#else
			return streamingAssetsPath;
#endif
		}

		private string CopyToTempLocation(string sourcePath)
		{
			try
			{
				tempEnginePath = Path.Combine(Path.GetTempPath(), $"sf-engine-{System.Guid.NewGuid():N}.exe");
				File.Copy(sourcePath, tempEnginePath, true);

				if (enableDebugLogging)
					UnityEngine.Debug.Log($"[Stockfish] Copied engine to temp location: {tempEnginePath}");

				return tempEnginePath;
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogError($"[Stockfish] Failed to copy engine to temp location: {e.Message}");
				return null;
			}
		}

		private void CleanupTempFile()
		{
			if (!string.IsNullOrEmpty(tempEnginePath) && File.Exists(tempEnginePath))
			{
				try
				{
					File.Delete(tempEnginePath);
					if (enableDebugLogging)
						UnityEngine.Debug.Log("[Stockfish] Cleaned up temp engine file");
				}
				catch (Exception e)
				{
					if (enableDebugLogging)
						UnityEngine.Debug.LogWarning($"[Stockfish] Failed to cleanup temp file: {e.Message}");
				}
				finally
				{
					tempEnginePath = null;
				}
			}
		}

		private void StartEngineProcess(string enginePath)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = enginePath,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			engineProcess = Process.Start(startInfo);

			if (engineProcess == null)
			{
				throw new Exception("Failed to start engine process");
			}
		}

		private void StartBackgroundReader()
		{
			shouldStop = false;
			readerThread = new Thread(BackgroundReaderLoop)
			{
				IsBackground = true,
				Name = "StockfishReader"
			};
			readerThread.Start();
		}

		private void BackgroundReaderLoop()
		{
			try
			{
				string line;
				while (!shouldStop && engineProcess != null && !engineProcess.HasExited)
				{
					line = engineProcess.StandardOutput.ReadLine();
					if (line != null)
					{
						incomingLines.Enqueue(line);
					}
				}
			}
			catch (Exception e)
			{
				if (!shouldStop)
				{
					incomingLines.Enqueue($"ERROR: Reader thread exception: {e.Message}");
				}
			}
		}

		#endregion
	}
}