using UnityEngine;

namespace TopDownShooter.Gameplay.Movement
{
    /// <summary>
    /// Thin Rigidbody2D adapter. Other systems set <see cref="DesiredVelocity"/>;
    /// this component drives the physics body in FixedUpdate. No input or AI logic here.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public class MovementComponent2D : MonoBehaviour
    {
        private Rigidbody2D _rb;

        public Vector2 DesiredVelocity { get; set; }
        public Vector2 CurrentVelocity => _rb ? _rb.linearVelocity : Vector2.zero;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
        }

        private void FixedUpdate()
        {
            // Unity 6: linearVelocity replaces deprecated velocity.
            _rb.linearVelocity = DesiredVelocity;
        }

        public void Stop()
        {
            DesiredVelocity = Vector2.zero;
            if (_rb) _rb.linearVelocity = Vector2.zero;
        }
    }
}
