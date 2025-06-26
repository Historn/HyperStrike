using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PlayerAbilityController : NetworkBehaviour
{
    [SerializeField, Header("Ability Slots")]
    private List<Ability> abilities = new List<Ability>();

    private Player player;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        player = GetComponent<Player>(); ;

        // Initialize all abilities
        foreach (var ability in abilities)
        {
            if (ability != null)
            {
                ability.Initialize(player);
            }
        }
    }

    public void CastAbility(int abilityIndex, ulong clientId)
    {
        if (abilityIndex < 0 || abilityIndex >= abilities.Count) return;

        var ability = abilities[abilityIndex];

        if (ability == null) return;
        if (ability.isOnCooldown || ability.currentCharges.Value <= 0) return;

        //Debug.Log(abilities[abilityIndex].abilityName);
        ability.ServerCast(clientId, player.GetCastTransform().position, player.GetCastTransform().forward);

        // Start cooldown timer
        if (ability.isOnCooldown)
        {
            ability.currentCooldownTime.Value = 0;
            StopCoroutine(StartReloading(abilityIndex));
            StartCoroutine(StartCooldown(abilityIndex));
        }
        else
        {
            StartCoroutine(StartReloading(abilityIndex));
        }
    }

    private IEnumerator StartCooldown(int index)
    {
        var ability = abilities[index];

        while (ability.currentCooldownTime.Value < ability.fullCooldown) 
        {
            ability.currentCooldownTime.Value++;
            yield return new WaitForSeconds(1f);
        }
        ability.currentCharges.Value = ability.maxCharges;
        ability.isOnCooldown = false;
    }
    private IEnumerator StartReloading(int index)
    {
        var ability = abilities[index];

        while (ability.currentCharges.Value < ability.maxCharges)
        {
            while (ability.currentReloadTime.Value < ability.chargeReloadTime)
            {
                ability.currentReloadTime.Value++;
                yield return new WaitForSeconds(1f);
            }

            ability.currentCharges.Value++;
            ability.currentReloadTime.Value = 0;
            yield return null;
        }

        ability.isReloading = false;
    }

    public void TryCastAbility(int index)
    {
        // Client-side prediction
        //abilityInstances[index]?.OnStartCast(OwnerClientId);
        // Send to server
        CastAbility(index, OwnerClientId);
    }
}