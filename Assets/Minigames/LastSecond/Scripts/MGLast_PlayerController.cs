using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGLast_PlayerController : MonoBehaviour
{
    enum GameStates { LookUp, IdleScared, Pressed, Bonked}

    Animator m_animator;

    private void Start()
    {
        m_animator = GetComponentInChildren<Animator>();
    }

    public void LookUp()
    {
        m_animator.SetInteger("state", (int)GameStates.LookUp);
    }

    public void StartGame()
    {
        m_animator.SetInteger("state", (int)GameStates.IdleScared);
    }

    public void PressedButton()
    {
        m_animator.SetInteger("state", (int)GameStates.Pressed);
    }

    public void Bonked()
    {
        m_animator.SetInteger("state", (int)GameStates.Bonked);
    }
}
