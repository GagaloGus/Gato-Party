using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MGFindItem_PlayerScores : MonoBehaviour
{
    public Transform PlayerList;
    List<int> playerSkinIDs;

    List<int> playerScores = new List<int>(); 
    // Start is called before the first frame update
    void Start()
    {
        playerScores.Clear();

        playerSkinIDs = CoolFunctions.GetAllPlayerSkinIDs();

        foreach(Transform child in PlayerList) { child.gameObject.SetActive(false); }
    }

    void Setup()
    {
        CoolFunctions.LoadAllTexturePacks<MGFindItem_PlayerController>();

        int playerCount = (int)PhotonNetwork.CurrentRoom.CustomProperties[Constantes.AmountPlayers_Room];


        for (int i = 0; i < playerCount; i++)
        {
            Transform child = PlayerList.transform.GetChild(i);

            child.Find("Score").GetComponent<TMP_Text>().text = 0.ToString("00");
            child.Find("Sprite").GetComponent<Image>().sprite = Resources.Load<Sprite>($"ReadySprites/{playerSkinIDs[i]}_notready");
            child.Find("Name").GetComponent<TMP_Text>().text = PhotonNetwork.CurrentRoom.Players[i].NickName;

            playerScores.Add(0);

            child.gameObject.SetActive(true);

        }
    }

    public void UpdateScore(int player, int scoreToAdd)
    {
        Transform child = PlayerList.transform.GetChild(player);

        playerScores[player] += scoreToAdd;

        child.Find("Score").GetComponent<TMP_Text>().text = playerScores[player].ToString("00");
    }

    public List<int> getPlayerScores
    {
        get { return playerScores; }
    }

    public void ChangeTurn(int turn)
    {
        for(int i = 0; i < PlayerList.transform.childCount;i++)
        {
            Transform child = PlayerList.transform.GetChild(i);

            child.Find("Turn").gameObject.SetActive(i == turn);
        }
    }
}
