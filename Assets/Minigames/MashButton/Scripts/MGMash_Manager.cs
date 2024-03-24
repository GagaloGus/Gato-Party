using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MGMash_Manager : MonoBehaviourPunCallbacks
{
    [Header("Minigame Info")]
    public int maxTime;

    [Header("References")]
    public GameObject resultsText;

    [Header("Prefab to spawn")]
    public GameObject playerPrefab;

    [Header("Spawner")]
    public GameObject spawnerParent;

    Dictionary<Player, int> resultPlayerlist = new();

    // Start is called before the first frame update
    void Start()
    {
        resultsText.SetActive(false);

        GameObject player =
            PhotonNetwork.Instantiate(
            prefabName: playerPrefab.name,
            position: spawnerParent.transform.GetChild(PhotonNetwork.LocalPlayer.ActorNumber - 1).position,
            rotation: Quaternion.identity);

        CoolFunctions.Invoke(this, () =>
        {
            player.GetComponent<MGMash_PlayerController>().canMove = true;

            GetComponent<CountdownController>().StartCountdown(
                maxTime: maxTime,
                incrementAmount: 1,
                stringFormat: "00",
                endCounterFunction: () =>
                {
                    player.GetComponent<MGMash_PlayerController>().MinigameFinished();                    
                });
        }, 2);

        string temp = "Lista jugadores: ";
        foreach (System.Collections.Generic.KeyValuePair<int, Player> entry in PhotonNetwork.CurrentRoom.Players)
        {
            if (entry.Value.IsLocal)
                temp += $"{entry.Value} (LOCAL), ";
            else
                temp += $"{entry.Value}, ";

        }
        print(temp);
    }

    int index = 0;
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            print($"{targetPlayer.NickName}:{(int)changedProps[Constantes.PlayerKey_MinigameScore]}");
            if (changedProps.ContainsKey(Constantes.PlayerKey_MinigameScore))
            {
                resultPlayerlist.Add(targetPlayer, (int)changedProps[Constantes.PlayerKey_MinigameScore]);
                index++;
            }

            if (index == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                var sortedResults = resultPlayerlist.OrderByDescending(x => x.Value);

                int index = 1;
                string results = "<color=yellow>-- Ranking --</color>\n";
                foreach (System.Collections.Generic.KeyValuePair<Player, int> entry in sortedResults)
                {
                    results += $"{index}# {entry.Key.NickName}: {entry.Value} puntos\n";
                    index++;
                }

                GetComponent<PhotonView>().RPC(nameof(ShowResults), RpcTarget.All, results);
            }
        }
    }

    [PunRPC]
    void ShowResults(string result)
    {
        resultsText.SetActive(true);
        resultsText.GetComponentInChildren<TMP_Text>().text = result;
    }
}
