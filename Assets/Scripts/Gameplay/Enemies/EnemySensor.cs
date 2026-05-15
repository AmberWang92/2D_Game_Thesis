using UnityEngine;

namespace TopDownShooter.Gameplay.Enemies
{
    /// <summary>
    /// Periodic overlap-circle scan for a target on the configured layer mask.
    /// Cached, allocation-free, throttled to <see cref="scanInterval"/> seconds.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemySensor : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float radius = 10f;
        [SerializeField] private LayerMask targetMask;
        [SerializeField, Min(0.02f)] private float scanInterval = 0.15f;
        [SerializeField, Min(1)] private int maxResults = 4;

        private Collider2D[] _hits;
        private float _nextScanTime;

        public Transform Target { get; private set; }
        public float Radius => radius;

        public void SetRadius(float r) => radius = Mathf.Max(0.1f, r);
        public void SetTargetMask(LayerMask mask) => targetMask = mask;

        private void Awake()
        {
            _hits = new Collider2D[Mathf.Max(1, maxResults)];
        }

        private void Update()
        {
            if (Time.time < _nextScanTime) return;
            _nextScanTime = Time.time + scanInterval;
            Scan();
        }

        private void Scan()
        {
            var filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = targetMask,
                useTriggers = true
            };

            int count = Physics2D.OverlapCircle(transform.position, radius, filter, _hits);
            if (count <= 0)
            {
                Target = null;
                return;
            }

            Transform best = null;
            float bestSq = float.MaxValue;
            Vector2 self = transform.position;

            for (int i = 0; i < count; i++)
            {
                var c = _hits[i];
                if (c == null) continue;
                var rb = c.attachedRigidbody;
                var t = rb != null ? rb.transform : c.transform;
                float d = ((Vector2)t.position - self).sqrMagnitude;
                if (d < bestSq)
                {
                    bestSq = d;
                    best = t;
                }
            }
            Target = best;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
