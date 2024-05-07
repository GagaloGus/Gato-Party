using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CanvasChat : MonoBehaviourPunCallbacks
{
    public TMP_InputField inputField;
    public Transform content;

    [Header("Prefabs")]
    public GameObject messagePrefab;
    public GameObject systemMessagePrefab;

    private void Update()
    {
        //No funciona
        /*
        if (Input.GetKeyDown(KeyCode.Return))
        {
            print("return");
            if (inputField.isFocused)
            {
                Debug.Log("AAAAAA");
                SendMessage();
            }
        }*/
        
    }

    public void SendSystemMessage(string message)
    {
        GetComponent<PhotonView>().RPC(nameof(GetSystemMessage), RpcTarget.All, message);
    }

    [PunRPC]
    void GetSystemMessage(string text)
    {
        GameObject message = Instantiate(systemMessagePrefab, Vector3.zero, Quaternion.identity, content.transform);
        message.transform.Find("Text").GetComponent<TMP_Text>().text = text;

        CanvasUpdate();
    }

    public void SendMessage()
    {
        if (!string.IsNullOrWhiteSpace(inputField.text))
        {
            inputField.text = inputField.text.Trim(); //elimina espacios antes y despues del mensaje

            object[] paramsMessage = new object[3];
            paramsMessage[0] = inputField.text;
            paramsMessage[1] = PhotonNetwork.LocalPlayer.NickName;
            paramsMessage[2] = (int)PhotonNetwork.LocalPlayer.CustomProperties[Constantes.PlayerKey_Skin];

            //MessageContent message = new MessageContent(inputField.text, PhotonNetwork.LocalPlayer.NickName);
            GetComponent<PhotonView>().RPC(
                methodName: nameof(GetMessage),
                target: RpcTarget.All,
                parameters: paramsMessage);

            inputField.text = "";
            inputField.DeactivateInputField();
        }
    }

    [PunRPC]
    void GetMessage(object[] paramsMessage)
    {
        try
        {
            Transform previousMessage = content.GetChild(content.transform.childCount - 1);
            //si el nickname del mensaje anterior es el mismo que el actual
            if (previousMessage.Find("Info").Find("Nickname").GetComponent<TMP_Text>().text == paramsMessage[1].ToString())
            {
                previousMessage.Find("Text").GetComponent<TMP_Text>().text += $"\n{paramsMessage[0]}";
                CanvasUpdate();
                return;
            }
        }
        catch { Debug.Log("No hay primer mensaje o no es de tipo Mensaje"); }

        Transform message = Instantiate(messagePrefab, Vector3.zero, Quaternion.identity, content).transform;

        message.Find("Text").GetComponent<TMP_Text>().text = paramsMessage[0].ToString();
        message.Find("Info").Find("Nickname").GetComponent<TMP_Text>().text = paramsMessage[1].ToString();
        message.Find("Info").Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"ReadySprites/{paramsMessage[2]}_notready");

        CanvasUpdate();
    }

    void CanvasUpdate()
    {
        Canvas.ForceUpdateCanvases();

        content.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
        content.GetComponent<ContentSizeFitter>().SetLayoutVertical();

        content.parent.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 0;
    }
}
