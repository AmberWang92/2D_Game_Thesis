using System;
using UnityEngine;

namespace TopDownShooter.Core.Events
{
    /// <summary>
    /// Parameterless ScriptableObject event channel. Publishers call <see cref="Raise"/>;
    /// listeners subscribe to <see cref="OnRaised"/>. Used to decouple systems
    /// (e.g. PlayerController raises PlayerDied; GameManager + HUD listen).
    /// </summary>
    [CreateAssetMenu(menuName = "TopDownShooter/Events/Void Event Channel", fileName = "VoidEventChannel")]
    public class VoidEventChannelSO : ScriptableObject
    {
        public event Action OnRaised;

        public void Raise() => OnRaised?.Invoke();
    }
}
