using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile Ability", menuName = "HyperStrike/Projectile Ability")]
public class ProjectileAbility : Ability
{
    public GameObject projectilePrefab;

    public override void ServerCast(ulong clientId, Vector3 initPos, Vector3 dir)
    {
        base.ServerCast(clientId, initPos, dir);

        // Spawn projectile on server
        var projectile = Instantiate(projectilePrefab, initPos + dir, owner.transform.rotation);
        projectile.GetComponent<NetworkObject>().Spawn();
        projectile.GetComponent<Projectile>().playerOwnerId = clientId;

        PlayEffects(initPos);
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