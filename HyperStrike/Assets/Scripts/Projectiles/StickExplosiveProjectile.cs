using Unity.Netcode;
using UnityEngine;

public class StickExplosiveProjectile : ExplosiveProjectile
{
    bool firstHit = true;

    protected override void HandleImpact(Collision collision)
    {
        Stick(collision);
    }

    void Stick(Collision collision)
    {
        if (!firstHit) return;

        NetworkObject netObj = collision.gameObject.GetComponent<NetworkObject>();
        SphereCollider sphereCollider = GetComponent<SphereCollider>();

        rigidBody.isKinematic = true;

        if (netObj && netObj.OwnerClientId != this.playerOwnerId)
        {
            if (sphereCollider != null) sphereCollider.isTrigger = true;
            transform.SetParent(collision.transform);
        }

        firstHit = false;
    }

    public override void Deactivate()
    {
        firstHit = true;
        transform.SetParent(null);

        rigidBody.isKinematic = false;

        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider != null)
            sphereCollider.isTrigger = false;

        base.Deactivate();
    }
}
