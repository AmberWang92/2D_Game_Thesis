using UnityEngine;

namespace TopDownShooter.Data
{
    [CreateAssetMenu(menuName = "Top Down Shooter/Weapon Config", fileName = "WeaponConfig")]
    public sealed class WeaponConfig : ScriptableObject
    {
        [field: SerializeField] public ProjectileConfig ProjectileConfig { get; private set; }
        [field: SerializeField, Min(0.01f)] public float FireRate { get; private set; } = 6f;
        [field: SerializeField, Min(0f)] public float MuzzleOffset { get; private set; } = 0.5f;
    }
}
