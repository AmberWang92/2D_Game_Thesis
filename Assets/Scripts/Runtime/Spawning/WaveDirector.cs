using System.Collections;
using TopDownShooter.Core.Events;
using TopDownShooter.Data;
using UnityEngine;

namespace TopDownShooter.Runtime.Spawning
{
    public sealed class WaveDirector : MonoBehaviour
    {
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private SpawnWaveConfig[] waves;
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private bool loopWaves;

        private Coroutine waveRoutine;
        private bool isRunning;

        public bool IsRunning => isRunning;

        private void Start()
        {
            if (playOnStart)
            {
                StartWaves();
            }
        }

        private void OnDisable()
        {
            StopWaves();
        }

        public void StartWaves()
        {
            if (isRunning || enemySpawner == null || waves == null || waves.Length == 0)
            {
                return;
            }

            isRunning = true;
            waveRoutine = StartCoroutine(RunWaves());
        }

        public void StopWaves()
        {
            if (waveRoutine != null)
            {
                StopCoroutine(waveRoutine);
                waveRoutine = null;
            }

            isRunning = false;
        }

        private IEnumerator RunWaves()
        {
            do
            {
                for (int waveIndex = 0; waveIndex < waves.Length; waveIndex++)
                {
                    SpawnWaveConfig wave = waves[waveIndex];

                    if (wave == null)
                    {
                        continue;
                    }

                    yield return new WaitForSeconds(wave.InitialDelay);
                    yield return SpawnWave(wave);
                    GameplayEventBus.Global.Publish(new WaveCompletedEvent(waveIndex));
                    yield return new WaitForSeconds(wave.DelayAfterWave);
                }
            }
            while (loopWaves);

            isRunning = false;
            waveRoutine = null;
        }

        private IEnumerator SpawnWave(SpawnWaveConfig wave)
        {
            SpawnGroup[] groups = wave.SpawnGroups;

            if (groups == null)
            {
                yield break;
            }

            for (int groupIndex = 0; groupIndex < groups.Length; groupIndex++)
            {
                SpawnGroup group = groups[groupIndex];

                if (group == null || group.EnemyPrefab == null)
                {
                    continue;
                }

                for (int count = 0; count < group.Count; count++)
                {
                    enemySpawner.SpawnEnemy(group.EnemyPrefab);
                    yield return new WaitForSeconds(wave.TimeBetweenSpawns);
                }
            }
        }
    }
}
