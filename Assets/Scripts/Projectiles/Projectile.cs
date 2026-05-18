using UnityEngine;
using TopDownShooter.Components;

namespace TopDownShooter.Projectiles
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(DamageDealer))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 5f;
        
        private Rigidbody2D _rb;
        private DamageDealer _damageDealer;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            
            _damageDealer = GetComponent<DamageDealer>();
        }

        public void Initialize(float speed, float damage, string targetTag = "")
        {
            _damageDealer.SetDamage(damage);
            
            if (!string.IsNullOrEmpty(targetTag))
            {
                _damageDealer.SetTargetTag(targetTag);
            }
            
            // Move forward (assuming Sprite's top is Up)
            _rb.linearVelocity = transform.up * speed; 
            
            Destroy(gameObject, lifetime);
        }
    }
}
