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
            this.config = config;
            this.direction = direction.normalized;
            this.ownerTeam = ownerTeam;
            this.owner = owner;
            despawnTime = Time.time + config.Lifetime;
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
