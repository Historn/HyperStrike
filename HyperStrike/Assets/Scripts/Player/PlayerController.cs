using HyperStrike;
using System;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

// PLAYER STATE????

public enum CameraTilt : byte
{
    NONE = 0,
    RIGHT,
    LEFT
}

[Serializable]
public struct InputData : INetworkSerializable
{
    public Vector2 move;
    public bool moveInProgress;
    public Vector2 look;
    public bool sprint;
    public bool jump;
    public bool slide;
    public bool melee;
    public bool shoot;
    public bool ability1;
    public bool ability2;
    public bool ultimate;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref move);
        serializer.SerializeValue(ref moveInProgress);
        serializer.SerializeValue(ref look);
        serializer.SerializeValue(ref sprint);
        serializer.SerializeValue(ref jump);
        serializer.SerializeValue(ref slide);
        serializer.SerializeValue(ref melee);
        serializer.SerializeValue(ref shoot);
        serializer.SerializeValue(ref ability1);
        serializer.SerializeValue(ref ability2);
        serializer.SerializeValue(ref ultimate);
    }
}

public class PlayerController : NetworkBehaviour
{
    Rigidbody rb;

    #region "Model-View Player"
    private Player player;
    private PlayerView view;
    private PlayerAbilityController abilityController;
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
    NetworkVariable<CameraTilt> cameraTilt = new NetworkVariable<CameraTilt>(CameraTilt.NONE);
    CameraTilt refCameraTilt;

    // Jump Vars
    bool readyToJump;
    float jumpCooldown = 0.5f;
    float jumpForce = 10.0f;

    // Ground Vars
    [Header("Ground Check")]
    [SerializeField] bool isGrounded;
    float characterHeight; // Change to character Data

    // Wall run
    [SerializeField] bool isWallRunning;
    RaycastHit wallHit;
    float stickWallForce = 10f;

    // Checkers
    private bool wasSprinting;
    private bool wasJumpPressed;
    private bool wasSliding;
    private bool wasWallRunning;
    private bool wasMelee;
    private bool wasShooting;
    private bool wasAbility1;
    private bool wasAbility2;
    private bool wasAbilityUltimate;
    #endregion

    #region "Attack Variables"
    [Header("Shooting Variables")]
    public Transform projectileSpawnOffset;
    private bool shootReady;

    #endregion

    HyperStrikeUtils hyperStrikeUtils;

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
        // Init Player MVC
        player = GetComponent<Player>();
        view = GetComponent<PlayerView>();
        abilityController = GetComponent<PlayerAbilityController>();

        view.UpdateView(player);

        // Init Physics variables
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.maxLinearVelocity = player.Character.maxSpeed;

        if (IsClient && IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            input = new PlayerInput();
            input?.Player.Enable();

            HideMeshRenderer();

            cinemachineCamera.Priority = 1;
            cameraTilt.OnValueChanged += OnCameraTiltChanged;

            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        else
        {
            cinemachineCamera.Priority = -1;

            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.None;
            characterHeight = GetComponent<CapsuleCollider>().height;
        }

        if (IsServer)
        {
            player.Score.Value = 0;
            cameraTilt.Value = CameraTilt.NONE;
            refCameraTilt = CameraTilt.NONE;
        }

        readyToJump = true;

        isGrounded = false;
        isWallRunning = false;

        // Init Attack Variables
        shootReady = true;

        hyperStrikeUtils = new HyperStrikeUtils();
    }

    private void OnCameraTiltChanged(CameraTilt previousValue, CameraTilt newValue)
    {
        if (cinemachineCamera == null) return;

        switch (cameraTilt.Value)
        {
            case CameraTilt.NONE:
                cinemachineCamera.Lens.Dutch = 0;
                break;
            case CameraTilt.RIGHT:
                cinemachineCamera.Lens.Dutch = 10;
                break;
            case CameraTilt.LEFT:
                cinemachineCamera.Lens.Dutch = -10;
                break;
        }
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
        if (MatchManager.Instance && !MatchManager.Instance.allowMovement.Value) return;

        if (IsClient && IsOwner)
        {
            InputData data = new InputData
            {
                move = input.Player.Move.ReadValue<Vector2>(),
                moveInProgress = input.Player.Move.IsInProgress(),
                look = input.Player.Look.ReadValue<Vector2>(),
                sprint = input.Player.Sprint.IsPressed(),
                jump = input.Player.Jump.IsPressed(),
                slide = input.Player.Slide.IsPressed(),
                melee = input.Player.MeleeAttack.IsPressed(),
                shoot = input.Player.Attack.IsPressed(),
                ability1 = input.Player.Ability1.IsPressed(),
                ability2 = input.Player.Ability2.IsPressed(),
                ultimate = input.Player.Ultimate.IsPressed(),
            };
            SendInputServerRPC(data);
        }

        if (IsServer)
            animator?.Animator.SetFloat(velocityHash, rb.linearVelocity.magnitude);
    }

    private void LateUpdate()
    {
        if (!IsOwner || cinemachineCamera == null) return;

        switch (cameraTilt.Value)
        {
            case CameraTilt.NONE:
                cinemachineCamera.Lens.Dutch = 0;
                break;
            case CameraTilt.RIGHT:
                cinemachineCamera.Lens.Dutch = 10;
                break;
            case CameraTilt.LEFT:
                cinemachineCamera.Lens.Dutch = -10;
                break;
        }
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

    [ServerRpc]
    void SendInputServerRPC(InputData input)
    {
        isGrounded = hyperStrikeUtils.CheckGrounded(transform, characterHeight);
        isWallRunning = hyperStrikeUtils.CheckWalls(transform, ref wallHit, ref refCameraTilt);

        // Only send when changed
        if (input.look != Vector2.zero)
            RotatePlayerWithCamera(input.look);

        if (input.move != Vector2.zero)
        {
            WalkAndRun(input.move, input.moveInProgress, input.sprint);
            wasSprinting = input.sprint;
        }

        WallRun(input.moveInProgress, input.jump);

        if (input.slide != wasSliding)
        {
            Slide(input.slide);
            wasSliding = input.slide;
        }

        if (input.jump && isGrounded && !isWallRunning)
        {
            Jump(input.jump, Vector3.zero);
        }

        if (input.melee != wasMelee)
        {
            MeleeAttack(input.melee);
            wasMelee = input.melee;
        }
        
        if (input.shoot != wasShooting)
        {
            Shoot(input.shoot);
            wasShooting = input.shoot;
        }
        
        if (input.ability1 != wasAbility1)
        {
            ActivateAbility1(input.ability1);
            wasAbility1 = input.ability1;
        }
        
        if (input.ability2 != wasAbility2)
        {
            ActivateAbility2(input.ability2);
            wasAbility2 = input.ability2;
        }
        
        if (input.ultimate != wasAbilityUltimate)
        {
            ActivateUltimate(input.ultimate);
            wasAbilityUltimate = input.ultimate;
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

        if (isWallRunning && !isGrounded) return;

        float speed = isSprinting && isGrounded ? player.Character.sprintSpeed : player.Character.speed;

        Vector3 dir = transform.forward * moveValue.y + transform.right * moveValue.x;
        rb.AddForce(dir.normalized * speed, ForceMode.Force);
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

    void Jump(bool isJumping, Vector3 jumpDir, float forceAdd = 0f)
    {
        if (isJumping && readyToJump)
        {
            readyToJump = false;
            //Reset Y Velocity
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce((transform.up + jumpDir.normalized) * (jumpForce + forceAdd), ForceMode.Impulse);
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
            cameraTilt.Value = refCameraTilt;
            // Stick to wall
            rb.AddForce(-wallHit.normal * stickWallForce, ForceMode.Force);

            rb.AddForce((transform.forward * player.Character.wallRunSpeed) + (transform.up * stickWallForce), ForceMode.Force); // Reduce gravity to stay more time in the wall but not infinite
            Jump(isJumping, wallHit.normal, 5f);
        }
        else
        {
            cameraTilt.Value = CameraTilt.NONE;
        }
    }
    #endregion

    #region "Attacks and Abilities"
    void MeleeAttack(bool isAttacking)
    {
        Debug.Log("Melee Attack");
        return;
    }

    void Shoot(bool isAttacking)
    {
        if (isAttacking && shootReady && player.Character != null && (projectileSpawnOffset != null && player.Character.projectilePrefab != null))
        {
            shootReady = false;

            GameObject projectileGO = Instantiate(player.Character.projectilePrefab, projectileSpawnOffset.position + cameraWeaponTransform.forward * player.Character.shootOffset, cameraWeaponTransform.rotation);
            projectileGO.GetComponent<Projectile>().playerOwnerId = this.NetworkObjectId;
            projectileGO.GetComponent<NetworkObject>().Spawn(true);
            Invoke(nameof(ResetShoot), player.Character.shootCooldown);    //Delay for attack to reset
            animator?.Animator.SetBool(isShootingHash, isAttacking);
        }
    }

    void ResetShoot()
    {
        shootReady = true;
    }

    void ActivateAbility1(bool isAb1)
    {
        Debug.Log("Trying to activate ABILITY 1");
        abilityController.TryCastAbility(0);
        return;
    }

    void ActivateAbility2(bool isAb2)
    {
        Debug.Log("Trying to activate ABILITY 2");
        abilityController.TryCastAbility(1);
        return;
    }
    
    void ActivateUltimate(bool isUlt)
    {
        Debug.Log("Trying to activate ULTIMATE");
        abilityController.TryCastAbility(2);
        return;
    }
    #endregion

    #region "Player Data Visualization Methods"
    public void IncreaseScore(int amount)
    {
        if (IsServer) player.Score.Value += amount;
        view.UpdateView(player);
    }
    #endregion
}
