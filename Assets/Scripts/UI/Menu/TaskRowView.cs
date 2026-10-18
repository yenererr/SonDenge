using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jam.UI.Menu
{
    /// <summary>Brifing görev satırı: ikon + görev adı.</summary>
    public sealed class TaskRowView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text label;

        public void Set(Sprite sprite, string text)
        {
            icon.sprite = sprite;
            icon.enabled = sprite != null;
            label.text = text;
        }
    }
}
