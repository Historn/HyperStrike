using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Area Ability", menuName = "HyperStrike/Area Ability")]
public class AreaAbility : Ability
{
    public bool useForce;
    public bool useDamage;
    public bool useHeal;
    public bool selfAffect;

    public float radius;
    public float force;
    public int damage;
    public int heal;
    
    Collider[] colliders;

    public override void ServerCast(ulong clientId, Vector3 initPos, Vector3 dir)
    {
        base.ServerCast(clientId, initPos, dir);

        colliders = Physics.OverlapSphere(initPos, radius);

        foreach (Collider collider in colliders)
        {
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if (rb != null) continue;

            if (useForce)
            {
                Vector3 d = rb.position - initPos;
                rb.AddForce(d.normalized * force, ForceMode.Impulse);
            }

            if (!collider.CompareTag("Player")) continue;

            if (useDamage) collider.GetComponent<Player>().ApplyDamage(damage);

            if (useHeal) collider.GetComponent<Player>().ApplyHeal(heal);
        }

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
