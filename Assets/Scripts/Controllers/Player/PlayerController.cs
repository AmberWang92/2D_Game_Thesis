using UnityEngine;
using UnityEngine.InputSystem;
using TopDownShooter.Components;
using TopDownShooter.Data;

namespace TopDownShooter.Controllers.Player
{
    [RequireComponent(typeof(MovementComponent))]
    [RequireComponent(typeof(WeaponSystem))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private CharacterStatsData stats;

        [Header("Input References")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private InputActionReference fireAction;

        private MovementComponent _movement;
        private WeaponSystem _weaponSystem;
        private Camera _mainCamera;

        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private bool _isFiring;

        private void Awake()
        {
            _movement = GetComponent<MovementComponent>();
            _weaponSystem = GetComponent<WeaponSystem>();
            _mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            if (moveAction != null)
            {
                moveAction.action.Enable();
                moveAction.action.performed += OnMove;
                moveAction.action.canceled += OnMove;
            }

            if (lookAction != null)
            {
                lookAction.action.Enable();
                lookAction.action.performed += OnLook;
                lookAction.action.canceled += OnLook;
            }

            if (fireAction != null)
            {
                fireAction.action.Enable();
                fireAction.action.performed += OnFire;
                fireAction.action.canceled += OnFire;
            }
        }

        private void OnDisable()
        {
            if (moveAction != null)
            {
                moveAction.action.performed -= OnMove;
                moveAction.action.canceled -= OnMove;
                moveAction.action.Disable();
            }

            if (lookAction != null)
            {
                lookAction.action.performed -= OnLook;
                lookAction.action.canceled -= OnLook;
                lookAction.action.Disable();
            }

            if (fireAction != null)
            {
                fireAction.action.performed -= OnFire;
                fireAction.action.canceled -= OnFire;
                fireAction.action.Disable();
            }
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            _lookInput = context.ReadValue<Vector2>();
        }

        private void OnFire(InputAction.CallbackContext context)
        {
            _isFiring = context.ReadValueAsButton();
        }

        private void Update()
        {
            HandleShooting();
        }

        private void FixedUpdate()
        {
            HandleMovement();
            HandleRotation();
        }

        private void HandleMovement()
        {
            float speed = stats != null ? stats.moveSpeed : 5f;
            _movement.Move(_moveInput, speed);
        }

        private void HandleRotation()
        {
            if (_lookInput == Vector2.zero) return;

            // Check if we are using mouse or gamepad for looking
            if (Mouse.current != null && lookAction.action.activeControl?.device == Mouse.current)
            {
                // Mouse position logic
                Vector3 worldMousePos = _mainCamera.ScreenToWorldPoint(new Vector3(_lookInput.x, _lookInput.y, -_mainCamera.transform.position.z));
                Vector2 lookDirection = (worldMousePos - transform.position).normalized;
                _movement.LookAt(lookDirection);
            }
            else
            {
                // Gamepad dual-stick logic
                _movement.LookAt(_lookInput);
            }
        }

        private void HandleShooting()
        {
            if (_isFiring)
            {
                _weaponSystem.Fire();
            }
        }
    }
}
