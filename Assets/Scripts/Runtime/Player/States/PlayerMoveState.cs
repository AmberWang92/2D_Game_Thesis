using TopDownShooter.Core.StateMachine;

namespace TopDownShooter.Runtime.Player.States
{
    public sealed class PlayerMoveState : IState<PlayerContext>
    {
        public void Enter(PlayerContext context)
        {
        }

        public void Tick(PlayerContext context, float deltaTime)
        {
            if (context.Input.IsFireHeld)
            {
                context.Weapon.TryFire(context.Mover.Position, context.Input.AimWorldPosition);
            }
        }

        public void FixedTick(PlayerContext context, float fixedDeltaTime)
        {
            context.Mover.LookAt(context.Input.AimWorldPosition, context.Config.RotationSpeed);
            context.Mover.Move(context.Input.Move, context.Config.MoveSpeed);
        }

        public void Exit(PlayerContext context)
        {
            context.Mover.Stop();
        }
    }
}
