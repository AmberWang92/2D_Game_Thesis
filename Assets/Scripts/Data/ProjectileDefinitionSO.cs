using UnityEngine;

namespace TopDownShooter.Data
{
    /// <summary>
    /// Data definition for a projectile: physics, lifetime, damage, what it can hit.
    /// The <see cref="prefab"/> must contain a Projectile component.
    /// </summary>
    [CreateAssetMenu(menuName = "TopDownShooter/Data/Projectile Definition", fileName = "ProjectileDefinition")]
    public class ProjectileDefinitionSO : ScriptableObject
    {
        [Tooltip("Prefab carrying a Projectile component.")]
        public GameObject prefab;

        [Min(1)] public int damage = 1;
        [Min(0.01f)] public float lifetime = 2f;

        [Tooltip("Layers the projectile is allowed to damage / be blocked by.")]
        public LayerMask hitMask = ~0;

        [Tooltip("Optional cosmetic colour for the placeholder sprite.")]
        public Color tint = Color.white;
    }
}
