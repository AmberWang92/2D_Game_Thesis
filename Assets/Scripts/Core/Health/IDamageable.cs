namespace TopDownShooter.Core.Health
{
    /// <summary>
    /// Anything that can take damage. Components implement this so projectiles
    /// can resolve damage targets without knowing concrete types.
    /// </summary>
    public interface IDamageable
    {
        bool IsDead { get; }
        void ApplyDamage(int amount);
    }
}
