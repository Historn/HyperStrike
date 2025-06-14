using Unity.Netcode;
using UnityEngine;

public abstract class Projectile : NetworkBehaviour
{
    protected float speed = 1f;
    protected int damage = 1;
    public ulong playerOwnerId = 0;

    public abstract void Move();
    public abstract void ApplyDamage(GameObject collidedGO);
}
