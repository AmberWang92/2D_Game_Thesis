using TopDownShooter.Core.StateMachine;
using UnityEngine;

namespace TopDownShooter.Runtime.Enemies.States
{
    public sealed class EnemyChaseState : IState<EnemyContext>
    {
        public void Enter(EnemyContext context)
        {
        }

        public void Tick(EnemyContext context, float deltaTime)
        {
        }

        public void FixedTick(EnemyContext context, float fixedDeltaTime)
        {
            if (!context.TargetProvider.HasTarget)
            {
                context.Mover.Stop();
                return;
            }

            Vector2 targetPosition = context.TargetProvider.Position;
            context.Mover.LookAt(targetPosition, context.Config.RotationSpeed);
            context.Mover.MoveToward(targetPosition, context.Config.MoveSpeed, context.Config.StoppingDistance);
        }

        public void Exit(EnemyContext context)
        {
            context.Mover.Stop();
        }
    }
}
