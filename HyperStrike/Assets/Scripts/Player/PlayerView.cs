using UnityEngine;
using UnityEngine.UI;
using TMPro;

// UI Layer || Visual components display
public class PlayerView : MonoBehaviour
{
    [Header("UI Leaderboard")]
    [SerializeField] private GameObject leaderboardPanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI characterNameText;

    [Header("UI Match Display")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI localScoreText;
    [SerializeField] private TextMeshProUGUI visitantScoreText;


    private void Start()
    {
        if (MatchManager.Instance != null)
            MatchManager.Instance.OnUpdateMatchScore += UpdateView;
    }

    public void UpdateView(Player player)
    {
        //scoreText.text = "Score: " + player.Score;
        //characterNameText.text = "Character: " + player.CharacterName;
    }
    public void UpdateView()
    {
        localScoreText.text = MatchManager.Instance.localGoals.ToString();
        visitantScoreText.text = MatchManager.Instance.visitantGoals.ToString();
    }

    public void UpdateLeaderboard()
    {

    }
}
