using TopDownShooter.Runtime.Enemies;
using UnityEngine;

namespace TopDownShooter.UnityAdapters.Physics
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Rigidbody2DEnemyMover : MonoBehaviour, IEnemyMover
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

        public void MoveToward(Vector2 targetPosition, float speed, float stoppingDistance)
        {
            Vector2 offset = targetPosition - body.position;

            if (offset.sqrMagnitude <= stoppingDistance * stoppingDistance)
            {
                Stop();
                return;
            }

            body.linearVelocity = offset.normalized * speed;
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
