using TopDownShooter.Data;
using TopDownShooter.Runtime.Targeting;
using TopDownShooter.Runtime.Weapons;

namespace TopDownShooter.Runtime.Enemies
{
    public sealed class EnemyContext
    {
        public EnemyConfig Config { get; }
        public IEnemyMover Mover { get; }
        public ITargetProvider TargetProvider { get; }
        public WeaponController Weapon { get; }
        public bool IsAlive { get; private set; } = true;

        public EnemyContext(EnemyConfig config, IEnemyMover mover, ITargetProvider targetProvider, WeaponController weapon)
        {
            Config = config;
            Mover = mover;
            TargetProvider = targetProvider;
            Weapon = weapon;
        }

        public void MarkDead()
        {
            IsAlive = false;
        }
    }
}
