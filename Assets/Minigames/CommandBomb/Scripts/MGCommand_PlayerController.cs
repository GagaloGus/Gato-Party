using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class MGCommand_PlayerController : MonoBehaviour
{
    Animator m_animator;
    enum ThrowStates { Recieve, Idle, Throw}
    // Start is called before the first frame update
    void Start()
    {
        m_animator = GetComponentInChildren<Animator>();
        m_animator.SetInteger("state", -1);
    }

    public void RecieveBomb()
    {
        m_animator.SetInteger("state", (int)ThrowStates.Recieve);
    }

    public void IdleBomb()
    {
        m_animator.SetInteger("state", (int)ThrowStates.Idle);
    }

    public void ThrowBomb()
    {
        m_animator.SetInteger("state", (int)ThrowStates.Throw);
    }

    public void Eliminated()
    {
        Debug.Log($"Me eliminaron :( {(int)PhotonNetwork.LocalPlayer.CustomProperties[Constantes.PlayerKey_CustomID]}");
        m_animator.Play("died");
    }
}
