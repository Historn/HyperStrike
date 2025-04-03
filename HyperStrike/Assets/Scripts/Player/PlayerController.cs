using System.Drawing;
using UnityEngine;
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
    float speed = 1.0f;

    // Jump Vars
    bool  readyToJump;
    float jumpCooldown = 0.0f;
    float jumpForce    = 10.0f;

    // Ground Vars
    [Header("Ground Check")]
    float characterHeight; // Change to character Data
    public LayerMask groundMask;
    [SerializeField] bool isGrounded;
    float groundDrag = 0.03f;
    #endregion

    private void Start()
    {
        // Init Player MVC
        player = new Player();
        view = GetComponent<PlayerView>();

        // Init Physics variables
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // Init Inputs
        InitInputs();

        // Init Movement variables
        readyToJump = true;

        characterHeight = GetComponent<CapsuleCollider>().height;
        isGrounded = false;
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
        Vector3 endRayPos = new Vector3(transform.position.x, characterHeight * 0.5f + 0.2f, transform.position.z);
        Debug.DrawLine(transform.position, endRayPos, UnityEngine.Color.red);
        //Debug.DrawLine(transform.position, new Vector3(0, 5, 0), UnityEngine.Color.red);
        isGrounded = Physics.Raycast(transform.position, Vector3.down, characterHeight * 0.5f + 0.2f);

        // Move
        Vector2 moveValue = moveAction.ReadValue<Vector2>(); // Gets Input Values
        rb.position += new Vector3(moveValue.x, 0.0f, moveValue.y) * speed * groundDrag;

        if (jumpAction.IsPressed() && readyToJump && isGrounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);    //Delay for jump to reset
        }

        //Handle Drag with the ground
        if (isGrounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    void InitInputs()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        jumpAction = InputSystem.actions.FindAction("Jump");
        attackAction = InputSystem.actions.FindAction("Move");
        interactAction = InputSystem.actions.FindAction("Look");
        crouchAction = InputSystem.actions.FindAction("Jump");
    }

    void DebugMovement()
    {
        return;
    }

    void Jump()
    {
        //Reset Y Velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    void ResetJump()
    {
        readyToJump = true;
    }

}
