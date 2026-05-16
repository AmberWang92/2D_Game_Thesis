using UnityEngine;

namespace TopDownShooter.Runtime.Targeting
{
    public sealed class TransformTargetProvider : MonoBehaviour, ITargetProvider
    {
        [SerializeField] private Transform target;

        public bool HasTarget => target != null && target.gameObject.activeInHierarchy;
        public Vector2 Position => target != null ? target.position : transform.position;
        public GameObject TargetObject => target != null ? target.gameObject : null;

        public void SetTarget(Transform target)
        {
            this.target = target;
        }
    }
}
