using System;
using TopDownShooter.Data;
using UnityEngine;

namespace TopDownShooter.Gameplay.Combat
{
    /// <summary>
    /// Projectile body. Driven by a <see cref="ProjectileDefinitionSO"/> for damage
    /// and lifetime; owned by an external pool that subscribes to <see cref="Despawned"/>.
    /// Uses a trigger Collider2D + kinematic-friendly Rigidbody2D.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private ProjectileDefinitionSO definition;

        private Rigidbody2D _rb;
        private GameObject _owner;
        private float _despawnAt;
        private bool _alive;

        public ProjectileDefinitionSO Definition => definition;
        public event Action<Projectile> Despawned;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            foreach (var c in GetComponents<Collider2D>()) c.isTrigger = true;
        }

        public void Configure(ProjectileDefinitionSO def)
        {
            definition = def;
        }

        public void Launch(Vector2 position, Vector2 velocity, GameObject owner)
        {
            if (definition == null)
            {
                Debug.LogError($"Projectile '{name}' has no definition assigned.", this);
                Despawn();
                return;
            }
            _owner = owner;
            transform.position = position;
            transform.right = velocity.sqrMagnitude > 0.0001f ? (Vector3)velocity.normalized : Vector3.right;
            _rb.linearVelocity = velocity;
            _despawnAt = Time.time + definition.lifetime;
            _alive = true;
        }

        private void Update()
        {
            if (_alive && Time.time >= _despawnAt) Despawn();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_alive || definition == null) return;

            // Ignore the shooter itself.
            if (_owner && (other.transform == _owner.transform || other.transform.IsChildOf(_owner.transform)))
                return;

            // Only react to layers in the hit mask.
            if ((definition.hitMask.value & (1 << other.gameObject.layer)) == 0) return;

            DamageDealer.TryApply(other, definition.damage, out _);
            Despawn();
        }

        private void Despawn()
        {
            if (!_alive) return;
            _alive = false;
            _rb.linearVelocity = Vector2.zero;
            Despawned?.Invoke(this);
        }
    }
}
