using Photon.Pun;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Photon.Realtime;

public class ShowcaseManager : MonoBehaviourPunCallbacks
{
    [Header("References")]
    public TMP_Text Name;
    public TMP_Text Description;
    public TMP_Text HowToPlay;
    public Image DisplayImage;
    public GameObject LoadingScreen;

    [Header("Minigame")]
    public MinigameInfo currentMinigame;

    private void Start()
    {
        LoadingScreen.SetActive(false);

        currentMinigame = null;

        Hashtable roomProps = new Hashtable();
        roomProps[Constantes.MinigameScene_Room] = "SHOWCASE";
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);

        //Coje todas las propiedades de la room
        Hashtable customRoomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        foreach(System.Collections.DictionaryEntry entry in customRoomProperties)
        {
            if((string)entry.Key == Constantes.MinigameOrder_Room) 
            {
                string[] temp = (string[])entry.Value;

                MinigameInfo currentMG = Resources.Load<MinigameInfo>($"Minigames/{temp[0]}");
                currentMinigame = currentMG;
                ShowMinigameInfo(currentMG);

                print($"Minigame loaded {currentMG.Name}");
            }
        }
    }

    //Borra el minijuego actual del hastable de la room cuando todos los players se hayan unido
    int playersJoined = 0;
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        int expectedPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
        playersJoined++;

        if (playersJoined == expectedPlayers)
        {
            //Coje todas las propiedades de la room
            Hashtable customRoomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            foreach (System.Collections.DictionaryEntry entry in customRoomProperties)
            {
                if ((string)entry.Key == Constantes.MinigameOrder_Room)
                {
                    string[] temp = (string[])entry.Value;

                    //El Master Client borra el minijuego de la lista
                    if (PhotonNetwork.IsMasterClient)
                    {
                        //quita el minijuego del array
                        List<string> tempList = temp.ToList();
                        tempList.RemoveAt(0);

                        //actualiza el array de los minijuegos
                        Hashtable roomMiniProps = new Hashtable();
                        roomMiniProps[Constantes.MinigameOrder_Room] = tempList.ToArray();
                        PhotonNetwork.CurrentRoom.SetCustomProperties(roomMiniProps);
                    }
                }
            }
        }
    }

    void ShowMinigameInfo(MinigameInfo minigameInfo)
    {
        Name.text = minigameInfo.Name;
        Description.text = minigameInfo.Description;
        HowToPlay.text = minigameInfo.HowToPlay;
        DisplayImage.sprite = minigameInfo.DisplayImage;
    }

    bool AllPlayersReady()
    {
        // Verificar si todos los jugadores en la sala están listos
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!(player.CustomProperties.ContainsKey(Constantes.ReadyPlayerKey_SMG) && (bool)player.CustomProperties[Constantes.ReadyPlayerKey_SMG]))
            {
                return false;
            }
        }
        return true;
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if(changedProps.ContainsKey(Constantes.ReadyPlayerKey_SMG) && AllPlayersReady())
        {
            FindObjectOfType<CountdownController>().StartCountdown(5, StartMinigame, "Starting minigame...");
        }
    }

    public void StartMinigame()
    {
        print($"Loading minigame {currentMinigame.Name}");
        LoadingScreen.SetActive(true);
        CoolFunctions.Invoke(this, () =>
        {
            PhotonNetwork.LoadLevel(currentMinigame.MG_SceneName);
        }, 2);
    }
}
