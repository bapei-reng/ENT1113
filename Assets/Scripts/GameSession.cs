using System.Collections.Generic;
using UnityEngine;

public static class GameSession
{
    private static readonly Dictionary<string, float> values = new Dictionary<string, float>();

    public static void SetFloat (string key, float value)
    {
        if (string.IsNullOrEmpty (key))
            return;

        values[key] = value;
    }

    public static float GetFloat (string key, float fallback = 0f)
    {
        if (string.IsNullOrEmpty (key))
            return fallback;

        return values.TryGetValue (key, out float value) ? value : fallback;
    }

    public static void SetInt (string key, int value)
    {
        SetFloat (key, value);
    }

    public static int GetInt (string key, int fallback = 0)
    {
        return Mathf.RoundToInt (GetFloat (key, fallback));
    }

    public static bool Has (string key)
    {
        return !string.IsNullOrEmpty (key) && values.ContainsKey (key);
    }

    public static void Remove (string key)
    {
        if (!string.IsNullOrEmpty (key))
            values.Remove (key);
    }

    public static void Clear ()
    {
        values.Clear ();
    }
}
