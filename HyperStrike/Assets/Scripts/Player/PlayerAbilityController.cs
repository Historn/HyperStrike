using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PlayerAbilityController : NetworkBehaviour
{
    [SerializeField, Header("Ability Slots")]
    private List<Ability> abilityInstances = new List<Ability>();

    private NetworkVariable<int> currentSelectedAbility = new NetworkVariable<int>();

    private Player player;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        player = GetComponent<Player>(); ;

        // Initialize all abilities
        if (IsServer)
        {
            foreach (var ability in player.Character.abilities)
            {
                if (ability != null)
                {
                    var instance = Instantiate(ability);
                    instance.Initialize(this);
                    abilityInstances.Add(instance);
                }
            }
        }
    }

    public void CastAbility(int abilityIndex, ulong clientId)
    {
        if (abilityIndex < 0 || abilityIndex >= abilityInstances.Count) return;

        var ability = abilityInstances[abilityIndex];

        if (ability == null) return;
        if (ability.isOnCooldown) return;

        //Debug.Log(abilities[abilityIndex].abilityName);
        ability.ServerCast(clientId);
        ability.isOnCooldown = true;

        // Start cooldown timer
        StartCoroutine(StartCooldown(abilityIndex));
    }

    private IEnumerator StartCooldown(int index)
    {
        var ability = abilityInstances[index];

        yield return new WaitForSeconds(ability.fullCooldown);
        ability.isOnCooldown = false;
    }
     
    public void TryCastAbility(int index)
    {
        // Client-side prediction
        abilityInstances[index]?.OnStartCast(OwnerClientId);
        // Send to server
        CastAbility(index, OwnerClientId);
    }
}