using UnityEngine;

enum AbilityType
{
    NORMAL,
    PASSIVE,
    ULTIMATE
}

enum EffectType
{
    AREA,
    PROJECTILE,
    RAYSHOT,
    TARGETED,
    MOVEMENT
}

enum CastType
{
    CHANNELED,
    UNSTOPPABLE,
}

public abstract class Ability : MonoBehaviour
{
    public string abilityName;
    public float damage;
    public float cooldownDuration = 5f;
    public float castTime = 0f;    // Time to cast the ability
    public float duration = 0f;    // Time the ability lasts, if applicable
    public bool isReady = true;    // Ability is ready when not on cooldown
    protected float cooldownTimer = 0f; // Timer to track cooldown
    AbilityType abilityType;
    EffectType effectType;
    CastType castType;

    // This method is called to trigger the ability
    public void UseAbility()
    {
        if (isReady)
        {
            StartCoroutine(CastAbility());
        }
        else
        {
            Debug.Log(abilityName + " is on cooldown.");
        }
    }

    // Cast the ability, with optional delay for castTime
    protected virtual System.Collections.IEnumerator CastAbility()
    {
        if (castTime > 0f)
        {
            Debug.Log("Casting " + abilityName + " for " + castTime + " seconds.");
            yield return new WaitForSeconds(castTime);
        }

        // Perform the actual ability logic
        ActivateAbility();

        // Start the cooldown after use
        StartCooldown();
    }

    // Ability logic is defined in derived classes
    protected abstract void ActivateAbility();

    // Cooldown management
    private void StartCooldown()
    {
        isReady = false;
        cooldownTimer = cooldownDuration;
        StartCoroutine(CooldownCoroutine());
    }

    private System.Collections.IEnumerator CooldownCoroutine()
    {
        while (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            yield return null;
        }
        isReady = true;
        Debug.Log(abilityName + " is ready again.");
    }

    // UI-related methods to check ability readiness or display cooldown timers
    public float GetCooldownRemaining()
    {
        return Mathf.Max(0f, cooldownTimer);
    }

    public bool IsAbilityReady()
    {
        return isReady;
    }
}