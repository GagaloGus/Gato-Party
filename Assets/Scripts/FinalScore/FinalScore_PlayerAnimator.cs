using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FinalScore_PlayerAnimator : MonoBehaviour
{
    public int order;

    private Transform groundPoint;

    Animator m_animator;

    [Header("Debug Variables")]
    [Range(0f, 2f)] public float rayDetectFloorDist;
    private bool isGrounded;

    public enum PlayerStates { Idle, JumpDown, Fall, Victory }
    public PlayerStates playerState;

    PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();

        groundPoint = transform.Find("GroundCheckPoint");

        m_animator = transform.Find("3dmodel Sko").GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
            return;

        //distintas configuraciones para cuando esta en el suelo y en el aire
        if (isGrounded) { GroundControl(); }
        else { AirControl(); }

        m_animator.SetInteger("player states", (int)playerState);
    }
    private void FixedUpdate()
    {
        if (!photonView.IsMine)
            return;

        Ray detectGround = new Ray(groundPoint.position, Vector3.down);

        if (Physics.Raycast(detectGround, out RaycastHit hit, rayDetectFloorDist, LayerMask.GetMask("Ground")))
        {
            isGrounded = true;
        }
        else { isGrounded = false; }
    }

    void GroundControl()
    {
        if(order == 1) //Es primero
        {
            playerState = PlayerStates.Victory;
        }
        else if(order == 4) //Es ultimo
        {
            playerState = PlayerStates.Fall;
        }
        else //Esta en el medio
        {
            playerState = PlayerStates.Idle;
        }
    }

    void AirControl()
    {
        playerState = PlayerStates.JumpDown;
    }

}