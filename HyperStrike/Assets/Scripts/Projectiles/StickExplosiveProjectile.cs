using Unity.Netcode;
using UnityEngine;

public class StickExplosiveProjectile : ExplosiveProjectile
{
    bool firstHit = true;

    private void OnCollisionEnter(Collision other)
    {
        if (!enabled || !IsServer || other == null) return;

        collision = other;
        NetworkObject netObj = other.gameObject.GetComponent<NetworkObject>();
        SphereCollider sphereCollider = GetComponent<SphereCollider>();

        if (firstHit)
        {
            if (netObj && netObj.OwnerClientId != this.playerOwnerId)
            {
                sphereCollider.isTrigger = true;
                rigidBody.isKinematic = true;
                transform.SetParent(other.transform);
            }
            else
            {
                rigidBody.isKinematic = true;
            }
            firstHit = false;
        }
    }
}
