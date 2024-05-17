using Photon.Pun;
using ExitGames.Client.Photon;

using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SalaEsperaSettings : MonoBehaviourPunCallbacks
{
    [Header("References")]
    public GameObject playerList;
    public Button playerReadyButton;
    public Button startGameButton;
    public GameObject errorMessage;

    [Header("Texts")]
    public TMP_Text roomName;
    public TMP_Text playerCount;

    [Header("Skin")]
    public int CurrentSkinID;

    PhotonView photonView;
    CanvasChat canvasChat;
    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        canvasChat = FindObjectOfType<CanvasChat>();

        Hashtable roomProps = new Hashtable
        {
            [Constantes.MinigameScene_Room] = "SALA_ESPERA",
            [Constantes.RoundsOver_Room] = false
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
        PhotonNetwork.CurrentRoom.IsOpen = true;
    }
    void Start()
    {
        //Establece el is ready de la sala de espera a false al entrar, por si acaso

        UpdatePlayerCount();
        roomName.text = $"Room Name:\n{PhotonNetwork.CurrentRoom.Name}";

        startGameButton.interactable = false;

        startGameButton.gameObject.GetComponent<Image>().color = (PhotonNetwork.IsMasterClient ? new Color(1, 0.72f, 0) : Color.gray);
        startGameButton.GetComponentInChildren<TMP_Text>().text = (PhotonNetwork.IsMasterClient ? "Start Game" : "Waiting...");

        playerReadyButton.onClick.AddListener(OnReadyButtonClicked);

        errorMessage.SetActive(false);
    }

    void OnReadyButtonClicked()
    {
        // Cambiar el estado de listo del jugador local
        bool isReady = !(bool)PhotonNetwork.LocalPlayer.CustomProperties[Constantes.PlayerKey_Ready_SalaEspera];

        Hashtable playerProps = new Hashtable
        {
            [Constantes.PlayerKey_Ready_SalaEspera] = isReady
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        //actualiza el boton de Ready
        playerReadyButton.GetComponent<Animator>().SetBool("ready", isReady);

        canvasChat.SendSystemMessage($"Player {PhotonNetwork.LocalPlayer.NickName} is {(isReady ? "<color=green>ready" : "<color=red>not ready")}</color>");
    }

    //Se llama cada vez que se actualizan las propiedades de un player con hastables
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        // Actualizar el estado de listo del jugador en el texto
        if (changedProps.ContainsKey(Constantes.PlayerKey_Ready_SalaEspera) && targetPlayer == PhotonNetwork.LocalPlayer)
        {
            bool isReady = (bool)changedProps[Constantes.PlayerKey_Ready_SalaEspera];
            Debug.Log("Jugador " + targetPlayer.NickName + ": " + (isReady ? "Listo" : "No Listo"));

            playerReadyButton.GetComponent<Animator>().SetBool("ready", isReady);
        }

        //Actualiza si el boton de empezar el juego es interactuable si somos el ADMIN y todos estan listos y hay mas de una persona en la sala
        startGameButton.interactable = PhotonNetwork.IsMasterClient && AllPlayersReady() && PhotonNetwork.CurrentRoom.PlayerCount > 1;
    }

    bool AllPlayersReady()
    {
        // Verificar si todos los jugadores en la sala están listos
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!(player.CustomProperties.ContainsKey(Constantes.PlayerKey_Ready_SalaEspera) && (bool)player.CustomProperties[Constantes.PlayerKey_Ready_SalaEspera]))
            {
                return false;
            }
        }
        return true;
    }

    //Actualiza el contador de gente en la sala
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        canvasChat.SendSystemMessage($"Player <color=yellow>{newPlayer.NickName}</color> entered the room!");
        UpdatePlayerCount();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        canvasChat.SendSystemMessage($"Player <color=yellow>{otherPlayer.NickName}</color> left the room");
        UpdatePlayerCount();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        //Al cambiar de Master Client (o sea, que el Master Client se salio) se cierra la room
        errorMessage.SetActive(true);
        errorMessage.GetComponentInChildren<TMP_Text>().text =
            $"Master Client left the room";

        //Hace la sala invisible para que nadie se una
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.CurrentRoom.IsOpen = false;

        //Los players no se muevan
        SE_PlayerController[] playerList = FindObjectsOfType<SE_PlayerController>();
        foreach (SE_PlayerController player in playerList)
        {
            player.player_canMove = false;
        }

        //Espera tantos segundos para salir de la room
        CoolFunctions.Invoke(this, () =>
        {
            PhotonNetwork.LeaveRoom();
        }, 3);
    }
    public void ExitRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        Hashtable playerProps = new Hashtable
        {
            [Constantes.PlayerKey_CustomID] = -1,
            [Constantes.PlayerKey_Skin] = -1
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        //Al salir de la sala carga la escena del lobby
        SceneManager.LoadScene("MainMenu");
    }

    void UpdatePlayerCount()
    {
        Hashtable resetProp = new Hashtable
        {
            [Constantes.PlayerKey_Ready_SalaEspera] = false
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(resetProp);

        playerCount.text = $"Players: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
    }

    //Cambia la skin de un player en especifico para todos los jugadores de la sala
    public void UpdatePlayerSkin(int photonViewID, int skinID)
    {
        photonView.RPC(nameof(LoadTexturePacks), RpcTarget.All, photonViewID, skinID);
        CurrentSkinID = skinID;

        Hashtable playerProp = new Hashtable
        {
            [Constantes.PlayerKey_Skin] = skinID
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties (playerProp);
    }

    [PunRPC]
    public void LoadTexturePacks(int photonViewID, int SkinID)
    {
        AnimationBundles animationBundles = FindObjectOfType<AnimationBundles>();

        //Encuentra el gameobject del player segun su PhotonView ID
        GameObject player = PhotonView.Find(photonViewID).gameObject;
        ChangeTextureAnimEvent textureScript = player.GetComponentInChildren<ChangeTextureAnimEvent>();

        AnimationSpriteBundle selectedBundle = System.Array.Find(animationBundles.bundles.ToArray(), x => (int)x.skinName == SkinID);

        try
        {
            textureScript.UpdateAnimationDictionary(selectedBundle.texturePacks, SkinID);
            print($"Loaded sprites of <color=cyan>{PhotonView.Find(photonViewID).Owner.NickName}</color>, skin id: {SkinID} <color=yellow>({selectedBundle.skinName})</color>");
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message, this);
            //Si algo falla, le pone la skin default
            textureScript.UpdateAnimationDictionary(animationBundles.bundles[0].texturePacks, 0);
        }
    }
}