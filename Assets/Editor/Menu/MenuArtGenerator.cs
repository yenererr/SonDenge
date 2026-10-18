using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Jam.EditorTools
{
    /// <summary>
    /// Menü UI sanatını (ahşap/parşömen 9-slice sprite'lar, beyaz ikonlar, kısa ahşap UI sesleri) kodla üretir.
    /// 3. parti indirme yok; çıktı: Assets/Prefabs/UI/Menu/Art/. Tekrar çalıştırılabilir (aynı dosyaları yeniler, GUID korunur).
    /// Menü: Jam > Menü > Sanat üret
    /// </summary>
    public static class MenuArtGenerator
    {
        public const string Root = "Assets/Prefabs/UI/Menu/Art";
        public const string SpriteDir = Root + "/Sprites";
        public const string IconDir = Root + "/Icons";
        public const string AudioDir = Root + "/Audio";

        // Şartname §2.1 paleti
        private static readonly Color WoodDark = Hex("503022"), WoodMid = Hex("80502D"), WoodLight = Hex("B37C42");
        private static readonly Color Parchment = Hex("F8E8C9"), ParchmentLight = Hex("FFF3DA"), Ink = Hex("33251C");
        private static readonly Color Gold = Hex("F4B640"), GoldLight = Hex("FFD978");

        [MenuItem("Jam/Menü/Sanat üret")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(SpriteDir);
            Directory.CreateDirectory(IconDir);
            Directory.CreateDirectory(AudioDir);
            GenerateSprites();
            GenerateIcons();
            GenerateAudio();
            AssetDatabase.Refresh();
            Debug.Log("[Menü] Sanat üretildi: " + Root);
        }

        // ------------------------------------------------------------------ 9-slice sprite'lar
        private static void GenerateSprites()
        {
            Save(SpriteDir, "wood_plank", WoodPlank(512, 132, WoodMid, WoodLight, WoodDark, 22, true), 46);
            Save(SpriteDir, "wood_plank_gold", WoodPlank(512, 132, Hex("E9A934"), GoldLight, Hex("8A5418"), 22, true), 46);
            Save(SpriteDir, "wood_sign", WoodSign(768, 288), 96);
            Save(SpriteDir, "wood_panel", WoodPlank(256, 256, Hex("6B4026"), Hex("8E5A32"), Hex("3E2416"), 26, true, true), 64);
            Save(SpriteDir, "parchment", ParchmentTex(512, 512, Parchment, 26, 0.12f), 96);
            Save(SpriteDir, "parchment_soft", ParchmentTex(256, 256, ParchmentLight, 20, 0.05f, Hex("D9C29A")), 44);
            Save(SpriteDir, "bubble", Bubble(256), 76);
            Save(SpriteDir, "bubble_tail", BubbleTail(128), 0);
            Save(SpriteDir, "card_frame", CardFrame(256), 56);
            Save(SpriteDir, "ribbon", WoodPlank(256, 96, Hex("D8D8D8"), Color.white, Hex("8C8C8C"), 14, false), 32);
            Save(SpriteDir, "glow", Glow(256), 110);
            Save(SpriteDir, "vignette", Vignette(256), 0);
            Save(SpriteDir, "bar_trough", BarTrough(256, 64), 30);
            Save(SpriteDir, "bar_fill", BarFill(256, 64), 30);
            Save(SpriteDir, "pill", RoundRectTex(128, 64, 31, Color.white, 0f), 31);
            Save(SpriteDir, "knob", Knob(64), 0);
            Save(SpriteDir, "keycap", Keycap(128), 44);
            Save(SpriteDir, "field", ParchmentTex(256, 80, Color.white, 12, 0.02f, Hex("B49A72")), 30);
            Save(SpriteDir, "round_white", RoundRectTex(64, 64, 14, Color.white, 0f), 20);
            Save(SpriteDir, "soft_shadow", SoftShadow(128), 56);
            Save(SpriteDir, "divider", Divider(512, 40), 0);
            Save(SpriteDir, "leaf", Leaves(128), 0);
            Save(SpriteDir, "nail", Nail(32), 0);
        }

        private static Texture2D WoodPlank(int w, int h, Color baseCol, Color light, Color rim, float radius, bool nails,
            bool seams = false)
        {
            var c = new Px(w, h);
            float seed = UnityEngine.Random.Range(0f, 100f);
            for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
            {
                var d = SdRoundBox(x + 0.5f - w * 0.5f, y + 0.5f - h * 0.5f, w * 0.5f - 2f, h * 0.5f - 2f, radius);
                var cov = Mathf.Clamp01(0.5f - d);
                if (cov <= 0f) continue;
                // Lif: yatay sinüs bantları, Perlin ile bükülür.
                var warp = Mathf.PerlinNoise(x * 0.008f + seed, y * 0.06f) * 9f;
                var grain = 0.5f + 0.5f * Mathf.Sin(y * 0.34f + warp);
                var fine = Mathf.PerlinNoise(x * 0.05f, y * 0.9f + seed);
                var t = Mathf.Clamp01(0.25f + grain * 0.45f + fine * 0.3f);
                var col = Color.Lerp(baseCol, light, t * 0.75f);
                if (seams && h > 140 && Mathf.Abs(Mathf.Repeat(y, h / 3f) - 1f) < 1.6f) col = Color.Lerp(col, rim, 0.7f);
                // Kenar gölgesi + üst ışık (bevel).
                var edge = Mathf.Clamp01(-d / 9f);
                col = Color.Lerp(rim, col, Mathf.SmoothStep(0f, 1f, edge));
                var vy = (y + 0.5f) / h;
                col = Color.Lerp(col, Color.Lerp(col, Color.white, 0.18f), Mathf.Clamp01((vy - 0.8f) * 5f) * edge);
                col = Color.Lerp(col, col * 0.78f, Mathf.Clamp01((0.18f - vy) * 5f));
                col.a = cov;
                c.Set(x, y, col);
            }
            if (nails)
            {
                var inset = Mathf.Min(24f, h * 0.22f);
                foreach (var p in new[] { new Vector2(inset, inset), new Vector2(w - inset, inset), new Vector2(inset, h - inset), new Vector2(w - inset, h - inset) })
                    DrawNail(c, p.x, p.y, Mathf.Max(4.5f, h * 0.045f));
            }
            return c.ToTexture();
        }

        private static Texture2D WoodSign(int w, int h)
        {
            var c = new Px(w, h);
            float seed = UnityEngine.Random.Range(0f, 100f);
            var planks = 3;
            for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
            {
                // Hafif düzensiz kenar
                var jag = (Mathf.PerlinNoise(x * 0.02f, seed) - 0.5f) * 6f;
                var d = SdRoundBox(x + 0.5f - w * 0.5f, y + 0.5f - h * 0.5f, w * 0.5f - 6f, h * 0.5f - 6f + jag, 30f);
                var cov = Mathf.Clamp01(0.5f - d);
                if (cov <= 0f) continue;
                var row = Mathf.FloorToInt(y / (float)h * planks);
                var local = Mathf.Repeat(y, h / (float)planks);
                var warp = Mathf.PerlinNoise(x * 0.006f + row * 13f + seed, y * 0.05f) * 10f;
                var grain = 0.5f + 0.5f * Mathf.Sin(y * 0.28f + warp);
                var fine = Mathf.PerlinNoise(x * 0.04f + row, y * 0.7f);
                var col = Color.Lerp(Hex("6E4226"), Hex("A06A3A"), Mathf.Clamp01(0.2f + grain * 0.5f + fine * 0.3f));
                if (local < 2.2f || local > h / (float)planks - 2.2f) col = Color.Lerp(col, Hex("2E1A0E"), 0.75f);
                var edge = Mathf.Clamp01(-d / 12f);
                col = Color.Lerp(Hex("2E1A0E"), col, Mathf.SmoothStep(0f, 1f, edge));
                col.a = cov;
                c.Set(x, y, col);
            }
            foreach (var p in new[] { new Vector2(34, 34), new Vector2(w - 34, 34), new Vector2(34, h - 34), new Vector2(w - 34, h - 34) })
                DrawNail(c, p.x, p.y, 8f);
            return c.ToTexture();
        }

        private static void DrawNail(Px c, float cx, float cy, float r)
        {
            c.Fill((x, y) => SdCircle(x, y, cx, cy - 1.5f, r + 1.5f), new Color(0f, 0f, 0f, 0.35f));
            c.Fill((x, y) => SdCircle(x, y, cx, cy, r), Hex("4A4A4F"));
            c.Fill((x, y) => SdCircle(x, y, cx - r * 0.25f, cy + r * 0.25f, r * 0.45f), Hex("B9B9C0"));
        }

        private static Texture2D ParchmentTex(int w, int h, Color baseCol, float radius, float burn, Color? outline = null)
        {
            var c = new Px(w, h);
            float seed = UnityEngine.Random.Range(0f, 100f);
            for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
            {
                var jag = burn > 0.08f ? (Mathf.PerlinNoise(x * 0.03f + seed, y * 0.03f) - 0.5f) * 8f : 0f;
                var d = SdRoundBox(x + 0.5f - w * 0.5f, y + 0.5f - h * 0.5f, w * 0.5f - 4f, h * 0.5f - 4f, radius) + jag;
                var cov = Mathf.Clamp01(0.5f - d);
                if (cov <= 0f) continue;
                var n = Mathf.PerlinNoise(x * 0.02f + seed, y * 0.02f) * 0.6f + Mathf.PerlinNoise(x * 0.12f, y * 0.12f + seed) * 0.4f;
                var col = Color.Lerp(baseCol, baseCol * new Color(0.93f, 0.88f, 0.8f, 1f), n * (0.4f + burn * 3f));
                var edge = Mathf.Clamp01(-d / Mathf.Max(6f, radius * 1.4f));
                col = Color.Lerp(Color.Lerp(baseCol, Hex("B88A52"), Mathf.Clamp01(burn * 5f)), col, Mathf.SmoothStep(0f, 1f, edge));
                if (outline.HasValue && d > -2.5f) col = outline.Value;
                col.a = cov;
                c.Set(x, y, col);
            }
            return c.ToTexture();
        }

        private static Texture2D Bubble(int s)
        {
            var c = new Px(s, s);
            c.Fill((x, y) => SdRoundBox(x - s * 0.5f, y - s * 0.5f, s * 0.5f - 4f, s * 0.5f - 4f, 52f), Hex("5B3A22"));
            c.Fill((x, y) => SdRoundBox(x - s * 0.5f, y - s * 0.5f, s * 0.5f - 9f, s * 0.5f - 9f, 47f), Hex("FFF6E2"));
            return c.ToTexture();
        }

        private static Texture2D BubbleTail(int s)
        {
            // Sol alta bakan kuyruk; üst kenarı balonun içine girer (balonun arkasında durur).
            var c = new Px(s, s);
            Vector2 a = new Vector2(s * 0.30f, s * 0.98f), b = new Vector2(s * 0.85f, s * 0.98f), tip = new Vector2(s * 0.06f, s * 0.08f);
            c.Fill((x, y) => SdTriangle(new Vector2(x, y), a, b, tip) - 4.5f, Hex("5B3A22"));
            c.Fill((x, y) => SdTriangle(new Vector2(x, y), a + new Vector2(2f, 8f), b + new Vector2(-6f, 8f), tip + new Vector2(5f, 9f)), Hex("FFF6E2"));
            return c.ToTexture();
        }

        private static Texture2D CardFrame(int s)
        {
            var c = new Px(s, s);
            var wood = WoodPlank(s, s, Hex("5A3620"), Hex("7E4F2C"), Hex("2A170C"), 22, false);
            c.Blit(wood);
            c.Fill((x, y) => SdRoundBox(x - s * 0.5f, y - s * 0.5f, s * 0.5f - 22f, s * 0.5f - 22f, 10f), new Color(0.07f, 0.07f, 0.1f, 1f));
            return c.ToTexture();
        }

        private static Texture2D Glow(int s)
        {
            var c = new Px(s, s);
            for (var y = 0; y < s; y++)
            for (var x = 0; x < s; x++)
            {
                var d = SdRoundBox(x + 0.5f - s * 0.5f, y + 0.5f - s * 0.5f, s * 0.5f - 64f, s * 0.5f - 64f, 26f);
                float a;
                if (d < -10f) a = 0f;
                else if (d < 0f) a = Mathf.InverseLerp(-10f, 0f, d);
                else a = Mathf.Exp(-d / 16f);
                c.Set(x, y, new Color(1f, 0.86f, 0.4f, a));
            }
            return c.ToTexture();
        }

        private static Texture2D Vignette(int s)
        {
            var c = new Px(s, s);
            for (var y = 0; y < s; y++)
            for (var x = 0; x < s; x++)
            {
                var dx = (x + 0.5f) / s - 0.5f;
                var dy = (y + 0.5f) / s - 0.5f;
                var r = Mathf.Sqrt(dx * dx + dy * dy) * 1.414f;
                c.Set(x, y, new Color(0.08f, 0.04f, 0.02f, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.45f, 1.05f, r)) * 0.85f));
            }
            return c.ToTexture();
        }

        private static Texture2D BarTrough(int w, int h)
        {
            var c = new Px(w, h);
            c.Fill((x, y) => SdRoundBox(x - w * 0.5f, y - h * 0.5f, w * 0.5f - 3f, h * 0.5f - 3f, h * 0.5f - 3f), Hex("8A6A48"));
            c.Fill((x, y) => SdRoundBox(x - w * 0.5f, y - h * 0.5f, w * 0.5f - 7f, h * 0.5f - 7f, h * 0.5f - 7f), Hex("2B1B11"));
            c.Fill((x, y) => SdRoundBox(x - w * 0.5f, y - h * 0.5f + 4f, w * 0.5f - 9f, h * 0.5f - 11f, h * 0.5f - 11f), Hex("3A2618"));
            return c.ToTexture();
        }

        private static Texture2D BarFill(int w, int h)
        {
            var c = new Px(w, h);
            for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
            {
                var d = SdRoundBox(x + 0.5f - w * 0.5f, y + 0.5f - h * 0.5f, w * 0.5f - 8f, h * 0.5f - 8f, h * 0.5f - 8f);
                var cov = Mathf.Clamp01(0.5f - d);
                if (cov <= 0f) continue;
                var vy = (y + 0.5f) / h;
                var col = Color.Lerp(Hex("E59A1C"), Hex("FFE16B"), Mathf.SmoothStep(0.2f, 0.75f, vy));
                if (vy > 0.62f && vy < 0.74f) col = Color.Lerp(col, Color.white, 0.45f);
                col.a = cov;
                c.Set(x, y, col);
            }
            return c.ToTexture();
        }

        private static Texture2D RoundRectTex(int w, int h, float r, Color col, float pad)
        {
            var c = new Px(w, h);
            c.Fill((x, y) => SdRoundBox(x - w * 0.5f, y - h * 0.5f, w * 0.5f - 1f - pad, h * 0.5f - 1f - pad, r), col);
            return c.ToTexture();
        }

        private static Texture2D Knob(int s)
        {
            var c = new Px(s, s);
            c.Fill((x, y) => SdCircle(x, y, s * 0.5f, s * 0.5f - 2.5f, s * 0.5f - 4f), new Color(0f, 0f, 0f, 0.3f));
            c.Fill((x, y) => SdCircle(x, y, s * 0.5f, s * 0.5f, s * 0.5f - 5f), Color.white);
            c.Fill((x, y) => SdCircle(x, y, s * 0.5f, s * 0.5f - 1.5f, s * 0.5f - 9f), Hex("F3EFE8"));
            return c.ToTexture();
        }

        private static Texture2D Keycap(int s)
        {
            var c = new Px(s, s);
            c.Fill((x, y) => SdRoundBox(x - s * 0.5f, y - s * 0.5f + -4f, s * 0.5f - 6f, s * 0.5f - 8f, 18f), Hex("3C3C40"));
            c.Fill((x, y) => SdRoundBox(x - s * 0.5f, y - s * 0.5f - 3f, s * 0.5f - 9f, s * 0.5f - 13f, 15f), Hex("B8B8BD"));
            c.Fill((x, y) => SdRoundBox(x - s * 0.5f, y - s * 0.5f + 6f, s * 0.5f - 15f, s * 0.5f - 20f, 12f), Hex("F4F4F6"));
            return c.ToTexture();
        }

        private static Texture2D SoftShadow(int s)
        {
            var c = new Px(s, s);
            for (var y = 0; y < s; y++)
            for (var x = 0; x < s; x++)
            {
                var d = SdRoundBox(x + 0.5f - s * 0.5f, y + 0.5f - s * 0.5f, s * 0.5f - 40f, s * 0.5f - 40f, 12f);
                c.Set(x, y, new Color(0f, 0f, 0f, d <= 0f ? 1f : Mathf.Exp(-d * d / 300f)));
            }
            return c.ToTexture();
        }

        private static Texture2D Divider(int w, int h)
        {
            var c = new Px(w, h);
            var col = Hex("B98A55");
            c.Fill((x, y) => SdSegment(x, y, 8f, h * 0.5f, w * 0.5f - 40f, h * 0.5f) - 1.4f, col);
            c.Fill((x, y) => SdSegment(x, y, w * 0.5f + 40f, h * 0.5f, w - 8f, h * 0.5f) - 1.4f, col);
            var m = w * 0.5f;
            c.Fill((x, y) => SdTriangle(new Vector2(x, y), new Vector2(m - 30f, 6f), new Vector2(m + 4f, 6f), new Vector2(m - 12f, h - 6f)), col);
            c.Fill((x, y) => SdTriangle(new Vector2(x, y), new Vector2(m - 8f, 6f), new Vector2(m + 30f, 6f), new Vector2(m + 12f, h - 10f)), col * 0.85f);
            return c.ToTexture();
        }

        private static Texture2D Leaves(int s)
        {
            var c = new Px(s, s);
            Color g1 = Hex("3F7A2C"), g2 = Hex("5FA03A"), vein = Hex("2C5A1E");
            void Leaf(Vector2 a, Vector2 b, float wdt, Color col)
            {
                c.Fill((x, y) => SdVesica(new Vector2(x, y), a, b, wdt), col);
                c.Fill((x, y) => SdSegment(x, y, a.x, a.y, b.x, b.y) - 0.8f, vein);
            }
            Leaf(new Vector2(s * 0.5f, s * 0.15f), new Vector2(s * 0.12f, s * 0.7f), 15f, g1);
            Leaf(new Vector2(s * 0.5f, s * 0.15f), new Vector2(s * 0.85f, s * 0.62f), 15f, g2);
            Leaf(new Vector2(s * 0.5f, s * 0.15f), new Vector2(s * 0.5f, s * 0.92f), 14f, g2 * 0.92f);
            return c.ToTexture();
        }

        private static Texture2D Nail(int s)
        {
            var c = new Px(s, s);
            DrawNail(c, s * 0.5f, s * 0.5f, s * 0.36f);
            return c.ToTexture();
        }

        // ------------------------------------------------------------------ ikonlar (beyaz, Image.color ile boyanır)
        private static void GenerateIcons()
        {
            foreach (var pair in IconRecipes())
            {
                var c = new Px(128, 128);
                pair.Value(c);
                Save(IconDir, "icon_" + pair.Key, c.ToTexture(), 0);
            }
        }

        private static Dictionary<string, Action<Px>> IconRecipes()
        {
            var W = Color.white;
            var E = new Color(0, 0, 0, 0); // silgi (Px.Fill negatif modda)
            var r = new Dictionary<string, Action<Px>>();
            r["play"] = c => c.Fill((x, y) => SdTriangle(new Vector2(x, y), new Vector2(34, 18), new Vector2(34, 110), new Vector2(110, 64)) - 6f, W);
            r["arrow_right"] = c =>
            {
                c.Fill((x, y) => SdSegment(x, y, 44, 24, 84, 64) - 11f, W);
                c.Fill((x, y) => SdSegment(x, y, 44, 104, 84, 64) - 11f, W);
            };
            r["arrow_left"] = c =>
            {
                c.Fill((x, y) => SdSegment(x, y, 84, 24, 44, 64) - 11f, W);
                c.Fill((x, y) => SdSegment(x, y, 84, 104, 44, 64) - 11f, W);
            };
            r["back_triangle"] = c => c.Fill((x, y) => SdTriangle(new Vector2(x, y), new Vector2(98, 18), new Vector2(98, 110), new Vector2(22, 64)) - 6f, W);
            r["book"] = c =>
            {
                c.Fill((x, y) => SdRoundBox(x - 36, y - 64, 30, 40, 6) , W);
                c.Fill((x, y) => SdRoundBox(x - 92, y - 64, 30, 40, 6), W);
                c.Erase((x, y) => SdRoundBox(x - 38, y - 66, 22, 32, 3));
                c.Erase((x, y) => SdRoundBox(x - 90, y - 66, 22, 32, 3));
                for (var i = 0; i < 3; i++)
                {
                    var yy = 78 - i * 14;
                    c.Fill((x, y) => SdSegment(x, y, 24, yy, 50, yy) - 2.5f, W);
                    c.Fill((x, y) => SdSegment(x, y, 78, yy, 104, yy) - 2.5f, W);
                }
            };
            r["gear"] = c =>
            {
                for (var i = 0; i < 8; i++)
                {
                    var a = i * Mathf.PI / 4f;
                    var cx = 64 + Mathf.Cos(a) * 42; var cy = 64 + Mathf.Sin(a) * 42;
                    c.Fill((x, y) => SdCircle(x, y, (float)cx, (float)cy, 12), W);
                }
                c.Fill((x, y) => SdCircle(x, y, 64, 64, 42), W);
                c.Erase((x, y) => SdCircle(x, y, 64, 64, 17));
            };
            r["exit"] = c =>
            {
                c.Fill((x, y) => SdRoundBox(x - 46, y - 64, 30, 44, 6), W);
                c.Erase((x, y) => SdRoundBox(x - 46, y - 64, 20, 34, 2));
                c.Erase((x, y) => SdRoundBox(x - 74, y - 64, 10, 22, 0));
                c.Fill((x, y) => SdSegment(x, y, 50, 64, 100, 64) - 7f, W);
                c.Fill((x, y) => SdSegment(x, y, 84, 82, 106, 64) - 7f, W);
                c.Fill((x, y) => SdSegment(x, y, 84, 46, 106, 64) - 7f, W);
            };
            r["home"] = c =>
            {
                c.Fill((x, y) => SdTriangle(new Vector2(x, y), new Vector2(12, 66), new Vector2(116, 66), new Vector2(64, 112)) - 3f, W);
                c.Fill((x, y) => SdRoundBox(x - 64, y - 42, 36, 30, 3), W);
                c.Erase((x, y) => SdRoundBox(x - 64, y - 30, 11, 18, 2));
            };
            r["clock"] = c =>
            {
                c.Fill((x, y) => Mathf.Abs(SdCircle(x, y, 64, 64, 46)) - 7f, W);
                c.Fill((x, y) => SdSegment(x, y, 64, 64, 64, 96) - 6f, W);
                c.Fill((x, y) => SdSegment(x, y, 64, 64, 86, 52) - 6f, W);
            };
            r["lock"] = c =>
            {
                c.Fill((x, y) => SdRoundBox(x - 64, y - 46, 38, 32, 8), W);
                c.Fill((x, y) => Mathf.Max(Mathf.Abs(SdCircle(x, y, 64, 82, 25)) - 7f, 70 - y), W);
                c.Fill((x, y) => SdSegment(x, y, 39, 70, 39, 82) - 7f, W);
                c.Fill((x, y) => SdSegment(x, y, 89, 70, 89, 82) - 7f, W);
                c.Erase((x, y) => SdCircle(x, y, 64, 50, 8));
                c.Erase((x, y) => SdSegment(x, y, 64, 50, 64, 32) - 4f);
            };
            r["check"] = c =>
            {
                c.Fill((x, y) => SdSegment(x, y, 22, 64, 52, 34) - 10f, W);
                c.Fill((x, y) => SdSegment(x, y, 52, 34, 108, 96) - 10f, W);
            };
            r["gamepad"] = c =>
            {
                c.Fill((x, y) => SdRoundBox(x - 64, y - 66, 52, 26, 24), W);
                c.Fill((x, y) => SdCircle(x, y, 28, 46, 18), W);
                c.Fill((x, y) => SdCircle(x, y, 100, 46, 18), W);
                c.Erase((x, y) => SdRoundBox(x - 36, y - 66, 13, 4, 1));
                c.Erase((x, y) => SdRoundBox(x - 36, y - 66, 4, 13, 1));
                c.Erase((x, y) => SdCircle(x, y, 88, 72, 6));
                c.Erase((x, y) => SdCircle(x, y, 100, 60, 6));
            };
            r["clipboard"] = c =>
            {
                c.Fill((x, y) => SdRoundBox(x - 64, y - 60, 38, 50, 8), W);
                c.Erase((x, y) => SdRoundBox(x - 64, y - 56, 28, 38, 3));
                c.Fill((x, y) => SdRoundBox(x - 64, y - 110, 18, 9, 4), W);
                for (var i = 0; i < 3; i++)
                {
                    var yy = 74 - i * 18;
                    c.Fill((x, y) => SdSegment(x, y, 46, yy, 82, yy) - 4f, W);
                }
            };
            r["scale"] = c =>
            {
                c.Fill((x, y) => SdSegment(x, y, 64, 20, 64, 104) - 5f, W);
                c.Fill((x, y) => SdSegment(x, y, 20, 96, 108, 96) - 5f, W);
                c.Fill((x, y) => SdRoundBox(x - 64, y - 18, 26, 6, 3), W);
                c.Fill((x, y) => Mathf.Max(SdCircle(x, y, 30, 62, 20), y - 62), W);
                c.Fill((x, y) => Mathf.Max(SdCircle(x, y, 98, 62, 20), y - 62), W);
                c.Fill((x, y) => SdSegment(x, y, 30, 96, 12, 62) - 2.5f, W);
                c.Fill((x, y) => SdSegment(x, y, 30, 96, 48, 62) - 2.5f, W);
                c.Fill((x, y) => SdSegment(x, y, 98, 96, 80, 62) - 2.5f, W);
                c.Fill((x, y) => SdSegment(x, y, 98, 96, 116, 62) - 2.5f, W);
            };
            r["mountain"] = c =>
            {
                c.Fill((x, y) => SdTriangle(new Vector2(x, y), new Vector2(8, 22), new Vector2(84, 22), new Vector2(46, 100)), W);
                c.Fill((x, y) => SdTriangle(new Vector2(x, y), new Vector2(50, 22), new Vector2(122, 22), new Vector2(88, 78)), W);
            };
            r["trophy"] = c =>
            {
                c.Fill((x, y) => Mathf.Max(SdCircle(x, y, 64, 92, 34), y - 104), W);
                c.Fill((x, y) => SdRoundBox(x - 64, y - 100, 34, 6, 2), W);
                c.Fill((x, y) => Mathf.Abs(SdCircle(x, y, 28, 86, 14)) - 5f, W);
                c.Fill((x, y) => Mathf.Abs(SdCircle(x, y, 100, 86, 14)) - 5f, W);
                c.Fill((x, y) => SdRoundBox(x - 64, y - 46, 8, 14, 2), W);
                c.Fill((x, y) => SdRoundBox(x - 64, y - 24, 28, 8, 3), W);
            };
            r["bulb"] = c =>
            {
                c.Fill((x, y) => SdCircle(x, y, 64, 76, 32), W);
                c.Fill((x, y) => SdRoundBox(x - 64, y - 42, 18, 12, 4), W);
                c.Fill((x, y) => SdRoundBox(x - 64, y - 22, 14, 6, 3), W);
                c.Erase((x, y) => SdSegment(x, y, 52, 46, 76, 46) - 2f);
                for (var i = 0; i < 5; i++)
                {
                    var a = Mathf.PI * (0.15f + i * 0.175f);
                    float ax = 64 + Mathf.Cos(a) * 44, ay = 76 + Mathf.Sin(a) * 44, bx = 64 + Mathf.Cos(a) * 58, by = 76 + Mathf.Sin(a) * 58;
                    if (i == 2) continue;
                    c.Fill((x, y) => SdSegment(x, y, ax, ay, bx, by) - 4f, W);
                }
            };
            r["speaker"] = c =>
            {
                c.Fill((x, y) => SdRoundBox(x - 30, y - 64, 14, 18, 3), W);
                c.Fill((x, y) => SdTriangle(new Vector2(x, y), new Vector2(30, 64), new Vector2(66, 104), new Vector2(66, 24)), W);
                c.Fill((x, y) => Mathf.Max(Mathf.Abs(SdCircle(x, y, 70, 64, 24)) - 5f, 74 - x), W);
                c.Fill((x, y) => Mathf.Max(Mathf.Abs(SdCircle(x, y, 70, 64, 44)) - 5f, 80 - x), W);
            };
            r["monitor"] = c =>
            {
                c.Fill((x, y) => SdRoundBox(x - 64, y - 74, 52, 36, 6), W);
                c.Erase((x, y) => SdRoundBox(x - 64, y - 76, 42, 26, 2));
                c.Fill((x, y) => SdRoundBox(x - 64, y - 30, 7, 10, 1), W);
                c.Fill((x, y) => SdRoundBox(x - 64, y - 20, 28, 5, 2), W);
            };
            r["globe"] = c =>
            {
                c.Fill((x, y) => Mathf.Abs(SdCircle(x, y, 64, 64, 46)) - 6f, W);
                c.Fill((x, y) => Mathf.Abs(SdEllipse(x - 64, y - 64, 20, 46)) - 5f, W);
                c.Fill((x, y) => SdSegment(x, y, 18, 64, 110, 64) - 4.5f, W);
                c.Fill((x, y) => SdSegment(x, y, 26, 88, 102, 88) - 4f, W);
                c.Fill((x, y) => SdSegment(x, y, 26, 40, 102, 40) - 4f, W);
            };
            r["person"] = c =>
            {
                c.Fill((x, y) => SdCircle(x, y, 64, 102, 14), W);
                c.Fill((x, y) => SdSegment(x, y, 22, 80, 106, 80) - 8f, W);
                c.Fill((x, y) => SdSegment(x, y, 64, 80, 64, 50) - 10f, W);
                c.Fill((x, y) => SdSegment(x, y, 60, 52, 42, 14) - 8f, W);
                c.Fill((x, y) => SdSegment(x, y, 68, 52, 86, 14) - 8f, W);
            };
            r["palette"] = c =>
            {
                c.Fill((x, y) => SdEllipse(x - 64, y - 64, 52, 44), W);
                c.Erase((x, y) => SdCircle(x, y, 88, 44, 12));
                c.Erase((x, y) => SdCircle(x, y, 42, 84, 8));
                c.Erase((x, y) => SdCircle(x, y, 66, 92, 8));
                c.Erase((x, y) => SdCircle(x, y, 88, 80, 8));
                c.Erase((x, y) => SdCircle(x, y, 36, 58, 8));
            };
            r["code"] = c =>
            {
                c.Fill((x, y) => SdSegment(x, y, 40, 96, 12, 64) - 7f, W);
                c.Fill((x, y) => SdSegment(x, y, 12, 64, 40, 32) - 7f, W);
                c.Fill((x, y) => SdSegment(x, y, 88, 96, 116, 64) - 7f, W);
                c.Fill((x, y) => SdSegment(x, y, 116, 64, 88, 32) - 7f, W);
                c.Fill((x, y) => SdSegment(x, y, 74, 104, 54, 24) - 6f, W);
            };
            r["cube"] = c =>
            {
                Vector2 top = new Vector2(64, 112), l = new Vector2(20, 88), m = new Vector2(64, 64), rr = new Vector2(108, 88), bl = new Vector2(20, 38), bm = new Vector2(64, 14), br = new Vector2(108, 38);
                c.Fill((x, y) => SdQuad(new Vector2(x, y), top, rr, m, l), W);
                c.Fill((x, y) => SdQuad(new Vector2(x, y), l, m, bm, bl), W * 0.8f + new Color(0, 0, 0, 0.2f));
                c.Fill((x, y) => SdQuad(new Vector2(x, y), m, rr, br, bm), W * 0.6f + new Color(0, 0, 0, 0.4f));
            };
            r["image"] = c =>
            {
                c.Fill((x, y) => SdRoundBox(x - 64, y - 64, 50, 42, 8), W);
                c.Erase((x, y) => SdRoundBox(x - 64, y - 64, 40, 32, 2));
                c.Fill((x, y) => SdTriangle(new Vector2(x, y), new Vector2(26, 34), new Vector2(76, 34), new Vector2(50, 70)), W);
                c.Fill((x, y) => SdTriangle(new Vector2(x, y), new Vector2(60, 34), new Vector2(102, 34), new Vector2(84, 58)), W);
                c.Fill((x, y) => SdCircle(x, y, 86, 80, 8), W);
            };
            r["mouse"] = c =>
            {
                c.Fill((x, y) => Mathf.Abs(SdRoundBox(x - 64, y - 64, 30, 46, 30)) - 6f, W);
                c.Fill((x, y) => SdSegment(x, y, 36, 70, 92, 70) - 4f, W);
                c.Fill((x, y) => SdSegment(x, y, 64, 70, 64, 108) - 4f, W);
                c.Fill((x, y) => SdRoundBox(x - 50, y - 88, 12, 16, 8), W);
            };
            r["exclamation"] = c =>
            {
                c.Fill((x, y) => SdCircle(x, y, 64, 64, 56), W);
                c.Erase((x, y) => SdSegment(x, y, 64, 52, 64, 96) - 9f);
                c.Erase((x, y) => SdCircle(x, y, 64, 30, 9));
            };
            r["info"] = c =>
            {
                c.Fill((x, y) => SdCircle(x, y, 64, 64, 56), W);
                c.Erase((x, y) => SdSegment(x, y, 64, 30, 64, 70) - 9f);
                c.Erase((x, y) => SdCircle(x, y, 64, 92, 9));
            };
            r["heart"] = c =>
            {
                c.Fill((x, y) => SdCircle(x, y, 44, 78, 26), W);
                c.Fill((x, y) => SdCircle(x, y, 84, 78, 26), W);
                c.Fill((x, y) => SdTriangle(new Vector2(x, y), new Vector2(20, 70), new Vector2(108, 70), new Vector2(64, 18)), W);
            };
            r["star"] = c => c.Fill((x, y) => SdStar(x - 64, y - 60, 56, 24), W);
            r["leaf_small"] = c => c.Fill((x, y) => SdVesica(new Vector2(x, y), new Vector2(20, 20), new Vector2(108, 108), 28), W);
            // Görev ikonları
            r["bookshelf"] = c =>
            {
                c.Fill((x, y) => SdRoundBox(x - 64, y - 64, 40, 54, 4), W);
                for (var i = 0; i < 3; i++)
                {
                    var yy = 28 + i * 34;
                    c.Erase((x, y) => SdRoundBox(x - 64, y - (yy + 13), 32, 13, 1));
                    for (var b = 0; b < 4; b++)
                    {
                        var bx = 40 + b * 14 + (i % 2) * 4;
                        var hh = 9 + (b * 7 + i * 3) % 5;
                        c.Fill((x, y) => SdRoundBox(x - bx, y - (yy + hh), 5, hh, 1), W);
                    }
                }
            };
            r["fridge"] = c =>
            {
                c.Fill((x, y) => SdRoundBox(x - 64, y - 64, 34, 54, 8), W);
                c.Erase((x, y) => SdSegment(x, y, 32, 80, 96, 80) - 3f);
                c.Erase((x, y) => SdSegment(x, y, 44, 92, 44, 106) - 3.5f);
                c.Erase((x, y) => SdSegment(x, y, 44, 50, 44, 70) - 3.5f);
            };
            r["medkit"] = c =>
            {
                c.Fill((x, y) => SdRoundBox(x - 64, y - 56, 50, 38, 8), W);
                c.Fill((x, y) => Mathf.Abs(SdRoundBox(x - 64, y - 98, 18, 10, 5)) - 5f, W);
                c.Erase((x, y) => SdRoundBox(x - 64, y - 56, 7, 22, 1));
                c.Erase((x, y) => SdRoundBox(x - 64, y - 56, 22, 7, 1));
            };
            r["window"] = c =>
            {
                c.Fill((x, y) => Mathf.Abs(SdRoundBox(x - 64, y - 64, 46, 50, 4)) - 6f, W);
                c.Fill((x, y) => SdSegment(x, y, 64, 14, 64, 114) - 4f, W);
                c.Fill((x, y) => SdSegment(x, y, 18, 64, 110, 64) - 4f, W);
            };
            r["roof"] = c =>
            {
                c.Fill((x, y) => Mathf.Abs(SdTriangle(new Vector2(x, y), new Vector2(8, 60), new Vector2(120, 60), new Vector2(64, 110))) - 6f, W);
                c.Fill((x, y) => SdRoundBox(x - 64, y - 36, 38, 24, 2), W);
                c.Fill((x, y) => SdSegment(x, y, 40, 72, 88, 72) - 3f, W);
            };
            r["box"] = c =>
            {
                c.Fill((x, y) => SdRoundBox(x - 64, y - 54, 44, 36, 4), W);
                c.Fill((x, y) => SdRoundBox(x - 64, y - 96, 50, 8, 3), W);
                c.Erase((x, y) => SdRoundBox(x - 64, y - 70, 12, 6, 2));
            };
            r["flashlight"] = c =>
            {
                c.Fill((x, y) => SdRoundBox(x - 46, y - 64, 30, 13, 4), W);
                c.Fill((x, y) => SdQuad(new Vector2(x, y), new Vector2(74, 78), new Vector2(98, 92), new Vector2(98, 36), new Vector2(74, 50)), W);
                for (var i = -1; i <= 1; i++)
                {
                    var yy = 64 + i * 26;
                    c.Fill((x, y) => SdSegment(x, y, 106, 64 + i * 14, 122, yy) - 3.5f, W);
                }
            };
            r["smoke_alarm"] = c =>
            {
                c.Fill((x, y) => SdEllipse(x - 64, y - 84, 46, 18), W);
                c.Erase((x, y) => SdCircle(x, y, 64, 86, 7));
                for (var i = 0; i < 3; i++)
                {
                    var rr = 22 + i * 14;
                    c.Fill((x, y) => Mathf.Max(Mathf.Abs(SdCircle(x, y, 64, 64, rr)) - 3.5f, y - 50), W);
                }
            };
            r["stove"] = c =>
            {
                c.Fill((x, y) => SdRoundBox(x - 64, y - 52, 48, 34, 6), W);
                c.Erase((x, y) => SdCircle(x, y, 40, 70, 10));
                c.Erase((x, y) => SdCircle(x, y, 88, 70, 10));
                c.Erase((x, y) => SdRoundBox(x - 64, y - 38, 30, 9, 3));
                c.Fill((x, y) => SdCircle(x, y, 64, 102, 10), W);
                c.Erase((x, y) => SdSegment(x, y, 64, 102, 64, 110) - 2.5f);
            };
            r["extinguisher"] = c =>
            {
                c.Fill((x, y) => SdRoundBox(x - 64, y - 52, 22, 40, 14), W);
                c.Fill((x, y) => SdRoundBox(x - 64, y - 98, 10, 8, 2), W);
                c.Fill((x, y) => SdSegment(x, y, 64, 104, 96, 108) - 5f, W);
                c.Fill((x, y) => SdSegment(x, y, 74, 98, 98, 70) - 4f, W);
            };
            r["door"] = c =>
            {
                c.Fill((x, y) => Mathf.Abs(SdRoundBox(x - 56, y - 62, 32, 50, 4)) - 6f, W);
                c.Fill((x, y) => SdCircle(x, y, 76, 60, 6), W);
                c.Fill((x, y) => SdSegment(x, y, 92, 30, 118, 30) - 6f, W);
                c.Fill((x, y) => SdTriangle(new Vector2(x, y), new Vector2(110, 16), new Vector2(110, 44), new Vector2(124, 30)), W);
            };
            r["bolt"] = c => c.Fill((x, y) => SdPoly(new Vector2(x, y), new[] { new Vector2(74, 120), new Vector2(30, 58), new Vector2(60, 58), new Vector2(48, 8), new Vector2(98, 74), new Vector2(68, 74) }), W);
            r["plug"] = c =>
            {
                c.Fill((x, y) => SdRoundBox(x - 64, y - 66, 30, 22, 10), W);
                c.Fill((x, y) => SdRoundBox(x - 50, y - 104, 5, 16, 2), W);
                c.Fill((x, y) => SdRoundBox(x - 78, y - 104, 5, 16, 2), W);
                c.Fill((x, y) => SdSegment(x, y, 64, 44, 64, 14) - 6f, W);
            };
            r["shield"] = c =>
            {
                c.Fill((x, y) => SdPoly(new Vector2(x, y), new[] { new Vector2(64, 118), new Vector2(110, 100), new Vector2(104, 50), new Vector2(64, 10), new Vector2(24, 50), new Vector2(18, 100) }), W);
                c.Erase((x, y) => SdPoly(new Vector2(x, y), new[] { new Vector2(70, 98), new Vector2(50, 62), new Vector2(64, 62), new Vector2(56, 30), new Vector2(80, 70), new Vector2(66, 70) }));
            };
            r["sandbag"] = c =>
            {
                c.Fill((x, y) => SdEllipse(x - 40, y - 38, 30, 20), W);
                c.Fill((x, y) => SdEllipse(x - 90, y - 38, 30, 20), W);
                c.Fill((x, y) => SdEllipse(x - 64, y - 76, 32, 20), W);
                c.Erase((x, y) => SdSegment(x, y, 64, 18, 64, 56) - 2.5f);
                c.Erase((x, y) => SdSegment(x, y, 34, 76, 94, 76) - 2.5f);
            };
            r["drop"] = c =>
            {
                c.Fill((x, y) => SdCircle(x, y, 64, 52, 34), W);
                c.Fill((x, y) => SdTriangle(new Vector2(x, y), new Vector2(32, 60), new Vector2(96, 60), new Vector2(64, 118)), W);
            };
            r["arrow_up"] = c =>
            {
                c.Fill((x, y) => SdSegment(x, y, 64, 16, 64, 100) - 9f, W);
                c.Fill((x, y) => SdSegment(x, y, 30, 72, 64, 108) - 9f, W);
                c.Fill((x, y) => SdSegment(x, y, 98, 72, 64, 108) - 9f, W);
            };
            // Çök – Kapan – Tutun piktogramları (insan + masa)
            r["drop_cover_1"] = c =>
            {
                c.Fill((x, y) => SdCircle(x, y, 42, 66, 13), W);
                c.Fill((x, y) => SdEllipse(x - 70, y - 40, 32, 18), W);
                c.Fill((x, y) => SdSegment(x, y, 46, 26, 100, 26) - 7f, W);
                c.Fill((x, y) => SdSegment(x, y, 108, 112, 108, 74) - 6f, W);
                c.Fill((x, y) => SdTriangle(new Vector2(x, y), new Vector2(94, 78), new Vector2(122, 78), new Vector2(108, 56)), W);
            };
            r["drop_cover_2"] = c =>
            {
                c.Fill((x, y) => SdRoundBox(x - 64, y - 100, 56, 6, 2), W);
                c.Fill((x, y) => SdSegment(x, y, 16, 100, 16, 18) - 5f, W);
                c.Fill((x, y) => SdSegment(x, y, 112, 100, 112, 18) - 5f, W);
                c.Fill((x, y) => SdCircle(x, y, 42, 50, 12), W);
                c.Fill((x, y) => SdEllipse(x - 70, y - 34, 28, 16), W);
                c.Fill((x, y) => SdSegment(x, y, 44, 20, 96, 20) - 5f, W);
            };
            r["drop_cover_3"] = c =>
            {
                c.Fill((x, y) => SdRoundBox(x - 64, y - 100, 56, 6, 2), W);
                c.Fill((x, y) => SdSegment(x, y, 16, 100, 16, 18) - 5f, W);
                c.Fill((x, y) => SdSegment(x, y, 112, 100, 112, 18) - 5f, W);
                c.Fill((x, y) => SdCircle(x, y, 62, 52, 12), W);
                c.Fill((x, y) => SdEllipse(x - 82, y - 34, 22, 15), W);
                c.Fill((x, y) => SdSegment(x, y, 60, 40, 22, 60) - 5f, W);
                c.Fill((x, y) => SdSegment(x, y, 60, 20, 104, 20) - 5f, W);
            };
            return r;
        }

        // ------------------------------------------------------------------ ses
        private static void GenerateAudio()
        {
            WriteWav("ui_click", 0.09f, (t, i) => Knock(t, 520f, 38f) * 0.8f + Noise(i) * Mathf.Exp(-t * 90f) * 0.35f);
            WriteWav("ui_hover", 0.05f, (t, i) => Knock(t, 900f, 70f) * 0.25f);
            WriteWav("ui_open", 0.22f, (t, i) => Noise(i) * Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / 0.22f)) * 0.22f * LowPass(i) + Knock(t, 330f, 20f) * 0.3f);
            WriteWav("ui_confirm", 0.28f, (t, i) => (t < 0.11f ? Tone(t, 660f, 30f) : Tone(t - 0.11f, 990f, 18f)) * 0.45f);
            WriteWav("ui_error", 0.22f, (t, i) => Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 140f * t)) * Mathf.Exp(-t * 12f) * 0.18f);
        }

        private static float Knock(float t, float f, float decay) => Mathf.Sin(2f * Mathf.PI * f * t * (1f - t * 2f)) * Mathf.Exp(-t * decay);
        private static float Tone(float t, float f, float decay) => Mathf.Sin(2f * Mathf.PI * f * t) * Mathf.Exp(-t * decay) * Mathf.Clamp01(t * 400f);
        private static uint noiseState = 12345u;
        private static float lp;
        private static float Noise(int i) { noiseState ^= noiseState << 13; noiseState ^= noiseState >> 17; noiseState ^= noiseState << 5; return (noiseState / (float)uint.MaxValue) * 2f - 1f; }
        private static float LowPass(int i) { return 1f; }

        private static void WriteWav(string name, float seconds, Func<float, int, float> sample)
        {
            const int rate = 44100;
            var n = Mathf.CeilToInt(seconds * rate);
            noiseState = 12345u;
            lp = 0f;
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                w.Write(new[] { 'R', 'I', 'F', 'F' }); w.Write(36 + n * 2); w.Write(new[] { 'W', 'A', 'V', 'E' });
                w.Write(new[] { 'f', 'm', 't', ' ' }); w.Write(16); w.Write((short)1); w.Write((short)1); w.Write(rate); w.Write(rate * 2); w.Write((short)2); w.Write((short)16);
                w.Write(new[] { 'd', 'a', 't', 'a' }); w.Write(n * 2);
                for (var i = 0; i < n; i++)
                {
                    var t = i / (float)rate;
                    var v = sample(t, i);
                    lp += (v - lp) * 0.35f; // yumuşatma
                    var fade = Mathf.Clamp01((n - i) / 200f);
                    w.Write((short)Mathf.Clamp(lp * fade * 32000f, -32000f, 32000f));
                }
                File.WriteAllBytes(AudioDir + "/" + name + ".wav", ms.ToArray());
            }
        }

        // ------------------------------------------------------------------ kayıt + import
        private static void Save(string dir, string name, Texture2D tex, int border)
        {
            var path = dir + "/" + name + ".png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.spritePixelsPerUnit = 100f;
            importer.spriteBorder = new Vector4(border, border, border, border);
            importer.SaveAndReimport();
        }

        // ------------------------------------------------------------------ SDF yardımcıları
        private static Color Hex(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var c); return c; }

        private static float SdCircle(float x, float y, float cx, float cy, float r) => Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) - r;

        private static float SdRoundBox(float px, float py, float hx, float hy, float r)
        {
            r = Mathf.Min(r, Mathf.Min(hx, hy));
            var qx = Mathf.Abs(px) - hx + r;
            var qy = Mathf.Abs(py) - hy + r;
            return Mathf.Min(Mathf.Max(qx, qy), 0f) + new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude - r;
        }

        private static float SdSegment(float x, float y, float ax, float ay, float bx, float by)
        {
            var pa = new Vector2(x - ax, y - ay);
            var ba = new Vector2(bx - ax, by - ay);
            var h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Mathf.Max(0.0001f, Vector2.Dot(ba, ba)));
            return (pa - ba * h).magnitude;
        }

        private static float SdEllipse(float px, float py, float a, float b)
        {
            // Yaklaşık elips SDF (UI ikonları için yeterli).
            var k = new Vector2(px / a, py / b).magnitude;
            return (k - 1f) * Mathf.Min(a, b);
        }

        private static float SdTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c) => SdPoly(p, new[] { a, b, c });
        private static float SdQuad(Vector2 p, Vector2 a, Vector2 b, Vector2 c, Vector2 d) => SdPoly(p, new[] { a, b, c, d });

        private static float SdPoly(Vector2 p, Vector2[] v)
        {
            var d = Vector2.Dot(p - v[0], p - v[0]);
            var s = 1f;
            for (int i = 0, j = v.Length - 1; i < v.Length; j = i, i++)
            {
                var e = v[j] - v[i];
                var w = p - v[i];
                var b = w - e * Mathf.Clamp01(Vector2.Dot(w, e) / Vector2.Dot(e, e));
                d = Mathf.Min(d, Vector2.Dot(b, b));
                var c1 = p.y >= v[i].y; var c2 = p.y < v[j].y; var c3 = e.x * w.y > e.y * w.x;
                if ((c1 && c2 && c3) || (!c1 && !c2 && !c3)) s *= -1f;
            }
            return s * Mathf.Sqrt(d);
        }

        private static float SdVesica(Vector2 p, Vector2 a, Vector2 b, float width)
        {
            var len = (b - a).magnitude;
            var dir = (b - a) / len;
            var local = p - a;
            var u = Vector2.Dot(local, dir);
            var v = Mathf.Abs(local.x * -dir.y + local.y * dir.x);
            var t = Mathf.Clamp01(u / len);
            var half = Mathf.Sin(t * Mathf.PI) * width;
            if (u < 0f || u > len) return Mathf.Min((p - a).magnitude, (p - b).magnitude);
            return v - half;
        }

        private static float SdStar(float x, float y, float r, float inner)
        {
            var pts = new Vector2[10];
            for (var i = 0; i < 10; i++)
            {
                var a = Mathf.PI / 2f + i * Mathf.PI / 5f;
                var rr = i % 2 == 0 ? r : inner;
                pts[i] = new Vector2(Mathf.Cos(a) * rr, Mathf.Sin(a) * rr);
            }
            return SdPoly(new Vector2(x, y), pts);
        }

        /// <summary>Basit RGBA tuval; SDF şekillerini kenar yumuşatmalı boyar.</summary>
        private sealed class Px
        {
            private readonly int w, h;
            private readonly Color[] px;
            public Px(int width, int height) { w = width; h = height; px = new Color[w * h]; }

            public void Set(int x, int y, Color c) => px[y * w + x] = c;

            public void Fill(Func<float, float, float> sdf, Color col)
            {
                for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++)
                {
                    var cov = Mathf.Clamp01(0.5f - sdf(x + 0.5f, y + 0.5f)) * col.a;
                    if (cov <= 0f) continue;
                    var dst = px[y * w + x];
                    var a = cov + dst.a * (1f - cov);
                    var rgb = a > 0f ? (new Color(col.r, col.g, col.b) * cov + new Color(dst.r, dst.g, dst.b) * dst.a * (1f - cov)) / a : Color.clear;
                    px[y * w + x] = new Color(rgb.r, rgb.g, rgb.b, a);
                }
            }

            public void Erase(Func<float, float, float> sdf)
            {
                for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++)
                {
                    var cov = Mathf.Clamp01(0.5f - sdf(x + 0.5f, y + 0.5f));
                    if (cov <= 0f) continue;
                    var c = px[y * w + x];
                    c.a *= 1f - cov;
                    px[y * w + x] = c;
                }
            }

            public void Blit(Texture2D t)
            {
                var src = t.GetPixels();
                Array.Copy(src, px, Mathf.Min(src.Length, px.Length));
                UnityEngine.Object.DestroyImmediate(t);
            }

            public Texture2D ToTexture()
            {
                var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
                t.SetPixels(px);
                t.Apply();
                return t;
            }
        }
    }
}
