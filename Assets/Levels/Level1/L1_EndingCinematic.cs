using System;
using Jam.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Jam.Levels.Level1
{
    /// <summary>
    /// Bölüm 5 başarıyla bitince Resources/Ending/ending_1..n videolarını sırayla tam ekran oynatır,
    /// en sonda "SON" yazısını gösterir. Tıklama / Boşluk sıradaki videoya geçer, Esc hepsini atlar.
    /// </summary>
    public sealed class L1_EndingCinematic : MonoBehaviour
    {
        [Tooltip("SON yazısı kaç saniye ekranda kalsın (tıklayınca da kapanır).")]
        public float endCardSeconds = 6f;

        private VideoClip[] clips;
        private VideoPlayer player;
        private RenderTexture target;
        private RawImage screen;
        private AspectRatioFitter fitter;
        private GameObject endCard;
        private Action onDone;
        private int index = -1;
        private float endCardLeft, inputGrace;

        public static void Play(Transform parent, Action done)
        {
            var clips = Resources.LoadAll<VideoClip>("Ending");
            if (clips == null || clips.Length == 0) { done?.Invoke(); return; }
            Array.Sort(clips, (a, b) => string.CompareOrdinal(a.name, b.name));
            var go = new GameObject("Final videoları");
            go.transform.SetParent(parent, false);
            var cinematic = go.AddComponent<L1_EndingCinematic>();
            cinematic.clips = clips;
            cinematic.onDone = done;
            cinematic.Build();
            cinematic.Next();
        }

        private void Build()
        {
            var canvas = UiKit.CreateCanvas(transform, "FinalCanvas", 900).transform;
            var black = UiKit.Stretch(canvas, "Siyah");
            UiKit.Panel(black, Color.black, true);
            var video = UiKit.Stretch(canvas, "Video");
            screen = video.gameObject.AddComponent<RawImage>();
            screen.raycastTarget = false;
            fitter = video.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 16f / 9f;

            endCard = UiKit.Stretch(canvas, "SON").gameObject;
            var label = UiKit.Label(endCard.transform, "Yazı", "SON", 220, UiKit.Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
            endCard.SetActive(false);

            player = gameObject.AddComponent<VideoPlayer>();
            player.playOnAwake = false;
            player.isLooping = false;
            player.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;
            player.renderMode = VideoRenderMode.RenderTexture;
            player.audioOutputMode = VideoAudioOutputMode.Direct;
            player.loopPointReached += _ => Next();
            if (Jam.Core.AudioManager.Instance != null) Jam.Core.AudioManager.Instance.StopMusic();
        }

        private void Next()
        {
            index++;
            inputGrace = 0.4f;
            if (index >= clips.Length) { ShowEndCard(); return; }
            var clip = clips[index];
            if (target == null || target.width != (int)clip.width || target.height != (int)clip.height)
            {
                if (target != null) target.Release();
                target = new RenderTexture((int)clip.width, (int)clip.height, 0);
            }
            fitter.aspectRatio = clip.width / (float)Mathf.Max(1u, clip.height);
            player.targetTexture = target;
            screen.texture = target;
            player.clip = clip;
            var volume = Jam.Core.AudioManager.Instance != null ? Jam.Core.AudioManager.Instance.sfxVolume : 1f;
            for (ushort t = 0; t < clip.audioTrackCount; t++) player.SetDirectAudioVolume(t, volume);
            player.Play();
        }

        private void ShowEndCard()
        {
            player.Stop();
            screen.gameObject.SetActive(false);
            endCard.SetActive(true);
            endCardLeft = endCardSeconds;
        }

        private void Update()
        {
            inputGrace -= Time.unscaledDeltaTime;
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            var advance = inputGrace <= 0f && ((mouse != null && mouse.leftButton.wasPressedThisFrame) ||
                (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)) ||
                (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame));
            var skipAll = keyboard != null && keyboard.escapeKey.wasPressedThisFrame;

            if (endCard.activeSelf)
            {
                endCardLeft -= Time.unscaledDeltaTime;
                if (advance || skipAll || endCardLeft <= 0f) Finish();
                return;
            }
            if (skipAll) { index = clips.Length - 1; Next(); }
            else if (advance) Next();
        }

        private void Finish()
        {
            var done = onDone;
            onDone = null;
            Destroy(gameObject);
            done?.Invoke();
        }

        private void OnDestroy()
        {
            if (target != null) target.Release();
        }
    }
}
