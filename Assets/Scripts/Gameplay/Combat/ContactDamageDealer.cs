using System.Collections.Generic;
using UnityEngine;

namespace TopDownShooter.Gameplay.Combat
{
    /// <summary>
    /// Applies damage to <see cref="Core.Health.IDamageable"/>s the owner is touching,
    /// throttled per-target by <see cref="interval"/>. Works with both regular and
    /// trigger colliders so chasers can use either setup.
    /// </summary>
    [DisallowMultipleComponent]
    public class ContactDamageDealer : MonoBehaviour
    {
        [SerializeField, Min(0)] private int damage = 1;
        [SerializeField, Min(0.05f)] private float interval = 0.5f;
        [SerializeField] private LayerMask targetMask;

        private readonly Dictionary<Collider2D, float> _nextHitAt = new();

        public LayerMask TargetMask => targetMask;

        public void Configure(int damageAmount, float damageInterval)
        {
            damage = Mathf.Max(0, damageAmount);
            interval = Mathf.Max(0.05f, damageInterval);
        }

        private void OnDisable() => _nextHitAt.Clear();

        private void OnCollisionStay2D(Collision2D collision) => TryHit(collision.collider);
        private void OnTriggerStay2D(Collider2D other) => TryHit(other);

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.collider) _nextHitAt.Remove(collision.collider);
        }
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other) _nextHitAt.Remove(other);
        }

        private void TryHit(Collider2D other)
        {
            if (damage <= 0 || other == null) return;
            if ((targetMask.value & (1 << other.gameObject.layer)) == 0) return;

            if (_nextHitAt.TryGetValue(other, out float next) && Time.time < next) return;

            if (DamageDealer.TryApply(other, damage, out _))
                _nextHitAt[other] = Time.time + interval;
        }
    }
}
