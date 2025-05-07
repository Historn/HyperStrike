using UnityEngine;
using UnityEngine.UI;
using TMPro;

// UI Layer || Visual components display
public class PlayerView : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private GameObject leaderboardPanel;

    [Header("UI Match Display")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI localScoreText;
    [SerializeField] private TextMeshProUGUI visitantScoreText;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI characterNameText;

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
        //scoreText.text = "Score: " + player.Score;
        //characterNameText.text = "Character: " + player.CharacterName;
    }
}
