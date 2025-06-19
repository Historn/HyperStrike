using UnityEngine;
using Unity.Netcode;

public class BallController : NetworkBehaviour
{
    private Rigidbody rb;

    public bool IsGoal;

    HyperStrikeUtils hyperStrikeUtils;

    private ulong lastPlayerHitId = 0;
    public Vector3 lastPlayerHitPosition;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        hyperStrikeUtils = new HyperStrikeUtils();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null) rb.maxLinearVelocity = 25.0f;
    }

    void FixedUpdate()
    {
        IsGoal = hyperStrikeUtils.CheckObjectInsideCollision(transform);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!enabled) return;

        if (other != null)
        {
            GameObject obj = other.gameObject;
            // Check hit with players to detect the last hit for the score
            if (obj.CompareTag("Player"))
            {
                this.lastPlayerHitId = obj.GetComponent<NetworkObject>().NetworkObjectId;
                lastPlayerHitPosition = other.GetContact(0).point;
            }
        }
    }
}