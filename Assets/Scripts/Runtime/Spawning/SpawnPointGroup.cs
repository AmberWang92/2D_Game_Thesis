using UnityEngine;

namespace TopDownShooter.Runtime.Spawning
{
    public sealed class SpawnPointGroup : MonoBehaviour
    {
        [SerializeField] private Transform[] spawnPoints;

        public Transform GetRandomSpawnPoint()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                return transform;
            }

            return spawnPoints[Random.Range(0, spawnPoints.Length)];
        }
    }
}
