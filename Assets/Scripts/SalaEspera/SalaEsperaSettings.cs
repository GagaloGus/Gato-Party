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


    private void Awake()
    {
        Hashtable roomProps = new Hashtable();
        roomProps[Constantes.MinigameScene_Room] = "SALA_ESPERA";
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
    }
    void Start()
    {
        //Establece el is ready de la sala de espera a false al entrar, por si acaso
        Hashtable playerProps = new Hashtable();
        playerProps[Constantes.ReadyPlayerKey_SalaEspera] = false;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        UpdatePlayerCount();
        roomName.text = $"Room Name:\n{PhotonNetwork.CurrentRoom.Name}";

        startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);

        playerReadyButton.onClick.AddListener(OnReadyButtonClicked);

        errorMessage.SetActive(false);
    }

    void OnReadyButtonClicked()
    {
        // Cambiar el estado de listo del jugador local
        bool isReady = !(bool)PhotonNetwork.LocalPlayer.CustomProperties[Constantes.ReadyPlayerKey_SalaEspera];

        Hashtable playerProps = new Hashtable();
        playerProps[Constantes.ReadyPlayerKey_SalaEspera] = isReady;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        //actualiza el boton de Ready
        playerReadyButton.GetComponentInChildren<TMP_Text>().text = isReady ? "Ready!" : "Ready?";
        playerReadyButton.GetComponent<Image>().color = isReady ? Color.gray : Color.white;
    }

    //Se llama cada vez que se actualizan las propiedades de un player con hastables
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        // Actualizar el estado de listo del jugador en el texto
        if (changedProps.ContainsKey(Constantes.ReadyPlayerKey_SalaEspera) && targetPlayer == PhotonNetwork.LocalPlayer)
        {
            bool isReady = (bool)changedProps[Constantes.ReadyPlayerKey_SalaEspera];
            Debug.Log("Jugador " + targetPlayer.ActorNumber + ": " + (isReady ? "Listo" : "No Listo"));

            playerReadyButton.GetComponentInChildren<TMP_Text>().text = isReady ? "Ready!" : "Ready?";
            playerReadyButton.GetComponent<Image>().color = isReady ? Color.gray : Color.white;
        }

        //Actualiza si el boton de empezar el juego es interactuable si somos el ADMIN y todos estan listos
        startGameButton.interactable = PhotonNetwork.IsMasterClient && AllPlayersReady();
    }

    bool AllPlayersReady()
    {
        // Verificar si todos los jugadores en la sala están listos
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!(player.CustomProperties.ContainsKey(Constantes.ReadyPlayerKey_SalaEspera) && (bool)player.CustomProperties[Constantes.ReadyPlayerKey_SalaEspera]))
            {
                return false;
            }
        }
        return true;
    }

    //Actualiza el contador de gente en la sala
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerCount();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerCount();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        //Al cambiar de ADMIN (o sea, que el admin se salio) se cierra la room
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
        //Al salir de la sala carga la escena del lobby
        SceneManager.LoadScene("MainMenu");
    }

    void UpdatePlayerCount()
    {
        playerCount.text = $"Players: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
    }

}
