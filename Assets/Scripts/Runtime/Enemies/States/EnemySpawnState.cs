using TopDownShooter.Core.StateMachine;
using UnityEngine;

namespace TopDownShooter.Runtime.Enemies.States
{
    public sealed class EnemySpawnState : IState<EnemyContext>
    {
        private float remainingTime;

        public bool IsComplete => remainingTime <= 0f;

        public void Enter(EnemyContext context)
        {
            remainingTime = context.Config.SpawnDuration;
            context.Mover.Stop();
        }

        public void Tick(EnemyContext context, float deltaTime)
        {
            remainingTime -= deltaTime;
        }

        public void FixedTick(EnemyContext context, float fixedDeltaTime)
        {
            context.Mover.Stop();

            if (context.TargetProvider.HasTarget)
            {
                context.Mover.LookAt(context.TargetProvider.Position, context.Config.RotationSpeed);
            }
        }

        public void Exit(EnemyContext context)
        {
        }
    }
}
