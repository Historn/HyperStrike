using System.Collections;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Targeted Ability", menuName = "HyperStrike/Targeted Ability")]
public class TargetLockAbility : Ability
{
    [Header("Settings")]
    public float lockRange;
    public LayerMask targetLayers;
    public bool targetOwnerTeam;
    public bool fixCameraToTarget;

    [SerializeField] protected Transform currentTarget;

    public override void Initialize(PlayerAbilityController player)
    {
        base.Initialize(player);
        requiresTarget = true;
    }

    public override void ServerCast(ulong clientId, Vector3 initPos, Vector3 dir)
    {
        base.ServerCast(clientId, initPos, dir);
        owner.StartCoroutine(LockCameraToTarget());
        PlayEffects(initPos);
    }
    private IEnumerator LockCameraToTarget()
    {
        castTime = 0;

        while (castTime < maxCastTime)
        {
            castTime += Time.deltaTime;
            Transform newTarget = FindClosestTarget();

            if (fixCameraToTarget && newTarget != null)
            {
                var targetNetObj = newTarget.GetComponent<NetworkObject>();
                if (targetNetObj != null)
                {
                    currentTarget = newTarget;
                    LookAtTargetClientRPC(targetNetObj);
                }
            }
            yield return null;
        }

        ResetLookAtTargetClientRPC();
    }

    private Transform FindClosestTarget()
    {
        Transform closestTarget = null;

        Collider[] colliders = Physics.OverlapSphere(owner.transform.position, lockRange, targetLayers);
        float closestDistance = Mathf.Infinity;

        foreach (Collider target in colliders)
        {
            Player player = target.GetComponent<Player>();

            if (player.OwnerClientId == owner.OwnerClientId) continue;

            //if (!targetOwnerTeam && player.Team.Value == owner.GetComponent<Player>().Team.Value) continue;
            Debug.Log("Found " + target.name);
            float distance = Vector3.Distance(owner.transform.position, target.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target.transform;
            }
        }

        return closestTarget;
    }

    [ClientRpc]
    void LookAtTargetClientRPC(NetworkObjectReference targetRef)
    {
        if (!targetRef.TryGet(out NetworkObject targetObj)) return;

        var cinemachineCamera = owner.GetComponentInChildren<CinemachineCamera>();
        if (cinemachineCamera == null) return;
        Debug.Log("CAMERA FOUND" + targetObj.transform.position);
        cinemachineCamera.Target.TrackingTarget = targetObj.transform;
    }

    [ClientRpc]
    void ResetLookAtTargetClientRPC()
    {
        var cinemachineCamera = owner.GetComponentInChildren<CinemachineCamera>();
        if (cinemachineCamera == null) return;

        cinemachineCamera.Target.TrackingTarget = null;
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
