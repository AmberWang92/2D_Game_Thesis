using TopDownShooter.Core.Health;
using TopDownShooter.Data;
using UnityEngine;

namespace TopDownShooter.Runtime.Projectiles
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class ProjectileView : MonoBehaviour
    {
        private ProjectileConfig config;
        private Vector2 direction;
        private Team ownerTeam;
        private GameObject owner;
        private float despawnTime;
        private bool initialized;

        public void Initialize(ProjectileConfig config, Vector2 direction, Team ownerTeam, GameObject owner)
        {
            Vector2 normalizedDirection = direction.normalized;

            this.config = config;
            this.direction = normalizedDirection;
            this.ownerTeam = ownerTeam;
            this.owner = owner;
            despawnTime = Time.time + config.Lifetime;
            transform.SetParent(null, true);
            transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0f, 0f, Mathf.Atan2(normalizedDirection.y, normalizedDirection.x) * Mathf.Rad2Deg));
            initialized = true;
        }

        private void Update()
        {
            if (!initialized)
            {
                return;
            }

            transform.position += (Vector3)(direction * config.Speed * Time.deltaTime);

            if (Time.time >= despawnTime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!initialized || other.gameObject == owner)
            {
                return;
            }

            IDamageable damageable = other.GetComponentInParent<IDamageable>();

            if (damageable == null || damageable.Team == ownerTeam || !damageable.IsAlive)
            {
                return;
            }

            damageable.ApplyDamage(new DamageInfo(config.Damage, ownerTeam, owner));
            Destroy(gameObject);
        }
    }
}
