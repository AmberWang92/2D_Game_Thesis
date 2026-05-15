using TopDownShooter.Core.FSM;
using UnityEngine;

namespace TopDownShooter.Gameplay.Enemies.States
{
    /// <summary>Shooter attack state: stops moving and fires the weapon while in range.
    /// Leaves to <see cref="EnemyChaseState"/> (with hysteresis) or
    /// <see cref="EnemyIdleState"/> if the target is lost.</summary>
    public sealed class EnemyAttackState : IState<EnemyController>
    {
        private const float ExitRangeMultiplier = 1.15f;

        public void Enter(EnemyController c)
        {
            c.Movement.DesiredVelocity = Vector2.zero;
        }

        public void Tick(EnemyController c, float dt)
        {
            var target = c.Sensor != null ? c.Sensor.Target : null;
            if (target == null)
            {
                c.ChangeState(new EnemyIdleState());
                return;
            }

            float exitRange = c.Definition.attackRange * ExitRangeMultiplier;
            Vector2 delta = (Vector2)target.position - (Vector2)c.transform.position;
            if (delta.sqrMagnitude > exitRange * exitRange)
            {
                c.ChangeState(new EnemyChaseState());
                return;
            }

            if (c.Weapon != null) c.Weapon.TryFire(c.AimDirection);
        }

        public void FixedTick(EnemyController c, float fdt) { }
        public void Exit(EnemyController c) { }
    }
}
