using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RaycastShot : Projectile
{
    [SerializeField] protected float force;
    [SerializeField] protected LineRenderer lineRenderer;

    [SerializeField] protected EffectType effectType;
    [SerializeField] protected AffectedBaseStats affectedBaseStats;
    [SerializeField] protected float quantity;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        lineRenderer = GetComponent<LineRenderer>();
    }

    public override void Activate(Vector3 position, Quaternion rotation, ulong ownerId)
    {
        base.Activate(position, rotation, ownerId);
        lineRenderer.SetPosition(0, transform.position);
        CastTheRay();
    }

    public void CastTheRay()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 100f))
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
                    rb.gameObject.GetComponent<Player>()?.ApplyEffect(effectType, quantity, affectedBaseStats);
                }
            }
        }

        if (lineRenderer)
        {
            lineRenderer.SetPosition(1, hit.point);
        }

        ShowLineClientRPC(hit.point);
    }

    public override void Deactivate()
    {
        isActive = false;
        gameObject.SetActive(false);
        //OnProjectileDeactivated?.Invoke(this);
    }

    [ClientRpc]
    private void ShowLineClientRPC(Vector3 finalPos)
    {
        if (lineRenderer)
        {
            lineRenderer.SetPosition(1, finalPos);
        }
    }

    protected override void HandleImpact(Collision collision)
    {
        return;
    }
}
