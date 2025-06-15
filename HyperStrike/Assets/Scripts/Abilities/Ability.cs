using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

//[CreateAssetMenu(fileName = "New Ability", menuName = "HyperStrike/Ability Data")]
public abstract class Ability : ScriptableObject
{
    [Header("Basic Info")]
    public string abilityName;
    public Sprite icon;
    public float chargeReloadTime;
    public float fullCooldown;
    public float castTime;
    public int maxCharges;

    [Header("Networking")]
    public int currentCharges;
    public bool isReloading;
    public bool requiresTarget;
    public bool isOnCooldown;

    // Reference to the player who owns this ability instance
    protected PlayerAbilityController owner;

    // Called when ability is assigned to a player
    public virtual void Initialize(PlayerAbilityController player)
    {
        owner = player;
        isOnCooldown = false;
        isReloading = false;
        currentCharges = maxCharges;
    }

    // Called when player presses the ability button (client-side prediction)
    public virtual void OnStartCast(ulong clientId) { }

    // Server-side validation and execution
    public virtual void ServerCast(ulong clientId) 
    {
        // Charges
        currentCharges--;
        if (currentCharges < 1) 
        {
            Debug.Log("Is on cooldown");
            isOnCooldown = true;
        }
        else if (!isOnCooldown && currentCharges < maxCharges)
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