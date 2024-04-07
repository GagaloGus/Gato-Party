using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CoolFunctions
{
    #region Math
    public static float MapValues(float value, float leftMin, float leftMax, float rightMin, float rightMax)
    {
        return rightMin + (value - leftMin) * (rightMax - rightMin) / (leftMax - leftMin);
    }

    public static Vector3 FlattenVector3(Vector3 value, float newYValue = 0)
    {
        value.y = newYValue;
        return value;
    }

    public static bool IsRightOfVector(Vector3 center, Vector3 direction, Vector3 target)
    {
        Vector3 vectorB = center + direction;

        float result = (target.x - center.x) * (vectorB.z - center.z) - (target.z - center.z) * (vectorB.x - center.x);

        return result >= 0;
    }

    public static Vector3 MultipyVectorValues(Vector3 v1, Vector3 v2)
    {
        Vector3 newVector = new Vector3(
            v1.x * v2.x,
            v1.y * v2.y,
            v1.z * v2.z
            );
        
        return newVector;
    }

    public static Vector3 MoveAlongAxis(Transform axis, Vector3 margin)
    {
        return axis.right * margin.x + axis.up * margin.y + axis.forward * margin.z;
    }
    #endregion

    public static void Invoke(this MonoBehaviour mb, Action f, float delay)
    {
        mb.StartCoroutine(InvokeRoutine(f, delay));
    }

    private static System.Collections.IEnumerator InvokeRoutine(System.Action f, float delay)
    {
        yield return new WaitForSeconds(delay);
        f();
    }

    public static List<T> ShuffleList<T>(List<T> lista)
    {
        int n = lista.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T valor = lista[k];
            lista[k] = lista[n];
            lista[n] = valor;
        }

        return lista;
    }

    public static bool SearchRoomByName(string roomName, List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            if (room.Name == roomName) { return true; }
        }

        return false;
    }

    //Seguramente habra que cambiarlo para que tambien devuelva players, por ahora funciona
    public static List<int> GetAllPlayerSkinIDs()
    {
        List<int> usingIDs = new List<int>();

        Hashtable roomProps = PhotonNetwork.CurrentRoom.CustomProperties;
        foreach(System.Collections.DictionaryEntry entry in roomProps)
        {
            if((string)entry.Key == Constantes.SkinIDOrder_Room)
            {
                int[] temp = (int[])entry.Value;
                usingIDs = temp.ToList();
                break;
            }
        }

        return usingIDs;
    }

    //Actualiza las texturas de el propio personaje y de todos los demas players
    //Esta funcion se debe llamar al inicio de cada minijuego para cargar las texturas, no en la Sala Espera
    public static void LoadAllTexturePacks<T>() where T : MonoBehaviour
    {
        T[] players = UnityEngine.Object.FindObjectsOfType<T>();
        foreach (T player in players)
        {
            PhotonView otherPhotonView = player.gameObject.GetComponent<PhotonView>();
            int otherSkinID = (int)otherPhotonView.Owner.CustomProperties[Constantes.PlayerKey_Skin];

            LoadPacks(otherPhotonView.ViewID, otherSkinID);
        }
    }

    static void LoadPacks(int photonViewID, int SkinID)
    {
        AnimationBundles animationBundles = UnityEngine.Object.FindObjectOfType<AnimationBundles>();

        //Encuentra el gameobject del player segun su PhotonView ID
        GameObject player = PhotonView.Find(photonViewID).gameObject;
        ChangeTextureAnimEvent textureScript = player.GetComponentInChildren<ChangeTextureAnimEvent>();

        AnimationSpriteBundle selectedBundle = System.Array.Find(animationBundles.bundles.ToArray(), x => x.ID == SkinID);

        try
        {
            textureScript.ID = SkinID;
            textureScript.UpdateAnimationDictionary(selectedBundle.texturePacks);
            Debug.Log($"Loaded sprites of <color=cyan>{PhotonView.Find(photonViewID).Owner.NickName}</color>, skin id: {SkinID} <color=yellow>({selectedBundle.Name})</color> -> {player.name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
            //Si algo falla, le pone la skin default
            textureScript.texturePacks = animationBundles.bundles[0].texturePacks;
            textureScript.ID = 0;
        }
    }
}

