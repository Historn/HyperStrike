using UnityEngine;
using UnityEngine.UI;

// UI Layer || Visual components display
public class PlayerView : MonoBehaviour
{
    public Text scoreText;
    public Text characterNameText;

    public void UpdateView(Player player)
    {
        //scoreText.text = "Score: " + player.Score;
        //characterNameText.text = "Character: " + player.CharacterName;
    }
}
