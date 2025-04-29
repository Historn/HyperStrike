using UnityEngine;
using System.Collections.Generic;

public class AbilityUser : MonoBehaviour
{
    [SerializeField] private List<AbilitySlot> abilitySlots = new List<AbilitySlot>();

    private Dictionary<Ability, float> cooldowns = new Dictionary<Ability, float>();
    private Ability currentlyActivatedAbility;
    private float currentCastTime;

    [System.Serializable]
    public class AbilitySlot
    {
        public Ability ability;
        public KeyCode activationKey;
    }

    private void Update()
    {
        // Handle cooldown timers
        List<Ability> finishedCooldowns = new List<Ability>();
        foreach (var cooldown in cooldowns)
        {
            cooldowns[cooldown.Key] -= Time.deltaTime;
            if (cooldowns[cooldown.Key] <= 0)
            {
                finishedCooldowns.Add(cooldown.Key);
            }
        }

        foreach (var ability in finishedCooldowns)
        {
            cooldowns.Remove(ability);
        }

        // Check for ability inputs if not currently casting
        if (currentlyActivatedAbility == null)
        {
            CheckAbilityInput();
        }
        else
        {
            // Update current ability if it's active
            currentCastTime -= Time.deltaTime;
            currentlyActivatedAbility.UpdateAbility(this);

            if (currentCastTime <= 0)
            {
                // Execute the ability when cast time completes
                currentlyActivatedAbility.ExecuteAbility(this);
                currentlyActivatedAbility.EndAbility(this);
                currentlyActivatedAbility = null;
            }
        }
    }

    private void CheckAbilityInput()
    {
        foreach (var abilitySlot in abilitySlots)
        {
            if (Input.GetKeyDown(abilitySlot.activationKey))
            {
                TryUseAbility(abilitySlot.ability);
            }
        }
    }

    public bool TryUseAbility(Ability ability)
    {
        // Check cooldown
        if (cooldowns.ContainsKey(ability))
        {
            Debug.Log($"Ability {ability.abilityName} on cooldown: {cooldowns[ability]:F1}s remaining");
            return false;
        }

        // Check if ability can be used
        if (!ability.CanUseAbility(this))
        {
            Debug.Log($"Cannot use ability {ability.abilityName} right now");
            return false;
        }

        // Start ability casting
        ability.InitiateAbility(this);

        // If ability has a cast time
        if (ability.castTime > 0)
        {
            currentlyActivatedAbility = ability;
            currentCastTime = ability.castTime;
        }
        else
        {
            // Instant cast
            ability.ExecuteAbility(this);
            ability.EndAbility(this);
        }

        // Set cooldown
        cooldowns[ability] = ability.cooldownDuration;

        return true;
    }

    // Additional helper methods and properties will go here
    // These might include character stats, resources, etc.
}