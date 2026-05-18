using System;
using TopDownShooter.Core.Events;
using TopDownShooter.Core.FSM;
using TopDownShooter.Game.States;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TopDownShooter.Game
{
    /// <summary>
    /// Survival-loop composition root. Owns a 2-state FSM (Running, GameOver),
    /// tracks elapsed in-run time, and brokers run lifecycle via SO channels so
    /// HUD/Spawner/Score don't need direct references to each other.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour
    {
        [Header("Inbound")]
        [Tooltip("Raised by PlayerController's HealthComponent when the player dies.")]
        [SerializeField] private VoidEventChannelSO playerDiedChannel;

        [Header("Outbound")]
        [SerializeField] private VoidEventChannelSO gameStartedChannel;
        [SerializeField] private VoidEventChannelSO gameOverChannel;

        [Header("Options")]
        [Tooltip("Auto-start the run on Start(). Disable if a menu/intro drives StartRun() manually.")]
        [SerializeField] private bool autoStart = true;

        private StateMachine<GameManager> _fsm;

        public float ElapsedTime { get; private set; }
        public bool IsRunning => _fsm?.Current is GameRunningState;
        public bool IsGameOver => _fsm?.Current is GameOverState;

        private void Awake()
        {
            _fsm = new StateMachine<GameManager>(this);
        }

        private void OnEnable()
        {
            if (playerDiedChannel != null) playerDiedChannel.OnRaised += HandlePlayerDied;
        }

        private void OnDisable()
        {
            if (playerDiedChannel != null) playerDiedChannel.OnRaised -= HandlePlayerDied;
        }

        private void Start()
        {
            if (autoStart) StartRun();
        }

        private void Update() => _fsm?.Tick(Time.deltaTime);

        public void StartRun()
        {
            ElapsedTime = 0f;
            _fsm.ChangeState(new GameRunningState());
            gameStartedChannel?.Raise();
        }

        public void EndRun()
        {
            if (IsGameOver) return;
            _fsm.ChangeState(new GameOverState());
            gameOverChannel?.Raise();
        }

        public void RestartScene()
        {
            int idx = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(idx);
        }

        /// <summary>Invoked by <see cref="GameRunningState"/> to advance the run clock.</summary>
        public void AdvanceTime(float dt) => ElapsedTime += dt;

        private void HandlePlayerDied() => EndRun();
    }
}
