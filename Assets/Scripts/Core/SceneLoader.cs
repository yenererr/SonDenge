using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Jam.Core
{
    /// <summary>
    /// Sahne geçişleri, siyaha fade ile.
    /// SceneLoader.Instance.Load("Game");
    /// Not: Sahnelerin File > Build Profiles (Build Settings) listesinde olması gerekir.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }
        public float fadeDuration = 0.3f;

        private CanvasGroup _fade;
        private bool _busy;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            CreateFadeOverlay();
        }

        public void Load(string sceneName)
        {
            if (!_busy) StartCoroutine(LoadRoutine(sceneName));
        }

        public void ReloadCurrent() => Load(SceneManager.GetActiveScene().name);

        public bool IsBusy => _busy;

        /// <summary>
        /// Gerçek async yükleme: progress(0..0.9) = Unity yükleme ilerlemesi, 1 = etkinleştirme.
        /// Çok hızlı yüklemede ekran en az minVisibleSeconds görünür; sahte yüzde üretilmez.
        /// Sahne build listesinde yoksa failed çağrılır ve hiçbir şey yüklenmez.
        /// </summary>
        public void LoadWithProgress(string sceneName, System.Action<float> progress, System.Action<string> failed,
            float minVisibleSeconds = 0.5f)
        {
            if (_busy) return;
            if (string.IsNullOrEmpty(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
            {
                failed?.Invoke("Bölüm sahnesi bulunamadı: " + sceneName);
                return;
            }
            StartCoroutine(ProgressRoutine(sceneName, progress, failed, minVisibleSeconds));
        }

        private IEnumerator ProgressRoutine(string sceneName, System.Action<float> progress, System.Action<string> failed,
            float minVisibleSeconds)
        {
            _busy = true;
            var start = Time.unscaledTime;
            var op = SceneManager.LoadSceneAsync(sceneName);
            if (op == null)
            {
                _busy = false;
                failed?.Invoke("Bölüm yüklenemedi: " + sceneName);
                yield break;
            }
            op.allowSceneActivation = false;
            while (op.progress < 0.9f)
            {
                progress?.Invoke(Mathf.Clamp01(op.progress / 0.9f) * 0.9f);
                yield return null;
            }
            progress?.Invoke(0.9f);
            while (Time.unscaledTime - start < minVisibleSeconds) yield return null;
            progress?.Invoke(1f);
            yield return Fade(0f, 1f);
            op.allowSceneActivation = true;
            while (!op.isDone) yield return null;
            if (GameManager.Instance != null) GameManager.Instance.SetPaused(false);
            Time.timeScale = 1f;
            yield return Fade(1f, 0f);
            _busy = false;
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            _busy = true;
            yield return Fade(0f, 1f);
            yield return SceneManager.LoadSceneAsync(sceneName);
            // Pause/GameOver panelinden sahne değişince durum takılı kalmasın
            if (GameManager.Instance != null) GameManager.Instance.SetPaused(false);
            Time.timeScale = 1f;
            yield return Fade(1f, 0f);
            _busy = false;
        }

        private IEnumerator Fade(float from, float to)
        {
            _fade.blocksRaycasts = true;
            for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
            {
                _fade.alpha = Mathf.Lerp(from, to, t / fadeDuration);
                yield return null;
            }
            _fade.alpha = to;
            _fade.blocksRaycasts = to > 0.5f;
        }

        private void CreateFadeOverlay()
        {
            var go = new GameObject("FadeCanvas");
            go.transform.SetParent(transform);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            _fade = go.AddComponent<CanvasGroup>();
            _fade.alpha = 0f;
            _fade.blocksRaycasts = false;

            var img = new GameObject("Black").AddComponent<UnityEngine.UI.Image>();
            img.transform.SetParent(go.transform, false);
            img.color = Color.black;
            var rt = img.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
