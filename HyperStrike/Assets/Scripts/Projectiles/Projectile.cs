using System.Collections;
using Unity.Netcode;
using UnityEngine;

public abstract class Projectile : NetworkBehaviour
{
    public float speed = 1f;
    public float effectQuantity = 0f;
    public ulong playerOwnerId = 0;
    public float lifetime = 5f;
    [SerializeField] protected GameObject projectileFX;
    [SerializeField] protected Rigidbody rigidBody;
    protected bool isActive;

    public GameObject projectilePrefabUsed;

    public override void OnNetworkSpawn()
    {
        // Spawns from the player that shot
        rigidBody = GetComponent<Rigidbody>();
    }

    public virtual void Activate(Vector3 position, Quaternion rotation, ulong ownerId)
    {
        if (isActive) return; // Prevent double-activation

        playerOwnerId = ownerId;
        transform.SetPositionAndRotation(position, rotation);
        isActive = true;

        if (IsServer) StartCoroutine(LifetimeCountdown());
    }

    public virtual void Deactivate()
    {
        if (!isActive) return; // Prevent double-deactivation

        isActive = false;
        rigidBody.linearVelocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        GetComponent<NetworkObject>().Despawn();
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (!isActive || !IsServer) return;
        HandleImpact(collision);
    }

    protected abstract void HandleImpact(Collision collision);

    protected virtual IEnumerator LifetimeCountdown()
    {
        yield return new WaitForSeconds(lifetime);
        if (isActive) Deactivate();
    }
}
