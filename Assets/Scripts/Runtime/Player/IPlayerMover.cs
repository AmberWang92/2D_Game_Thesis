using UnityEngine;

namespace TopDownShooter.Runtime.Player
{
    public interface IPlayerMover
    {
        Vector2 Position { get; }
        void Move(Vector2 direction, float speed);
        void LookAt(Vector2 worldPosition, float rotationSpeed);
        void Stop();
    }
}
