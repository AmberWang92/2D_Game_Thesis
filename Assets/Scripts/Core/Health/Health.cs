using System;

namespace TopDownShooter.Core.Health
{
    public sealed class Health
    {
        public event Action<int, int> Changed;
        public event Action<DamageInfo> Damaged;
        public event Action<DamageInfo> Died;

        public int Current { get; private set; }
        public int Max { get; }
        public bool IsAlive => Current > 0;

        public Health(int maxHealth)
        {
            if (maxHealth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHealth));
            }

            Max = maxHealth;
            Current = maxHealth;
        }

        public void ApplyDamage(DamageInfo damageInfo)
        {
            if (!IsAlive || damageInfo.Amount <= 0)
            {
                return;
            }

            Current = Math.Max(0, Current - damageInfo.Amount);
            Damaged?.Invoke(damageInfo);
            Changed?.Invoke(Current, Max);

            if (Current == 0)
            {
                Died?.Invoke(damageInfo);
            }
        }
    }
}
