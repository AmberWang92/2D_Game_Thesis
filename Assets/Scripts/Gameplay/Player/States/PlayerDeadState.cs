using TopDownShooter.Core.FSM;

namespace TopDownShooter.Gameplay.Player.States
{
    /// <summary>Terminal state: stops movement and ignores input until external reset.</summary>
    public sealed class PlayerDeadState : IState<PlayerController>
    {
        public void Enter(PlayerController c)
        {
            c.Movement.Stop();
        }

        public void Tick(PlayerController c, float dt) { }
        public void FixedTick(PlayerController c, float fdt) { }
        public void Exit(PlayerController c) { }
    }
}
