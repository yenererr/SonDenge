using UnityEngine;

namespace Jam.Levels.Level1
{
    /// <summary>
    /// Kayıtlı sesler (Assets/Audio/Resources/Sfx): adım, afet döngüleri, eşya sabitleme.
    /// Sesler Resources'tan yüklendiği için sahnelere elle atama gerekmez.
    /// </summary>
    public sealed partial class L1_ProjectGameController
    {
        [Header("Ses — adım ve afet döngüleri")]
        [Tooltip("İki ayak sesi arası süre (sn).")] public float footstepInterval = 0.34f;
        [Range(0f, 1f)] public float footstepVolume = 0.45f;
        [Range(0f, 1f)] public float loopVolume = 0.55f;

        private Vector3 lastResidentPosition;
        private float stepClock, stormThunderClock = 6f;
        private bool sprayingNow;

        /// <summary>Görev bileşenleri (yönetmen, düğmeler) için tek giriş: kayıtlı sesi çalar.</summary>
        public void PlayRecordedSfx(string cue, float volume = 1f)
        {
            if (sfx != null) sfx.PlayClip(cue, volume);
        }

        private void UpdateAudio(bool paused)
        {
            if (sfx == null) return;
            var dt = Time.deltaTime;
            var disaster = !paused && state == RoundState.Disaster;
            sfx.SetLoop("fire", disaster && level == 3 && fires.Count > 0, loopVolume);
            sfx.SetLoop("spray", disaster && level == 3 && sprayingNow, loopVolume * 0.8f, 4f);
            sfx.SetLoop("wind", disaster && (level == 2 || level == 4 || level == 5), loopVolume * (level == 2 ? 1f : 0.45f));
            sfx.SetLoop("flood", disaster && level == 5, loopVolume);
            sfx.SetLoop("quake", disaster && level == 1, loopVolume * 1.2f, 2.5f);
            if (!disaster) sprayingNow = false;

            if (disaster && level == 2)
            {
                stormThunderClock -= dt;
                if (stormThunderClock <= 0f) { stormThunderClock = Random.Range(7f, 12f); sfx.PlayClip("thunder", 0.55f); }
            }
            if (paused || resident == null) return;
            UpdateFootsteps(dt);
        }

        private void UpdateFootsteps(float dt)
        {
            var position = resident.transform.position;
            var delta = position - lastResidentPosition;
            lastResidentPosition = position;
            if (dt <= 0f || delta.sqrMagnitude > 1f) return; // ışınlanma / yeniden doğma
            var speed = Mathf.Abs(delta.x) / dt;
            if (speed < 0.4f || resident.IsHiding || resident.IsClimbing) { stepClock = 0f; return; }
            stepClock -= dt;
            if (stepClock > 0f) return;
            stepClock = footstepInterval;
            sfx.PlayClip("step", footstepVolume, 0.08f);
        }
    }
}
