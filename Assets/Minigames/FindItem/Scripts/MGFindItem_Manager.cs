using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class MGFindItem_Manager : MonoBehaviour
{
    public Transform ObjetoParent;
    [SerializeField] List<GameObject> ObjetosList = new();
    [SerializeField] int turnCount;
    [SerializeField] bool choseObject;

    public List<GameObject> PlayerObjects = new List<GameObject>();

    [Header("Lata managment")]
    public int normalLataAmount;
    public int goldenLataAmount;
    public int nLata_Score, gLata_Score;
    [SerializeField] int chosenChest;

    [Header("UI")]
    public TMP_Text LataNText;
    public TMP_Text LataGText;
    public GameObject FinishScreen;

    [SerializeField] bool[] isOpened;

    MGFindItem_Camera cam;
    MGFindItem_PlayerScores scoreScript;
    PhotonView photonView;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        scoreScript = FindObjectOfType<MGFindItem_PlayerScores>();
        cam = FindObjectOfType<MGFindItem_Camera>();

        FinishScreen.SetActive(false);

        FindObjectOfType<AssignObjectToPlayer>().AssignObject();

        CoolFunctions.Invoke(this, () =>
        {
            CoolFunctions.LoadAllTexturePacks<MGFindItem_PlayerController>();
        }, 0.5f);


        //aparecen todos como false
        isOpened = new bool[ObjetoParent.childCount];
        choseObject = false;

        //Por si se pasa
        normalLataAmount = Mathf.Clamp(normalLataAmount, 0, ObjetoParent.childCount - 2);
        goldenLataAmount = Mathf.Clamp(goldenLataAmount, 0, ObjetoParent.childCount - 1 - normalLataAmount);

        foreach (Transform child in ObjetoParent)
        {
            ObjetosList.Add(child.gameObject);
        }

        CoolFunctions.Invoke(this, () =>
        {
            if (PhotonNetwork.IsMasterClient)
            {
                List<int> randomPlaces = GenerateRandomChests(normalLataAmount, goldenLataAmount);

                photonView.RPC(nameof(RPC_StartMiniGame), RpcTarget.All, randomPlaces.ToArray());
            }
        }, 0.5f);
    }

    [PunRPC]
    void RPC_StartMiniGame(int[] numbers)
    {
        turnCount = 0;
        List<int> randomPlaces = numbers.ToList();

        Debug.Log($"<color=yellow>Random ints:</color> {CoolFunctions.StringContentOfList(randomPlaces, false)}");

        for (int i = 0; i < ObjetosList.Count; i++)
        {
            MGFindItem_Cofre cofreScript = ObjetosList[i].GetComponent<MGFindItem_Cofre>();

            //Cambia las latas
            cofreScript.UpdateLata((MGFindItem_Cofre.LataState)randomPlaces[i]);
        }

        RPC_NewRound(turnCount);
    }

    [PunRPC]
    void RPC_NewRound(int turn)
    {
        if ((int)PhotonNetwork.LocalPlayer.CustomProperties[Constantes.PlayerKey_CustomID] - 1 == turn)
        {
            StartCoroutine(Round());
        }

        turnCount = turn;

        scoreScript.ChangeTurn(turnCount);
    }

    System.Collections.IEnumerator Round()
    {
        yield return new WaitUntil(() =>
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Input.GetKeyUp(KeyCode.Mouse0) && !choseObject)
            {
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Debug.Log(hit.collider.gameObject.name);
                    for (int i = 0; i < ObjetosList.Count; i++)
                    {
                        if (hit.collider.gameObject == ObjetosList[i] && !isOpened[i])
                        {
                            chosenChest = i;
                            return true;
                        }
                    }
                }
            }

            return false;
        });

        photonView.RPC(nameof(RPC_OpenChest), RpcTarget.All, chosenChest);
    }

    private void Update()
    {
        LataNText.text = normalLataAmount.ToString();
        LataGText.text = goldenLataAmount.ToString();
    }

    [PunRPC]
    void RPC_OpenChest(int index)
    {
        isOpened[index] = true;
        chosenChest = index;
        choseObject = true;

        Debug.Log($"<color=red>{chosenChest} / {index}</color>");

        StartCoroutine(OpenChestMiniCinematic(index));
    }

    System.Collections.IEnumerator OpenChestMiniCinematic(int index)
    {
        GameObject currentPlayer = PlayerObjects[turnCount];
        MGFindItem_PlayerController playerScript = currentPlayer.GetComponent<MGFindItem_PlayerController>();
        MGFindItem_Cofre cofreScript = ObjetosList[index].GetComponent<MGFindItem_Cofre>();

        Vector3 objectPos = ObjetosList[index].transform.position;
        Vector3 originalPlayerPos = currentPlayer.transform.position;

        cam.MoveRotateTowards(objectPos, 1f);

        playerScript.Walk();

        float elapsedTime = 0f;

        currentPlayer.transform.rotation = Quaternion.LookRotation((objectPos - originalPlayerPos).normalized);
        //Ligero movimiento del player hacia adelante
        while (elapsedTime < 0.5f)
        {
            float t = elapsedTime / 0.5f;
            currentPlayer.transform.position = Vector3.Lerp(originalPlayerPos, originalPlayerPos + (objectPos - originalPlayerPos).normalized, t); // Movimiento
            yield return null;
            elapsedTime += Time.deltaTime;
        }
        elapsedTime = 0f;

        yield return new WaitForSeconds(0.75f);

        currentPlayer.transform.rotation = Quaternion.Euler(Vector3.zero);
        while (elapsedTime < 0.5f)
        {
            float t = elapsedTime / 0.5f;
            currentPlayer.transform.position = Vector3.Lerp(objectPos + Vector3.forward * -3f, objectPos + Vector3.forward * -0.6f, t) + Vector3.up * 0.1f; // Movimiento
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        playerScript.OpenChest((int)cofreScript.lataState);
        cofreScript.OpenChest();
    }

    public void FinishedOpeningChest()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPC_FinishRound), RpcTarget.All, turnCount, (int)ObjetosList[chosenChest].GetComponent<MGFindItem_Cofre>().lataState);
        }
    }

    [PunRPC]
    void RPC_FinishGame()
    {
        CoolFunctions.Invoke(this, () =>
        {
            FinishScreen.SetActive(true);

            List<int> scores = scoreScript.getPlayerScores;

            int ID = (int)PhotonNetwork.LocalPlayer.CustomProperties[Constantes.PlayerKey_CustomID] - 1;

            Hashtable playerP = new Hashtable
            {
                [Constantes.PlayerKey_MinigameScore] = scores[ID]
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerP);

        }, 2);

        CoolFunctions.Invoke(this, () =>
        {
            PhotonNetwork.LoadLevel("Puntuacion");
        }, 6);
    }

    [PunRPC]
    void RPC_FinishRound(int turn, int lataState)
    {
        turnCount = turn;
        Debug.Log($"<color=cyan>{turnCount}</color>");

        switch (lataState)
        {
            case 1: //normal
                scoreScript.UpdateScore(turnCount, nLata_Score);
                normalLataAmount--;
                break;

            case 2: //golden
                scoreScript.UpdateScore(turnCount, gLata_Score);
                goldenLataAmount--;
                break;
        }

        if(AllOpened())
        {
            RPC_FinishGame();
            return;
        }

        cam.ResetPosRotOriginal();
        PlayerObjects[turnCount].GetComponent<MGFindItem_PlayerController>().ResetPosRot();

        turnCount++;
        turnCount %= PhotonNetwork.CurrentRoom.PlayerCount;

        choseObject = false;

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPC_NewRound), RpcTarget.All, turnCount);
        }
    }

    bool AllOpened()
    {
        /*foreach(bool b in isOpened)
        {
            if (b == false) { return false; }
        }

        return true;*/

        return goldenLataAmount <= 0 && normalLataAmount <= 0;
    }

    List<int> GenerateRandomChests(int normalAmount, int goldenAmount)
    {
        List<int> result = new List<int>();
        foreach (GameObject cofre in ObjetosList)
        {
            result.Add(0);
        }

        while (normalAmount > 0)
        {
            int position = Random.Range(0, result.Count);

            if (result[position] == 0)
            {
                result[position] = 1;
                normalAmount--;
            }
        }

        while (goldenAmount > 0)
        {
            int position = Random.Range(0, result.Count);

            if (result[position] == 0)
            {
                result[position] = 2;
                goldenAmount--;
            }
        }

        return result;
    }

}