using UnityEngine;
using UnityEngine.UI;
using TMPro;

// UI Layer || Visual components display
public class PlayerView : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] GameObject leaderboardPanel;

    public TextMeshPro scoreText;
    public TextMeshPro characterNameText;

    private void Start()
    {
        //MatchManager.OnUpdateMatchScore += UpdateView;
    }

    public void UpdateView(Player player)
    {
        //scoreText.text = "Score: " + player.Score;
        //characterNameText.text = "Character: " + player.CharacterName;
    }
}
