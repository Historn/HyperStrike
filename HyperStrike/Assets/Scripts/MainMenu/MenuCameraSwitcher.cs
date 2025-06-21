using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class MenuCameraSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private CinemachineCamera mainMenuCamera;
    [SerializeField] private int activePriority = 15;
    [SerializeField] private int inactivePriority = -5;

    private void Update()
    {
        // Continuously ensure camera priority reflects the panel's visibility
        if (mainMenuCamera != null && mainMenuPanel != null)
        {
            mainMenuCamera.Priority = mainMenuPanel.activeInHierarchy ? activePriority : inactivePriority;
        }
    }
}
