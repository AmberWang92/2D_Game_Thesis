using UnityEngine;

namespace TopDownShooter.Components
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovementComponent : MonoBehaviour
    {
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f; // Ensure top-down perspective
        }

        public void Move(Vector2 direction, float speed)
        {
            // Unity 6000.3 uses linearVelocity instead of velocity
            _rb.linearVelocity = direction.normalized * speed;
        }

        public void Stop()
        {
            _rb.linearVelocity = Vector2.zero;
        }

        public void LookAt(Vector2 direction)
        {
            if (direction != Vector2.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                // Assuming Up (y-axis) is forward for the sprite
                _rb.rotation = angle - 90f; 
            }
        }
    }
}
