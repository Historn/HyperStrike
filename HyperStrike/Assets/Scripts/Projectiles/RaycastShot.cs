using Unity.Netcode;
using UnityEngine;

public class RaycastShot : Projectile
{
    [SerializeField] protected float range;
    [SerializeField] protected float force;
    [SerializeField] protected LineRenderer lineRenderer;

    [SerializeField] protected EffectType effectType;
    [SerializeField] protected AffectedBaseStats affectedBaseStats;
    [SerializeField] protected float effectTime;

    public override void OnNetworkSpawn()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public override void Activate(Vector3 position, Quaternion rotation, ulong ownerId, Player ownerProj)
    {
        base.Activate(position, rotation, ownerId, ownerProj);
        lineRenderer.SetPosition(0, position);
        CastTheRay(position);
    }

    public void CastTheRay(Vector3 initPos)
    {
        RaycastHit hit;
        if (Physics.Raycast(initPos, transform.forward, out hit, range)) // Add layers affected to make it more simple
        {
            if (lineRenderer)
            {
                lineRenderer.SetPosition(1, hit.point);
            }
            Rigidbody rb = hit.rigidbody;
            if (rb)
            {
                if (rb.gameObject.CompareTag("Ball"))
                {
                    rb.AddForce(transform.forward * force, ForceMode.Impulse);
                }

                if (hit.collider.TryGetComponent<Player>(out Player otherPlayer) && this.playerOwnerId != rb.gameObject.GetComponent<NetworkObject>().NetworkObjectId)
                {
                    if (effectType == EffectType.DAMAGE && otherPlayer.Team.Value == owner.Team.Value) return;

                    otherPlayer.ApplyEffect(effectType, effectQuantity, effectTime, affectedBaseStats);
                }
            }
        }
        ShowLineClientRPC(initPos, initPos+transform.forward*range);
    }

    public override void Deactivate()
    {
        if (!isActive) return; // Prevent double-deactivation

        isActive = false;
        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, Vector3.zero);
        GetComponent<NetworkObject>().Despawn();
    }

    [ClientRpc]
    private void ShowLineClientRPC(Vector3 initPos, Vector3 finalPos)
    {
        if (lineRenderer)
        {
            lineRenderer.SetPosition(0, initPos);
            lineRenderer.SetPosition(1, finalPos);
        }
    }

    protected override void HandleImpact(Collision collision)
    {
        return;
    }
}
