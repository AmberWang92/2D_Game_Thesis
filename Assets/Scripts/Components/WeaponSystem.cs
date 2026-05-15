using UnityEngine;
using TopDownShooter.Data;
using TopDownShooter.Projectiles;

namespace TopDownShooter.Components
{
    public class WeaponSystem : MonoBehaviour
    {
        [SerializeField] private WeaponData currentWeapon;
        [SerializeField] private Transform firePoint;

        private float _nextFireTime;

        public void Fire()
        {
            if (currentWeapon == null || currentWeapon.projectilePrefab == null) return;
            if (firePoint == null) return;

            if (Time.time >= _nextFireTime)
            {
                _nextFireTime = Time.time + currentWeapon.fireRate;
                SpawnProjectile();
            }
        }

        private void SpawnProjectile()
        {
            GameObject projObj = Instantiate(currentWeapon.projectilePrefab, firePoint.position, firePoint.rotation);
            
            if (projObj.TryGetComponent(out Projectile projectile))
            {
                projectile.Initialize(currentWeapon.projectileSpeed, currentWeapon.damage);
            }
        }

        public void EquipWeapon(WeaponData newWeapon)
        {
            currentWeapon = newWeapon;
        }
    }
}
