using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Username : MonoBehaviour
{
    public TMP_InputField inputField;
    public TMP_Text myUsername;
    public GameObject UsernamePanel;

    private void Start()
    {
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("USERNAME")))
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("USERNAME");
            myUsername.text = PlayerPrefs.GetString("USERNAME");
            UsernamePanel.SetActive(false);
        }
        else
        {
            UsernamePanel.SetActive(true);
        }
    }

    public void SaveUsername()
    {
        string newUsername = inputField.text;
        if (string.IsNullOrEmpty(newUsername))
        {
            newUsername = $"Player{Random.Range(1000,10000)}";
        }
        PhotonNetwork.NickName = newUsername;
        myUsername.text = newUsername;

        PlayerPrefs.SetString("USERNAME", newUsername);

        UsernamePanel.SetActive(false);
    }
}
