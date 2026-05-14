using TopDownShooter.Core.StateMachine;
using UnityEngine;

namespace TopDownShooter.Runtime.Player.States
{
    public sealed class PlayerIdleState : IState<PlayerContext>
    {
        public void Enter(PlayerContext context)
        {
            context.Mover.Stop();
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
            context.Mover.Stop();
        }

        public void Exit(PlayerContext context)
        {
        }
    }
}
