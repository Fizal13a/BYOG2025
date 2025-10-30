using UnityEngine;

public static class DebugLogger
{
    public static bool isEnabled
    {
        get { return true; }
    }

    public static void Log(object message) { if(isEnabled) { Debug.Log(message); } }
    public static void Log(object message, Object context) { if(isEnabled) { Debug.Log(message, context); } }

    public static void Log(object message, string color)
    {
        if(isEnabled)
        {
            Debug.Log($"<color={color}>{message}</color>");
        }
    }
    public static void Log(object message, string color, Object context)
    {
        if(isEnabled)
        {
            Debug.Log($"<color={color}>{message}</color>", context);
        }
    }

    public static void LogWarning(object message) { if(isEnabled) { Debug.LogWarning(message); } }
    public static void LogWarning(object message, Object context) { if(isEnabled) { Debug.LogWarning(message, context); } }

    public static void LogError(object message) { if(isEnabled) { Debug.LogError(message); } }
    public static void LogError(object message, Object context) { if(isEnabled) { Debug.LogError(message, context); } }
}