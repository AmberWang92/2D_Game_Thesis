using UnityEngine;
using TopDownShooter.Controllers.Game;

namespace TopDownShooter.Controllers.Enemy
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private float initialSpawnInterval = 2f;
        [SerializeField] private float minimumSpawnInterval = 0.5f;
        [SerializeField] private float difficultyRampRate = 0.05f; // How much the interval drops per spawn
        [SerializeField] private float spawnRadius = 15f; // Distance from the player to spawn

        private Transform _playerTransform;
        private float _spawnTimer;
        private float _currentInterval;

        private void Start()
        {
            _currentInterval = initialSpawnInterval;
            
            // Automatically find the player. 
            // In a more robust setup, the GameManager could broadcast the player's transform on start.
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                _playerTransform = playerObj.transform;
            }
        }

        private void Update()
        {
            // Only spawn if the game is actively playing
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                return;
            }

            if (_playerTransform == null) return;

            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= _currentInterval)
            {
                SpawnEnemy();
                _spawnTimer = 0f;
                
                // Gradually increase difficulty by reducing the spawn interval
                _currentInterval = Mathf.Max(minimumSpawnInterval, _currentInterval - difficultyRampRate);
            }
        }

        private void SpawnEnemy()
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

            // Pick a random angle to spawn an enemy in a circle outside the camera view
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector2 spawnDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 spawnPosition = (Vector2)_playerTransform.position + (spawnDirection * spawnRadius);

            // Pick a random enemy prefab
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Instantiate(prefab, spawnPosition, Quaternion.identity);
        }
        
        private void OnDrawGizmosSelected()
        {
            // Visualize the spawn radius in the editor
            if (_playerTransform != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(_playerTransform.position, spawnRadius);
            }
        }
    }
}
