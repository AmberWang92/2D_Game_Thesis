using TopDownShooter.Core.Health;
using TopDownShooter.Data;
using UnityEngine;

namespace TopDownShooter.Runtime.Weapons
{
    public sealed class WeaponController
    {
        private readonly WeaponConfig config;
        private readonly IProjectileSpawner projectileSpawner;
        private readonly Team ownerTeam;
        private readonly GameObject owner;
        private readonly Transform muzzle;
        private readonly FireRateGate fireRateGate;

        public WeaponController(WeaponConfig config, IProjectileSpawner projectileSpawner, Team ownerTeam, GameObject owner, Transform muzzle)
        {
            this.config = config;
            this.projectileSpawner = projectileSpawner;
            this.ownerTeam = ownerTeam;
            this.owner = owner;
            this.muzzle = muzzle;
            fireRateGate = new FireRateGate(config.FireRate);
        }

        public bool TryFire(Vector2 ownerPosition, Vector2 targetPosition)
        {
            if (config.ProjectileConfig == null || !fireRateGate.CanFire(Time.time))
            {
                return false;
            }

            Vector2 direction = targetPosition - ownerPosition;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            direction.Normalize();
            Vector2 spawnPosition = muzzle != null ? (Vector2)muzzle.position : ownerPosition + direction * config.MuzzleOffset;
            projectileSpawner.Spawn(config.ProjectileConfig, spawnPosition, direction, ownerTeam, owner);
            fireRateGate.MarkFired(Time.time);
            return true;
        }
    }
}
