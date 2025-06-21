using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile Ability", menuName = "HyperStrike/Projectile Ability")]
public class ProjectileAbility : Ability
{
    [Header("Projectiles Settings")]
    public GameObject projectilePrefab;
    public uint projectilesQuantity = 1;
    protected int projectilesEnabled = 0;
    List<GameObject> projectilesPool;

    public override void Initialize(PlayerAbilityController player)
    {
        base.Initialize(player);

        if (!owner.IsServer) return;

        projectilesEnabled = 0;
        //projectilesPool = new List<GameObject>();
        //projectilePrefab.SetActive(false);
        //for (uint i = 0; i < projectilesQuantity; i++)
        //{
        //    if (projectilePrefab == null) { Debug.LogWarning("Projectile prefab not found"); return; }
        //    if (owner == null) { Debug.LogWarning("Owner not found"); return; };

        //    // Spawn projectile on server
        //    var projectile = Instantiate(projectilePrefab, owner.GetCastTransform().position + owner.GetCastTransform().forward, owner.GetCastTransform().rotation);
        //    projectile.GetComponent<NetworkObject>().Spawn(true);
        //    projectile.GetComponent<Projectile>().playerOwnerId = owner.OwnerClientId;
        //    projectilesPool.Add(projectile);
        //}
    }

    public override void ServerCast(ulong clientId, Vector3 initPos, Vector3 dir)
    {
        base.ServerCast(clientId, initPos, dir);

        owner.StartCoroutine(EnableProjectiles());

        PlayEffects(initPos);
    }

    private IEnumerator EnableProjectiles()
    {
        //projectilesEnabled = 0;

        for (uint i = 0; i < projectilesQuantity; i++)
        {
            //if (projectilePrefab == null) { Debug.LogWarning("Projectile prefab not found"); return; }
            //if (owner == null) { Debug.LogWarning("Owner not found"); return; };

            // Spawn projectile on server
            var projectile = Instantiate(projectilePrefab, owner.GetCastTransform().position + owner.GetCastTransform().forward, owner.GetCastTransform().rotation);
            projectile.GetComponent<NetworkObject>().Spawn(true);
            projectile.GetComponent<Projectile>().playerOwnerId = owner.OwnerClientId;
            //projectilesPool.Add(projectile);
            yield return new WaitForSeconds(0.5f);
        }

        //while (projectilesEnabled < projectilesQuantity)
        //{
        //    projectilesPool[projectilesEnabled].transform.forward = owner.GetCastTransform().forward;
        //    projectilesPool[projectilesEnabled].SetActive(true);
        //    projectilesEnabled++;
        //    yield return new WaitForSeconds(0.5f);
        //}
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