using TMPro;
using Unity.Netcode;
using UnityEngine;

public class MatchUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI initTimerText;

    [SerializeField] private GameObject characterSelection;

    private void Awake()
    {
        if (IsOwner) NetworkManager.Singleton.OnClientConnectedCallback += _ => DisplayCharacterSelection();

        MatchManager.Instance.currentWaitTime.OnValueChanged += (previous, current) => UpdateWaitTimerText();
        MatchManager.Instance.currentMatchTime.OnValueChanged += (previous, current) => UpdateMatchTimerAsText();
    }

    void UpdateWaitTimerText()
    {
        if (initTimerText != null)
        {
            int time = Mathf.FloorToInt(MatchManager.Instance.GetCurrentWaitTime());
            initTimerText.text = time.ToString();
            if (time <= 0) initTimerText.enabled = false;
        }
    }

    string UpdateMatchTimerAsText()
    {
        int minutes = Mathf.FloorToInt(MatchManager.Instance.GetCurrentMatchTime() / 60f);
        int seconds = Mathf.FloorToInt(MatchManager.Instance.GetCurrentMatchTime() % 60f);

        string timeText = $"{minutes:D2}:{seconds:D2}";

        return timeText;
    }

    void DisplayCharacterSelection()
    {
        characterSelection.SetActive(true);
    }
}
