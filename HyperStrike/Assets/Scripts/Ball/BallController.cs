using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Physics Properties")]
    public float gravityMultiplier = 1.0f;
    public float bounciness = 0.7f;

    private Rigidbody rb;
    private PhysicsMaterial ballMaterial;

    private Vector3 velocity;

    [SerializeField] bool isGrounded;
    float groundDrag = 0.25f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Create a new Physics Material for runtime changes
        ballMaterial = new PhysicsMaterial
        {
            bounciness = bounciness,
            frictionCombine = PhysicsMaterialCombine.Multiply,
            bounceCombine = PhysicsMaterialCombine.Maximum
        };

        // Apply the material to the collider
        var collider = GetComponent<Collider>();
        if (collider != null)
            collider.material = ballMaterial;

        // Adjust initial gravity and maximum velocity to improve performance and reliability
        Physics.gravity = Vector3.down * 9.81f * gravityMultiplier;
        if (rb != null) rb.maxLinearVelocity = 25.0f;
    }

    void Update()
    {
        // Dynamically update gravity if the multiplier changes
        Physics.gravity = Vector3.down * 9.81f * gravityMultiplier;

        // Update the bounciness
        if (ballMaterial != null)
            ballMaterial.bounciness = bounciness;
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

    void DebugBall()
    {
        return;
    }
}