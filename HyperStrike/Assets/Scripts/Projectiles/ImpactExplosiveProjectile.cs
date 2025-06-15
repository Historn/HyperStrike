using Unity.Netcode;
using UnityEngine;

public class ImpactExplosiveProjectile : ExplosiveProjectile
{
    private void OnCollisionEnter(Collision other)
    {
        if (!enabled && !IsServer) return;

        if (other != null)
        {
            Explode(other);
        }
    }
}
