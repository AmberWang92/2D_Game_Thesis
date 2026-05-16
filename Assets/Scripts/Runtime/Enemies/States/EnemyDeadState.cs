using TopDownShooter.Core.StateMachine;

namespace TopDownShooter.Runtime.Enemies.States
{
    public sealed class EnemyDeadState : IState<EnemyContext>
    {
        public void Enter(EnemyContext context)
        {
            context.MarkDead();
            context.Mover.Stop();
        }

        public void Tick(EnemyContext context, float deltaTime)
        {
        }

        public void FixedTick(EnemyContext context, float fixedDeltaTime)
        {
            context.Mover.Stop();
        }

        public void Exit(EnemyContext context)
        {
        }
    }
}
