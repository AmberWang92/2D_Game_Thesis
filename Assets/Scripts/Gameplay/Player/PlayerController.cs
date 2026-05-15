using TopDownShooter.Core.FSM;
using TopDownShooter.Data;
using TopDownShooter.Gameplay.Combat;
using TopDownShooter.Gameplay.Movement;
using TopDownShooter.Gameplay.Player.States;
using UnityEngine;

namespace TopDownShooter.Gameplay.Player
{
    /// <summary>
    /// Composition root for the player. Wires up dependencies (input, movement,
    /// combat, health) and delegates per-frame behaviour to a state machine.
    /// </summary>
    [RequireComponent(typeof(MovementComponent2D))]
    [RequireComponent(typeof(HealthComponent))]
    [DisallowMultipleComponent]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerStatsSO stats;
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private WeaponHolder weapon;
        [Tooltip("Optional transform that rotates to face the aim direction (e.g. a muzzle pivot).")]
        [SerializeField] private Transform aimPivot;

        private StateMachine<PlayerController> _fsm;

        public PlayerStatsSO Stats => stats;
        public PlayerInputReader Input => input;
        public WeaponHolder Weapon => weapon;
        public MovementComponent2D Movement { get; private set; }
        public HealthComponent Health { get; private set; }
        public Vector2 AimDirection { get; private set; } = Vector2.right;

        private void Awake()
        {
            Movement = GetComponent<MovementComponent2D>();
            Health = GetComponent<HealthComponent>();

            if (stats == null)
                Debug.LogError($"{name}: PlayerStatsSO is not assigned.", this);
            else
                Health.Initialize(stats.maxHP);

            Health.Died += HandleDied;

            _fsm = new StateMachine<PlayerController>(this);
            _fsm.ChangeState(new PlayerIdleState());
        }

        private void OnDestroy()
        {
            if (Health != null) Health.Died -= HandleDied;
        }

        private void Update()
        {
            UpdateAim();
            _fsm.Tick(Time.deltaTime);
        }

        private void FixedUpdate() => _fsm.FixedTick(Time.fixedDeltaTime);

        public void ChangeState(IState<PlayerController> next) => _fsm.ChangeState(next);

        /// <summary>Shared combat logic invoked by Idle/Move states.</summary>
        public void HandleCombat()
        {
            if (input == null || weapon == null) return;
            if (input.FireHeld) weapon.TryFire(AimDirection);
        }

        private void UpdateAim()
        {
            if (input == null) return;

            if (input.HasStickAim)
            {
                AimDirection = input.StickAim;
            }
            else if (input.HasPointerAim)
            {
                Vector2 toAim = input.PointerWorld - (Vector2)transform.position;
                if (toAim.sqrMagnitude > 0.0001f) AimDirection = toAim.normalized;
            }

            if (aimPivot != null)
            {
                float deg = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
                aimPivot.rotation = Quaternion.Euler(0f, 0f, deg);
            }
        }

        private void HandleDied() => ChangeState(new PlayerDeadState());
    }
}
