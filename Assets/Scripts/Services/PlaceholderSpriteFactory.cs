using UnityEngine;

namespace TopDownShooter.Services
{
    /// <summary>
    /// Generates a colored circle/square sprite at runtime and assigns it to the
    /// SpriteRenderer on this GameObject. Lets the project run without art assets.
    /// Drop on Player / Enemy / Bullet prefabs that have an empty SpriteRenderer.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [DisallowMultipleComponent]
    public class PlaceholderSpriteFactory : MonoBehaviour
    {
        public enum Shape { Circle, Square }

        [SerializeField] private Shape shape = Shape.Circle;
        [SerializeField] private Color color = Color.white;
        [SerializeField, Range(8, 256)] private int pixelSize = 64;
        [SerializeField] private bool overrideExisting = false;

        private void Awake()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) return;
            if (sr.sprite != null && !overrideExisting) return;
            sr.sprite = Build(shape, color, pixelSize);
        }

        public static Sprite Build(Shape shape, Color color, int size)
        {
            size = Mathf.Clamp(size, 8, 256);
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = $"Placeholder_{shape}_{ColorUtility.ToHtmlStringRGBA(color)}"
            };

            var pixels = new Color32[size * size];
            Color32 fill = color;
            Color32 clear = new Color32(0, 0, 0, 0);
            float r = size * 0.5f - 0.5f;
            float r2 = r * r;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inside = shape == Shape.Square ||
                                  ((x - r) * (x - r) + (y - r) * (y - r) <= r2);
                    pixels[y * size + x] = inside ? fill : clear;
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, true);

            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
