using Jam.Core;
using UnityEngine;

namespace Jam.UI
{
    /// <summary>
    /// Bir Canvas'a ekle; panelleri inspector'dan bağla.
    /// Faza / duraklatmaya göre doğru panel otomatik açılır.
    /// Butonlara OnClick ile Resume / Restart / LoadScene / Quit bağlanabilir.
    /// </summary>
    public class StatePanels : MonoBehaviour
    {
        public GameObject pausePanel;
        public GameObject resultsPanel;
        public GameObject gameOverPanel;

        private void OnEnable()
        {
            GameManager.OnPhaseChanged += OnPhase;
            GameManager.OnPauseChanged += OnPause;
        }

        private void OnDisable()
        {
            GameManager.OnPhaseChanged -= OnPhase;
            GameManager.OnPauseChanged -= OnPause;
        }

        private void Start() => Refresh();

        private void OnPhase(GamePhase _) => Refresh();
        private void OnPause(bool _) => Refresh();

        private void Refresh()
        {
            var gm = GameManager.Instance;
            GamePhase phase = gm != null ? gm.Phase : GamePhase.Preparation;
            bool paused = gm != null && gm.IsPaused;

            if (pausePanel)    pausePanel.SetActive(paused);
            if (resultsPanel)  resultsPanel.SetActive(phase == GamePhase.Results);
            if (gameOverPanel) gameOverPanel.SetActive(phase == GamePhase.GameOver);
        }

        public void Resume() => GameManager.Instance.SetPaused(false);
        public void Restart() => GameManager.Instance.Restart();
        public void LoadScene(string sceneName) => SceneLoader.Instance.Load(sceneName);
        public void Quit() => SceneLoader.Instance.Quit();
    }
}
