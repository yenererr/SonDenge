using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jam.UI.Menu
{
    /// <summary>Nasıl Oynanır satırı: klavye tuşu (keycap), fare ikonu veya madde ikonu + açıklama.</summary>
    public sealed class HelpRowView : MonoBehaviour
    {
        [SerializeField] private GameObject keycap;
        [SerializeField] private LayoutElement keycapLayout;
        [SerializeField] private TMP_Text keyText;
        [SerializeField] private Image badgeIcon;
        [SerializeField] private TMP_Text text;

        public void Set(HelpItem item)
        {
            var hasKey = !string.IsNullOrEmpty(item.key);
            keycap.SetActive(hasKey);
            if (hasKey)
            {
                keyText.text = item.key;
                if (keycapLayout != null) keycapLayout.preferredWidth = keycapLayout.minWidth = Mathf.Max(84f, 40f + item.key.Length * 24f);
            }
            badgeIcon.gameObject.SetActive(!hasKey && item.icon != null);
            badgeIcon.sprite = item.icon;
            text.text = item.text;
        }
    }

    [System.Serializable]
    public sealed class HelpItem
    {
        [Tooltip("Klavye tuşu etiketi (ör. A / D). Boşsa ikon gösterilir.")]
        public string key;
        public Sprite icon;
        [TextArea(1, 3)] public string text;
    }

    [System.Serializable]
    public sealed class HelpSection
    {
        public string tabTitle;
        public Sprite tabIcon;
        public string heading;
        [TextArea(2, 3)] public string wilsonLine;
        public HelpItem[] items = new HelpItem[0];
    }
}
