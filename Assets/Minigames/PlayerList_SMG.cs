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

    [Header("Button")]
    public Button readyButton;

    [Header("Sprites")]
    public Sprite notReady;
    public Sprite ready;
    // Start is called before the first frame update
    void Start()
    {
        Hashtable playerProps = new Hashtable();
        playerProps[Constantes.ReadyPlayerKey_SMG] = false;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        foreach (Transform child in playerList.transform) 
        { 
            playerImageList.Add(child.GetComponent<Image>());
            child.GetComponent<Image>().sprite = notReady;
        }
        readyButton.interactable = true;

        readyButton.onClick.AddListener(OnReadyButtonClicked);
    }

    //Te jodes, si le das es true siempre no puedes decir no ready
    void OnReadyButtonClicked()
    {
        //bool isReady = !PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(Constantes.ReadyPlayerKey_SMG) || !(bool)PhotonNetwork.LocalPlayer.CustomProperties[Constantes.ReadyPlayerKey_SMG];
        Hashtable playerProps = new Hashtable();
        playerProps[Constantes.ReadyPlayerKey_SMG] = true;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        readyButton.interactable = false;
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if(changedProps.ContainsKey(Constantes.ReadyPlayerKey_SMG))
        {
            bool isReady = (bool)changedProps[Constantes.ReadyPlayerKey_SMG];
            Debug.Log("Jugador " + targetPlayer.NickName + " está minujuego " + (isReady ? "listo" : "no listo"));

            foreach (Image image in playerImageList)
            {
                //Si el ID del player es el mismo que el de su sprite
                if(int.Parse(image.gameObject.name) == targetPlayer.ActorNumber && isReady)
                {
                    //Checkar si le dio a ready o no
                    image.sprite = ready;
                    break;
                }
            }
        }
    }
}
