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
        if(canMove && photonView.IsMine)
        {
            if (Input.GetKeyDown(push))
            {
                m_animator.SetTrigger("push");
                score+= (score == -1 ? 2 : 1);
            }
        }
    }

    public void MinigameFinished()
    {
        canMove = false;
        int newScore = (score == -1 ? 0 : score);

        Hashtable playerProps = new Hashtable();
        playerProps[Constantes.PlayerKey_MinigameScore] = newScore;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
    }
}
