using TopDownShooter.Core.StateMachine;

namespace TopDownShooter.Runtime.Enemies.States
{
    public sealed class EnemyAttackState : IState<EnemyContext>
    {
        public void Enter(EnemyContext context)
        {
            context.Mover.Stop();
        }

        public void Tick(EnemyContext context, float deltaTime)
        {
            if (context.TargetProvider.HasTarget)
            {
                context.Weapon.TryFire(context.Mover.Position, context.TargetProvider.Position);
            }
        }

        public void FixedTick(EnemyContext context, float fixedDeltaTime)
        {
            if (!context.TargetProvider.HasTarget)
            {
                context.Mover.Stop();
                return;
            }

            context.Mover.LookAt(context.TargetProvider.Position, context.Config.RotationSpeed);
            context.Mover.MoveToward(context.TargetProvider.Position, context.Config.MoveSpeed, context.Config.StoppingDistance);
        }

        public void Exit(EnemyContext context)
        {
            context.Mover.Stop();
        }
    }
}
