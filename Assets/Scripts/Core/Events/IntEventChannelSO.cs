using System;
using UnityEngine;

namespace TopDownShooter.Core.Events
{
    /// <summary>
    /// Int-payload event channel (score deltas, damage amounts, wave numbers, ...).
    /// </summary>
    [CreateAssetMenu(menuName = "TopDownShooter/Events/Int Event Channel", fileName = "IntEventChannel")]
    public class IntEventChannelSO : ScriptableObject
    {
        public event Action<int> OnRaised;

        public void Raise(int value) => OnRaised?.Invoke(value);
    }
}
