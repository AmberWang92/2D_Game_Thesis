using UnityEngine;

namespace TopDownShooter.Runtime.Enemies
{
    public interface IEnemyMover
    {
        Vector2 Position { get; }
        void MoveToward(Vector2 targetPosition, float speed, float stoppingDistance);
        void LookAt(Vector2 worldPosition, float rotationSpeed);
        void Stop();
    }
}
