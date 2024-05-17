using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WaitMinigameHandler : MonoBehaviourPunCallbacks
{
    public Transform SetupObject;
    GameObject LoadingScreen, GuideTextDisplay, CountdownSpriteDisplay, FinishScreen;

    int playerCount, maxPlayers;

    [Header("Countdown")]
    public int maxTime = 3;
    public int currentTime;

    [Header("Setup")]
    public string guideText;
    List<Sprite> countdownSprites = new List<Sprite>();

    PhotonView photonView;

    private void Awake()
    {
        countdownSprites.Clear();
        countdownSprites.Add(Resources.Load<Sprite>("CountdownSprites/start"));
        for(int i = 1; i <= 3; i++) { countdownSprites.Add(Resources.Load<Sprite>($"CountdownSprites/{i}")); }
    }

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();

        maxTime = 3;
        playerCount = 0;
        maxPlayers = (int)PhotonNetwork.CurrentRoom.CustomProperties[Constantes.AmountPlayers_Room];

        LoadingScreen = SetupObject.Find("LoadingScreen").gameObject;
        GuideTextDisplay = SetupObject.Find("GuideText").gameObject;
        CountdownSpriteDisplay = SetupObject.Find("WaitSprites").gameObject;
        FinishScreen = SetupObject.Find("Finish").gameObject;

        LoadingScreen.SetActive(true);
        GuideTextDisplay.SetActive(false);
        CountdownSpriteDisplay.SetActive(false);
        FinishScreen.SetActive(false);

        //allbuffered lo manda tambien a jugadores que aun no dentro de la partida
        CoolFunctions.Invoke(this, () => { photonView.RPC(nameof(RPC_PlayerJoined), RpcTarget.AllBuffered); }, 2);
    }

    void SetupMinigame()
    {
        Debug.Log("<color=magenta>Setup</color>");
        gameObject.SendMessage("Setup");

        GuideTextDisplay.SetActive(true);
        GuideTextDisplay.GetComponentInChildren<TMP_Text>(true).text = guideText;

        StartCoroutine(nameof(FadeOutLoading));

        if(PhotonNetwork.IsMasterClient)
        {
            CoolFunctions.Invoke(this, () =>
            {
                photonView.RPC(nameof(RPC_StartCountdown), RpcTarget.All);
            }, 3.5f);
        }
    }

    void FinishMinigame()
    {
        FinishScreen.SetActive(true);

        CoolFunctions.Invoke(this, () =>
        {
            PhotonNetwork.LoadLevel("Puntuacion");
        }, 7);
    }

    [PunRPC]
    void RPC_StartMinigame()
    {
        Debug.Log("<color=magenta>Minigame</color>");
        CountdownSpriteDisplay.SetActive(false);
        gameObject.SendMessage("StartMinigame");
    }

    [PunRPC]
    void RPC_PlayerJoined()
    {
        playerCount++;
        if (playerCount == maxPlayers)
        {
            SetupMinigame();
        }
    }

    [PunRPC]
    void RPC_StartCountdown()
    {
        currentTime = maxTime;
        InvokeRepeating(nameof(Countdown), 0, 1);
    }

    void Countdown()
    {
        CountdownSpriteDisplay.SetActive(true);
        CountdownSpriteDisplay.GetComponentInChildren<Image>().sprite = countdownSprites[currentTime];
        Debug.Log($"<color=magenta>{currentTime}</color>");
        currentTime--;

        if(currentTime < 0)
        {
            CancelInvoke(nameof(Countdown));

            if (PhotonNetwork.IsMasterClient)
                CoolFunctions.Invoke(this, () => { photonView.RPC(nameof(RPC_StartMinigame), RpcTarget.All);}, 2);
        }
    }

    System.Collections.IEnumerator FadeOutLoading()
    {
        CanvasGroup canvasGroup = LoadingScreen.GetComponent<CanvasGroup>();

        canvasGroup.alpha = 1;
        for (float i = 1; i >= 1; i-= 0.03f)
        {
            canvasGroup.alpha = i;
            yield return null;
        }
        canvasGroup.alpha = 0;
    }
}
