namespace TopDownShooter.Core.StateMachine
{
    public sealed class StateMachine<TContext>
    {
        private readonly TContext context;

        public IState<TContext> CurrentState { get; private set; }

        public StateMachine(TContext context)
        {
            this.context = context;
        }

        public void ChangeState(IState<TContext> nextState)
        {
            if (ReferenceEquals(CurrentState, nextState))
            {
                return;
            }

            CurrentState?.Exit(context);
            CurrentState = nextState;
            CurrentState?.Enter(context);
        }

        public void Tick(float deltaTime)
        {
            CurrentState?.Tick(context, deltaTime);
        }

        public void FixedTick(float fixedDeltaTime)
        {
            CurrentState?.FixedTick(context, fixedDeltaTime);
        }
    }
}
