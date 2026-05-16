using TopDownShooter.Core.Health;
using TopDownShooter.Data;
using TopDownShooter.Runtime.Player;
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

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
            Transform parent = ResolveParent(owner);
            ProjectileView projectile = Instantiate(projectilePrefab, position, rotation, parent);
            projectile.Initialize(config, direction, ownerTeam, owner);
        }

        private Transform ResolveParent(GameObject owner)
        {
            if (projectileParent == null || owner == null)
            {
                return projectileParent;
            }

            if (projectileParent == owner.transform || projectileParent.IsChildOf(owner.transform))
            {
                Debug.LogWarning($"{nameof(ProjectileSpawner)} ignored a projectile parent under the firing owner. Use a scene-level projectile parent instead.", this);
                return null;
            }

            if (projectileParent.GetComponentInParent<IDamageable>() != null || projectileParent.GetComponentInParent<PlayerFacade>() != null)
            {
                Debug.LogWarning($"{nameof(ProjectileSpawner)} ignored a projectile parent under a gameplay actor. Use a scene-level projectile parent instead.", this);
                return null;
            }

            return projectileParent;
        }
    }
}
