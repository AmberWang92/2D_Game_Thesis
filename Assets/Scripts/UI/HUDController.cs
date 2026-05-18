using TMPro;
using TopDownShooter.Core.Events;
using TopDownShooter.Game;
using TopDownShooter.Gameplay.Combat;
using UnityEngine;

namespace TopDownShooter.UI
{
    /// <summary>
    /// In-run HUD: HP (subscribed directly to the player's HealthComponent),
    /// running score (from the score-changed channel) and elapsed time (queried
    /// from GameManager). Pure view — owns no game state.
    /// </summary>
    [DisallowMultipleComponent]
    public class HUDController : MonoBehaviour
    {
        [Header("Sources")]
        [SerializeField] private HealthComponent playerHealth;
        [SerializeField] private GameManager game;
        [SerializeField] private IntEventChannelSO scoreChangedChannel;

        [Header("Text targets")]
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text timeText;

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.Changed += OnHpChanged;
                OnHpChanged(playerHealth.Current, playerHealth.Max);
            }
            if (scoreChangedChannel != null) scoreChangedChannel.OnRaised += OnScoreChanged;
            OnScoreChanged(0);
        }

        private void OnDisable()
        {
            if (playerHealth != null) playerHealth.Changed -= OnHpChanged;
            if (scoreChangedChannel != null) scoreChangedChannel.OnRaised -= OnScoreChanged;
        }

        private void Update()
        {
            if (timeText == null || game == null) return;
            int total = Mathf.FloorToInt(game.ElapsedTime);
            timeText.text = $"{total / 60}:{total % 60:00}";
        }

        private void OnHpChanged(int current, int max)
        {
            if (hpText != null) hpText.text = $"HP {current}/{max}";
        }

        private void OnScoreChanged(int score)
        {
            if (scoreText != null) scoreText.text = $"Score {score}";
        }
    }
}
