using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// StreamingAssetsInspector
/// - Lists StreamingAssets (editor/standalone) or reads a manifest on Android/WebGL.
/// - Reads file contents (text or bytes) in a coroutine-safe way using UnityWebRequest on platforms where direct IO isn't available.
/// - Example usage shown in Start() and public helper methods for manual calls.
/// </summary>
public class StreamingAssetsInspector : MonoBehaviour
{
	[Tooltip("If true, list files recursively (Editor/Standalone only).")]
	public bool recursive = true;

	[Tooltip("Name of manifest file placed inside StreamingAssets that lists files (one per line). Useful for Android/WebGL.")]
	public string manifestFileName = "files.txt";

	/// <summary>
	/// Last result: full multiline output describing the last listing operation or file read.
	/// </summary>
	public string LastResult { get; private set; }

	void Awake()
	{
		Debug.Log("Awake(): " + this);
		// Example: list files on start
		StartCoroutine(ListStreamingAssetsCoroutine());
	}

	/// <summary>
	/// Coroutine that lists StreamingAssets contents and fills LastResult.
	/// On Editor/Standalone it uses Directory.GetFiles.
	/// On Android/WebGL it attempts to load a manifest (files.txt) from StreamingAssets.
	/// </summary>
	public IEnumerator ListStreamingAssetsCoroutine()
	{
		LastResult = string.Empty;
		string path = Application.streamingAssetsPath;
		Debug.Log($"StreamingAssets path: {path}");
		// Debug.Log($"Platform: {Application.platform}");

		// Standalone / Editor -> we can enumerate the folder directly
		if (Application.platform == RuntimePlatform.WindowsPlayer ||
			Application.platform == RuntimePlatform.OSXPlayer ||
			Application.platform == RuntimePlatform.LinuxPlayer ||
			Application.isEditor)
		{
			try
			{
				string[] files = Directory.GetFiles(path, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
				var sb = new System.Text.StringBuilder();
				sb.AppendLine($"Found {files.Length} files under {path}:");
				foreach (var f in files)
				{
					sb.AppendLine(f);
					Debug.Log("[StreamingAssets] " + f);
				}

				LastResult = sb.ToString();
			}
			catch (Exception ex)
			{
				LastResult = $"Error listing streaming assets: {ex}";
				Debug.LogError(LastResult);
			}

			yield break; // done
		}
		// Android / WebGL / other platforms -> try manifest
		else
		{
			string manifestUrl = PathCombineForStreamingAssets(path, manifestFileName);
			Debug.Log($"Attempting to read manifest at: {manifestUrl}");
			using (UnityWebRequest uwr = UnityWebRequest.Get(manifestUrl))
			{
				yield return uwr.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
				bool err = uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError;
#else
                bool err = uwr.isNetworkError || uwr.isHttpError;
#endif
				if (err)
				{
					LastResult = $"Could not read manifest '{manifestFileName}' from StreamingAssets. Error: {uwr.error}\nPlatform: {Application.platform}\nIf you need a listing on this platform, run the Editor menu: Tools/Generate StreamingAssets Manifest.";
					Debug.LogWarning(LastResult);
				}
				else
				{
					string text = uwr.downloadHandler.text;
					var sb = new System.Text.StringBuilder();
					sb.AppendLine($"Manifest {manifestFileName} contents (from StreamingAssets):");
					sb.AppendLine(text);
					LastResult = sb.ToString();

					// Optionally log each entry
					var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
					foreach (var line in lines)
						Debug.Log("[StreamingAssets (manifest)] " + line);
				}
			}
		}
	}

	/// <summary>
	/// Coroutine to read a file from StreamingAssets.
	/// - For Editor/Standalone: reads directly via File.ReadAllText if path is a local path.
	/// - For Android/WebGL: uses UnityWebRequest to fetch the file via the streaming assets URL.
	/// </summary>
	/// <param name="relativePath">Path relative to StreamingAssets root, e.g. "data/myfile.txt"</param>
	/// <returns></returns>
	public IEnumerator ReadStreamingAssetTextCoroutine(string relativePath)
	{
		LastResult = string.Empty;
		string saPath = Application.streamingAssetsPath;
		string fullPathPlatform = PathCombineForStreamingAssets(saPath, relativePath);

		// Editor / Standalone: try direct file IO if path is local
		if (Application.isEditor ||
			Application.platform == RuntimePlatform.WindowsPlayer ||
			Application.platform == RuntimePlatform.OSXPlayer ||
			Application.platform == RuntimePlatform.LinuxPlayer)
		{
			string fullFilesystemPath = Path.Combine(saPath, relativePath);
			if (File.Exists(fullFilesystemPath))
			{
				try
				{
					string text = File.ReadAllText(fullFilesystemPath);
					LastResult = text;
					Debug.Log($"Read streaming asset (file IO): {fullFilesystemPath}");
				}
				catch (Exception ex)
				{
					LastResult = $"Error reading file '{fullFilesystemPath}': {ex}";
					Debug.LogError(LastResult);
				}

				yield break;
			}
			// Fallthrough: if not found using File IO, we still try UnityWebRequest (rare)
		}

		// Android / WebGL / fallback: UnityWebRequest
		using (UnityWebRequest uwr = UnityWebRequest.Get(fullPathPlatform))
		{
			yield return uwr.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
			bool err = uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError;
#else
            bool err = uwr.isNetworkError || uwr.isHttpError;
#endif
			if (err)
			{
				LastResult = $"Error reading streaming asset '{relativePath}' via UnityWebRequest: {uwr.error}\nRequested URL: {fullPathPlatform}";
				Debug.LogError(LastResult);
			}
			else
			{
				LastResult = uwr.downloadHandler.text;
				Debug.Log($"Read streaming asset via UnityWebRequest: {relativePath} (bytes: {uwr.downloadedBytes})");
			}
		}
	}

	/// <summary>
	/// Read streaming asset bytes (useful for binary files).
	/// Example usage: yield return StartCoroutine(ReadStreamingAssetBytesCoroutine("models/mybin.dat", bytes => { ... }));
	/// </summary>
	public IEnumerator ReadStreamingAssetBytesCoroutine(string relativePath, Action<byte[]> onComplete)
	{
		string saPath = Application.streamingAssetsPath;
		string fullPathPlatform = PathCombineForStreamingAssets(saPath, relativePath);

		// Editor / Standalone fast path
		if (Application.isEditor ||
			Application.platform == RuntimePlatform.WindowsPlayer ||
			Application.platform == RuntimePlatform.OSXPlayer ||
			Application.platform == RuntimePlatform.LinuxPlayer)
		{
			string fullFilesystemPath = Path.Combine(saPath, relativePath);
			if (File.Exists(fullFilesystemPath))
			{
				try
				{
					byte[] data = File.ReadAllBytes(fullFilesystemPath);
					onComplete?.Invoke(data);
					yield break;
				}
				catch (Exception ex)
				{
					Debug.LogError($"Error reading bytes from '{fullFilesystemPath}': {ex}");
				}
			}
		}

		using (UnityWebRequest uwr = UnityWebRequest.Get(fullPathPlatform))
		{
			yield return uwr.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
			bool err = uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError;
#else
            bool err = uwr.isNetworkError || uwr.isHttpError;
#endif
			if (err)
			{
				Debug.LogError($"Error reading bytes via UnityWebRequest for '{relativePath}': {uwr.error}");
				onComplete?.Invoke(null);
			}
			else
			{
				onComplete?.Invoke(uwr.downloadHandler.data);
			}
		}
	}

	/// <summary>
	/// Utility: combine a path to a streaming assets "URL" correctly across platforms.
	/// Application.streamingAssetsPath may already be a "jar:file://" URI on Android.
	/// </summary>
	private string PathCombineForStreamingAssets(string streamingAssetsPath, string relative)
	{
		// If streamingAssetsPath already looks like a URL (starts with "jar:" or "http" or "file:" or "content:")
		// then simply append with a slash.
		if (streamingAssetsPath.StartsWith("jar:") ||
			streamingAssetsPath.StartsWith("file:") ||
			streamingAssetsPath.StartsWith("http:") ||
			streamingAssetsPath.StartsWith("https:") ||
			streamingAssetsPath.StartsWith("content:"))
		{
			// Ensure correct slash (no duplicate)
			if (!streamingAssetsPath.EndsWith("/")) streamingAssetsPath += "/";
			return streamingAssetsPath + relative;
		}
		else
		{
			// Regular filesystem path
			return Path.Combine(streamingAssetsPath, relative);
		}
	}
}
