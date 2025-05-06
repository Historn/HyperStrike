using UnityEngine;

[CreateAssetMenu(fileName = "New Movement Ability", menuName = "HyperStrike/Movement Ability")]
public class MovementAbility : Ability
{
    [Header("Movement Settings")]
    public float dashDistance;
    public float dashSpeed;
    public bool canDamageEnemies;
    public float damageAmount;
    public LayerMask collisionLayers;

    private Vector3 dashDirection;
    private float dashProgress;

    public override bool CanUseAbility(AbilityHolder user)
    {
        // Check for obstacles, valid direction, etc.
        // Could trace the path to ensure it's valid
        return true;
    }

    public override void InitiateAbility(AbilityHolder user)
    {
        // Store the direction to dash in
        dashDirection = user.transform.forward;
        dashProgress = 0f;
    }

    public override void ExecuteAbility(AbilityHolder user)
    {
        // This will be called if it's an instant ability,
        // but for movement, we usually want to use UpdateAbility
    }

    public override void UpdateAbility(AbilityHolder user)
    {
        // Move the character over time during cast time
        CharacterController controller = user.GetComponent<CharacterController>();
        if (controller != null)
        {
            float distanceThisFrame = dashSpeed * Time.deltaTime;
            controller.Move(dashDirection * distanceThisFrame);

            dashProgress += distanceThisFrame;

            // Check for enemies to damage
            if (canDamageEnemies)
            {
                Collider[] hits = Physics.OverlapSphere(
                    user.transform.position,
                    1.0f,
                    collisionLayers
                );

                foreach (var hit in hits)
                {
                    // Apply damage or effects to hit objects
                    Player target = hit.GetComponent<Player>();
                    if (target != null)
                    {
                        Debug.Log("Hit a Player has to take damage!");
                        //target.TakeDamage(damageAmount);
                    }
                }
            }
        }
    }

    public override void EndAbility(AbilityHolder user)
    {
        // Apply ending effects, momentum reduction, etc.
    }
}
