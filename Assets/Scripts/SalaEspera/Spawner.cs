using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Prefab to spawn")]
    public GameObject objectToSpawn;

    [Header("Random Position to spawn")]
    public Vector3 minPositions;
    public Vector3 maxPositions;

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.Instantiate(
            prefabName: objectToSpawn.name, 
            position: new Vector3(
                Random.Range(minPositions.x, maxPositions.x),
                Random.Range(minPositions.y, maxPositions.y),
                Random.Range(minPositions.z, maxPositions.z)), 
            rotation: Quaternion.identity);
    }
}
