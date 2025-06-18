using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RaycastShot : Projectile
{
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
            GameObject go = hit.transform.gameObject;
            if (go)
            {
                if (go.CompareTag("Player") && this.playerOwnerId != go.GetComponent<NetworkObject>().NetworkObjectId)
                {
                    go.GetComponent<Player>().ApplyDamage(damage);
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
