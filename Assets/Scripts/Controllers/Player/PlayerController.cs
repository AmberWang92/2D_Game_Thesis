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
        private HealthComponent _health;
        private Camera _mainCamera;

        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private bool _isFiring;
        
        // Track the current control scheme to swap aiming logic
        private bool _isUsingGamepad;

        private void Awake()
        {
            _movement = GetComponent<MovementComponent>();
            _weaponSystem = GetComponent<WeaponSystem>();
            _health = GetComponent<HealthComponent>();
            _mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.OnDied.AddListener(HandleDeath);
            }

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
            if (_health != null)
            {
                _health.OnDied.RemoveListener(HandleDeath);
            }

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
            UpdateDeviceType(context);
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            _lookInput = context.ReadValue<Vector2>();
            UpdateDeviceType(context);
        }

        private void OnFire(InputAction.CallbackContext context)
        {
            _isFiring = context.ReadValueAsButton();
            UpdateDeviceType(context);
        }

        private void UpdateDeviceType(InputAction.CallbackContext context)
        {
            if (context.control != null)
            {
                _isUsingGamepad = context.control.device is Gamepad;
            }
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
            if (_isUsingGamepad)
            {
                // Gamepad dual-stick logic gives an absolute direction
                if (_lookInput != Vector2.zero)
                {
                    _movement.LookAt(_lookInput);
                }
            }
            else
            {
                // Keyboard & Mouse logic: Character should look at the pointer position
                if (Pointer.current != null)
                {
                    Vector2 pointerScreenPos = Pointer.current.position.ReadValue();
                    
                    // The absolute distance from the camera on the Z axis
                    float zDistance = Mathf.Abs(_mainCamera.transform.position.z - transform.position.z);
                    Vector3 worldPointerPos = _mainCamera.ScreenToWorldPoint(new Vector3(pointerScreenPos.x, pointerScreenPos.y, zDistance));
                    
                    Vector2 lookDirection = (worldPointerPos - transform.position).normalized;
                    _movement.LookAt(lookDirection);
                }
            }
        }

        private void HandleShooting()
        {
            if (_isFiring)
            {
                _weaponSystem.Fire();
            }
        }

        private void HandleDeath()
        {
            // Trigger game over state if GameManager exists
            if (TopDownShooter.Controllers.Game.GameManager.Instance != null)
            {
                TopDownShooter.Controllers.Game.GameManager.Instance.TriggerGameOver();
            }
            
            // Disable the player to visually "die" and stop processing updates
            gameObject.SetActive(false);
        }
    }
}
