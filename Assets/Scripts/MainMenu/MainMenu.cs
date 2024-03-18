using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviourPunCallbacks
{
    [Header("Room Info")]
    public Vector2Int minAndMaxPlayers;

    [Header("Menus")]
    public GameObject startMenu;
    public GameObject loadingScreen;
    public GameObject roomMenu;

    [Header("Create and Join Rooms")]
    public GameObject createRoomMenu;
    public TMP_InputField input_Create;
    public TMP_InputField input_MaxPlayers;
    public TMP_InputField input_Join;

    [Header("Rooms")]
    public GameObject roomListDisplay;
    public GameObject roomButtonPrefab;
    // Start is called before the first frame update
    void Start()
    {
        ToggleMenu(startMenu.name);
    }

    public void StartGame()
    {
        ToggleMenu(loadingScreen.name);
        createRoomMenu.SetActive(false);

        //Tarde 1 segundo en empezar a conectarse a Photon
        //prob se puede hacer mejor con un Anim event
        CoolFunctions.Invoke(this,
            () => { PhotonNetwork.ConnectUsingSettings(); }, 
            1);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        ToggleMenu(roomMenu.name);
    }

    #region Create And Join
    public void CreateRoom()
    {
        int maxPlayers = 4;

        if(int.TryParse(input_MaxPlayers.text, out int maxNum))
        {
            if (minAndMaxPlayers.x <= maxNum && maxNum <= minAndMaxPlayers.y)
            {
                maxPlayers = maxNum;
            }
            else { Debug.LogWarning($"{maxNum} se sale del rango de jugadores, se cambio a 4"); }
            
        }
        else { Debug.LogWarning($"No se pudo parsear: {input_MaxPlayers.text}, se cambio a 4"); }        

        PhotonNetwork.CreateRoom(
            roomName: input_Create.text,
            roomOptions: new RoomOptions() { MaxPlayers = maxPlayers, IsVisible = true, IsOpen = true },
            typedLobby: TypedLobby.Default,
            expectedUsers: null);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(input_Join.text);
    }

    public void JoinRoomInList(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Sala Espera");
    }

    #endregion

    //Genera los botones para unirse a las diferentes salas disponibles cada vez que se llama a la funcion
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Transform roomListContent = roomListDisplay.transform.Find("Viewport").Find("Content");

        //Destruye todos los botones
        foreach(Transform child in roomListContent)
        {
            Destroy(child.gameObject);
        }

        //Instancia nuevos botones con su informacion correspondiente
        for (int i = 0; i < roomList.Count; i++)
        {
            print(roomList[i].Name);
            GameObject roomButton = Instantiate(roomButtonPrefab, Vector3.zero, Quaternion.identity, roomListContent);

            roomButton.GetComponent<Room>().roomName.text = roomList[i].Name;
            roomButton.GetComponent<Room>().playerCountText.text = $"Players: {roomList[i].PlayerCount}/{roomList[i].MaxPlayers}";

        }
    }

    public void ToggleMenu(string menuName)
    {
        startMenu.SetActive(menuName == startMenu.name);
        loadingScreen.SetActive(menuName == loadingScreen.name);
        roomMenu.SetActive(menuName == roomMenu.name);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
