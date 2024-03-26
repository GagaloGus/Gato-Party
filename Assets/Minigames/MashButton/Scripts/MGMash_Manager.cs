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

        //Instancia los players en orden segun su ID (Actor number)
        GameObject player =
            PhotonNetwork.Instantiate(
            prefabName: playerPrefab.name,
            position: spawnerParent.transform.GetChild(PhotonNetwork.LocalPlayer.ActorNumber - 1).position,
            rotation: Quaternion.identity);

        //Espera 2 segundos
        CoolFunctions.Invoke(this, () =>
        {
            //Deja que los players se muevan
            player.GetComponent<MGMash_PlayerController>().canMove = true;

            //Cuenta atras hasta el final del minijuego
            GetComponent<CountdownController>().StartCountdown(
                maxTime: maxTime,
                incrementAmount: 1,
                stringFormat: "00",
                endCounterFunction: () =>
                {
                    player.GetComponent<MGMash_PlayerController>().MinigameFinished();                    
                });
        }, 2);
        
        //Debug de la lista de jugadores
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

    int playerCount = 0;
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //Solo si es el Master Client
        if (PhotonNetwork.IsMasterClient)
        {
            print($"Puntuacion: {targetPlayer.NickName}->{(int)changedProps[Constantes.PlayerKey_MinigameScore]}");

            //Se ejecuta al final para mostrar el ranking
            //Si ha cambiado su puntuacion, y no es la de reseteo ( != -1 )
            if (changedProps.ContainsKey(Constantes.PlayerKey_MinigameScore) && (int)changedProps[Constantes.PlayerKey_MinigameScore] != -1)
            {
                //añadimos la puntuacion al diccionario
                resultPlayerlist.Add(targetPlayer, (int)changedProps[Constantes.PlayerKey_MinigameScore]);
                playerCount++;
            }

            //Si todos los players actualizaron su informacion
            if (playerCount == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                //Ordena la lista de mayor a menor
                var sortedResults = resultPlayerlist.OrderByDescending(x => x.Value);

                //Añade las puntuaciones a un string en orden
                string results = "<color=yellow>-- Ranking --</color>\n";
                int index = 1;
                foreach (System.Collections.Generic.KeyValuePair<Player, int> entry in sortedResults)
                {
                    results += $"{index}# {entry.Key.NickName}: {entry.Value} puntos\n";
                    index++;
                }

                //muestra las puntuaciones para el resto de jugadores
                GetComponent<PhotonView>().RPC(nameof(ShowResults), RpcTarget.All, results);
            }
        }
    }

    [PunRPC]
    void ShowResults(string result)
    {
        resultsText.SetActive(true);
        resultsText.GetComponentInChildren<TMP_Text>().text = result;

        CoolFunctions.Invoke(this, () =>
        {
            PhotonNetwork.LoadLevel("Puntuacion");
        }, 1.5f);
    }
}
