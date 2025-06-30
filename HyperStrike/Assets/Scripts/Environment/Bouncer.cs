using Unity.Netcode;
using UnityEngine;

public class Bouncer : NetworkBehaviour
{
    [SerializeField] float force = 100f;
    public string[] allowedTags;
    private void OnCollisionEnter(Collision other)
    {
        if (other == null && !IsServer) return;

        foreach (var tag in allowedTags)
        {
            if (other.gameObject.CompareTag(tag))
            {
                Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    Vector3 dir = rb.position - transform.position;
                    float finalForce = force;

                    if (tag == "Player")
                    {
                        finalForce *= 3f;
                        Debug.Log("Bouncer on Player");
                    }
                    rb.linearVelocity = Vector3.zero;
                    rb.AddForce(dir.normalized * finalForce, ForceMode.Impulse);
                }
            }
        }
    }
}
