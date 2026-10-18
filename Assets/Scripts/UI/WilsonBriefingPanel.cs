using System;
using System.Collections.Generic;
using Jam.Missions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Jam.UI
{
    /// <summary>
    /// Her bölüm başında Wilson'ın brifingi (şartname §3.3): açılış, neden önemli, görev kartları,
    /// kontroller, son sayfa. GERİ / İLERİ / ATLA / GÖREVLERE BAŞLA. Modal açıkken alttaki oyun
    /// input'u alamaz (tam ekran raycast engeli) ve oyun zamanı durur (kontrolcü timeScale=0 yapar).
    /// Daktilo efekti: tıklama/İleri önce metni tamamlar, ikinci basış sonraki sayfaya geçer.
    /// </summary>
    public sealed class WilsonBriefingPanel : MonoBehaviour
    {
        private sealed class Page
        {
            public string header, body;
            public WilsonMood mood;
            public TaskDefinition task;
            public int taskIndex, taskCount;
            public bool controls;
        }

        private readonly List<Page> pages = new List<Page>();
        private WilsonPortrait portrait;
        private CanvasGroup group;
        private Text headerText, levelText, bodyText, pageText, taskTitle, taskBody, taskWhy, taskNumber;
        private GameObject taskCard, controlsCard;
        private Button backButton, nextButton, skipButton, startButton;
        private RawImage portraitImage;
        private Action onStart;
        private int index;
        private float typeStart;
        private string fullBody = "";
        private const float CharsPerSecond = 55f;

        public bool IsOpen => group != null && group.alpha > 0.5f;

        public static WilsonBriefingPanel Create(Transform parent, WilsonPortrait portrait)
        {
            var canvas = UiKit.CreateCanvas(parent, "Wilson Brifing", 60);
            var panel = canvas.gameObject.AddComponent<WilsonBriefingPanel>();
            panel.portrait = portrait;
            panel.Build((RectTransform)canvas.transform);
            panel.SetVisible(false);
            return panel;
        }

        private void Build(RectTransform root)
        {
            group = root.gameObject.AddComponent<CanvasGroup>();
            // Evi biraz karartan, tıklamaları yutan arka plan.
            var shade = UiKit.Stretch(root, "Karartma");
            UiKit.Panel(shade, new Color(0.08f, 0.06f, 0.05f, 0.55f), true);

            // Sol 1/3: Wilson portresi.
            var portraitFrame = UiKit.Rect(root, "Wilson çerçevesi", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f), new Vector2(110f, 10f), new Vector2(520f, 560f));
            var frameImg = UiKit.Panel(portraitFrame, new Color32(245, 239, 226, 235));
            UiKit.Border(frameImg, UiKit.Wood, 5f);
            var portraitRt = UiKit.Rect(portraitFrame, "Portre", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                new Vector2(0f, 20f), new Vector2(-30f, -70f));
            portraitImage = portraitRt.gameObject.AddComponent<RawImage>();
            portraitImage.raycastTarget = false;
            if (portrait != null) portraitImage.texture = portrait.Texture;
            var nameTag = UiKit.Rect(portraitFrame, "İsim", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 26f), new Vector2(260f, 58f));
            UiKit.Panel(nameTag, UiKit.Wood);
            UiKit.Label(nameTag, "Yazı", "WILSON", 34, UiKit.Cream, TextAnchor.MiddleCenter, FontStyle.Bold);

            // Sağ 2/3: diyalog kartı.
            var card = UiKit.Rect(root, "Diyalog kartı", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f), new Vector2(-100f, 10f), new Vector2(1120f, 700f));
            var cardImg = UiKit.Panel(card, new Color32(245, 239, 226, 250));
            UiKit.Border(cardImg, UiKit.Wood, 5f);

            headerText = UiKit.Label(card, "Başlık", "WILSON'DAN MESAJ VAR!", 46, new Color32(40, 42, 44, 255),
                TextAnchor.UpperLeft, FontStyle.Bold);
            Inset(headerText.rectTransform, 50f, 34f, 50f, 0f, 70f);
            levelText = UiKit.Label(card, "Bölüm", "", 26, new Color32(156, 106, 67, 255), TextAnchor.UpperLeft, FontStyle.Bold);
            Inset(levelText.rectTransform, 52f, 96f, 50f, 0f, 40f);
            var rule = UiKit.Rect(card, "Çizgi", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -146f), new Vector2(-100f, 4f));
            UiKit.Panel(rule, new Color32(211, 162, 75, 255));

            bodyText = UiKit.Label(card, "Metin", "", 36, new Color32(40, 42, 44, 255), TextAnchor.UpperLeft);
            Inset(bodyText.rectTransform, 52f, 172f, 52f, 0f, 190f);
            bodyText.lineSpacing = 1.1f;

            // Kontroller sayfası: tuş görseli (Art/UI/Resources/ui_controls.png).
            var controls = UiKit.Rect(card, "Kontroller görseli", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero);
            controls.offsetMin = new Vector2(40f, 112f);
            controls.offsetMax = new Vector2(-40f, -160f);
            var controlsImage = controls.gameObject.AddComponent<Image>();
            controlsImage.sprite = Resources.Load<Sprite>("ui_controls");
            controlsImage.preserveAspect = true;
            controlsImage.raycastTarget = false;
            controlsCard = controls.gameObject;
            controlsCard.SetActive(false);

            // Görev kartı (görev sayfalarında görünür).
            var task = UiKit.Rect(card, "Görev kartı", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 124f), new Vector2(-100f, 200f));
            var taskImg = UiKit.Panel(task, new Color32(255, 250, 240, 255));
            UiKit.Border(taskImg, new Color32(78, 150, 125, 255), 3f);
            taskCard = task.gameObject;
            taskNumber = UiKit.Label(task, "No", "1/4", 26, new Color32(78, 150, 125, 255), TextAnchor.UpperRight, FontStyle.Bold);
            Inset(taskNumber.rectTransform, 24f, 16f, 24f, 0f, 36f);
            taskTitle = UiKit.Label(task, "Görev adı", "", 32, new Color32(40, 42, 44, 255), TextAnchor.UpperLeft, FontStyle.Bold);
            Inset(taskTitle.rectTransform, 24f, 16f, 120f, 0f, 44f);
            taskBody = UiKit.Label(task, "Eylem", "", 26, new Color32(40, 42, 44, 255), TextAnchor.UpperLeft);
            Inset(taskBody.rectTransform, 24f, 66f, 24f, 0f, 64f);
            taskWhy = UiKit.Label(task, "Neden", "", 23, new Color32(156, 106, 67, 255), TextAnchor.UpperLeft, FontStyle.Italic);
            Inset(taskWhy.rectTransform, 24f, 136f, 24f, 0f, 56f);

            pageText = UiKit.Label(card, "Sayfa", "", 24, new Color32(120, 110, 100, 255), TextAnchor.LowerLeft);
            Inset(pageText.rectTransform, 52f, 640f, 50f, 0f, 40f);

            var bar = UiKit.Rect(card, "Düğmeler", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 24f), new Vector2(-100f, 80f));
            skipButton = UiKit.Button(UiKit.Rect(bar, "ATLA", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(260f, 0f), new Vector2(170f, 70f)), "ATLA", 28, UiKit.Paper, UiKit.Ink, Skip);
            backButton = UiKit.Button(UiKit.Rect(bar, "GERİ", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(0f, 0f), new Vector2(220f, 70f)), "◄ GERİ", 28, UiKit.Paper, UiKit.Ink, Back);
            nextButton = UiKit.Button(UiKit.Rect(bar, "İLERİ", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                Vector2.zero, new Vector2(260f, 70f)), "İLERİ ►", 30, new Color32(211, 162, 75, 255), UiKit.Ink, Next);
            startButton = UiKit.Button(UiKit.Rect(bar, "BAŞLA", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                Vector2.zero, new Vector2(420f, 70f)), "GÖREVLERE BAŞLA", 30, new Color32(78, 150, 125, 255), Color.white, Begin);
        }

        private static void Inset(RectTransform rt, float left, float top, float right, float bottom, float height)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(left, -top - height);
            rt.offsetMax = new Vector2(-right, -top);
        }

        /// <summary>Brifingi açar. fullFlow=false ise yalnız görev kartları ve son sayfa (tekrar dinle/kısa yol).</summary>
        public void Open(LevelDefinition level, bool fullFlow, Action start)
        {
            onStart = start;
            BuildPages(level, fullFlow);
            if (levelText != null && level != null)
                levelText.text = "BÖLÜM " + level.levelNumber.ToString("00") + " · " + level.titleTr + " — " + level.disasterNameTr;
            index = 0;
            SetVisible(true);
            ShowPage();
        }

        private void BuildPages(LevelDefinition level, bool fullFlow)
        {
            pages.Clear();
            if (level == null) return;
            var d = level.dialogue;
            if (fullFlow && d != null)
                foreach (var line in d.For(WilsonTrigger.Intro))
                    pages.Add(new Page { header = "WILSON'DAN MESAJ VAR!", body = line.textTr, mood = line.mood });
            if (fullFlow && !string.IsNullOrEmpty(level.realLifeTipTr))
                pages.Add(new Page { header = "NEDEN ÖNEMLİ?", body = level.realLifeTipTr, mood = WilsonMood.Anlatici });
            var previews = d != null ? d.For(WilsonTrigger.TaskPreview) : new List<WilsonLine>();
            var count = level.tasks.Length;
            for (var i = 0; i < count; i++)
            {
                var task = level.tasks[i];
                if (task == null) continue;
                var line = previews.Count > 0 ? previews[Mathf.Min(i * previews.Count / Mathf.Max(1, count), previews.Count - 1)] : null;
                pages.Add(new Page
                {
                    header = "GÖREVLER", body = line != null ? line.textTr : "", mood = WilsonMood.Anlatici,
                    task = task, taskIndex = i, taskCount = count
                });
            }
            pages.Add(new Page
            {
                header = "KONTROLLER", mood = WilsonMood.Normal, controls = Resources.Load<Sprite>("ui_controls") != null,
                body = Resources.Load<Sprite>("ui_controls") != null ? "" : "A / D  yürü     ·     W / S  merdiven\nE  al / bırak / kullan     ·     F  seçili eşyayı sabitle\n" +
                       "Fare  eşya seç ve sürükle     ·     Tekerlek  yakınlaş\nBOŞLUK  afeti başlat · depremde Çök–Kapan–Tutun     ·     ESC  duraklat"
            });
            var ready = d != null ? d.First(WilsonTrigger.PrepStart) : null;
            pages.Add(new Page
            {
                header = "HAZIR MISIN?", mood = ready != null ? ready.mood : WilsonMood.Heyecanli,
                body = (ready != null ? ready.textTr + "\n\n" : "") +
                       "Görevleri tamamlamak afeti hafifletir; fakat dengeyi unutma!\n" +
                       "Not: Evin ağırlıkla dengelenmesi bu oyunun hayal ürünü kuralıdır."
            });
        }

        private void ShowPage()
        {
            if (pages.Count == 0) { Begin(); return; }
            index = Mathf.Clamp(index, 0, pages.Count - 1);
            var page = pages[index];
            headerText.text = page.header;
            fullBody = page.body ?? "";
            typeStart = Time.unscaledTime;
            bodyText.text = "";
            taskCard.SetActive(page.task != null);
            controlsCard.SetActive(page.controls);
            if (page.task != null)
            {
                taskNumber.text = "GÖREV " + (page.taskIndex + 1) + " / " + page.taskCount;
                taskTitle.text = page.task.titleTr;
                taskBody.text = page.task.descriptionTr;
                taskWhy.text = page.task.whyItMattersTr;
            }
            pageText.text = (index + 1) + " / " + pages.Count;
            var last = index == pages.Count - 1;
            backButton.interactable = index > 0;
            nextButton.gameObject.SetActive(!last);
            startButton.gameObject.SetActive(last);
            skipButton.gameObject.SetActive(!last);
            if (portrait != null) { portrait.SetMood(page.mood); portrait.Talk(Mathf.Min(2.5f, fullBody.Length / CharsPerSecond)); }
        }

        private bool Typing => bodyText.text.Length < fullBody.Length;

        private void Next()
        {
            if (Typing) { bodyText.text = fullBody; return; }
            if (index < pages.Count - 1) { index++; ShowPage(); }
        }

        private void Back()
        {
            if (index > 0) { index--; ShowPage(); }
        }

        /// <summary>Atla: görev listesi HUD'da kalır, doğrudan son sayfaya gider.</summary>
        private void Skip()
        {
            index = pages.Count - 1;
            ShowPage();
            bodyText.text = fullBody;
        }

        private void Begin()
        {
            SetVisible(false);
            var callback = onStart;
            onStart = null;
            callback?.Invoke();
        }

        private void SetVisible(bool visible)
        {
            if (group == null) return;
            group.alpha = visible ? 1f : 0f;
            group.blocksRaycasts = visible;
            group.interactable = visible;
            if (portrait != null) portrait.SetActive(visible);
        }

        private void Update()
        {
            if (!IsOpen) return;
            if (Typing)
            {
                var n = Mathf.Clamp(Mathf.FloorToInt((Time.unscaledTime - typeStart) * CharsPerSecond), 0, fullBody.Length);
                bodyText.text = fullBody.Substring(0, n);
            }
            var k = Keyboard.current;
            if (k == null) return;
            if (k.enterKey.wasPressedThisFrame || k.rightArrowKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame)
            {
                if (index == pages.Count - 1 && !Typing) Begin(); else Next();
            }
            else if (k.leftArrowKey.wasPressedThisFrame) Back();
        }
    }
}
