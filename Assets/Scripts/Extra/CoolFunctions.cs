using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoolFunctions
{
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

    public static Vector3 VectorMoveAlongTransformAxis(Vector3 v1, Transform axis)
    {
        Vector3 newVector = v1.x * axis.right + v1.y * axis.up + v1.z * axis.forward;

        return newVector;
    }

    public static void Invoke(this MonoBehaviour mb, Action f, float delay)
    {
        mb.StartCoroutine(InvokeRoutine(f, delay));
    }

    private static IEnumerator InvokeRoutine(System.Action f, float delay)
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
}

