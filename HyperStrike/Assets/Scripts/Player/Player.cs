using HyperStrike;
using UnityEngine;

// CLASS FOR VARIABLES AND FUNCTIONS - NOT ACTIONS/INPUTS
public class Player
{
    public string PlayerName = "PlayerName";
    public ulong PlayerId = 0;
    public byte Team = 0;
    public int Score { get; set; }

    public int Goals { get; set; }

    public Character Character;
}
