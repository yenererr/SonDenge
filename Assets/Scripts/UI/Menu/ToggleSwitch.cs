using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jam.UI.Menu
{
    /// <summary>Referans 06'daki yeşil anahtar görünümü: Toggle durumunu düğme konumu, renk ve "Açık/Kapalı" ile gösterir.</summary>
    [RequireComponent(typeof(Toggle))]
    public sealed class ToggleSwitch : MonoBehaviour
    {
        [SerializeField] private Image track;
        [SerializeField] private RectTransform knob;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private Color onColor = new Color32(58, 160, 74, 255);
        [SerializeField] private Color offColor = new Color32(150, 136, 120, 255);
        [SerializeField] private float travel = 26f;

        private Toggle toggle;
        private float k;

        private void Awake()
        {
            toggle = GetComponent<Toggle>();
            k = toggle.isOn ? 1f : 0f;
            Apply();
        }

        private void Update()
        {
            var target = toggle.isOn ? 1f : 0f;
            if (Mathf.Approximately(k, target)) return;
            k = GameSettings.ReduceMotion ? target : Mathf.MoveTowards(k, target, Time.unscaledDeltaTime * 8f);
            Apply();
        }

        private void Apply()
        {
            if (knob != null) knob.anchoredPosition = new Vector2(Mathf.Lerp(-travel, travel, k), 0f);
            if (track != null) track.color = Color.Lerp(offColor, onColor, k);
            if (stateText != null) stateText.text = toggle.isOn ? "Açık" : "Kapalı";
        }
    }
}
