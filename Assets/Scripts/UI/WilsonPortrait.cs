using Jam.Missions;
using UnityEngine;

namespace Jam.UI
{
    /// <summary>
    /// Mevcut 3D Wilson modelinin kütlesiz bir kopyasını sahnenin çok uzağında, yardımcı bir kamerayla
    /// RenderTexture'a çizer. Kamera MainCamera değildir ve AudioListener içermez (şartname §3.2).
    /// Ruh hâline göre yalnız transform ile düşük riskli hareket: baş sallama, zıplama, sinmiş poz.
    /// </summary>
    public sealed class WilsonPortrait : MonoBehaviour
    {
        private static readonly Vector3 StagePosition = new Vector3(0f, -400f, 0f);

        public RenderTexture Texture { get; private set; }
        private Camera portraitCamera;
        private Transform pivot, model;
        private WilsonMood mood = WilsonMood.Normal;
        private float moodStart, talkUntil;
        private Material modelMaterial;

        /// <summary>buildModel: verilen host altına modeli kurar ve oluşturduğu materyali döndürür.</summary>
        public static WilsonPortrait Create(Transform parent, System.Func<Transform, Material> buildModel)
        {
            var go = new GameObject("Wilson portre sahnesi");
            go.transform.SetParent(parent, false);
            var portrait = go.AddComponent<WilsonPortrait>();
            portrait.Build(buildModel);
            return portrait;
        }

        private void Build(System.Func<Transform, Material> buildModel)
        {
            Texture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32) { name = "Wilson portre", antiAliasing = 2 };
            var stage = new GameObject("Sahne").transform;
            stage.SetParent(transform, false);
            stage.position = StagePosition;
            pivot = new GameObject("Pivot").transform;
            pivot.SetParent(stage, false);
            model = new GameObject("Model").transform;
            model.SetParent(pivot, false);
            if (buildModel != null) modelMaterial = buildModel(model);
            // Kamera tavuğa hafif sağdan, -Z yönünden bakar (oyun kamerasıyla aynı taraf).
            var camGo = new GameObject("Wilson portre kamerası");
            camGo.transform.SetParent(stage, false);
            camGo.transform.localPosition = new Vector3(0.25f, 0.42f, -1.55f);
            camGo.transform.LookAt(stage.position + new Vector3(0f, 0.3f, 0f));
            portraitCamera = camGo.AddComponent<Camera>();
            portraitCamera.clearFlags = CameraClearFlags.SolidColor;
            portraitCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            portraitCamera.fieldOfView = 30f;
            portraitCamera.nearClipPlane = 0.05f;
            portraitCamera.farClipPlane = 6f;
            portraitCamera.targetTexture = Texture;
            portraitCamera.depth = -10f;
            var light = new GameObject("Portre ışığı").AddComponent<Light>();
            light.transform.SetParent(stage, false);
            light.transform.localPosition = new Vector3(-0.8f, 1.2f, -1.2f);
            light.type = LightType.Point;
            light.range = 4f;
            light.intensity = 2.2f;
            light.color = new Color(1f, 0.93f, 0.82f);
            SetActive(false);
        }

        public void SetActive(bool on)
        {
            if (portraitCamera != null) portraitCamera.enabled = on;
        }

        public void SetMood(WilsonMood next)
        {
            mood = next;
            moodStart = Time.unscaledTime;
        }

        /// <summary>Konuşurken gaga yerine gövdeye küçük ritmik baş sallama verir.</summary>
        public void Talk(float seconds) => talkUntil = Time.unscaledTime + seconds;

        private void Update()
        {
            if (pivot == null) return;
            var t = Time.unscaledTime;
            var since = t - moodStart;
            var reduce = GameSettings.ReduceShake ? 0.35f : 1f;
            var bob = Mathf.Sin(t * 2.2f) * 0.012f;
            var yaw = Mathf.Sin(t * 0.9f) * 9f;
            var roll = 0f;
            var pitch = 0f;
            var hop = 0f;
            var scale = 1f;
            switch (mood)
            {
                case WilsonMood.Sevincli:
                case WilsonMood.Heyecanli:
                    hop = Mathf.Abs(Mathf.Sin(since * 7f)) * 0.08f * Mathf.Clamp01(1.4f - since * 0.5f + 0.6f);
                    roll = Mathf.Sin(t * 6f) * 4f;
                    break;
                case WilsonMood.Endiseli:
                    yaw += Mathf.Sin(t * 13f) * 6f;
                    break;
                case WilsonMood.Uzgun:
                    pitch = 14f;
                    scale = 0.96f;
                    break;
                case WilsonMood.Anlatici:
                    roll = Mathf.Sin(t * 1.6f) * 5f;
                    break;
            }
            if (t < talkUntil) pitch += Mathf.Sin(t * 16f) * 5f;
            pivot.localPosition = new Vector3(0f, (bob + hop) * reduce, 0f);
            pivot.localRotation = Quaternion.Euler(pitch * reduce, 180f + 25f + yaw * reduce, roll * reduce);
            pivot.localScale = Vector3.one * scale;
        }

        private void OnDestroy()
        {
            if (portraitCamera != null) portraitCamera.targetTexture = null;
            if (Texture != null) { Texture.Release(); Destroy(Texture); }
            if (modelMaterial != null) Destroy(modelMaterial);
        }
    }
}
