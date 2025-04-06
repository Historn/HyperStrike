using System.Drawing;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
//using Unity.Cinemachine;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;

    #region "Model-View Player"
    private Player player;
    private PlayerView view;
    #endregion

    #region "Movement Inputs"
    InputAction moveAction;
    InputAction lookAction;
    InputAction jumpAction;
    InputAction attackAction;
    InputAction interactAction;
    InputAction crouchAction;
    InputAction sprintAction;
    InputAction previousAction;
    InputAction nextAction;
    #endregion

    #region "Movement Variables"
    float movementSpeed = 20.0f;
    float sprintSpeed = 30.0f;

    // Jump Vars
    bool readyToJump;
    float jumpCooldown = 0.0f;
    float jumpForce = 10.0f;

    // Ground Vars
    [Header("Ground Check")]
    float characterHeight; // Change to character Data
    public LayerMask groundMask;
    [SerializeField] bool isGrounded;
    float groundDrag = 0.25f;

    // Wall run
    [SerializeField] bool isWallRunning;
    RaycastHit wallHit;
    float angleRoll = 25.0f; // Var to rotate camera while wallrunning

    #endregion

    private void Start()
    {
        // Init Player MVC
        player = new Player();
        view = GetComponent<PlayerView>();

        // Init Physics variables
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.maxLinearVelocity = 15.0f;

        // Init Inputs
        InitInputs();

        // Init Movement variables
        readyToJump = true;

        characterHeight = GetComponent<CapsuleCollider>().height;
        isWallRunning = false;
        isGrounded = false;

        view.UpdateView(player);
    }

    private void Update()
    {
        //Show Leaderboard
        //if (UnityEngine.Input.GetKey(showLeaderboard))
        //    leaderboardPanel.SetActive(true);
        //else
        //    leaderboardPanel.SetActive(false);
    }

    // Physics-based + Rigidbody Actions
    private void FixedUpdate()
    {
        //Ground Check
        float dist = characterHeight * 0.5f + 0.1f;
        Vector3 endRayPos = new Vector3(transform.position.x, transform.position.y - dist, transform.position.z);
        Debug.DrawLine(transform.position, endRayPos, UnityEngine.Color.red);
        isGrounded = Physics.Raycast(transform.position, Vector3.down, dist);

        // Move
        WalkAndRun();

        WallRun();

        // Crouch and Slide
        CrouchSlide();

        if (isGrounded) Jump(Vector3.zero);

        //Handle Drag with the ground after all movement inputs
        HandleDrag();
    }

    void InitInputs()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        jumpAction = InputSystem.actions.FindAction("Jump");
        attackAction = InputSystem.actions.FindAction("Attack");
        interactAction = InputSystem.actions.FindAction("Interact");
        crouchAction = InputSystem.actions.FindAction("Crouch");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        previousAction = InputSystem.actions.FindAction("Previous");
        nextAction = InputSystem.actions.FindAction("Next");
    }

    void DebugMovement()
    {
        return;
    }

    #region "Movement Mechanics Methods"
    void WalkAndRun()
    {
        // Sprint
        float speed = movementSpeed;
        if (sprintAction.IsPressed() && isGrounded) speed = sprintSpeed;
        //Debug.Log(speed);
        //Debug.Log(rb.linearVelocity.magnitude);

        Vector2 moveValue = moveAction.ReadValue<Vector2>(); // Gets Input Values
        Vector3 dir = new Vector3(moveValue.x, 0.0f, moveValue.y);
        rb.AddForce(dir.normalized * speed, ForceMode.Force);
    }

    void CrouchSlide()
    {
        // Set scale to (transform.position.y * 0.5f)
        // If running + crouch --> Slide --> Higher velocity? or less drag
        if (crouchAction.IsPressed())
        {
            Vector3 crouchedScale = Vector3.one * 0.5f;
            transform.localScale = crouchedScale;
            //Activate Crouch Anim


            if (sprintAction.IsPressed())
            {
                // Activate Sliding Anim
                // Higher speed
            }
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }

    void Jump(Vector3 jumpDir)
    {
        if (jumpAction.IsPressed() && readyToJump)
        {
            readyToJump = false;

            //Reset Y Velocity
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce((transform.up + jumpDir) * jumpForce, ForceMode.Impulse);

            Invoke(nameof(ResetJump), jumpCooldown);    //Delay for jump to reset
        }
    }

    void ResetJump()
    {
        readyToJump = true;
    }

    void WallRun()
    {
        Vector3[] directions = new Vector3[]
            {
            Vector3.right,
            Vector3.right + Vector3.forward,
            Vector3.forward,
            Vector3.left + Vector3.forward,
            Vector3.left
            };

        for (int i = 0; i < directions.Length; i++)
        {
            Debug.DrawLine(transform.position, transform.position + directions[i], UnityEngine.Color.green);
            isWallRunning = Physics.Raycast(transform.position, directions[i], out wallHit, transform.localScale.x + 0.15f);
            if (!isGrounded && isWallRunning && moveAction.IsPressed())
            {
                Physics.gravity = Physics.gravity / 3.0f; // Reduce gravity to stay more time in the wall but not infinite
                rb.AddForce(Vector3.forward * sprintSpeed, ForceMode.Force);
                Jump(wallHit.normal);
            }
        }

        // Rotate camera a bit on the z-axis
    }

    void HandleDrag()
    {
        if (isGrounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }
    #endregion

    #region "Data Methods"
    public void IncreaseScore(int amount)
    {
        player.Score += amount;
        view.UpdateView(player);
    }
    #endregion
}
