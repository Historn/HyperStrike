using HyperStrike;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

// CLASS FOR VARIABLES AND FUNCTIONS - NOT ACTIONS/INPUTS
public class Player : NetworkBehaviour
{
    public string PlayerName = "PlayerName";
    public ulong PlayerId = 0;

    public NetworkVariable<byte> Team = new NetworkVariable<byte>(0);
    public NetworkVariable<int> Score = new NetworkVariable<int>(0);
    public NetworkVariable<int> Goals = new NetworkVariable<int>(0);

    public bool isProtected { get; private set; }

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

    public void ApplyDamage(int damage)
    {
        if (!MatchManager.Instance) return;
        
        Character.health -= damage;
        if (Character.health <= 0)
        {
            playerEventSubscriber.OnDeath.Invoke();
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            StartCoroutine(DeadTimer());
        }
        playerEventSubscriber.OnReceiveDamage.Invoke();
    }

    public void ApplyHeal(int heal)
    {
        if (!MatchManager.Instance) return;

        Character.health += heal;
        if (Character.health >= Character.maxHealth)
        {
            Character.health = Character.maxHealth;
        }
    }
    
    public void ApplyProtection(bool protect)
    {
        isProtected = protect;
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
