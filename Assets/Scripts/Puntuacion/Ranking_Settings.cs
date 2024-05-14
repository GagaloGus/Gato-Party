using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.GridBrushBase;

public class Ranking_Settings : MonoBehaviour
{
    public bool roundsOver;

    [Header("References")]
    public GameObject NextMinigamePanel;
    public GameObject LoadingScreen;

    [Header("Lists")]
    List<Sprite> rankingNumberSprites = new List<Sprite>();
    List<GameObject> RankingPanels = new List<GameObject>();
    [SerializeField] List<Vector3> PanelPositions = new List<Vector3>();

    private void Awake()
    {
        NextMinigamePanel.SetActive(false);
        LoadingScreen.SetActive(false);

        //Guarda las variables de los numeritos de ranking
        rankingNumberSprites.Clear();
        PanelPositions.Clear();
        rankingNumberSprites = Resources.LoadAll<Sprite>("Ranking/Numbers").ToList();

        //Guarda los paneles donde iran las puntuaciones
        Transform rankingPanel = FindObjectOfType<Canvas>().transform.Find("Ranking");

        foreach (Transform panel in rankingPanel)
        {
            RankingPanels.Add(panel.gameObject);
            PanelPositions.Add(panel.position);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        //Mira si quedan minijuegos disponibles
        roundsOver = (bool)PhotonNetwork.CurrentRoom.CustomProperties[Constantes.RoundsOver_Room];

        foreach (GameObject panel in RankingPanels)
        {
            panel.SetActive(false);
        }

        StartCoroutine(nameof(TimeLine));
    }

    System.Collections.IEnumerator TimeLine()
    {
        yield return new WaitForSeconds(0.5f);

        //Actualiza la interfaz con el ranking de pt globales anterior al minijuego
        List<Player> RanklistGlob = GetPlayerListSorted(Constantes.PlayerKey_TotalScore);
        UpdateUI(RanklistGlob);

        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            RankingPanels[i].SetActive(true);
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(0.5f);
        //Actualiza las puntuaciones solo si es el Master Client
        if (PhotonNetwork.IsMasterClient)
        {
            //Coje una lista ordenada segun la pt del minijuego anterior
            List<Player> RanklistMinigame = GetPlayerListSorted(Constantes.PlayerKey_MinigameScore);

            // Añade puntos respectivamente
            for (int i = 0; i < RanklistMinigame.Count; i++)
            {
                Player currentPlayer = RanklistMinigame[i];
                int currentWinPoints = (int)currentPlayer.CustomProperties[Constantes.PlayerKey_TotalScore];
                int finalScore = currentWinPoints;

                // Si hay puntuaciones repetidas
                /*int amountOfPlayersSameMGScore = 1; // Incluir el currentPlayer
                List<Player> playersWithSameScore = new List<Player> { currentPlayer };

                for (int j = 0; j < RanklistMinigame.Count; j++)
                {
                    if (j == i) continue;

                    Player nextPlayer = RanklistMinigame[j];
                    int nextWinPoints = (int)nextPlayer.CustomProperties[Constantes.PlayerKey_TotalScore];

                    if (nextWinPoints == currentWinPoints)
                    {
                        amountOfPlayersSameMGScore++;
                        playersWithSameScore.Add(nextPlayer);
                    }
                }


                if (amountOfPlayersSameMGScore > 1)
                {
                    int totalScore = 0;
                    for (int k = 0; k < amountOfPlayersSameMGScore; k++)
                    {
                        totalScore += Constantes.Win_Points[k];
                    }
                    int averageScore = Mathf.RoundToInt((float)totalScore / amountOfPlayersSameMGScore);

                    finalScore += averageScore;

                }
                else
                {
                }*/
                finalScore += Constantes.Win_Points[i];

                Hashtable newPlayerProps = new Hashtable
                {
                    [Constantes.PlayerKey_TotalScore] = finalScore
                };
                currentPlayer.SetCustomProperties(newPlayerProps);
            }
        }

        yield return new WaitForSeconds(1.5f);

        //Actualiza la interfaz con el ranking de pt globales POSTERIOR al minijuego
        List<Player> NewRankListGlob = GetPlayerListSorted(Constantes.PlayerKey_TotalScore);
        ChangeRankedScore(NewRankListGlob);

        List<string> playerNames = GetTMPNames();

        for (int i = 0; i < NewRankListGlob.Count; i++)
        {
            int position = playerNames.IndexOf(NewRankListGlob[i].NickName);
            Debug.Log($"Mover <color=red>{i} -> {position}</color>");
            StartCoroutine(MovePanel(i, position));
        }

        ChangeRankedNumbers();

        yield return new WaitForSeconds(10);

        //Mira si se acabaron las rondas
        if (!roundsOver)
        {
            MinigameInfo nextMinigame = null;

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

            FindObjectOfType<Canvas>().GetComponent<Animator>().SetTrigger("next");

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

    System.Collections.IEnumerator MovePanel(int panel, int newPos)
    {
        GameObject panelToMove = RankingPanels[panel];

        float elapsedTime = 0f;
        Vector3 startPos = panelToMove.transform.position, endPos = PanelPositions[newPos];

        while (elapsedTime < 1)
        {
            float t = elapsedTime / 1;
            panelToMove.transform.position = Vector3.Lerp(startPos, endPos, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        panelToMove.transform.position = endPos;
    }


    List<Player> GetPlayerListSorted(string key)
    {
        Dictionary<Player, int> sortedDic = new Dictionary<Player, int>();

        //Si la propKey es la de la puntuacion del anterior minijuego, guarda el propValue en el diccionario con su respectivo player
        foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
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
        return temp;
    }

    void UpdateUI(List<Player> sortedPlayerList)
    {
        for (int i = 0; i < sortedPlayerList.Count; i++)
        {
            Player currentPlayer = sortedPlayerList[i];
            int currentWinPoints = (int)currentPlayer.CustomProperties[Constantes.PlayerKey_TotalScore];
            Transform currentPanel = RankingPanels[i].transform;

            currentPanel.Find("Name").GetComponent<TMP_Text>().text = currentPlayer.NickName;
            currentPanel.Find("PuntGlobal").GetComponent<TMP_Text>().text = currentWinPoints.ToString();
        }

        ChangeRankedScore(sortedPlayerList);
        ChangeRankedNumbers();
    }

    void ChangeRankedScore(List<Player> sortedPlayerList)
    {
        for (int i = 0; i < sortedPlayerList.Count; i++)
        {
            int currentWinPoints = (int)sortedPlayerList[i].CustomProperties[Constantes.PlayerKey_TotalScore];

            RankingPanels[i].transform.Find("PuntGlobal").GetComponent<TMP_Text>().text = currentWinPoints.ToString();
        }
    }

    void ChangeRankedNumbers()
    {
        for(int i = 0; i < RankingPanels.Count; i++)
        {
            RankingPanels[i].transform.Find("Rank").GetComponent<Image>().sprite = rankingNumberSprites[i];
        }
    }

    List<string> GetTMPNames() 
    { 
        List<string> names = new List<string>();

        foreach(GameObject panel in RankingPanels)
        {
            if (panel.activeInHierarchy)
            {
                TMP_Text text = panel.transform.Find("Name").GetComponent<TMP_Text>();
                names.Add(text.text);
            }
        }

        return names;
    }
}
