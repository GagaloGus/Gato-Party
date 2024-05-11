using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomHandler : MonoBehaviourPunCallbacks
{
    [Header("Room Info")]
    public Vector2Int minAndMaxPlayers;

    [Header("Create and Join Rooms")]
    public GameObject createRoomMenu;
    TMP_InputField input_Create, input_MaxPlayers;
    public TMP_InputField input_Join;

    [Header("Rooms")]
    public GameObject roomListDisplay;
    public GameObject roomButtonPrefab;
    public List<RoomInfo> roomList;
    TMP_Text PeopleConnectedText;

    MainMenu mainMenu;
    // Start is called before the first frame update
    void Start()
    {
        createRoomMenu.SetActive(false);
        mainMenu = FindObjectOfType<MainMenu>();

        input_Create = createRoomMenu.transform.Find("CreateInput").GetComponent<TMP_InputField>();
        input_MaxPlayers = createRoomMenu.transform.Find("MaxPlayersInput").GetComponent<TMP_InputField>();
        PeopleConnectedText = roomListDisplay.transform.Find("PlayersOnline").GetComponent<TMP_Text>();
    }

    private void Update()
    {
        PeopleConnectedText.text = $"Players Connected: {PhotonNetwork.CountOfPlayers}";
    }

    #region Create And Join
    public void CreateRoom()
    {
        string customRoomName = $"{PhotonNetwork.NickName}'s room";
        int maxPlayers = 4;

        if (!string.IsNullOrEmpty(input_Create.text))
        {
            if (CoolFunctions.SearchRoomByName(input_Create.text, roomList))
            {
                Debug.Log($"Ya existe una room con el nombre: {input_Create.text}");
                return;
            }

            customRoomName = input_Create.text;
        }

        if (int.TryParse(input_MaxPlayers.text, out int maxNum))
        {
            if (minAndMaxPlayers.x <= maxNum && maxNum <= minAndMaxPlayers.y)
            {
                maxPlayers = maxNum;
            }
            else { Debug.LogWarning($"{maxNum} se sale del rango de jugadores, se cambio a 4"); }

        }
        else { Debug.LogWarning($"No se pudo parsear: {input_MaxPlayers.text}, se cambio a 4"); }

        mainMenu.ChangeMenu(mainMenu.LoadingScreen.name, () =>
        {
            PhotonNetwork.CreateRoom(
                roomName: customRoomName,
                roomOptions: new RoomOptions() { MaxPlayers = maxPlayers, IsVisible = true, IsOpen = true },
                typedLobby: TypedLobby.Default,
                expectedUsers: null);
        });
    }

    public void JoinRoomInputText()
    {
        if (!string.IsNullOrEmpty(input_Join.text))
        {
            if (CoolFunctions.SearchRoomByName(input_Join.text, roomList))
            {
                PhotonNetwork.JoinRoom(input_Join.text);
            }
            else { Debug.LogWarning($"No se encontro ninguna room llamada: {input_Join.text}"); }
        }
        else { Debug.LogWarning("No se ha dado ningun input de cuarto"); }
    }

    public void JoinRoomInList(string roomName)
    {
        Debug.Log($"Join room {roomName}");
        mainMenu.ChangeMenu(mainMenu.LoadingScreen.name, () => { PhotonNetwork.JoinRoom(roomName); });
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning(message);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning(message);
    }

    #endregion

    //Genera los botones para unirse a las diferentes salas disponibles cada vez que se llama a la funcion
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //actualiza la roomlist guardada localmente
        this.roomList = roomList;

        Transform roomListContent = roomListDisplay.transform.Find("Viewport").Find("Content");
        GameObject NoRooms = roomListDisplay.transform.Find("Viewport").Find("NoRooms").gameObject;
        NoRooms.SetActive(true);

        //Destruye todos los botones
        foreach (Transform child in roomListContent)
        {
            Destroy(child.gameObject);
        }

        //Instancia nuevos botones con su informacion correspondiente
        for (int i = 0; i < roomList.Count; i++)
        {
            //Si la sala no esta vacia
            if (roomList[i].PlayerCount > 0)
            {
                GameObject roomButton = Instantiate(roomButtonPrefab, roomListContent);
                roomButton.transform.SetParent(roomButton.transform, false);

                Room room = roomButton.GetComponent<Room>();
                room.roomInfo = roomList[i];

                room.roomName.text = roomList[i].Name;

                if (!roomList[i].IsOpen)
                {
                    roomButton.GetComponent<Button>().interactable = false;
                    room.playerCountText.text = "Round started...";
                }
                else if (roomList[i].PlayerCount >= roomList[i].MaxPlayers)
                {
                    roomButton.GetComponent<Button>().interactable = false;
                    room.playerCountText.text = $"Players: {roomList[i].PlayerCount}/{roomList[i].MaxPlayers}";
                }
                else
                {
                    roomButton.GetComponent<Button>().interactable = true;
                    room.playerCountText.text = $"Players: {roomList[i].PlayerCount}/{roomList[i].MaxPlayers}";
                }

                NoRooms.SetActive(false);
            }
        }
    }
}
