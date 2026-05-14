using TopDownShooter.Core.StateMachine;
using TopDownShooter.Data;
using TopDownShooter.Runtime.Player.States;
using TopDownShooter.Runtime.Weapons;

namespace TopDownShooter.Runtime.Player
{
    public sealed class PlayerController
    {
        private readonly PlayerContext context;
        private readonly StateMachine<PlayerContext> stateMachine;
        private readonly PlayerIdleState idleState = new PlayerIdleState();
        private readonly PlayerMoveState moveState = new PlayerMoveState();
        private readonly PlayerDeadState deadState = new PlayerDeadState();

        public PlayerController(PlayerConfig config, IPlayerInput input, IPlayerMover mover, WeaponController weapon)
        {
            context = new PlayerContext(config, input, mover, weapon);
            stateMachine = new StateMachine<PlayerContext>(context);
            stateMachine.ChangeState(idleState);
        }

        public void Tick(float deltaTime)
        {
            if (!context.IsAlive)
            {
                return;
            }

            stateMachine.ChangeState(context.Input.Move.sqrMagnitude > 0.001f ? moveState : idleState);
            stateMachine.Tick(deltaTime);
        }

        public void FixedTick(float fixedDeltaTime)
        {
            stateMachine.FixedTick(fixedDeltaTime);
        }

        public void Die()
        {
            stateMachine.ChangeState(deadState);
        }
    }
}
