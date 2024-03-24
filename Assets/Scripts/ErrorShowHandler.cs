using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorShowHandler : MonoBehaviour
{
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
            logText.Substring(0, 100);

        if(stackTrace.Length > 100)
            stackTrace.Substring(0, 100);

        if (GetComponent<TMP_Text>().text.Length > 300)
            GetComponent<TMP_Text>().text = "";

        gameObject.SetActive(true);
        GetComponent<TMP_Text>().text += $"{logType}:{logText}\n{stackTrace}\n\n";
    }
}
