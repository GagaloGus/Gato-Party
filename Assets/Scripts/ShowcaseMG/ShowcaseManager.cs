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
    [Header("Display Settings")]
    public float swapImageTime;

    [Header("Minigame info")]
    public TMP_Text Name;
    public TMP_Text Description;
    public TMP_Text HowToPlay;
    public Transform DisplayImage;
    public Image CartuchoImage;
    Transform Puntos;
    Image Image1, Image2;

    [Header("References")]
    public GameObject BlackScreen;

    [Header("Audios")]
    public AudioClip Theme;
    

    MinigameInfo currentMinigame;

    private void Awake()
    {
        BlackScreen.SetActive(true);
    }

    private void Start()
    {
        StartCoroutine(FadeInOutBlack(false));

        Puntos = DisplayImage.Find("Puntos");
        Image1 = DisplayImage.Find("Mask").Find("Image1").GetComponent<Image>();
        Image2 = DisplayImage.Find("Mask").Find("Image2").GetComponent<Image>();

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

        AudioManager.instance.ClearAudioList();
        AudioManager.instance.PlayAmbientMusic(Theme);
    }

    void ShowMinigameInfo(MinigameInfo minigameInfo)
    {
        Name.text = minigameInfo.Name;
        Description.text = minigameInfo.Description;
        HowToPlay.text = minigameInfo.HowToPlay;
        CartuchoImage.sprite = minigameInfo.Icon;

        for (int i = 0; i < Puntos.childCount; i++)
        {
            Puntos.GetChild(i).GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
            Puntos.GetChild(i).gameObject.SetActive(i < minigameInfo.DisplayImages.Length); 
        }

        Image1.sprite = currentMinigame.DisplayImages[0];
        Puntos.GetChild(0).GetComponent<Image>().color = Color.white;

        StartCoroutine(SwapDisplayImages());
    }

    System.Collections.IEnumerator SwapDisplayImages()
    {
        if(currentMinigame.DisplayImages.Length > 1)
        {
            for (int i = 0; i < currentMinigame.DisplayImages.Length; i++)
            {
                yield return new WaitForSeconds(swapImageTime);

                DisplayImage.GetComponent<Animator>().SetTrigger("swap");

                int nextInt = (i == currentMinigame.DisplayImages.Length - 1 ? 0 : i + 1);

                Image1.sprite = currentMinigame.DisplayImages[i];
                foreach (Transform child in Puntos) { child.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f); }

                Image2.sprite = currentMinigame.DisplayImages[nextInt];
                Puntos.GetChild(nextInt).GetComponent<Image>().color = Color.white;

            }

            StartCoroutine(SwapDisplayImages());
        }
        else
        {
            Image1.sprite = currentMinigame.DisplayImages[0];
            Puntos.gameObject.SetActive(false);
        }
    }

    bool AllPlayersReady()
    {
        // Verificar si todos los jugadores en la sala están listos
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!(player.CustomProperties.ContainsKey(Constantes.PlayerKey_Ready_SMG) && (bool)player.CustomProperties[Constantes.PlayerKey_Ready_SMG]))
            {
                return false;
            }
        }
        return true;
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //Si estan los players ready
        if (changedProps.ContainsKey(Constantes.PlayerKey_Ready_SMG))
        {
            if (AllPlayersReady())
            {
                //Si es el Master Client borra el minijuego de las propiedades de la room
                if (PhotonNetwork.IsMasterClient)
                    EraseFirstMinigame();

                StartCoroutine(FadeInOutBlack(true));

                CoolFunctions.Invoke(this, () =>
                {
                    PhotonNetwork.LoadLevel(currentMinigame.MG_SceneName);
                }, 4);
            }
            
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
        yield return new WaitForSeconds(0.7f);

        CanvasGroup patata = BlackScreen.GetComponent<CanvasGroup>();
        if (fadeIn)
        {
            yield return new WaitForSeconds(1f);

            patata.alpha = 0;
            BlackScreen.SetActive(true);       
            for(float i = 0; i <= 1; i += 0.1f)
            {
                patata.alpha = i;
                yield return null;
            }
            patata.alpha = 1;
            AudioManager.instance.StopAmbientMusic();
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
