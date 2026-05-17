using UnityEngine;
using UnityEngine.Events;
using TopDownShooter.Core.Interfaces;
using TopDownShooter.Data;

namespace TopDownShooter.Components
{
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [SerializeField] private CharacterStatsData stats;
        
        [Header("Events")]
        public UnityEvent<float, float> OnHealthChanged; // current, max
        public UnityEvent OnDamageTaken;
        public UnityEvent OnDied;

        private float _currentHealth;
        private bool _isDead;

        private void Start()
        {
            float maxHealth = stats != null ? stats.maxHealth : 100f;
            _currentHealth = maxHealth;
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        public void TakeDamage(float amount)
        {
            if (_isDead) return;

            _currentHealth -= amount;
            OnDamageTaken?.Invoke();
            OnHealthChanged?.Invoke(_currentHealth, stats != null ? stats.maxHealth : 100f);

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (_isDead) return;

            float max = stats != null ? stats.maxHealth : 100f;
            _currentHealth = Mathf.Min(_currentHealth + amount, max);
            OnHealthChanged?.Invoke(_currentHealth, max);
        }

        private void Die()
        {
            _isDead = true;
            OnDied?.Invoke();
        }
    }
}
