using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;

public class SalaEsperaPlayerList : MonoBehaviourPunCallbacks
{
    [Header("Prefabs")]
    public GameObject playerTag;

    [Header("List")]
    public GameObject playerList;

    private void Start()
    {
        UpdatePlayerList();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }

    void UpdatePlayerList()
    {
        Transform display = playerList.transform.Find("Display");

        //Destruye todos los tags
        foreach(Transform child in display) 
        { Destroy(child.gameObject); }

        Transform MasterTag = null;

        //Crea tags acorde a los players que hay en la sala
        foreach (KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
        {
            GameObject tag = Instantiate(playerTag, Vector3.zero, Quaternion.identity, display);

            tag.transform.Find("Name").GetComponent<TMP_Text>().text = playerInfo.Value.NickName;

            //Si somos el owner activa la coronita
            if(playerInfo.Value.IsMasterClient)
            {
                tag.transform.Find("IsOwner").gameObject.SetActive(true);
                MasterTag = tag.transform;
            }
        }

        MasterTag.SetAsFirstSibling();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        // Se llama automaticamente cuando las propiedades personalizadas de un jugador cambian
        if (changedProps.ContainsKey(Constantes.PlayerKey_Ready_SalaEspera))
        {
            bool isReady = (bool)changedProps[Constantes.PlayerKey_Ready_SalaEspera];

            //Activa la imagen de isReady si el player esta listo
            Transform display = playerList.transform.Find("Display");
            foreach (Transform tag in display)
            {
                if (tag.Find("Name").GetComponent<TMP_Text>().text == targetPlayer.NickName)
                {
                    tag.transform.Find("IsReady").gameObject.SetActive(isReady);
                    break; // Termina el bucle una vez que se encuentra el tag del jugador
                }
            }
        }
    }

}
