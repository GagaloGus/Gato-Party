using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinSelector : MonoBehaviourPunCallbacks
{
    public GameObject SkinPanel;
    public List<GameObject> skinButtons = new List<GameObject>();

    List<int> avaliableIDs = new List<int>();
    GameObject playerObject;
    GameObject onRangeIcon, skinpanel;

    private void Start()
    {
        Transform buttonPanel = SkinPanel.transform.Find("ButtonPanel");

        int temp = 0;
        foreach (Transform child in buttonPanel)
        {
            int index = temp;
            skinButtons.Add(child.gameObject);
            child.GetComponentInChildren<Button>().onClick.AddListener(() => { ClickedSkinButton(index); });

            temp++;
        }

        PhotonView[] allPhotonViews = FindObjectsOfType<PhotonView>();

        //Actualiza las skins
        foreach(PhotonView view in allPhotonViews)
        {
            if(view.IsMine)
            {
                playerObject = view.gameObject;
                break;
            }
        }

        onRangeIcon = transform.Find("OnRange").gameObject;
        skinpanel = FindObjectOfType<BasicButtonFunctions>().transform.Find("SkinSelectorPanel").gameObject;
    }

    private void Update()
    {
        if(Vector3.Distance(playerObject.transform.position, transform.position) < 1)
        {
            onRangeIcon.SetActive(true);
            if (Input.GetKeyDown(PlayerKeybinds.openSkinMenu))
            {
                skinpanel.SetActive(!skinpanel.activeInHierarchy);
            }
        }
        else
        {
            onRangeIcon.SetActive(false);
            skinpanel.SetActive(false);
        }
    }

    //Aqui ira toda la logica con la desactivacion de botones si las skins ya estan cogidas
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey(Constantes.PlayerKey_Skin))
        {
            //Actualiza la lista de las skins disponibles
            avaliableIDs = GetRemainingSkinIDs();

            string debugtext = "Remaining skins: <color=green> ";
            foreach(int i in avaliableIDs)
            {
                debugtext += $"{i}, ";
            }
            Debug.Log($"{debugtext}</color>");

            //Desactiva botones segun las skins que esten disponibles
            for (int i = 0; i < skinButtons.Count; i++)
            {
                Button button = skinButtons[i].GetComponentInChildren<Button>();
                button.interactable = avaliableIDs.Contains(i);
            }
        }
    }
    
    //Onclick del boton de la skin
    public void ClickedSkinButton(int ID)
    {
        //No hace falta ninguna comprobacion del boton aqui, ya la hace en el OnPlayerPropertiesUpdate
        DeselectOtherButtons(ID);
        FindObjectOfType<SalaEsperaSettings>().UpdatePlayerSkin(playerObject.GetComponent<PhotonView>().ViewID, ID);
    }

    //Cambia las animaciones dependiendo del boton que hayamos pulsado
    void DeselectOtherButtons(int buttonIndex)
    {
        for (int i = 0; i <= skinButtons.Count - 1; i++)
        {
            GameObject currentButton = skinButtons[i];
            Animator bordeAnimator = currentButton.GetComponent<Animator>();

            if(bordeAnimator != null)
            {
                bordeAnimator.SetBool("select", i == buttonIndex);
            }
            else
            {
                Debug.LogError($"No se encontro animator para {i}");
            }

        }
    }

    //Devuelve una lista con todas las skins disponibles
    public List<int> GetRemainingSkinIDs()
    {
        List<int> AllIDs = new List<int>();
        List<int> remainingIDs = new List<int>() { 0, 1, 2, 3, 4, 5 };

        foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            Player currentPlayer = player.Value;
            foreach (System.Collections.DictionaryEntry entry in currentPlayer.CustomProperties)
            {
                if ((string)entry.Key == Constantes.PlayerKey_Skin)
                {
                    AllIDs.Add((int)entry.Value);
                    break;
                }
            }
        }

        foreach (int id in AllIDs)
        {
            if (remainingIDs.Contains(id))
            { remainingIDs.Remove(id); }
        }

        return remainingIDs;
    }

    //Devuelve la skin disponible mas baja
    public int GetSkinIDAvaliable()
    {
        //Recorre todos los players y guarda sus IDs de sus skins
        List<int> skinList = new List<int>();
        foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            Player currentPlayer = player.Value;
            foreach (System.Collections.DictionaryEntry entry in currentPlayer.CustomProperties)
            {
                if ((string)entry.Key == Constantes.PlayerKey_Skin)
                {
                    skinList.Add((int)currentPlayer.CustomProperties[Constantes.PlayerKey_Skin]);
                    break;
                }
            }
        }

        //Ordena la lista de menor a mayor
        skinList.Sort();

        //ID mas bajo por default es 0
        int avaliableSkin = 0;

        //Si los IDs coinciden, se le suma 1
        foreach (int skinID in skinList)
        {
            if (skinID == avaliableSkin)
            {
                avaliableSkin++;
            }
        }

        return avaliableSkin;
    }
}
