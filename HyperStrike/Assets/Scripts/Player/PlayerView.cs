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

    private void Start()
    {
        
    }

    public void UpdateView(Player player)
    {
        //scoreText.text = "Score: " + player.Score;
        //characterNameText.text = "Character: " + player.CharacterName;
    }
    
    public void UpdateLeaderboard()
    {

    }
}
