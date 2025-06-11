using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CameraManagerTMP : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Dropdown cameraDropdown;
    public TMP_Text currentCameraLabel;
    public GameObject specialActionObject;

    [Header("Cameras")]
    public List<Camera> cameras = new List<Camera>();

    void Start()
    {
        if (cameraDropdown == null)
        {
            Debug.LogError("Camera Dropdown is not assigned.");
            return;
        }

        if (currentCameraLabel == null)
        {
            Debug.LogError("Current Camera Label is not assigned.");
            return;
        }

        if (specialActionObject == null)
        {
            Debug.LogError("Special Action Object is not assigned.");
            return;
        }

        if (cameras == null || cameras.Count == 0)
        {
            Debug.LogError("No cameras assigned to the script.");
            return;
        }

        PopulateDropdown();
        ActivateCamera(0);

        cameraDropdown.onValueChanged.AddListener(OnCameraSelected);

        specialActionObject.SetActive(false);
    }

    void PopulateDropdown()
    {
        List<string> cameraNames = new List<string>();

        foreach (Camera cam in cameras)
        {
            if (cam != null)
            {
                cameraNames.Add(cam.gameObject.name);
            }
            else
            {
                Debug.LogWarning("A null camera is assigned in the list.");
            }
        }

        cameraDropdown.ClearOptions();
        cameraDropdown.AddOptions(cameraNames);
    }

    void OnCameraSelected(int index)
    {
        ActivateCamera(index);
    }

    void ActivateCamera(int index)
    {
        for (int i = 0; i < cameras.Count; i++)
        {
            if (cameras[i] != null)
            {
                cameras[i].gameObject.SetActive(i == index);
            }
        }

        string activeCameraName = cameras[index].gameObject.name;
        UpdateCurrentCameraLabel(activeCameraName);

        specialActionObject.SetActive(index == 5);
    }

    void UpdateCurrentCameraLabel(string cameraName)
    {
        if (currentCameraLabel != null)
        {
            currentCameraLabel.text = $"Current Camera: {cameraName}";
        }
    }
}