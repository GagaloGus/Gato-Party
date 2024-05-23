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

    public void PlaySFX(AudioClip clip)
    {
        AudioManager.instance.PlaySFX2D(clip);
    }

    public void SetMusicVolume(float volume)
    {
        AudioManager.instance.SetMusicVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        AudioManager.instance.SetSFXVolume(volume);
    }

    public void SetMasterVolume(float volume)
    {
        AudioManager.instance.SetMasterVolume(volume);
    }
}
