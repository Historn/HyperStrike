using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ExplosiveProjectile : Projectile
{
    public bool explodeOnContact = true;
    public float explosionForce = 25f;
    public float explosionRadius = 20f;
    public LayerMask targetLayers;
    [SerializeField] protected bool isForwardDirection = true;
    [SerializeField] protected Vector3 direction;

    public override void Activate(Vector3 position, Quaternion rotation, ulong ownerId)
    {
        base.Activate(position, rotation, ownerId);
        rigidBody.AddForce(transform.forward * speed, ForceMode.Impulse);
    }

    protected override void HandleImpact(Collision collision)
    {
        if (!explodeOnContact) return;

        Explode(collision);
        SpawnParticlesClientRPC(collision.GetContact(0).point, collision.GetContact(0).normal);
        Deactivate();
    }

    protected virtual void Explode(Collision collision)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, targetLayers);

        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent<Rigidbody>(out var rb))
            {
                if (collider.CompareTag("Player"))
                {
                    Player player = collider.GetComponent<Player>();

                    if (player.IsProtected) return;

                    if (player.OwnerClientId != playerOwnerId)
                        player.ApplyEffect(EffectType.DAMAGE, effectQuantity);
                }
                if (collider.CompareTag("Shield")) continue;
                Vector3 dir = rb.position - transform.position;
                rb.AddForce(dir.normalized * explosionForce, ForceMode.Impulse);
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

    protected override IEnumerator LifetimeCountdown()
    {
        yield return new WaitForSeconds(lifetime);
        Explode(collision: null);
        SpawnParticlesClientRPC(transform.position, transform.up);
        if (isActive) Deactivate();
    }
}
