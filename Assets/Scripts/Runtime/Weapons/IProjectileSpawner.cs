using TopDownShooter.Core.Health;
using TopDownShooter.Data;
using UnityEngine;

namespace TopDownShooter.Runtime.Weapons
{
    public interface IProjectileSpawner
    {
        void Spawn(ProjectileConfig config, Vector2 position, Vector2 direction, Team ownerTeam, GameObject owner);
    }
}
