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

    public static string StringContentOfList<T>(List<T> list, bool saltoDeLinea)
    {
        string content = "";
        foreach (T item in list)
        {
            content += item.ToString();

            if(saltoDeLinea)
            {
                content += "\n";
            }
            else
            {
                content += ", ";
            }
        }

        return content;
    }

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

    //Actualiza las texturas de el propio personaje y de todos los demas jugadores
    //Esta funcion se debe llamar al inicio para cargar las texturas de todos los jugadores
    public static void LoadAllTexturePacks<T>() where T : MonoBehaviour
    {
        //Coje todos los jugadores con un componente en especifico
        T[] players = UnityEngine.Object.FindObjectsOfType<T>();
        foreach (T player in players)
        {
            //Coge el PhotonView y el ID de la skin de cada player
            PhotonView otherPhotonView = player.gameObject.GetComponent<PhotonView>();
            int otherSkinID = (int)otherPhotonView.Owner.CustomProperties[Constantes.PlayerKey_Skin];

            //Encuentra el script que contiene todos los paquetes de texturas
            AnimationBundles animationBundles = UnityEngine.Object.FindObjectOfType<AnimationBundles>();

            //Coge el script del jugador que se encarga de el cambio de sprites
            ChangeTextureAnimEvent textureScript = player.GetComponentInChildren<ChangeTextureAnimEvent>();

            //Busca el paquete de texturas acorde al ID del jugador
            AnimationSpriteBundle selectedBundle = Array.Find(animationBundles.bundles.ToArray(), x => x.ID == otherSkinID);

            try
            {
                //Establece el ID del script y actualiza su diccionario
                textureScript.UpdateAnimationDictionary(selectedBundle.texturePacks, otherSkinID);
                Debug.Log($"Loaded sprites of <color=cyan>{otherPhotonView.Owner.NickName}</color>, skin id: {otherSkinID} <color=yellow>({selectedBundle.Name})</color> -> {player.name}");
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                //Si algo falla, le pone la skin default

                textureScript.UpdateAnimationDictionary(animationBundles.bundles[0].texturePacks, 0);
            }
        }
    }

    public static List<T> GetFilteredObjects<T>(params VariableValuePair[] filters) where T : MonoBehaviour
    {
        List<T> AllObjects =  UnityEngine.Object.FindObjectsOfType<T>().ToList();

        List<T> filteredObjects = new List<T>();

        if (filters != null)
        {
            foreach (T obj in AllObjects)
            {
                Debug.Log("------------------------------------------------------");
                bool passFilter = true;

                foreach (VariableValuePair filter in filters)
                {
                    object value = GetFieldValue(obj, filter.Name);

                    Debug.Log($"({value.GetType()}){value} // {filter.Value}({filter.Value.GetType()})");

                    if (value == null)
                    {
                        Debug.Log($"No existe la variable {filter.Name}");
                        break;
                    }

                    if (!value.Equals(filter.Value))
                    {
                        Debug.Log($"<color=red>{obj.GetType().Name} no paso el filtro {filter.Name} ({filter.Value} != {value})</color>");
                        passFilter = false;
                        break;
                    }
                }

                if (passFilter)
                {
                    Debug.Log($"<color=green>{obj.GetType().Name} paso los filtros!</color>");
                    filteredObjects.Add(obj);
                }
            }

            Debug.Log($"<color=cyan>Se devolvio una lista de {filteredObjects.Count} {typeof(T).Name}(s)</color>");
            return filteredObjects;
        }
        else
        {
            return AllObjects;
        }
    }

    static object GetFieldValue<T>(T obj, string fieldName)
    {
        var field = typeof(T).GetField(fieldName);
        if (field != null)
        {
            Debug.Log($"Field {field} found! -> {field.GetValue(obj)}");
            return field.GetValue(obj);
        }
        else
        {
            Debug.LogError($"Field {fieldName} not found in {typeof(T).Name} object.");
            return null;
        }
    }
}

public class VariableValuePair
{
    public string Name;
    public object Value;

    public VariableValuePair(string name, object value)
    {
        Name = name;
        Value = value;
    }
}
