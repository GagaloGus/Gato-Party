using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class MGCommand_Manager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public KeySpritePair[] CommandKeys;
    public GameObject keySpritePrefab;
    public Transform KeyHolder;
    public GameObject ExplosionSprite;

    [Header("Objects")]
    public List<GameObject> PlayerObjects = new List<GameObject>();
    public GameObject bombPrefab;

    [Header("Countdown")]
    public Slider CountdownSlider;
    public float maxTime;
    public float interval;
    public float playerEliminatedReduceTime;
    public float roundCountReduceTime;
    public float minTime;

    float currentTime;

    [Header("Photon")]
    public int[] randomKeyOrder;
    public int turnCount;
    public int roundCount;
    public int playersRemaining;
    PhotonView photonView;

    // Start is called before the first frame update
    void Start()
    {
        interval /= 10;
        photonView = GetComponent<PhotonView>();
        turnCount = 1;
        roundCount = 0;

        ExplosionSprite.SetActive(false);

        GameObject player = FindObjectOfType<AssignObjectToPlayer>().AssignObject(PlayerObjects);
    }

    void Setup()
    {
        CoolFunctions.LoadAllTexturePacks<MGCommand_PlayerController>();

        foreach (Transform child in KeyHolder.Find("Content"))
        {
            Destroy(child.gameObject);
        }
        KeyHolder.gameObject.SetActive(false);
    }

    void StartMinigame()
    {
        playersRemaining = PhotonNetwork.CurrentRoom.PlayerCount;
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPC_StartTurn), RpcTarget.All, GenerateRandomList(Mathf.FloorToInt(roundCount / 3) + 4), 1);
        }
    }

    [PunRPC]
    void RPC_StartTurn(int[] randoms, int turn)
    {
        //turnCount = turno anterior
        //turn = turno actual
        Debug.Log($"Actual Turn:{turn} -> Object: {PlayerObjects[turn - 1].name} / Rounds: {roundCount}");

        PlayerObjects[turnCount - 1].GetComponent<MGCommand_PlayerController>().ThrowBomb();
        PlayerObjects[turn - 1].GetComponent<MGCommand_PlayerController>().RecieveBomb();

        roundCount++;
        turnCount = turn;
        randomKeyOrder = randoms;
        CountdownSlider.value = 1;
        currentTime = maxTime;

        bombPrefab.GetComponent<MGCommand_Bomb>().ThrowBomb(turn - 1, 1);
        Camera.main.GetComponent<MGCommand_Camera>().RotateTowardsPlayer(turn - 1, 1);

        CoolFunctions.Invoke(this, () =>
        {
            //Instancia segun el orden del array 
            for (int i = 0; i < randomKeyOrder.Length; i++)
            {
                GameObject key = Instantiate(keySpritePrefab, KeyHolder.Find("Content"));
                key.GetComponent<Image>().sprite = CommandKeys[randomKeyOrder[i]].Sprite;
            }
            //Activa el panel
            KeyHolder.gameObject.SetActive(true);

            //Si el turno corresponde con el ID del player
            if ((int)PhotonNetwork.LocalPlayer.CustomProperties[Constantes.PlayerKey_CustomID] == turnCount)
            {
                StartCoroutine(nameof(Round));
            }
        }, 1);
    }

    [PunRPC]
    void RPC_StartCountdown(float maxTime, float interval)
    {
        this.maxTime = maxTime;
        this.interval = interval;

        PlayerObjects[turnCount - 1].GetComponent<MGCommand_PlayerController>().IdleBomb();

        Debug.Log($"StartCountdown {this.maxTime}, {this.interval}");
        InvokeRepeating(nameof(Countdown), 0, interval);

    }

    //Se ejecuta para todos pq se invoca dentro de un RPC
    void Countdown()
    {
        currentTime -= interval;
        CountdownSlider.value = CoolFunctions.MapValues(currentTime, 0, maxTime, 0, 1);
        bombPrefab.GetComponent<Animator>().SetFloat("multi", CoolFunctions.MapValues(currentTime, 0, maxTime, 5, 1));

        if (currentTime < 0)
        {
            ExplosionSprite.SetActive(true);
            bombPrefab.transform.position = PlayerObjects[GetNextTurn()].transform.position + Vector3.up * 15;
            CancelInvoke(nameof(Countdown));

            //Se cancela la corrutina de la ronda
            StopAllCoroutines();

            PlayerObjects[turnCount - 1].GetComponent<MGCommand_PlayerController>().Eliminated();
            //Si el turno corresponde con el ID del player
            if ((int)PhotonNetwork.LocalPlayer.CustomProperties[Constantes.PlayerKey_CustomID] == turnCount)
            {
                Hashtable playerElim = new Hashtable
                {
                    [Constantes.PlayerKey_Eliminated] = true,
                    [Constantes.PlayerKey_MinigameScore] = PhotonNetwork.CurrentRoom.PlayerCount - playersRemaining
                };
                PhotonNetwork.LocalPlayer.SetCustomProperties(playerElim);
            }

            playersRemaining--;
            Debug.Log($"Players remaining: {playersRemaining}");

            ReduceMaxTime(playerEliminatedReduceTime);

            CoolFunctions.Invoke(this, () =>
            {
                if (PhotonNetwork.IsMasterClient) { EndTurn(); }
            }, 2.5f);
        }
    }

    //Cada ronda
    System.Collections.IEnumerator Round()
    {
        ReduceMaxTime(roundCountReduceTime);
        photonView.RPC(nameof(RPC_StartCountdown), RpcTarget.All, maxTime, interval);

        //Espera a que el Player le de a la tecla que toca, y avanza a la siguiente
        for (int i = 0; i < randomKeyOrder.Length; i++)
        {
            KeySpritePair currentPair = CommandKeys[randomKeyOrder[i]];

            bool correctKey = false;

            yield return new WaitUntil(() =>
            {
                if (Input.anyKeyDown)
                {
                    if (Input.GetKeyDown(currentPair.KeyCode))
                    {
                        correctKey = true;
                    }
                    else
                    {
                        correctKey = false;
                    }

                    photonView.RPC(nameof(RPC_ClickedKey), RpcTarget.All, i, correctKey);
                }

                return correctKey;
            });

            yield return new WaitForSeconds(0.01f);
        }

        EndTurn();
    }

    [PunRPC]
    void RPC_ClickedKey(int i, bool correct)
    {
        if (correct)
        {
            KeyHolder.Find("Content").GetChild(i).gameObject.GetComponent<Image>().color = Color.gray;
        }
        else
        {
            currentTime -= interval * 7;
        }
    }

    //Determina si acaba la ronda segun los jugadores que queden
    void EndTurn()
    {
        //Ajustes de cada vez que termina una ronda
        photonView.RPC(nameof(RPC_Finished), RpcTarget.All);
        int nextPlayerturn = GetNextTurn();

        if (playersRemaining <= 1)
        {
            photonView.RPC(nameof(RPC_FinishedGame), RpcTarget.All);
        }
        else
        {
            photonView.RPC(nameof(RPC_StartTurn), RpcTarget.All, GenerateRandomList(Mathf.FloorToInt(roundCount / 3) + 4), nextPlayerturn);
        }
    }

    [PunRPC]
    void RPC_FinishedGame()
    {
        Debug.Log($"PLAYER {(turnCount % PhotonNetwork.CurrentRoom.PlayerCount) + 1} WINS!!!!");

        //Le da la puntuacion mas alta al jugador que no quedo eliminado
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (KeyValuePair<int, Player> playerEntry in PhotonNetwork.CurrentRoom.Players)
            {
                if (!(bool)playerEntry.Value.CustomProperties[Constantes.PlayerKey_Eliminated])
                {
                    Hashtable newProps = new Hashtable
                    {
                        [Constantes.PlayerKey_MinigameScore] = 888 //Numero alto, en verdad no tiene nada que ver, solo tiene que ser el mas alto
                    };
                    playerEntry.Value.SetCustomProperties(newProps);

                    break;
                }
            }
        }

        CoolFunctions.Invoke(this, () =>
        {
            PhotonNetwork.LoadLevel("Puntuacion");
        }, 1.5f);
    }


    [PunRPC]
    void RPC_Finished()
    {
        CancelInvoke(nameof(Countdown));
        KeyHolder.gameObject.SetActive(false);
        foreach (Transform child in KeyHolder.Find("Content"))
        {
            Destroy(child.gameObject);
        }
    }

    int[] GenerateRandomList(int length)
    {
        length = Mathf.Clamp(length, 0, 8);
        List<int> result = new List<int>();

        for (int i = 0; i < length; i++)
        {
            int rnd = Random.Range(0, CommandKeys.Length);
            result.Add(rnd);
        }

        return result.ToArray();
    }

    int GetNextTurn()
    {
        int nextTurn = turnCount;

        Dictionary<int, bool> eliminated = new Dictionary<int, bool>();
        foreach (KeyValuePair<int, Player> playerEntry in PhotonNetwork.CurrentRoom.Players)
        {
            Player player = playerEntry.Value;
            eliminated.Add((int)player.CustomProperties[Constantes.PlayerKey_CustomID], (bool)player.CustomProperties[Constantes.PlayerKey_Eliminated]);
        }

        //Ordena de menor a mayor
        Dictionary<int, bool> sortedDic = eliminated.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

        do
        {
            nextTurn = (nextTurn % sortedDic.Count) + 1;
            Debug.Log($"Next turn: {nextTurn}, <color={(sortedDic[nextTurn] ? "red" : "green")}>{sortedDic[nextTurn]}</color>");
        }
        while (sortedDic[nextTurn]);

        return nextTurn;
    }

    void ReduceMaxTime(float reduceAmount)
    {
        maxTime = Mathf.Clamp(maxTime-reduceAmount, minTime, Mathf.Infinity);
    }
}

[System.Serializable]
public class KeySpritePair
{
    public KeyCode KeyCode;
    public Sprite Sprite;

    public KeySpritePair(KeyCode keyCode, Sprite sprite)
    {
        this.KeyCode = keyCode;
        this.Sprite = sprite;
    }
}
