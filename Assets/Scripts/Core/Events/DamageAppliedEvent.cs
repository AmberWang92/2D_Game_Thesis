using TopDownShooter.Core.Health;
using UnityEngine;

namespace TopDownShooter.Core.Events
{
    public readonly struct DamageAppliedEvent
    {
        public GameObject Target { get; }
        public DamageInfo DamageInfo { get; }

        public DamageAppliedEvent(GameObject target, DamageInfo damageInfo)
        {
            Target = target;
            DamageInfo = damageInfo;
        }
    }
}
