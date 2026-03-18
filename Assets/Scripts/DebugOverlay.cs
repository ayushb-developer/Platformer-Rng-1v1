using UnityEngine;
using TMPro;

public class DebugOverlay : MonoBehaviour
{
    public static TMP_Text debugText;

    static string log = "";

    void Awake()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    static void HandleLog(string condition, string stackTrace, LogType type)
    {
        log += condition + "\n";

        if (log.Length > 1000)
            log = log.Substring(log.Length - 1000);

        debugText.text = log;
    }
}