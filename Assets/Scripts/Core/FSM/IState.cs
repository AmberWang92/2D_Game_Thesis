namespace TopDownShooter.Core.FSM
{
    /// <summary>
    /// Generic state interface used by <see cref="StateMachine{TContext}"/>.
    /// States are pure C# objects and receive their owning context on every callback,
    /// keeping per-state allocations down and avoiding per-state context fields.
    /// </summary>
    public interface IState<TContext>
    {
        void Enter(TContext context);
        void Tick(TContext context, float deltaTime);
        void FixedTick(TContext context, float fixedDeltaTime);
        void Exit(TContext context);
    }
}
