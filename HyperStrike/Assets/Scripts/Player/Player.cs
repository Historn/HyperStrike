using UnityEngine;

// CLASS FOR VARIABLES AND FUNCTIONS - NOT ACTIONS/INPUTS
public class Player
{
    // Related to server user
    int uniqueID = 0;
    int currentLevel = 1;
    int currenntLevelProgress = 10000; // XP

    // Related to match
    public int Score { get; set; }
    public string CharacterName { get; set; }
    //int character = CharacterType.Tank / DPS / Healer;
    //[SerializeField] Ability[] abilities = null;
}
