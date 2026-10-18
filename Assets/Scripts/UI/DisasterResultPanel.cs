using System;
using System.Collections.Generic;
using Jam.Missions;
using UnityEngine;
using UnityEngine.UI;

namespace Jam.UI
{
    /// <summary>
    /// AFET RAPORU: yıldızlar, 100 puanlık döküm, görev bazında gerçekleşen sonuçlar, Wilson'ın
    /// sonuca uyan cümlesi, gerçek hayat notu ve TEKRAR DENE / SONRAKİ BÖLÜM / ANA MENÜ.
    /// Son bölümde beş bölümün yıldız özeti ve "AFETE HAZIRLIK HAYAT KURTARIR".
    /// </summary>
    public sealed class DisasterResultPanel : MonoBehaviour
    {
        public struct Data
        {
            public string levelTitle;
            public bool survived;
            public string failReason;
            public int stars, score;
            public float preparation, behaviour, balance, protection;
            public List<string> lines;
            public string wilsonLine;
            public WilsonMood wilsonMood;
            public string realLifeTip;
            public bool hasNext;
            public bool isFinal;
            public string finalLine;
        }

        private CanvasGroup group;
        private Text title, subtitle, scoreText, linesText, wilsonText, tipText, summaryText;
        private Image[] starImages = new Image[3];
        private Button retryButton, nextButton, menuButton;
        private Text nextLabel;
        private RawImage portraitImage;
        private WilsonPortrait portrait;
        private Sprite starSprite;

        public Action RetryRequested, NextRequested, MenuRequested, ReplayAllRequested;
        private bool finalMode;

        public static DisasterResultPanel Create(Transform parent, WilsonPortrait portrait)
        {
            var canvas = UiKit.CreateCanvas(parent, "Afet Raporu", 70);
            var panel = canvas.gameObject.AddComponent<DisasterResultPanel>();
            panel.portrait = portrait;
            panel.Build((RectTransform)canvas.transform);
            panel.Hide();
            return panel;
        }

        private void Build(RectTransform root)
        {
            group = root.gameObject.AddComponent<CanvasGroup>();
            UiKit.Panel(UiKit.Stretch(root, "Karartma"), new Color(0.06f, 0.05f, 0.04f, 0.62f), true);
            var card = UiKit.Rect(root, "Rapor", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(1500f, 900f));
            UiKit.Border(UiKit.Panel(card, new Color32(245, 239, 226, 252)), UiKit.Wood, 5f);

            title = UiKit.Label(card, "Başlık", "AFET RAPORU", 54, new Color32(40, 42, 44, 255), TextAnchor.UpperCenter, FontStyle.Bold);
            Top(title.rectTransform, 26f, 70f);
            subtitle = UiKit.Label(card, "Alt başlık", "", 30, new Color32(156, 106, 67, 255), TextAnchor.UpperCenter, FontStyle.Bold);
            Top(subtitle.rectTransform, 92f, 44f);

            starSprite = MakeStarSprite();
            for (var i = 0; i < 3; i++)
            {
                var s = UiKit.Rect(card, "Yıldız " + (i + 1), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2((i - 1) * 130f, -142f - (i == 1 ? 0f : 18f)), new Vector2(i == 1 ? 120f : 100f, i == 1 ? 120f : 100f));
                starImages[i] = s.gameObject.AddComponent<Image>();
                starImages[i].sprite = starSprite;
                starImages[i].raycastTarget = false;
            }

            scoreText = UiKit.Label(card, "Puan", "", 26, UiKit.Ink, TextAnchor.UpperLeft);
            Box(scoreText.rectTransform, 60f, 290f, 520f, 190f);
            linesText = UiKit.Label(card, "Sonuçlar", "", 24, UiKit.Ink, TextAnchor.UpperLeft);
            Box(linesText.rectTransform, 600f, 290f, 840f, 330f);
            linesText.lineSpacing = 1.15f;

            var wilsonRow = UiKit.Rect(card, "Wilson", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 150f), new Vector2(-120f, 130f));
            UiKit.Panel(wilsonRow, new Color32(255, 250, 240, 255));
            var face = UiKit.Rect(wilsonRow, "Yüz", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(6f, 0f), new Vector2(124f, 124f));
            portraitImage = face.gameObject.AddComponent<RawImage>();
            portraitImage.raycastTarget = false;
            if (portrait != null) portraitImage.texture = portrait.Texture;
            wilsonText = UiKit.Label(wilsonRow, "Söz", "", 28, UiKit.Ink, TextAnchor.UpperLeft, FontStyle.Bold);
            wilsonText.rectTransform.offsetMin = new Vector2(150f, 50f);
            wilsonText.rectTransform.offsetMax = new Vector2(-20f, -10f);
            tipText = UiKit.Label(wilsonRow, "Gerçek hayat", "", 22, new Color32(78, 150, 125, 255), TextAnchor.LowerLeft, FontStyle.Italic);
            tipText.rectTransform.offsetMin = new Vector2(150f, 8f);
            tipText.rectTransform.offsetMax = new Vector2(-20f, -70f);

            summaryText = UiKit.Label(card, "Özet", "", 21, UiKit.Ink, TextAnchor.UpperLeft, FontStyle.Bold);
            Box(summaryText.rectTransform, 60f, 486f, 520f, 128f);

            var bar = UiKit.Rect(card, "Düğmeler", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 36f), new Vector2(1260f, 84f));
            retryButton = UiKit.Button(UiKit.Rect(bar, "Tekrar", new Vector2(0f, 0f), new Vector2(0.32f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero), "BU BÖLÜMÜ TEKRAR DENE", 26, UiKit.Paper, UiKit.Ink, () => RetryRequested?.Invoke());
            nextButton = UiKit.Button(UiKit.Rect(bar, "Sonraki", new Vector2(0.34f, 0f), new Vector2(0.66f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero), "SONRAKİ BÖLÜM", 28, new Color32(78, 150, 125, 255), Color.white, OnNext);
            nextLabel = nextButton.GetComponentInChildren<Text>();
            menuButton = UiKit.Button(UiKit.Rect(bar, "Menü", new Vector2(0.68f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero), "ANA MENÜ", 26, UiKit.Paper, UiKit.Ink, () => MenuRequested?.Invoke());
        }

        private void OnNext()
        {
            if (finalMode) ReplayAllRequested?.Invoke();
            else NextRequested?.Invoke();
        }

        private static void Top(RectTransform rt, float top, float height)
        {
            rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(1f, 1f); rt.pivot = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(40f, -top - height); rt.offsetMax = new Vector2(-40f, -top);
        }

        private static void Box(RectTransform rt, float left, float top, float width, float height)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f); rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(left, -top); rt.sizeDelta = new Vector2(width, height);
        }

        public void Show(Data d)
        {
            finalMode = d.isFinal && d.survived;
            title.text = d.survived ? "AFET RAPORU" : "DENGE BOZULDU";
            subtitle.text = d.levelTitle + (d.survived ? "" : " — " + d.failReason);
            for (var i = 0; i < 3; i++)
                starImages[i].color = i < d.stars ? new Color32(211, 162, 75, 255) : new Color32(200, 190, 175, 255);
            scoreText.text = "PUAN: " + d.score + " / 100\n\n" +
                "Hazırlık görevleri   " + Mathf.RoundToInt(d.preparation) + " / 35\n" +
                "Doğru afet davranışı   " + Mathf.RoundToInt(d.behaviour) + " / 35\n" +
                "Denge   " + Mathf.RoundToInt(d.balance) + " / 20\n" +
                "Eşya koruma   " + Mathf.RoundToInt(d.protection) + " / 10";
            linesText.text = d.lines != null ? string.Join("\n", d.lines) : "";
            wilsonText.text = "WILSON: " + d.wilsonLine;
            tipText.text = d.realLifeTip + "  ·  Evin ağırlıkla dengelenmesi oyunun hayal ürünü kuralıdır.";
            if (finalMode)
            {
                var summary = "BEŞ BÖLÜM ÖZETİ\n";
                for (var i = 1; i <= LevelProgressSave.LevelCount; i++)
                    summary += i + ": " + new string('●', LevelProgressSave.BestStars(i)) +
                               new string('○', 3 - LevelProgressSave.BestStars(i)) + (i == 3 ? "\n" : "    ");
                summaryText.text = summary + "\nAFETE HAZIRLIK HAYAT KURTARIR.";
                // Wilson'ın final cümlesi, sonuç cümlesinin ardından aynı satırda gösterilir.
                if (!string.IsNullOrEmpty(d.finalLine)) wilsonText.text += " " + d.finalLine;
            }
            else summaryText.text = "";
            nextButton.gameObject.SetActive(d.survived && (d.hasNext || finalMode));
            nextLabel.text = finalMode ? "YENİDEN OYNA" : "SONRAKİ BÖLÜM";
            if (portrait != null) { portrait.SetMood(d.wilsonMood); portrait.SetActive(true); portrait.Talk(2f); }
            group.alpha = 1f;
            group.blocksRaycasts = true;
            group.interactable = true;
        }

        public void Hide()
        {
            if (group == null) return;
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        public bool IsOpen => group != null && group.alpha > 0.5f;

        /// <summary>Beş köşeli yıldızı çalışma anında dokuya çizer (harici görsel gerekmez).</summary>
        private static Sprite MakeStarSprite()
        {
            const int n = 128;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { name = "Yıldız", wrapMode = TextureWrapMode.Clamp };
            var pts = new Vector2[10];
            for (var i = 0; i < 10; i++)
            {
                var a = Mathf.PI / 2f + i * Mathf.PI / 5f;
                var r = i % 2 == 0 ? 0.48f : 0.2f;
                pts[i] = new Vector2(0.5f + Mathf.Cos(a) * r, 0.5f + Mathf.Sin(a) * r) * n;
            }
            var pixels = new Color32[n * n];
            for (var y = 0; y < n; y++)
            for (var x = 0; x < n; x++)
            {
                var inside = false;
                var p = new Vector2(x + 0.5f, y + 0.5f);
                for (int i = 0, j = 9; i < 10; j = i++)
                    if ((pts[i].y > p.y) != (pts[j].y > p.y) &&
                        p.x < (pts[j].x - pts[i].x) * (p.y - pts[i].y) / (pts[j].y - pts[i].y) + pts[i].x) inside = !inside;
                pixels[y * n + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
        }

        private void OnDestroy()
        {
            if (starSprite != null) { Destroy(starSprite.texture); Destroy(starSprite); }
        }
    }
}
