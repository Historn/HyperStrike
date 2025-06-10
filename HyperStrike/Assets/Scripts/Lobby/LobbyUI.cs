using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LobbyUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI initTimerText;

    public override void OnNetworkSpawn()
    {
        LobbyManager.Instance.currentWaitTime.OnValueChanged += UpdateLobbyTimerAsText;
    }

    private void UpdateLobbyTimerAsText(float previousValue, float newValue)
    {
        if (initTimerText != null)
        {
            int time = Mathf.FloorToInt(newValue);
            initTimerText.text = time.ToString();
            if (time <= 0) initTimerText.enabled = false;
            else initTimerText.enabled = true;
        }
    }
}
