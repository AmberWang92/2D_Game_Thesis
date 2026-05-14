using UnityEngine;

namespace TopDownShooter.Data
{
    [CreateAssetMenu(menuName = "Top Down Shooter/Player Config", fileName = "PlayerConfig")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [field: SerializeField, Min(1)] public int MaxHealth { get; private set; } = 10;
        [field: SerializeField, Min(0f)] public float MoveSpeed { get; private set; } = 6f;
        [field: SerializeField, Min(0f)] public float RotationSpeed { get; private set; } = 720f;
    }
}
