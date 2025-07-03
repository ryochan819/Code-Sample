using UnityEngine;

public class ConsoleToGUI : MonoBehaviour
{
#if !UNITY_EDITOR
    static string myLog = "";
    private string output;
    private string stack;

    private static ConsoleToGUI instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // prevent duplicates
        }
    }

    void OnEnable()
    {
        Application.logMessageReceived += Log;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= Log;
    }

    public void Log(string logString, string stackTrace, LogType type)
    {
        string logEntry = $"[{type}] {logString}";

        // Include stack trace for errors & exceptions
        if (type == LogType.Error || type == LogType.Exception)
        {
            logEntry += $"\n{stackTrace}";
        }

        myLog = logEntry + "\n\n" + myLog;

        // Optional size limit
        if (myLog.Length > 10000)
        {
            myLog = myLog.Substring(0, 8000);
        }
    }

    void OnGUI()
    {
        //if (!Application.isEditor) //Do not display in editor ( or you can use the UNITY_EDITOR macro to also disable the rest)
        {
            // Top Left
            // x = 10f; y = 10f;
            // Top right
            // x = Screen.width - width; y = 10f;
            
            float width = Screen.width * 0.2f;
            float height = Screen.height * 1f;

            float x = Screen.width - width;
            float y = 10f;

            myLog = GUI.TextArea(new Rect(x, y, Screen.width * 0.2f, Screen.height * 1f), myLog);
        }
    }
#endif
}
