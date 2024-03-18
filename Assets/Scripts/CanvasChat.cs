using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CanvasChat : MonoBehaviourPunCallbacks
{
    public TMP_InputField inputField;
    public GameObject messagePrefab;
    public GameObject content;

    public void SendMessage()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            object[] paramsMessage = new object[2];
            paramsMessage[0] = inputField.text;
            paramsMessage[1] = PhotonNetwork.LocalPlayer.NickName;

            //MessageContent message = new MessageContent(inputField.text, PhotonNetwork.LocalPlayer.NickName);
            GetComponent<PhotonView>().RPC(
                methodName: nameof(GetMessage),
                target: RpcTarget.All,
                parameters: paramsMessage);

            inputField.text = "";
        }
    }

    [PunRPC]
    public void GetMessage(object[] paramsMessage)
    {
        GameObject message = Instantiate(messagePrefab, Vector3.zero, Quaternion.identity, content.transform);
        message.transform.Find("Text").GetComponent<TMP_Text>().text = paramsMessage[0].ToString();
        message.transform.Find("Nickname").GetComponent<TMP_Text>().text = paramsMessage[1].ToString();
    }

}
public class MessageContent
{
    public string message;
    public string nickname;

    public MessageContent(string message, string nickname)
    {
        this.message = message;
        this.nickname = nickname;
    }
}
