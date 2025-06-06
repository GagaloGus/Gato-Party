using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MGMash_Manager : MonoBehaviourPunCallbacks
{
    [Header("References")]
    public GameObject resultsText;
    public List<GameObject> PlayerObjects = new List<GameObject>();

    [Header("Timer")]
    public GameObject Timer;
    public int maxTime, currentTime, interval;

    Dictionary<Player, int> resultPlayerlist = new();

    public Color[] aireColors;
    PhotonView photonView;
    GameObject player;
    DigitalTimerDisplay timerDisplay;

    // Start is called before the first frame update
    void Start() 
    {
        photonView = GetComponent<PhotonView>();
        resultsText.SetActive(false);
        Timer.SetActive(false);

        player = FindObjectOfType<AssignObjectToPlayer>().AssignObject();
        timerDisplay = FindObjectOfType<DigitalTimerDisplay>(true);

        for(int i = 0; i < Mathf.Min(PlayerObjects.Count, aireColors.Length); i++)
        {
            Transform playr = PlayerObjects[i].transform;
            MeshRenderer mshr = playr.Find("3dmodel Sko").Find("aire").Find("tubo").GetComponent<MeshRenderer>();

            mshr.materials[0].color = aireColors[i];
        }
    }

    public void Setup()
    {
        CoolFunctions.LoadAllTexturePacks<MGMash_PlayerController>();
        player.GetComponentInChildren<Animator>().SetBool("push", false);
    }

    public void StartMinigame()
    {
        player.GetComponent<MGMash_PlayerController>().canMove = true;
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPC_StartCountdown), RpcTarget.All, maxTime, interval);
        }
    }

    int playerCount = 0;
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //Solo si es el Master Client
        if (PhotonNetwork.IsMasterClient)
        {
            //Se ejecuta al final para mostrar el ranking
            //Si ha cambiado su puntuacion, y no es la de reseteo ( != -1 )
            if (changedProps.ContainsKey(Constantes.PlayerKey_MinigameScore))
            {
                print($"Puntuacion: {targetPlayer.NickName}->{(int)changedProps[Constantes.PlayerKey_MinigameScore]}");
                //a�adimos la puntuacion al diccionario
                resultPlayerlist.Add(targetPlayer, (int)changedProps[Constantes.PlayerKey_MinigameScore]);
                playerCount++;
            }

            //Si todos los players actualizaron su informacion
            if (playerCount == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                //Ordena la lista de mayor a menor
                var sortedResults = resultPlayerlist.OrderByDescending(x => x.Value);

                //A�ade las puntuaciones a un string en orden
                string results = "<color=yellow>-- Ranking --</color>\n";
                int index = 1;
                foreach (KeyValuePair<Player, int> entry in sortedResults)
                {
                    results += $"{index}# {entry.Key.NickName}: {entry.Value} puntos\n";
                    index++;
                }

                //muestra las puntuaciones para el resto de jugadores
                photonView.RPC(nameof(RPC_Finish), RpcTarget.All, results);
            }
        }
    }

    [PunRPC]
    void RPC_Finish(string result)
    {
        gameObject.SendMessage("FinishMinigame");

        resultsText.SetActive(true);
        resultsText.GetComponentInChildren<TMP_Text>().text = result;
    }

    [PunRPC]
    void RPC_StartCountdown(int maxTime, int interval)
    {
        currentTime = maxTime;
        this.maxTime = maxTime;
        this.interval = interval;

        Timer.SetActive(true);

        InvokeRepeating(nameof(Countdown), 0, interval);
    }

    void Countdown()
    {
        currentTime -= interval;
        timerDisplay.ChangeNumber(currentTime);

        if(currentTime <= 0)
        {
            CancelInvoke(nameof(Countdown));
            if(PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(RPC_FinishGame), RpcTarget.All);
            }
        }
    }

    [PunRPC]
    void RPC_FinishGame()
    {
        player.GetComponent<MGMash_PlayerController>().MinigameFinished();
    }
}
