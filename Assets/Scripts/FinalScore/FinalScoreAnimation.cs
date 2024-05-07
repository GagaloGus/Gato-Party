using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class FinalScoreAnimation : MonoBehaviour
{
    [Header("Player Objects")]
    [SerializeField] List<GameObject> PlayerObjects;
    [SerializeField] List<GameObject> Boxes;

    [Header("UI Objects")]
    public GameObject Rankings;
    [SerializeField] Animator CanvasAnimator;

    List<Player> Players = new List<Player>();

    // Start is called before the first frame update
    void Start()
    {
        CanvasAnimator = FindObjectOfType<BasicButtonFunctions>().GetComponent<Animator>();
        CanvasAnimator.speed = 0;

        Rankings.SetActive(false);

        StartCoroutine(nameof(AnimationTimeLine));

        for (int i = 0; i < PlayerObjects.Count; i++)
        {
            PlayerObjects[i].GetComponent<FinalScore_PlayerAnimator>().order = 4 - i;
        }
    }

    System.Collections.IEnumerator AnimationTimeLine()
    {
        //Espera (suspense)
        yield return new WaitForSeconds(1);

        //Ordena los players de mayor a menor por su puntuacion
        Players = GetPlayerFinalScoreSorted();

        //Carga sus skins acorde a la lista ordenada
        LoadSkins();

        //Recorre solo los 3 primeros players
        //Van cayendo poco a poco en orden inverso
        for (int i = 3; i >= 1; i--)
        {
            Boxes[i].SetActive(false);
            print($"Cae el player {i + 1}");

            yield return new WaitForSeconds(1f);
        }

        //Espera un poco y cae el primer player
        yield return new WaitForSeconds(0.5f);
        Boxes[0].SetActive(false);
        print($"Cae el player {1}");

        //Mostrar la UI de la puntuacion
        yield return new WaitForSeconds(2.5f);
        UpdateScoreUI();
        Rankings.SetActive(true);

        yield return new WaitForSeconds(1f);
        CanvasAnimator.speed = 1f;
    }

    List<Player> GetPlayerFinalScoreSorted()
    {
        Dictionary<Player, int> sortedDic = new Dictionary<Player, int>();

        //Si la propKey es la de la puntuacion del anterior minijuego, guarda el propValue en el diccionario con su respectivo player
        foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            Hashtable playerProps = player.Value.CustomProperties;
            foreach (System.Collections.DictionaryEntry props in playerProps)
            {
                if ((string)props.Key == Constantes.PlayerKey_TotalScore)
                {
                    sortedDic.Add(player.Value, (int)props.Value);
                    break;
                }
            }
        }

        var sortedRanking = sortedDic.OrderByDescending(x => x.Value);
        List<Player> temp = sortedRanking.Select(x => x.Key).ToList();

        return temp;
    }
    void UpdateScoreUI()
    {
        Transform content = Rankings.transform.Find("Content");

        for (int i = 0; i < content.childCount; i++)
        {
            Transform child = content.transform.GetChild(i);

            child.gameObject.SetActive(true);

            try
            {
                child.Find("Name").GetComponent<TMP_Text>().text = Players[i].NickName;
                child.Find("Score").GetComponent<TMP_Text>().text = $"{(int)Players[i].CustomProperties[Constantes.PlayerKey_TotalScore]} pts.";
            }
            catch
            {
                child.Find("Name").GetComponent<TMP_Text>().text = $"Player {i+1}";
                child.Find("Score").GetComponent<TMP_Text>().text = $"0 pts.";
            }
            
        }
    }

    void LoadSkins()
    {
        AnimationBundles animationBundles = FindObjectOfType<AnimationBundles>();

        for (int i = 0; i < PlayerObjects.Count; i++)
        {
            ChangeTextureAnimEvent textureScript = PlayerObjects[i].GetComponentInChildren<ChangeTextureAnimEvent>();
            
            try
            {
                int SkinID = (int)Players[i].CustomProperties[Constantes.PlayerKey_Skin];

                AnimationSpriteBundle selectedBundle = System.Array.Find(animationBundles.bundles.ToArray(), x => (int)x.skinName == SkinID);

                textureScript.UpdateAnimationDictionary(selectedBundle.texturePacks, SkinID);
                Debug.Log($"Loaded sprites of <color=cyan>{Players[i].NickName}</color>, skin id: {SkinID} <color=yellow>({selectedBundle.skinName})</color> -> {PlayerObjects[i].name}");
            }
            catch
            {
                Debug.Log($"Loaded default sprite -> {PlayerObjects[i].name}");
                //Si algo falla, le pone la skin default
                textureScript.UpdateAnimationDictionary(animationBundles.bundles[0].texturePacks, 0);
            }
        }
    }
}
