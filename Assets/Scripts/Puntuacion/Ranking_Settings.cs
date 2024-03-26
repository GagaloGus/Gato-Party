using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Ranking_Settings : MonoBehaviour
{
    MinigameInfo nextMinigame;

    [Header("References")]
    public GameObject NextMinigamePanel;
    public GameObject LoadingScreen;

    [Header("Lists")]
    List<Sprite> rankingNumberSprites = new List<Sprite>();
    List<GameObject> RankingPanels = new List<GameObject>();
    readonly int[] Win_Points = new int[] { 5, 3, 2, 0 };
    private void Awake()
    {
        NextMinigamePanel.SetActive(false);
        LoadingScreen.SetActive(false);

        //Guarda las variables de los numeritos de ranking
        rankingNumberSprites.Clear();
        rankingNumberSprites = Resources.LoadAll<Sprite>("Ranking/Numbers").ToList();

        //Guarda los paneles donde iran las puntuaciones
        Transform rankingPanel = FindObjectOfType<Canvas>().transform.Find("Ranking");

        foreach(Transform panel in rankingPanel)
        {
            RankingPanels.Add(panel.gameObject);
        }
    }

    List<Player> GetSortedHashtableList(string key)
    {
        Dictionary<Player, int> rankingList = new Dictionary<Player, int>();

        //Si la propKey es la de la puntuacion del anterior minijuego, guarda el propValue en el diccionario con su respectivo player
        foreach (System.Collections.Generic.KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            Hashtable playerProps = player.Value.CustomProperties;
            foreach (System.Collections.DictionaryEntry props in playerProps)
            {
                if ((string)props.Key == key)
                {
                    rankingList.Add(player.Value, (int)props.Value);
                    break;
                }
            }
        }

        //ordena el diccionario de mayor a menor
        var sortedRanking = rankingList.OrderByDescending(x => x.Value);

        string debugRank = "Ranking total:\n";
        foreach (System.Collections.Generic.KeyValuePair<Player, int> player in sortedRanking)
        { debugRank += $"{player.Key} : {player.Value}\n"; }
        Debug.Log(debugRank);

        return sortedRanking.Select(x => x.Key).ToList();
    }

    void UpdateRankingUI(bool saveProperties)
    {
        //Crea una lista de solo las keys (PLAYERS) ya ordenados de mayor a menor
        List<Player> orderedPlayerList = GetSortedHashtableList(Constantes.PlayerKey_TotalScore);

        for (int i = 0; i < orderedPlayerList.Count; i++)
        {
            Player currentPlayer = orderedPlayerList[i];
            int currentWinPoints = (int)currentPlayer.CustomProperties[Constantes.PlayerKey_TotalScore];

            Transform currentPanel = RankingPanels[i].transform;

            currentPanel.Find("Rank").GetComponent<Image>().sprite = rankingNumberSprites[i];
            currentPanel.Find("Name").GetComponent<TMP_Text>().text = currentPlayer.NickName;
            currentPanel.Find("PuntGlobal").GetComponent<TMP_Text>().text = currentWinPoints.ToString();

            //Solo el Master Client cambia las propiedades
            if (PhotonNetwork.IsMasterClient && saveProperties)
            {
                Hashtable newPlayerProps = new Hashtable();
                newPlayerProps[Constantes.PlayerKey_TotalScore] = currentWinPoints + Win_Points[i];
                currentPlayer.SetCustomProperties(newPlayerProps);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateRankingUI(true);

        //Tarda 2 segundos en actualizar la puntuacion a la actual (suspense)
        CoolFunctions.Invoke(this, () =>
        {
            UpdateRankingUI(false);
        }, 2);

        Hashtable roomProps = PhotonNetwork.CurrentRoom.CustomProperties;
        foreach (System.Collections.DictionaryEntry entry in roomProps)
        {
            if ((string)entry.Key == Constantes.MinigameOrder_Room)
            {
                string[] temp = (string[])entry.Value;
                nextMinigame = Resources.Load<MinigameInfo>($"Minigames/{temp[0]}");
            }
        }

        CoolFunctions.Invoke(this, () =>
        {
            NextMinigamePanel.SetActive(true);

            NextMinigamePanel.transform.Find("Name").GetComponent<TMP_Text>().text = nextMinigame.Name;
            NextMinigamePanel.transform.Find("DisplayImage").GetComponent<Image>().sprite = nextMinigame.Icon;

            CoolFunctions.Invoke(this, () =>
            {
                LoadingScreen.SetActive(true);
                PhotonNetwork.LoadLevel("ShowcaseMG");
            }, 4);
        }, 6);

    }

}
