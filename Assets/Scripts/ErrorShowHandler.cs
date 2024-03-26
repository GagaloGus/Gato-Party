using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorShowHandler : MonoBehaviour
{
    TMP_Text errorText;

    private void Awake()
    {
        errorText = GetComponent<TMP_Text>();
    }
    // Start is called before the first frame update
    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logText, string stackTrace, LogType logType)
    {
        gameObject.SetActive(true);

        if(logText.Length > 200)
            logText = logText.Substring(0, 200);

        if(stackTrace.Length > 200)
            stackTrace = stackTrace.Substring(0, 200);

        string debugString = errorText.text + $"{logType}:{logText}\n{stackTrace}\n\n";
        

        errorText.text = debugString.Length > 500 ? debugString.Substring(200) : debugString;
    }
}
