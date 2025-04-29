using System;
using System.Collections;
using UnityEngine;

//[CreateAssetMenu(fileName = "New Ability", menuName = "HyperStrike/Ability Data")]
public abstract class Ability : ScriptableObject
{
    [Header("Basic Info")]
    public string abilityName;
    public Sprite abilityIcon;
    public string description;

    [Header("Timing")]
    public float cooldownDuration;
    private float cooldownTimer;
    public float castTime;
    public float duration;
    public bool canMoveWhileCasting;

    [Header("Resources")]
    public float energyCost;
    
    public bool isReady { get; private set; }

    // Core methods that each ability will implement
    public abstract bool CanUseAbility(AbilityUser user);
    public abstract void InitiateAbility(AbilityUser user);
    public abstract void ExecuteAbility(AbilityUser user);
    public abstract void EndAbility(AbilityUser user);

    // Optional override for ability update logic (continuous effects)
    public virtual void UpdateAbility(AbilityUser user) { }
}