using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MGMash_PlayerController : MonoBehaviour
{
    public bool canMove;

    Animator m_animator;

    KeyCode push;

    [Header("Photon Stuff")]
    public GameObject Mark;
    public GameObject nicknameHolder;
    PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        m_animator = transform.Find("3dmodel Sko").gameObject.GetComponent<Animator>();

        push = PlayerKeybinds.push_mashMG;
    }
    // Start is called before the first frame update
    void Start()
    {
        Mark.SetActive(photonView.IsMine);
        if (!photonView.IsMine)
        {
            nicknameHolder.SetActive(true);
            nicknameHolder.GetComponentInChildren<TMP_Text>().text = photonView.Controller.NickName;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
            return;

        if (Input.GetKeyDown(push))
        {
            m_animator.SetTrigger("push");
        }
    }
}
