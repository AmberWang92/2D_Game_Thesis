using UnityEngine;

namespace TopDownShooter.Services
{
    /// <summary>
    /// Lightweight camera follow for the player. Frame-rate independent smoothing.
    /// Replace with Cinemachine later if desired.
    /// </summary>
    [DisallowMultipleComponent]
    public class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(0f)] private float smoothing = 10f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

        public void SetTarget(Transform t) => target = t;

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 desired = target.position + offset;
            float k = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, k);
        }
    }
}
