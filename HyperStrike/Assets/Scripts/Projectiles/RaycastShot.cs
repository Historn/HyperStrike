using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RaycastShot : Projectile
{
    [SerializeField] private float force;
    [SerializeField] private float timeToDestroy;
    [SerializeField] private LineRenderer lineRenderer;

    public override void OnNetworkSpawn()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, transform.position);
        if (!IsServer) return;

        StartCoroutine(DestroyProjectile());
    }

    protected override void OnNetworkPostSpawn()
    {
        Use();
    }

    public override void Use()
    {
        if (!IsServer) return;

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
                    rb.gameObject.GetComponent<Player>().ApplyDamage(damage);
                }
            }
        }

        if (lineRenderer)
        {
            lineRenderer.SetPosition(1, hit.point);
        }

        ShowLineClientRPC(hit.point);
    }

    [ClientRpc]
    private void ShowLineClientRPC(Vector3 finalPos)
    {
        if (lineRenderer)
        {
            lineRenderer.SetPosition(1, finalPos);
        }
    }

    IEnumerator DestroyProjectile()
    {
        yield return new WaitForSeconds(timeToDestroy);
        gameObject.GetComponent<NetworkObject>().Despawn();
    }
}
