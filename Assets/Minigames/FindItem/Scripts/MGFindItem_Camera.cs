using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGFindItem_Camera : MonoBehaviour
{
    [Header("Transforms")]
    public Vector3 offsetPosition;
    public Vector3 offsetRotation;
    Vector3 originalPosition;
    Vector3 originalRotation;

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation.eulerAngles;
    }

    public void MoveRotateTowards(Vector3 objectPosition, float duration)
    {
        StartCoroutine(MoveRotateTowardsCorroutine(objectPosition, duration));
    }

    public void ResetPosRotOriginal()
    {
        transform.position = originalPosition;
        transform.rotation = Quaternion.Euler(originalRotation);
    }

    IEnumerator MoveRotateTowardsCorroutine(Vector3 objectPos, float duration)
    {
        float elapsedTime = 0f;
        Vector3 targetPosition = objectPos + offsetPosition;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            transform.position = Vector3.Lerp(originalPosition, targetPosition, t); // Movimiento
            transform.rotation = Quaternion.Slerp(Quaternion.Euler(originalRotation), Quaternion.Euler(offsetRotation), t); //Rotacion

            yield return null;
            elapsedTime += Time.deltaTime;
        }

        // Asegurar de que la posicion y rotacion final sea exacta
        transform.position = targetPosition;
        transform.rotation = Quaternion.Euler(offsetRotation);
    }
}
