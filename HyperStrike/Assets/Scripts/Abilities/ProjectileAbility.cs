using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile Ability", menuName = "HyperStrike/Projectile Ability")]
public class ProjectileAbility : Ability
{
    public GameObject projectilePrefab;
    public float projectileSpeed;
    public int damage;

    public override void ServerCast(ulong clientId)
    {
        base.ServerCast(clientId);

        Vector3 pos = new Vector3(owner.transform.forward.x, owner.transform.forward.y + 1, owner.transform.forward.z + 1);

        // Spawn projectile on server
        var projectile = Instantiate(projectilePrefab, owner.transform.position + pos, owner.transform.rotation);
        projectile.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);

        PlayEffects(owner.transform.position);
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