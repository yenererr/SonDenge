using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Jam.Core
{
    /// <summary>
    /// Oyun fazının tek kaynağı. Diğer sistemler OnPhaseChanged'i dinler,
    /// birbirine doğrudan referans vermez.
    /// Kullanım: GameManager.Instance.SetPhase(GamePhase.Disaster);
    /// Duraklatma bir faz değildir: SetPaused(true) / IsPaused.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // Sandbox sahnelerinde Play'e basınca doğrudan hazırlıktan başlansın
        public GamePhase Phase { get; private set; } = GamePhase.Preparation;
        public bool IsPaused { get; private set; }

        public static event Action<GamePhase> OnPhaseChanged;
        public static event Action<bool> OnPauseChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Update()
        {
            bool pausable = Phase == GamePhase.Preparation || Phase == GamePhase.Disaster;
            if (pausable && PausePressed()) SetPaused(!IsPaused);
        }

        public void SetPhase(GamePhase newPhase)
        {
            if (newPhase == Phase) return;
            Phase = newPhase;
            SetPaused(false);
            OnPhaseChanged?.Invoke(newPhase);
        }

        public void SetPaused(bool paused)
        {
            if (paused == IsPaused) return;
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            OnPauseChanged?.Invoke(paused);
        }

        public void Restart()
        {
            SetPaused(false);
            Phase = GamePhase.Preparation;
            SceneLoader.Instance.ReloadCurrent();
            OnPhaseChanged?.Invoke(Phase);
        }

        private static bool PausePressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }
    }
}
