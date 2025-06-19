using System.Collections;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Movement Area Ability", menuName = "HyperStrike/Movement Area Ability")]
public class AreaMovementAbility : MovementAbility
{
    [Header("Activators")]
    public bool useForce;
    public bool useDamage;
    public bool useHeal;
    public bool affectSelf;
    public bool affectAllies;
    public bool affectEnemies;

    [Header("Values")]
    public float radius;
    public float force;
    public bool setManualForceDir;
    public Vector3 forceDir;
    public int heal;

    Collider[] colliders;

    public override void ServerCast(ulong clientId, Vector3 initPos, Vector3 dir)
    {
        base.ServerCast(clientId, initPos, dir);

        owner.StartCoroutine(FinishMovementToArea());

        PlayEffects(initPos);
    }

    private IEnumerator FinishMovementToArea()
    {
        while (isDashing) yield return null;

        ApplyAreaEffect();
    }

    void ApplyAreaEffect()
    {
        colliders = Physics.OverlapSphere(owner.transform.position, radius);

        foreach (Collider collider in colliders)
        {
            Rigidbody rb = collider.GetComponent<Rigidbody>();

            if (useForce && rb != null)
            {
                Vector3 d = rb.position - owner.transform.position;

                if (setManualForceDir) d = forceDir;

                rb.AddForce(d.normalized * force, ForceMode.Impulse);
            }

            if (!collider.CompareTag("Player")) continue;

            if (useDamage) collider.GetComponent<Player>().ApplyDamage(damage);

            if (useHeal) collider.GetComponent<Player>().ApplyHeal(heal);
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
