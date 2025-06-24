using Unity.Netcode;
using UnityEngine;

public abstract class Ability : NetworkBehaviour
{
    [Header("Basic Info")]
    public string abilityName;
    public Sprite icon;
    public float chargeReloadTime;
    public float fullCooldown;
    [SerializeField] protected float castTime;
    public float maxCastTime;
    public byte maxCharges;

    public bool isReloading;
    public bool requiresTarget;
    public bool isOnCooldown;

    [Header("Networking")]
    public NetworkVariable<byte> currentCharges = new NetworkVariable<byte>(1);
    public NetworkVariable<float> currentCooldownTime = new NetworkVariable<float>(0f);
    public NetworkVariable<float> currentReloadTime = new NetworkVariable<float>(0f);
    

    // Reference to the player who owns this ability instance
    protected PlayerAbilityController owner;

    // Called when ability is assigned to a player
    public virtual void Initialize(PlayerAbilityController player)
    {
        owner = player;
        isOnCooldown = false;
        isReloading = false;
        castTime = 0f;

        if (!IsServer) return;

        currentCharges.Value = maxCharges;
        currentCooldownTime.Value = 0;
        currentReloadTime.Value = 0;
    }

    // Called when player presses the ability button (client-side prediction)
    public virtual void OnStartCast(ulong clientId) { }

    // Server-side validation and execution
    public virtual void ServerCast(ulong clientId, Vector3 initPos, Vector3 dir)
    {
        // Charges
        currentCharges.Value--;
        if (currentCharges.Value < 1)
        {
            Debug.Log("Is on cooldown");
            isOnCooldown = true;
        }
        else if (!isOnCooldown && currentCharges.Value < maxCharges)
        {
            Debug.Log("Is reloading");
            isReloading = true;
        }
    }

    // Called when ability is done (cleanup, etc.)
    public virtual void OnFinishCast() { }

    // Visual/audio effects that run on all clients
    public virtual void PlayEffects(Vector3 position) { }
}