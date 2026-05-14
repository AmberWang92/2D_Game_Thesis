using UnityEngine;

namespace TopDownShooter.Data
{
    [CreateAssetMenu(menuName = "Top Down Shooter/Projectile Config", fileName = "ProjectileConfig")]
    public sealed class ProjectileConfig : ScriptableObject
    {
        [field: SerializeField, Min(0f)] public float Speed { get; private set; } = 12f;
        [field: SerializeField, Min(1)] public int Damage { get; private set; } = 1;
        [field: SerializeField, Min(0.01f)] public float Lifetime { get; private set; } = 2f;
    }
}
