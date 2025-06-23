using HyperStrike;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public enum EffectType : byte
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

    public NetworkVariable<Team> Team = new NetworkVariable<Team>(0);
    public NetworkVariable<int> Score = new NetworkVariable<int>(0);
    public NetworkVariable<int> Goals = new NetworkVariable<int>(0);

    public bool IsProtected { get; private set; }
    public float BoostPercentage { get; private set; }
    public AffectedBaseStats AffectedStats { get; private set; }

    float maxDeadTime = 5f;
    public NetworkVariable<float> deadTime = new NetworkVariable<float>(5.0f);

    public Character Character;

    private PlayerEventSubscriber playerEventSubscriber;

    public override void OnNetworkSpawn()
    {
        maxDeadTime = deadTime.Value;
        if (IsServer)
        {
            playerEventSubscriber = GetComponent<PlayerEventSubscriber>();
            Character.health = Character.maxHealth;
        }
    }

    public void ApplyEffect(EffectType effectType, float quantity = 0f, AffectedBaseStats affectedBaseStats = AffectedBaseStats.NONE)
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
                    ApplyBoost(quantity, affectedBaseStats);
                }
                break;
            default:
                break;
        }
    }

    private void ApplyDamage(int damage)
    {
        Character.health -= damage;
        if (Character.health <= 0)
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

        Character.health += heal;
        if (Character.health >= Character.maxHealth)
        {
            Character.health = Character.maxHealth;
        }
    }

    private void ApplyProtection(bool protect)
    {
        IsProtected = protect;
    }

    private void ApplyBoost(float percentage, AffectedBaseStats affectedBaseStats)
    {

        if (BoostPercentage < 0 || AffectedStats != AffectedBaseStats.NONE) { return; }

        if (affectedBaseStats.HasFlag(AffectedBaseStats.MAX_HP))
        {
            Character.health = Character.maxHealth;
            Character.health += (int)(Character.maxHealth * percentage);
        }
        
        if (affectedBaseStats.HasFlag(AffectedBaseStats.SPEED))
        {
            Character.speed = Character.maxSpeed;
            Character.speed += (int)(Character.maxSpeed * percentage);
        }

        if (affectedBaseStats.HasFlag(AffectedBaseStats.DAMAGE))
        {
            //Character.maxSpeed += (int)(Character.maxSpeed * percentage);
        }

        AffectedStats = affectedBaseStats;
        BoostPercentage = percentage;
    }

    private IEnumerator DeadTimer()
    {
        while (deadTime.Value >= 0.0f)
        {
            yield return new WaitForSeconds(1f);
            deadTime.Value--;
        }

        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        Character.health = Character.maxHealth;
        deadTime.Value = maxDeadTime;
    }
}
