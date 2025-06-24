using Unity.Netcode;
using UnityEngine;

public class EffectProjectile : Projectile
{
    [SerializeField] protected EffectType effectType;
    [SerializeField] protected AffectedBaseStats affectedBaseStats;
    [SerializeField] protected float effectTime;
    

    public override void Activate(Vector3 position, Quaternion rotation, ulong ownerId)
    {
        base.Activate(position, rotation, ownerId);
        rigidBody.AddForce(transform.forward * speed, ForceMode.Impulse);
    }

    protected override void HandleImpact(Collision collision)
    {
        ApplyEffect(collision);
        SpawnParticlesClientRPC(collision.GetContact(0).point, collision.GetContact(0).normal);
        Deactivate();
    }

    private void ApplyEffect(Collision collision)
    {
        if (collision.collider.TryGetComponent<Player>(out var player))
        {
            if (player.OwnerClientId != playerOwnerId)
            {
                player.ApplyEffect(effectType, effectQuantity, effectTime, affectedBaseStats);
            }
        }
    }

    [ClientRpc]
    protected virtual void SpawnParticlesClientRPC(Vector3 position, Vector3 normal)
    {
        if (projectileFX != null)
        {
            var rotation = Quaternion.LookRotation(normal);
            var vfx = Instantiate(projectileFX, position, rotation);
            Destroy(vfx, 3f);
        }
    }
}
