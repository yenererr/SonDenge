using System;
using Jam.Missions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Jam.UI.Menu
{
    /// <summary>
    /// Tek bölüm kartı (referans 02): tema renkli şerit, ev görseli, 0–3 yıldız (gerçek kayıttan), afet süresi, kilit.
    /// Seçili kart altın parıltıyla yükselir; seçili olmayanlar hafif mat görünür.
    /// </summary>
    public sealed class ChapterCardView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Button button;
        [SerializeField] private RectTransform visual;
        [SerializeField] private Image ribbon;
        [SerializeField] private TMP_Text numberText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private RawImage thumbnail;
        [SerializeField] private Image overlay;
        [SerializeField] private Image disasterIcon;
        [SerializeField] private Image[] stars = new Image[3];
        [SerializeField] private Sprite starOn;
        [SerializeField] private Sprite starOff;
        [SerializeField] private TMP_Text durationText;
        [SerializeField] private GameObject lockGroup;
        [SerializeField] private TMP_Text lockText;
        [SerializeField] private Image glow;
        [SerializeField] private CanvasGroup dim;
        [SerializeField] private float liftPixels = 18f;
        [SerializeField] private float selectedScale = 1.04f;

        public Action<int> Clicked;
        public Action<int> DoubleClicked;
        public int Index { get; private set; }
        public bool Unlocked { get; private set; }

        private bool selected;
        private Color baseTint = Color.white;
        private float k;

        private void Awake()
        {
            button.onClick.AddListener(() => Clicked?.Invoke(Index));
        }

        public void OnPointerClick(PointerEventData e)
        {
            if (e.clickCount >= 2) DoubleClicked?.Invoke(Index);
        }

        public void Setup(int index, LevelDefinition def, ChapterInfo info, bool unlocked, int bestStars)
        {
            Index = index;
            Unlocked = unlocked;
            numberText.text = (def != null ? def.levelNumber : index + 1) + ". BÖLÜM";
            nameText.text = def != null ? ShortName(def.disasterNameTr) : "?";
            ribbon.color = info.ribbonColor;
            baseTint = info.thumbnailTint;
            baseTint.a = 1f;
            if (thumbnail != null) thumbnail.color = unlocked ? info.thumbnailTint : Color.Lerp(info.thumbnailTint, new Color(0.3f, 0.3f, 0.35f), 0.6f);
            if (overlay != null) overlay.color = info.overlayTint;
            if (disasterIcon != null)
            {
                disasterIcon.sprite = info.disasterIcon;
                disasterIcon.enabled = info.disasterIcon != null;
            }
            for (var i = 0; i < stars.Length; i++)
                if (stars[i] != null) stars[i].sprite = i < bestStars ? starOn : starOff;
            durationText.text = def != null ? Mathf.RoundToInt(def.disasterSeconds) + " saniye" : "";
            lockGroup.SetActive(!unlocked);
            if (lockText != null) lockText.text = "KİLİTLİ";
        }

        /// <summary>"ŞİDDETLİ YAĞMUR" gibi uzun adları kart başlığına sığacak biçimde kısaltır.</summary>
        private static string ShortName(string disaster)
        {
            if (string.IsNullOrEmpty(disaster)) return "";
            var parts = disaster.Split(' ');
            return parts[parts.Length - 1];
        }

        public void SetSelected(bool value) => selected = value;

        private void Update()
        {
            var target = selected ? 1f : 0f;
            k = GameSettings.ReduceMotion ? target : Mathf.MoveTowards(k, target, Time.unscaledDeltaTime * 6f);
            var e = k * k * (3f - 2f * k);
            if (visual != null)
            {
                visual.anchoredPosition = new Vector2(0f, liftPixels * e);
                visual.localScale = Vector3.one * Mathf.Lerp(1f, selectedScale, e);
            }
            if (glow != null)
            {
                var pulse = GameSettings.ReduceMotion ? 1f : 0.85f + 0.15f * Mathf.Sin(Time.unscaledTime * 3f);
                glow.color = new Color(glow.color.r, glow.color.g, glow.color.b, e * pulse);
            }
            if (dim != null) dim.alpha = 1f;
            if (thumbnail != null && Unlocked) thumbnail.color = Color.Lerp(baseTint * 0.8f, baseTint, e);
        }
    }
}
