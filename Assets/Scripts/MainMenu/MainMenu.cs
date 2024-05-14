using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviourPunCallbacks
{
    [SerializeField] string currentMenu;

    [Header("Menus")]
    public GameObject StartMenu;
    public GameObject LobbyMenu;
    public GameObject LoadingScreen;

    // Start is called before the first frame update
    void Start()
    {
        if(PhotonNetwork.IsConnected)
        {
            ToggleMenu(LoadingScreen.name);
        }
        else
        {
            ToggleMenu(StartMenu.name);
        }
    }

    public void StartGame()
    {
        ChangeMenu(LoadingScreen.name, () => { PhotonNetwork.ConnectUsingSettings(); });
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        if(currentMenu != LobbyMenu.name)
            ChangeMenu(LobbyMenu.name);
    }

    public void ChangeMenu(string menuName, System.Action f)
    {
        StartCoroutine(FadeInFadeOutMenu(menuName, f));
    }

    public void ChangeMenu(string menuName)
    {
        StartCoroutine(FadeInFadeOutMenu(menuName, () => { bool fart; }));
    }

    void ToggleMenu(string menuName)
    {
        StartMenu.SetActive(menuName == StartMenu.name);
        LobbyMenu.SetActive(menuName == LobbyMenu.name);
        LoadingScreen.SetActive(menuName == LoadingScreen.name);

        currentMenu = menuName;
    }

    IEnumerator FadeInFadeOutMenu(string menuName, System.Action f)
    {
        Debug.Log($"Changing menu: {currentMenu} -> {menuName}");

        GameObject currentM = GetMenu(currentMenu), nextM = GetMenu(menuName);
        CanvasGroup cmCanvas = currentM.GetComponent<CanvasGroup>(), nmCanvas = nextM.GetComponent<CanvasGroup>();

        nextM.SetActive(true);
        nmCanvas.alpha = 0;

        cmCanvas.interactable = nmCanvas.interactable = false;

        for (float i = 0; i <= 1; i+= 0.02f)
        {
            cmCanvas.alpha = 1- i;
            yield return null;
        }

        for (float i = 0; i <= 1; i += 0.02f)
        {
            nmCanvas.alpha = i;
            yield return null;
        }

        ToggleMenu(menuName);

        if(menuName == LoadingScreen.name) { yield return new WaitForSeconds(0.5f); }

        cmCanvas.interactable = nmCanvas.interactable = true;
        cmCanvas.alpha = nmCanvas.alpha = 1;

        f();
    }

    public GameObject GetMenu(string menuName)
    {
        if (menuName == StartMenu.name) { return StartMenu; }
        else if (menuName == LobbyMenu.name) { return LobbyMenu; }
        else if (menuName == LoadingScreen.name) { return LoadingScreen; }

        return StartMenu;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void LeaveLobby()
    {
        ChangeMenu(LoadingScreen.name, () => { ChangeMenu(StartMenu.name); PhotonNetwork.Disconnect(); });
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("SalaEspera");
    }

    public string _currentMenu
    {
        get { return currentMenu; }
        set { currentMenu = value; }
    }
}
