using Unity.Netcode;
using UnityEngine;

public abstract class Projectile : NetworkBehaviour
{
    public float speed = 1f;
    public int damage = 1;
    public ulong playerOwnerId = 0;

    public abstract void Move();
}
