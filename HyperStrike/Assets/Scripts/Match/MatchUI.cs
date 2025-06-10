using TMPro;
using Unity.Netcode;
using UnityEngine;

public class MatchUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI initTimerText;

    [SerializeField] private GameObject characterSelection;

    [Header("UI Match Display")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI localScoreText;
    [SerializeField] private TextMeshProUGUI visitantScoreText;

    public override void OnNetworkSpawn()
    {
        if (IsServer) MatchManager.Instance.OnDisplayCharacterSelection += DisplayCharacterSelectionRpc; // This invokwed by server

        if (!IsClient) return;

        MatchManager.Instance.currentWaitTime.OnValueChanged += UpdateWaitTimerText;
        MatchManager.Instance.currentMatchTime.OnValueChanged += UpdateMatchTimerAsText;

        MatchManager.Instance.localGoals.OnValueChanged += UpdateView;
        MatchManager.Instance.visitantGoals.OnValueChanged += UpdateView;
    }

    void UpdateWaitTimerText(float previous, float current)
    {
        if (initTimerText != null)
        {
            int time = Mathf.FloorToInt(MatchManager.Instance.GetCurrentWaitTime());
            initTimerText.text = time.ToString();
            if (time <= 0) initTimerText.enabled = false;
            else initTimerText.enabled = true;
        }
    }

    void UpdateMatchTimerAsText(float previous, float current)
    {
        int minutes = Mathf.FloorToInt(MatchManager.Instance.GetCurrentMatchTime() / 60f);
        int seconds = Mathf.FloorToInt(MatchManager.Instance.GetCurrentMatchTime() % 60f);

        string timeText = $"{minutes:D2}:{seconds:D2}";
        timerText.text = timeText;
    }

    [Rpc(SendTo.NotServer)]
    void DisplayCharacterSelectionRpc()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void UpdateView(int previous, int current)
    {
        localScoreText.text = MatchManager.Instance.localGoals.Value.ToString();
        visitantScoreText.text = MatchManager.Instance.visitantGoals.Value.ToString();
    }
}
