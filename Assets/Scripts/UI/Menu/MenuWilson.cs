using Jam.Levels.Level1;
using Jam.Missions;
using UnityEngine;
using UnityEngine.UI;

namespace Jam.UI.Menu
{
    /// <summary>
    /// Menüde tek bir Wilson: oyundaki lisanslı low-poly tavuk modeli (Pjager, CC BY 4.0) yardımcı bir
    /// kamerayla RenderTexture'a çizilir; bütün sayfalardaki RawImage'lar aynı dokuyu paylaşır.
    /// Kamera MainCamera değildir ve AudioListener taşımaz (WilsonPortrait).
    /// </summary>
    public sealed class MenuWilson : MonoBehaviour
    {
        [SerializeField] private GameObject chickenModel;
        [SerializeField] private Texture2D chickenTexture;
        [SerializeField] private float modelHeight = 0.62f;
        [Tooltip("Wilson'ın göründüğü bütün RawImage'lar.")]
        [SerializeField] private RawImage[] targets = new RawImage[0];

        private WilsonPortrait portrait;

        private void Awake()
        {
            if (chickenModel == null)
            {
                Debug.LogError("MenuWilson: tavuk modeli atanmadı.", this);
                return;
            }
            portrait = WilsonPortrait.Create(transform,
                host => L1_WilsonChicken.BuildPortraitModel(chickenModel, chickenTexture, host, modelHeight));
            foreach (var target in targets)
                if (target != null) target.texture = portrait.Texture;
            portrait.SetActive(true);
        }

        /// <summary>Kısa selamlama: sevinçli zıplama + konuşma baş sallaması.</summary>
        public void Greet() => Say(WilsonMood.Sevincli, 1.4f);

        public void Say(WilsonMood mood, float seconds)
        {
            if (portrait == null) return;
            portrait.SetMood(GameSettings.ReduceMotion ? WilsonMood.Normal : mood);
            portrait.Talk(GameSettings.ReduceMotion ? 0f : seconds);
        }
    }
}
