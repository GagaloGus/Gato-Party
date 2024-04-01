using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerList_SMG : MonoBehaviourPunCallbacks
{
    public GameObject playerList;
    List<Image> playerImageList = new List<Image>();
    List<int> playerSkinsIDs = new List<int>();

    [Header("Button")]
    public Button readyButton;

    // Start is called before the first frame update
    void Start()
    {
        playerSkinsIDs.Clear();
        playerSkinsIDs = CoolFunctions.GetAllPlayerSkinIDs();

        //Solucion temporal para que siempre haya 4 imagenes
        while(playerSkinsIDs.Count < 4)
        {
            playerSkinsIDs.Add(0);
        }

        Hashtable playerProps = new Hashtable
        {
            [Constantes.PlayerKey_Ready_SMG] = false
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        for (int i = 0; i < playerList.transform.childCount; i++)
        {
            Transform child = playerList.transform.GetChild(i);

            playerImageList.Add(child.GetComponent<Image>());

            child.GetComponent<Image>().sprite = Resources.Load<Sprite>($"ReadySprites/{playerSkinsIDs[i]}_notready");
        }
        readyButton.interactable = true;

        readyButton.onClick.AddListener(OnReadyButtonClicked);
    }

    //si le das es true siempre no puedes decir no ready
    void OnReadyButtonClicked()
    {
        Hashtable playerProps = new Hashtable();
        playerProps[Constantes.PlayerKey_Ready_SMG] = true;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        readyButton.interactable = false;
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if(changedProps.ContainsKey(Constantes.PlayerKey_Ready_SMG))
        {
            bool isReady = (bool)changedProps[Constantes.PlayerKey_Ready_SMG];

            for (int i = 0; i < playerImageList.Count; i++)
            {
                Image image = playerImageList[i];

                //Si el ID del player es el mismo que el de su sprite
                if(int.Parse(image.gameObject.name) == (int)targetPlayer.CustomProperties[Constantes.PlayerKey_CustomID] && isReady)
                {
                    image.sprite = Resources.Load<Sprite>($"ReadySprites/{playerSkinsIDs[i]}_ready"); ;
                    break;
                }
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //Pone el sprite del respectivo player que se fue en gris
        for(int i = 0; i < playerImageList.Count; i++)
        {
            Image image = playerImageList[i];

            if (int.Parse(image.gameObject.name) == (int)otherPlayer.CustomProperties[Constantes.PlayerKey_CustomID])
            {
                image.sprite = Resources.Load<Sprite>($"ReadySprites/{playerSkinsIDs[i]}_notready"); ;
                image.color = new Color(0, 0, 0, 0.5f);
            }
        }
    }
}
