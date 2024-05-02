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

        //Solucion temporal para que siempre haya 4 imagenes
        while (playerSkinIDs.Count < 4)
        {
            playerSkinIDs.Add(0);
        }

        for (int i = 0; i < PlayerList.transform.childCount; i++)
        {
            Transform child = PlayerList.transform.GetChild(i);

            if(i < PhotonNetwork.CurrentRoom.PlayerCount)
            {
                TMP_Text text = child.Find("Score").GetComponent<TMP_Text>();

                child.Find("Sprite").GetComponent<Image>().sprite = Resources.Load<Sprite>($"ReadySprites/{playerSkinIDs[i]}_notready");

                text.text = 0.ToString("00");
                playerScores.Add(0);
            }
         
            child.gameObject.SetActive(i < PhotonNetwork.CurrentRoom.PlayerCount);
        }
    }

    public void UpdateScore(int player, int scoreToAdd)
    {
        Debug.LogWarning($"turncount -> {player} + {scoreToAdd}");
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
