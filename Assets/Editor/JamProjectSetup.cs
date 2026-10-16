using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Jam.EditorTools
{
    /// <summary>
    /// Proje açıldığında git dostu ayarları zorlar ve "Jam" menüsü ekler.
    /// </summary>
    [InitializeOnLoad]
    public static class JamProjectSetup
    {
        static JamProjectSetup()
        {
            EditorApplication.delayCall += EnforceSettings;
        }

        [MenuItem("Jam/Proje Ayarlarını Uygula")]
        public static void EnforceSettings()
        {
            bool changed = false;

            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                EditorSettings.serializationMode = SerializationMode.ForceText;
                changed = true;
            }

            if (VersionControlSettings.mode != "Visible Meta Files")
            {
                VersionControlSettings.mode = "Visible Meta Files";
                changed = true;
            }

            if (changed)
            {
                AssetDatabase.SaveAssets();
                Debug.Log("[Jam] Force Text + Visible Meta Files uygulandı.");
            }
        }

        [MenuItem("Jam/Build/WebGL")]
        public static void BuildWebGL() => Build(BuildTarget.WebGL, "Builds/WebGL");

        [MenuItem("Jam/Build/Windows")]
        public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, $"Builds/Windows/{PlayerSettings.productName}.exe");

        private static void Build(BuildTarget target, string path)
        {
            var scenes = System.Array.FindAll(EditorBuildSettings.scenes, s => s.enabled);
            if (scenes.Length == 0)
            {
                EditorUtility.DisplayDialog("Jam Build", "Build listesinde sahne yok. File > Build Profiles üzerinden sahneleri ekleyin.", "Tamam");
                return;
            }

            if (target == BuildTarget.WebGL)
            {
                // itch.io'da sorunsuz çalışması için
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
                PlayerSettings.WebGL.decompressionFallback = true;
            }

            var opts = new BuildPlayerOptions
            {
                scenes = System.Array.ConvertAll(scenes, s => s.path),
                locationPathName = path,
                target = target,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(opts);
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[Jam] Build tamam: {path} ({report.summary.totalSize / (1024 * 1024)} MB)");
                EditorUtility.RevealInFinder(path);
            }
            else
            {
                Debug.LogError($"[Jam] Build başarısız: {report.summary.result}");
            }
        }
    }
}
