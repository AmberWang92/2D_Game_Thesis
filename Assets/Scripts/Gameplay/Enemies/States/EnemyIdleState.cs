using TopDownShooter.Core.FSM;
using UnityEngine;

namespace TopDownShooter.Gameplay.Enemies.States
{
    /// <summary>No target visible. Holds position until the sensor finds one.</summary>
    public sealed class EnemyIdleState : IState<EnemyController>
    {
        public void Enter(EnemyController c)
        {
            c.Movement.DesiredVelocity = Vector2.zero;
        }

        public void Tick(EnemyController c, float dt)
        {
            if (c.Sensor != null && c.Sensor.Target != null)
                c.ChangeState(new EnemyChaseState());
        }

        public void FixedTick(EnemyController c, float fdt) { }
        public void Exit(EnemyController c) { }
    }
}
