using System.Collections;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Windows;

[RequireComponent(typeof(NetworkObject))]
public class PlayerAbilityController : NetworkBehaviour
{
    [SerializeField, Header("Ability Slots")]
    private Ability[] abilities;

    private NetworkVariable<int> currentSelectedAbility = new NetworkVariable<int>();

    private Player player;
    private PlayerInput input;

    private void OnEnable()
    {
        input?.Player.Enable();
    }

    private void OnDisable()
    {
        input?.Player.Disable();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        player = GetComponent<Player>(); ;

        abilities = player.Character.abilities;

        // Initialize all abilities
        foreach (var ability in abilities)
        {
            if (ability != null) ability.Initialize(this);
        }

        if (IsServer)
        {
            foreach (var ability in abilities)
            {
                if (ability != null)
                {
                    ability.currentCharges.Value = ability.maxCharges;
                    ability.isOnCooldown.Value = false;
                }
            }
        }

        if (IsClient && IsOwner)
        {
            input = new PlayerInput();
            input?.Player.Enable();
        }
    }

    public void CastAbility(int abilityIndex, ulong clientId)
    {
        Debug.Log(abilities[abilityIndex].abilityName);
        Debug.Log(abilities[abilityIndex].isOnCooldown.Value);

        if (abilityIndex < 0 || abilityIndex >= abilities.Length) return;
        if (abilities[abilityIndex] == null) return;
        if (abilities[abilityIndex].isOnCooldown.Value) return;

        Debug.Log(abilities[abilityIndex].abilityName);
        abilities[abilityIndex].ServerCast(clientId);
        abilities[abilityIndex].isOnCooldown.Value = true;

        // Start cooldown timer
        StartCoroutine(StartCooldown(abilityIndex, abilities[abilityIndex].fullCooldown));
    }

    private IEnumerator StartCooldown(int index, float duration)
    {
        yield return new WaitForSeconds(duration);
        abilities[index].isOnCooldown.Value = false;
    }
     
    public void TryCastAbility(int index)
    {
        // Client-side prediction
        abilities[index]?.OnStartCast(OwnerClientId);
        // Send to server
        CastAbility(index, OwnerClientId);
    }
}