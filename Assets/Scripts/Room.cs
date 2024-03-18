using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Room : MonoBehaviour
{
    public TMP_Text roomName;

    private void Start()
    {
        roomName = GetComponentInChildren<TMP_Text>();
    }

    public void JoinRoom()
    {
        FindObjectOfType<CreateAndJoin>().JoinRoomInList(roomName.text);
    }
}
