using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FinalScore_PlayerAnimator : MonoBehaviour
{
    public int order;
    public int skinID;

    private Transform groundPoint;

    Animator m_animator;

    [Header("Debug Variables")]
    [Range(0f, 2f)] public float rayDetectFloorDist;
    private bool hasTouchedGround;

    [Header("Audios")]
    public AudioClip[] orderedSounds;

    public enum PlayerStates { Idle, JumpDown, Fall, Victory }
    public PlayerStates playerState;

    private void Awake()
    {
        hasTouchedGround = false;
        groundPoint = transform.Find("GroundCheckPoint");

        m_animator = transform.Find("3dmodel Sko").GetComponent<Animator>();
        m_animator.SetInteger("player states", (int)PlayerStates.JumpDown);
    }

    private void FixedUpdate()
    {
        if (hasTouchedGround)
            return; 

        Ray detectGround = new Ray(groundPoint.position, Vector3.down);

        if (Physics.Raycast(detectGround, out RaycastHit hit, rayDetectFloorDist, LayerMask.GetMask("Ground")))
        {
            hasTouchedGround = true;
            DeterminePlayerState();
        }
    }

    void DeterminePlayerState()
    {
        if (order == 1) // Es primero
        {
            playerState = PlayerStates.Victory;
        }
        else if (order == 4) // Es ultimo
        {
            playerState = PlayerStates.Fall;
        }
        else // Esta en el medio
        {
            playerState = PlayerStates.Idle;
        }

        AudioManager.instance.PlaySFX3D(orderedSounds[order-1], transform.position);

        // Se actualiza el estado del animador
        m_animator.SetInteger("player states", (int)playerState);
    }
}