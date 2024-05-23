using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerList_SMG : MonoBehaviourPunCallbacks
{
    public Transform playerListDisplay;
    List<Image> playerImageList = new List<Image>();
    List<int> playerSkinsIDs = new List<int>();

    AudioClip[] catSounds;

    [Header("Button")]
    public Button readyButton;

    private void Awake()
    {
        foreach(Transform child in playerListDisplay.Find("Display"))
        {
            child.gameObject.SetActive(true);
            child.GetChild(0).gameObject.SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        playerSkinsIDs.Clear();
        playerSkinsIDs = CoolFunctions.GetAllPlayerSkinIDs();

        Hashtable playerProps = new Hashtable
        {
            [Constantes.PlayerKey_Ready_SMG] = false
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        Transform display = playerListDisplay.Find("Display");
        int playerCount = (int)PhotonNetwork.CurrentRoom.CustomProperties[Constantes.AmountPlayers_Room];

        for (int i = 0; i < display.childCount; i++)
        {
            Transform child = display.GetChild(i);
            Image childImage = child.GetComponent<Image>();

            child.gameObject.SetActive(true);

            playerImageList.Add(childImage);

            if(i < playerCount)
            {
                childImage.sprite = Resources.Load<Sprite>($"ReadySprites/{playerSkinsIDs[i]}_notready");
            }
            else
            {
                childImage.sprite = Resources.Load<Sprite>($"ReadySprites/0_notready"); ;
                childImage.color = new Color(0, 0, 0, 0.5f);
            }

        }

        readyButton.interactable = true;
        readyButton.GetComponentInChildren<TMP_Text>().text = "Ready?";
        readyButton.onClick.AddListener(OnReadyButtonClicked);

        catSounds = Resources.LoadAll<AudioClip>("Sounds/Gato/Meow");
    }

    //si le das es true siempre no puedes decir no ready
    void OnReadyButtonClicked()
    {
        Hashtable playerProps = new Hashtable
        {
            [Constantes.PlayerKey_Ready_SMG] = true
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        readyButton.interactable = false;
        readyButton.GetComponentInChildren<TMP_Text>().text = "Ready!";
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if(changedProps.ContainsKey(Constantes.PlayerKey_Ready_SMG))
        {
            bool isReady = (bool)changedProps[Constantes.PlayerKey_Ready_SMG];
            int playerID = (int)targetPlayer.CustomProperties[Constantes.PlayerKey_CustomID];

            AudioManager.instance.PlaySFX2D(catSounds[playerID-1]);

            for (int i = 0; i < playerImageList.Count; i++)
            {
                Image image = playerImageList[i];

                //Si el ID del player es el mismo que el de su sprite
                if(int.Parse(image.gameObject.name) == playerID && isReady)
                {
                    image.sprite = Resources.Load<Sprite>($"ReadySprites/{playerSkinsIDs[i]}_ready"); ;
                    image.transform.GetChild(0).gameObject.SetActive(true);
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
