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
        public float sprintSpeed;
        public float basicDamage;

        [Header("Character Shoot Attack")]
        public GameObject projectilePrefab;
        public float shootCooldown;
        public float shootOffset; // Forward offset for the projectile spawn

        [Header("Character Abilities")]
        public Ability ability1;
        public Ability ability2;
        public Ability ability3;
    }
}
