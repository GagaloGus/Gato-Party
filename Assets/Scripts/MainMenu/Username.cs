using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        if (CheckForDuplicateNicknames(newUsername))
        {
            Debug.LogWarning($"Nickname {PhotonNetwork.NickName} ya usado");
            return;
        }

        if (string.IsNullOrWhiteSpace(newUsername))
        {
            newUsername = $"Player{Random.Range(1000,10000)}";
        }
        PhotonNetwork.NickName = newUsername;
        myUsername.text = newUsername;

        PlayerPrefs.SetString("USERNAME", newUsername);
        UsernamePanel.SetActive(false);
    }

    bool CheckForDuplicateNicknames(string nick)
    {
        /*List<Player> playersInLobby = PhotonNetwork.PlayerList.ToList();

        string debug = "<color=yellow>Nicks:</color> ";
        foreach (Player p in playersInLobby)
        {
            debug += $"{p.NickName}, ";
        }
        print(debug);

        foreach (Player p in playersInLobby)
        {
            if (p.NickName == nick)
                return true;
        }*/

        return false;
    }
}
