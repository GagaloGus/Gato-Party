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
    [SerializeField] List<Vector3> PanelPositions = new List<Vector3>();

    List<PlayerPanelPair> playerPanelPairs = new List<PlayerPanelPair>(4);

    [Header("Audios")]
    public AudioClip victoryTheme;
    public AudioClip drumrollSound, drumCymbalSound;

    private void Awake()
    {
        NextMinigamePanel.SetActive(false);
        LoadingScreen.SetActive(true);

        //Guarda las variables de los numeritos de ranking
        rankingNumberSprites.Clear();
        PanelPositions.Clear();
        rankingNumberSprites = Resources.LoadAll<Sprite>("Ranking/Numbers").ToList();
    }


    // Start is called before the first frame update
    void Start()
    {
        //Mira si quedan minijuegos disponibles
        roundsOver = (bool)PhotonNetwork.CurrentRoom.CustomProperties[Constantes.RoundsOver_Room];

        //Guarda los paneles donde iran las puntuaciones
        
        Transform rankingPanel = FindObjectOfType<Canvas>().transform.Find("Ranking");

        foreach (Transform panel in rankingPanel)
        {
            PanelPositions.Add(panel.position);
            panel.gameObject.SetActive(false);
        }

        AudioManager.instance.StopAmbientMusic();
        StartCoroutine(nameof(TimeLine));
    }

    System.Collections.IEnumerator TimeLine()
    {
        yield return new WaitForSeconds(2);
        yield return StartCoroutine(FadeInOutLoadingScreen(false));

        AudioManager.instance.PlaySFX2D(drumrollSound);
        yield return new WaitForSeconds(1);

        //Actualiza la interfaz con el ranking de pt globales anterior al minijuego
        List<Player> RanklistGlob = GetPlayerListSorted(Constantes.PlayerKey_TotalScore);
        Transform rankingPanel = FindObjectOfType<Canvas>().transform.Find("Ranking");

        //guarda los playerpairs
        for (int i = 0; i < RanklistGlob.Count; i++)
        {
            playerPanelPairs.Add(new PlayerPanelPair(RanklistGlob[i], rankingPanel.GetChild(i).gameObject));
        }

        UpdateUI();
        ChangeRankedNumbers(RanklistGlob);

        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            playerPanelPairs[i].Panel.SetActive(true);

            yield return new WaitForSeconds(0.5f);
        }

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

        for (int i = 0; i < NewRankListGlob.Count; i++)
        {
            int position = NewRankListGlob.IndexOf(Array.Find(NewRankListGlob.ToArray(), x => x == playerPanelPairs[i].player));
            Debug.Log($"Mover <color=red>{i} -> {position}</color>");
            StartCoroutine(MovePanel(i, position));
        }

        UpdateUI();
        ChangeRankedNumbers(NewRankListGlob);

        AudioManager.instance.ClearAudioList();
        AudioManager.instance.PlaySFX2D(drumCymbalSound);

        yield return new WaitForSeconds(1);
        AudioManager.instance.ForcePlayAmbientMusic(victoryTheme);

        yield return new WaitForSeconds(2.5f);

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

            yield return new WaitForSeconds(7);

            yield return StartCoroutine(FadeInOutLoadingScreen(true));

            yield return new WaitForSeconds(2);
            PhotonNetwork.LoadLevel("ShowcaseMG");
        }
        else
        {
            yield return new WaitForSeconds(5);
            SceneManager.LoadScene("FinalScores");
        }
    }

    System.Collections.IEnumerator FadeInOutLoadingScreen(bool fadeIn)
    {
        CanvasGroup patata = LoadingScreen.GetComponent<CanvasGroup>();

        if (fadeIn)
        {
            patata.alpha = 0;
            LoadingScreen.SetActive(true);

            for (float i = 0; i <= 1; i += 0.05f)
            {
                patata.alpha = i;
                yield return null;
            }

            patata.alpha = 1;
        }
        else
        {
            patata.alpha = 1;

            for (float i = 0; i <= 1; i += 0.05f)
            {
                patata.alpha = 1 - i;
                yield return null;
            }

            patata.alpha = 0;
            LoadingScreen.SetActive(false);
        }
    }

    System.Collections.IEnumerator MovePanel(int panel, int newPos)
    {
        GameObject panelToMove = playerPanelPairs[panel].Panel;

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

    public class PlayerPanelPair
    {
        public Player player;
        public GameObject Panel;

        public PlayerPanelPair(Player player, GameObject panel)
        {
            this.player = player;
            Panel = panel;

            Debug.Log($"Created panel: {player.NickName} -> {panel.name}");
        }
    }

    public void UpdateUI()
    {
        foreach(PlayerPanelPair PPP in playerPanelPairs)
        {
            int currentWinPoints = (int)PPP.player.CustomProperties[Constantes.PlayerKey_TotalScore];

            Sprite icon = Resources.Load<Sprite>($"ReadySprites/{(int)PPP.player.CustomProperties[Constantes.PlayerKey_Skin]}_notready");

            PPP.Panel.transform.Find("Icon").GetComponent<Image>().sprite = icon;
            PPP.Panel.transform.Find("Name").GetComponent<TMP_Text>().text = PPP.player.NickName;
            PPP.Panel.transform.Find("PuntGlobal").GetComponent<TMP_Text>().text = currentWinPoints.ToString();
        }
    }

    void ChangeRankedNumbers(List<Player> allPlayers)
    {
        int rank = 0;
        List<int> ranks = new List<int>();
        for (int i = 0; i < allPlayers.Count; i++)
        {
            int p_score = (int)allPlayers[i].CustomProperties[Constantes.PlayerKey_TotalScore];
            int prev_p_score;

            if(i > 0)
            {
                prev_p_score = (int)allPlayers[i - 1].CustomProperties[Constantes.PlayerKey_TotalScore];

                if(p_score != prev_p_score)
                {
                    rank++;
                }

            }
            
            ranks.Add(rank);

            PlayerPanelPair PPP = Array.Find(playerPanelPairs.ToArray(), x => x.player == allPlayers[i]);

            PPP.Panel.transform.Find("Rank").GetComponent<Image>().sprite = rankingNumberSprites[rank];
        }
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
}

