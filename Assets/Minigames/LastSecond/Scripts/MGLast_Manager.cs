using ExitGames.Client.Photon;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MGLast_Manager : MonoBehaviour
{
    public List<GameObject> PlayerObjects = new List<GameObject>();
    public List<GameObject> Bonks = new List<GameObject>();

    [Header("Mechanics")]
    public bool gameStarted;
    public float bonkHeight;
    [Range(1, 4)] int TargetPlayer;
    float originalHeight;
    GameObject targetBonk;
    public Vector2Int minMaxScore;

    [Header("Photon")]
    public int remainingPlayers;
    PhotonView photonView;

    [Header("UI")]
    public GameObject rankingText;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();

        for (int i = 0; i < Bonks.Count; i++)
        { BonkPhysics(i, false); }
    }

    // Start is called before the first frame update
    void Start()
    {
        TargetPlayer = (int)PhotonNetwork.LocalPlayer.CustomProperties[Constantes.PlayerKey_CustomID] - 1;
        targetBonk = Bonks[TargetPlayer];
        originalHeight = Bonks[0].transform.position.y;

        FindObjectOfType<AssignObjectToPlayer>().AssignObject(PlayerObjects);

        gameStarted = false;
        rankingText.SetActive(false);

        //Cooldown hasta que se hayan unido todos los jugadores
        CoolFunctions.Invoke(this, () =>
        {
            CoolFunctions.LoadAllTexturePacks<MGLast_PlayerController>();
            remainingPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
            PlayerObjects[TargetPlayer].GetComponent<MGLast_PlayerController>().LookUp();

            for (int i = 0; i < Bonks.Count; i++)
            {
                Bonks[i].SetActive(i < PhotonNetwork.CurrentRoom.PlayerCount);
            }
        }, 0.5f);

        //Cooldown hasta que empiece el minijuego
        CoolFunctions.Invoke(this, () =>
        {
            gameStarted = true;
            
            Debug.Log("Game Started!");

            for(int i = 0; i < PlayerObjects.Count; i++)
            {
                PlayerObjects[i].GetComponent<MGLast_PlayerController>().StartGame();
                BonkPhysics(i, true);
            }
        }, 2);
    }



    // Update is called once per frame
    void Update()
    {
        if (gameStarted)
        {
            if (Input.GetKeyDown(PlayerKeybinds.stop_lastSecMG))
            {
                photonView.RPC(nameof(RPC_StoppedBonk), RpcTarget.All, TargetPlayer);
                gameStarted = false;
            }

            if (targetBonk.transform.position.y <= bonkHeight)
            {
                Hashtable dead = new Hashtable
                {
                    [Constantes.PlayerKey_Eliminated] = true
                };
                PhotonNetwork.LocalPlayer.SetCustomProperties(dead);

                //- Cambiar por un RPC -//
                photonView.RPC(nameof(RPC_BonkedPlayer), RpcTarget.All, TargetPlayer);
                gameStarted = false;   
            }
        }
    }

    [PunRPC]
    void RPC_StoppedBonk(int playerInt)
    {
        remainingPlayers--;
        PlayerObjects[playerInt].GetComponent<MGLast_PlayerController>().PressedButton();
        BonkPhysics(playerInt, false);

        CoolFunctions.Invoke(this, () =>
        {
            if (remainingPlayers == 0 && PhotonNetwork.IsMasterClient)
                FinishGame();
        }, 1);
        
    }

    [PunRPC]
    void RPC_BonkedPlayer(int playerInt)
    {
        remainingPlayers--;
        PlayerObjects[playerInt].GetComponent<MGLast_PlayerController>().Bonked();
        BonkPhysics(playerInt, false);

        CoolFunctions.Invoke(this, () =>
        {
            if (remainingPlayers == 0 && PhotonNetwork.IsMasterClient)
                FinishGame();
        }, 1);
    }

    void FinishGame()
    {
        Debug.Log(Mathf.RoundToInt(CoolFunctions.MapValues(targetBonk.transform.position.y, originalHeight, bonkHeight, minMaxScore.x, minMaxScore.y)));

        //ordena de mayor a menor segun las posiciones en y
        List<GameObject> OrderedBonks = Bonks.OrderByDescending(x => x.transform.position.y).ToList();
        //Invierte la lista, ahora es de menor a mayor
        OrderedBonks.Reverse();

        //Crea un diccionario donde la Key son los GameObjects y el Value son valores ordenados del 0 al ...
        Dictionary<GameObject, int> indexMap = new Dictionary<GameObject, int>();
        for (int i = 0; i < Bonks.Count; i++)
        {
            indexMap.Add(Bonks[i], i);
        }

        //Saca el int de cada Bonk desordenado, siguiendo el orden de la lista ordenada
        List<int> orderedIndexes = new List<int>();
        foreach (GameObject bonk in OrderedBonks)
        {
            orderedIndexes.Add(indexMap[bonk] + 1);
        }

        foreach (KeyValuePair<int, Player> playerEntry in PhotonNetwork.CurrentRoom.Players)
        {
            Hashtable playerMGprop = new Hashtable();
            if (!(bool)playerEntry.Value.CustomProperties[Constantes.PlayerKey_Eliminated])
            {
                playerMGprop[Constantes.PlayerKey_MinigameScore] = orderedIndexes[(int)playerEntry.Value.CustomProperties[Constantes.PlayerKey_CustomID]];
            }
            else
            {
                playerMGprop[Constantes.PlayerKey_MinigameScore] = -1;
            }

            playerEntry.Value.SetCustomProperties(playerMGprop);
        }

        photonView.RPC(nameof(RPC_FinishedGame), RpcTarget.All);
    }

    [PunRPC]
    void RPC_FinishedGame()
    {
        Player[] currentPlayers = PhotonNetwork.CurrentRoom.Players.Select(x => x.Value).ToArray();
        string results = $"<color=yellow>Ranking:</color>\n";

        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            //Encuentra el player actual segun su Custom ID
            Player currentPlayer = System.Array.Find(currentPlayers, x => (int)x.CustomProperties[Constantes.PlayerKey_CustomID] == i + 1);
            int score = Mathf.RoundToInt(CoolFunctions.MapValues(Bonks[i].transform.position.y, originalHeight, bonkHeight, minMaxScore.x, minMaxScore.y));

            if (!(bool)currentPlayer.CustomProperties[Constantes.PlayerKey_Eliminated])
            {
                results += $"{currentPlayer.NickName} -> {score}m";
            }
            else
            {
                results += $"{currentPlayer.NickName} -> ------";
            }
        }

        rankingText.SetActive(true);
        rankingText.GetComponent<TMP_Text>().text = results;

        CoolFunctions.Invoke(this, () =>
        {
            PhotonNetwork.LoadLevel("Puntuacion");
        }, 4);
    }


    void BonkPhysics(int bonkInt, bool enable)
    {
        GameObject targetBonk = Bonks[bonkInt];

        targetBonk.GetComponent<ConstantForce>().enabled = enable;
        targetBonk.GetComponent<Rigidbody>().useGravity = enable;

        if (!enable)
        {
            targetBonk.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(
            CoolFunctions.FlattenVector3(PlayerObjects[0].transform.position, bonkHeight),
            (PlayerObjects[PlayerObjects.Count - 1].transform.position - PlayerObjects[0].transform.position)
            );

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(
            Bonks[0].transform.position,
            (Bonks[Bonks.Count - 1].transform.position - Bonks[0].transform.position)
            );
    }
}
