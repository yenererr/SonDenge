# Level 1 + Menü — Uygulama Durumu

Kaynak plan: `docs/SON_DENGE_UNITY_MCP_LEVEL1_MENU_MASTER_PLAN.md` (Faz A–F). Branch: `kisi1-level1`.
Testler Unity 6000.3.26f1 Editor'de, Unity MCP üzerinden Play modunda yapıldı. Play kareleri, Editor
odakta değilken `EditorApplication.Step()` ile ilerletildi. Windows build **alınmadı**.

## Yapılanlar

| Faz | İş | Dosyalar |
|---|---|---|
| B | Tek katlı oturma odası; Kat 2 yalnız düz tavan döşemesi olarak kaldı, eğimli çatı / merdiven / balkon / üst kat eşyaları **silinmeden** gizlendi. 4 başlangıç eşyası (kanepe 60, kitaplık 85, sehpa 20, tekli koltuk 35 kg) alt kata ve torku ~0 olacak şekilde dizildi. Low-poly sivri kaya + denge noktası taşı. | `Scenes/Level1.unity`, `Meshes/L1_RockSpireMesh.asset` |
| C | Fareyle **yalnız yatay** sürükleme (yerel X), sağ tık iptal, karaktere yakınlık şartı kaldırıldı, UI üstünde tıklama dünyaya geçmez. Kamera: tekerlek FOV zoom 27–52°, orta fare sınırlı pan, Home/Sığdır, ayrık sarsıntı ofseti. **Tork hatası düzeltildi** (`Mathf.SmoothStep` yanlış kullanımı ağırlıkları ~10 kat ve ters işaretli yapıyordu). Eğim artık gerçek tork toplamından; hazırlıkta eşyalar eğimde kendiliğinden kaymaz. Çöküş: 26° + 1 sn kesintisiz. | `L1_PropDrag.cs`, `L1_CameraController.cs`, `L1_ProjectGameController.cs` |
| D | uGUI ana menü (OYNA / NASIL OYNANIR / AYARLAR / ÇIKIŞ / Atıflar, ESC ile kapanan paneller, ses ve tam ekran ayarları PlayerPrefs). Level1 uGUI HUD: kompakt denge göstergesi (GÜVENLİ/DENGESİZ/KRİTİK), bütçe, süre, seçili eşya, SABİTLE/SAT/MAĞAZA/AFETE HAZIRIM, zoom düğmeleri, 3-2-1 geri sayım, eğim riski onayı. Eski IMGUI başlangıç menüsü ve üst bar kapatıldı (`useLegacyHud`, `autoStart`). Mağazada Level1 için yalnız Mobilya + Deprem sekmeleri. | `Scripts/UI/UiKit.cs`, `MainMenuUI.cs`, `GameSettings.cs`, `L1_Hud.cs`, `L1_ProjectGameController.Api.cs`, `Scenes/Menu.unity` |
| E | Değişken deprem ivmesi (iki sinüs + Perlin gürültü + zarf eğrisi), kamera sarsıntısı (ayarlardan azaltılabilir), kayıp eşya takibi (bir kez), AFET RAPORU (ev durumu, sabitlenenler, korunan/kaybolan, en yüksek eğim, 0–100 hazırlık puanı, tek seferlik +450, gerçek hayat mesajı). TEKRAR DENE / SONRAKİ (YAKINDA) / ANA MENÜ; geçişler `SceneLoader` ile. Level1 artık fırtınaya vb. geçmiyor. | `L1_ProjectGameController.Quake.cs` |
| F | Raf seramiklerinin hata gibi görünen mor rengi düzeltildi; menü arka planı yeniden çekildi; bu belgeler. | `docs/` |

## Editor'de yapılan testler (sonuçlar)

| ID | Sonuç |
|---|---|
| T01–T03 | Menü açılıyor, Türkçe 4 düğme; NASIL OYNANIR paneli açılıp kapanıyor (kaydırılabilir). Ayarlar paneli derleniyor, **elle denenmedi**. |
| T02 | Menü → OYNA → Level1, faz `Preparation`. |
| T07/T08 | Kitaplık 1,75 → 2,7 m: ev −6,5° (sağ aşağı); geri alınca 0°. Sehpa 20 kg aynı uca: −5,4°. |
| T09 | Sabitli kitaplık depremde yerinden oynamadı (x = 1,75 sabit). |
| T10 | Sabitleme 750 → 720; deprem çantası iki kez alınamadı (720 → 640). |
| T13 | AFETE HAZIRIM → 3,02 sn geri sayım → Deprem. |
| T14/T15 | Sabitsiz, kitaplık kenarda: ev 24,6°'ye eğildi, eşyalar kaydı ve düştü, raporda kaybolanlar bir kez listelendi. Dengeli ve sabitsiz: eşyalar kaydı, kayıp yok, puan 67/100. Sabitli: 79/100. |
| T16 | Eşik aşıldıktan 1,00 sn sonra GameOver; başarısızlıkta para verilmedi (test için eşik geçici olarak 6°'ye indirildi). |
| T18/T19 | TEKRAR DENE: bütçe 750, Preparation, timeScale 1. ANA MENÜ: Menu sahnesi, faz Menu. |
| T20 | Level1'de 1 kamera, 1 AudioListener, 1 EventSystem. |
| T21 | Console: derleme ve çalışma hatası yok. |

## Açık konular / sonraki iş

- Mağaza, rapor, duraklatma ve "nasıl oynanır" panelleri uGUI'ye taşındı (`L1_Hud.Panels.cs`); eski IMGUI
  sürümleri yalnız `useLegacyHud` açıkken görünür. Editor'de düğmeler tek tek denendi (sekme, satın alma,
  duraklat → nasıl oynanır → devam, deprem → rapor → ana menü).
- Gerçek fare sürüklemesi kod yoluyla test edildi; **elle fareyle sürükleme oynanarak denenmedi**.
- Yan kapı ve dış ahşap platform eklenmedi (eski balkon gizlendi).
- Yeni CC0 paketleri (Kenney/Quaternius) indirilmedi; ses efektleri eski `L1_ProjectSfx` ile.
- Müzik lisansı belgelenmeli (bkz. `ASSET_ATTRIBUTIONS_LEVEL1.md`).
- Windows build ve 1366×768 / 1280×720 çözünürlük testleri yapılmadı.
- `docs/OYUN_TASARIMI.md`'ye tek kat / düz çatı tasarım revizyonu ayrıca işlenmeli (Lead onayı).
