using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MGCommand_Bomb : MonoBehaviour
{
    [Header("Positions")]
    Vector3 endPoint;
    public List<GameObject> PlayerObjects = new List<GameObject>();

    [Header("Movement")]
    public float timeToReachDestination = 3f;
    public float maxHeight = 2f; // Altura m�xima de la par�bola
    public Vector3 margin;

    [Header("Rotation")]
    public Vector3 rotationDirection = Vector3.up;

    public void Start()
    {
        PlayerObjects.Clear();

        //Nada mas empezar agarra la lista de los players de otro script que esta siempre activo
        PlayerObjects = FindObjectOfType<MGCommand_Manager>().PlayerObjects;
    }

    public void ThrowBomb(int targetPlayer, float time)
    {
        Transform targetTransform = PlayerObjects[targetPlayer].transform;
        endPoint = targetTransform.position + CoolFunctions.MoveAlongAxis(targetTransform, margin);
        timeToReachDestination = time;

        StartCoroutine(nameof(MoveObjectCoroutine));
    }

    IEnumerator MoveObjectCoroutine()
    {
        float elapsedTime = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = endPoint;

        while (elapsedTime < timeToReachDestination)
        {
            float t = elapsedTime / timeToReachDestination;
            float height = Mathf.Sin(t * Mathf.PI) * maxHeight; // Funcion seno para la parabola
            Vector3 newPos = Vector3.Lerp(startPos, endPos, t) + Vector3.up * height; // A�ade la altura en Y
            transform.position = newPos;

            // Rota
            float rotationAmount = 360 * Time.deltaTime;
            transform.Rotate(rotationDirection.normalized, rotationAmount);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.right*rotationDirection.x);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.up * rotationDirection.y);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, Vector3.forward * rotationDirection.z);
    }
}
