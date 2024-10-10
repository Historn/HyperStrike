using UnityEngine;

public class DashAbility : Ability
{
    public float dashDistance = 10f;
    public float dashSpeed = 20f;

    protected override void ActivateAbility()
    {
        Vector3 dashDirection = transform.forward * dashDistance;
        transform.position += dashDirection * dashSpeed * Time.deltaTime;

        Debug.Log(abilityName + " used! Dashing forward.");
    }
}