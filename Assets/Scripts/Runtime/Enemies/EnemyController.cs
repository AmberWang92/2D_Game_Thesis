using TopDownShooter.Core.StateMachine;
using TopDownShooter.Data;
using TopDownShooter.Runtime.Enemies.States;
using TopDownShooter.Runtime.Targeting;
using TopDownShooter.Runtime.Weapons;
using UnityEngine;

namespace TopDownShooter.Runtime.Enemies
{
    public sealed class EnemyController
    {
        private readonly EnemyContext context;
        private readonly StateMachine<EnemyContext> stateMachine;
        private readonly EnemySpawnState spawnState = new EnemySpawnState();
        private readonly EnemyChaseState chaseState = new EnemyChaseState();
        private readonly EnemyAttackState attackState = new EnemyAttackState();
        private readonly EnemyDeadState deadState = new EnemyDeadState();

        public EnemyController(EnemyConfig config, IEnemyMover mover, ITargetProvider targetProvider, WeaponController weapon)
        {
            context = new EnemyContext(config, mover, targetProvider, weapon);
            stateMachine = new StateMachine<EnemyContext>(context);
            stateMachine.ChangeState(spawnState);
        }

        public void Tick(float deltaTime)
        {
            if (!context.IsAlive)
            {
                return;
            }

            if (stateMachine.CurrentState == spawnState)
            {
                stateMachine.Tick(deltaTime);

                if (spawnState.IsComplete)
                {
                    stateMachine.ChangeState(SelectCombatState());
                }

                return;
            }

            stateMachine.ChangeState(SelectCombatState());
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

        private IState<EnemyContext> SelectCombatState()
        {
            if (!context.TargetProvider.HasTarget)
            {
                return chaseState;
            }

            float distance = Vector2.Distance(context.Mover.Position, context.TargetProvider.Position);
            return distance <= context.Config.AttackRange ? attackState : chaseState;
        }
    }
}
