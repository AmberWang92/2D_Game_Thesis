using TopDownShooter.Runtime.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TopDownShooter.UnityAdapters.Input
{
    public sealed class InputSystemPlayerInput : MonoBehaviour, IPlayerInput
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private InputActionReference attackAction;
        [SerializeField] private bool useMouseFireFallback = true;

        private InputAction resolvedMoveAction;
        private InputAction resolvedLookAction;
        private InputAction resolvedAttackAction;

        public Vector2 Move => resolvedMoveAction != null ? resolvedMoveAction.ReadValue<Vector2>() : Vector2.zero;
        public Vector2 AimWorldPosition { get; private set; }
        public bool IsFireHeld => (resolvedAttackAction != null && resolvedAttackAction.IsPressed()) || (useMouseFireFallback && Mouse.current != null && Mouse.current.leftButton.isPressed);

        private void Awake()
        {
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }

            PlayerInput playerInput = GetComponent<PlayerInput>();
            resolvedMoveAction = ResolveAction(moveAction, playerInput, "Move");
            resolvedLookAction = ResolveAction(lookAction, playerInput, "Look");
            resolvedAttackAction = ResolveAction(attackAction, playerInput, "Attack");
        }

        private void OnEnable()
        {
            resolvedMoveAction?.Enable();
            resolvedLookAction?.Enable();
            resolvedAttackAction?.Enable();
        }

        private void OnDisable()
        {
            resolvedMoveAction?.Disable();
            resolvedLookAction?.Disable();
            resolvedAttackAction?.Disable();
        }

        private void Update()
        {
            AimWorldPosition = ReadAimWorldPosition();
        }

        private Vector2 ReadAimWorldPosition()
        {
            if (worldCamera == null)
            {
                return transform.position;
            }

            if (Mouse.current != null)
            {
                Vector2 mousePosition = Mouse.current.position.ReadValue();
                Vector3 worldPosition = worldCamera.ScreenToWorldPoint(mousePosition);
                return worldPosition;
            }

            if (resolvedLookAction != null)
            {
                Vector2 look = resolvedLookAction.ReadValue<Vector2>();

                if (look.sqrMagnitude > 0.001f)
                {
                    return (Vector2)transform.position + look.normalized;
                }
            }

            return AimWorldPosition;
        }

        private static InputAction ResolveAction(InputActionReference actionReference, PlayerInput playerInput, string actionName)
        {
            if (actionReference != null)
            {
                return actionReference.action;
            }

            return playerInput != null ? playerInput.actions.FindAction(actionName, false) : null;
        }
    }
}
