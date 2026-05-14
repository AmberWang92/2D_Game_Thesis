using TopDownShooter.Core.Health;
using TopDownShooter.Data;
using TopDownShooter.Runtime.Weapons;
using UnityEngine;

namespace TopDownShooter.Runtime.Projectiles
{
    public sealed class ProjectileSpawner : MonoBehaviour, IProjectileSpawner
    {
        [SerializeField] private ProjectileView projectilePrefab;
        [SerializeField] private Transform projectileParent;

        public void Spawn(ProjectileConfig config, Vector2 position, Vector2 direction, Team ownerTeam, GameObject owner)
        {
            if (projectilePrefab == null || config == null)
            {
                Debug.LogError($"{nameof(ProjectileSpawner)} is missing required dependencies.", this);
                return;
            }

            ProjectileView projectile = Instantiate(projectilePrefab, position, Quaternion.identity, projectileParent);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            projectile.Initialize(config, direction, ownerTeam, owner);
        }
    }
}
