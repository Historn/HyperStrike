using HyperStrike;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Timeline;

// PLAYER STATE????

public class PlayerController : NetworkBehaviour
{
    Rigidbody rb;

    #region "Model-View Player"
    private Player player;
    private PlayerView view;
    public Character characterData;
    #endregion

    #region "Movement Inputs"
    InputAction moveAction;
    InputAction lookAction;
    InputAction jumpAction;
    InputAction attackAction;
    InputAction ability1Action;
    InputAction ability2Action;
    InputAction interactAction;
    InputAction slideAction;
    InputAction sprintAction;
    InputAction previousAction;
    InputAction nextAction;
    #endregion

    #region "Movement Variables"
    [Header("Cinemachine Settings")]
    public CinemachineCamera cinemachineCamera; // Reference to the Cinemachine virtual camera
    [SerializeField] private Transform mainCameraTransform;
    [SerializeField] private Transform cameraWeaponTransform; // The transform of the Cinemachine camera's LookAt target
    float sensitivity = 5.0f;
    float xRotation;
    float yRotation;

    // Jump Vars
    bool readyToJump;
    float jumpCooldown = 0.0f;
    float jumpForce = 10.0f;

    // Ground Vars
    [Header("Ground Check")]
    [SerializeField] bool isGrounded;
    float characterHeight; // Change to character Data

    // Wall run
    [SerializeField] bool isWallRunning;
    RaycastHit wallHit;
    //float angleRoll = 25.0f; // Var to rotate camera while wallrunning

    Vector3 gravity;

    #endregion

    #region "Attack Variables"
    [Header("Shooting Variables")]
    public Transform projectileSpawnOffset;
    private bool shootReady;

    #endregion

    public override void OnNetworkSpawn()
    {
        if (IsClient && IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Init Inputs
            InitInputs();

            cinemachineCamera.Priority = 1;
        }
        else
        {
            cinemachineCamera.Priority = -1;
        }

        // Init Player MVC
        player = new Player();
        player.Character = characterData;
        view = GetComponent<PlayerView>();

        // Net Owner Only?
        player.Score = 0;
        view.UpdateView(player);

        // Init Physics variables
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.maxLinearVelocity = player.Character.maxSpeed;

        readyToJump = true;

        characterHeight = GetComponent<CapsuleCollider>().height;
        isGrounded = false;
        isWallRunning = false;

        // Init Attack Variables
        shootReady = true;

        gravity = Physics.gravity;
    }

    private void Start()
    {
        if (!IsOwner)
        {
            // Disable this camera if not owned by this client
            GetComponentInChildren<Camera>().enabled = false;
            GetComponentInChildren<AudioListener>().enabled = false;
            GetComponentInChildren<CinemachineBrain>().enabled = false;
        }
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
        // Set it to avoid flying bugs
        if (IsServer) Physics.gravity = gravity;

        //Ground Check
        isGrounded = GroundCheck.CheckGrounded(transform, characterHeight);

        if (IsClient && IsOwner)
        {
            if (cameraWeaponTransform != null)
            {
                RotatePlayerWithCameraServerRPC(lookAction.ReadValue<Vector2>());
            }

            // Move
            WalkAndRunServerRPC(moveAction.ReadValue<Vector2>(), sprintAction.IsPressed());

            WallRunServerRPC(moveAction.IsPressed(), jumpAction.IsPressed());

            SlideServerRPC(slideAction.IsPressed());

            // Jump
            if (isGrounded && !isWallRunning) JumpServerRPC(jumpAction.IsPressed(), Vector3.zero);
        } 
    }

    void InitInputs()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        jumpAction = InputSystem.actions.FindAction("Jump");

        attackAction = InputSystem.actions.FindAction("Attack");
        attackAction.started += _ => ShootServerRPC();

        //ability1Action = InputSystem.actions.FindAction("Ability1");
        //ability1Action.started += _ => ActivateAbility(player.Character.ability1);

        //ability2Action = InputSystem.actions.FindAction("Attack");
        //ability2Action.started += _ => ActivateAbility(player.Character.ability2);

        interactAction = InputSystem.actions.FindAction("Interact");
        slideAction = InputSystem.actions.FindAction("Crouch");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        previousAction = InputSystem.actions.FindAction("Previous");
        nextAction = InputSystem.actions.FindAction("Next");
    }

    #region "Movement Mechanics Methods"

    [ServerRpc]
    void RotatePlayerWithCameraServerRPC(Vector2 lookValue)
    {
        // Get mouse input
        float mouseX = lookValue.x * sensitivity * Time.fixedDeltaTime;
        float mouseY = lookValue.y * sensitivity * Time.fixedDeltaTime;

        // Adjust xRotation for vertical rotation and clamp it
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        yRotation += mouseX;

        // Update the Cinemachine camera's rotation
        cameraWeaponTransform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        mainCameraTransform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        // Apply horizontal rotation to the player
        rb.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    [ServerRpc]
    void WalkAndRunServerRPC(Vector2 moveValue, bool isSprinting)
    {
        // Sprint
        float speed = player.Character.speed;
        if (isSprinting && isGrounded) speed = player.Character.sprintSpeed;

        Vector3 dir = transform.forward * moveValue.y + transform.right * moveValue.x;
        if (!isWallRunning) rb.AddForce(dir.normalized * speed, ForceMode.Force);
    }

    [ServerRpc]
    void SlideServerRPC(bool isSliding)
    {
        if (isSliding)
        {
            rb.maxLinearVelocity = player.Character.maxSlidingSpeed;
            rb.linearDamping = 0.1f;
            transform.localScale = Vector3.one * 0.75f;
        }
        else
        {
            rb.maxLinearVelocity = player.Character.maxSpeed;
            rb.linearDamping = 0.2f;
            transform.localScale = Vector3.one;
        }
    }

    [ServerRpc]
    void JumpServerRPC(bool isJumping, Vector3 jumpDir)
    {
        if (isJumping && readyToJump)
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

    [ServerRpc]
    void WallRunServerRPC(bool isMoving, bool isJumping)
    {
        Vector3[] directions = new Vector3[]
            {
            transform.right,
            transform.right + transform.forward,
            transform.forward,
            -transform.right + transform.forward,
            -transform.right
            };

        for (int i = 0; i < directions.Length; i++)
        {
            Debug.DrawLine(transform.position, transform.position + directions[i], UnityEngine.Color.green);
            isWallRunning = Physics.Raycast(transform.position, directions[i], out wallHit, transform.localScale.x + 0.15f);
            if (!isGrounded && isWallRunning && isMoving)
            {
                Physics.gravity = Physics.gravity / 2.0f; // Reduce gravity to stay more time in the wall but not infinite
                rb.AddForce(transform.forward * player.Character.sprintSpeed, ForceMode.Force);
                JumpServerRPC(isJumping, wallHit.normal);
            }
        }

        // Rotate camera a bit on the z-axis
    }
    #endregion

    #region "Attacks and Abilities"

    [ServerRpc]
    void ShootServerRPC()
    {
        if (shootReady && player.Character != null && (projectileSpawnOffset != null && player.Character.projectilePrefab != null))
        {
            shootReady = false;
            
            GameObject projectileGO = Instantiate(player.Character.projectilePrefab, projectileSpawnOffset.position + cameraWeaponTransform.forward * player.Character.shootOffset, cameraWeaponTransform.rotation);
            projectileGO.GetComponent<NetworkObject>().Spawn(true);
            Invoke(nameof(ResetShoot), player.Character.shootCooldown);    //Delay for attack to reset
        }
    }

    void ResetShoot()
    {
        shootReady = true;
    }

    void ActivateAbility(Ability ability)
    {
        return;
    }
    #endregion

    #region "Player Data Visualization Methods"
    public void IncreaseScore(int amount)
    {
        player.Score += amount;
        view.UpdateView(player);
    }
    #endregion
}
