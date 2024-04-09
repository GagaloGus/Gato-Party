using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignObjectToPlayer : MonoBehaviour
{
    public List<GameObject> objetosControlables;

    public GameObject AssignObject(List<GameObject> playerObjectsList)
    {
        objetosControlables = playerObjectsList;
        return AssignObject();
    }

    public GameObject AssignObject()
    {
        for(int i = 0; i < objetosControlables.Count; i++)
        {
            //Activa objetos segun los jugadores que esten en la sala
            objetosControlables[i].SetActive(i < PhotonNetwork.CurrentRoom.PlayerCount);
        }

        GameObject temp = null;
        // Obtener el indice del LocalPlayer
        int localPlayerIndex = (int)PhotonNetwork.LocalPlayer.CustomProperties[Constantes.PlayerKey_CustomID] - 1;

        // Asegurarse de que el indice del cliente local este dentro del rango
        if (localPlayerIndex >= 0 && localPlayerIndex < objetosControlables.Count)
        {
            GameObject objetoAsignado = objetosControlables[localPlayerIndex];
            PhotonView photonView = objetoAsignado.GetComponent<PhotonView>();

            // Transferir la propiedad del objeto al LocalPlayer
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer);

            temp = objetoAsignado;
            Debug.Log($"Objeto asignado: " +
                $"Player <color=yellow>{PhotonNetwork.LocalPlayer.NickName} / {(int)PhotonNetwork.LocalPlayer.CustomProperties[Constantes.PlayerKey_CustomID]}</color> -> <color=green>{objetoAsignado.name}</color> ");
        }
        else
        {
            Debug.LogWarning($"No hay un objeto controlable asignado para el LocalPlayer: {PhotonNetwork.LocalPlayer.NickName} / {(int)PhotonNetwork.LocalPlayer.CustomProperties[Constantes.PlayerKey_CustomID]}");
        }

        return temp;
    }
}
