using UnityEngine;

namespace HyperStrike
{
    [CreateAssetMenu(fileName = "New Character", menuName = "HyperStrike/Character")]
    public class Character : ScriptableObject
    {
        [Header("Character Name")]
        public string characterName;

        [Header("Character Health")]
        public int health;
        public int maxHealth;

        [Header("Character Speeds")]
        public float speed;
        public float sprintSpeed;
        public float wallRunSpeed;
        public float maxSpeed;
        public float maxSlidingSpeed;

        [Header("Character Shoot Attack")]
        public GameObject projectilePrefab;
        public int shootDamage;
        public float shootCooldown;
        public float shootOffset; // Forward offset for the projectile spawn

        [Header("Character Melee Attack")]
        public int meleeDamage;
        public float meleeForce;
        public float meleeOffset;
    }
}
