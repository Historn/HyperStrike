using Unity.Netcode;
using UnityEngine;

public class StickExplosiveProjectile : ExplosiveProjectile
{
    float time = 50f;
    bool firstHit = true;
    private void OnCollisionEnter(Collision other)
    {
        if (!enabled && !IsServer) return;

        if (other != null)
        {
            NetworkObject netObj = other.gameObject.GetComponent<NetworkObject>();

            if (firstHit)
            {
                if (netObj && netObj.OwnerClientId != this.playerOwnerId)
                {
                    transform.SetParent(other.transform);
                    transform.position = other.GetContact(0).point;
                }
                else
                {
                    rigidBody.isKinematic = true;
                }
                firstHit = false;
            }

            time -= Time.deltaTime;
            if (time < 0) Explode(other);
        }
    }
}
