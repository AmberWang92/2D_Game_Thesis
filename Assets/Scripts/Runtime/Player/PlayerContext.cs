using TopDownShooter.Data;
using TopDownShooter.Runtime.Weapons;

namespace TopDownShooter.Runtime.Player
{
    public sealed class PlayerContext
    {
        public PlayerConfig Config { get; }
        public IPlayerInput Input { get; }
        public IPlayerMover Mover { get; }
        public WeaponController Weapon { get; }
        public bool IsAlive { get; private set; } = true;

        public PlayerContext(PlayerConfig config, IPlayerInput input, IPlayerMover mover, WeaponController weapon)
        {
            Config = config;
            Input = input;
            Mover = mover;
            Weapon = weapon;
        }

        public void MarkDead()
        {
            IsAlive = false;
        }
    }
}
