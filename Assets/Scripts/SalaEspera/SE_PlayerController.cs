using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SE_PlayerController : MonoBehaviour
{
    Rigidbody rb;
    Animator animator;
    Vector3 moveInput, moveDirection;
    Transform groundPoint;

    [Header("Movement")]
    public float moveSpeed;
    public float jumpForce;
    public float respawnYHeight;
    [Range(1, 5)] public float runSpeedMult;

    //variables del modelo 3d
    GameObject m_gameobj;
    Animator m_animator;

    [Header("Debug Variables")]
    [Range(0f, 2f)] public float rayDetectFloorDist;
    public float nearGroundDist;
    bool isGrounded, isFlipped, isFacingBackwards, canMove, isGliding, isRunning, isTyping;

    public enum PlayerStates { Idle, Walk, Run, JumpUp, JumpDown, Glide, Attack, Typing }
    public PlayerStates playerState;

    KeyCode jump, run;

    [Header("Photon Stuff")]
    public GameObject Mark;
    public GameObject Nickname;
    PhotonView photonView;
    TMP_InputField chat_inputField;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        photonView = GetComponent<PhotonView>();
        chat_inputField = FindObjectOfType<CanvasChat>().inputField;

        groundPoint = transform.Find("GroundCheckPoint");

        m_gameobj = transform.Find("3dmodel Sko").gameObject;
        m_animator = m_gameobj.GetComponent<Animator>();

        isFlipped = false;
        isFacingBackwards = false;
        canMove = true;

        //Mapeado de teclas
        jump = PlayerKeybinds.jump;
        run = PlayerKeybinds.run;
    }

    private void OnDrawGizmos()
    {
        groundPoint = transform.Find("GroundCheckPoint");
        Gizmos.DrawRay(groundPoint.position, Vector3.down * rayDetectFloorDist);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(groundPoint.position + Vector3.right/10, Vector3.down * nearGroundDist);
    }

    private void Start()
    {
        Mark.SetActive(photonView.IsMine);
        Nickname.SetActive(!photonView.IsMine);

        nearGroundDist = rayDetectFloorDist * jumpForce;

        if (!photonView.IsMine)
        {
            Nickname.GetComponentInChildren<TMP_Text>(true).text = photonView.Controller.NickName;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
            return;

        isTyping = chat_inputField.isFocused && isGrounded;

        if (isTyping)
        {
            playerState = PlayerStates.Typing;
            rb.velocity = Vector3.zero;
            m_animator.SetInteger("player states", (int)playerState);
            return;
        }

        if (transform.position.y < respawnYHeight)
            FindObjectOfType<Spawner>().Respawn();

        if (canMove)
        {
            //distintas configuraciones para cuando esta en el suelo y en el aire
            if (isGrounded) { GroundControl(); }
            else { AirControl(); }

        }

        #region Movement
        moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        //direccion hacia adelante de la camara
        moveDirection = CoolFunctions.FlattenVector3(Camera.main.transform.forward);

        isRunning = Input.GetKey(run);
        #endregion

        #region Flip
        //cambia la escala para imitar el "giro" del personaje
        if (canMove)
        {
            FlipCharacter();
        }

        #endregion


        animator.SetBool(nameof(isFlipped), isFlipped);
        animator.SetBool(nameof(isFacingBackwards), isFacingBackwards);
        m_animator.SetInteger("player states", (int)playerState);
    }
    private void FixedUpdate()
    {
        if (!photonView.IsMine || isTyping)
            return;

        Ray detectGround = new Ray(groundPoint.position, Vector3.down);

        isGrounded = Physics.Raycast(detectGround, out RaycastHit hit, rayDetectFloorDist, LayerMask.GetMask("Ground"));

        if (canMove)
        {
            Vector3 direction = (moveInput.x * Camera.main.transform.right + moveInput.z * moveDirection);
            float multiplySpeedFac = (float)(1 * (isRunning && isGrounded ? runSpeedMult : 1) * (isGliding ? runSpeedMult /1.5 : 1) + moveSpeed /15);

            Vector3 vel = direction * moveSpeed * multiplySpeedFac;
            //Moverse
            rb.velocity = (vel.magnitude < 1f ? rb.velocity : vel + Vector3.up * rb.velocity.y);

            transform.forward = CoolFunctions.FlattenVector3(Camera.main.transform.forward);
        }
    }

    #region movement functions
    void FlipCharacter()
    {
        if ((!isFlipped && moveInput.x < 0) || (isFlipped && moveInput.x > 0))
        {
            isFlipped = !isFlipped;
        }

        if ((!isFacingBackwards && moveInput.z < 0) || (isFacingBackwards && moveInput.z > 0))
        {
            isFacingBackwards = !isFacingBackwards;
        }
    }

    void GroundControl()
    {
        rb.drag = 0;
        isGliding = false;

        //Basicamente si estamos dandole a alguna tecla para moverse
        if(moveInput.magnitude > 0.1)
        {
            if(isRunning) { playerState = PlayerStates.Run; }
            else { playerState = PlayerStates.Walk; }
        }
        else
        {
            playerState = PlayerStates.Idle;
        }

        //Saltar
        if (Input.GetKeyDown(jump))
        { 
            rb.velocity += new Vector3(0, jumpForce, 0);
        }
    }

    void AirControl()
    {
        //raycast que detecta si hay suelo a tanta distancia de nosotros hacia abajo
        bool nearGround = Physics.Raycast(transform.position, Vector3.down, nearGroundDist, LayerMask.GetMask("Ground"));
            //si estamos planeando
        if (isGliding)
        {
            //fisicas
            rb.drag = 15;

            playerState = PlayerStates.Glide;

            if (Input.GetKeyDown(jump)) { isGliding = false; }
        }
        else
        {
            //fisicas 2
            rb.drag = 0;

            if (rb.velocity.y > 0f) { playerState = PlayerStates.JumpUp; }
            else if (rb.velocity.y < 0f) { playerState = PlayerStates.JumpDown; }

            //si le damos al espacio y el raycast no detecto un suelo debajo del player podemos planear
            if(Input.GetKeyDown(jump) && !nearGround) { isGliding = true; }
        }
    }

    public bool player_canMove
    {
        get { return canMove; }
        set { canMove = value; }
    }
    public bool player_isGrounded
    {
        get { return isGrounded; }
    }

    #endregion

}
