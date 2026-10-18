using System.IO;
using Jam.UI.Menu;
using UnityEngine;

namespace Jam.EditorTools
{
    /// <summary>
    /// Play modunda menü QA'sı için yardımcılar (MCP execute_code ile reflection üzerinden çağrılır):
    /// sayfa açma, seçili bölüm, ekran görüntüsünü Assets dışına (docs/production/screenshots/menu) kaydetme.
    /// </summary>
    public static class MenuQa
    {
        public static string Open(string page)
        {
            var flow = Object.FindAnyObjectByType<MenuFlowController>();
            if (flow == null) return "MenuFlowController yok (Play modunda Menu sahnesi açık olmalı)";
            flow.Open((MenuPageId)System.Enum.Parse(typeof(MenuPageId), page));
            return "açık: " + flow.Current.name;
        }

        public static string Select(int chapter)
        {
            var flow = Object.FindAnyObjectByType<MenuFlowController>();
            flow.SelectedChapter = chapter;
            return "seçili: " + chapter;
        }

        public static string Back()
        {
            var flow = Object.FindAnyObjectByType<MenuFlowController>();
            flow.Back();
            return "açık: " + flow.Current.name;
        }

        public static string Current()
        {
            var flow = Object.FindAnyObjectByType<MenuFlowController>();
            if (flow == null || flow.Current == null) return "yok";
            var active = "";
            foreach (var page in flow.GetComponentsInChildren<MenuPage>(true))
                if (page.gameObject.activeSelf) active += page.name + (page.IsOpen ? "(açık)" : "(kapanıyor)") + " ";
            return flow.Current.name + " | aktif: " + active;
        }

        public static string Capture(string fileName)
        {
            var dir = Path.GetFullPath(Path.Combine(Application.dataPath, "../docs/production/screenshots/menu"));
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, fileName + ".png");
            ScreenCapture.CaptureScreenshot(path);
            return path;
        }

        /// <summary>Bir düğmeye gerçek tıklama olayı gönderir (onClick).</summary>
        public static string Click(string path)
        {
            var go = GameObject.Find(path);
            if (go == null) return "bulunamadı: " + path;
            var button = go.GetComponent<UnityEngine.UI.Button>();
            if (button == null) return "Button yok: " + path;
            if (!button.IsInteractable()) return "etkileşimsiz: " + path;
            button.onClick.Invoke();
            return "tıklandı: " + path + " → " + Current();
        }
    }
}
