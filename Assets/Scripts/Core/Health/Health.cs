using System;

namespace TopDownShooter.Core.Health
{
    /// <summary>
    /// Pure C# HP container. Raises events for HUD/FX hookup. No Unity dependency.
    /// </summary>
    public sealed class Health
    {
        public int Max { get; private set; }
        public int Current { get; private set; }
        public bool IsDead => Current <= 0;

        /// <summary>(damageAmount, currentHP)</summary>
        public event Action<int, int> Damaged;
        /// <summary>(healAmount, currentHP)</summary>
        public event Action<int, int> Healed;
        /// <summary>(currentHP, maxHP)</summary>
        public event Action<int, int> Changed;
        public event Action Died;

        public Health(int max)
        {
            if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max));
            Max = max;
            Current = max;
        }

        public void Reset(int max)
        {
            if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max));
            Max = max;
            Current = max;
            Changed?.Invoke(Current, Max);
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || IsDead) return;
            Current = Math.Max(0, Current - amount);
            Damaged?.Invoke(amount, Current);
            Changed?.Invoke(Current, Max);
            if (Current == 0) Died?.Invoke();
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || IsDead) return;
            Current = Math.Min(Max, Current + amount);
            Healed?.Invoke(amount, Current);
            Changed?.Invoke(Current, Max);
        }
    }
}
