using System.Linq;
using UnityEngine;

namespace Jam.Levels.Level1
{
    public sealed partial class L1_ProjectGameController
    {
        private static readonly Color Stone = L1_PaperGui.Parchment;
        private static readonly Color Deep = L1_PaperGui.Slip;
        private static readonly Color Border = L1_PaperGui.Edge;
        private static readonly Color Amber = new Color32(156, 88, 33, 255);
        private static readonly Color Cream = L1_PaperGui.Ink;
        private static readonly Color Muted = L1_PaperGui.MutedInk;
        private static readonly Color Green = new Color32(62, 107, 73, 255);
        private static readonly Color Red = new Color32(158, 58, 47, 255);
        private GUIStyle uiText, uiSmall, uiTiny, uiTitle, uiButton, uiCentered;
        private Font paperFont;
        private Texture2D uiTransparent;

        private void EnsureUiStyles()
        {
            if (uiText != null) return;
            paperFont = Font.CreateDynamicFontFromOSFont(new[] { "Georgia", "Times New Roman", "Arial" }, 16);
            uiText = new GUIStyle(GUI.skin.label) { font = paperFont, fontSize = 16, wordWrap = true, richText = false };
            uiText.normal.textColor = Cream;
            uiSmall = new GUIStyle(uiText) { fontSize = 13 };
            uiSmall.normal.textColor = Muted;
            uiTiny = new GUIStyle(uiSmall) { fontSize = 11 };
            uiTitle = new GUIStyle(uiText) { fontSize = 27, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            uiTitle.normal.textColor = Amber;
            uiCentered = new GUIStyle(uiText) { alignment = TextAnchor.MiddleCenter };
            uiTransparent = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            uiTransparent.SetPixel(0, 0, Color.clear);
            uiTransparent.Apply();
            uiButton = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
                font = paperFont, wordWrap = true, padding = new RectOffset(7, 7, 3, 3)
            };
            uiButton.normal.background = uiTransparent;
            uiButton.hover.background = uiTransparent;
            uiButton.active.background = uiTransparent;
            uiButton.focused.background = uiTransparent;
            uiButton.normal.textColor = Cream;
            uiButton.hover.textColor = Amber;
            uiButton.active.textColor = Amber;
        }

        private static void Fill(Rect rect, Color color)
        {
            L1_PaperGui.Fill(rect, color);
        }

        private static void Panel(Rect rect, Color fill, Color border)
        {
            L1_PaperGui.Panel(rect, fill, border);
        }

        private bool ActionButton(Rect rect, string caption, Color color)
        {
            var hover = rect.Contains(Event.current.mousePosition);
            var tint = Color.Lerp(L1_PaperGui.Parchment, color, hover ? 0.08f : 0.19f);
            Panel(rect, tint, color);
            return GUI.Button(rect, caption, uiButton);
        }

        private void OnGUI()
        {
            EnsureUiStyles();
            var previousMatrix = GUI.matrix;
            var scale = Mathf.Max(0.55f, Mathf.Min(Screen.width / 1376f, Screen.height / 768f));
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            var width = Screen.width / scale;
            var height = Screen.height / scale;

            if (state == RoundState.Disaster && level >= 4) DrawDisasterWeather(width, height);

            if (state != RoundState.StartMenu && useLegacyHud)
            {
                DrawTopBar(width);
                if (warningUntil > 0f)
                {
                    Panel(new Rect(0f, 66f, width, 37f), new Color32(255, 229, 209, 255), Red);
                    GUI.Label(new Rect(24f, 71f, width - 48f, 27f), "⚠  " + warning, uiText);
                }
                if (state == RoundState.Planning) DrawForecast(width);
                if (state == RoundState.Disaster) DrawDisasterBanner(width);
                if (state == RoundState.Planning && !marketOpen && !menuOpen && !howToOpen) DrawSelectedBar(width, height);
            }

            // Yeni uGUI HUD (L1_Hud) mağaza, rapor, duraklatma ve nasıl oynanır panellerini çizer.
            // Eski IMGUI panelleri yalnız useLegacyHud açıkken (geliştirme modu) görünür.
            if (useLegacyHud)
            {
                if (marketOpen && state == RoundState.Planning && !menuOpen && !howToOpen) DrawMarket(width, height);
                var earthquakeOnly = singleFloorLevel && level == 1;
                if (state == RoundState.Report) { if (earthquakeOnly) DrawEarthquakeReport(width, height, true); else DrawReport(width, height); }
                if (state == RoundState.GameOver) { if (earthquakeOnly) DrawEarthquakeReport(width, height, false); else DrawGameOver(width, height); }
                if (state == RoundState.Victory) DrawVictory(width, height);
                if (menuOpen && !howToOpen) DrawPauseMenu(width, height);
                if (howToOpen) DrawHowTo(width, height);
            }
            if (state == RoundState.StartMenu && !howToOpen && !autoStart) DrawStartMenu(width, height);

            GUI.matrix = previousMatrix;
            if (state == RoundState.Planning || state == RoundState.Disaster) L1_TouchControls.Draw();
            if (drag != null) drag.canDrag = state == RoundState.Planning && (!marketOpen || !useLegacyHud) &&
                !menuOpen && !howToOpen && !countdownActive && !growing;
        }

        private void DrawTopBar(float width)
        {
            Panel(new Rect(0f, 0f, width, 64f), Stone, Border);
            var room = level == 1 ? "Oturma Odası (Deprem)" : level == 2 ? "Mutfak (Fırtına)" :
                level == 3 ? "Yatak Odası (Yangın)" : level == 4 ? "Zirve (Yıldırım)" : "Zirve (Yağmur)";
            Panel(new Rect(12f, 12f, 262f, 39f), Deep, Border);
            GUI.Label(new Rect(20f, 17f, 250f, 29f), "Seviye " + level + "/5: " + room, uiSmall);
            Panel(new Rect(282f, 12f, 115f, 39f), Deep, Border);
            GUI.Label(new Rect(292f, 17f, 105f, 29f), budget + " altın", uiText);
            Panel(new Rect(405f, 12f, 112f, 39f), Deep, Border);
            GUI.Label(new Rect(416f, 17f, 100f, 29f), score + " puan", uiSmall);

            if (level <= 2)
            {
                Panel(new Rect(528f, 12f, 250f, 39f), Deep, Border);
                GUI.Label(new Rect(537f, 17f, 85f, 26f), "Su terazisi", uiSmall);
                Fill(new Rect(623f, 26f, 90f, 10f), new Color32(68, 64, 60, 255));
                Fill(new Rect(657f, 26f, 22f, 10f), new Color32(18, 100, 72, 255));
                // Positive gauge means the right side is lower.
                var angle = -houseAngle * Mathf.Rad2Deg;
                Fill(new Rect(662f + Mathf.Clamp(angle * 1.5f, -38f, 38f), 22f, 12f, 18f),
                    Mathf.Abs(angle) < 10f ? Green : Mathf.Abs(angle) < 20f ? Amber : Red);
                GUI.Label(new Rect(718f, 18f, 58f, 25f), angle.ToString("F1") + "°", uiSmall);
            }
            else if (level == 4) GUI.Label(new Rect(538f, 18f, 275f, 30f),
                equipment.Contains("lightning_rod_system") ? "Paratoner koruyor ✓" : "Paratoner yok: darbe riski", uiSmall);
            else if (level == 5) GUI.Label(new Rect(538f, 18f, 275f, 30f), "Yüzey suyu: " + Mathf.RoundToInt(water) + " L", uiSmall);

            var right = width - 290f;
            if (state == RoundState.Planning)
            {
                if (ActionButton(new Rect(right, 13f, 118f, 37f), "Ev Marketi", Border)) marketOpen = true;
                if (ActionButton(new Rect(right + 125f, 13f, 118f, 37f), "Afeti Başlat", Red)) StartDisaster();
            }
            else if (state == RoundState.Disaster)
                GUI.Label(new Rect(right - 20f, 19f, 260f, 28f), "Kalan süre: " + Mathf.CeilToInt(timer) + " sn", uiText);
            if (ActionButton(new Rect(width - 41f, 13f, 30f, 37f), "☰", Border)) ToggleMenu();
        }

        private void DrawForecast(float width)
        {
            var rect = new Rect(Mathf.Max(12f, (width - 1010f) * 0.5f), 105f, Mathf.Min(1010f, width - 24f), 100f);
            Panel(rect, Stone, Border);
            GUI.Label(new Rect(rect.x + 18f, rect.y + 10f, 180f, 24f), "⚠  AFET RAPORU " + level + "/5", uiSmall);
            var heading = level == 1 ? "SİSMİK UYARI: Deprem sarsıntısı bekleniyor" :
                level == 2 ? "METEOROLOJİ UYARISI: Dağ fırtınası yaklaşıyor" :
                level == 3 ? "YANGIN TEHLİKESİ: Aşırı ısınma ve kıvılcım riski" :
                level == 4 ? "YILDIRIM TEHLİKESİ: Zirvede fırtına yaklaşıyor" :
                "ŞİDDETLİ YAĞMUR: Dağ suyunun yönünü kontrol et";
            GUI.Label(new Rect(rect.x + 20f, rect.y + 34f, rect.width - 235f, 30f), heading, uiText);
            var hint = level == 1 ? "Deprem çantası edin, eşyaları duvara çivile ve evi dengele." :
                level == 2 ? "Ağır mobilyaları rüzgârın ters yönüne taşı; çatıyı güçlendir." :
                level == 3 ? "Üçüncü kat açıldı. Yangın tüpü edin ve alevleri söndür." :
                level == 4 ? "Paratoneri ve gerilim koruyucuyu kur." :
                "Oluk, drenaj, kum torbaları ve pompayla suyu uzaklaştır.";
            GUI.Label(new Rect(rect.x + 20f, rect.y + 65f, rect.width - 225f, 25f), hint, uiSmall);
            GUI.Label(new Rect(rect.xMax - 205f, rect.y + 24f, 185f, 25f),
                "Hazırlık: " + Mathf.CeilToInt(timer) + " sn", uiText);
            Fill(new Rect(rect.xMax - 205f, rect.y + 58f, 178f, 8f), new Color32(60, 55, 50, 255));
            Fill(new Rect(rect.xMax - 205f, rect.y + 58f, 178f * Mathf.Clamp01(timer / 45f), 8f), Amber);
        }

        private void DrawDisasterBanner(float width)
        {
            var detail = level == 3 ? "Yangın: " + fires.Count + " alev  |  " + fireCause + "  |  Yaklaş ve E ile köpük sık" :
                level == 4 ? "Bina sağlığı: %" + Mathf.RoundToInt(buildingHealth) :
                level == 5 ? "Yüzey suyu: " + Mathf.RoundToInt(water) + " L  |  İçeri sızma: %" + Mathf.RoundToInt(indoorFlood) :
                level == 2 ? "Fırtına sürüyor: evi dengede tut" : "Deprem sürüyor: çivilenmeyen eşyalar kayabilir";
            Panel(new Rect((width - 740f) * 0.5f, 108f, 740f, 45f), new Color32(255, 228, 209, 255), Red);
            GUI.Label(new Rect((width - 718f) * 0.5f, 117f, 718f, 28f), detail, uiText);
        }

        private void DrawDisasterWeather(float width, float height)
        {
            var rainColor = level == 4 ? new Color(0.62f, 0.76f, 0.97f, 0.30f) :
                new Color(0.43f, 0.78f, 0.95f, 0.50f);
            for (var i = 0; i < 75; i++)
            {
                var x = Mathf.Repeat(i * 97.31f + Time.time * (level == 4 ? 30f : 53f), width);
                var y = Mathf.Repeat(i * 71.73f + Time.time * (level == 4 ? 270f : 390f), height);
                Fill(new Rect(x, y, 2f, level == 4 ? 10f : 17f), rainColor);
            }
            if (level == 5 && water > 0f)
                Fill(new Rect(0f, height - Mathf.Min(70f, water * 0.35f), width,
                    Mathf.Min(70f, water * 0.35f)), new Color(0.15f, 0.55f, 0.72f, 0.27f));
            if (lightningFlash > 0f)
                Fill(new Rect(0f, 0f, width, height), new Color(0.85f, 0.93f, 1f, lightningFlash * 0.38f));
        }

        private void DrawSelectedBar(float width, float height)
        {
            if (drag == null || drag.Selected == null) return;
            var selected = drag.Selected;
            var rect = new Rect((width - 560f) * 0.5f, height - 68f, 560f, 54f);
            Panel(rect, Stone, Border);
            GUI.Label(new Rect(rect.x + 12f, rect.y + 9f, 182f, 36f), selected.displayName + "\n" + selected.Mass + " kg", uiSmall);
            if (drag.Carried == selected)
            {
                if (ActionButton(new Rect(rect.x + 205f, rect.y + 9f, 198f, 36f), "Yere Bırak (E)", Border))
                    drag.DropCarried();
            }
            else if (ActionButton(new Rect(rect.x + 205f, rect.y + 9f, 198f, 36f),
                selected.IsAnchored ? "Çiviyi Çıkar" : "Duvara Çivile (30 altın)", Border)) ToggleAnchor();
            if (ActionButton(new Rect(rect.x + 411f, rect.y + 9f, 135f, 36f), "Sat / Kaldır", Red)) SellSelected();
        }

        private void DrawMarket(float width, float height)
        {
            Fill(new Rect(0f, 0f, width, height), new Color(0f, 0f, 0f, 0.63f));
            var panel = new Rect(width - 455f, 0f, 455f, height);
            Panel(panel, Stone, Border);
            Panel(new Rect(panel.x + 8f, 9f, panel.width - 16f, 75f), Deep, Border);
            GUI.Label(new Rect(panel.x + 34f, 15f, 254f, 34f), "EV MARKETİ", uiTitle);
            L1_PaperGui.Rule(panel.x + 36f, 51f, 255f, Border);
            GUI.Label(new Rect(panel.x + 38f, 55f, 299f, 23f), "Erzak ve tamir defteri  •  No. " + level, uiSmall);
            GUI.Label(new Rect(panel.x + 324f, 45f, 86f, 27f), budget + " altın", uiSmall);
            if (ActionButton(new Rect(panel.x + 410f, 19f, 29f, 31f), "X", Red)) marketOpen = false;

            // Level 1 yalnız deprem bölümü: mobilya ve deprem önlemleri gösterilir.
            var categoryCount = singleFloorLevel ? 2 : Categories.Length;
            for (var i = 0; i < categoryCount; i++)
            {
                var x = panel.x + 13f + (i % 3) * 145f;
                var y = 93f + (i / 3) * 36f;
                if (ActionButton(new Rect(x, y, 138f, 31f), Categories[i], i == categoryIndex ? Amber : Muted))
                { categoryIndex = i; marketScroll = Vector2.zero; }
            }

            var items = Catalog.Where(item => item.category == Categories[categoryIndex]).ToArray();
            var viewport = new Rect(panel.x + 11f, 173f, panel.width - 20f, height - 229f);
            var content = new Rect(0f, 0f, viewport.width - 19f, Mathf.Max(viewport.height, items.Length * 122f));
            marketScroll = GUI.BeginScrollView(viewport, marketScroll, content);
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                var row = new Rect(1f, i * 122f + 1f, content.width - 4f, 113f);
                var locked = level < item.unlockLevel;
                var installed = item.category != "Mobilya" && item.id != "sabitleme_aparati" && equipment.Contains(item.id);
                Panel(row, installed ? new Color32(230, 247, 218, 255) : Deep,
                    installed ? Green : locked ? Muted : Border);
                var icon = Icon(item.id);
                if (icon != null)
                {
                    var texture = icon.texture;
                    var rect = icon.textureRect;
                    GUI.DrawTextureWithTexCoords(new Rect(row.x + 12f, row.y + 40f, 62f, 62f), texture,
                        new Rect(rect.x / texture.width, rect.y / texture.height,
                            rect.width / texture.width, rect.height / texture.height));
                }
                GUI.Label(new Rect(row.x + 14f, row.y + 8f, 27f, 25f), (i + 1).ToString("00"), uiSmall);
                GUI.Label(new Rect(row.x + 43f, row.y + 8f, 256f, 25f), item.title, uiText);
                GUI.Label(new Rect(row.x + 319f, row.y + 9f, 81f, 22f), item.mass + " kg", uiSmall);
                L1_PaperGui.Rule(row.x + 13f, row.y + 34f, row.width - 26f, new Color(0.46f, 0.28f, 0.15f, 0.5f));
                GUI.Label(new Rect(row.x + 83f, row.y + 41f, row.width - 95f, 32f), Description(item), uiSmall);
                GUI.Label(new Rect(row.x + 83f, row.y + 75f, 170f, 29f), item.cost + " altın" +
                    (item.category == "Mobilya" ? "  ·  +" + FurnitureIncome(item) : ""), uiSmall);
                var buttonRect = new Rect(row.xMax - 130f, row.y + 77f, 118f, 29f);
                if (locked) GUI.Label(buttonRect, item.unlockLevel + ". seviyede açılır", uiTiny);
                else if (installed) GUI.Label(buttonRect, "KURULDU ✓", uiSmall);
                else if (budget < item.cost) GUI.Label(buttonRect, "YETERSİZ ALTIN", uiTiny);
                else if (ActionButton(buttonRect, item.category == "Mobilya" || item.id == "deprem_cantasi" ? "SATIN AL" : "KUR", Border)) Purchase(item);
            }
            GUI.EndScrollView();
            Panel(new Rect(panel.x + 8f, height - 49f, panel.width - 16f, 43f), Deep, Border);
            GUI.Label(new Rect(panel.x + 13f, height - 36f, 285f, 28f), "Çivileme aparatı stoğu: " + anchors, uiSmall);
            if (ActionButton(new Rect(panel.x + 345f, height - 39f, 94f, 29f), "Tamam", Border)) marketOpen = false;
        }

        private static string Description(ShopItem item)
        {
            switch (item.id)
            {
                case "green_sofa": return "Yıpranmış, rahat ikili kanepe.";
                case "red_armchair": return "Tekli deri berjer; denge için taşınabilir.";
                case "coffee_table": return "Rustik ahşap orta sehpa.";
                case "bookshelf": return "Ağır kitaplık; çivilemek iyi fikir.";
                case "refrigerator": return "Ağır retro buzdolabı; dengenin yönünü değiştirir.";
                case "stove_counter": return "Ocak ve fırın; olası yangın kaynağı.";
                case "dining_table": return "Mutfak için yemek masası. Geçici 2D eşya.";
                case "wooden_barrel": return "Erzak fıçısı. Geçici 2D eşya.";
                case "cozy_bed": return "Üst kattaki gerçek 3D yatak modeli.";
                case "wardrobe": return "Ağır gardırop. Geçici 2D eşya.";
                case "bedside_table": return "Ahşap başucu komodini.";
                case "standing_lamp": return "Sıcak ışıklı lambader. Geçici 2D eşya.";
                case "deprem_cantasi": return "İlk yardım ve erzak çantası; sürüklenebilir.";
                case "sabitleme_aparati": return "Seçili mobilyayı duvara çiviler; kaymayı önler.";
                case "storm_brace": return "Şiddetli rüzgârın ev üzerindeki etkisini azaltır.";
                case "yangin_tupu": return "Alevlere yaklaşarak E ile köpük sıkın.";
                case "duman_dedektoru": return "Yangını erken haber verir; raporda görünür.";
                case "lightning_rod_system": return "Yıldırımı çatıdan kayaya güvenle iletir.";
                case "surge_protector": return "Elektrik panosunu gerilim sıçramasından korur.";
                case "gutter_system": return "Çatı suyunu kontrollü tahliye eder.";
                case "drainage_channel": return "Dağ yüzeyindeki akışı evden uzaklaştırır.";
                case "waterproof_sealing": return "Kapı ve duvardaki su sızmasını yavaşlatır.";
                case "sandbags": return "Suya karşı giriş barikatı oluşturur.";
                case "water_pump": return "Birikmiş suyu motor gücüyle tahliye eder.";
                case "water_sensor": return "Su yükseldiğinde erken uyarı verir.";
                default: return item.title;
            }
        }

        private static void Shade(float width, float height)
        {
            Fill(new Rect(0f, 0f, width, height), new Color(0.02f, 0.02f, 0.02f, 0.78f));
        }

        private void DrawStartMenu(float width, float height)
        {
            Shade(width, height);
            var rect = new Rect((width - 500f) * 0.5f, (height - 395f) * 0.5f, 500f, 395f);
            Panel(rect, Stone, Border);
            GUI.Label(new Rect(rect.x + 25f, rect.y + 42f, 450f, 54f), "SON DENGE", uiTitle);
            L1_PaperGui.Rule(rect.x + 89f, rect.y + 99f, 322f, Border);
            GUI.Label(new Rect(rect.x + 36f, rect.y + 105f, 428f, 34f), "Dengeyi Kur, 5 Büyük Afete Karşı Diren", uiCentered);
            GUI.Label(new Rect(rect.x + 40f, rect.y + 159f, 420f, 85f),
                "Deprem, dağ fırtınası, yangın, yıldırım ve şiddetli yağmura karşı 3D dağ evini koru.", uiCentered);
            if (ActionButton(new Rect(rect.x + 55f, rect.y + 268f, 390f, 48f), "OYUNA BAŞLA", Amber)) BeginGame();
            if (ActionButton(new Rect(rect.x + 55f, rect.y + 328f, 390f, 38f), "Nasıl Oynanır?", Border))
            { howToOpen = true; SyncPause(); }
        }

        private void DrawPauseMenu(float width, float height)
        {
            Shade(width, height);
            var rect = new Rect((width - 390f) * 0.5f, (height - 390f) * 0.5f, 390f, 390f);
            Panel(rect, Stone, Border);
            GUI.Label(new Rect(rect.x + 15f, rect.y + 22f, 360f, 45f), "OYUN MENÜSÜ", uiTitle);
            L1_PaperGui.Rule(rect.x + 58f, rect.y + 73f, 274f, Border);
            if (ActionButton(new Rect(rect.x + 37f, rect.y + 80f, 316f, 42f), "Devam Et", Amber)) ToggleMenu();
            if (ActionButton(new Rect(rect.x + 37f, rect.y + 136f, 316f, 38f),
                musicMuted ? "Müzik: Kapalı" : "Müzik: Açık", Border))
            { musicMuted = !musicMuted; SyncPause(); }
            if (ActionButton(new Rect(rect.x + 37f, rect.y + 187f, 316f, 38f), "Nasıl Oynanır?", Border))
            { howToOpen = true; SyncPause(); }
            if (ActionButton(new Rect(rect.x + 37f, rect.y + 238f, 316f, 38f), "Yeniden Başlat", Border)) Restart();
            if (ActionButton(new Rect(rect.x + 37f, rect.y + 289f, 316f, 38f), "Ana Menüye Dön", Red)) ReturnToMainMenu();
        }

        private void DrawHowTo(float width, float height)
        {
            Shade(width, height);
            var rect = new Rect((width - 580f) * 0.5f, (height - 465f) * 0.5f, 580f, 465f);
            Panel(rect, Stone, Border);
            GUI.Label(new Rect(rect.x + 25f, rect.y + 16f, 530f, 48f), "NASIL OYNANIR?", uiTitle);
            L1_PaperGui.Rule(rect.x + 72f, rect.y + 69f, 436f, Border);
            var instructions =
                "1. Deprem: Çanta edin, mobilyaları çivile, dengeyi kur.\n\n" +
                "2. Fırtına: Ağır eşyaları rüzgârın tersine yerleştir.\n\n" +
                "3. Yangın: Üçüncü kat açılır. Yangın tüpüyle alevleri söndür.\n\n" +
                "4. Yıldırım: Paratoner ve gerilim koruyucuyu kur.\n\n" +
                "5. Yağmur: Oluk, drenaj, kum torbası ve pompayla suyu uzaklaştır.\n\n" +
                "A/D: yürü  •  W/S: merdiven  •  Fare/dokunma: eşya taşı  •  F: çivile  •  E: köpük";
            GUI.Label(new Rect(rect.x + 39f, rect.y + 76f, 502f, 305f), instructions, uiText);
            if (ActionButton(new Rect(rect.x + 95f, rect.y + 398f, 390f, 42f), "Anladım, Oyuna Dön", Amber))
            { howToOpen = false; SyncPause(); }
        }

        private void DrawReport(float width, float height)
        {
            Shade(width, height);
            var rect = new Rect((width - 540f) * 0.5f, (height - 365f) * 0.5f, 540f, 365f);
            Panel(rect, Stone, Border);
            GUI.Label(new Rect(rect.x + 25f, rect.y + 20f, 490f, 40f), "SEVİYE " + level + " TAMAMLANDI", uiTitle);
            L1_PaperGui.Rule(rect.x + 74f, rect.y + 70f, 392f, Border);
            GUI.Label(new Rect(rect.x + 34f, rect.y + 78f, 472f, 52f), LevelNames[level] + " RAPORU", uiCentered);
            Panel(new Rect(rect.x + 28f, rect.y + 136f, 484f, 132f), Deep, Border);
            GUI.Label(new Rect(rect.x + 45f, rect.y + 148f, 452f, 110f), report, uiText);
            if (ActionButton(new Rect(rect.x + 50f, rect.y + 291f, 440f, 48f),
                level == 5 ? "BÜYÜK ZAFERİ GÖR" : "SONRAKİ SEVİYEYE GEÇ", Green)) Advance();
        }

        private void DrawGameOver(float width, float height)
        {
            Shade(width, height);
            var rect = new Rect((width - 500f) * 0.5f, (height - 300f) * 0.5f, 500f, 300f);
            Panel(rect, Stone, Red);
            GUI.Label(new Rect(rect.x + 25f, rect.y + 25f, 450f, 45f), "DENGE BOZULDU", uiTitle);
            L1_PaperGui.Rule(rect.x + 74f, rect.y + 82f, 352f, Red);
            GUI.Label(new Rect(rect.x + 35f, rect.y + 100f, 430f, 85f), report + "\nToplam skor: " + score, uiCentered);
            if (ActionButton(new Rect(rect.x + 75f, rect.y + 215f, 350f, 48f), "TEKRAR DENE", Red)) Restart();
        }

        private void DrawVictory(float width, float height)
        {
            Shade(width, height);
            var rect = new Rect((width - 555f) * 0.5f, (height - 355f) * 0.5f, 555f, 355f);
            Panel(rect, Stone, Green);
            GUI.Label(new Rect(rect.x + 25f, rect.y + 29f, 505f, 80f),
                "BİLGE AFET DAYANIKLILIK SERTİFİKASI", uiTitle);
            L1_PaperGui.Rule(rect.x + 82f, rect.y + 117f, 391f, Green);
            GUI.Label(new Rect(rect.x + 40f, rect.y + 140f, 475f, 105f),
                "✓ Deprem   ✓ Fırtına   ✓ Yangın\n✓ Yıldırım   ✓ Yağmur\n\nToplam başarı skoru: " + score, uiCentered);
            if (ActionButton(new Rect(rect.x + 88f, rect.y + 280f, 380f, 48f), "YENİDEN OYNA", Green)) Restart();
        }
    }
}
