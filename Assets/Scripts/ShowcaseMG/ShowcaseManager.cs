using Photon.Pun;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class ShowcaseManager : MonoBehaviourPunCallbacks
{
    [Header("Minigame info")]
    public TMP_Text Name;
    public TMP_Text Description;
    public TMP_Text HowToPlay;
    public Image DisplayImage;

    [Header("References")]
    public GameObject BlackScreen;

    MinigameInfo currentMinigame;

    private void Awake()
    {
        BlackScreen.SetActive(true);
    }

    private void Start()
    {
        StartCoroutine(FadeInOutBlack(false));

        currentMinigame = null;

        Hashtable playerProps = new Hashtable
        {
            [Constantes.PlayerKey_Eliminated] = false,
            [Constantes.PlayerKey_MinigameScore] = 0,
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        Hashtable roomProps = new Hashtable
        {
            [Constantes.MinigameScene_Room] = "SHOWCASE"
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);

        //Coje todas las propiedades de la room
        Hashtable customRoomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        foreach (System.Collections.DictionaryEntry entry in customRoomProperties)
        {
            if ((string)entry.Key == Constantes.MinigameOrder_Room)
            {
                string[] temp = (string[])entry.Value;

                //Guarda el minijuego en posicion 0, el actual
                MinigameInfo currentMG = Resources.Load<MinigameInfo>($"Minigames/{temp[0]}");
                currentMinigame = currentMG;
                ShowMinigameInfo(currentMG);

                print($"Minigame loaded {currentMG.Name}");
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
            if (!(bool)player.CustomProperties[Constantes.PlayerKey_Ready_SMG])
            {
                return false;
            }
        }
        return true;
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //Si estan los players ready
        if (changedProps.ContainsKey(Constantes.PlayerKey_Ready_SMG) && AllPlayersReady())
        {
            //Si es el Master Client borra el minijuego de las propiedades de la room
            if (PhotonNetwork.IsMasterClient)
                EraseFirstMinigame();

            StartCoroutine(FadeInOutBlack(true));

            CoolFunctions.Invoke(this, () =>
            {
                PhotonNetwork.LoadLevel(currentMinigame.MG_SceneName);
            }, 2);
        }
    }

    void EraseFirstMinigame()
    {
        //Borra el minijuego actual del array de la room
        Hashtable customRoomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        foreach (System.Collections.DictionaryEntry entry in customRoomProperties)
        {
            if ((string)entry.Key == Constantes.MinigameOrder_Room)
            {
                string[] temp = (string[])entry.Value;
                //quita el minijuego del array
                List<string> tempList = temp.ToList();
                tempList.RemoveAt(0);

                //actualiza el array de los minijuegos
                Hashtable roomMiniProps = new Hashtable
                {
                    [Constantes.MinigameOrder_Room] = tempList.ToArray(),
                    [Constantes.RoundsOver_Room] = (tempList.Count == 0)
                };

                PhotonNetwork.CurrentRoom.SetCustomProperties(roomMiniProps);

                break;
            }
        }
    }

    System.Collections.IEnumerator FadeInOutBlack(bool fadeIn)
    {
        yield return new WaitForSeconds(0.2f);

        CanvasGroup patata = BlackScreen.GetComponent<CanvasGroup>();
        if (fadeIn)
        {
            patata.alpha = 0;
            BlackScreen.SetActive(true);       
            for(float i = 0; i <= 1; i += 0.1f)
            {
                patata.alpha = i;
                yield return null;
            }
            patata.alpha = 1;
        }
        else
        {
            patata.alpha = 1;
            BlackScreen.SetActive(true);
            for (float i = 0; i <= 1; i += 0.1f)
            {
                patata.alpha = 1-i;
                yield return null;
            }
            patata.alpha = 0;
            BlackScreen.SetActive(false);
        }
    }

    public void ExitRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        //Al salir de la sala carga la escena del lobby
        SceneManager.LoadScene("MainMenu");
    }
}
