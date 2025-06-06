using UnityEngine;
using Unity.Netcode;

public class BallController : NetworkBehaviour
{
    private Rigidbody rb;

    [SerializeField] bool isGrounded;
    public bool IsGoal;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null) rb.maxLinearVelocity = 25.0f;
    }

    void FixedUpdate()
    {
        if (!MatchManager.Instance.allowMovement.Value) rb.linearVelocity = Vector3.zero;

        //Ground Check
        isGrounded = HyperStrikeUtils.CheckGrounded(transform);
        IsGoal = HyperStrikeUtils.CheckObjectInsideCollision(transform);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!enabled) return;

        if (other != null)
        {
            // Check hit with players to detect the last hit for the score
        }
    }
}