using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class MGFindItem_Manager : MonoBehaviour
{
    public Transform ObjetoParent;
    List<GameObject> ObjetosList = new();
    int turnCount;
    bool choseObject;

    public List<GameObject> PlayerObjects = new List<GameObject>();

    [Header("Lata managment")]
    public int normalLataAmount;
    public int goldenLataAmount;

    bool[] isOpened;

    MGFindItem_Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = FindObjectOfType<MGFindItem_Camera>();

        //aparecen todos como false
        isOpened = new bool[ObjetoParent.childCount];
        choseObject = false;

        //Por si se pasa
        normalLataAmount = Mathf.Clamp(normalLataAmount, 0, ObjetoParent.childCount - 1);
        goldenLataAmount = Mathf.Clamp(goldenLataAmount, 0, ObjetoParent.childCount - 1 - normalLataAmount);

        foreach (Transform child in ObjetoParent)
        {
            ObjetosList.Add(child.gameObject);
        }

        List<int> randomPlaces = GenerateRandomChests(normalLataAmount, goldenLataAmount);

        RPC_StartMiniGame(randomPlaces.ToArray());
    }

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
    }

    private void Update()
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
                        RPC_OpenChest(i);
                        break;
                    }
                }
            }
        }
    }

    void RPC_OpenChest(int index)
    {
        isOpened[index] = true;
        choseObject = true;
        StartCoroutine(OpenChestMiniCinematic(index));
    }

    IEnumerator OpenChestMiniCinematic(int index)
    {
        GameObject currentPlayer = PlayerObjects[turnCount];
        Vector3 objectPos = ObjetosList[index].transform.position;
        Vector3 originalPlayerPos = currentPlayer.transform.position;

        cam.MoveRotateTowards(objectPos, 1f);

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
            currentPlayer.transform.position = Vector3.Lerp(objectPos + Vector3.forward * -2.5f, objectPos + Vector3.forward * -0.7f, t); // Movimiento
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        ObjetosList[index].GetComponent<MGFindItem_Cofre>().OpenChest();

        yield return new WaitForSeconds(3);

        cam.ResetPosRotOriginal();
        currentPlayer.transform.position = originalPlayerPos;

        turnCount = GetNextTurn();
        choseObject = false;
    }

    int GetNextTurn()
    {
        int nextTurn = turnCount;

        /*Dictionary<int, bool> eliminated = new Dictionary<int, bool>();
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
        while (sortedDic[nextTurn]);*/

        nextTurn++;
        nextTurn %= 4;

        return nextTurn;
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