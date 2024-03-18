using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomList : MonoBehaviourPunCallbacks
{
    public GameObject roomButtonPrefab;
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        for(int i = 0; i < roomList.Count; i++)
        {
            print(roomList[i].Name);
            GameObject roomButton = Instantiate(roomButtonPrefab, Vector3.zero, Quaternion.identity, transform.Find("Viewport").Find("Content"));

            roomButton.GetComponent<Room>().roomName.text = roomList[i].Name;
        }
    }
}
