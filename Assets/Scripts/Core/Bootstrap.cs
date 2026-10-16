using UnityEngine;

namespace Jam.Core
{
    /// <summary>
    /// Oyun hangi sahneden başlatılırsa başlatılsın çekirdek sistemleri kurar.
    /// Böylece herkes kendi test sahnesinde Play'e basabilir; ayrı bir "Boot" sahnesi gerekmez.
    /// </summary>
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            if (Object.FindFirstObjectByType<GameManager>() != null) return;

            var root = new GameObject("[Systems]");
            Object.DontDestroyOnLoad(root);

            root.AddComponent<GameManager>();
            root.AddComponent<AudioManager>();
            root.AddComponent<SceneLoader>();
        }
    }
}
