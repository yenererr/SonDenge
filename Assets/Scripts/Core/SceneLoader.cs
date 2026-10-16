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
