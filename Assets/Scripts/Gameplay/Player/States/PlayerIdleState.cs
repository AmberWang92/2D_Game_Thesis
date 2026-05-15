using TopDownShooter.Core.FSM;
using UnityEngine;

namespace TopDownShooter.Gameplay.Player.States
{
    /// <summary>Player is alive but not moving. Still aims and fires.</summary>
    public sealed class PlayerIdleState : IState<PlayerController>
    {
        private const float MoveThreshold = 0.01f;

        public void Enter(PlayerController c)
        {
            c.Movement.DesiredVelocity = Vector2.zero;
        }

        public void Tick(PlayerController c, float dt)
        {
            c.HandleCombat();
            if (c.Input != null && c.Input.Move.sqrMagnitude > MoveThreshold)
                c.ChangeState(new PlayerMoveState());
        }

        public void FixedTick(PlayerController c, float fdt) { }
        public void Exit(PlayerController c) { }
    }
}
