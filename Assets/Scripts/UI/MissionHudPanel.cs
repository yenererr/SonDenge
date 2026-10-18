using System;
using System.Collections.Generic;
using Jam.Missions;
using UnityEngine;
using UnityEngine.UI;

namespace Jam.UI
{
    /// <summary>
    /// Hazırlıkta solda görev listesi (Boş / Tamam / Kaçırıldı), "WILSON'DAN İPUCU" ve
    /// "TEKRAR DİNLE" düğmeleri ve Wilson'ın kısa konuşma balonu. Durumu yalnız MissionManager'dan okur.
    /// </summary>
    public sealed class MissionHudPanel : MonoBehaviour
    {
        private sealed class Row
        {
            public TaskDefinition task;
            public Text mark, title;
            public Image background;
        }

        private readonly List<Row> rows = new List<Row>();
        private MissionManager missions;
        private CanvasGroup group, toastGroup;
        private Text header, toastText;
        private RectTransform list;
        private float toastUntil;
        private WilsonPortrait portrait;
        private Button hintButton, replayButton;

        public Action HintRequested;
        public Action ReplayRequested;
        public bool Visible { get; private set; } = true;

        public static MissionHudPanel Create(Transform parent, WilsonPortrait portrait)
        {
            var canvas = UiKit.CreateCanvas(parent, "Görev HUD", 20);
            var hud = canvas.gameObject.AddComponent<MissionHudPanel>();
            hud.portrait = portrait;
            hud.Build((RectTransform)canvas.transform);
            return hud;
        }

        private void Build(RectTransform root)
        {
            group = root.gameObject.AddComponent<CanvasGroup>();
            var card = UiKit.Rect(root, "Görev listesi", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -200f), new Vector2(470f, 330f));
            var img = UiKit.Panel(card, new Color32(245, 239, 226, 235));
            UiKit.Border(img, UiKit.Wood, 3f);
            header = UiKit.Label(card, "Başlık", "GÖREVLER 0/0", 28, UiKit.Ink, TextAnchor.UpperLeft, FontStyle.Bold);
            header.rectTransform.offsetMin = new Vector2(20f, 0f);
            header.rectTransform.offsetMax = new Vector2(-20f, -12f);
            list = UiKit.Rect(card, "Satırlar", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -56f), new Vector2(-28f, 260f));

            var buttons = UiKit.Rect(root, "Wilson düğmeleri", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(24f, 150f), new Vector2(470f, 60f));
            hintButton = UiKit.Button(UiKit.Rect(buttons, "İpucu", new Vector2(0f, 0f), new Vector2(0.56f, 1f), new Vector2(0f, 0.5f),
                Vector2.zero, Vector2.zero), "? WILSON'DAN İPUCU", 22, new Color32(211, 162, 75, 255), UiKit.Ink,
                () => HintRequested?.Invoke());
            replayButton = UiKit.Button(UiKit.Rect(buttons, "Tekrar", new Vector2(0.59f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f),
                Vector2.zero, Vector2.zero), "TEKRAR DİNLE", 22, UiKit.Paper, UiKit.Ink, () => ReplayRequested?.Invoke());

            // Wilson konuşma balonu (sol alt, küçük portre + metin).
            var toast = UiKit.Rect(root, "Wilson balonu", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 150f), new Vector2(900f, 120f));
            toastGroup = toast.gameObject.AddComponent<CanvasGroup>();
            toastGroup.blocksRaycasts = false;
            var toastImg = UiKit.Panel(toast, new Color32(255, 250, 240, 245));
            UiKit.Border(toastImg, new Color32(78, 150, 125, 255), 3f);
            var face = UiKit.Rect(toast, "Yüz", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(8f, 0f), new Vector2(110f, 110f));
            var raw = face.gameObject.AddComponent<RawImage>();
            raw.raycastTarget = false;
            if (portrait != null) raw.texture = portrait.Texture;
            toastText = UiKit.Label(toast, "Metin", "", 27, UiKit.Ink, TextAnchor.MiddleLeft);
            toastText.rectTransform.offsetMin = new Vector2(130f, 8f);
            toastText.rectTransform.offsetMax = new Vector2(-20f, -8f);
            toastGroup.alpha = 0f;
        }

        public void Bind(MissionManager manager)
        {
            missions = manager;
            foreach (Transform child in list) Destroy(child.gameObject);
            rows.Clear();
            if (missions == null || missions.Level == null) return;
            var y = 0f;
            foreach (var task in missions.Level.tasks)
            {
                if (task == null) continue;
                var rt = UiKit.Rect(list, task.id, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0f, -y), new Vector2(0f, 58f));
                var bg = UiKit.Panel(rt, new Color(1f, 1f, 1f, 0.35f));
                var mark = UiKit.Label(rt, "İşaret", "○", 30, UiKit.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
                mark.rectTransform.anchorMax = new Vector2(0f, 1f);
                mark.rectTransform.sizeDelta = new Vector2(44f, 0f);
                mark.rectTransform.anchoredPosition = new Vector2(22f, 0f);
                var title = UiKit.Label(rt, "Ad", task.titleTr, 21, UiKit.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
                title.rectTransform.offsetMin = new Vector2(50f, 0f);
                title.rectTransform.offsetMax = new Vector2(-8f, 0f);
                rows.Add(new Row { task = task, mark = mark, title = title, background = bg });
                y += 64f;
            }
            ((RectTransform)list.parent).sizeDelta = new Vector2(470f, 70f + y);
        }

        public void SetVisible(bool visible)
        {
            Visible = visible;
            if (group == null) return;
            group.alpha = visible ? 1f : 0f;
            group.blocksRaycasts = visible;
        }

        public void SetButtonsVisible(bool visible)
        {
            if (hintButton != null) hintButton.transform.parent.gameObject.SetActive(visible);
        }

        /// <summary>Wilson'ın kısa balonu (1.5–2.5 sn). Ses/hareket yalnız portre üzerinden.</summary>
        public void Say(string text, WilsonMood mood, float seconds = 2.5f)
        {
            if (string.IsNullOrEmpty(text)) return;
            toastText.text = text;
            toastUntil = Time.unscaledTime + seconds;
            if (portrait != null) { portrait.SetMood(mood); portrait.Talk(Mathf.Min(seconds, 2f)); portrait.SetActive(true); }
        }

        private void Update()
        {
            var showToast = Time.unscaledTime < toastUntil;
            toastGroup.alpha = Mathf.MoveTowards(toastGroup.alpha, showToast ? 1f : 0f, Time.unscaledDeltaTime * 5f);
            if (!showToast && toastGroup.alpha <= 0f && portrait != null && !briefingUsesPortrait) portrait.SetActive(false);
            if (missions == null) return;
            header.text = "GÖREVLER " + missions.CompletedCount + "/" + missions.TotalCount;
            foreach (var row in rows)
            {
                var state = missions.StateOf(row.task.id);
                row.mark.text = state == TaskState.Completed ? "●" : state == TaskState.Missed ? "×" : "○";
                row.mark.color = state == TaskState.Completed ? new Color32(78, 150, 125, 255) :
                    state == TaskState.Missed ? new Color32(197, 98, 86, 255) : (Color)UiKit.Ink;
                row.background.color = state == TaskState.Completed ? new Color(0.31f, 0.59f, 0.49f, 0.22f) :
                    state == TaskState.Missed ? new Color(0.77f, 0.38f, 0.34f, 0.18f) : new Color(1f, 1f, 1f, 0.35f);
            }
        }

        /// <summary>Brifing açıkken portre kamerası brifinge aittir; balon kapatmasın.</summary>
        [NonSerialized] public bool briefingUsesPortrait;
    }
}
