using Photon.Pun;
using ExitGames.Client.Photon;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinigameSelector : MonoBehaviourPunCallbacks
{
    [Header("Minigame lists")]
    public List<MinigameInfo> minigames;
    public List<MinigameInfo> randomizedMinigames;

    [Header("References")]
    public GameObject minigameDisplay;

    private void Start()
    {
        minigameDisplay.SetActive(false);
    }

    void RandomizeMinigames()
    {
        //Limpia y randomiza los minijuegos nada mas empezar
        randomizedMinigames.Clear();

        List<MinigameInfo> shuffledList = CoolFunctions.ShuffleList(minigames);

        foreach (MinigameInfo minigame in shuffledList)
        {
            randomizedMinigames.Add(minigame);
        }

        /*Hashtable roomProps = PhotonNetwork.CurrentRoom.CustomProperties;
        roomProps[Constantes.MinigameOrder_Room] = randomizedMinigames;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);*/

        string temp = "Minijuegos Randomizados: ";
        foreach (MinigameInfo minigameInfo in randomizedMinigames)
        {
            temp += $"{minigameInfo.Name}\n";
        }
        print(temp);
    }

    public void StartMinigameSelection()
    {
        RandomizeMinigames();

        minigameDisplay.SetActive(true);

        Transform content = minigameDisplay.transform.Find("Content");

        //Borra todos los hijos por si acaso
        foreach(Transform child in content) { Destroy(child.gameObject); }

        //instancia los minijuegos en orden
        for(int i = 0; i < randomizedMinigames.Count; i++)
        {
            GameObject display = content.GetChild(i).gameObject;

            display.transform.Find("Image").GetComponent<Image>().sprite = randomizedMinigames[i].Icon;
            display.transform.Find("Name").GetComponent<TMP_Text>().text = randomizedMinigames[i].Name;
        }
    }
}
