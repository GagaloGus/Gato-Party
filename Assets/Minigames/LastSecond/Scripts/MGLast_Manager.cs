using ExitGames.Client.Photon;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public Transform Rankings;

    [Header("Audio")]
    public AudioClip suspenseTheme;
    public AudioClip mandoClickSound ,ArrowSound, BonkSound;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();

        for (int i = 0; i < Bonks.Count; i++)
        {
            BonkPhysics(i, false);
            Bonks[i].GetComponent<Animator>().speed = 0;
            Bonks[i].SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        TargetPlayer = (int)PhotonNetwork.LocalPlayer.CustomProperties[Constantes.PlayerKey_CustomID] - 1;
        targetBonk = Bonks[TargetPlayer];
        originalHeight = Bonks[0].transform.position.y;

        FindObjectOfType<AssignObjectToPlayer>().AssignObject(PlayerObjects);

        gameStarted = false;
        Rankings.gameObject.SetActive(false);
    }

    void Setup()
    {
        CoolFunctions.LoadAllTexturePacks<MGLast_PlayerController>();
        remainingPlayers = PhotonNetwork.CurrentRoom.PlayerCount;

        for (int i = 0; i < PlayerObjects.Count; i++)
        {
            Bonks[i].SetActive(i < PhotonNetwork.CurrentRoom.PlayerCount);
            PlayerObjects[i].GetComponent<MGLast_PlayerController>().LookUp();
        }

        if (PhotonNetwork.IsMasterClient)
        {
            int[] randomFruits = new int[Bonks.Count];
            for (int i = 0; i < randomFruits.Length; i++) { randomFruits[i] = Random.Range(0, Bonks[0].transform.Find("Fruits").childCount); }

            photonView.RPC(nameof(RPC_Setup), RpcTarget.All, randomFruits);
        }
    }

    void StartMinigame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPC_StartGame), RpcTarget.All);
        }

    }

    [PunRPC]
    void RPC_Setup(int[] randomFruits)
    {
        for (int j = 0; j < Bonks.Count; j++)
        {
            Transform fruitParent = Bonks[j].transform.Find("Fruits");

            for (int i = 0; i < fruitParent.childCount; i++)
            {
                fruitParent.GetChild(i).gameObject.SetActive(i == randomFruits[j]);
            }
        }
    }

    [PunRPC]
    void RPC_StartGame()
    {
        gameStarted = true;

        Debug.Log("Game Started!");

        for (int i = 0; i < PlayerObjects.Count; i++)
        {
            PlayerObjects[i].GetComponent<MGLast_PlayerController>().StartGame();
            BonkPhysics(i, true);
        }

        AudioManager.instance.ForcePlayAmbientMusic(suspenseTheme);
        Camera.main.GetComponent<Animator>().SetTrigger("move");
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStarted)
        {
            if (Input.GetKeyDown(PlayerKeybinds.stop_lastSecMG))
            {
                photonView.RPC(nameof(RPC_ResultBonk), RpcTarget.All, TargetPlayer, targetBonk.transform.position.y, false);
                gameStarted = false;
            }

            if (targetBonk.transform.position.y <= bonkHeight)
            {
                Hashtable dead = new Hashtable
                {
                    [Constantes.PlayerKey_Eliminated] = true,
                };
                PhotonNetwork.LocalPlayer.SetCustomProperties(dead);

                //- Cambiar por un RPC -//
                photonView.RPC(nameof(RPC_ResultBonk), RpcTarget.All, TargetPlayer, bonkHeight, true);
                gameStarted = false;   
            }
        }
    }

    [PunRPC]
    void RPC_ResultBonk(int playerInt, float height, bool bonked)
    {
        GameObject theBonk = Bonks[playerInt];

        remainingPlayers--;

        if (bonked)
        {
            theBonk.transform.Find("Canvas").Find("Explosion").gameObject.SetActive(true);
            theBonk.transform.Find("Fruits").gameObject.SetActive(false);
            PlayerObjects[playerInt].GetComponent<MGLast_PlayerController>().Bonked();

            AudioManager.instance.PlaySFX2D(BonkSound);
        }
        else
        {
            theBonk.GetComponent<Animator>().speed = 1;
            PlayerObjects[playerInt].GetComponent<MGLast_PlayerController>().PressedButton();

            AudioManager.instance.PlaySFX2D(mandoClickSound);
            CoolFunctions.Invoke(this, () => { AudioManager.instance.PlaySFX2D(ArrowSound); }, 0.1f);
        }

        BonkPhysics(playerInt, false);

        theBonk.transform.position = new Vector3(
            theBonk.transform.position.x,
            height,
            theBonk.transform.position.z
            );

        CoolFunctions.Invoke(this, () =>
        {
            if (remainingPlayers <= 0 && PhotonNetwork.IsMasterClient)
                FinishGame();
        }, 1);
        
    }


    void FinishGame()
    {
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
                playerMGprop[Constantes.PlayerKey_MinigameScore] = 0;
            }

            playerEntry.Value.SetCustomProperties(playerMGprop);
        }

        photonView.RPC(nameof(RPC_FinishedGame), RpcTarget.All);
    }

    [PunRPC]
    void RPC_FinishedGame()
    {
        AudioManager.instance.StopAmbientMusic();

        Player[] currentPlayers = PhotonNetwork.CurrentRoom.Players.Select(x => x.Value).ToArray();
        List<string> results = new();

        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            //Encuentra el player actual segun su Custom ID
            Player currentPlayer = System.Array.Find(currentPlayers, x => (int)x.CustomProperties[Constantes.PlayerKey_CustomID] == i + 1);
            int score = Mathf.RoundToInt(CoolFunctions.MapValues(Bonks[i].transform.position.y, originalHeight, bonkHeight, minMaxScore.x, minMaxScore.y));

            if ((bool)currentPlayer.CustomProperties[Constantes.PlayerKey_Eliminated])
            {
                results.Add("---");
            }
            else
            {
                results.Add(score.ToString("000"));
                
            }
        }

        UpdateRankingUI(results);
        Rankings.gameObject.SetActive(true);

        CoolFunctions.Invoke(this, () => { gameObject.SendMessage("FinishMinigame"); }, 1);
    }

    void UpdateRankingUI(List<string> results)
    {
        int playerCount = (int)PhotonNetwork.CurrentRoom.CustomProperties[Constantes.AmountPlayers_Room];
        List<Player> playerList = CoolFunctions.GetPlayerListOrdered();

        for (int i = 0; i < Rankings.childCount; i++)
        {
            Transform child = Rankings.GetChild(i);

            child.gameObject.SetActive(i < playerCount);

            if (i < playerCount)
            {
                int skinID = (int)playerList[i].CustomProperties[Constantes.PlayerKey_Skin];

                child.Find("Score").GetComponent<TMP_Text>().text = $"{results[i]}m";
                child.Find("Sprite").GetComponent<Image>().sprite = Resources.Load<Sprite>($"ReadySprites/{skinID}_notready");
                child.Find("Name").GetComponent<TMP_Text>().text = playerList[i].NickName;
            }
        }
    }


    void BonkPhysics(int bonkInt, bool enable)
    {
        GameObject targetBonk = Bonks[bonkInt];

        targetBonk.GetComponent<Rigidbody>().isKinematic = !enable;   }


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
