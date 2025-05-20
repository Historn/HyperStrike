using Unity.Netcode;
using UnityEngine;

public abstract class Projectile : NetworkBehaviour
{
    protected float speed = 1f;
    protected float damage = 1f;

    public abstract void Move();
    public abstract void ApplyDamage(GameObject collidedGO);
}
