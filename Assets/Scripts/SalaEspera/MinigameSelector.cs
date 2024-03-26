using Photon.Pun;
using ExitGames.Client.Photon;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

public class MinigameSelector : MonoBehaviourPunCallbacks
{
    [Header("Minigame lists")]
    public List<MinigameInfo> minigames;

    [Header("References")]
    public GameObject minigameDisplay;
    public GameObject loadingScreen;

    private void Start()
    {
        MinigameInfo[] miniList = Resources.LoadAll<MinigameInfo>($"Minigames");
        foreach (MinigameInfo mini in miniList)
        {
            minigames.Add(mini);
        }

        minigameDisplay.SetActive(false);
        loadingScreen.SetActive(false);

        if(PhotonNetwork.IsMasterClient)
            RandomizeMinigames();
    }

    void RandomizeMinigames()
    {
        //crea una lista de los minijuegos randomizados
        List<MinigameInfo> tempList = CoolFunctions.ShuffleList(minigames);
        //crea una lista de strings donde iran los nombres de los minijuegos
        List<string> minigameNames = new List<string>();

        foreach (MinigameInfo minigame in tempList)
        {
            minigameNames.Add(minigame.name);
        }

        //guarda el array en las custom properties
        Hashtable roomProps = PhotonNetwork.CurrentRoom.CustomProperties;
        roomProps[Constantes.MinigameOrder_Room] = minigameNames.ToArray();
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);

        //debug
        string temp = "Minijuegos Randomizados y Sincronizados:\n ";
        foreach (MinigameInfo minigameInfo in tempList)
        {
            temp += $"{minigameInfo.Name}\n";
        }
        print(temp);
    }


    public void StartMinigameSelection()
    {
        //El Master Client llama a la funcion y la manda a todos los clientes
        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC(nameof(RPC_StartMinigameSelection), RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_StartMinigameSelection()
    {
        MinigameSelection();
    }

    void MinigameSelection()
    {
        minigameDisplay.SetActive(true);

        Transform content = minigameDisplay.transform.Find("Content");

        //Borra todos los hijos por si acaso
        //foreach(Transform child in content) { Destroy(child.gameObject); }

        List<MinigameInfo> shuffledMinigames = new List<MinigameInfo>();

        //guarda los minijuegos randomizados en orden en la lista
        Hashtable roomProps = PhotonNetwork.CurrentRoom.CustomProperties;
        foreach(System.Collections.DictionaryEntry entry in roomProps)
        {
            if((string)entry.Key == Constantes.MinigameOrder_Room)
            {
                string[] minigameString = (string[])entry.Value;

                for(int i = 0; i < minigameString.Length; i++)
                {
                    MinigameInfo mg = Resources.Load<MinigameInfo>($"Minigames/{minigameString[i]}");

                    shuffledMinigames.Add(mg);
                }
            }
        }

        //cambia los valores de los iconos en orden
        for(int i = 0; i < shuffledMinigames.Count; i++)
        {
            Transform display = content.GetChild(i);

            display.Find("Image").GetComponent<Image>().sprite = shuffledMinigames[i].Icon;
            display.Find("Name").GetComponent<TMP_Text>().text = shuffledMinigames[i].Name;
        }

        //Activa la pantalla de carga, y luego carga la escena de showcase
        CoolFunctions.Invoke(this, () =>
        {
            loadingScreen.SetActive(true);
            CoolFunctions.Invoke(this, () => PhotonNetwork.LoadLevel("ShowcaseMG"), 1);
        }, 2);
    }
}
