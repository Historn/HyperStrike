using HyperStrike;
using System.Collections;
using UnityEngine;

public class Rocket : Projectile
{
    float explosionForce = 25f;
    float radius = 20f;
    [SerializeField] GameObject shootFX;
    [SerializeField] GameObject explosionFX;
    [SerializeField] Rigidbody rb;

    public override void OnNetworkSpawn()
    {
        SpawnParticles(shootFX, 0.3f);
        damage = 20.0f;
        speed = 30f;

        // Spawns from the player that shot
        rb = GetComponent<Rigidbody>();

        
        

        StartCoroutine(DestroyRocket());
    }

    protected override void OnNetworkPostSpawn()
    {
        if (IsServer)
        {
            if (rb != null)
            {
                Move();
            }
            else if (rb == null)
            {
                Debug.Log("Rigidbody Not Found!");
                Destroy(gameObject);

            } // In case the rigidbody is null

        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!enabled) return;

        if (other != null)
        {
            SpawnParticles(explosionFX, 3f, true, other);
            if (IsServer) Explode(other);
            Destroy(gameObject);
        }
    }

    public override void Move()
    {
        rb.AddForce(transform.forward * speed, ForceMode.Impulse);
    }

    void Explode(Collision other)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider collider in colliders)
        {
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = rb.position - transform.position;

                rb.AddForce(dir.normalized * explosionForce, ForceMode.Impulse);
            }
        }
    }
    
    public override void ApplyDamage(GameObject collidedGO) { }

    void SpawnParticles(GameObject particlesGO, float destructionTime = -1f, bool useImpactNormal = false, Collision other = null)
    {
        if (particlesGO != null)
        {
            if (!useImpactNormal)
            {
                Quaternion spawnRotation = Quaternion.LookRotation(transform.up, transform.forward);
                GameObject vfxInstance = Instantiate(particlesGO, transform.position, spawnRotation);
                Destroy(vfxInstance, destructionTime);
            }
            else
            {
                if (other == null) return;

                // Find the nearest axis to the impact normal
                ContactPoint contact = other.contacts[0];
                Vector3 impactNormal = contact.normal;
                Vector3 nearestAxis = FindNearestAxis(impactNormal);

                Quaternion spawnRotation = Quaternion.LookRotation(nearestAxis);
                GameObject vfxInstance = Instantiate(particlesGO, transform.position, spawnRotation);
                Destroy(vfxInstance, destructionTime);
            }
        }
    }

    private Vector3 FindNearestAxis(Vector3 normal)
    {
        // Compare the normal with the primary axes and choose the closest one
        Vector3[] axes = { Vector3.right, Vector3.up, Vector3.forward, -Vector3.right, -Vector3.up, -Vector3.forward };
        Vector3 nearest = axes[0];
        float maxDot = Vector3.Dot(normal, axes[0]);

        foreach (Vector3 axis in axes)
        {
            float dot = Vector3.Dot(normal, axis);
            if (dot > maxDot)
            {
                maxDot = dot;
                nearest = axis;
            }
        }

        return nearest;
    }

    IEnumerator DestroyRocket()
    {
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
    }
}
