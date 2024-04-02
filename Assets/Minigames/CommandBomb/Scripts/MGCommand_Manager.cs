using Photon.Pun;
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
    public int roundCount;

    // Start is called before the first frame update
    void Start()
    {
        roundCount = 1;
        foreach (Transform child in KeyHolder)
        {
            Destroy(child.gameObject);
        }

        StartCoroutine(nameof(Round));
    }

    System.Collections.IEnumerator Round()
    {
        yield return new WaitForSeconds(0.5f);

        //Genera un array de INTs random
        randomKeyOrder = GenerateRandomList(Mathf.FloorToInt(roundCount / 3) + 4);

        //Instancia segun el orden del array 
        for (int i = 0; i < randomKeyOrder.Length; i++)
        {
            GameObject key = Instantiate(keySpritePrefab, KeyHolder);
            key.GetComponent<Image>().sprite = CommandKeys[randomKeyOrder[i]].Sprite;
        }

        //Activa el panel
        KeyHolder.gameObject.SetActive(true);

        //Espera a que el Player le de a la tecla que toca, y avanza a la siguiente
        for (int i = 0; i < randomKeyOrder.Length; i++) 
        {
            KeySpritePair currentPair = CommandKeys[randomKeyOrder[i]];
            yield return new WaitUntil(() => Input.GetKeyDown(currentPair.KeyCode));
            
            KeyHolder.GetChild(i).gameObject.GetComponent<Image>().color = Color.gray;
            yield return new WaitForSeconds(0.05f);
        }

        //Al acabar espera un poquito
        yield return new WaitForSeconds(0.5f);

        //Desactiva el panel y borra los hijos
        KeyHolder.gameObject.SetActive(false);
        foreach(Transform child in KeyHolder)
        {
            Destroy(child.gameObject);
        }

        roundCount++;

        StartCoroutine(nameof(Round));
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
