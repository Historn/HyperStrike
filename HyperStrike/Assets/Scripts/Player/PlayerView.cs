using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.Netcode;
using System.Collections.Generic;

// UI Layer || Visual components display
public class PlayerView : NetworkBehaviour
{
    [SerializeField] private GameObject playerCanvas;

    [SerializeField, Header("UI Ability Slots")]
    private List<Ability> abilities = new List<Ability>();

    [SerializeField] private Image healthBar;
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private Image ability1CooldownFill;
    [SerializeField] private Image ability2CooldownFill;
    [SerializeField] private Image ultimateCooldownFill;


    Player player;

    public override void OnNetworkSpawn()
    {
        player = GetComponent<Player>();
    }

    private void Start()
    {
        if (NetworkManager.Singleton && (IsServer || !IsLocalPlayer)) { playerCanvas.SetActive(false); return; }

        if (player == null) { Debug.Log("Is Null Player"); return; }

        player.Health.OnValueChanged += UpdateHealthBarView;

        for (int i = 0; i < abilities.Count; i++)
        {
            if (abilities[i] == null) continue;

            switch (i)
            {
                case 0:
                    abilities[i].currentCooldownTime.OnValueChanged += OnAbility1CooldownChanged;
                    break;
                case 1:
                    abilities[i].currentCooldownTime.OnValueChanged += OnAbility2CooldownChanged;
                    break;
                case 2:
                    abilities[i].currentCooldownTime.OnValueChanged += OnUltimateCooldownChanged;
                    break;
                default:
                    break;
            }
        }
    }

    private void OnAbility1CooldownChanged(float previousValue, float newValue)
    {
        float maxCooldown = abilities[0].fullCooldown;

        float normalizedCooldown = Mathf.Clamp01(newValue / maxCooldown);

        ability1CooldownFill.fillAmount = normalizedCooldown;
    }

    private void OnAbility2CooldownChanged(float previousValue, float newValue)
    {
        float maxCooldown = abilities[1].fullCooldown;

        float normalizedCooldown = Mathf.Clamp01(newValue / maxCooldown);

        ability2CooldownFill.fillAmount = normalizedCooldown;
    }

    private void OnUltimateCooldownChanged(float previousValue, float newValue)
    {
        float maxCooldown = abilities[2].fullCooldown;

        float normalizedCooldown = Mathf.Clamp01(newValue / maxCooldown);

        ultimateCooldownFill.fillAmount = normalizedCooldown;
    }

    private void UpdateHealthBarView(int previousValue, int newValue)
    {
        

        float maxHealth = player.MaxHealth.Value;

        if (newValue < 0f) ShowDeathPanel(true);
        else if (newValue >= maxHealth) ShowDeathPanel(false);

        float normalizedHealth = Mathf.Clamp01(newValue / maxHealth);

        healthBar.fillAmount = normalizedHealth;
    }

    private void ShowDeathPanel(bool isDead)
    {
        if (deathPanel != null)
        {
            if (!isDead)
            {
                deathPanel.SetActive(false);
            }
            else
            {
                deathPanel.SetActive(true);
            }
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton && NetworkManager.Singleton.IsServer) return;

        if (player == null) return;

        player.Health.OnValueChanged -= UpdateHealthBarView;

        for (int i = 0; i < abilities.Count; i++)
        {
            if (abilities[i] == null) continue;

            switch (i)
            {
                case 0:
                    abilities[i].currentCooldownTime.OnValueChanged -= OnAbility1CooldownChanged;
                    break;
                case 1:
                    abilities[i].currentCooldownTime.OnValueChanged -= OnAbility2CooldownChanged;
                    break;
                case 2:
                    abilities[i].currentCooldownTime.OnValueChanged -= OnUltimateCooldownChanged;
                    break;
                default:
                    break;
            }
        }
    }
}
