using UnityEngine;
using UnityEngine.UI;

public class HeatmapToggleManager : MonoBehaviour
{
    [SerializeField] private HeatmapManager deathsHeatmapManager;
    [SerializeField] private HeatmapManager damageHeatmapManager;

    [SerializeField] private Toggle deathsToggle;
    [SerializeField] private Toggle damageToggle;

    private void Start()
    {
        deathsToggle.onValueChanged.AddListener(isOn => deathsHeatmapManager.SetCubeVisibility(isOn));
        damageToggle.onValueChanged.AddListener(isOn => damageHeatmapManager.SetCubeVisibility(isOn));

        deathsToggle.isOn = true;
        damageToggle.isOn = true;
    }
}
