using Photon.Pun;
using ExitGames.Client.Photon;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;
using System.Linq;

public class MinigameSelector : MonoBehaviourPunCallbacks
{
    [Header("Minigame lists")]
    [SerializeField] List<MinigameInfo> minigames;

    [Header("References")]
    public GameObject minigameDisplay;
    public GameObject loadingScreen;

    private void Start()
    {
        //Guarda todos los minijuegos en Resources
        minigames = Resources.LoadAll<MinigameInfo>($"Minigames").ToList();
        minigameDisplay.SetActive(false);

        if(PhotonNetwork.IsMasterClient)
            RandomizeMinigames();
    }

    //Randomiza los minijuegos nada mas empezar, los guarda en las propiedades de la sala
    void RandomizeMinigames()
    {
        //crea una lista de los minijuegos randomizados
        List<MinigameInfo> tempList = CoolFunctions.ShuffleList(minigames);
        //crea una lista de strings donde iran los nombres de los minijuegos
        List<string> minigameNames = new List<string>();
        
        //Guardan los nombres de los minijuegos
        foreach (MinigameInfo minigame in tempList)
        {
            minigameNames.Add(minigame.name);
        }

        //guarda el array en las custom properties
        Hashtable roomProps = new Hashtable
        {
            [Constantes.MinigameOrder_Room] = minigameNames.ToArray()
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);

        //debug
        string temp = "Minijuegos Randomizados y Sincronizados:\n ";
        foreach (MinigameInfo minigameInfo in tempList)
        {
            temp += $"{minigameInfo.Name}\n";
        }
        print(temp);
    }

    //Llamado desde el boton de Start Game
    public void StartMinigameSelection()
    {
        //El Master Client llama a la funcion y la manda a todos los clientes
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            GetComponent<PhotonView>().RPC(nameof(RPC_StartMinigameSelection), RpcTarget.All);

            //IDs de los jugadores en orden
            List<int> usingIDs = new List<int>();

            foreach (KeyValuePair<int, Player> playerEntry in PhotonNetwork.CurrentRoom.Players)
            {
                foreach (System.Collections.DictionaryEntry entry in playerEntry.Value.CustomProperties)
                {
                    if ((string)entry.Key == Constantes.PlayerKey_Skin)
                    {
                        usingIDs.Add((int)entry.Value);
                        break;
                    }
                }
            }

            Hashtable roomNewProp = new Hashtable
            {
                [Constantes.SkinIDOrder_Room] = usingIDs.ToArray(),
                [Constantes.AmountPlayers_Room] = PhotonNetwork.CurrentRoom.PlayerCount
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomNewProp);
        }
    }

    [PunRPC]
    void RPC_StartMinigameSelection()
    {
        FindObjectOfType<SalaEsperaSettings>().playerReadyButton.interactable = false;
        FindObjectOfType<SalaEsperaSettings>().startGameButton.interactable = false;

        Transform content = minigameDisplay.transform.Find("Display").Find("Content");

        List<MinigameInfo> shuffledMinigames = new List<MinigameInfo>();

        //guarda los minijuegos randomizados en orden en la lista
        Hashtable roomProps = PhotonNetwork.CurrentRoom.CustomProperties;
        foreach (System.Collections.DictionaryEntry entry in roomProps)
        {
            if ((string)entry.Key == Constantes.MinigameOrder_Room)
            {
                string[] minigameString = (string[])entry.Value;

                for (int i = 0; i < minigameString.Length; i++)
                {
                    MinigameInfo mg = Resources.Load<MinigameInfo>($"Minigames/{minigameString[i]}");

                    shuffledMinigames.Add(mg);
                }
            }
        }

        foreach (Transform child in content.transform)
        {
            child.gameObject.SetActive(false);
        }

        for (int i = 0; i < shuffledMinigames.Count; i++)
        {
            Transform display = content.GetChild(i);

            display.Find("Image").GetComponent<Image>().sprite = shuffledMinigames[i].Icon;
            display.Find("Name").GetComponent<TMP_Text>().text = shuffledMinigames[i].Name;
        }

        StartCoroutine(ShowMinigamesTimeLine(shuffledMinigames));
    }

    System.Collections.IEnumerator ShowMinigamesTimeLine(List<MinigameInfo> minigames)
    {
        Transform content = minigameDisplay.transform.Find("Display").Find("Content");

        minigameDisplay.SetActive(true);
        yield return new WaitForSeconds(1);

        for (int i  = 0; i < content.childCount; i++)
        {
            content.GetChild(i).gameObject.SetActive(i < minigames.Count);
            yield return new WaitForSeconds(0.5f);
        }
        yield return new WaitForSeconds(2);

        loadingScreen.SetActive(true);
        CanvasGroup loadingScCanvas = loadingScreen.GetComponent<CanvasGroup>();
        for (float i = 0; i <= 1; i += 0.03f)
        {
            loadingScCanvas.alpha = i;
            yield return null;
        }

        loadingScCanvas.alpha = 1;
        yield return new WaitForSeconds(3.5f);

        PhotonNetwork.LoadLevel("ShowcaseMG");
    }
}
