using TopDownShooter.Core.Events;
using TopDownShooter.Core.FSM;
using TopDownShooter.Data;
using TopDownShooter.Gameplay.Combat;
using TopDownShooter.Gameplay.Enemies.States;
using TopDownShooter.Gameplay.Movement;
using UnityEngine;

namespace TopDownShooter.Gameplay.Enemies
{
    /// <summary>
    /// Composition root for an enemy. Wires HP / movement / sensor / weapon / contact
    /// damage and delegates per-frame behaviour to a state machine. Applies values
    /// from the <see cref="EnemyDefinitionSO"/> on Awake (and on enable for pool reuse).
    /// </summary>
    [RequireComponent(typeof(MovementComponent2D))]
    [RequireComponent(typeof(HealthComponent))]
    [DisallowMultipleComponent]
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyDefinitionSO definition;
        [SerializeField] private EnemySensor sensor;
        [SerializeField] private WeaponHolder weapon;            // shooter only
        [SerializeField] private ContactDamageDealer contact;    // chaser only
        [SerializeField] private Transform aimPivot;             // optional muzzle pivot
        [Tooltip("Optional channel raised when this enemy dies (used by ScoreService in M3).")]
        [SerializeField] private VoidEventChannelSO killedChannel;
        [SerializeField, Min(0f)] private float despawnDelay = 0.2f;

        private StateMachine<EnemyController> _fsm;
        private bool _despawnScheduled;

        public EnemyDefinitionSO Definition => definition;
        public EnemySensor Sensor => sensor;
        public WeaponHolder Weapon => weapon;
        public ContactDamageDealer Contact => contact;
        public MovementComponent2D Movement { get; private set; }
        public HealthComponent Health { get; private set; }
        public Vector2 AimDirection { get; private set; } = Vector2.right;

        private void Awake()
        {
            Movement = GetComponent<MovementComponent2D>();
            Health = GetComponent<HealthComponent>();

            ApplyDefinition();

            Health.Died += HandleDied;
            _fsm = new StateMachine<EnemyController>(this);
        }

        private void OnEnable()
        {
            // Reset for pooled reuse.
            _despawnScheduled = false;
            CancelInvoke(nameof(DoDespawn));
            foreach (var c in GetComponentsInChildren<Collider2D>(true)) c.enabled = true;
            if (Health != null && definition != null) Health.Initialize(definition.maxHP);
            _fsm?.ChangeState(new EnemyIdleState());
        }

        private void OnDestroy()
        {
            if (Health != null) Health.Died -= HandleDied;
        }

        private void Start()
        {
            // For instances placed directly in the scene (not via pool), OnEnable runs
            // before Awake-built FSM; ensure we have a starting state.
            if (_fsm != null && _fsm.Current == null) _fsm.ChangeState(new EnemyIdleState());
        }

        private void Update()
        {
            UpdateAim();
            _fsm?.Tick(Time.deltaTime);
        }

        private void FixedUpdate() => _fsm?.FixedTick(Time.fixedDeltaTime);

        public void ChangeState(IState<EnemyController> next) => _fsm.ChangeState(next);

        public void ScheduleDespawn()
        {
            if (_despawnScheduled) return;
            _despawnScheduled = true;
            Invoke(nameof(DoDespawn), despawnDelay);
        }

        private void DoDespawn()
        {
            gameObject.SetActive(false);
        }

        private void ApplyDefinition()
        {
            if (definition == null)
            {
                Debug.LogError($"{name}: EnemyDefinitionSO is not assigned.", this);
                return;
            }
            Health.Initialize(definition.maxHP);

            if (sensor != null) sensor.SetRadius(definition.detectionRadius);

            if (contact != null)
                contact.Configure(definition.contactDamage, definition.contactInterval);

            if (weapon != null && definition.weapon != null)
                weapon.SetWeapon(definition.weapon);

            ApplyTint();
        }

        private void ApplyTint()
        {
            foreach (var sr in GetComponentsInChildren<SpriteRenderer>(true))
                sr.color = definition.tint;
        }

        private void UpdateAim()
        {
            var target = sensor != null ? sensor.Target : null;
            if (target == null) return;

            Vector2 toTarget = (Vector2)target.position - (Vector2)transform.position;
            if (toTarget.sqrMagnitude > 0.0001f) AimDirection = toTarget.normalized;

            if (aimPivot != null)
            {
                float deg = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
                aimPivot.rotation = Quaternion.Euler(0f, 0f, deg);
            }
        }

        private void HandleDied()
        {
            killedChannel?.Raise();
            ChangeState(new EnemyDeadState());
        }
    }
}
