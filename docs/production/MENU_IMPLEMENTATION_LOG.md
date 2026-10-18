# Son Denge — Menü Yeniden Tasarımı Uygulama Günlüğü

Şartname: `docs/design/SON_DENGE_MENU_CLAUDE_CODE_URETIM_PROMPTU.md`
Referans görseller (yalnız tasarım panosu, runtime'a girmez): `docs/design/menu_references/01..07*.png`
Ekran görüntüleri: `docs/production/screenshots/menu/`

---

### FAZ 0 — Repo denetimi
- Tarih: 18.10.2026 · branch: `feature/menu-redesign` (kaynak: `feature/wilson-missions-five-levels` @ `73b5c7a`)
- Unity: 6000.3.26f1, URP 3D, yeni Input System. Unity MCP: `mcp-for-unity-server 3.4.9` @ `http://localhost:8080/mcp`
  (yerel `.mcp.json` girişi kaldırılmış olduğundan bu oturumda HTTP JSON-RPC ile doğrudan çağrıldı; `.mcp.json`'a dokunulmadı).
- Uzak dallar: `main`, `kisi1-level1`, `kisi2-level2`, `kisi3-level3`, `kisi4-level4`, `feature/wilson-missions-five-levels`. Hiçbiri merge edilmedi.
- Kirli çalışma ağacı (başkasına/öncesine ait): `.mcp.json`, CloudSea/L1 materyalleri, `Assets/Settings/*`, `ProjectSettings.asset`,
  Yughues `.meta` silinmeleri, Medieval Room Kit yeni dosyaları. **Bunlar bu çalışmada stage edilmez/commit edilmez.**
- Sahiplik: Kullanıcı Lead (Kişi 1). `Menu.unity`, `Scripts/UI/`, `Scripts/Core/` (Contracts.cs hariç), `Assets/Prefabs/`, `Levels/Level1/` Lead'e ait → yetkili alan.

**Mevcut sistem haritası**
| Alan | Dosya | Not |
|---|---|---|
| Eski menü | `Scripts/UI/MainMenuUI.cs` (kodla kurulan legacy `Text` UI) | Yerine yeni `MenuFlowController` sistemi gelecek, eski sınıf sahneden kaldırılacak (dosya korunur) |
| UI yardımcı | `Scripts/UI/UiKit.cs`, `UiHoverScale` | Level HUD'ları kullanıyor → değiştirilmez |
| Ayarlar | `Scripts/UI/GameSettings.cs` (PlayerPrefs, ses + ReduceShake) | Genişletilecek (anahtar adları korunur) |
| İlerleme | `Scripts/Missions/LevelProgressSave.cs` (`sondenge.v1.*`, yıldız/kilit/brifing) | Tek ilerleme kaynağı olarak yeniden kullanılacak |
| Bölüm verisi | `Data/Levels/L1..L5*.asset` (`LevelDefinition`: sahne, afet süresi, görevler) + `Data/Missions/*` | Kart süresi ve görev listesi buradan okunur |
| Sahne geçişi | `Scripts/Core/SceneLoader.cs` (fade + LoadSceneAsync) | Gerçek ilerleme API'si eklenecek |
| Wilson | `Levels/Level1/Art/Characters/Wilson/low_poly_chicken.obj/.png` (Pjager, CC BY 4.0), `L1_WilsonChicken.BuildPortraitModel`, `Scripts/UI/WilsonPortrait.cs` (RenderTexture, MainCamera/AudioListener yok) | Menüde tek portre kamerası, tüm sayfalar aynı RT'yi paylaşır |
| Oyun içi duraklatma | `L1_ProjectGameController` ESC → `ToggleMenu`, UI: `L1_Hud.Panels.BuildPause` | FAZ 9'da aynı tasarım diliyle yenilenecek, tek ESC sahibi korunacak |
| Görseller | `Levels/Level1/Art/L1_MenuBackdrop.png` (oyunun ev+gün batımı render'ı, yazısız), `L1_MistyMountainValley.png`, `L1_DuskSky.png`, `L1_CozyPreview.png` (evin iç kesiti) | Yazısız "background-only" katmanlar olarak kullanılacak |
| Font | TMP `LiberationSans SDF` (Türkçe glif destekli) | Lisanslı ek font yok; yeni font indirilmedi |
| UI ses | Yok | Kısa ahşap tık sesleri editörde sentezlenecek (3. parti indirme yok) |

**Riskler / kararlar**
- Level2-5 sahneleri aynı `L1_ProjectGameController`'ı kullanır; menü ve duraklatma değişiklikleri tek koddan beş bölüme yansır.
- Referans PNG'ler arayüzü baked içerir → runtime'da kullanılmaz; tüm metin/buton gerçek TMP/Button.
- Parlaklık ayarı için projede gerçek bir uygulama yok → sahte slider eklenmeyecek.
- Durum: **PASS** (denetim) · Sonraki: FAZ 1

---

### FAZ 1 — Tasarım temeli
- Branch: `feature/menu-redesign`. Kod: `Assets/Editor/Menu/MenuArtGenerator.cs`, `MenuUiFactory.cs`, `MenuPrefabBuilder.cs`.
- Sahiplik notu: `Art/` (Kişi 4) ve `Audio/` (Kişi 2) yerine menü varlıkları Lead'e ait `Assets/Prefabs/UI/Menu/Art/` altında.
- Üretilenler: 23 adet 9-slice ahşap/parşömen/altın sprite, 50+ beyaz ikon (Image.color ile boyanır), 5 kısa ahşap UI sesi (editörde sentez, 3. parti indirme yok), dinamik TMP font `SonDenge_UI SDF` (LiberationSans.ttf, Türkçe glifler) + Title/OnWood/Ink materyal ön ayarları (tam `TextMeshPro/Distance Field`; Mobile shader underlay'i bozuk çiziyordu → düzeltildi).
- Prefablar (`Assets/Prefabs/UI/Menu`): UI_WoodButton_Primary/Secondary/Small, UI_WoodTabButton, UI_ParchmentPanel, UI_StarRating, UI_ChapterCard, UI_WilsonMessage, UI_WilsonPortrait, UI_TipCard, UI_SettingsRow_Slider/Toggle/Dropdown, UI_ConfirmDialog, UI_LoadingProgress, UI_BrandLogo, UI_TitleSign, UI_TaskRow, UI_HelpRow, UI_PauseMenu.
- Arka planlar: kullanıcının verdiği 7 yazısız görsel `Art/Backgrounds/bg_0X_*.png` (her ekran kendi arka planı; kart ve Hakkında küçük resimleri aynı görsellerden kırpılır). Referans PNG'ler runtime'a girmedi.
- Console kırmızı hata: 0 · **PASS**

### FAZ 2 — Menü navigasyonu
- `MenuFlowController` (tek aktif sayfa + geri yığını + ESC + onay), `MenuPage` (CanvasGroup fade/ölçek, unscaled, kapanan sayfa raycast'i hemen bırakır), `ConfirmDialog`, `MenuAudioBridge` (mevcut AudioManager), `WoodButtonFeedback` (hover/odak/basma, parıltı, ses).
- Test (Play, gerçek `Button.onClick`): Ana→Nasıl Oynanır→Geri, Ana→Ayarlar→Geri, Ana→Hakkında→Teşekkürler, Ana→Çıkış onayı: PASS. Aynı anda tek sayfa `IsOpen` doğrulandı (MenuQa.Current).
- Not: Bu oturumda Unity Editor Play modu ~10–20 sn sonra kendiliğinden kapanıyor (menü kodundan bağımsız; boşta bekleyen Play'de de oluyor). Testler kısa oturumlara bölündü. **PASS**

### FAZ 3 — Ana Menü (ref 01)
- Logo tabelası, 4 ahşap düğme (kademeli giriş), Oyun Hakkında bağlantısı, `v`+`Application.version` (0.1.0), Wilson 3D portre (tek RenderTexture, MainCamera/AudioListener yok), konuşma balonu, slogan levhası. Kayıt varsa "OYUNA DEVAM ET".
- Screenshot: `screenshots/menu/menu-01-ana-menu.png`, `menu-01b-cikis-onay.png` · **PASS**

### FAZ 4 — Bölüm Seçimi (ref 02)
- 5 kart: tema rengi, afet ikonu, yıldız `LevelProgressSave`'den, süre `LevelDefinition.disasterSeconds`'tan (12/12/30/25/25), kilit; ←/→ ve tıklama, çift tık/Devam Et; kilitli kartta CTA pasif + Wilson nedeni söyler.
- Test: kilitli Kart 3 seçilince Devam Et etkileşimsiz; Kart 1 → Brifing açıldı. Screenshot `menu-02-bolum-secimi.png` · **PASS**

### FAZ 5 — Wilson Görev Tanıtımı (ref 03)
- Görevler `LevelDefinition.tasks` → `TaskDefinition.titleTr` (Türkçe cümle düzeni), görev ikonları, UNUTMA kartı (depremde Çök–Kapan–Tutun şeridi, diğerlerinde afet ikonu), bölüme özel Wilson cümlesi.
- Zamanlayıcı: menü brifingi süre başlatmaz; HAZIRLIĞA BAŞLA → yükleme. Level1 açıldığında `timeScale=0` (bölüm içi kısa hazır akışı açık) — sayaç oyuncu başlatınca akar. Screenshot `menu-03-gorev-tanitimi.png` · **PASS**

### FAZ 6 — Gerçek yükleme (ref 04)
- `SceneLoader.LoadWithProgress`: `allowSceneActivation=false`, ilerleme 0–0.9 → %0–90, etkinleştirmede %100, min. 0.6 sn görünürlük, build'de olmayan sahne için hata + ANA MENÜ. Menü müziği etkinleştirmeden önce durur. 16 ipucu (8 oyun, 8 gerçek hayat).
- Test: Brifing→Hazırlığa Başla→Level1 (aktif sahne Level1, hata yok). Yükleme görseli önizlemeyle alındı (editörde yükleme çok hızlı): `menu-04-yukleme.png` · **PASS**

### FAZ 7 — Nasıl Oynanır? (ref 05)
- 6 sekme, iki sütun, ScrollRect, Tab/↑/↓ ile sekme; kontroller gerçek koddan: A/D, W/S (merdiven — görseldeki "W=Zıpla" kopyalanmadı), E, F, fare, BOŞLUK, tekerlek/Home, ESC. TAB görev listesi gerçekte olmadığı için yazılmadı. Puanlama 35/35/20/10, 70/90 eşikleri `MissionScoring`'den. Screenshot `menu-05-nasil-oynanir.png` · **PASS**

### FAZ 8 — Ayarlar (ref 06)
- Ses (ana/müzik/efekt + sessiz), Görüntü (yalnız `Screen.resolutions`, tam ekran, VSync, kalite `QualitySettings.names`), Oynanış (sarsıntı azalt, ilerlemeyi sıfırla onaylı), Dil (Türkçe varsayılan, sahte seçenek yok), Erişilebilirlik (hareketleri azalt). Parlaklık: projede gerçek uygulaması olmadığı için **eklenmedi**.
- Taslak/UYGULA modeli; uygulanmamış değişiklikle GERİ/ESC → "Kaydetmeden çıkılsın mı?"; çözünürlük değişikliği 10 sn içinde onaylanmazsa geri alınır; PlayerPrefs. Screenshot `menu-06-ayarlar.png` · **PASS** (tam ekran/çözünürlük değişimi yalnız Windows build'de anlamlı; editörde etkisi sınırlı)

### FAZ 9 — Hakkında + Duraklat
- Hakkında: açıklama, ev görseli, Ekibimiz (Yener Er · Lead; Emre Gündoğdu; Tülay Aktenli; Saygın Çamsoy — roller doğrulanmadığı için yazılmadı), Teşekkürler/krediler kaydırma, güvenlik dipnotu. `menu-07-oyun-hakkinda.png`, `menu-07b-tesekkurler.png`.
- Duraklat: `UI_PauseMenu` (DEVAM ET / YENİDEN BAŞLAT / AYARLAR / ANA MENÜ, onaylı), `L1_MissionDirector.pauseMenuPrefab` ile Level1–5'e bağlandı (her sahnede tek satır). Eski panel `ExternalPauseMenu` ile gizlenir.
- Çifte pause düzeltmesi: `GameManager.PauseInputEnabled=false` bölüm boyunca; ESC'nin tek sahibi `L1_ProjectGameController`; ayarlar/onay açıkken ESC alt paneli kapatır; Wilson brifingi açıkken ESC pause açmaz.
- Test (Level1 Play): pause → `timeScale=0`, Ayarlar açıldı, Devam Et → `timeScale=1`, `GameManager.IsPaused=false`. `menu-08-duraklat.png`, `menu-08b-duraklat-ayarlar.png` · **PASS**

### FAZ 10 — Entegrasyon ve görsel QA
- Menu → Bölüm Seçimi → Brifing → Yükleme → Level1 zinciri Play'de doğrulandı; Level1–5 build listesinde (6 sahne). Duraklat menüsü 5 bölüme bağlı.
- Referans karşılaştırması: kompozisyon, ahşap/parşömen paneller, altın CTA hiyerarşisi, 5 kart hizası, Wilson konumları yakın. Farklar: Wilson oyundaki gerçek low-poly tavuk modeli (referanstaki çizgi film tavuk değil), logo yazı tipi LiberationSans (lisanslı yuvarlak kalın font yok), Çök–Kapan–Tutun piktogramları basit çizim.
- **NOT_RUN:** 1600×900 / 1280×720 / 2560×1440 / 1920×1200 çözünürlük tek tek test edilmedi (Canvas Scale With Screen Size 1920×1080, match 0.5; ekran görüntüleri 2560×1440 Game görünümünden). Gerçek ESC tuş basışı Play'de simüle edilemedi; ESC yolları kod üzerinden ve düğmelerle test edildi.

### FAZ 11 — Build ve teslim
- Windows64 build: **Başarılı** · 65 sn · 536 MB · `C:/Users/yener/Desktop/game_jam 2026/Builds/SonDenge_Menu/SonDenge.exe` (depo dışı).
- Build konsolu: 1 hata = URP Lit shader'ının tek bir varyantında "out of memory during compilation" (d3d11; menüyle ilgisiz, build tamamlandı). 3 uyarı = `UI_DisasterReportTemplate.prefab` içinde eksik script (önceki commit'ten, bu çalışmada değiştirilmedi).
- Çalıştırma: exe 1280×720 pencerede 25 sn açık kaldı; `Player.log`'da hata/exception yok. Build içi görsel gezinme manuel kontrol gerektirir.
- Açık konular: Unity Editor Play modu bu oturumda kendiliğinden kapanıyordu (araştırılmalı); bölüm içi eski Wilson brifingi kısa hâliyle hâlâ açılıyor (menü brifingi sonrası "hazır mısın" adımı).
