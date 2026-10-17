using UnityEngine;

namespace Jam.Levels.Level1
{
    /// <summary>Shared, self-contained parchment skin for the game's immediate-mode UI.</summary>
    internal static class L1_PaperGui
    {
        internal static readonly Color Ink = new Color32(57, 39, 26, 255);
        internal static readonly Color MutedInk = new Color32(110, 79, 53, 255);
        internal static readonly Color Edge = new Color32(113, 70, 38, 255);
        internal static readonly Color Parchment = new Color32(255, 249, 229, 255);
        internal static readonly Color Slip = new Color32(245, 228, 190, 255);

        private static Texture2D texture;

        private static Texture2D Texture
        {
            get
            {
                if (texture == null) CreateTexture();
                return texture;
            }
        }

        private static void CreateTexture()
        {
            const int size = 256;
            texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Weathered parchment UI",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave
            };
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var edge = Mathf.Min(x, y, size - 1 - x, size - 1 - y);
                var ragged = Mathf.PerlinNoise(x * 0.17f + 37f, y * 0.17f + 11f) * 2.6f;
                var age = 1f - Mathf.SmoothStep(0f, 25f, edge);
                var cloud = Mathf.PerlinNoise(x * 0.019f + 19f, y * 0.022f + 31f);
                var fiber = Mathf.PerlinNoise(x * 0.08f + 72f, y * 0.38f + 13f);
                var grain = Hash(x, y);
                var fleck = grain > 0.994f ? 32f : 0f;
                var stain = Mathf.Max(0f, Mathf.PerlinNoise(x * 0.043f + 106f, y * 0.043f + 207f) - 0.64f) * 75f;
                var wear = age * (31f + ragged * 7f) + cloud * 11f + fiber * 4f + grain * 6f + stain + fleck;
                var alpha = edge < 1.4f + ragged * 0.5f ? (byte)0 : (byte)255;
                pixels[y * size + x] = new Color32(
                    (byte)Mathf.Clamp(251f - wear * 0.64f, 0f, 255f),
                    (byte)Mathf.Clamp(232f - wear * 0.86f, 0f, 255f),
                    (byte)Mathf.Clamp(190f - wear, 0f, 255f), alpha);
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
        }

        private static float Hash(int x, int y)
        {
            var n = unchecked((uint)(x * 374761393 + y * 668265263));
            n = unchecked((n ^ (n >> 13)) * 1274126177u);
            return (n & 0xffffu) / 65535f;
        }

        internal static void Fill(Rect rect, Color color)
        {
            if (rect.width <= 0f || rect.height <= 0f) return;
            var previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        internal static void Panel(Rect rect, Color tint, Color border)
        {
            if (rect.width <= 2f || rect.height <= 2f) return;
            Fill(new Rect(rect.x + 4f, rect.y + 5f, rect.width, rect.height), new Color(0.06f, 0.035f, 0.02f, 0.34f));
            Fill(rect, border);
            var face = new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f);
            var previous = GUI.color;
            GUI.color = tint;
            GUI.DrawTexture(face, Texture);
            GUI.color = previous;
            var rule = new Color(border.r, border.g, border.b, 0.52f);
            if (rect.height > 34f && rect.width > 46f)
            {
                Fill(new Rect(rect.x + 8f, rect.y + 8f, rect.width - 16f, 1f), rule);
                Fill(new Rect(rect.x + 8f, rect.yMax - 9f, rect.width - 16f, 1f), rule);
            }
            if (rect.height > 34f && rect.width > 46f)
            {
                Fill(new Rect(rect.x + 8f, rect.y + 9f, 1f, rect.height - 18f), rule);
                Fill(new Rect(rect.xMax - 9f, rect.y + 9f, 1f, rect.height - 18f), rule);
                Fill(new Rect(rect.x + 6f, rect.y + 6f, 5f, 5f), border);
                Fill(new Rect(rect.xMax - 11f, rect.y + 6f, 5f, 5f), border);
                Fill(new Rect(rect.x + 6f, rect.yMax - 11f, 5f, 5f), border);
                Fill(new Rect(rect.xMax - 11f, rect.yMax - 11f, 5f, 5f), border);
            }
        }

        internal static void Rule(float x, float y, float width, Color color)
        {
            Fill(new Rect(x, y, width, 1f), color);
            Fill(new Rect(x + width * 0.5f - 3f, y - 2f, 6f, 5f), color);
        }

        internal static void Release()
        {
            if (texture == null) return;
            if (Application.isPlaying) Object.Destroy(texture);
            else Object.DestroyImmediate(texture);
            texture = null;
        }
    }
}
