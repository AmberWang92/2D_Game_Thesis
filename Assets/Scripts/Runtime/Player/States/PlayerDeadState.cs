using TopDownShooter.Core.StateMachine;

namespace TopDownShooter.Runtime.Player.States
{
    public sealed class PlayerDeadState : IState<PlayerContext>
    {
        public void Enter(PlayerContext context)
        {
            context.MarkDead();
            context.Mover.Stop();
        }

        public void Tick(PlayerContext context, float deltaTime)
        {
        }

        public void FixedTick(PlayerContext context, float fixedDeltaTime)
        {
            context.Mover.Stop();
        }

        public void Exit(PlayerContext context)
        {
        }
    }
}
