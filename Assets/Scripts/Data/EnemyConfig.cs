using UnityEngine;

namespace TopDownShooter.Data
{
    [CreateAssetMenu(menuName = "Top Down Shooter/Enemy Config", fileName = "EnemyConfig")]
    public sealed class EnemyConfig : ScriptableObject
    {
        [field: SerializeField, Min(1)] public int MaxHealth { get; private set; } = 3;
        [field: SerializeField, Min(0f)] public float MoveSpeed { get; private set; } = 3f;
        [field: SerializeField, Min(0f)] public float RotationSpeed { get; private set; } = 540f;
        [field: SerializeField, Min(0f)] public float AttackRange { get; private set; } = 6f;
        [field: SerializeField, Min(0f)] public float StoppingDistance { get; private set; } = 3f;
        [field: SerializeField, Min(0f)] public float SpawnDuration { get; private set; } = 0.25f;
        [field: SerializeField, Min(0)] public int ScoreValue { get; private set; } = 1;
    }
}
