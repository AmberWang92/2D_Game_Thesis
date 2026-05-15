using TopDownShooter.Core.FSM;
using UnityEngine;

namespace TopDownShooter.Gameplay.Player.States
{
    /// <summary>Player is alive and translating. Drives MovementComponent2D from input.</summary>
    public sealed class PlayerMoveState : IState<PlayerController>
    {
        private const float MoveThreshold = 0.01f;

        public void Enter(PlayerController c) { }

        public void Tick(PlayerController c, float dt)
        {
            c.HandleCombat();
            if (c.Input == null || c.Input.Move.sqrMagnitude <= MoveThreshold)
                c.ChangeState(new PlayerIdleState());
        }

        public void FixedTick(PlayerController c, float fdt)
        {
            Vector2 input = c.Input != null ? c.Input.Move : Vector2.zero;
            // Clamp magnitude so diagonals aren't faster than cardinals (gamepads provide pre-clamped vectors; keyboard does not).
            if (input.sqrMagnitude > 1f) input.Normalize();
            float speed = c.Stats != null ? c.Stats.moveSpeed : 0f;
            c.Movement.DesiredVelocity = input * speed;
        }

        public void Exit(PlayerController c)
        {
            c.Movement.DesiredVelocity = Vector2.zero;
        }
    }
}
