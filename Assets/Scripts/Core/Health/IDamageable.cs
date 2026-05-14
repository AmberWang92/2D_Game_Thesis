namespace TopDownShooter.Core.Health
{
    public interface IDamageable
    {
        Team Team { get; }
        bool IsAlive { get; }
        void ApplyDamage(DamageInfo damageInfo);
    }
}
