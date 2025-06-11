using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class HeatmapOptions : MonoBehaviour
{
    public GameObject settingsPanel;

    public CameraController cameraController;

    private bool isPanelActive = false;

    void Start()
    {
        settingsPanel.SetActive(false);
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
}