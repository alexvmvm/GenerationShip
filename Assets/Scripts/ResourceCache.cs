using System;
using System.Collections.Generic;
using UnityEngine;

public static class ResourceCache
{
    // Keyed by (Type + "|" + path) so different types can share the same path safely.
    private static readonly Dictionary<string, UnityEngine.Object> cache = new();

    /// <summary>
    /// Get a resource from Resources/ and cache it. Path is relative to a Resources folder and has no extension.
    /// Example: "UI/Icons/Sword"
    /// </summary>
    public static T Get<T>(string path) where T : UnityEngine.Object
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null/empty.", nameof(path));

        string key = MakeKey<T>(path);

        if (cache.TryGetValue(key, out var obj) && obj != null)
            return (T)obj;

        // Load and cache (including null results? We choose NOT to cache nulls by default).
        T loaded = Resources.Load<T>(path);
        if (loaded == null)
        {
            // Optional: log to help catch typos.
            Debug.LogWarning($"ResCache: Resources.Load<{typeof(T).Name}> failed for path '{path}'.");
            // Don't cache null by default to allow fixing resources without restarting play mode.
            cache.Remove(key);
            return null;
        }

        cache[key] = loaded;
        return loaded;
    }

    /// <summary>
    /// Convenience: get a Sprite from Resources and cache it.
    /// </summary>
    public static Sprite Sprite(string path) => Get<Sprite>(path);

    /// <summary>
    /// Convenience: get a Texture2D from Resources and cache it.
    /// </summary>
    public static Texture2D Texture(string path) => Get<Texture2D>(path);

    /// <summary>
    /// Convenience: get an AudioClip from Resources and cache it.
    /// </summary>
    public static AudioClip Audio(string path) => Get<AudioClip>(path);

    /// <summary>
    /// Clear the entire cache.
    /// </summary>
    public static void ClearAll()
    {
        cache.Clear();
    }

    /// <summary>
    /// Clear one cached entry.
    /// </summary>
    public static bool Clear<T>(string path) where T : UnityEngine.Object
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        return cache.Remove(MakeKey<T>(path));
    }

    /// <summary>
    /// Preload a set of resources (useful during loading screens).
    /// </summary>
    public static void Preload<T>(IEnumerable<string> paths) where T : UnityEngine.Object
    {
        foreach (var p in paths)
            _ = Get<T>(p);
    }

    private static string MakeKey<T>(string path) where T : UnityEngine.Object
        => typeof(T).FullName + "|" + path;
}