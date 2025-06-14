using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Movement Ability", menuName = "HyperStrike/Movement Ability")]
public class MovementAbility : Ability
{
    [Header("Movement Settings")]
    public float dashDistance;
    public float dashSpeed;
    public bool canDamageEnemies;
    public float damageAmount;
    public Vector3 dashDirection;
    public bool isForwardDirection;
    public LayerMask collisionLayers;

    public override void ServerCast(ulong clientId)
    {
        base.ServerCast(clientId);

        // Movmeent behaviour on server
        if (isForwardDirection) dashDirection = owner.transform.forward;
        Debug.Log("Movement Abilty");
        owner.GetComponent<Rigidbody>().MovePosition(dashDirection);

        PlayEffects(owner.transform.position);
    }

    public override void PlayEffects(Vector3 position)
    {
        // Play VFX/SFX on all clients
        PlayEffectsClientRpc(position);
    }

    [ClientRpc]
    private void PlayEffectsClientRpc(Vector3 position)
    {
        // Instantiate visual/audio effects
    }
}
