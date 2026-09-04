using UnityEngine;

namespace ProjectBloodbath.Prototype
{
    internal static class PrototypeHudSkin
    {
        private const string MetalTextureResourceName = "RetroHudBiomedMetal";
        private const float DefaultTileSize = 256f;

        private static Texture2D metalTexture;
        private static Texture2D discTexture;

        public static Texture2D MetalTexture
        {
            get
            {
                if (metalTexture == null)
                {
                    metalTexture = Resources.Load<Texture2D>(
                        MetalTextureResourceName);
                }

                return metalTexture;
            }
        }

        public static void DrawTiledTexture(
            Rect rect,
            Color tint,
            float tileSize = DefaultTileSize)
        {
            Texture2D texture = MetalTexture;
            if (texture == null || rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            float safeTileSize = Mathf.Max(1f, tileSize);
            Rect textureCoordinates = new(
                rect.x / safeTileSize,
                rect.y / safeTileSize,
                rect.width / safeTileSize,
                rect.height / safeTileSize);
            Color previousColor = GUI.color;
            GUI.color = tint;
            GUI.DrawTextureWithTexCoords(
                rect,
                texture,
                textureCoordinates,
                true);
            GUI.color = previousColor;
        }

        public static void DrawTiledNotchedTexture(
            Rect rect,
            Color tint,
            float notch,
            float tileSize = DefaultTileSize)
        {
            float safeNotch = Mathf.Clamp(
                notch,
                0f,
                Mathf.Min(rect.width, rect.height) * 0.5f);
            DrawTiledTexture(
                new Rect(
                    rect.x + safeNotch,
                    rect.y,
                    rect.width - safeNotch * 2f,
                    safeNotch),
                tint,
                tileSize);
            DrawTiledTexture(
                new Rect(
                    rect.x,
                    rect.y + safeNotch,
                    rect.width,
                    rect.height - safeNotch * 2f),
                tint,
                tileSize);
            DrawTiledTexture(
                new Rect(
                    rect.x + safeNotch,
                    rect.yMax - safeNotch,
                    rect.width - safeNotch * 2f,
                    safeNotch),
                tint,
                tileSize);
        }

        public static void DrawDisplayGlass(Rect rect, float alpha = 1f)
        {
            DrawSolidRect(
                rect,
                new Color(0.018f, 0.033f, 0.038f, alpha));
            DrawSolidRect(
                new Rect(rect.x + 2f, rect.y + 2f,
                    rect.width - 4f, 1f),
                new Color(0.46f, 0.65f, 0.64f, alpha * 0.2f));

            const float scanlineSpacing = 8f;
            for (float y = rect.y + 6f; y < rect.yMax - 3f;
                y += scanlineSpacing)
            {
                DrawSolidRect(
                    new Rect(rect.x + 4f, y, rect.width - 8f, 1f),
                    new Color(0.31f, 0.46f, 0.47f, alpha * 0.035f));
            }
        }

        public static void DrawSprite(Rect rect, Sprite sprite)
        {
            if (sprite == null || sprite.texture == null)
            {
                return;
            }

            Rect textureRect = sprite.textureRect;
            Texture2D texture = sprite.texture;
            Rect textureCoordinates = new(
                textureRect.x / texture.width,
                textureRect.y / texture.height,
                textureRect.width / texture.width,
                textureRect.height / texture.height);
            GUI.DrawTextureWithTexCoords(
                rect,
                texture,
                textureCoordinates,
                true);
        }

        public static void DrawDisc(Rect rect, Color tint)
        {
            Texture2D texture = DiscTexture;
            if (texture == null || rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            Color previousColor = GUI.color;
            GUI.color = tint;
            GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
            GUI.color = previousColor;
        }

        public static void DrawPromptFrame(Rect rect)
        {
            DrawTiledNotchedTexture(
                rect,
                new Color(0.42f, 0.5f, 0.5f, 0.72f),
                7f,
                256f);
            Rect inner = new(
                rect.x + 3f,
                rect.y + 3f,
                rect.width - 6f,
                rect.height - 6f);
            DrawSolidRect(inner, new Color(0.018f, 0.035f, 0.04f, 0.94f));
            DrawSolidRect(
                new Rect(inner.x + 8f, inner.y + 2f,
                    inner.width - 16f, 1f),
                new Color(0.55f, 0.72f, 0.68f, 0.48f));
        }

        private static void DrawSolidRect(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previousColor;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCachedTexture()
        {
            metalTexture = null;
            discTexture = null;
        }

        private static Texture2D DiscTexture
        {
            get
            {
                if (discTexture != null)
                {
                    return discTexture;
                }

                const int size = 64;
                Color32[] pixels = new Color32[size * size];
                float centre = (size - 1) * 0.5f;
                float radius = centre - 1f;
                float feather = 1.5f;
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float distance = Vector2.Distance(
                            new Vector2(x, y),
                            new Vector2(centre, centre));
                        byte alpha = (byte)Mathf.RoundToInt(
                            Mathf.Clamp01((radius - distance) / feather) * 255f);
                        pixels[y * size + x] =
                            new Color32(255, 255, 255, alpha);
                    }
                }

                discTexture = new Texture2D(
                    size,
                    size,
                    TextureFormat.RGBA32,
                    false)
                {
                    name = "PrototypeHudDisc",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.HideAndDontSave
                };
                discTexture.SetPixels32(pixels);
                discTexture.Apply(false, true);
                return discTexture;
            }
        }
    }
}
