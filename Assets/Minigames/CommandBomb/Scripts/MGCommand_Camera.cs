using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.GridBrushBase;

public class MGCommand_Camera : MonoBehaviour
{
    public List<GameObject> PlayerObjects = new List<GameObject>();
    public float duration;
    [Range(0,3)] public int temp;

    public Vector3 margin;
    // Start is called before the first frame update
    void Start()
    {
        PlayerObjects.Clear();

        //Nada mas empezar agarra la lista de los players de otro script que esta siempre activo
        PlayerObjects = FindObjectOfType<MGCommand_Manager>().PlayerObjects;
    }

    public void RotateTowardsPlayer(int targetPlayer, float duration)
    {
        Transform targetTransform = PlayerObjects[targetPlayer].transform;
        Vector3 offset = CoolFunctions.MoveAlongAxis(targetTransform, margin);

        StartCoroutine(RotateTowardsCoroutine(targetTransform.position + offset, duration));
    }

    IEnumerator RotateTowardsCoroutine(Vector3 targetPos, float duration)
    {
        float elapsedTime = 0f;

        Quaternion startRotation = transform.rotation;
        Quaternion endRotation1 = Quaternion.LookRotation(targetPos - transform.position);

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            transform.rotation = Quaternion.Slerp(startRotation, endRotation1, t);
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        // Asegúrate de que la rotación final sea exactamente la dirección del primer objetivo
        transform.rotation = endRotation1;
    }

    private void OnDrawGizmos()
    {
        Transform targetTransform = PlayerObjects[temp].transform;
        Vector3 offset = targetTransform.right * margin.x + targetTransform.up * margin.y + targetTransform.forward * margin.z;

        Gizmos.color = Color.blue;
        Gizmos.DrawCube(targetTransform.position + offset, Vector3.one/5);
    }
}
