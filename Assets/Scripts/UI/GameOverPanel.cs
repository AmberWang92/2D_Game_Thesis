using TMPro;
using TopDownShooter.Core.Events;
using TopDownShooter.Game;
using TopDownShooter.Services;
using UnityEngine;
using UnityEngine.UI;

namespace TopDownShooter.UI
{
    /// <summary>
    /// Shown when the gameOverChannel is raised. Displays final score + survived
    /// duration and exposes a Restart button that asks GameManager to reload the scene.
    /// Hidden by default at Awake.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameOverPanel : MonoBehaviour
    {
        [Header("Sources")]
        [SerializeField] private VoidEventChannelSO gameOverChannel;
        [SerializeField] private GameManager game;
        [SerializeField] private ScoreService score;

        [Header("View")]
        [Tooltip("Root GameObject toggled on/off. Usually this panel itself or a child container.")]
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private TMP_Text survivedText;
        [SerializeField] private Button restartButton;

        private void Awake()
        {
            // IMPORTANT: subscribe BEFORE we (potentially) deactivate our own
            // GameObject below. If `root` is this GameObject, SetActive(false)
            // would otherwise prevent OnEnable from ever running and the panel
            // would never be shown when the channel fires.
            if (gameOverChannel != null) gameOverChannel.OnRaised += Show;
            if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
            if (root != null) root.SetActive(false);
        }

        private void OnDestroy()
        {
            if (gameOverChannel != null) gameOverChannel.OnRaised -= Show;
            if (restartButton != null) restartButton.onClick.RemoveListener(OnRestartClicked);
        }

        private void Show()
        {
            if (root != null) root.SetActive(true);

            if (finalScoreText != null && score != null)
                finalScoreText.text = $"Score: {score.Score}";

            if (survivedText != null && game != null)
            {
                int total = Mathf.FloorToInt(game.ElapsedTime);
                survivedText.text = $"Survived: {total / 60}:{total % 60:00}";
            }
        }

        private void OnRestartClicked()
        {
            if (game != null) game.RestartScene();
        }
    }
}
