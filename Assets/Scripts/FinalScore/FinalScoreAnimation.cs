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
    public List<GameObject> PlayerObjects;
    public List<GameObject> Boxes;
    List<Vector3> Positions = new List<Vector3>();
    [SerializeField] int position;

    [Header("UI Objects")]
    public GameObject Rankings;
    public GameObject Confetti;
    [SerializeField] Animator CanvasAnimator;

    List<Player> Players = new List<Player>();

    [Header("Audio")]
    public AudioClip Theme;
    public AudioClip[] Tingles;

    // Start is called before the first frame update
    void Start()
    {
        AudioManager.instance.StopAmbientMusic();

        Confetti.SetActive(false);

        CanvasAnimator = FindObjectOfType<BasicButtonFunctions>().GetComponent<Animator>();
        CanvasAnimator.speed = 0;

        StartCoroutine(nameof(AnimationTimeLine));

        for (int i = 0; i < PlayerObjects.Count; i++)
        {
            PlayerObjects[i].GetComponent<FinalScore_PlayerAnimator>().order = 4 - i;
        }

        Positions.Clear();
        foreach(GameObject obj in Boxes) 
        {
            Positions.Add(obj.transform.position);
        }
    }

    System.Collections.IEnumerator AnimationTimeLine()
    {
        //Espera (suspense)
        yield return new WaitForSeconds(1);

        //Ordena los players de mayor a menor por su puntuacion
        Players = GetPlayerFinalScoreSorted();

        //Obtiene la posicion del jugador local
        position = Players.IndexOf(PhotonNetwork.LocalPlayer);

        //Carga sus skins acorde a la lista ordenada
        LoadSkins();

        //Recorre solo los 3 primeros players
        //Van cayendo poco a poco en orden inverso
        for(int i = 3; i >= 0; i--)
        {
            //Espera un poco mas si es el primer player
            yield return new WaitForSeconds((i == 0 ? 1.5f : 1));

            PlayerObjects[i].transform.position = Positions[i];
        }

        yield return new WaitForSeconds(1.5f);
        AudioClip positionClip = Tingles[Mathf.Clamp(position, 0, Tingles.Length)];

        AudioManager.instance.PlaySFX2D(positionClip);
        Confetti.SetActive(true);
        
        //Espera lo que dura el tingle
        yield return new WaitForSeconds(positionClip.length + 0.5f);

        AudioManager.instance.ClearAudioList();
        AudioManager.instance.PlayAmbientMusic(Theme);
        //Mostrar la UI
        UpdateScoreUI();
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
