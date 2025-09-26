using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loggers : MonoBehaviour
{
    public bool IsDebug { get; set; }

    public void LogInfo(string className, string methodName, string message)
    {
        if (!IsDebug) return;
        Debug.Log($"[{className}]::::({methodName})::::{message}");
    }
    public void LogWarning(string className, string methodName, string message)
    {
        if (!IsDebug) return;
        Debug.LogWarning($"[{className}]::::({methodName}):::: {message}");
    }
    public void LogError(string className, string methodName, string message)
    {
        if (!IsDebug) return;
        Debug.LogError($"[{className}]::::({methodName})::::{message}");
    }
}