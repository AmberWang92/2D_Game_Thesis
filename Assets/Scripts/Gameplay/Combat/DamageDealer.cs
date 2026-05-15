using TopDownShooter.Core.Health;
using UnityEngine;

namespace TopDownShooter.Gameplay.Combat
{
    /// <summary>
    /// Stateless helper for resolving an <see cref="IDamageable"/> from a Collider2D.
    /// Looks at the rigidbody first (compound colliders) then the collider itself.
    /// </summary>
    public static class DamageDealer
    {
        public static bool TryApply(Collider2D collider, int amount, out IDamageable target)
        {
            target = null;
            if (!collider) return false;

            var rb = collider.attachedRigidbody;
            if (rb && rb.TryGetComponent(out target))
            {
                target.ApplyDamage(amount);
                return true;
            }
            if (collider.TryGetComponent(out target))
            {
                target.ApplyDamage(amount);
                return true;
            }
            return false;
        }
    }
}
