using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CameraSettingsMenu : MonoBehaviour
{
    public GameObject settingsPanel;
    public Slider sensitivitySlider;
    public Slider movementSpeedSlider;
    public Slider verticalSpeedSlider;

    public TextMeshProUGUI sensitivityValueText;
    public TextMeshProUGUI movementSpeedValueText;
    public TextMeshProUGUI verticalSpeedValueText;

    public CameraController cameraController;

    private bool isPanelActive = false;

    void Start()
    {
        settingsPanel.SetActive(false);

        // Configure slider ranges
        sensitivitySlider.minValue = 400f;
        sensitivitySlider.maxValue = 1400f;
        movementSpeedSlider.minValue = 5f;
        movementSpeedSlider.maxValue = 15f;
        verticalSpeedSlider.minValue = 0f;
        verticalSpeedSlider.maxValue = 10f;

        // Set default slider values to match CameraController settings
        sensitivitySlider.value = cameraController.mouseSensitivity;
        movementSpeedSlider.value = cameraController.movementSpeed;
        verticalSpeedSlider.value = cameraController.verticalSpeed;

        // Update value texts to display initial values
        sensitivityValueText.text = cameraController.mouseSensitivity.ToString("F1");
        movementSpeedValueText.text = cameraController.movementSpeed.ToString("F1");
        verticalSpeedValueText.text = cameraController.verticalSpeed.ToString("F1");

        // Add listeners to update CameraController settings and value texts when sliders are changed
        sensitivitySlider.onValueChanged.AddListener(UpdateSensitivity);
        movementSpeedSlider.onValueChanged.AddListener(UpdateMovementSpeed);
        verticalSpeedSlider.onValueChanged.AddListener(UpdateVerticalSpeed);
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
        // Disable interaction via Submit (space/enter keys) for the settings panel button
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

    private void UpdateSensitivity(float value)
    {
        cameraController.mouseSensitivity = value;
        sensitivityValueText.text = value.ToString("F1");
    }

    private void UpdateMovementSpeed(float value)
    {
        cameraController.movementSpeed = value;
        movementSpeedValueText.text = value.ToString("F1");
    }

    private void UpdateVerticalSpeed(float value)
    {
        cameraController.verticalSpeed = value;
        verticalSpeedValueText.text = value.ToString("F1");
    }
}