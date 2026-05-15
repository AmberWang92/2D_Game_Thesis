using System;
using TopDownShooter.Core.Events;
using TopDownShooter.Core.Health;
using UnityEngine;

namespace TopDownShooter.Gameplay.Combat
{
    /// <summary>
    /// MonoBehaviour adapter around the pure-C# <see cref="Core.Health.Health"/> model.
    /// Implements <see cref="IDamageable"/> so projectiles can damage it through
    /// a narrow interface, and optionally raises an SO event channel on death.
    /// </summary>
    [DisallowMultipleComponent]
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1)] private int initialMax = 10;

        [Tooltip("Optional channel raised when this entity dies (e.g. PlayerDiedChannel).")]
        [SerializeField] private VoidEventChannelSO diedChannel;

        private Health _health;

        public int Current => _health?.Current ?? 0;
        public int Max => _health?.Max ?? 0;
        public bool IsDead => _health == null || _health.IsDead;

        public event Action<int, int> Damaged
        {
            add { EnsureModel(); _health.Damaged += value; }
            remove { if (_health != null) _health.Damaged -= value; }
        }
        public event Action<int, int> Changed
        {
            add { EnsureModel(); _health.Changed += value; }
            remove { if (_health != null) _health.Changed -= value; }
        }
        public event Action Died
        {
            add { EnsureModel(); _health.Died += value; }
            remove { if (_health != null) _health.Died -= value; }
        }

        private void Awake() => EnsureModel();

        private void OnEnable()
        {
            // When pooled enemies are reused we want a fresh HP pool.
            if (_health != null) _health.Reset(_health.Max);
        }

        public void Initialize(int max)
        {
            if (_health == null) _health = new Health(max);
            else _health.Reset(max);
        }

        public void ApplyDamage(int amount)
        {
            EnsureModel();
            bool wasDead = _health.IsDead;
            _health.TakeDamage(amount);
            if (!wasDead && _health.IsDead) diedChannel?.Raise();
        }

        public void Heal(int amount)
        {
            EnsureModel();
            _health.Heal(amount);
        }

        private void EnsureModel()
        {
            if (_health == null) _health = new Health(initialMax);
        }
    }
}
