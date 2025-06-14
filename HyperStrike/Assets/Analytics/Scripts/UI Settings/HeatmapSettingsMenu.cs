using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class HeatmapSettingsMenu : MonoBehaviour
{
    public GameObject settingsPanel;
    public Slider cubeSizeSlider;
    public TextMeshProUGUI cubeSizeValueText;

    [Header("Heatmap Managers")]
    public HeatmapManager playerDeathsHeatmapManager;
    public HeatmapManager playerDamageHeatmapManager;
    public HeatmapManager enemiesDeathsHeatmapManager;

    [Header("Color Dropdowns")]
    public TMP_Dropdown playerDeathsColorDropdown;
    public TMP_Dropdown playerDamageColorDropdown;
    public TMP_Dropdown enemiesDeathsColorDropdown;

    [Header("Toggle Visibility")]
    public Toggle playerDeathsToggle;
    public Toggle playerDamageToggle;
    public Toggle enemiesDeathsToggle;

    public CameraController cameraController;

    private bool isPanelActive = false;

    private readonly Dictionary<string, Color> colorOptions = new Dictionary<string, Color>
    {
        { "Red", Color.red },
        { "Green", Color.green },
        { "Blue", Color.blue },
        { "Yellow", Color.yellow },
        { "Magenta", Color.magenta },
        { "Cyan", Color.cyan },
        { "Orange", new Color(1.0f, 0.5f, 0.0f) },
        { "Purple", new Color(0.5f, 0.0f, 0.5f) },
        { "White", Color.white },
        { "Black", Color.black }
    };

    void Start()
    {
        settingsPanel.SetActive(false);

        // Initialize color dropdowns
        InitializeColorDropdown(playerDeathsColorDropdown);
        InitializeColorDropdown(playerDamageColorDropdown);
        InitializeColorDropdown(enemiesDeathsColorDropdown);

        // Set slider initial value
        cubeSizeSlider.value = 0.5f;
        cubeSizeValueText.text = cubeSizeSlider.value.ToString("F2");

        // Add listeners to UI elements
        cubeSizeSlider.onValueChanged.AddListener(UpdateCubeSize);
        playerDeathsToggle.onValueChanged.AddListener(TogglePlayerDeathsVisibility);
        playerDamageToggle.onValueChanged.AddListener(TogglePlayerDamageVisibility);
        enemiesDeathsToggle.onValueChanged.AddListener(ToggleEnemiesDeathsVisibility);

        // Add listeners for color dropdowns
        playerDeathsColorDropdown.onValueChanged.AddListener(ChangePlayerDeathsColor);
        playerDamageColorDropdown.onValueChanged.AddListener(ChangePlayerDamageColor);
        enemiesDeathsColorDropdown.onValueChanged.AddListener(ChangeEnemiesDeathsColor);

        // Set initial cube sizes
        playerDeathsHeatmapManager.SetCubeSizeMultiplier(0.5f);
        playerDamageHeatmapManager.SetCubeSizeMultiplier(0.5f);
        enemiesDeathsHeatmapManager.SetCubeSizeMultiplier(0.5f);

        // Initialize cube colors based on dropdown defaults
        InitializeCubeColors();
    }

    void Update()
    {
        if (isPanelActive)
        {
            cameraController.DisableInput();
        }
        else
        {
            cameraController.EnableInput();
        }
    }

    public void ToggleSettingsPanel()
    {
        EventSystem.current.SetSelectedGameObject(null);

        isPanelActive = !isPanelActive;
        settingsPanel.SetActive(isPanelActive);

        if (isPanelActive)
        {
            cameraController.DisableInput();
        }
        else
        {
            cameraController.EnableInput();
        }
    }

    private void InitializeCubeColors()
    {
        ChangePlayerDeathsColor(playerDeathsColorDropdown.value);
        ChangePlayerDamageColor(playerDamageColorDropdown.value);
        ChangeEnemiesDeathsColor(enemiesDeathsColorDropdown.value);
    }

    private void InitializeColorDropdown(TMP_Dropdown dropdown)
    {
        dropdown.options.Clear();
        foreach (var colorName in colorOptions.Keys)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(colorName));
        }
        dropdown.value = 0;
        dropdown.RefreshShownValue();
    }

    private void ChangePlayerDeathsColor(int index)
    {
        playerDeathsHeatmapManager?.SetCubeColor(GetColorFromDropdownIndex(index));
    }

    private void ChangePlayerDamageColor(int index)
    {
        playerDamageHeatmapManager?.SetCubeColor(GetColorFromDropdownIndex(index));
    }

    private void ChangeEnemiesDeathsColor(int index)
    {
        enemiesDeathsHeatmapManager?.SetCubeColor(GetColorFromDropdownIndex(index));
    }

    private Color GetColorFromDropdownIndex(int index)
    {
        string colorName = playerDeathsColorDropdown.options[index].text;
        return colorOptions.ContainsKey(colorName) ? colorOptions[colorName] : Color.white;
    }

    private void UpdateCubeSize(float value)
    {
        Debug.Log($"Cube Size Multiplier Changed: {value}");

        // Update cube sizes for both heatmap managers
        playerDeathsHeatmapManager?.SetCubeSizeMultiplier(value);
        playerDamageHeatmapManager?.SetCubeSizeMultiplier(value);
        enemiesDeathsHeatmapManager?.SetCubeSizeMultiplier(value);

        // Update the slider value text
        cubeSizeValueText.text = value.ToString("F2");
    }

    private void TogglePlayerDeathsVisibility(bool isVisible)
    {
        Debug.Log($"Toggle Player Deaths Visibility: {isVisible}");
        playerDeathsHeatmapManager?.SetCubeVisibility(isVisible);
    }

    private void TogglePlayerDamageVisibility(bool isVisible)
    {
        Debug.Log($"Toggle Player Damage Visibility: {isVisible}");
        playerDamageHeatmapManager?.SetCubeVisibility(isVisible);
    }

    private void ToggleEnemiesDeathsVisibility(bool isVisible)
    {
        Debug.Log($"Toggle Enemies Deaths Visibility: {isVisible}");
        enemiesDeathsHeatmapManager?.SetCubeVisibility(isVisible);
    }
}