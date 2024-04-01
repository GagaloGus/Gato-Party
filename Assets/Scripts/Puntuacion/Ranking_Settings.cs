using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Ranking_Settings : MonoBehaviour
{
    MinigameInfo nextMinigame;
    public bool roundsOver;

    [Header("References")]
    public GameObject NextMinigamePanel;
    public GameObject LoadingScreen;

    [Header("Lists")]
    List<Sprite> rankingNumberSprites = new List<Sprite>();
    List<GameObject> RankingPanels = new List<GameObject>();

    private void Awake()
    {
        NextMinigamePanel.SetActive(false);
        LoadingScreen.SetActive(false);

        //Guarda las variables de los numeritos de ranking
        rankingNumberSprites.Clear();
        rankingNumberSprites = Resources.LoadAll<Sprite>("Ranking/Numbers").ToList();

        //Guarda los paneles donde iran las puntuaciones
        Transform rankingPanel = FindObjectOfType<Canvas>().transform.Find("Ranking");

        foreach (Transform panel in rankingPanel)
        {
            RankingPanels.Add(panel.gameObject);
        }

        for (int i = 0; i < RankingPanels.Count; i++)
        {
            RankingPanels[i].SetActive(i < PhotonNetwork.CurrentRoom.PlayerCount);
        }
    }

    List<Player> GetPlayerListSorted(string key)
    {
        Dictionary<Player, int> sortedDic = new Dictionary<Player, int>();

        //Si la propKey es la de la puntuacion del anterior minijuego, guarda el propValue en el diccionario con su respectivo player
        foreach (System.Collections.Generic.KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            Hashtable playerProps = player.Value.CustomProperties;
            foreach (System.Collections.DictionaryEntry props in playerProps)
            {
                if ((string)props.Key == key)
                {
                    sortedDic.Add(player.Value, (int)props.Value);
                    break;
                }
            }
        }

        var sortedRanking = sortedDic.OrderByDescending(x => x.Value);
        List<Player> temp = sortedRanking.Select(x => x.Key).ToList();

        string debugLine = $"Sorted {key}:\n";
        foreach (Player player in temp)
        {
            debugLine += $"Player <color=yellow>{player.NickName}:{(int)player.CustomProperties[key]}</color>\n";
        }
        print(debugLine);

        return temp;
    }

    void UpdateUI(List<Player> sortedPlayerList)
    {
        for (int i = 0; i < sortedPlayerList.Count; i++)
        {
            Player currentPlayer = sortedPlayerList[i];
            int currentWinPoints = (int)currentPlayer.CustomProperties[Constantes.PlayerKey_TotalScore];
            Transform currentPanel = RankingPanels[i].transform;

            currentPanel.Find("Rank").GetComponent<Image>().sprite = rankingNumberSprites[i];
            currentPanel.Find("Name").GetComponent<TMP_Text>().text = currentPlayer.NickName;
            currentPanel.Find("PuntGlobal").GetComponent<TMP_Text>().text = currentWinPoints.ToString();

            print($"Updated {i} panel");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //Mira si quedan minijuegos disponibles
        roundsOver = (bool)PhotonNetwork.CurrentRoom.CustomProperties[Constantes.RoundsOver_Room];

        StartCoroutine(nameof(TimeLine));
    }

    System.Collections.IEnumerator TimeLine()
    {
        //Actualiza la interfaz con el ranking de pt globales anterior al minijuego
        List<Player> RanklistGlob = GetPlayerListSorted(Constantes.PlayerKey_TotalScore);
        UpdateUI(RanklistGlob);

        //Actualiza las puntuaciones solo si es el Master Client
        if (PhotonNetwork.IsMasterClient)
        {
            //Coje una lista ordenada segun la pt del minijuego anterior
            List<Player> RanklistMinigame = GetPlayerListSorted(Constantes.PlayerKey_MinigameScore);

            //Añade puntos respectivamente
            for (int i = 0; i < RanklistMinigame.Count; i++)
            {
                Player currentPlayer = RanklistMinigame[i];
                int currentWinPoints = (int)currentPlayer.CustomProperties[Constantes.PlayerKey_TotalScore];

                int amountOfPlayersSameMGScore = 0;
                for (int j = 0; j < RanklistMinigame.Count; j++)
                {
                    Player nextPlayer = RanklistMinigame[j];
                    int nextWinPoints = (int)nextPlayer.CustomProperties[Constantes.PlayerKey_TotalScore];

                    if (nextWinPoints == currentWinPoints
                        && nextPlayer != currentPlayer)
                    {
                        amountOfPlayersSameMGScore++;
                    }
                }

                if (amountOfPlayersSameMGScore > 0)
                {
                    int totalScore = 0;
                    for (int k = 0; k < amountOfPlayersSameMGScore; k++)
                    {
                        totalScore += Constantes.Win_Points[k];
                    }
                    int averageScore = Mathf.RoundToInt((float)totalScore / amountOfPlayersSameMGScore);
                }

                Hashtable newPlayerProps = new Hashtable();
                newPlayerProps[Constantes.PlayerKey_TotalScore] = currentWinPoints + Constantes.Win_Points[i];
                currentPlayer.SetCustomProperties(newPlayerProps);
            }
        }

        yield return new WaitForSeconds(2);

            //Actualiza la interfaz con el ranking de pt globales POSTERIOR al minijuego
            List<Player> NewRankListGlob = GetPlayerListSorted(Constantes.PlayerKey_TotalScore);
            UpdateUI(NewRankListGlob);

        yield return new WaitForSeconds(3);

        //Mira si se acabaron las rondas
        if (!roundsOver)
        {
            //Cambia al siguiente minijuego
            Hashtable roomProps = PhotonNetwork.CurrentRoom.CustomProperties;
            foreach (System.Collections.DictionaryEntry entry in roomProps)
            {
                if ((string)entry.Key == Constantes.MinigameOrder_Room)
                {
                    string[] temp = (string[])entry.Value;
                    nextMinigame = Resources.Load<MinigameInfo>($"Minigames/{temp[0]}");
                    break;
                }
            }

            NextMinigamePanel.SetActive(true);

            NextMinigamePanel.transform.Find("Name").GetComponent<TMP_Text>().text = nextMinigame.Name;
            NextMinigamePanel.transform.Find("DisplayImage").GetComponent<Image>().sprite = nextMinigame.Icon;

            yield return new WaitForSeconds(4);

            LoadingScreen.SetActive(true);

            yield return new WaitForSeconds(1);
            PhotonNetwork.LoadLevel("ShowcaseMG");
        }
        else
        {
            SceneManager.LoadScene("FinalScores");
        }
    }

}
