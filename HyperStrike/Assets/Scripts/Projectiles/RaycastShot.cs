using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class RaycastShot : Projectile
{
    [SerializeField] protected float force;
    [SerializeField] protected LineRenderer lineRenderer;

    [SerializeField] protected EffectType effectType;
    [SerializeField] protected AffectedBaseStats affectedBaseStats;

    public override void OnNetworkSpawn()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public override void Activate(Vector3 position, Quaternion rotation, ulong ownerId)
    {
        base.Activate(position, rotation, ownerId);
        lineRenderer.SetPosition(0, position);
        CastTheRay(position);
    }

    public void CastTheRay(Vector3 initPos)
    {
        RaycastHit hit;
        if (Physics.Raycast(initPos, transform.forward, out hit, 100f))
        {
            Rigidbody rb = hit.rigidbody;
            if (rb)
            {
                if (rb.gameObject.CompareTag("Ball"))
                {
                    rb.AddForce(transform.forward * force, ForceMode.Impulse);
                }

                if (rb.gameObject.CompareTag("Player") && this.playerOwnerId != rb.gameObject.GetComponent<NetworkObject>().NetworkObjectId)
                {
                    rb.gameObject.GetComponent<Player>()?.ApplyEffect(effectType, effectQuantity, affectedBaseStats);
                }
            }
        }

        if (lineRenderer)
        {
            lineRenderer.SetPosition(1, hit.point);
        }

        ShowLineClientRPC(initPos, hit.point);
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
