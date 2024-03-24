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
        gameObject.SetActive(true);
        GetComponent<TMP_Text>().text += $"{logType}:{logText}\n{stackTrace}\n\n";
    }
}
