using System;
using System.Collections.Generic;
using Jam.Missions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jam.UI
{
    /// <summary>
    /// Beş bölümün ortak AFET RAPORU ekranı. Arka plan, "Dağ Günbatımında Oyun Sonuç Paneli"
    /// şablon görselidir (yalnız çerçeve/kompozisyon); başlık, yıldızlar, puanlar, madde listesi,
    /// Wilson alanı ve düğmeler bu prefab içindeki gerçek UI nesneleridir ve Show() ile doldurulur.
    /// Prefab: Assets/Levels/Level1/Prefabs/UI_DisasterReportTemplate.prefab
    /// </summary>
    public sealed class DisasterReportPanel : MonoBehaviour
    {
        /// <summary>Rapor verisi. Tüm metinler çağıran taraftan gelir; panel içerik üretmez.</summary>
        public struct Data
        {
            public string chapterTitle;      // "BÖLÜM 01 · İLK SARSINTI"
            public bool survived;
            public string statusText;        // "EV AYAKTA KALDI" / "DENGE BOZULDU"
            public int totalScore;
            public int stars;                // 0..3
            public float preparation, behaviour, balance, protection;
            public float maxTiltDegrees;
            public float collapseAngle;
            public List<Line> lines;
            public string wilsonMessage;
            public WilsonMood wilsonMood;
            public string safetyNote;
            public string primaryLabel;      // orta (vurgulu) düğme
            public string leftLabel;
            public string rightLabel;
            public Action onPrimary, onLeft, onRight;
        }

        public enum LineKind { Success, Missing, Critical, Info }

        public struct Line
        {
            public LineKind kind;
            public string text;
            public Line(LineKind kind, string text) { this.kind = kind; this.text = text; }
        }

        [Header("Kök")]
        [SerializeField] private CanvasGroup group;
        [Header("Başlık")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text chapterText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text scoreText;
        [Header("Eğim göstergesi (üst orta çubuk)")]
        [SerializeField] private RectTransform tiltTrack;
        [SerializeField] private RectTransform tiltMarker;
        [SerializeField] private TMP_Text tiltText;
        [Header("Yıldızlar")]
        [SerializeField] private Image[] starImages = new Image[3];
        [SerializeField] private Sprite starActive;
        [SerializeField] private Sprite starInactive;
        [Tooltip("Sprite'ın kendi rengiyle çarpılır; sprite varsa beyaz bırakın.")]
        [SerializeField] private Color starActiveColor = Color.white;
        [SerializeField] private Color starInactiveColor = Color.white;
        [Header("Puan satırları (sırayla: hazırlık, davranış, denge, eşya)")]
        [SerializeField] private TMP_Text[] categoryValues = new TMP_Text[4];
        [Header("Madde listesi")]
        [SerializeField] private RectTransform lineContainer;
        [SerializeField] private TMP_Text linePrototype;
        [SerializeField] private Color successColor = new Color32(62, 120, 98, 255);
        [SerializeField] private Color missingColor = new Color32(176, 84, 72, 255);
        [SerializeField] private Color criticalColor = new Color32(176, 60, 48, 255);
        [SerializeField] private Color infoColor = new Color32(88, 70, 54, 255);
        [Header("Wilson")]
        [SerializeField] private RawImage wilsonPortrait;
        [SerializeField] private TMP_Text wilsonMessageText;
        [SerializeField] private TMP_Text safetyNoteText;
        [Header("Düğmeler (sol, orta-vurgulu, sağ)")]
        [SerializeField] private Button leftButton;
        [SerializeField] private Button primaryButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private TMP_Text leftLabel;
        [SerializeField] private TMP_Text primaryLabel;
        [SerializeField] private TMP_Text rightLabel;

        private readonly List<TMP_Text> lineInstances = new List<TMP_Text>();
        private Action onLeft, onPrimary, onRight;
        private WilsonPortrait portrait;

        public bool IsOpen => group != null && group.alpha > 0.5f;

        private void Awake()
        {
            if (leftButton != null) leftButton.onClick.AddListener(() => onLeft?.Invoke());
            if (primaryButton != null) primaryButton.onClick.AddListener(() => onPrimary?.Invoke());
            if (rightButton != null) rightButton.onClick.AddListener(() => onRight?.Invoke());
            if (linePrototype != null) linePrototype.gameObject.SetActive(false);
            Hide();
        }

        /// <summary>Canlı 3D Wilson portresini (RenderTexture) bağlar; yoksa şablondaki boş kutu kalır.</summary>
        public void BindPortrait(WilsonPortrait wilson)
        {
            portrait = wilson;
            if (wilsonPortrait != null && wilson != null) wilsonPortrait.texture = wilson.Texture;
            if (wilsonPortrait != null) wilsonPortrait.enabled = wilsonPortrait.texture != null;
        }

        public void Show(Data d)
        {
            chapterText.text = d.chapterTitle;
            statusText.text = d.statusText;
            statusText.color = d.survived ? successColor : criticalColor;
            scoreText.text = "PUAN  <b>" + d.totalScore + "</b> / 100";
            SetStars(d.stars);
            SetCategory(0, d.preparation, MissionScoring.PreparationMax);
            SetCategory(1, d.behaviour, MissionScoring.BehaviourMax);
            SetCategory(2, d.balance, MissionScoring.BalanceMax);
            SetCategory(3, d.protection, MissionScoring.ProtectionMax);
            SetTilt(d.maxTiltDegrees, d.collapseAngle <= 0f ? 26f : d.collapseAngle);
            SetLines(d.lines);
            wilsonMessageText.text = "<b>WILSON:</b> " + d.wilsonMessage;
            safetyNoteText.text = d.safetyNote;
            SetButton(leftButton, leftLabel, d.leftLabel, d.onLeft, out onLeft);
            SetButton(primaryButton, primaryLabel, d.primaryLabel, d.onPrimary, out onPrimary);
            SetButton(rightButton, rightLabel, d.rightLabel, d.onRight, out onRight);
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

        private void SetStars(int count)
        {
            count = Mathf.Clamp(count, 0, starImages.Length);
            for (var i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] == null) continue;
                var on = i < count;
                if (starActive != null && starInactive != null) starImages[i].sprite = on ? starActive : starInactive;
                starImages[i].color = on ? starActiveColor : starInactiveColor;
            }
        }

        private void SetCategory(int index, float value, float max)
        {
            if (index >= categoryValues.Length || categoryValues[index] == null) return;
            categoryValues[index].text = Mathf.RoundToInt(value) + " / " + Mathf.RoundToInt(max);
        }

        private void SetTilt(float degrees, float collapse)
        {
            if (tiltText != null) tiltText.text = "EN YÜKSEK EĞİM  " + degrees.ToString("F1") + "°";
            if (tiltTrack == null || tiltMarker == null) return;
            // Çubuk ortası 0°, iki uç devrilme açısı. En yüksek eğim büyüklük olduğundan sağa işaretlenir.
            var t = Mathf.Clamp01(degrees / collapse);
            var x = Mathf.Lerp(0.5f, 1f, t);
            tiltMarker.anchorMin = new Vector2(x, 0f);
            tiltMarker.anchorMax = new Vector2(x, 1f);
            tiltMarker.anchoredPosition = Vector2.zero;
        }

        private void SetLines(List<Line> lines)
        {
            var count = lines != null ? lines.Count : 0;
            while (lineInstances.Count < count)
            {
                var copy = Instantiate(linePrototype, lineContainer);
                copy.name = "Madde " + (lineInstances.Count + 1);
                lineInstances.Add(copy);
            }
            for (var i = 0; i < lineInstances.Count; i++)
            {
                var item = lineInstances[i];
                var on = i < count;
                item.gameObject.SetActive(on);
                if (!on) continue;
                var line = lines[i];
                var mark = line.kind == LineKind.Success ? "●" : line.kind == LineKind.Info ? "–" : "×";
                item.text = mark + "  " + line.text;
                item.color = line.kind == LineKind.Success ? successColor : line.kind == LineKind.Missing ? missingColor :
                    line.kind == LineKind.Critical ? criticalColor : infoColor;
                item.fontStyle = line.kind == LineKind.Critical ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        private static void SetButton(Button button, TMP_Text label, string caption, Action callback, out Action slot)
        {
            slot = callback;
            if (button == null) return;
            var visible = !string.IsNullOrEmpty(caption);
            button.interactable = visible && callback != null;
            if (label != null) label.text = visible ? caption : "";
        }
    }
}
