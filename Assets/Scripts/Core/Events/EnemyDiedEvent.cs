using TopDownShooter.Core.Health;
using UnityEngine;

namespace TopDownShooter.Core.Events
{
    public readonly struct EnemyDiedEvent
    {
        public GameObject Enemy { get; }
        public int ScoreValue { get; }
        public DamageInfo DamageInfo { get; }

        public EnemyDiedEvent(GameObject enemy, int scoreValue, DamageInfo damageInfo)
        {
            Enemy = enemy;
            ScoreValue = scoreValue;
            DamageInfo = damageInfo;
        }
    }
}
