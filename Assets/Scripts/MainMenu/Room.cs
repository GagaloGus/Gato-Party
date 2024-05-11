using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Room : MonoBehaviour
{
    public TMP_Text roomName;
    public TMP_Text playerCountText;

    public RoomInfo roomInfo;

    private void Start()
    {
        roomName = transform.Find("Name").GetComponent<TMP_Text>();
        playerCountText = transform.Find("PlayerCount").GetComponent<TMP_Text>();
    }

    public void JoinRoom()
    {
        FindObjectOfType<RoomHandler>().JoinRoomInList(roomInfo.Name);
    }
}
