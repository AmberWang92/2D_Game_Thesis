using UnityEngine;

namespace TopDownShooter.Data
{
    public enum EnemyBehavior
    {
        /// <summary>Closes range and deals damage on physical contact.</summary>
        Chaser,
        /// <summary>Stops at <see cref="EnemyDefinitionSO.attackRange"/> and fires a weapon.</summary>
        Shooter
    }

    /// <summary>
    /// Data definition for an enemy archetype. The <see cref="prefab"/> must carry an
    /// EnemyController; the spawner reads from this asset both to instantiate and to
    /// tune runtime values (HP, speed, sensor range, contact/weapon parameters).
    /// </summary>
    [CreateAssetMenu(menuName = "TopDownShooter/Data/Enemy Definition", fileName = "EnemyDefinition")]
    public class EnemyDefinitionSO : ScriptableObject
    {
        [Tooltip("Prefab carrying an EnemyController + supporting components.")]
        public GameObject prefab;

        public EnemyBehavior behavior = EnemyBehavior.Chaser;

        [Min(0.1f)] public float moveSpeed = 3f;
        [Min(1)] public int maxHP = 3;
        [Min(0.1f)] public float detectionRadius = 10f;

        [Tooltip("Distance at which a Shooter stops to fire. Unused for Chasers.")]
        [Min(0.1f)] public float attackRange = 6f;

        [Header("Chaser")]
        [Min(0)] public int contactDamage = 1;
        [Tooltip("Seconds between successive contact-damage applications to the same target.")]
        [Min(0.05f)] public float contactInterval = 0.5f;

        [Header("Shooter")]
        public WeaponDefinitionSO weapon;

        [Header("Scoring (M3)")]
        [Min(0)] public int scoreValue = 10;

        [Header("Cosmetic")]
        public Color tint = new Color(0.9f, 0.25f, 0.25f, 1f);
    }
}
