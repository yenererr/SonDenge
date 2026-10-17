using System;
using System.Collections;
using UnityEngine.Rendering;
using System.Collections.Generic;
using Jam.Core;
using Jam.Economy;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Jam.Shop
{
    /// <summary>
    /// Level2 mağaza paneli. Hazırlık fazında mobilya satın almayı yönetir.
    /// Satın alınan veri, event üzerinden mobilya yerleştirme sistemine aktarılır.
    /// </summary>
    public sealed class L2_ShopPanel : MonoBehaviour
    {
        [Serializable]
        public sealed class FurniturePurchasedEvent : UnityEvent<FurnitureData> { }

        [Header("Katalog")]
        [SerializeField] private FurnitureData[] catalog;
        [Min(0)] [SerializeField] private int startingBudget = 100;

        [Header("UI Referansları")]
        [SerializeField] private bool autoCreateUi = true;
        [SerializeField] private bool showOnStart = true;
        [SerializeField] private GameObject shopContent;
        [SerializeField] private Transform itemListRoot;
        [SerializeField] private Button itemButtonPrefab;
        [SerializeField] private Text budgetText;
        [SerializeField] private Text feedbackText;
        [SerializeField] private Button closeButton;

        [Header("Entegrasyon")]
        [SerializeField] private FurniturePurchasedEvent onFurniturePurchased;

        private readonly List<Button> _itemButtons = new List<Button>();
        private readonly List<FurnitureData> _catalogItems = new List<FurnitureData>();
        [Header("Mağaza görünümü")]
        [SerializeField] private Color copper = new Color(0.66f, 0.45f, 0.29f);
        [SerializeField] private Color parchment = new Color(0.94f, 0.86f, 0.74f);
        [SerializeField] private Color stone = new Color(0.23f, 0.22f, 0.20f);
        [SerializeField] private Color purchaseGreen = new Color(0.27f, 0.40f, 0.24f);
        [SerializeField, Range(128, 1024)] private int previewResolution = 384;
        private readonly List<UnityEngine.Object> _visualResources = new List<UnityEngine.Object>();
        private readonly List<RawImage> _previews = new List<RawImage>();
        private readonly List<Text> _prices = new List<Text>();
        private GameObject _generatedCanvas;
        private Sprite _cardSurface, _buySurface, _backSurface;
        private int _budget;
        private bool _purchasesEnabled = true;

        /// <summary>Mağazadaki mevcut para. Lead ekonomi sistemiyle senkronlanabilir.</summary>
        public int Budget => _budget;

        /// <summary>Satın alma sonrası yerleştirme sisteminin dinleyebileceği event.</summary>
        public event Action<FurnitureData> OnFurniturePurchased;

        private void Awake()
        {
            _budget = startingBudget;

            CreateDefaultUiIfNeeded();

            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            BuildCatalogButtons();
            RefreshView();

            if (!showOnStart)
                Close();
        }

        private void OnEnable()
        {
            GameManager.OnPhaseChanged += OnPhaseChanged;
        }

        private void OnDisable()
        {
            GameManager.OnPhaseChanged -= OnPhaseChanged;
        }

        /// <summary>
        /// Mağazayı açar. Mağaza yalnızca hazırlık fazında kullanılabilir.
        /// </summary>
        public void Open()
        {
            if (!IsPreparationPhase())
            {
                SetFeedback("Mağaza yalnızca hazırlık aşamasında kullanılabilir.");
                return;
            }

            if (shopContent != null)
                shopContent.SetActive(true);

            RefreshView();
        }

        public void Close()
        {
            if (shopContent != null)
                shopContent.SetActive(false);
        }

        public void Toggle()
        {
            if (shopContent != null && shopContent.activeSelf)
                Close();
            else
                Open();
        }

        /// <summary>
        /// Lead'in ortak ekonomi sistemi hazır olduğunda başlangıç bütçesini aktarabilmesi için kullanılır.
        /// </summary>
        public void SetBudget(int budget)
        {
            _budget = Mathf.Max(0, budget);
            RefreshView();
        }

        /// <summary>Yerleştirme sistemi gibi yardımcı sistemlerin mağaza mesajını güncellemesi için kullanılır.</summary>
        public void ShowFeedback(string message)
        {
            SetFeedback(message);
        }

        /// <summary>Hazırlıkta iptal edilen yerleştirme için satın alma bedelini iade eder.</summary>
        public void RefundPurchase(int amount)
        {
            _budget += Mathf.Max(0, amount);
            RefreshView();
        }

        /// <summary>Bir mobilya yerleştirilirken ikinci satın almayı engeller.</summary>
        public void SetPurchasingEnabled(bool enabled)
        {
            _purchasesEnabled = enabled;
            RefreshView();
        }

        /// <summary>
        /// Bir mobilya satın alır; başarı durumunu döndürür.
        /// </summary>
        public bool TryPurchase(FurnitureData furniture)
        {
            if (furniture == null)
            {
                SetFeedback("Satın alınacak eşya bulunamadı.");
                return false;
            }

            if (!_purchasesEnabled)
                return false;

            if (!IsPreparationPhase())
            {
                SetFeedback("Eşya yalnızca hazırlık aşamasında satın alınabilir.");
                return false;
            }

            if (furniture.PurchasePrice > _budget)
            {
                SetFeedback("Bütçen bu eşya için yeterli değil.");
                RefreshView();
                return false;
            }

            _budget -= furniture.PurchasePrice;
            SetFeedback(furniture.DisplayName + " satın alındı.");
            OnFurniturePurchased?.Invoke(furniture);
            if (onFurniturePurchased != null)
                onFurniturePurchased.Invoke(furniture);

            RefreshView();
            return true;
        }

        private void OnPhaseChanged(GamePhase phase)
        {
            if (phase != GamePhase.Preparation)
                Close();

            RefreshView();
        }

        private void BuildCatalogButtons()
        {
            if (itemListRoot == null || catalog == null) return;
            _itemButtons.Clear();
            _catalogItems.Clear();
            _previews.Clear();
            _prices.Clear();
            foreach (FurnitureData furniture in catalog)
            {
                if (furniture == null) continue;
                var card = Surface("Kart_" + furniture.ItemId, itemListRoot, new Vector2(284, 310), Vector2.zero, _cardSurface);
                var picture = new GameObject("MobilyaGorseli", typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
                picture.transform.SetParent(card.transform, false);
                TopRect(picture.rectTransform, new Vector2(250, 160), new Vector2(0, -12));
                picture.raycastTarget = false;
                _previews.Add(picture);
                Label("Ad", card.transform, furniture.DisplayName, 28, new Vector2(258, 38), new Vector2(0, -174), true);
                Label("Bilgi", card.transform, "Kütle: " + furniture.Mass + "  •  Sabitleme: " + furniture.AnchoringCost + " para",
                    17, new Vector2(256, 44), new Vector2(0, -213), false);
                Button buy = StyledButton("SatinAl", card.transform, new Vector2(256, 48), new Vector2(0, -253), _buySurface);
                Text price = Label("Fiyat", buy.transform, "", 25, new Vector2(246, 46), Vector2.zero, true);
                _prices.Add(price);
                FurnitureData selected = furniture;
                buy.onClick.AddListener(() => TryPurchase(selected));
                _itemButtons.Add(buy);
                _catalogItems.Add(furniture);
            }
        }

        private void CreateDefaultUiIfNeeded()
        {
            if (!autoCreateUi) return;
            EnsureEventSystem();
            _cardSurface = MakeSurface(284, 310, 22, stone, true);
            _buySurface = MakeSurface(256, 48, 12, purchaseGreen, true);
            _backSurface = MakeSurface(360, 72, 17, stone, true);
            Canvas canvas = new GameObject("Level2ShopCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
            _generatedCanvas = canvas.gameObject;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // The opener is behind the full-screen shop, so it cannot cover its controls.
            Button opener = StyledButton("MagazayiAc", canvas.transform, new Vector2(220, 60),
                Vector2.zero, _backSurface);
            RectTransform openerRect = (RectTransform)opener.transform;
            openerRect.anchorMin = openerRect.anchorMax = new Vector2(1, 0);
            openerRect.pivot = new Vector2(1, 0);
            openerRect.anchoredPosition = new Vector2(-24, 24);
            Label("Yazi", opener.transform, "MAĞAZA", 26, new Vector2(210, 58), Vector2.zero, true);
            opener.onClick.AddListener(Open);

            Image backdrop = new GameObject("Level2ShopPanel", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            backdrop.transform.SetParent(canvas.transform, false);
            backdrop.rectTransform.anchorMin = Vector2.zero;
            backdrop.rectTransform.anchorMax = Vector2.one;
            backdrop.rectTransform.offsetMin = backdrop.rectTransform.offsetMax = Vector2.zero;
            backdrop.sprite = MakeBackdrop();
            shopContent = backdrop.gameObject;
            var content = new GameObject("MagazaIcerik", typeof(RectTransform)).GetComponent<RectTransform>();
            content.SetParent(backdrop.transform, false);
            content.anchorMin = content.anchorMax = content.pivot = new Vector2(0.5f, 0.5f);
            content.sizeDelta = new Vector2(960, 1020);
            Label("OyunAdi", content, "SON DENGE", 70, new Vector2(900, 94), new Vector2(0, -10), true);
            Label("Baslik", content, "SİVRİTEPE MOBİLYACISI", 24, new Vector2(900, 38), new Vector2(0, -100), false);
            budgetText = Label("Butce", content, "", 24, new Vector2(900, 40), new Vector2(0, -143), true);

            var grid = new GameObject("EsyaListesi", typeof(RectTransform), typeof(GridLayoutGroup)).GetComponent<GridLayoutGroup>();
            grid.transform.SetParent(content, false);
            TopRect((RectTransform)grid.transform, new Vector2(892, 640), new Vector2(0, -200));
            grid.cellSize = new Vector2(284, 310);
            grid.spacing = new Vector2(20, 20);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            itemListRoot = grid.transform;
            feedbackText = Label("Bilgi", content, "Yeni eşya alabilirsin — ama her eşya dengeyi etkiler!",
                20, new Vector2(900, 48), new Vector2(0, -853), false);
            closeButton = StyledButton("MagazayiKapat", content, new Vector2(360, 72),
                new Vector2(0, -920), _backSurface);
            Label("Yazi", closeButton.transform, "GERİ", 44, new Vector2(340, 70), Vector2.zero, true);
        }

        private static void TopRect(RectTransform rect, Vector2 size, Vector2 position)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private Image Surface(string name, Transform parent, Vector2 size, Vector2 position, Sprite sprite)
        {
            Image image = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            image.transform.SetParent(parent, false);
            TopRect(image.rectTransform, size, position);
            image.sprite = sprite;
            return image;
        }

        private Button StyledButton(string name, Transform parent, Vector2 size, Vector2 position, Sprite sprite)
        {
            Image image = Surface(name, parent, size, position, sprite);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.20f, 1.17f, 1.08f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color(0.72f, 0.72f, 0.65f);
            colors.disabledColor = new Color(0.42f, 0.42f, 0.42f);
            colors.fadeDuration = 0.12f;
            button.colors = colors;
            return button;
        }

        private Text Label(string name, Transform parent, string value, int size, Vector2 bounds, Vector2 position, bool bold)
        {
            Text text = new GameObject(name, typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            text.transform.SetParent(parent, false);
            TopRect(text.rectTransform, bounds, position);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = size;
            text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            text.color = parchment;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            if (bold)
            {
                Outline outline = text.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(0.025f, 0.02f, 0.015f, 0.95f);
                outline.effectDistance = new Vector2(1.5f, -1.5f);
            }
            return text;
        }

        // Generated UI artwork: softly rounded copper edge, worn stone and fine grain.
        private Sprite MakeSurface(int width, int height, float radius, Color fill, bool border)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                Vector2 q = new Vector2(Mathf.Abs(x - (width - 1) * 0.5f) - (width * 0.5f - radius),
                    Mathf.Abs(y - (height - 1) * 0.5f) - (height * 0.5f - radius));
                float d = new Vector2(Mathf.Max(q.x, 0), Mathf.Max(q.y, 0)).magnitude
                    + Mathf.Min(Mathf.Max(q.x, q.y), 0) - radius;
                float noise = Mathf.PerlinNoise(x * 0.12f, y * 0.12f) - 0.5f;
                float grain = ((x * 73 + y * 179 + x * y * 3) % 101) / 100f - 0.5f;
                Color c = fill * (0.88f + noise * 0.34f + grain * 0.12f + 0.18f * y / height);
                if (border && d > -6) c = copper * (0.93f + 0.16f * y / height + noise * 0.12f);
                else if (border && d > -9) c *= 0.42f;
                c.a = Mathf.Clamp01(-d);
                pixels[y * width + x] = c;
            }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            texture.wrapMode = TextureWrapMode.Clamp;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.one * 0.5f, 100);
            _visualResources.Add(sprite);
            _visualResources.Add(texture);
            return sprite;
        }

        private Sprite MakeBackdrop()
        {
            const int w = 512, h = 288;
            Texture2D texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[w * h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float radius = new Vector2((x - w * 0.5f) / w, (y - h * 0.5f) / h).magnitude;
                float glow = Mathf.Clamp01(1 - radius * 1.7f);
                float cloud = Mathf.PerlinNoise(x * 0.022f, y * 0.022f);
                pixels[y * w + x] = new Color(0.009f + glow * 0.016f, 0.021f + glow * 0.048f + cloud * 0.008f, 0.008f, 1);
            }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, w, h), Vector2.one * 0.5f);
            _visualResources.Add(sprite);
            _visualResources.Add(texture);
            return sprite;
        }

        private IEnumerator Start()
        {
            // Wait for the render pipeline to initialize before requesting thumbnails.
            yield return null;
            for (int i = 0; i < _previews.Count; i++)
            {
                if (_catalogItems[i].ModelPrefab != null)
                    _previews[i].texture = RenderFurniture(_catalogItems[i].ModelPrefab);
                yield return null;
            }
        }

        private Texture RenderFurniture(GameObject prefab)
        {
            GameObject studio = new GameObject("L2_GeciciOnizleme");
            studio.transform.position = new Vector3(10000, 10000, 10000);
            RenderTexture target = new RenderTexture(previewResolution, previewResolution, 24, RenderTextureFormat.ARGB32);
            target.Create();
            try
            {
                // Copy only mesh data: prefab scripts, physics and events must never run here.
                foreach (MeshFilter source in prefab.GetComponentsInChildren<MeshFilter>(true))
                {
                    MeshRenderer sourceRenderer = source.GetComponent<MeshRenderer>();
                    if (source.sharedMesh == null || sourceRenderer == null) continue;
                    GameObject mesh = new GameObject("Gorsel", typeof(MeshFilter), typeof(MeshRenderer));
                    mesh.transform.SetParent(studio.transform, false);
                    Matrix4x4 local = prefab.transform.worldToLocalMatrix * source.transform.localToWorldMatrix;
                    mesh.transform.localPosition = local.GetColumn(3);
                    mesh.transform.localRotation = local.rotation;
                    mesh.transform.localScale = local.lossyScale;
                    mesh.GetComponent<MeshFilter>().sharedMesh = source.sharedMesh;
                    mesh.GetComponent<MeshRenderer>().sharedMaterials = sourceRenderer.sharedMaterials;
                }
                Renderer[] renderers = studio.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0) return null;
                Bounds bounds = renderers[0].bounds;
                foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
                Camera camera = new GameObject("OnizlemeKamerasi", typeof(Camera)).GetComponent<Camera>();
                camera.transform.SetParent(studio.transform, false);
                camera.enabled = false;
                camera.orthographic = true;
                camera.orthographicSize = Mathf.Max(0.1f, bounds.extents.magnitude * 1.08f);
                float distance = Mathf.Max(5, bounds.size.magnitude * 3);
                camera.transform.position = bounds.center + new Vector3(1, 0.65f, -1.5f).normalized * distance;
                camera.transform.LookAt(bounds.center);
                camera.nearClipPlane = 0.01f;
                camera.farClipPlane = distance * 2;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.clear;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                var request = new RenderPipeline.StandardRequest { destination = target };
                RenderPipeline.SubmitRenderRequest(camera, request);
                _visualResources.Add(target);
                return target;
            }
            finally
            {
                studio.SetActive(false);
                Destroy(studio);
                if (!_visualResources.Contains(target)) { target.Release(); Destroy(target); }
            }
        }

        private void OnDestroy()
        {
            if (_generatedCanvas != null) Destroy(_generatedCanvas);
            foreach (UnityEngine.Object resource in _visualResources)
            {
                if (resource is RenderTexture renderTexture) renderTexture.Release();
                if (resource != null) Destroy(resource);
            }
            _visualResources.Clear();
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;

            GameObject eventSystem = new GameObject("Level2EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.transform.SetParent(null);
        }

        private void RefreshView()
        {
            if (budgetText != null)
                budgetText.text = "BÜTÇE: " + _budget + " PARA";

            bool canBuy = IsPreparationPhase();
            for (int i = 0; i < _itemButtons.Count && i < _catalogItems.Count; i++)
            {
                FurnitureData furniture = _catalogItems[i];
                bool affordable = furniture.PurchasePrice <= _budget;
                _itemButtons[i].interactable = _purchasesEnabled && canBuy && affordable;
                if (i < _prices.Count)
                    _prices[i].text = affordable ? "SATIN AL  " + furniture.PurchasePrice : "YETERSİZ BÜTÇE";
            }
        }

        private void SetFeedback(string message)
        {
            if (feedbackText != null)
                feedbackText.text = message;
        }

        private static bool IsPreparationPhase()
        {
            return GameManager.Instance == null || GameManager.Instance.Phase == GamePhase.Preparation;
        }
    }
}
