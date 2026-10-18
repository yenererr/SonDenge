using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jam.UI.Menu
{
    /// <summary>Sol sütun sekmesi: etkin sekme altın kalas, diğerleri açık parşömen.</summary>
    public sealed class TabButtonView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private Sprite inactiveSprite;
        [SerializeField] private Color activeText = new Color32(51, 37, 28, 255);
        [SerializeField] private Color inactiveText = new Color32(70, 50, 36, 255);
        [SerializeField] private GameObject activeArrow;

        public Button Button => button;

        public void Set(string text, Sprite sprite)
        {
            label.text = text;
            if (icon != null)
            {
                icon.sprite = sprite;
                icon.enabled = sprite != null;
            }
        }

        public void SetActive(bool active)
        {
            background.sprite = active ? activeSprite : inactiveSprite;
            label.color = active ? activeText : inactiveText;
            label.fontStyle = active ? FontStyles.Bold : FontStyles.Normal;
            if (activeArrow != null) activeArrow.SetActive(active);
        }
    }
}
