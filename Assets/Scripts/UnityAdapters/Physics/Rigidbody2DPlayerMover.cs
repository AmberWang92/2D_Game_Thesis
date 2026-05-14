using TopDownShooter.Runtime.Player;
using UnityEngine;

namespace TopDownShooter.UnityAdapters.Physics
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Rigidbody2DPlayerMover : MonoBehaviour, IPlayerMover
    {
        [SerializeField] private Rigidbody2D body;

        public Vector2 Position => body.position;

        private void Reset()
        {
            body = GetComponent<Rigidbody2D>();
        }

        private void Awake()
        {
            if (body == null)
            {
                body = GetComponent<Rigidbody2D>();
            }

            body.gravityScale = 0f;
            body.freezeRotation = true;
        }

        public void Move(Vector2 direction, float speed)
        {
            Vector2 normalizedDirection = direction.sqrMagnitude > 1f ? direction.normalized : direction;
            body.linearVelocity = normalizedDirection * speed;
        }

        public void LookAt(Vector2 worldPosition, float rotationSpeed)
        {
            Vector2 direction = worldPosition - body.position;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float angle = Mathf.MoveTowardsAngle(body.rotation, targetAngle, rotationSpeed * Time.fixedDeltaTime);
            body.MoveRotation(angle);
        }

        public void Stop()
        {
            body.linearVelocity = Vector2.zero;
        }
    }
}
