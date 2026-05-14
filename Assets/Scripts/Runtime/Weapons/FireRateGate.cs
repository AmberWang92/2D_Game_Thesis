using UnityEngine;

namespace TopDownShooter.Runtime.Weapons
{
    public sealed class FireRateGate
    {
        private readonly float interval;
        private float nextAllowedFireTime;

        public FireRateGate(float shotsPerSecond)
        {
            interval = 1f / Mathf.Max(0.01f, shotsPerSecond);
        }

        public bool CanFire(float time)
        {
            return time >= nextAllowedFireTime;
        }

        public void MarkFired(float time)
        {
            nextAllowedFireTime = time + interval;
        }
    }
}
