using TopDownShooter.Runtime.Enemies;
using TopDownShooter.Runtime.Targeting;
using TopDownShooter.Runtime.Weapons;
using UnityEngine;

namespace TopDownShooter.Runtime.Spawning
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyFacade enemyPrefab;
        [SerializeField] private SpawnPointGroup spawnPoints;
        [SerializeField] private MonoBehaviour targetProviderBehaviour;
        [SerializeField] private MonoBehaviour projectileSpawnerBehaviour;
        [SerializeField, Min(0)] private int spawnOnStartCount = 3;
        [SerializeField, Min(0f)] private float spawnRadiusFallback = 8f;
        [SerializeField] private Transform enemyParent;

        private ITargetProvider targetProvider;
        private IProjectileSpawner projectileSpawner;

        private void Awake()
        {
            targetProvider = targetProviderBehaviour as ITargetProvider;
            projectileSpawner = projectileSpawnerBehaviour as IProjectileSpawner;
        }

        private void Start()
        {
            for (int i = 0; i < spawnOnStartCount; i++)
            {
                SpawnEnemy();
            }
        }

        public EnemyFacade SpawnEnemy()
        {
            if (enemyPrefab == null || targetProvider == null || projectileSpawner == null)
            {
                Debug.LogError($"{nameof(EnemySpawner)} is missing required dependencies.", this);
                return null;
            }

            Vector2 spawnPosition = GetSpawnPosition();
            EnemyFacade enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, enemyParent);
            enemy.Initialize(targetProvider, projectileSpawner);
            return enemy;
        }

        private Vector2 GetSpawnPosition()
        {
            if (spawnPoints != null)
            {
                return spawnPoints.GetRandomSpawnPoint().position;
            }

            Vector2 offset = Random.insideUnitCircle.normalized * spawnRadiusFallback;
            return (Vector2)transform.position + offset;
        }
    }
}
