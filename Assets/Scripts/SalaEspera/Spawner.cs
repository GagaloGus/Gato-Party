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
        int SkinID = FindObjectOfType<SkinSelector>().GetSkinIDAvaliable();
        print(SkinID);

        //Coje el id del player instanciado y su SkinID y lo manda a todos los jugadores
        object[] parameters = { playerObj.GetComponent<PhotonView>().ViewID, SkinID };
        photonView.RPC(nameof(LoadTexturePacks), RpcTarget.All, parameters);

        //Le asigna ahora el valor correcto
        Hashtable newPlayerProps = new Hashtable
        {
            [Constantes.PlayerKey_Skin] = SkinID
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(newPlayerProps);

        CoolFunctions.Invoke(this, () =>
        {
            SE_PlayerController[] players = FindObjectsOfType<SE_PlayerController>();
            foreach (SE_PlayerController player in players)
            {
                PhotonView otherPhotonView = player.gameObject.GetComponent<PhotonView>();
                print($"<color=cyan>{otherPhotonView.Owner.NickName}</color>");
                int otherSkinID = (int)otherPhotonView.Owner.CustomProperties[Constantes.PlayerKey_Skin];

                print($"{otherPhotonView.Owner.NickName} -> {otherSkinID}");

                LoadTexturePacks(otherPhotonView.ViewID, otherSkinID);
            }
        }, 0.2f);
        
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
    }

    [PunRPC]
    void LoadTexturePacks(int photonViewID, int SkinID)
    {
        GameObject player = PhotonView.Find(photonViewID).gameObject;
        ChangeTextureAnimEvent textureScript = player.GetComponentInChildren<ChangeTextureAnimEvent>();

        AnimationSpriteBundle selectedBundle = System.Array.Find(animationBundles.bundles.ToArray(), x => x.ID == SkinID);

        try
        {
            textureScript.texturePacks = selectedBundle.texturePacks;
            textureScript.ID = SkinID;
            print($"Loaded sprite of {PhotonView.Find(photonViewID).Owner.NickName}, skin id: {SkinID} -> {player.name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message, this);
            //Si algo falla, le pone la skin default
            textureScript.texturePacks = animationBundles.bundles[0].texturePacks;
            textureScript.ID = 0;
        }
    }
}
