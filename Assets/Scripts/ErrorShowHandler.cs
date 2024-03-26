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
        if(logText.Length > 100)
            logText = logText.Substring(0, 100);

        if(stackTrace.Length > 100)
            stackTrace = stackTrace.Substring(0, 100);

        if (errorText.text.Length > 200)
            errorText.text = errorText.text.Substring(0, 200);

        gameObject.SetActive(true);
        errorText.text += $"{logType}:{logText}\n{stackTrace}\n\n";
    }
}
