using UnityEngine;
using UnityEngine.Events;

namespace TopDownShooter.Controllers.Game
{
    public class GameManager : MonoBehaviour
    {
        public enum GameState { StartMenu, Playing, GameOver }
        
        [Header("State")]
        public GameState CurrentState { get; private set; }

        [Header("Events")]
        public UnityEvent OnGameStarted;
        public UnityEvent OnGameOver;
        public UnityEvent<int> OnSurvivalTimeChanged;

        private float _survivalTime;
        private int _lastSecondRecorded;

        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            // Simple Singleton pattern so other systems (like spawners) can easily check the game state
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            CurrentState = GameState.Playing;
            _survivalTime = 0f;
            _lastSecondRecorded = 0;
            
            // Ensure time runs normally
            Time.timeScale = 1f;
            
            OnGameStarted?.Invoke();
            OnSurvivalTimeChanged?.Invoke(0);
        }

        private void Update()
        {
            if (CurrentState == GameState.Playing)
            {
                _survivalTime += Time.deltaTime;
                int currentSecond = Mathf.FloorToInt(_survivalTime);
                
                if (currentSecond > _lastSecondRecorded)
                {
                    _lastSecondRecorded = currentSecond;
                    OnSurvivalTimeChanged?.Invoke(currentSecond);
                }
            }
        }

        // Hook this method into the Player's HealthComponent OnDied UnityEvent
        public void TriggerGameOver()
        {
            if (CurrentState == GameState.GameOver) return;
            
            CurrentState = GameState.GameOver;
            OnGameOver?.Invoke();
            
            // Pause the game
            Time.timeScale = 0f; 
        }
    }
}
