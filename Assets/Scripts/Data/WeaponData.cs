using UnityEngine;

namespace TopDownShooter.Data
{
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "TopDownShooter/Data/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Weapon Config")]
        public float fireRate = 0.2f;
        
        [Header("Projectile Config")]
        public GameObject projectilePrefab;
        public float projectileSpeed = 20f;
        public float damage = 10f;
    }
}
