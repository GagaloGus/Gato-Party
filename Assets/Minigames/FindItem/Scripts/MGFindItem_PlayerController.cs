using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MGFindItem_PlayerController : MonoBehaviour
{
    Vector3 originalPos;
    Quaternion originalRotation;

    public Vector3 chestOffset;
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        originalPos = transform.position;
        originalRotation = transform.rotation;

        animator = GetComponentInChildren<Animator>();
        animator.SetInteger("state", 0);
    }

    public void Walk()
    {
        animator.SetInteger("state", 1);
    }

    public void OpenChest(int chestReward)
    {
        animator.SetInteger("state", 2);
        animator.SetInteger("chestRew", chestReward);
    }

    public void ResetPosRot()
    {
        transform.position = originalPos;
        transform.rotation = originalRotation;

        animator.SetInteger("state", 0);
    }
}
