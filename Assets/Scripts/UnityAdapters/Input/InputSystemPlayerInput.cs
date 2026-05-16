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

        public Vector2 Move => moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        public Vector2 AimWorldPosition { get; private set; }
        public bool IsFireHeld => attackAction != null && attackAction.action.IsPressed();

        private void Awake()
        {
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }
        }

        private void OnEnable()
        {
            moveAction?.action.Enable();
            lookAction?.action.Enable();
            attackAction?.action.Enable();
        }

        private void OnDisable()
        {
            moveAction?.action.Disable();
            lookAction?.action.Disable();
            attackAction?.action.Disable();
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

            if (lookAction != null)
            {
                Vector2 look = lookAction.action.ReadValue<Vector2>();

                if (look.sqrMagnitude > 0.001f)
                {
                    return (Vector2)transform.position + look.normalized;
                }
            }

            return AimWorldPosition;
        }
    }
}
