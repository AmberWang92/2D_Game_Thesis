using UnityEngine;

namespace TopDownShooter.Data
{
    /// <summary>
    /// Data definition for a weapon: fire pattern + which projectile it spawns.
    /// </summary>
    [CreateAssetMenu(menuName = "TopDownShooter/Data/Weapon Definition", fileName = "WeaponDefinition")]
    public class WeaponDefinitionSO : ScriptableObject
    {
        public ProjectileDefinitionSO projectile;

        [Tooltip("Rounds per second.")]
        [Min(0.01f)] public float fireRate = 5f;

        [Tooltip("Muzzle speed applied to each projectile (units/sec).")]
        [Min(0f)] public float muzzleSpeed = 18f;

        [Tooltip("Number of projectiles per trigger pull.")]
        [Min(1)] public int projectilesPerShot = 1;

        [Tooltip("Half-angle (degrees) of random spread per projectile.")]
        [Range(0f, 45f)] public float spreadDegrees = 0f;

        [Tooltip("Initial pool size. Pool grows on demand.")]
        [Min(0)] public int poolPrewarm = 16;
    }
}
