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

        public void SetTargetTag(string tag)
        {
            targetTag = tag;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Debug.Log($"[DamageDealer] '{gameObject.name}' collided with '{collision.gameObject.name}' (Tag: {collision.tag})");

            // If targetTag is set, only hit objects with that tag. Otherwise hit everything.
            if (!string.IsNullOrEmpty(targetTag) && !collision.CompareTag(targetTag))
            {
                Debug.Log($"[DamageDealer] Ignored '{collision.gameObject.name}' because its tag is not '{targetTag}'");
                return;
            }

            if (collision.TryGetComponent(out IDamageable damageable))
            {
                Debug.Log($"[DamageDealer] Successfully applying {_damage} damage to '{collision.gameObject.name}'");
                damageable.TakeDamage(_damage);
                if (destroyOnHit)
                {
                    Destroy(gameObject);
                }
            }
            else if (destroyOnHit)
            {
                Debug.Log($"[DamageDealer] '{collision.gameObject.name}' has no IDamageable component. Destroying '{gameObject.name}' anyway.");
                // Hit something else (e.g., a wall), destroy it anyway
                Destroy(gameObject);
            }
        }
    }
}
