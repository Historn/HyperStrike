using UnityEngine;
using Unity.Netcode;

public class BallController : NetworkBehaviour
{
    private Rigidbody rb;

    [SerializeField] bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null) rb.maxLinearVelocity = 25.0f;
    }

    void FixedUpdate()
    {
        //Ground Check
        Vector3 endRayPos = new Vector3(transform.position.x, transform.position.y - (transform.localScale.x * 0.5f + 0.1f), transform.position.z);
        Debug.DrawLine(transform.position, endRayPos, UnityEngine.Color.red);
        isGrounded = Physics.Raycast(transform.position, Vector3.down, transform.localScale.x * 0.5f + 0.2f);
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