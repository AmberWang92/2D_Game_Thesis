using TopDownShooter.Runtime.Enemies;
using UnityEngine;

namespace TopDownShooter.Data
{
    [CreateAssetMenu(menuName = "Top Down Shooter/Spawn Wave Config", fileName = "SpawnWaveConfig")]
    public sealed class SpawnWaveConfig : ScriptableObject
    {
        [field: SerializeField, Min(0f)] public float InitialDelay { get; private set; } = 1f;
        [field: SerializeField, Min(0f)] public float TimeBetweenSpawns { get; private set; } = 1f;
        [field: SerializeField, Min(0f)] public float DelayAfterWave { get; private set; } = 3f;
        [field: SerializeField] public SpawnGroup[] SpawnGroups { get; private set; }
    }

    [System.Serializable]
    public sealed class SpawnGroup
    {
        [field: SerializeField] public EnemyFacade EnemyPrefab { get; private set; }
        [field: SerializeField, Min(0)] public int Count { get; private set; } = 3;
    }
}
