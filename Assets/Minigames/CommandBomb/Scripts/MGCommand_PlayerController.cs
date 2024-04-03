using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class MGCommand_PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Eliminated()
    {
        Debug.Log($"Me eliminaron :( {(int)PhotonNetwork.LocalPlayer.CustomProperties[Constantes.PlayerKey_CustomID]}");
    }
}
