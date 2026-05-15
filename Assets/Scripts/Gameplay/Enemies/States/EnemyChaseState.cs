using TopDownShooter.Core.FSM;
using TopDownShooter.Data;
using UnityEngine;

namespace TopDownShooter.Gameplay.Enemies.States
{
    /// <summary>Drives the enemy toward its target. Transitions:
    /// - Target lost → <see cref="EnemyIdleState"/>.
    /// - Shooter within attack range → <see cref="EnemyAttackState"/>.
    /// Chasers stay in this state and rely on a ContactDamageDealer for damage.
    /// </summary>
    public sealed class EnemyChaseState : IState<EnemyController>
    {
        public void Enter(EnemyController c) { }

        public void Tick(EnemyController c, float dt)
        {
            var target = c.Sensor != null ? c.Sensor.Target : null;
            if (target == null)
            {
                c.ChangeState(new EnemyIdleState());
                return;
            }

            if (c.Definition.behavior == EnemyBehavior.Shooter && c.Weapon != null)
            {
                float range = c.Definition.attackRange;
                Vector2 delta = (Vector2)target.position - (Vector2)c.transform.position;
                if (delta.sqrMagnitude <= range * range)
                    c.ChangeState(new EnemyAttackState());
            }
        }

        public void FixedTick(EnemyController c, float fdt)
        {
            var target = c.Sensor != null ? c.Sensor.Target : null;
            if (target == null)
            {
                c.Movement.DesiredVelocity = Vector2.zero;
                return;
            }
            Vector2 dir = ((Vector2)target.position - (Vector2)c.transform.position).normalized;
            c.Movement.DesiredVelocity = dir * c.Definition.moveSpeed;
        }

        public void Exit(EnemyController c)
        {
            c.Movement.DesiredVelocity = Vector2.zero;
        }
    }
}
