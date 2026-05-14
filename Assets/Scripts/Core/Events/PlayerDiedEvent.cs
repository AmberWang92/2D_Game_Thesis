using TopDownShooter.Core.Health;
using UnityEngine;

namespace TopDownShooter.Core.Events
{
    public readonly struct PlayerDiedEvent
    {
        public GameObject Player { get; }
        public DamageInfo DamageInfo { get; }

        public PlayerDiedEvent(GameObject player, DamageInfo damageInfo)
        {
            Player = player;
            DamageInfo = damageInfo;
        }
    }
}
