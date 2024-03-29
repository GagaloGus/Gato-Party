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

    [Header("Random Position to spawn")]
    public Vector3 minPositions;
    public Vector3 maxPositions;

    AnimationBundles animationBundles;
    PhotonView photonView;
    private void Awake()
    {
        animationBundles = FindObjectOfType<AnimationBundles>();
        photonView = GetComponent<PhotonView>();
    }

    // Start is called before the first frame update
    void Start()
    {
        SalaEsperaSettings salaEsperaSettings = FindObjectOfType<SalaEsperaSettings>();

        FindObjectOfType<MinigameSelector>(true).loadingScreen.SetActive(true);

        //Instancia el player
        GameObject playerObj = 
            PhotonNetwork.Instantiate(
            prefabName: objectToSpawn.name, 
            position: new Vector3(
                Random.Range(minPositions.x, maxPositions.x),
                Random.Range(minPositions.y, maxPositions.y),
                Random.Range(minPositions.z, maxPositions.z)), 
            rotation: Quaternion.identity);

        //Le asigna temporalmente el -1 de skin
        Hashtable playerProps = new Hashtable
        {
            [Constantes.PlayerKey_Skin] = -1
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
            SE_PlayerController[] players = FindObjectsOfType<SE_PlayerController>();
            foreach (SE_PlayerController player in players)
            {
                PhotonView otherPhotonView = player.gameObject.GetComponent<PhotonView>();
                int otherSkinID = (int)otherPhotonView.Owner.CustomProperties[Constantes.PlayerKey_Skin];

                salaEsperaSettings.LoadTexturePacks(otherPhotonView.ViewID, otherSkinID);
            }
        }, 0.1f);

        //desactiva la pantalla de carga
        CoolFunctions.Invoke(this, () =>
        {
            FindObjectOfType<MinigameSelector>(true).loadingScreen.SetActive(false);
        }, 0.5f);
    }
}
