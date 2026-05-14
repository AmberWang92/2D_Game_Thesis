using UnityEngine;

namespace TopDownShooter.Core.Health
{
    public readonly struct DamageInfo
    {
        public int Amount { get; }
        public Team SourceTeam { get; }
        public GameObject Source { get; }

        public DamageInfo(int amount, Team sourceTeam, GameObject source)
        {
            Amount = amount;
            SourceTeam = sourceTeam;
            Source = source;
        }
    }
}
