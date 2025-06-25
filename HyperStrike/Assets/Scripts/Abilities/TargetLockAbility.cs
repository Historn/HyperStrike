using System.Collections;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class TargetLockAbility : Ability
{
    [Header("Settings")]
    public float lockRange;
    public LayerMask targetLayers;
    public bool targetOwnerTeam;
    public bool fixCameraToTarget;

    [SerializeField] protected Transform currentTarget;

    public override void Initialize(Player player)
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

        currentTarget = FindClosestTarget();

        if (fixCameraToTarget && currentTarget != null)
        {
            var targetNetObj = currentTarget.GetComponent<NetworkObject>();
            if (targetNetObj != null)
            {
                LookAtTargetClientRPC(targetNetObj);
            }
        }

        while (castTime < maxCastTime)
        {
            castTime += Time.deltaTime;
            
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


        currentTarget = targetObj.transform;

        var cinemachineCamera = owner.GetComponentInChildren<CinemachineCamera>();
        if (cinemachineCamera == null) return;
        Debug.Log($"CAMERA FOUND: {targetObj.transform.position}, {targetObj.name}");
        
        cinemachineCamera.Target.TrackingTarget = targetObj.transform;
    }

    [ClientRpc]
    void ResetLookAtTargetClientRPC()
    {
        var cinemachineCamera = owner.GetComponentInChildren<CinemachineCamera>();
        if (cinemachineCamera == null) return;

        cinemachineCamera.Target.TrackingTarget = null;
        currentTarget = null;
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
