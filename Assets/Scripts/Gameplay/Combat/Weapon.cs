using TopDownShooter.Data;
using TopDownShooter.Services;
using UnityEngine;

namespace TopDownShooter.Gameplay.Combat
{
    /// <summary>
    /// Pure-ish runtime weapon: cooldown + projectile spawning. No MonoBehaviour
    /// dependency so it can be used by player or enemy holders alike.
    /// </summary>
    public sealed class Weapon
    {
        private readonly WeaponDefinitionSO _definition;
        private readonly ObjectPool<Projectile> _pool;
        private float _nextFireTime;

        public WeaponDefinitionSO Definition => _definition;

        public Weapon(WeaponDefinitionSO definition, ObjectPool<Projectile> pool)
        {
            _definition = definition;
            _pool = pool;
        }

        public bool TryFire(Vector2 origin, Vector2 direction, GameObject owner)
        {
            if (_definition == null || _pool == null) return false;
            if (Time.time < _nextFireTime) return false;
            if (direction.sqrMagnitude < 0.0001f) return false;

            _nextFireTime = Time.time + 1f / Mathf.Max(0.01f, _definition.fireRate);

            Vector2 baseDir = direction.normalized;
            int count = Mathf.Max(1, _definition.projectilesPerShot);
            float spread = _definition.spreadDegrees;

            for (int i = 0; i < count; i++)
            {
                float angle = spread > 0f ? Random.Range(-spread, spread) : 0f;
                Vector2 dir = Rotate(baseDir, angle);
                Vector2 velocity = dir * _definition.muzzleSpeed;

                var projectile = _pool.Get();
                projectile.Launch(origin, velocity, owner);
            }
            return true;
        }

        private static Vector2 Rotate(Vector2 v, float degrees)
        {
            float r = degrees * Mathf.Deg2Rad;
            float c = Mathf.Cos(r);
            float s = Mathf.Sin(r);
            return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
        }
    }
}
