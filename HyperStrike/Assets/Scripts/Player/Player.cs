using HyperStrike;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public enum EffectType : byte // Convert to flags to be able to apply multiple effects like poison and boost or smth
{
    NONE,
    DAMAGE,
    HEAL,
    PROTECT,
    UNPROTECT,
    BOOST
}

[Flags]
public enum AffectedBaseStats : byte
{
    NONE,
    MAX_HP,
    SPEED,
    DAMAGE
}

public enum Team : byte
{
    LOCAL,
    VISITANT
}

// CLASS FOR VARIABLES AND FUNCTIONS - NOT ACTIONS/INPUTS
public class Player : NetworkBehaviour
{
    public string PlayerName = "PlayerName";
    public ulong PlayerId = 0;

    // Network variables tienen que ser las que se vean en UI
    [Header("Name")]
    public string Name = "DefaultCharacterName";

    [Header("Health")]
    public NetworkVariable<int> Health = new NetworkVariable<int>(100);
    public NetworkVariable<int> MaxHealth = new NetworkVariable<int>(100); // A

    [Header("Speeds")]
    public float Speed = 30f;
    public float SprintSpeed = 60f;
    public float WallRunSpeed = 100f;
    public float MaxSpeed = 15f;
    public float MaxSlidingSpeed = 15f;

    
    public NetworkVariable<int> ShootDamage = new NetworkVariable<int>(15);
    [Header("Shoot Attack")]
    public float ShootCooldown;
    public float ShootOffset;

    
    public NetworkVariable<int> MeleeDamage = new NetworkVariable<int>(15);
    [Header("Melee Attack")]
    public float MeleeForce;
    public float MeleeOffset;

    public GameObject ProjectilePrefab;

    public NetworkVariable<Team> Team = new NetworkVariable<Team>(0);
    public NetworkVariable<int> Score = new NetworkVariable<int>(0);
    public NetworkVariable<int> Goals = new NetworkVariable<int>(0);

    public bool IsProtected { get; private set; }
    public float BoostPercentage { get; private set; }
    public AffectedBaseStats AffectedStats { get; private set; }

    float maxDeadTime = 5f;
    public NetworkVariable<float> deadTime = new NetworkVariable<float>(5.0f);

    [SerializeField] private Character Character;

    private PlayerEventSubscriber playerEventSubscriber;

    [SerializeField] private Transform CastTransform;

    public override void OnNetworkSpawn()
    {
        maxDeadTime = deadTime.Value;
        if (IsServer)
        {
            playerEventSubscriber = GetComponent<PlayerEventSubscriber>();
            ResetInitCharacterValues();
            ProjectilePrefab = Character.projectilePrefab;
        }
    }

    void ResetInitCharacterValues()
    {
        Name = Character.name;

        Health.Value = Character.health;
        MaxHealth.Value = Character.maxHealth;

        Speed = Character.speed;
        SprintSpeed = Character.sprintSpeed;
        WallRunSpeed = Character.wallRunSpeed;
        MaxSpeed = Character.maxSpeed;
        MaxSlidingSpeed = Character.maxSlidingSpeed;

        ShootDamage.Value = Character.shootDamage;
        ShootCooldown = Character.shootCooldown;
        ShootOffset = Character.shootOffset;

        MeleeDamage.Value = Character.meleeDamage;
        MeleeForce = Character.meleeForce;
        MeleeOffset = Character.meleeOffset;
    }

    public void ApplyEffect(EffectType effectType, float quantity = 0f, float time = 0.0f, AffectedBaseStats affectedBaseStats = AffectedBaseStats.NONE)
    {
        switch (effectType)
        {
            case EffectType.NONE:
                break;
            case EffectType.DAMAGE:
                {
                    ApplyDamage((int)quantity);
                }
                break;
            case EffectType.HEAL:
                {
                    ApplyHeal((int)quantity);
                }
                break;
            case EffectType.PROTECT:
                {
                    ApplyProtection(true);
                }
                break;
            case EffectType.UNPROTECT:
                {
                    ApplyProtection(false);
                }
                break;
            case EffectType.BOOST:
                {
                    ApplyBoost(quantity, time, affectedBaseStats);
                }
                break;
            default:
                break;
        }
    }

    private void ApplyDamage(int damage)
    {
        Health.Value -= damage;
        if (Health.Value <= 0)
        {
            if (MatchManager.Instance) playerEventSubscriber.OnDeath.Invoke();
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            StartCoroutine(DeadTimer());
        }
        if (MatchManager.Instance) playerEventSubscriber.OnReceiveDamage.Invoke();
    }

    private void ApplyHeal(int heal)
    {
        if (!MatchManager.Instance) return;

        Health.Value += heal;
        if (Health.Value >= MaxHealth.Value)
        {
            Health.Value = MaxHealth.Value;
        }
    }

    private void ApplyProtection(bool protect)
    {
        IsProtected = protect;
    }

    private void ApplyBoost(float percentage, float time, AffectedBaseStats affectedBaseStats)
    {
        if (BoostPercentage < 0 || AffectedStats != AffectedBaseStats.NONE) { return; }

        StopCoroutine(BoostTimer(time));

        if (affectedBaseStats.HasFlag(AffectedBaseStats.MAX_HP))
        {
            MaxHealth.Value += (int)(MaxHealth.Value * percentage);
            Health.Value = MaxHealth.Value;
        }

        if (affectedBaseStats.HasFlag(AffectedBaseStats.SPEED))
        {
            MaxSpeed += (int)(MaxSpeed * percentage);
            Speed += (int)(Speed * percentage);
        }

        if (affectedBaseStats.HasFlag(AffectedBaseStats.DAMAGE))
        {
            ShootDamage.Value += (int)(ShootDamage.Value * percentage);
            MeleeDamage.Value += (int)(MeleeDamage.Value * percentage);
        }

        AffectedStats = affectedBaseStats;
        BoostPercentage = percentage;
        StartCoroutine(BoostTimer(time));
    }

    private IEnumerator BoostTimer(float time)
    {
        yield return new WaitForSeconds(time);
        ResetInitCharacterValues();
    }

    private IEnumerator DeadTimer()
    {
        while (deadTime.Value >= 0.0f)
        {
            yield return new WaitForSeconds(1f);
            deadTime.Value--;
        }

        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        Health.Value = Character.maxHealth;
        deadTime.Value = maxDeadTime;
    }

    public Transform GetCastTransform() { return CastTransform; }
}
