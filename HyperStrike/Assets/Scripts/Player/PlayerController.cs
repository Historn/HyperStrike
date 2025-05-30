using HyperStrike;
using System;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

// PLAYER STATE????

[Serializable]
public struct InputData : INetworkSerializable
{
    public Vector2 move;
    public Vector2 look;
    public bool sprint;
    public bool jump;
    public bool slide;
    public bool shoot;
    public bool moveInProgress;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref move);
        serializer.SerializeValue(ref look);
        serializer.SerializeValue(ref sprint);
        serializer.SerializeValue(ref jump);
        serializer.SerializeValue(ref slide);
        serializer.SerializeValue(ref shoot);
        serializer.SerializeValue(ref moveInProgress);
    }
}

public class PlayerController : NetworkBehaviour
{
    Rigidbody rb;

    #region "Model-View Player"
    private Player player;
    private PlayerView view;
    public Character characterData;
    #endregion

    NetworkAnimator animator;
    int velocityHash;
    int isWalkingHash;
    int isJumpingHash;
    int isSlidingHash;
    int isShootingHash;

    PlayerInput input;

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

    // Checkers
    private bool wasSprinting;
    private bool wasJumpPressed;
    private bool wasSliding;
    private bool wasWallRunning;
    private bool wasShooting;
    #endregion

    #region "Attack Variables"
    [Header("Shooting Variables")]
    public Transform projectileSpawnOffset;
    private bool shootReady;

    #endregion

    private void OnEnable()
    {
        input?.Player.Enable();
    }

    private void OnDisable()
    {
        input?.Player.Disable();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient && IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            input = new PlayerInput();
            input?.Player.Enable();

            HideMeshRenderer();

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
    }

    private void Start()
    {
        animator = GetComponent<NetworkAnimator>();
        InitAnimatorHashes();
        if (!IsOwner)
        {
            // Disable this camera if not owned by this client
            GetComponentInChildren<Camera>().enabled = false;
            GetComponentInChildren<AudioListener>().enabled = false;
            GetComponentInChildren<CinemachineBrain>().enabled = false;
        }
    }

    // Physics-based + Rigidbody Actions
    private void FixedUpdate()
    {
        isGrounded = HyperStrikeUtils.CheckGrounded(transform, characterHeight);
        isWallRunning = HyperStrikeUtils.CheckWalls(transform, ref wallHit);

        if (IsClient && IsOwner && GameManager.Instance.allowMovement)
        {
            InputData data = new InputData
            {
                move = input.Player.Move.ReadValue<Vector2>(),
                look = input.Player.Look.ReadValue<Vector2>(),
                sprint = input.Player.Sprint.IsPressed(),
                jump = input.Player.Jump.IsPressed(),
                slide = input.Player.Slide.IsPressed(),
                shoot = input.Player.Attack.IsPressed(),
                moveInProgress = input.Player.Move.IsInProgress()
            };
            SendInputServerRPC(data);
        }

        if (IsServer)
            animator?.Animator.SetFloat(velocityHash, rb.linearVelocity.magnitude);
    }

    void HideMeshRenderer()
    {
        var meshes = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var mesh in meshes)
        {
            if (mesh != null) mesh.enabled = false;
        }
    }

    void InitAnimatorHashes()
    {
        velocityHash = Animator.StringToHash("Velocity");
        isWalkingHash = Animator.StringToHash("isWalking");
        isJumpingHash = Animator.StringToHash("isJumping");
        isSlidingHash = Animator.StringToHash("isSliding");
        isShootingHash = Animator.StringToHash("isShooting");
    }

    [ServerRpc(Delivery = RpcDelivery.Unreliable)]
    void SendInputServerRPC(InputData input)
    {
        // Only send when changed
        if (input.look != Vector2.zero)
            RotatePlayerWithCamera(input.look);

        if (input.move != Vector2.zero)
        {
            WalkAndRun(input.move, input.moveInProgress, input.sprint);
            wasSprinting = input.sprint;
        }

        if (input.moveInProgress != wasWallRunning || input.jump != wasJumpPressed)
        {
            WallRun(input.moveInProgress, input.jump);
            // Rotate camera a bit on the z-axis
            wasWallRunning = input.moveInProgress;
            wasJumpPressed = input.jump;
        }

        if (input.slide != wasSliding)
        {
            Slide(input.slide);
            wasSliding = input.slide;
        }

        if (input.jump && isGrounded && !isWallRunning)
        {
            Jump(input.jump, Vector3.zero);
        }

        if (input.shoot != wasShooting)
        {
            Shoot(input.shoot);
            wasShooting = input.shoot;
        }
    }

    #region "Movement Mechanics Methods"
    void RotatePlayerWithCamera(Vector2 lookValue)
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

    void WalkAndRun(Vector2 moveValue, bool isWalking, bool isSprinting)
    {
        animator?.Animator.SetBool(isWalkingHash, isWalking);
        // Sprint
        float speed = player.Character.speed;
        if (isSprinting && isGrounded) speed = player.Character.sprintSpeed;

        Vector3 dir = transform.forward * moveValue.y + transform.right * moveValue.x;
        if (!isWallRunning) rb.AddForce(dir.normalized * speed, ForceMode.Force);
    }

    void Slide(bool isSliding)
    {
        if (isSliding)
        {
            rb.maxLinearVelocity = player.Character.maxSlidingSpeed;
            rb.linearDamping = 0.1f;
        }
        else
        {
            rb.maxLinearVelocity = player.Character.maxSpeed;
            rb.linearDamping = 0.2f;
        }
        animator?.Animator.SetBool(isSlidingHash, isSliding);
    }

    void Jump(bool isJumping, Vector3 jumpDir)
    {
        if (isJumping && readyToJump)
        {
            readyToJump = false;
            //Reset Y Velocity
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce((transform.up + jumpDir) * jumpForce, ForceMode.Impulse);
            animator?.Animator.SetBool(isJumpingHash, isJumping);
            Invoke(nameof(ResetJump), jumpCooldown);    //Delay for jump to reset
        }
    }

    void ResetJump()
    {
        readyToJump = true;
    }

    void WallRun(bool isMoving, bool isJumping)
    {
        if (!isGrounded && isWallRunning && isMoving)
        {
            rb.AddForce(transform.forward * player.Character.wallRunSpeed + transform.up * 0.8f, ForceMode.Force); // Reduce gravity to stay more time in the wall but not infinite
            Jump(isJumping, wallHit.normal);
        }
    }
    #endregion

    #region "Attacks and Abilities"
    void Shoot(bool isAttacking)
    {
        if (isAttacking && shootReady && player.Character != null && (projectileSpawnOffset != null && player.Character.projectilePrefab != null))
        {
            shootReady = false;

            GameObject projectileGO = Instantiate(player.Character.projectilePrefab, projectileSpawnOffset.position + cameraWeaponTransform.forward * player.Character.shootOffset, cameraWeaponTransform.rotation);
            projectileGO.GetComponent<NetworkObject>().Spawn(true);
            Invoke(nameof(ResetShoot), player.Character.shootCooldown);    //Delay for attack to reset
            animator?.Animator.SetBool(isShootingHash, isAttacking);
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
