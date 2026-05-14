namespace TopDownShooter.Core.StateMachine
{
    public interface IState<in TContext>
    {
        void Enter(TContext context);
        void Tick(TContext context, float deltaTime);
        void FixedTick(TContext context, float fixedDeltaTime);
        void Exit(TContext context);
    }
}
