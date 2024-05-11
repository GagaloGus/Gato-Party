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
    public int maxTime;
    float currentTime;
    public float interval;

    Dictionary<Player, int> resultPlayerlist = new();

    public Color[] aireColors;
    PhotonView photonView;
    GameObject player;

    // Start is called before the first frame update
    void Start() 
    {
        photonView = GetComponent<PhotonView>();
        resultsText.SetActive(false);
        Timer.SetActive(false);

        player = FindObjectOfType<AssignObjectToPlayer>().AssignObject();

        //carga las texturas de los jugadores localmente, necesita un delay mas grande para que esten todos los jugadores en la sala
        //- aqui habra que poner una pantalla de carga en vez del delay -//
        /*CoolFunctions.Invoke(this, () =>
        {
            CoolFunctions.LoadAllTexturePacks<MGMash_PlayerController>();
            player.GetComponentInChildren<Animator>().SetBool("push", false);
        }, 0.5f);


        //Espera 2 segundos
        CoolFunctions.Invoke(this, () =>
        {
            //Deja que los players se muevan
            player.GetComponent<MGMash_PlayerController>().canMove = true;
            if(PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(RPC_StartCountdown), RpcTarget.All, maxTime, interval);
            }
        }, 2);*/

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
                //añadimos la puntuacion al diccionario
                resultPlayerlist.Add(targetPlayer, (int)changedProps[Constantes.PlayerKey_MinigameScore]);
                playerCount++;
            }

            //Si todos los players actualizaron su informacion
            if (playerCount == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                //Ordena la lista de mayor a menor
                var sortedResults = resultPlayerlist.OrderByDescending(x => x.Value);

                //Añade las puntuaciones a un string en orden
                string results = "<color=yellow>-- Ranking --</color>\n";
                int index = 1;
                foreach (KeyValuePair<Player, int> entry in sortedResults)
                {
                    results += $"{index}# {entry.Key.NickName}: {entry.Value} puntos\n";
                    index++;
                }

                //muestra las puntuaciones para el resto de jugadores
                photonView.RPC(nameof(ShowResults), RpcTarget.All, results);
            }
        }
    }

    [PunRPC]
    void ShowResults(string result)
    {
        resultsText.SetActive(true);
        resultsText.GetComponentInChildren<TMP_Text>().text = result;

        CoolFunctions.Invoke(this, () =>
        {
            PhotonNetwork.LoadLevel("Puntuacion");
        }, 4);
    }

    [PunRPC]
    void RPC_StartCountdown(int maxTime, float interval)
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
        Timer.GetComponentInChildren<TMP_Text>().text = currentTime.ToString("00");

        if(currentTime <= 0)
        {
            Debug.Log("stopu");
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
