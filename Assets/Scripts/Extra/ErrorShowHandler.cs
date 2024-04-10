using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
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

#if UNITY_EDITOR_WIN
    [MenuItem("GameObject/UI/Custom Objects/Error TMP Text Handler")]
    static void CreateErrorHandlerObject(MenuCommand menuCommand)
    {
        // Crea un nuevo GameObject y añade el componente MyCustomObject
        GameObject newObject = new GameObject("ErrorHandlerText");
        newObject.AddComponent<ErrorShowHandler>();
        newObject.AddComponent<CanvasRenderer>();
        RectTransform rectTransform = newObject.AddComponent<RectTransform>();
        TextMeshProUGUI txt = newObject.AddComponent<TextMeshProUGUI>();

        // Registra el nuevo GameObject en la escena
        GameObjectUtility.SetParentAndAlign(newObject, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(newObject, "Create " + newObject.name);
        Selection.activeObject = newObject;

        newObject.transform.SetParent(FindObjectOfType<BasicButtonFunctions>().transform, false);
        rectTransform.localPosition = Vector3.zero;
        rectTransform.sizeDelta = Vector2.zero;

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;

        txt.color = Color.black;
        txt.fontStyle = FontStyles.Bold;
    }
#endif
}
