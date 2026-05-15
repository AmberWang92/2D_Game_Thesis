using UnityEngine;
using UnityEngine.InputSystem;

namespace TopDownShooter.Gameplay.Player
{
    /// <summary>
    /// Wraps the new Input System InputActionReferences used by the player.
    /// Exposes only what gameplay needs (move vector, aim source, fire flags),
    /// so PlayerController never touches Input System APIs directly.
    ///
    /// Aim sources:
    /// - <see cref="HasStickAim"/> + <see cref="StickAim"/> — gamepad right-stick direction.
    /// - <see cref="HasPointerAim"/> + <see cref="PointerWorld"/> — absolute pointer
    ///   position projected to world space (read from <c>Mouse.current.position</c>
    ///   because the existing input asset binds <c>Look</c> to pointer <i>delta</i>).
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionReference moveAction;
        [Tooltip("Optional gamepad-stick aim. Mouse aim is sourced from Mouse.current.position regardless.")]
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private InputActionReference attackAction;

        [Tooltip("Camera used to project pointer position into world space. Defaults to Camera.main.")]
        [SerializeField] private Camera worldCamera;

        [Tooltip("Stick magnitude above which gamepad aim overrides pointer aim.")]
        [SerializeField, Range(0f, 1f)] private float stickDeadzone = 0.3f;

        public Vector2 Move { get; private set; }
        public Vector2 StickAim { get; private set; }
        public bool HasStickAim { get; private set; }
        /// <summary>Pointer position projected onto z=0 in world space.</summary>
        public Vector2 PointerWorld { get; private set; }
        public bool HasPointerAim { get; private set; }
        public bool FirePressed { get; private set; }
        public bool FireHeld { get; private set; }

        private void OnEnable()
        {
            EnableAction(moveAction);
            EnableAction(lookAction);
            EnableAction(attackAction);
        }

        private void OnDisable()
        {
            DisableAction(moveAction);
            DisableAction(lookAction);
            DisableAction(attackAction);
        }

        private void Update()
        {
            Move = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;

            // Gamepad stick aim: read directly from device so we don't conflict with
            // the Look action being used as pointer delta on mouse.
            var pad = Gamepad.current;
            Vector2 stick = pad != null ? pad.rightStick.ReadValue() : Vector2.zero;
            HasStickAim = stick.magnitude >= stickDeadzone;
            StickAim = HasStickAim ? stick.normalized : Vector2.zero;

            // Pointer aim: absolute screen position from Mouse, projected to world.
            var cam = worldCamera != null ? worldCamera : Camera.main;
            var mouse = Mouse.current;
            if (mouse != null && cam != null)
            {
                Vector2 screen = mouse.position.ReadValue();
                Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -cam.transform.position.z));
                PointerWorld = world;
                HasPointerAim = true;
            }
            else
            {
                HasPointerAim = false;
            }

            if (attackAction != null)
            {
                FirePressed = attackAction.action.WasPressedThisFrame();
                FireHeld = attackAction.action.IsPressed();
            }
            else
            {
                FirePressed = false;
                FireHeld = false;
            }
        }

        private static void EnableAction(InputActionReference reference)
        {
            if (reference != null && reference.action != null) reference.action.Enable();
        }

        private static void DisableAction(InputActionReference reference)
        {
            if (reference != null && reference.action != null) reference.action.Disable();
        }
    }
}
