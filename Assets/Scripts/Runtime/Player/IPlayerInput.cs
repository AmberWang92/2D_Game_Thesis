using UnityEngine;

namespace TopDownShooter.Runtime.Player
{
    public interface IPlayerInput
    {
        Vector2 Move { get; }
        Vector2 AimWorldPosition { get; }
        bool IsFireHeld { get; }
    }
}
