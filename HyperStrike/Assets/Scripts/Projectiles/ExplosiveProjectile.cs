using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public struct SendParticles : INetworkSerializable
{
    public Vector3 collisionPoint;
    public Vector3 collisionNormal;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref collisionPoint);
        serializer.SerializeValue(ref collisionNormal);
    }
}

public class ExplosiveProjectile : Projectile
{
    public float explosionForce = 25f;
    public float explosionRadius = 20f;
    [SerializeField] private int timeToDestroy;
    [SerializeField] protected bool isForwardDirection = true;
    [SerializeField] protected Vector3 direction;
    [SerializeField] protected GameObject spawnFX;
    [SerializeField] protected GameObject explosionFX;
    [SerializeField] protected Rigidbody rigidBody;

    protected Collision collision = null;

    public override void OnNetworkSpawn()
    {
        SendParticles sendParticles = new SendParticles
        {
            collisionPoint = Vector3.zero,
            collisionNormal = Vector3.zero
        };

        //SpawnParticlesClientRPC(sendParticles, 0.3f);

        // Spawns from the player that shot
        rigidBody = GetComponent<Rigidbody>();

        if (IsServer) StartCoroutine(DestroyRocket());
    }

    protected override void OnNetworkPostSpawn()
    {
        if (IsServer)
        {
            if (rigidBody != null)
            {
                Move();
            }
            else if (rigidBody == null)
            {
                Debug.Log("Rigidbody Not Found!");
                gameObject.GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    public override void Move()
    {
        if (isForwardDirection) rigidBody.AddForce(transform.forward * speed, ForceMode.Impulse);
        else rigidBody.AddForce((transform.forward + direction.normalized) * speed, ForceMode.Impulse);
    }

    protected virtual void Explode(Collision other = null)
    {
        SendParticles explosion = new SendParticles
        {
            collisionPoint = Vector3.zero,
            collisionNormal = Vector3.zero
        };

        
        if (other != null && other.contactCount > 0)
        {
            explosion.collisionPoint = other.GetContact(0).point;
            explosion.collisionNormal = other.GetContact(0).normal;
        }

        SpawnParticlesClientRPC(explosion, 3f, true);

        if (other != null)
        {
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = rb.position - transform.position;

                rb.AddForce(dir.normalized * explosionForce, ForceMode.Impulse);
                
                if (other.gameObject.CompareTag("Player") && this.playerOwnerId != other.gameObject.GetComponent<NetworkObject>().NetworkObjectId)
                {
                    other.gameObject.GetComponent<Player>().ApplyDamage(damage);
                }
            }
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider collider in colliders)
        {
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = rb.position - transform.position;

                rb.AddForce(dir.normalized * explosionForce, ForceMode.Impulse);

                if (collider.CompareTag("Player") && this.playerOwnerId != collider.GetComponent<NetworkObject>().NetworkObjectId)
                {
                    collider.GetComponent<Player>().ApplyDamage(damage);
                }
            }
        }

        gameObject.GetComponent<NetworkObject>().Despawn();
    }

    [ClientRpc]
    protected virtual void SpawnParticlesClientRPC(SendParticles sendParticles, float destructionTime = -1f, bool useImpactNormal = false)
    {
        if (explosionFX != null)
        {
            if (!useImpactNormal)
            {
                Quaternion spawnRotation = Quaternion.LookRotation(transform.up, transform.forward);
                GameObject vfxInstance = Instantiate(explosionFX, transform.position, spawnRotation);
                Debug.Log("Particle Active");
                Destroy(vfxInstance, destructionTime);
            }
            else
            {
                // Find the nearest axis to the impact normal
                Vector3 impactNormal = sendParticles.collisionNormal;
                Vector3 nearestAxis = FindNearestAxis(impactNormal);
                
                Quaternion spawnRotation = Quaternion.LookRotation(nearestAxis);
                GameObject vfxInstance = Instantiate(explosionFX, transform.position, spawnRotation);
                Debug.Log("Particle Active");
                Destroy(vfxInstance, destructionTime);
            }
        }
        else Debug.Log("PARTICLE NULL");
    }

    private Vector3 FindNearestAxis(Vector3 normal)
    {
        // Compare the normal with the primary axes and choose the closest one
        Vector3[] axes = { Vector3.right, Vector3.up, Vector3.forward, -Vector3.right, -Vector3.up, -Vector3.forward };
        Vector3 nearest = axes[0];
        float maxDot = Vector3.Dot(normal, axes[0]);

        foreach (Vector3 axis in axes)
        {
            float dot = Vector3.Dot(normal, axis);
            if (dot > maxDot)
            {
                maxDot = dot;
                nearest = axis;
            }
        }

        return nearest;
    }

    IEnumerator DestroyRocket()
    {
        yield return new WaitForSeconds(timeToDestroy);
        Explode(collision);
    }
}
