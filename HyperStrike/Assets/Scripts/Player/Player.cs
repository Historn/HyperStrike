using UnityEngine;

// CLASS FOR VARIABLES AND FUNCTIONS - NOT ACTIONS/INPUTS
public class Player : MonoBehaviour
{

    // Related to server user
    int uniqueID = 0;
    int currentLevel = 1;
    int currenntLevelProgress = 10000; // XP

    // Related to match
    int score = 0;
    //int character = CharacterType.Tank / DPS / Healer;
    //[SerializeField] Ability[] abilities = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
