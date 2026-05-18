using TopDownShooter.Core.Events;
using UnityEngine;

namespace TopDownShooter.Services
{
    /// <summary>
    /// Listens for per-enemy score deltas on <see cref="enemyScoreChannel"/>, accumulates,
    /// and re-broadcasts the running total on <see cref="scoreChangedChannel"/> for the HUD.
    /// Optionally resets on <see cref="gameStartedChannel"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class ScoreService : MonoBehaviour
    {
        [Header("Inbound")]
        [Tooltip("Per-enemy score deltas raised by EnemyController on death.")]
        [SerializeField] private IntEventChannelSO enemyScoreChannel;
        [Tooltip("Optional: clears the total when a new run starts.")]
        [SerializeField] private VoidEventChannelSO gameStartedChannel;

        [Header("Outbound")]
        [Tooltip("Re-broadcasts the running total whenever it changes.")]
        [SerializeField] private IntEventChannelSO scoreChangedChannel;

        public int Score { get; private set; }

        private void OnEnable()
        {
            if (enemyScoreChannel != null) enemyScoreChannel.OnRaised += AddScore;
            if (gameStartedChannel != null) gameStartedChannel.OnRaised += ResetScore;
        }

        private void OnDisable()
        {
            if (enemyScoreChannel != null) enemyScoreChannel.OnRaised -= AddScore;
            if (gameStartedChannel != null) gameStartedChannel.OnRaised -= ResetScore;
        }

        public void AddScore(int delta)
        {
            if (delta <= 0) return;
            Score += delta;
            scoreChangedChannel?.Raise(Score);
        }

        public void ResetScore()
        {
            Score = 0;
            scoreChangedChannel?.Raise(0);
        }
    }
}
