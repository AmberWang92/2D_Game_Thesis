using TopDownShooter.Core.Events;
using UnityEngine;

namespace TopDownShooter.Runtime.Session
{
    public sealed class GameSessionController : MonoBehaviour
    {
        [SerializeField] private bool startOnAwake = true;
        [SerializeField, Min(0.01f)] private float statsPublishInterval = 0.25f;

        private float survivalTime;
        private float nextStatsPublishTime;
        private int kills;
        private int score;

        public GameSessionState State { get; private set; } = GameSessionState.NotStarted;
        public GameSessionStats Stats => new GameSessionStats(survivalTime, kills, score);

        private void Awake()
        {
            GameplayEventBus.Global.Subscribe<PlayerDiedEvent>(HandlePlayerDied);
            GameplayEventBus.Global.Subscribe<EnemyDiedEvent>(HandleEnemyDied);

            if (startOnAwake)
            {
                StartSession();
            }
        }

        private void OnDestroy()
        {
            GameplayEventBus.Global.Unsubscribe<PlayerDiedEvent>(HandlePlayerDied);
            GameplayEventBus.Global.Unsubscribe<EnemyDiedEvent>(HandleEnemyDied);
        }

        private void Update()
        {
            if (State != GameSessionState.Playing)
            {
                return;
            }

            survivalTime += Time.deltaTime;

            if (Time.time >= nextStatsPublishTime)
            {
                PublishStats();
                nextStatsPublishTime = Time.time + statsPublishInterval;
            }
        }

        public void StartSession()
        {
            survivalTime = 0f;
            kills = 0;
            score = 0;
            State = GameSessionState.Playing;
            nextStatsPublishTime = Time.time;
            PublishStats();
        }

        private void HandleEnemyDied(EnemyDiedEvent enemyDiedEvent)
        {
            if (State != GameSessionState.Playing)
            {
                return;
            }

            kills++;
            score += enemyDiedEvent.ScoreValue;
            PublishStats();
        }

        private void HandlePlayerDied(PlayerDiedEvent playerDiedEvent)
        {
            if (State != GameSessionState.Playing)
            {
                return;
            }

            State = GameSessionState.GameOver;
            PublishStats();
            GameplayEventBus.Global.Publish(new GameOverEvent(survivalTime, kills, score));
        }

        private void PublishStats()
        {
            GameplayEventBus.Global.Publish(new SessionStatsChangedEvent(survivalTime, kills, score));
        }
    }
}
