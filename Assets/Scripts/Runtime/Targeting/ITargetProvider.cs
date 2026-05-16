using UnityEngine;

namespace TopDownShooter.Runtime.Targeting
{
    public interface ITargetProvider
    {
        bool HasTarget { get; }
        Vector2 Position { get; }
        GameObject TargetObject { get; }
    }
}
