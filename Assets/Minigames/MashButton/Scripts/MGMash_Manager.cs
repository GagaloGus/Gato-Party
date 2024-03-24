using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class MGMash_Manager : MonoBehaviour
{
    [Header("Prefab to spawn")]
    public GameObject playerPrefab;

    [Header("Spawner")]
    public GameObject spawnerParent;

    // Start is called before the first frame update
    void Start()
    {
        GameObject player =
            PhotonNetwork.Instantiate(
            prefabName: playerPrefab.name,
            position: spawnerParent.transform.GetChild(PhotonNetwork.LocalPlayer.ActorNumber - 1).position,
            rotation: Quaternion.identity);

        player.GetComponent<MGMash_PlayerController>().canMove = true;

        print(PhotonNetwork.CurrentRoom.PlayerCount);
        string temp = "Lista jugadores: ";

        foreach(System.Collections.Generic.KeyValuePair<int,Player> entry in PhotonNetwork.CurrentRoom.Players)
        {
            temp += $"{entry.Value}, ";
        }
        print(temp);
    }
}
