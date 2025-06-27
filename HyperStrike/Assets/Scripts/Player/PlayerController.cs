using System;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

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

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref move);
        serializer.SerializeValue(ref moveInProgress);
        serializer.SerializeValue(ref look);
        serializer.SerializeValue(ref sprint);
        serializer.SerializeValue(ref jump);
        serializer.SerializeValue(ref slide);
    }
}

public class PlayerController : NetworkBehaviour
{
    Rigidbody rb;

    #region "Model-View Player"
    private Player player;
    private PlayerAbilityController abilityController;
    #endregion

    #region "ANIMATIONS"
    NetworkAnimator animator;
    int VelocityHash;
    int GroundHash;
    int WalkingHash;
    int JumpingHash;
    int SlidingHash;
    int MeleeHash;
    int ShootHash;
    int Ability1Hash;
    int Ability2Hash;
    int UltimateHash;
    int DanceHash;
    int BackflipHash;
    #endregion

    PlayerInput input;

    #region "Movement Variables"
    [Header("Cinemachine Settings")]
    public CinemachineCamera cinemachineCamera; // Reference to the Cinemachine virtual camera
    [SerializeField] private Transform mainCameraTransform;
    [SerializeField] private Transform cameraWeaponTransform; // The transform of the Cinemachine camera's LookAt target
    float sensitivity = 5.0f;
    int invertY = 0;
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
    [SerializeField] LayerMask wallMask;
    RaycastHit wallHit;
    float stickWallForce = 10f;
    

    // Checkers
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
    [Header("Melee Variables")]
    private bool meleeReady;

    [Header("Shooting Variables")]
    public Transform projectileSpawnOffset;
    private bool shootReady;
    #endregion

    HyperStrikeUtils hyperStrikeUtils;

    public override void OnNetworkSpawn()
    {
        // Init Player MVC
        player = GetComponent<Player>();
        abilityController = GetComponent<PlayerAbilityController>();

        // Init Physics variables
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.maxLinearVelocity = player.MaxSpeed;

        if (IsClient && IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (PlayerPrefs.HasKey("masterSen"))
            {
                sensitivity = PlayerPrefs.GetFloat("masterSen");
            }

            if (PlayerPrefs.HasKey("masterInvertY"))
            {
                invertY = PlayerPrefs.GetInt("masterInvertY");
            }

            input = new PlayerInput();
            if (!input.Player.enabled) input?.Player.Enable();
            InitInputs();

            HideMeshRenderer();

            cinemachineCamera.Priority = 1;
            cameraTilt.OnValueChanged += OnCameraTiltChanged;

            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            NetworkManager.NetworkTickSystem.Tick += OnNetworkTick;
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
            cameraTilt.Value = CameraTilt.NONE;
            refCameraTilt = CameraTilt.NONE;
        }

        readyToJump = true;

        isGrounded = false;
        isWallRunning = false;

        // Init Attack Variables
        meleeReady = true;
        shootReady = true;

        hyperStrikeUtils = new HyperStrikeUtils();
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient && IsOwner)
        {
            input?.Player.Disable();
            cameraTilt.OnValueChanged -= OnCameraTiltChanged;
            NetworkManager.NetworkTickSystem.Tick -= OnNetworkTick;
        }
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

    private void OnNetworkTick()
    {
        if (MatchManager.Instance && !MatchManager.Instance.allowMovement.Value) return;
        if (GameManager.Instance && GameManager.Instance.isPaused) return;
        if (player && player.Health.Value <= 0f) return;

        if (!IsClient || !IsOwner) return;

        InputData data = new InputData
        {
            move = input.Player.Move.ReadValue<Vector2>(),
            moveInProgress = input.Player.Move.IsInProgress(),
            look = new Vector2(input.Player.Look.ReadValue<Vector2>().x * GameManager.Instance.mainSensitivity, (GameManager.Instance.invertY != 0 ? input.Player.Look.ReadValue<Vector2>().y : -input.Player.Look.ReadValue<Vector2>().y) * GameManager.Instance.mainSensitivity),
            sprint = input.Player.Sprint.IsPressed(),
            jump = input.Player.Jump.IsPressed(),
            slide = input.Player.Slide.IsPressed(),
        };

        SendInputServerRPC(data);
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
        else if (!IsOwner && !IsServer) // Doesnt work dk why
        {
            GetComponentInChildren<Player>().enabled = false;
            GetComponentInChildren<PlayerView>().enabled = false;
            GetComponentInChildren<PlayerAbilityController>().enabled = false;
            Debug.Log("Disable player thingies");
            GetComponentInChildren<PlayerController>().enabled = false;
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

    private void InitInputs()
    {
        input.Player.MeleeAttack.started += ctx => MeleeAttackServer();
        input.Player.Attack.started += ctx => ShootServer();
        input.Player.Ability1.started += ctx => ActivateAbility1Server();
        input.Player.Ability2.started += ctx => ActivateAbility2Server();
        input.Player.Ultimate.started += ctx => ActivateUltimateServer();
        input.Player.Emote1.started += ctx => Emote1ServerRPC();
        input.Player.Emote2.started += ctx => Emote2ServerRPC();
    }

    void InitAnimatorHashes()
    {
        VelocityHash = Animator.StringToHash("Velocity");
        GroundHash = Animator.StringToHash("Ground");
        WalkingHash = Animator.StringToHash("Walking");
        JumpingHash = Animator.StringToHash("Jumping");
        SlidingHash = Animator.StringToHash("Sliding");
        MeleeHash = Animator.StringToHash("Melee");
        ShootHash = Animator.StringToHash("Shoot");
        Ability1Hash = Animator.StringToHash("Ability1");
        Ability2Hash = Animator.StringToHash("Ability2");
        UltimateHash = Animator.StringToHash("Ultimate");
        DanceHash = Animator.StringToHash("Dance");
        BackflipHash = Animator.StringToHash("Backflip");
    }

    [ServerRpc]
    void SendInputServerRPC(InputData input)
    {
        isGrounded = hyperStrikeUtils.CheckGrounded(transform, characterHeight);
        animator?.Animator?.SetBool(GroundHash, isGrounded);

        isWallRunning = hyperStrikeUtils.CheckWalls(transform, ref wallHit, ref refCameraTilt, wallMask);

        // Only send when changed
        if (input.look != Vector2.zero && cinemachineCamera?.Target.TrackingTarget == null)
        {
            RotatePlayerWithCamera(input.look);
        }

        if (cinemachineCamera?.Target.TrackingTarget != null)
        {
            FollowTarget(input.look);
        }

        if (input.move != Vector2.zero)
        {
            WalkAndRun(input.move, input.sprint);

            WallRun(input.jump);
        }
        else
        {
            animator?.Animator?.ResetTrigger(WalkingHash);
            //animator?.Animator.ResetTrigger(WallrunHash);
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

        animator?.Animator?.SetFloat(VelocityHash, rb.linearVelocity.magnitude);
    }

    #region "Movement Mechanics Methods"
    void RotatePlayerWithCamera(Vector2 lookValue)
    {
        // Get mouse input
        float mouseX = lookValue.x * Time.fixedDeltaTime;
        float mouseY = lookValue.y * Time.fixedDeltaTime;

        // Adjust xRotation for vertical rotation and clamp it
        xRotation -= -mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        yRotation += mouseX;

        // Update the Cinemachine camera's rotation
        cameraWeaponTransform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        mainCameraTransform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        // Apply horizontal rotation to the player
        rb.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    void FollowTarget(Vector2 lookValue)
    {
        // Get direction to target
        Vector3 targetDirection = (cinemachineCamera.Target.TrackingTarget.position - transform.position).normalized;

        // Allow some camera movement while locked (reduced sensitivity)
        float mouseXx = lookValue.x * (sensitivity * 0.2f) * Time.fixedDeltaTime;
        float mouseYy = lookValue.y * (sensitivity * 0.2f) * Time.fixedDeltaTime;

        // Apply limited camera rotation
        xRotation -= mouseYy;
        xRotation = Mathf.Clamp(xRotation, -10f, 10f); // Reduced vertical range
        yRotation += mouseXx;
        yRotation = Mathf.Clamp(xRotation, -10f, 10f); // Reduced horzontal range

        cameraWeaponTransform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        mainCameraTransform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        // Only rotate player on Y axis to face target
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(targetDirection.x, 0, targetDirection.z));
        rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 100f);
    }

    void WalkAndRun(Vector2 moveValue, bool isSprinting)
    {
        if (isWallRunning && !isGrounded) return;

        float speed = isSprinting && isGrounded ? player.SprintSpeed : player.Speed;

        Vector3 dir = transform.forward * moveValue.y + transform.right * moveValue.x;
        rb.AddForce(dir.normalized * speed, ForceMode.Force);

        animator?.Animator?.SetTrigger(WalkingHash);
    }

    void Slide(bool isSliding)
    {
        if (isSliding)
        {
            rb.maxLinearVelocity = player.MaxSlidingSpeed;
            rb.linearDamping = 0.1f;
            animator?.Animator?.SetTrigger(SlidingHash);
        }
        else
        {
            rb.maxLinearVelocity = player.MaxSpeed;
            rb.linearDamping = 0.2f;
            animator?.Animator?.ResetTrigger(SlidingHash);
        }
    }

    void Jump(bool isJumping, Vector3 jumpDir, float forceAdd = 0f)
    {
        if (isJumping && readyToJump)
        {
            readyToJump = false;
            //Reset Y Velocity
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce((transform.up + jumpDir.normalized) * (jumpForce + forceAdd), ForceMode.Impulse);
            animator?.Animator?.SetTrigger(JumpingHash);
            Invoke(nameof(ResetJump), jumpCooldown);    //Delay for jump to reset
        }
        else animator?.Animator?.ResetTrigger(JumpingHash);
    }

    void ResetJump()
    {
        readyToJump = true;
    }

    void WallRun(bool isJumping)
    {
        if (!isGrounded && isWallRunning)
        {
            cameraTilt.Value = refCameraTilt;
            // Stick to wall
            rb.AddForce(-wallHit.normal * stickWallForce, ForceMode.Force);

            rb.AddForce((transform.forward * player.WallRunSpeed) + (transform.up * stickWallForce), ForceMode.Force); // Reduce gravity to stay more time in the wall but not infinite
            Jump(isJumping, wallHit.normal, 5f);
        }
        else
        {
            cameraTilt.Value = CameraTilt.NONE;
        }
    }
    #endregion

    #region "Attacks and Abilities"

    void MeleeAttackServer()
    {
        if (MatchManager.Instance && !MatchManager.Instance.allowMovement.Value) return;
        if (GameManager.Instance && GameManager.Instance.isPaused) return;
        if (player && player.Health.Value <= 0f) return;
        MeleeAttackServerRPC();
    }

    [ServerRpc]
    void MeleeAttackServerRPC()
    {
        if (meleeReady)
        {
            meleeReady = false;

            if (Physics.Raycast(cameraWeaponTransform.position, cameraWeaponTransform.forward, out RaycastHit hit, player.MeleeOffset))
            {
                Rigidbody rb = hit.rigidbody;

                if (rb != null)
                {
                    rb.AddForce(transform.forward * player.MeleeForce, ForceMode.Impulse);
                }

                if (hit.transform.TryGetComponent<Player>(out Player p))
                {
                    p.ApplyEffect(EffectType.DAMAGE, player.MeleeDamage.Value);
                }
            }

            Invoke(nameof(ResetMeleeAttack), 1f);    //Delay for attack to reset
            animator?.Animator?.ResetTrigger(MeleeHash);
            animator?.Animator?.SetTrigger(MeleeHash);
        }
    }

    void ResetMeleeAttack()
    {
        meleeReady = true;
    }

    void ShootServer()
    {
        if (MatchManager.Instance && !MatchManager.Instance.allowMovement.Value) return;
        if (GameManager.Instance && GameManager.Instance.isPaused) return;
        if (player && player.Health.Value <= 0f) return;
        ShootServerRPC();
    }

    [ServerRpc]
    void ShootServerRPC()
    {
        if (shootReady && (projectileSpawnOffset != null && player.ProjectilePrefab != null) && NetworkObjectPool.Singleton != null)
        {
            shootReady = false;

            var projectileNO = NetworkObjectPool.Singleton.GetNetworkObject(player.ProjectilePrefab, projectileSpawnOffset.position + cameraWeaponTransform.forward * player.ShootOffset, cameraWeaponTransform.rotation);

            if (!projectileNO.IsSpawned)
            {
                projectileNO.Spawn();
            }

            var projectile = projectileNO.GetComponent<Projectile>();
            projectile.projectilePrefabUsed = player.ProjectilePrefab;
            projectile.effectQuantity = player.ShootDamage.Value;
            projectile.Activate(projectileSpawnOffset.position + cameraWeaponTransform.forward * player.ShootOffset, cameraWeaponTransform.rotation, OwnerClientId, player);

            Invoke(nameof(ResetShoot), player.ShootCooldown);    //Delay for attack to reset
            animator?.Animator?.ResetTrigger(ShootHash);
            animator?.Animator?.SetTrigger(ShootHash);
        }
    }

    void ResetShoot()
    {
        shootReady = true;
    }

    void ActivateAbility1Server()
    {
        if (MatchManager.Instance && !MatchManager.Instance.allowMovement.Value) return;
        if (GameManager.Instance && GameManager.Instance.isPaused) return;
        if (player && player.Health.Value <= 0f) return;
        ActivateAbility1ServerRPC();
    }

    [ServerRpc]
    void ActivateAbility1ServerRPC()
    {
        abilityController.TryCastAbility(0);
    }

    void ActivateAbility2Server()
    {
        if (MatchManager.Instance && !MatchManager.Instance.allowMovement.Value) return;
        if (GameManager.Instance && GameManager.Instance.isPaused) return;
        if (player && player.Health.Value <= 0f) return;
        ActivateAbility2ServerRPC();
    }

    [ServerRpc]
    void ActivateAbility2ServerRPC()
    {
        abilityController.TryCastAbility(1);
    }

    void ActivateUltimateServer()
    {
        if (MatchManager.Instance && !MatchManager.Instance.allowMovement.Value) return;
        if (GameManager.Instance && GameManager.Instance.isPaused) return;
        if (player && player.Health.Value <= 0f) return;
        ActivateUltimateServerRPC();
    }

    [ServerRpc]
    void ActivateUltimateServerRPC()
    {
        abilityController.TryCastAbility(2);
    }
    #endregion

    #region "Emotes"
    [ServerRpc]
    void Emote1ServerRPC()
    {
        animator?.Animator?.ResetTrigger(DanceHash);
        animator?.Animator?.SetTrigger(DanceHash);
    }

    [ServerRpc]
    void Emote2ServerRPC()
    {
        animator?.Animator?.ResetTrigger(BackflipHash);
        animator?.Animator?.SetTrigger(BackflipHash);
    }
    #endregion
}
