using Unity.Netcode;
using UnityEngine;

public class Bouncer : NetworkBehaviour
{
    [SerializeField] float force = 100f;
    public string[] allowedTags;
    private void OnCollisionEnter(Collision other)
    {
        if (other == null) return;
        
        // Check all the tags the object can impulse
        for (int i = 0; i < allowedTags.Length; i++)
        {
            if (other.gameObject.CompareTag(allowedTags[i]))
            {
                Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    Vector3 dir = rb.position - transform.position;
                    //Vector3 dir = other.GetContact(0).normal; // Player doesnt bounce

                    rb.AddForce(dir.normalized * force, ForceMode.Impulse);
                }
            }
        }
    }
}
