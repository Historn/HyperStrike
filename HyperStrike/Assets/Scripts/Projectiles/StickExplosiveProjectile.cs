using Unity.Netcode;
using UnityEngine;

public class StickExplosiveProjectile : ExplosiveProjectile
{
    bool firstHit = true;

    protected override void HandleImpact(Collision collision)
    {
        Stick(collision);
        SpawnParticlesClientRPC(collision.GetContact(0).point, collision.GetContact(0).normal);
        Deactivate();
    }

    void Stick (Collision collision)
    {
        if (firstHit)
        {
            NetworkObject netObj = collision.gameObject.GetComponent<NetworkObject>();
            SphereCollider sphereCollider = GetComponent<SphereCollider>();

            if (netObj && netObj.OwnerClientId != this.playerOwnerId)
            {
                sphereCollider.isTrigger = true;
                rigidBody.isKinematic = true;
                transform.SetParent(collision.transform);
            }
            else
            {
                rigidBody.isKinematic = true;
            }
            firstHit = false;
        }
    }
}
