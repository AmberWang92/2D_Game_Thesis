using System.Collections.Generic;
using TopDownShooter.Data;
using TopDownShooter.Game;
using TopDownShooter.Gameplay.Enemies;
using UnityEngine;

namespace TopDownShooter.Services
{
    /// <summary>
    /// Time-based endless spawner. For each <see cref="EnemyDefinitionSO"/> in
    /// <see cref="enemyPool"/> it owns an <see cref="ObjectPool{EnemyController}"/>
    /// and spawns instances on a ring just outside the camera viewport.
    /// Spawn interval and concurrency cap are sampled from animation curves keyed
    /// on <see cref="GameManager.ElapsedTime"/>, so difficulty scales over time.
    /// </summary>
    [DisallowMultipleComponent]
    public class SpawnDirector : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Used as fallback spawn center if the camera is missing.")]
        [SerializeField] private Transform player;
        [SerializeField] private Camera worldCamera;
        [Tooltip("Spawner only runs while this GameManager is in the Running state.")]
        [SerializeField] private GameManager game;

        [Header("Archetypes")]
        [SerializeField] private EnemyDefinitionSO[] enemyPool;
        [SerializeField, Min(0)] private int prewarmPerArchetype = 4;

        [Header("Difficulty (X = elapsed seconds)")]
        [Tooltip("Seconds between spawns sampled at ElapsedTime. Should decrease over time.")]
        [SerializeField] private AnimationCurve spawnIntervalOverTime =
            AnimationCurve.Linear(0f, 2.0f, 120f, 0.4f);

        [Tooltip("Maximum simultaneously alive enemies sampled at ElapsedTime. Should increase over time.")]
        [SerializeField] private AnimationCurve concurrentCapOverTime =
            AnimationCurve.Linear(0f, 4f, 120f, 24f);

        [Header("Spawn placement")]
        [Tooltip("Extra distance outside the camera frustum corners where enemies appear.")]
        [SerializeField, Min(0f)] private float spawnRingMargin = 2f;
        [Tooltip("Delay before the first spawn after the run starts, so the player can orient.")]
        [SerializeField, Min(0f)] private float warmupDelay = 1.5f;

        private readonly Dictionary<EnemyDefinitionSO, ObjectPool<EnemyController>> _pools = new();
        private int _aliveCount;
        private float _nextSpawnAt;

        private void Awake()
        {
            if (worldCamera == null) worldCamera = Camera.main;
            if (enemyPool == null) return;

            foreach (var def in enemyPool)
            {
                if (def == null || def.prefab == null) continue;
                GetOrCreatePool(def).Prewarm(prewarmPerArchetype);
            }
        }

        private void Update()
        {
            if (game != null && !game.IsRunning) return;
            float t = game != null ? game.ElapsedTime : Time.timeSinceLevelLoad;

            if (t < warmupDelay)
            {
                _nextSpawnAt = warmupDelay;
                return;
            }

            int cap = Mathf.Max(1, Mathf.RoundToInt(concurrentCapOverTime.Evaluate(t)));
            if (_aliveCount >= cap) return;

            if (t < _nextSpawnAt) return;
            _nextSpawnAt = t + Mathf.Max(0.05f, spawnIntervalOverTime.Evaluate(t));

            SpawnOne();
        }

        private void SpawnOne()
        {
            if (enemyPool == null || enemyPool.Length == 0) return;

            var def = enemyPool[Random.Range(0, enemyPool.Length)];
            if (def == null || def.prefab == null) return;

            var pool = GetOrCreatePool(def);
            var inst = pool.Get();
            inst.transform.SetPositionAndRotation(PickOffscreenPoint(), Quaternion.identity);
            _aliveCount++;
        }

        private Vector2 PickOffscreenPoint()
        {
            Vector2 center;
            float h, w;

            if (worldCamera != null)
            {
                center = worldCamera.transform.position;
                h = worldCamera.orthographic ? worldCamera.orthographicSize : 5f;
                w = h * worldCamera.aspect;
            }
            else
            {
                center = player != null ? (Vector2)player.position : Vector2.zero;
                h = 5f; w = 9f;
            }

            // Ring radius = corner distance + margin, so the spawn is always
            // outside the rectangular viewport regardless of angle.
            float dist = Mathf.Sqrt(w * w + h * h) + spawnRingMargin;
            float angle = Random.value * Mathf.PI * 2f;
            return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
        }

        private ObjectPool<EnemyController> GetOrCreatePool(EnemyDefinitionSO def)
        {
            if (_pools.TryGetValue(def, out var existing)) return existing;

            ObjectPool<EnemyController> pool = null;
            pool = new ObjectPool<EnemyController>(
                factory: () =>
                {
                    // Instantiate at scene root: enemies must NOT be parented under
                    // a transform that may move (mirrors the WeaponHolder constraint).
                    var go = Instantiate(def.prefab);
                    var ctrl = go.GetComponent<EnemyController>();
                    if (ctrl == null)
                    {
                        Debug.LogError($"SpawnDirector: '{def.name}'.prefab has no EnemyController.", this);
                        Destroy(go);
                        return null;
                    }
                    ctrl.gameObject.SetActive(false);
                    ctrl.Despawned += inst =>
                    {
                        _aliveCount = Mathf.Max(0, _aliveCount - 1);
                        pool.Release(inst);
                    };
                    return ctrl;
                },
                onGet: c => c.gameObject.SetActive(true),
                onRelease: c => c.gameObject.SetActive(false));

            _pools[def] = pool;
            return pool;
        }
    }
}
