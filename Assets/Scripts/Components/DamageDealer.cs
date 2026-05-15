using UnityEngine;
using TopDownShooter.Core.Interfaces;

namespace TopDownShooter.Components
{
    public class DamageDealer : MonoBehaviour
    {
        [SerializeField] private bool destroyOnHit = true;
        [SerializeField] private string targetTag = "Enemy";
        
        private float _damage;

        public void SetDamage(float damage)
        {
            _damage = damage;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // If targetTag is set, only hit objects with that tag. Otherwise hit everything.
            if (!string.IsNullOrEmpty(targetTag) && !collision.CompareTag(targetTag))
                return;

            if (collision.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(_damage);
                if (destroyOnHit)
                {
                    Destroy(gameObject);
                }
            }
            else if (destroyOnHit)
            {
                // Hit something else (e.g., a wall), destroy it anyway
                Destroy(gameObject);
            }
        }
    }
}
