using System;
using System.Collections.Generic;
using Jam.Core;
using Jam.Missions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Jam.UI.Menu
{
    public enum MenuPageId { Main, ChapterSelect, Briefing, Loading, Help, Settings, About }

    /// <summary>Kartta ve brifingde gösterilen, bölüme özel sunum verisi (görevler/süre LevelDefinition'dan gelir).</summary>
    [Serializable]
    public sealed class ChapterInfo
    {
        public string subtitle = "İlk Sarsıntı";
        [Tooltip("Kart başlık şeridi rengi.")]
        public Color ribbonColor = new Color(0.72f, 0.45f, 0.2f);
        [Tooltip("Kart görselinin tonu (afet atmosferi).")]
        public Color thumbnailTint = Color.white;
        [Tooltip("Kart görselinin üstündeki afet katmanı (alt-üst degrade).")]
        public Color overlayTint = new Color(1f, 0.6f, 0.2f, 0.25f);
        public Sprite disasterIcon;
        [TextArea(2, 3)] public string selectLine;
        [TextArea(2, 3)] public string briefingLine;
        [TextArea(2, 3)] public string safetyNote;
        [Tooltip("Görev ikonları, LevelDefinition.tasks sırasıyla.")]
        public Sprite[] taskIcons = new Sprite[0];
        [Tooltip("Deprem: Çök–Kapan–Tutun şeridi gösterilir.")]
        public bool showDropCoverHold;
    }

    /// <summary>
    /// Menünün tek akış kaynağı: tek aktif sayfa, geri yığını, ESC, onay penceresi, bölüm seçimi ve başlatma.
    /// Sahne geçişleri yalnız SceneLoader üzerinden yapılır.
    /// </summary>
    public sealed class MenuFlowController : MonoBehaviour
    {
        [Serializable]
        private struct PageEntry
        {
            public MenuPageId id;
            public MenuPage page;
        }

        [SerializeField] private PageEntry[] pages = new PageEntry[0];
        [SerializeField] private ConfirmDialog confirm;
        [SerializeField] private MenuAudioBridge menuAudio;
        [SerializeField] private MenuWilson wilson;
        [SerializeField] private LoadingView loading;
        [Header("Bölümler (sırayla 1..5)")]
        [SerializeField] private LevelDefinition[] levels = new LevelDefinition[0];
        [SerializeField] private ChapterInfo[] chapters = new ChapterInfo[0];
        [Header("Müzik")]
        [SerializeField] private AudioClip menuMusic;

        public LevelDefinition[] Levels => levels;
        public ChapterInfo[] Chapters => chapters;
        public MenuWilson Wilson => wilson;
        public ConfirmDialog Confirm => confirm;
        public MenuPage Current { get; private set; }
        public int SelectedChapter { get; set; }

        private readonly Stack<MenuPage> history = new Stack<MenuPage>();
        private readonly Dictionary<MenuPageId, MenuPage> map = new Dictionary<MenuPageId, MenuPage>();

        private void Awake()
        {
            foreach (var entry in pages)
            {
                if (entry.page == null) continue;
                map[entry.id] = entry.page;
                entry.page.Bind(this);
                entry.page.Hide(true);
            }
            SelectedChapter = HighestUnlockedIndex();
        }

        private void Start()
        {
            Time.timeScale = 1f;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetPaused(false);
                GameManager.Instance.SetPhase(GamePhase.Menu);
            }
            GameSettings.Apply();
            GameSettings.ApplyQuality();
            if (AudioManager.Instance != null && menuMusic != null) AudioManager.Instance.PlayMusic(menuMusic);
            Open(MenuPageId.Main, false);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame) return;
            if (confirm != null && confirm.IsOpen) { confirm.Cancel(); return; }
            if (Current == null || Current.HandleBack()) return;
            if (Current == Page(MenuPageId.Main) || Current == Page(MenuPageId.Loading)) return;
            Back();
        }

        public MenuPage Page(MenuPageId id) => map.TryGetValue(id, out var p) ? p : null;

        public bool IsCurrent(MenuPage page) => page != null && Current == page && (confirm == null || !confirm.IsOpen);

        /// <summary>Yeni sayfa açar; push=true ise GERİ ile önceki sayfaya dönülür.</summary>
        public void Open(MenuPageId id, bool push = true)
        {
            var next = Page(id);
            if (next == null || next == Current) return;
            if (Current != null)
            {
                if (push) history.Push(Current);
                Current.Hide();
            }
            Current = next;
            next.Show();
            Play(MenuSound.Open);
        }

        public void Back()
        {
            if (history.Count == 0) return;
            Current.Hide();
            Current = history.Pop();
            Current.Show();
            Play(MenuSound.Open);
        }

        public void Play(MenuSound sound)
        {
            if (menuAudio != null) menuAudio.Play(sound);
        }

        // ------------------------------------------------------------------ bölümler
        public int HighestUnlockedIndex()
        {
            var best = 0;
            for (var i = 0; i < levels.Length; i++)
                if (LevelProgressSave.IsUnlocked(LevelNumber(i))) best = i;
            return best;
        }

        public int LevelNumber(int index) => levels[index] != null ? levels[index].levelNumber : index + 1;
        public bool IsUnlocked(int index) => LevelProgressSave.IsUnlocked(LevelNumber(index));
        public int BestStars(int index) => LevelProgressSave.BestStars(LevelNumber(index));
        public ChapterInfo Chapter(int index) => index >= 0 && index < chapters.Length ? chapters[index] : new ChapterInfo();

        public bool AnyProgress()
        {
            for (var i = 0; i < levels.Length; i++)
                if (BestStars(i) > 0 || (i > 0 && IsUnlocked(i))) return true;
            return false;
        }

        /// <summary>Brifingde HAZIRLIĞA BAŞLA: yükleme ekranına geçer ve gerçek async yüklemeyi başlatır.</summary>
        public void StartChapter(int index)
        {
            if (index < 0 || index >= levels.Length || levels[index] == null || !IsUnlocked(index))
            {
                Play(MenuSound.Error);
                return;
            }
            var def = levels[index];
            // Menü brifingi görüldü: bölüm sahnesi kısa "hazır mısın" akışıyla açılır, hazırlık süresi orada başlar.
            LevelProgressSave.MarkBriefingSeen(def.levelNumber);
            Play(MenuSound.Confirm);
            history.Clear();
            Open(MenuPageId.Loading, false);
            loading.Begin(def, Chapter(index), OnLoadFailed);
        }

        private void OnLoadFailed(string message)
        {
            Play(MenuSound.Error);
            confirm.Show("BÖLÜM AÇILAMADI", message, "ANA MENÜ", null, ReturnToMain);
        }

        public void ReturnToMain()
        {
            history.Clear();
            Open(MenuPageId.Main, false);
        }

        public void RequestQuit()
        {
            confirm.Show("ÇIKIŞ", "Oyundan çıkmak istediğine emin misin?", "EVET, ÇIK", "VAZGEÇ",
                () => { if (SceneLoader.Instance != null) SceneLoader.Instance.Quit(); });
        }
    }
}
