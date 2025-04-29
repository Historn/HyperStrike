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

        public GameObject weaponProjectile;

        [Header("Character Abilities")]
        public Ability ability1;
        public Ability ability2;
        public Ability ability3;
    }
}
