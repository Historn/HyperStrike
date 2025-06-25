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

    [Header("Win Loose Display")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject loosePanel;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            MatchManager.Instance.OnDisplayCharacterSelection += DisplayCharacterSelectionRpc; // This invokwed by server
            MatchManager.Instance.OnMatchEnded += DisplayWinLooseRpc;
        }

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
        int minutes = Mathf.FloorToInt(current / 60f);
        int seconds = Mathf.FloorToInt(current % 60f);

        string timeText = $"{minutes:D2}:{seconds:D2}";

        if (current <= 0.0f) timerText.text = "OVERTIME";
        else timerText.text = timeText;
    }

    [Rpc(SendTo.NotServer)]
    void DisplayCharacterSelectionRpc()
    {
        characterSelection.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    [Rpc(SendTo.NotServer)]
    private void DisplayWinLooseRpc(Team team)
    {
        if (NetworkManager.Singleton.LocalClient.PlayerObject.TryGetComponent<Player>(out Player player))
        {
            Debug.Log($"Win {player.Team.Value} == {team}");
            if (player.Team.Value == team)
            {
                winPanel.SetActive(true);
            }
            else
            {
                loosePanel.SetActive(true);
            }
        }
    }


    public void UpdateView(int previous, int current)
    {
        localScoreText.text = MatchManager.Instance.localGoals.Value.ToString();
        visitantScoreText.text = MatchManager.Instance.visitantGoals.Value.ToString();
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            MatchManager.Instance.OnDisplayCharacterSelection -= DisplayCharacterSelectionRpc; // This invokwed by server
            MatchManager.Instance.OnMatchEnded -= DisplayWinLooseRpc;
        }

        if (!IsClient) return;

        MatchManager.Instance.currentWaitTime.OnValueChanged -= UpdateWaitTimerText;
        MatchManager.Instance.currentMatchTime.OnValueChanged -= UpdateMatchTimerAsText;

        MatchManager.Instance.localGoals.OnValueChanged -= UpdateView;
        MatchManager.Instance.visitantGoals.OnValueChanged -= UpdateView;
    }
}
