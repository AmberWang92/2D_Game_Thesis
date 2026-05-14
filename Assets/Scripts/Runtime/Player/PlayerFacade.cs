using TopDownShooter.Core.Events;
using TopDownShooter.Core.Health;
using TopDownShooter.Data;
using TopDownShooter.Runtime.Weapons;
using TopDownShooter.UnityAdapters.Input;
using UnityEngine;

namespace TopDownShooter.Runtime.Player
{
    [RequireComponent(typeof(InputSystemPlayerInput))]
    public sealed class PlayerFacade : MonoBehaviour, IDamageable
    {
        [SerializeField] private PlayerConfig config;
        [SerializeField] private WeaponConfig weaponConfig;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private MonoBehaviour moverBehaviour;
        [SerializeField] private MonoBehaviour projectileSpawnerBehaviour;

        private Health health;
        private PlayerController controller;
        private GameplayEventBus eventBus;

        public Team Team => Team.Player;
        public bool IsAlive => health != null && health.IsAlive;

        private void Awake()
        {
            IPlayerInput input = GetComponent<IPlayerInput>();
            IPlayerMover mover = moverBehaviour as IPlayerMover;
            IProjectileSpawner projectileSpawner = projectileSpawnerBehaviour as IProjectileSpawner;

            if (config == null || weaponConfig == null || input == null || mover == null || projectileSpawner == null)
            {
                enabled = false;
                Debug.LogError($"{nameof(PlayerFacade)} is missing required dependencies.", this);
                return;
            }

            eventBus = new GameplayEventBus();
            health = new Health(config.MaxHealth);
            health.Died += HandleDied;

            WeaponController weapon = new WeaponController(weaponConfig, projectileSpawner, Team.Player, gameObject, projectileSpawnPoint);
            controller = new PlayerController(config, input, mover, weapon);
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
            controller?.Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            controller?.FixedTick(Time.fixedDeltaTime);
        }

        public void ApplyDamage(DamageInfo damageInfo)
        {
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
            eventBus.Publish(new PlayerDiedEvent(gameObject, damageInfo));
        }
    }
}
