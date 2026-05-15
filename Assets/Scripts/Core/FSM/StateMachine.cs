using System;

namespace TopDownShooter.Core.FSM
{
    /// <summary>
    /// Minimal generic state machine. Holds the current state and forwards
    /// Tick/FixedTick callbacks to it. Knows nothing about Unity.
    /// </summary>
    public sealed class StateMachine<TContext>
    {
        private readonly TContext _context;

        public IState<TContext> Current { get; private set; }
        public event Action<IState<TContext>, IState<TContext>> StateChanged;

        public StateMachine(TContext context)
        {
            _context = context;
        }

        public void ChangeState(IState<TContext> next)
        {
            if (ReferenceEquals(Current, next)) return;

            var previous = Current;
            previous?.Exit(_context);
            Current = next;
            Current?.Enter(_context);
            StateChanged?.Invoke(previous, Current);
        }

        public void Tick(float deltaTime) => Current?.Tick(_context, deltaTime);
        public void FixedTick(float fixedDeltaTime) => Current?.FixedTick(_context, fixedDeltaTime);
    }
}
