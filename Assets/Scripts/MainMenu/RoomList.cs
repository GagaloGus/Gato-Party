using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomList : MonoBehaviourPunCallbacks
{
    public GameObject roomListDisplay;
    public GameObject roomButtonPrefab;
    
    //Actualiza la informacion de los botones
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        for(int i = 0; i < roomList.Count; i++)
        {
            print(roomList[i].Name);
            GameObject roomButton = Instantiate(roomButtonPrefab, Vector3.zero, Quaternion.identity, roomListDisplay.transform.Find("Viewport").Find("Content"));

            roomButton.GetComponent<Room>().roomName.text = roomList[i].Name;
            print($"Players: {roomList[i].PlayerCount}/{roomList[i].MaxPlayers}");
            roomButton.GetComponent<Room>().playerCountText.text = $"Players: {roomList[i].PlayerCount}/{roomList[i].MaxPlayers}";
        }
    }
}
