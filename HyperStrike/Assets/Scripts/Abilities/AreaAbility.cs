using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AreaAbility : Ability
{
    [Header("Activators")]
    public bool useForce;
    public bool useDamage;
    public bool useHeal;
    public bool useProtection;
    public bool selfAffect;
    public bool affectAllies;
    public bool affectEnemies;

    [Header("Values")]
    public float radius;
    public float force;
    public bool setManualForceDir;
    public Vector3 forceDir;
    public int damage;
    public int heal;

    Collider[] colliders;
    private HashSet<Player> previouslyAffectedPlayers = new HashSet<Player>();

    [Header("Prefab Area Mesh")]
    [SerializeField] protected bool useAreaPrefab;
    [SerializeField] protected bool parentAreaToOwner;
    [SerializeField] protected GameObject areaObjectPrefab;
    private GameObject areaObject;

    public override void ServerCast(ulong clientId, Vector3 initPos, Vector3 dir)
    {
        base.ServerCast(clientId, initPos, dir);
        owner.StartCoroutine(AreaCast());
        PlayEffects(initPos);
    }

    void ApplyAreaEffect()
    {
        if (useAreaPrefab && areaObjectPrefab != null)
        {
            areaObject = Instantiate(areaObjectPrefab, owner.transform.position, owner.transform.rotation);
            areaObject.GetComponent<NetworkObject>().Spawn(true);
            if (parentAreaToOwner) areaObject.transform.SetParent(owner.transform);
            radius = areaObject.transform.localScale.x;
            useAreaPrefab = false;
        }

        colliders = Physics.OverlapSphere(owner.transform.position, radius);

        HashSet<Player> currentlyAffectedPlayers = new HashSet<Player>();

        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent<Player>(out Player player) && owner.TryGetComponent<Player>(out Player ownerPlayer))
            {
                if (!selfAffect && player.OwnerClientId == ownerPlayer.OwnerClientId) return;
                if (!affectAllies && player.Team.Value == ownerPlayer.Team.Value) return;
                if (!affectEnemies && player.Team.Value != ownerPlayer.Team.Value) return;

                if (useDamage)
                {
                    player.ApplyEffect(EffectType.DAMAGE, damage);
                    currentlyAffectedPlayers.Add(player);
                }

                if (useHeal)
                {
                    player.ApplyEffect(EffectType.HEAL, heal);
                    if (!currentlyAffectedPlayers.Contains(player)) currentlyAffectedPlayers.Add(player);
                }

                if (useProtection)
                {
                    player.ApplyEffect(EffectType.PROTECT);
                    if (!currentlyAffectedPlayers.Contains(player)) currentlyAffectedPlayers.Add(player);
                }
            }

            Rigidbody rb = collider.GetComponent<Rigidbody>();

            if (useForce && rb != null)
            {
                Vector3 d = rb.position - owner.transform.position;

                if (setManualForceDir) d = forceDir;

                rb.AddForce(d.normalized * force, ForceMode.Impulse);
            }

            // Revoke protection for players who left the area
            foreach (Player p in previouslyAffectedPlayers)
            {
                if (!currentlyAffectedPlayers.Contains(p))
                {
                    p.ApplyEffect(EffectType.UNPROTECT);
                }
            }

            previouslyAffectedPlayers = currentlyAffectedPlayers;
        }
    }

    private IEnumerator AreaCast()
    {
        castTime = 0;

        while (castTime < maxCastTime)
        {
            castTime += Time.deltaTime;
            ApplyAreaEffect();
            yield return null;
        }

        if (areaObject != null)
        {
            areaObject.GetComponent<NetworkObject>().Despawn();
            useAreaPrefab = true;
        }

        foreach (Collider collider in colliders)
        {
            if (useProtection) collider.GetComponent<Player>()?.ApplyEffect(EffectType.UNPROTECT);
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
