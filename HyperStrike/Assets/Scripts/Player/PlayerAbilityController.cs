using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PlayerAbilityController : NetworkBehaviour
{
    [SerializeField, Header("Ability Slots")]
    private List<Ability> abilityInstances = new List<Ability>();

    private Player player;

    [SerializeField] private Transform CastTransform;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        player = GetComponent<Player>(); ;

        // Initialize all abilities
        foreach (var ability in abilityInstances)
        {
            if (ability != null)
            {
                var instance = Instantiate(ability);
                instance.Initialize(this);
                abilityInstances.Add(instance);
            }
        }
    }

    public void CastAbility(int abilityIndex, ulong clientId)
    {
        if (abilityIndex < 0 || abilityIndex >= abilityInstances.Count) return;

        var ability = abilityInstances[abilityIndex];

        if (ability == null) return;
        if (ability.isOnCooldown || ability.currentCharges <= 0) return;

        //Debug.Log(abilities[abilityIndex].abilityName);
        ability.ServerCast(clientId, CastTransform.position, CastTransform.forward);

        // Start cooldown timer
        if (ability.isOnCooldown)
        {
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
        var ability = abilityInstances[index];

        yield return new WaitForSeconds(ability.fullCooldown);
        ability.currentCharges = ability.maxCharges;
        ability.isOnCooldown = false;
    }
    private IEnumerator StartReloading(int index)
    {
        var ability = abilityInstances[index];

        yield return new WaitForSeconds(ability.chargeReloadTime);

        ability.currentCharges++;

        if (ability.currentCharges < ability.maxCharges)
        {
            yield return new WaitForSeconds(ability.chargeReloadTime);
        }
        else
        {
            ability.isReloading = false;
        }
    }

    public void TryCastAbility(int index)
    {
        // Client-side prediction
        //abilityInstances[index]?.OnStartCast(OwnerClientId);
        // Send to server
        CastAbility(index, OwnerClientId);
    }

    public Transform GetCastTransform() { return CastTransform; }
}