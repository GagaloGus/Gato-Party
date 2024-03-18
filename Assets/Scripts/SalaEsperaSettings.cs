using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SalaEsperaSettings : MonoBehaviourPunCallbacks
{
    public GameObject playerList;

    public TMP_Text roomName;
    public TMP_Text playerCount;

    // Start is called before the first frame update
    void Start()
    {
        UpdatePlayerCount();
        roomName.text = $"Room Name:\n{PhotonNetwork.CurrentRoom.Name}";
    }

    public override void OnJoinedRoom()
    {
        UpdatePlayerCount();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerCount();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerCount();
    }

    void UpdatePlayerCount()
    {
        playerCount.text = $"Players: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
    }
}
