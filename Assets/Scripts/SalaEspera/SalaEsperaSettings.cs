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

    [Header("Texts")]
    public TMP_Text roomName;
    public TMP_Text playerCount;

    // Start is called before the first frame update
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
    }

    void OnReadyButtonClicked()
    {
        // Cambiar el estado de listo del jugador local
        bool isReady = !PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(Constantes.ReadyPlayerKey_SalaEspera) || !(bool)PhotonNetwork.LocalPlayer.CustomProperties[Constantes.ReadyPlayerKey_SalaEspera];

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
        if (changedProps.ContainsKey(Constantes.ReadyPlayerKey_SalaEspera))
        {
            bool isReady = (bool)changedProps[Constantes.ReadyPlayerKey_SalaEspera];
            Debug.Log("Jugador " + targetPlayer.ActorNumber + ": " + (isReady ? "Listo" : "No Listo"));
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

        if (PhotonNetwork.CurrentRoom.PlayerCount == 0)
        {
            // Oculta la sala de la lista de salas disponibles
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("MainMenu");
        Debug.Log("Salido de la sala");
    }

    void UpdatePlayerCount()
    {
        playerCount.text = $"Players: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
    }

    public void ExitRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
}
