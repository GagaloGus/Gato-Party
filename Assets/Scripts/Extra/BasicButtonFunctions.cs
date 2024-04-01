using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicButtonFunctions : MonoBehaviourPunCallbacks
{
    public void ChangeScene(string sceneToChange)
    {
        SceneManager.LoadScene(sceneToChange);
    }

    public void ExitGame()
    {
        Application.Quit();
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
}
