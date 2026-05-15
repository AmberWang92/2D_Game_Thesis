using TopDownShooter.Core.FSM;
using UnityEngine;

namespace TopDownShooter.Gameplay.Enemies.States
{
    /// <summary>Terminal: stops the body, disables colliders, schedules return-to-pool.</summary>
    public sealed class EnemyDeadState : IState<EnemyController>
    {
        public void Enter(EnemyController c)
        {
            c.Movement.Stop();
            foreach (var col in c.GetComponentsInChildren<Collider2D>())
                col.enabled = false;
            c.ScheduleDespawn();
        }

        public void Tick(EnemyController c, float dt) { }
        public void FixedTick(EnemyController c, float fdt) { }
        public void Exit(EnemyController c) { }
    }
}
