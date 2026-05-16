using TopDownShooter.Core.Events;
using TopDownShooter.Core.Health;
using TopDownShooter.Data;
using TopDownShooter.Runtime.Targeting;
using TopDownShooter.Runtime.Weapons;
using UnityEngine;

namespace TopDownShooter.Runtime.Enemies
{
    public sealed class EnemyFacade : MonoBehaviour, IDamageable
    {
        [SerializeField] private EnemyConfig config;
        [SerializeField] private WeaponConfig weaponConfig;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private MonoBehaviour moverBehaviour;
        [SerializeField] private MonoBehaviour targetProviderBehaviour;
        [SerializeField] private MonoBehaviour projectileSpawnerBehaviour;
        [SerializeField] private bool destroyOnDeath = true;
        [SerializeField, Min(0f)] private float destroyDelay = 0.05f;

        private Health health;
        private EnemyController controller;
        private GameplayEventBus eventBus;
        private bool initializationAttempted;

        public Team Team => Team.Enemy;
        public bool IsAlive => health != null && health.IsAlive;

        public void Initialize(ITargetProvider targetProvider, IProjectileSpawner projectileSpawner)
        {
            targetProviderBehaviour = targetProvider as MonoBehaviour;
            projectileSpawnerBehaviour = projectileSpawner as MonoBehaviour;
            TryInitialize();
        }

        private void Awake()
        {
            TryInitialize();
        }

        private void TryInitialize()
        {
            IEnemyMover mover = moverBehaviour as IEnemyMover;
            ITargetProvider targetProvider = targetProviderBehaviour as ITargetProvider;
            IProjectileSpawner projectileSpawner = projectileSpawnerBehaviour as IProjectileSpawner;

            if (config == null || weaponConfig == null || mover == null || targetProvider == null || projectileSpawner == null)
            {
                if (!initializationAttempted && targetProviderBehaviour != null && projectileSpawnerBehaviour != null)
                {
                    Debug.LogError($"{nameof(EnemyFacade)} is missing required dependencies.", this);
                }

                initializationAttempted = true;
                return;
            }

            if (controller != null)
            {
                return;
            }

            eventBus = new GameplayEventBus();
            health = new Health(config.MaxHealth);
            health.Died += HandleDied;

            WeaponController weapon = new WeaponController(weaponConfig, projectileSpawner, Team.Enemy, gameObject, projectileSpawnPoint);
            controller = new EnemyController(config, mover, targetProvider, weapon);
            initializationAttempted = true;
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Died -= HandleDied;
            }
        }

        private void Update()
        {
            if (controller == null)
            {
                TryInitialize();
            }

            controller?.Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            controller?.FixedTick(Time.fixedDeltaTime);
        }

        public void ApplyDamage(DamageInfo damageInfo)
        {
            if (health == null)
            {
                return;
            }

            if (damageInfo.SourceTeam == Team)
            {
                return;
            }

            health.ApplyDamage(damageInfo);
            eventBus.Publish(new DamageAppliedEvent(gameObject, damageInfo));
        }

        private void HandleDied(DamageInfo damageInfo)
        {
            controller.Die();
            eventBus.Publish(new EnemyDiedEvent(gameObject, config.ScoreValue, damageInfo));

            if (destroyOnDeath)
            {
                Destroy(gameObject, destroyDelay);
            }
        }
    }
}
