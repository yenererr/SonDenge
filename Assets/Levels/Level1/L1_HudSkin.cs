using UnityEngine;
using UnityEngine.UI;

namespace Jam.Levels.Level1
{
    /// <summary>Level 1 HUD için çalışma anında üretilen dokulu, dokuz dilimli ahşap/kâğıt kaplamaları.</summary>
    internal static class L1_HudSkin
    {
        internal enum Surface { FramedPaper, Paper, Wood, PaperButton, CopperButton, DarkButton, Coin }

        private static readonly Sprite[] Sprites = new Sprite[7];
        private static readonly Texture2D[] Textures = new Texture2D[7];

        internal static void Apply(Image image, Surface surface)
        {
            if (image == null) return;
            var index = (int)surface;
            if (Sprites[index] == null) Sprites[index] = Create(surface);
            image.sprite = Sprites[index];
            image.type = surface == Surface.Coin ? Image.Type.Simple : Image.Type.Sliced;
            image.color = Color.white;
            var outline = image.GetComponent<Outline>();
            if (outline != null) outline.enabled = false;
        }

        internal static void Button(Button button, Surface surface)
        {
            if (button == null) return;
            Apply(button.targetGraphic as Image, surface);
            var caption = button.GetComponentInChildren<Text>();
            if (caption != null) caption.color = surface == Surface.CopperButton || surface == Surface.DarkButton
                ? new Color32(255, 241, 215, 255) : new Color32(55, 35, 23, 255);
        }

        internal static Sprite Coin
        {
            get
            {
                var index = (int)Surface.Coin;
                if (Sprites[index] == null) Sprites[index] = Create(Surface.Coin);
                return Sprites[index];
            }
        }

        private static Sprite Create(Surface surface)
        {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "L1 HUD " + surface,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave
            };
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
                pixels[y * size + x] = Pixel(surface, x, y);
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            Textures[(int)surface] = texture;
            var border = surface == Surface.Coin ? Vector4.zero : new Vector4(19f, 19f, 19f, 19f);
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f,
                0u, SpriteMeshType.FullRect, border);
            sprite.name = "L1 HUD " + surface;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        private static Color32 Pixel(Surface surface, int x, int y)
        {
            const int size = 128;
            var edge = Mathf.Min(x, y, size - 1 - x, size - 1 - y);
            var grain = Mathf.Sin(y * 0.37f + Mathf.Sin(x * 0.055f) * 2.2f) * 4f +
                Mathf.PerlinNoise(x * 0.018f + 2.3f, y * 0.12f + 5.7f) * 11f + Hash(x, y) * 5f;
            var paperNoise = Mathf.PerlinNoise(x * 0.035f + 17f, y * 0.04f + 29f) * 12f + Hash(x, y) * 4f;

            if (surface == Surface.Coin)
            {
                var distance = Vector2.Distance(new Vector2(x, y), new Vector2(63.5f, 63.5f));
                if (distance > 58f) return new Color32(0, 0, 0, 0);
                if (distance > 52f) return new Color32(95, 54, 20, 255);
                if (distance > 45f) return new Color32(245, 187, 55, 255);
                if (distance > 39f) return new Color32(139, 83, 25, 255);
                return new Color32((byte)(230 + Hash(x, y) * 16f), (byte)(171 + Hash(x, y) * 12f), 51, 255);
            }

            // Hafif kırpılmış köşeler ve kenar gölgeleri, 9-slice ile her panelde korunur.
            if (edge < 3 && (x < 8 || x > 119) && (y < 8 || y > 119)) return new Color32(0, 0, 0, 0);
            if (surface == Surface.FramedPaper)
            {
                // Perçinler ahşap kenarın üzerinde durmalı; iç kâğıt alanında kaybolmamalı.
                var cx = x < 64 ? 12 : 115;
                var cy = y < 64 ? 12 : 115;
                if ((x - cx) * (x - cx) + (y - cy) * (y - cy) < 13)
                    return new Color32(49, 38, 31, 255);
                if (edge < 5) return WoodColor(grain - 32f);
                if (edge < 13) return WoodColor(grain + (edge < 8 ? 5f : 20f));
                if (edge < 16) return new Color32(71, 42, 25, 255);
                if (edge < 19) return new Color32(251, 226, 187, 255);
                return PaperColor(paperNoise, edge);
            }
            if (surface == Surface.Paper)
            {
                if (edge < 3) return new Color32(113, 75, 46, 255);
                if (edge < 6) return new Color32(228, 194, 150, 255);
                return PaperColor(paperNoise, edge);
            }
            if (surface == Surface.Wood || surface == Surface.DarkButton)
            {
                if (edge < 4) return new Color32(48, 30, 22, 255);
                if (edge < 8) return WoodColor(grain + 12f);
                return WoodColor(grain - (surface == Surface.DarkButton ? 12f : 0f));
            }
            if (surface == Surface.PaperButton || surface == Surface.CopperButton)
            {
                if (edge < 4) return new Color32(67, 42, 29, 255);
                if (edge < 8) return WoodColor(grain + 8f);
                if (edge < 11) return surface == Surface.CopperButton
                    ? new Color32(184, 114, 67, 255) : new Color32(255, 237, 205, 255);
                if (surface == Surface.PaperButton) return PaperColor(paperNoise, edge);
                var shade = Mathf.Clamp(8f - paperNoise * 0.4f + (y < 64 ? 8f : -4f), -12f, 17f);
                return new Color32((byte)(176 + shade), (byte)(104 + shade * 0.65f), (byte)(60 + shade * 0.45f), 255);
            }
            return Color.white;
        }

        private static Color32 WoodColor(float grain) => new Color32(
            (byte)Mathf.Clamp(96f + grain, 0f, 255f),
            (byte)Mathf.Clamp(59f + grain * 0.72f, 0f, 255f),
            (byte)Mathf.Clamp(36f + grain * 0.52f, 0f, 255f), 255);

        private static Color32 PaperColor(float noise, float edge)
        {
            var age = 16f * (1f - Mathf.SmoothStep(18f, 44f, edge));
            var variation = noise + age;
            return new Color32((byte)(248f - variation * 0.44f), (byte)(226f - variation * 0.74f),
                (byte)(192f - variation), 255);
        }

        private static float Hash(int x, int y)
        {
            var n = unchecked((uint)(x * 374761393 + y * 668265263));
            n = unchecked((n ^ (n >> 13)) * 1274126177u);
            return (n & 0xffffu) / 65535f;
        }
    }
}
