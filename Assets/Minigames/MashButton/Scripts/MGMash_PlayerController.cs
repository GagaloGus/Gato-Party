using Photon.Pun;
using ExitGames.Client.Photon;

using TMPro;
using UnityEngine;

public class MGMash_PlayerController : MonoBehaviour
{
    public bool canMove;
    public int score;
    Animator m_animator;
    KeyCode push;

    [Header("Photon Stuff")]
    public GameObject Mark;
    public GameObject nicknameHolder;
    PhotonView photonView;

    private void Awake()
    {
        score = -1;
        canMove = false;
        photonView = GetComponent<PhotonView>();
        m_animator = transform.Find("3dmodel Sko").gameObject.GetComponent<Animator>();

        push = PlayerKeybinds.push_mashMG;
    }
    // Start is called before the first frame update
    void Start()
    {
        //Resetea la puntuacion
        Hashtable playerProps = new Hashtable
        {
            [Constantes.PlayerKey_MinigameScore] = -1
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

        Mark.SetActive(photonView.IsMine);
        /*if (!photonView.IsMine)
        {
            nicknameHolder.SetActive(true);
            nicknameHolder.GetComponentInChildren<TMP_Text>().text = photonView.Controller.NickName;
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        //Si nos podemos mover y somos el player
        if(canMove)
        {
            if (Input.GetKeyDown(push))
            {
                m_animator.SetBool("push", true);
                score += (score == -1 ? 2 : 1);
            }

            if (Input.GetKeyUp(push))
            {
                m_animator.SetBool("push", false);
            }
        }
    }

    //Al acabar el minijuego, llamado desde el manager
    public void MinigameFinished()
    {
        canMove = false; //quieto
        int newScore = (score == -1 ? 0 : score); //Si no hicimos nada, nos da 0, si no nos da nuestra puntuacion

        //Cambia las propiedades del Player
        Hashtable playerProps = new Hashtable
        {
            [Constantes.PlayerKey_MinigameScore] = newScore
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
    }
}
