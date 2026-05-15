using TopDownShooter.Data;
using TopDownShooter.Services;
using UnityEngine;

namespace TopDownShooter.Gameplay.Combat
{
    /// <summary>
    /// MonoBehaviour facade that owns a runtime <see cref="Weapon"/> and a projectile
    /// pool. Anyone with an aim direction can call <see cref="TryFire"/>; switching
    /// loadouts at runtime is supported via <see cref="SetWeapon"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class WeaponHolder : MonoBehaviour
    {
        [SerializeField] private WeaponDefinitionSO definition;
        [Tooltip("Spawn point for projectiles. Falls back to this transform if unset.")]
        [SerializeField] private Transform muzzle;
        [Tooltip("Optional parent for pooled projectiles; defaults to scene root.")]
        [SerializeField] private Transform projectileParent;

        private Weapon _weapon;
        private ObjectPool<Projectile> _pool;
        private WeaponDefinitionSO _activeDefinition;

        public WeaponDefinitionSO Definition => _activeDefinition;

        private void Awake()
        {
            if (definition != null) SetWeapon(definition);
        }

        public void SetWeapon(WeaponDefinitionSO def)
        {
            if (def == null || def.projectile == null || def.projectile.prefab == null)
            {
                Debug.LogError($"{name}: weapon definition is missing projectile/prefab.", this);
                return;
            }
            if (def.projectile.prefab.GetComponent<Projectile>() == null)
            {
                Debug.LogError($"{name}: projectile prefab '{def.projectile.prefab.name}' has no Projectile component.", this);
                return;
            }

            _activeDefinition = def;
            BuildPool(def);
            _weapon = new Weapon(def, _pool);
        }

        public bool TryFire(Vector2 aimDirection)
        {
            if (_weapon == null) return false;
            Vector2 origin = muzzle ? (Vector2)muzzle.position : (Vector2)transform.position;
            return _weapon.TryFire(origin, aimDirection, gameObject);
        }

        private void BuildPool(WeaponDefinitionSO def)
        {
            var prefab = def.projectile.prefab.GetComponent<Projectile>();
            ObjectPool<Projectile> pool = null;

            pool = new ObjectPool<Projectile>(
                factory: () =>
                {
                    var inst = Instantiate(prefab, projectileParent);
                    inst.Configure(def.projectile);
                    inst.gameObject.SetActive(false);
                    inst.Despawned += p => pool.Release(p);
                    return inst;
                },
                onGet: p => p.gameObject.SetActive(true),
                onRelease: p => p.gameObject.SetActive(false));

            pool.Prewarm(def.poolPrewarm);
            _pool = pool;
        }
    }
}
