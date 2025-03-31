using System.Collections;
using System.Collections.Generic;
using UnityEditor.Playables;
using UnityEngine;

namespace HyperStrike
{
    [CreateAssetMenu(fileName = "New Character", menuName = "HyperStrike/Character")]
    public class Character : ScriptableObject
    {
        public string characterName;
        public int health;
        public float speed;
        public float basicDamage;

        public Ability ability_RMB;
        public Ability ability_LSHIFT;
        public Ability ability_E;
        public Ability ability_Q;
    }
}
