using HyperStrike;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

// CLASS FOR VARIABLES AND FUNCTIONS - NOT ACTIONS/INPUTS
public class Player : NetworkBehaviour
{
    public string PlayerName = "PlayerName";
    public ulong PlayerId = 0;
    //public byte Team = 0;
    //public int Score { get; set; }
    //public int Goals { get; set; }

    public NetworkVariable<byte> Team = new NetworkVariable<byte>(0);
    public NetworkVariable<int> Score = new NetworkVariable<int>(0);
    public NetworkVariable<int> Goals = new NetworkVariable<int>(0);

    public Character Character;

    private PlayerEventSubscriber playerEventSubscriber;

    public NetworkVariable<float> deadTime = new NetworkVariable<float>(5.0f);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            playerEventSubscriber = GetComponent<PlayerEventSubscriber>();
            Character.health = Character.maxHealth;
        }
    }

    public void ApplyDamage(int damage)
    {
        Character.health -= damage;
        if (Character.health <= 0) 
        {
            playerEventSubscriber.OnDeath.Invoke();
        }
        else
        {
            playerEventSubscriber.OnReceiveDamage.Invoke();
        }
    }

    private IEnumerator DeadTimer()
    {
        while (deadTime.Value >= 0.0f)
        {
            yield return new WaitForSeconds(1f);
            deadTime.Value--;
        }

        this.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        Character.health = Character.maxHealth;
    }
}
