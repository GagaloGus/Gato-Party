using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class MGCommand_Manager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public KeySpritePair[] CommandKeys;
    public GameObject keySpritePrefab;
    public Transform KeyHolder;

    [Header("Photon")]
    public int[] randomKeyOrder;
    public int turnCount;
    public int roundCount;
    PhotonView photonView;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        roundCount = 0;

        Debug.LogAssertion("START COMMAND");

        foreach (Transform child in KeyHolder)
        {
            Destroy(child.gameObject);
        }
        KeyHolder.gameObject.SetActive(false);

        CoolFunctions.Invoke(this, () =>
        {
            if (PhotonNetwork.IsMasterClient)
            {
                StartGame();
                print("Start");
            }
        }, 0.3f);
        

        //StartCoroutine(nameof(Round));
    }

    System.Collections.IEnumerator Round()
    {
        yield return new WaitForSeconds(0.7f);

        //Espera a que el Player le de a la tecla que toca, y avanza a la siguiente
        for (int i = 0; i < randomKeyOrder.Length; i++)
        {
            KeySpritePair currentPair = CommandKeys[randomKeyOrder[i]];
            yield return new WaitUntil(() => Input.GetKeyDown(currentPair.KeyCode));

            photonView.RPC(nameof(RPC_ClickedKey), RpcTarget.All, i, true);
            yield return new WaitForSeconds(0.05f);
        }

        photonView.RPC(nameof(RPC_Finished), RpcTarget.All);

        //Al acabar espera un poquito
        yield return new WaitForSeconds(0.5f);
        EndTurn();
    }

    void StartGame()
    {
        object[] parameters =
        {
            GenerateRandomList(Mathf.FloorToInt(roundCount / 3) + 4),
            1
        };

        photonView.RPC(nameof(RPC_StartTurn), RpcTarget.All, parameters);
    }

    [PunRPC]
    void RPC_StartTurn(int[] randoms, int turn)
    {
        roundCount++;
        turnCount = turn;
        randomKeyOrder = randoms;

        //Instancia segun el orden del array 
        for (int i = 0; i < randomKeyOrder.Length; i++)
        {
            GameObject key = Instantiate(keySpritePrefab, KeyHolder);
            key.GetComponent<Image>().sprite = CommandKeys[randomKeyOrder[i]].Sprite;
        }
        //Activa el panel
        KeyHolder.gameObject.SetActive(true);

        //Si el turno corresponde con el ID del player
        if ((int)PhotonNetwork.LocalPlayer.CustomProperties[Constantes.PlayerKey_CustomID] == turn)
        {
            StartCoroutine(nameof(Round));
        }

        Debug.Log($"Turn: {turnCount} / Round number {roundCount}");
    }

    [PunRPC]
    void RPC_ClickedKey(int i, bool correct)
    {
        KeyHolder.GetChild(i).gameObject.GetComponent<Image>().color = Color.gray;
    }

    void EndTurn()
    {
        int nextPlayerturn = (turnCount % PhotonNetwork.CurrentRoom.PlayerCount) + 1;

        object[] parameters =
        {
            GenerateRandomList(Mathf.FloorToInt(roundCount / 3) + 4),
            nextPlayerturn
        };

        photonView.RPC(nameof(RPC_StartTurn), RpcTarget.All, parameters);
    }

    [PunRPC]
    void RPC_Finished()
    {
        KeyHolder.gameObject.SetActive(false);
        foreach (Transform child in KeyHolder)
        {
            Destroy(child.gameObject);
        }
    }

    int[] GenerateRandomList(int length)
    {
        List<int> result = new List<int>();

        for (int i = 0; i < length; i++)
        {
            int rnd = Random.Range(0, CommandKeys.Length);
            result.Add(rnd);
        }

        return result.ToArray();
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
