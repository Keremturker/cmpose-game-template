using UnityEngine;

namespace Kturker.UnityBase.Features.Showcase.Runner2D
{
    // White shapes made in code (the template ships no art): a one-unit square and a one-unit circle, tinted by the
    // renderer. The caller destroys the sprite and its texture.
    public static class ShapeSprites
    {
        public static Sprite Square()
        {
            const int size = 4;
            var texture = NewTexture(size, FilterMode.Point);
            var pixels = new Color32[size * size];
            for (var i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite Circle()
        {
            const int size = 64;
            var texture = NewTexture(size, FilterMode.Bilinear);
            var pixels = new Color32[size * size];
            var centre = (size - 1) / 2f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var distance = Mathf.Sqrt((x - centre) * (x - centre) + (y - centre) * (y - centre));
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(size / 2f - distance) * 255f));
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static void Destroy(Sprite sprite)
        {
            if (sprite == null) return;
            Object.Destroy(sprite.texture);
            Object.Destroy(sprite);
        }

        private static Texture2D NewTexture(int size, FilterMode filter) =>
            new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = filter, wrapMode = TextureWrapMode.Clamp };
    }
}
