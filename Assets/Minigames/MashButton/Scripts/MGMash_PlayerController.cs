using Photon.Pun;
using ExitGames.Client.Photon;

using TMPro;
using UnityEngine;

public class MGMash_PlayerController : MonoBehaviour
{
    public bool canMove;
    public int score;
    Animator m_animator;

    private void Awake()
    {
        score = 0;
        canMove = false;
        m_animator = transform.Find("3dmodel Sko").gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //No hace falta chekear si el photon view es nuestro, solo se activa el canMove del player local desde el manager

        //Si nos podemos mover y somos el player
        if (canMove)
        {
            if (Input.GetKeyDown(PlayerKeybinds.push_mashMG))
            {
                m_animator.SetBool("push", true);
                score++;
            }

            if (Input.GetKeyUp(PlayerKeybinds.push_mashMG))
            {
                m_animator.SetBool("push", false);
            }
        }
    }

    //Al acabar el minijuego, llamado desde el manager
    public void MinigameFinished()
    {
        canMove = false; //quieto //Si no hicimos nada, nos da 0, si no nos da nuestra puntuacion

        //Cambia las propiedades del Player
        Hashtable playerProps = new Hashtable
        {
            [Constantes.PlayerKey_MinigameScore] = score
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
    }
}
