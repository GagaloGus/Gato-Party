using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Spawner : MonoBehaviourPunCallbacks
{
    [Header("Prefab to spawn")]
    public GameObject objectToSpawn;
    public GameObject playerSpawned;

    [Header("Random Position to spawn")]
    public Vector3 minPositions;
    public Vector3 maxPositions;

    // Debe ser awake por que en el Skin Selector se ejecuta en el start
    void Awake()
    {
        SalaEsperaSettings salaEsperaSettings = FindObjectOfType<SalaEsperaSettings>();

        //Instancia el player
        GameObject playerObj = 
            PhotonNetwork.Instantiate(
            prefabName: objectToSpawn.name, 
            position: new Vector3(
                Random.Range(minPositions.x, maxPositions.x),
                Random.Range(minPositions.y, maxPositions.y),
                Random.Range(minPositions.z, maxPositions.z)), 
            rotation: Quaternion.identity);

        playerSpawned = playerObj;

        Hashtable playerProps = new Hashtable
        {
            //Le asigna temporalmente el -1 de skin
            [Constantes.PlayerKey_Skin] = -1,

            //ID de identificacion
            [Constantes.PlayerKey_CustomID] = PhotonNetwork.IsMasterClient ? 1 : PhotonNetwork.CurrentRoom.PlayerCount,

            //No esta listo
            [Constantes.PlayerKey_Ready_SalaEspera] = false,

            //Reseteo de las puntuaciones
            [Constantes.PlayerKey_TotalScore] = 0,
            [Constantes.PlayerKey_MinigameScore] = 0
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        //Coje la skin de ID mas bajo disponible
        int SkinID = FindObjectOfType<SkinSelector>(true).GetSkinIDAvaliable();

        //Coje el id del player instanciado y su SkinID y lo manda a todos los jugadores
        salaEsperaSettings.UpdatePlayerSkin(playerObj.GetComponent<PhotonView>().ViewID, SkinID);

        //Le asigna ahora el valor correcto
        Hashtable newPlayerProps = new Hashtable
        {
            [Constantes.PlayerKey_Skin] = SkinID
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(newPlayerProps);

        //Ligero delay para que todo este iniciado y cargar las texturas correctamente
        CoolFunctions.Invoke(this, () =>
        {
            CoolFunctions.LoadAllTexturePacks<SE_PlayerController>();
        }, 0.1f);

        //desactiva la pantalla de carga
        CoolFunctions.Invoke(this, () =>
        {
            FindObjectOfType<MinigameSelector>(true).loadingScreen.SetActive(false);
        }, 0.5f);
    }

    public void Respawn()
    {
        playerSpawned.GetComponent<Rigidbody>().velocity = Vector3.zero;

        playerSpawned.transform.position = new Vector3(
                Random.Range(minPositions.x, maxPositions.x),
                Random.Range(minPositions.y, maxPositions.y),
                Random.Range(minPositions.z, maxPositions.z));
    }
}
