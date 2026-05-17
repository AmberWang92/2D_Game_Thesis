using UnityEngine;
using TopDownShooter.Core.StateMachine;
using TopDownShooter.Components;
using TopDownShooter.Data;

namespace TopDownShooter.Controllers.Enemy
{
    [RequireComponent(typeof(MovementComponent))]
    public class EnemyBrain : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private CharacterStatsData stats;
        [SerializeField] private float detectionRadius = 10f;
        [SerializeField] private float attackRadius = 5f;
        
        public MovementComponent Movement { get; private set; }
        public WeaponSystem Weapon { get; private set; }
        public Transform Target { get; private set; }
        public CharacterStatsData Stats => stats;
        public float DetectionRadius => detectionRadius;
        public float AttackRadius => attackRadius;

        private Core.StateMachine.StateMachine _stateMachine;

        private void Awake()
        {
            Movement = GetComponent<MovementComponent>();
            Weapon = GetComponent<WeaponSystem>(); // Optional, for ranged enemies
            _stateMachine = new Core.StateMachine.StateMachine();
        }

        private void Start()
        {
            // Simple approach to find the player for now
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                Target = playerObj.transform;
            }

            var idleState = new EnemyIdleState(this);
            _stateMachine.Initialize(idleState);
        }

        private void Update()
        {
            _stateMachine.Update();
        }

        private void FixedUpdate()
        {
            _stateMachine.FixedUpdate();
        }

        public void TransitionTo(IState nextState)
        {
            _stateMachine.TransitionTo(nextState);
        }

        // To be called by HealthComponent.OnDied UnityEvent
        public void OnDied()
        {
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRadius);
        }
    }
}
