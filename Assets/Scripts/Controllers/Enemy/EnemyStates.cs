using UnityEngine;
using TopDownShooter.Core.StateMachine;

namespace TopDownShooter.Controllers.Enemy
{
    public class EnemyIdleState : IState
    {
        private EnemyBrain _brain;
        private float _idleTimer;

        public EnemyIdleState(EnemyBrain brain)
        {
            _brain = brain;
        }

        public void Enter()
        {
            _brain.Movement.Stop();
            _idleTimer = Random.Range(1f, 3f);
        }

        public void Update()
        {
            if (_brain.Target != null)
            {
                float distance = Vector2.Distance(_brain.transform.position, _brain.Target.position);
                if (distance <= _brain.DetectionRadius)
                {
                    _brain.TransitionTo(new EnemyChaseState(_brain));
                    return;
                }
            }

            _idleTimer -= Time.deltaTime;
            if (_idleTimer <= 0f)
            {
                // Simple behavior: just reset the timer. 
                // A wander state could be transitioned to here.
                _idleTimer = Random.Range(1f, 3f);
            }
        }

        public void FixedUpdate() { }
        public void Exit() { }
    }

    public class EnemyChaseState : IState
    {
        private EnemyBrain _brain;

        public EnemyChaseState(EnemyBrain brain)
        {
            _brain = brain;
        }

        public void Enter() { }

        public void Update()
        {
            if (_brain.Target == null)
            {
                _brain.TransitionTo(new EnemyIdleState(_brain));
                return;
            }

            float distance = Vector2.Distance(_brain.transform.position, _brain.Target.position);
            
            if (distance > _brain.DetectionRadius * 1.5f) // Lose interest
            {
                _brain.TransitionTo(new EnemyIdleState(_brain));
            }
            else if (distance <= _brain.AttackRadius)
            {
                _brain.TransitionTo(new EnemyAttackState(_brain));
            }
        }

        public void FixedUpdate()
        {
            if (_brain.Target != null)
            {
                Vector2 direction = ((Vector2)_brain.Target.position - (Vector2)_brain.transform.position).normalized;
                float speed = _brain.Stats != null ? _brain.Stats.moveSpeed : 3f;
                
                _brain.Movement.Move(direction, speed);
                _brain.Movement.LookAt(direction);
            }
        }

        public void Exit() { }
    }

    public class EnemyAttackState : IState
    {
        private EnemyBrain _brain;

        public EnemyAttackState(EnemyBrain brain)
        {
            _brain = brain;
        }

        public void Enter()
        {
            _brain.Movement.Stop();
        }

        public void Update()
        {
            if (_brain.Target == null)
            {
                _brain.TransitionTo(new EnemyIdleState(_brain));
                return;
            }

            float distance = Vector2.Distance(_brain.transform.position, _brain.Target.position);
            
            if (distance > _brain.AttackRadius)
            {
                _brain.TransitionTo(new EnemyChaseState(_brain));
                return;
            }

            Vector2 direction = ((Vector2)_brain.Target.position - (Vector2)_brain.transform.position).normalized;
            _brain.Movement.LookAt(direction);

            if (_brain.Weapon != null)
            {
                _brain.Weapon.Fire();
            }
        }

        public void FixedUpdate() { }
        public void Exit() { }
    }
}
