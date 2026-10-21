using System;
using UnityEngine;

public static class LogUtil
{
    [System.Diagnostics.Conditional("DEBUG_LOG")]
    public static void Log(object message, UnityEngine.Object context = null) { Debug.Log(message, context); }
    public static void LogWarning(object message) { Debug.LogWarning(message); }
    public static void LogError(object message) { Debug.LogError(message); }
    public static void LogException(Exception exception, UnityEngine.Object context = null) { Debug.LogException(exception, context); }
}
