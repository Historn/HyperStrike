using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ProjectileAbility : Ability
{
    [Header("Projectiles Settings")]
    public GameObject projectilePrefab;
    public uint projectilesQuantity = 1;

    public override void ServerCast(ulong clientId, Vector3 initPos, Vector3 dir)
    {
        base.ServerCast(clientId, initPos, dir);

        owner.StartCoroutine(EnableProjectiles());

        PlayEffects(initPos);
    }

    private IEnumerator EnableProjectiles()
    {
        for (uint i = 0; i < projectilesQuantity; i++)
        {
            var projectileNO = NetworkObjectPool.Singleton.GetNetworkObject(projectilePrefab, owner.GetCastTransform().position + owner.GetCastTransform().forward, owner.GetCastTransform().rotation);

            if (!projectileNO.IsSpawned)
            {
                projectileNO.Spawn();
            }

            var projectile = projectileNO.GetComponent<Projectile>();
            projectile.projectilePrefabUsed = projectilePrefab;
            projectile.Activate(owner.GetCastTransform().position + owner.GetCastTransform().forward, owner.GetCastTransform().rotation, owner.OwnerClientId, owner);

            yield return new WaitForSeconds(maxCastTime);
        }
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