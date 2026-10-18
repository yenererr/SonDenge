# Son Denge (Last Balance)

Afete hazırlık temalı 3D (sabit yandan kamera) fizik/strateji oyunu. Sivri bir kayanın tepesindeki evin dengesini koru, afet gelmeden önce hazırlan.
Oyun tasarımı: [docs/OYUN_TASARIMI.md](docs/OYUN_TASARIMI.md) · Plan: [docs/GDD.md](docs/GDD.md) · API: [docs/API_CONTRACTS.md](docs/API_CONTRACTS.md) · Görevler: [docs/TASK_BOARD.md](docs/TASK_BOARD.md)

## 0. Takım kuralları (önce bunu okuyun)

> **Zorunlu kurallar:** [docs/ZORUNLU_KURALLAR.md](docs/ZORUNLU_KURALLAR.md) — herkes ve her AI ajanı okumak zorunda.

1. **Herkes aynı Unity sürümünü kullanır:** `6000.3.26f1` (Unity 6.3 LTS), Universal 3D şablonu (URP).
2. **Önce ortak çekirdek, sonra bölüm bölüm:** plan [docs/TEAM_PLAN.md](docs/TEAM_PLAN.md). Herkes yalnızca kendi `LevelX.unity` sahnesine ve `Assets/Levels/LevelX/` klasörüne dokunur; `Menu` ve `CoreTest` Lead'in.
3. Çekirdek parçalar `CoreTest` sahnesinde denenir. Kişisel denemeler `Assets/_Sandbox/<isim>/` içinde (git'e gitmez).
4. Çekirdek sistemler (GameManager, AudioManager, SceneLoader) her sahnede otomatik kurulur; sahneye eklemeyin.
5. Küçük ve sık commit. Commit'ten önce `git pull`. Herkes kendi branch'inde (`kisi1-level1`, `kisi2-level2`, `kisi3-level3`, `kisi4-level4`).
6. `main` her zaman çalışır durumda olur. `Contracts.cs`, `ProjectSettings/`, `Packages/` değişikliği takım onayı ister.
7. AI ajanları için kurallar: [CLAUDE.md](CLAUDE.md). Unity MCP kurulumu: [docs/MCP_KURULUM.md](docs/MCP_KURULUM.md).

## 1. Kurulum (sadece Lead, bir kez)

1. Unity'yi kapatın, terminalde proje kökünde:
   - Windows: `powershell -ExecutionPolicy Bypass -File setup.ps1 <github-repo-url>`
   - macOS/Linux: `bash setup.sh <github-repo-url>`
2. Commit + push sonrası herkese repo linkini gönderin.

## 2. Diğerleri: repoyu çekip çalıştırma

```bash
git lfs install
git clone <github-repo-url>
git checkout kisiN-...   # kendi branch'iniz, bkz. docs/TEAM_PLAN.md
```
Unity Hub > **Add > Add project from disk** ile klasörü ekleyin ve **6000.3.26f1** ile açın. İlk açılış birkaç dakika sürer, normal.

## 3. Hazır sistemler

| Sistem | Kullanım |
|---|---|
| `GameManager` | `GameManager.Instance.SetPhase(GamePhase.Disaster)` · `GameManager.OnPhaseChanged += ...` · ESC ile duraklatma (`IsPaused`, `OnPauseChanged`) |
| `AudioManager` | `AudioManager.Instance.PlaySfx(clip)` · `PlayMusic(clip)` |
| `SceneLoader` | `SceneLoader.Instance.Load("Main")` · `ReloadCurrent()` · `Quit()` (fade'li) |
| `Juice` | `Juice.Shake(0.15f, 0.2f)` · `Juice.HitStop(0.05f)` |
| `StatePanels` | Canvas'a ekleyin, Pause / Results / GameOver panellerini bağlayın; butonlara `Resume`, `Restart`, `LoadScene`, `Quit` |

Fazlar: `Menu, Preparation, Disaster, Results, GameOver`. Sözleşme arayüzleri: `IFurnitureItem`, `IBalanceService`, `IDisasterService`, `IPreparationService` (`Assets/Scripts/Core/Contracts.cs`).

## 4. Kontroller

_(Oyun kontrolleri buraya yazılacak.)_ ESC: duraklat.

## 5. Build

- `Jam > Build > Windows` → `Builds/Windows`
- `Jam > Build > WebGL` (P2; WebGL Build Support modülü gerekir)
- **İlk saatlerde bir kez Windows build alıp başka bir bilgisayarda deneyin.**

## 6. Merge conflict çıkarsa

- `.cs` dosyası: normal şekilde çözün.
- Sahne / prefab: elle çözmeyin. UnityYAMLMerge kullanın (`Editor/Data/Tools/`) ya da birinin sürümünü alın (`git checkout --theirs <dosya>`) ve değişikliği yeniden yapın.

## Klasör yapısı

```
Assets/
  Art/ (Environment, Furniture, UI)   Audio/   Data/Furniture/
  Prefabs/ (Furniture, Visual, UI)    Scenes/ (Menu, Level1-4, CoreTest)  Settings/ (URP)
  Levels/Level1..4/   ← bölüme özel dosyalar (sahibi o bölümün kişisi)
  Scripts/
    Core/ Player/ Physics/ Disasters/ UI/ Economy/ Preparation/   ← ortak çekirdek (Kişi 1)
    Shop/                 (Kişi 2)     Disasters/Fire/    (Kişi 3)
    Awareness/            (Kişi 4)     Disasters/Storm/   (Kişi 4)
  Data/Furniture/ Audio/  (Kişi 2)     Art/ (Kişi 4) · Art/FX/ (Kişi 3)
  Editor/        ← proje ayarları + build menüsü
  _Sandbox/      ← kişisel deneme alanı (git'e gitmez)
docs/            ← GDD, API sözleşmesi, görev panosu, playtest
```

## Wilson görev modu — 5 bölüm (feature/wilson-missions-five-levels)

Mağaza ve altın kaldırıldı. Her bölümde Wilson önce görevleri anlatır (süre durur), sonra hazırlık → 3-2-1 → afet → yıldızlı AFET RAPORU.

| Bölüm | Afet | Hazırlık / Afet | Kat | Görev |
|---|---|---|---|---|
| 1 | Deprem | 90 / 12 sn | 2 | 4 |
| 2 | Fırtına | 100 / 12 sn | 2 | 4 |
| 3 | Yangın | 120 / 30 sn | 3 | 4 |
| 4 | Yıldırım | 110 / 25 sn | 3 | 3 |
| 5 | Şiddetli yağmur | 140 / 25 sn | 3 | 4 |

- Kontroller: A/D yürü, W/S merdiven, E al/bırak/kullan, fare seç/sürükle, F sabitle, BOŞLUK afete hazırım / depremde Çök–Kapan–Tutun, ESC duraklat, yangında T tahliye.
- Veriler: `Assets/Data/{Levels,Missions,Wilson}`. Kod: `Assets/Scripts/Missions`, `Assets/Scripts/UI`, `Assets/Levels/Level1/L1_Mission*.cs`.
- Sahneleri yeniden üretmek: `Jam > Wilson > Bölüm verilerini oluştur`, `Jam > Wilson > Level1 görevlerini kur`, `Jam > Wilson > Level2-5 sahnelerini Level1'den üret`.
- Rapor ve testler: `docs/production/` (WILSON_IMPLEMENTATION_LOG, FINAL_VALIDATION_REPORT, SAFETY_REVIEW).
- Evin ağırlıkla dengelenmesi oyunun hayal ürünü kuralıdır; gerçek afet tavsiyesi değildir.

## Menü (yedi ekran)

`Assets/Scenes/Menu.unity` → Ana Menü · Bölüm Seçimi · Wilson Görev Tanıtımı · Yükleme · Nasıl Oynanır? · Ayarlar · Oyun Hakkında; bölümlerde ESC ile Duraklat menüsü.

- Kurulum/yeniden kurma: Unity'de **Jam > Menü > Hepsini kur** (sanat + prefab + sahne) veya **Menü sahnesini kur**; duraklatma için **Duraklat menüsünü kur ve bölümlere bağla**.
- Kod: `Assets/Scripts/UI/Menu/` (`MenuFlowController` tek akış kaynağı), prefablar ve menü sanatı: `Assets/Prefabs/UI/Menu/`.
- Bölüm verisi `Assets/Data/Levels/*.asset`'ten, yıldız/kilit `LevelProgressSave`'den, geçiş `SceneLoader.LoadWithProgress` ile (gerçek async ilerleme).
- Menü UI görselleri (ahşap/parşömen sprite'ları, ikonlar, arayüz sesleri) proje içinde kodla üretildi; arka plan görselleri ekibin hazırladığı yazısız sahne görselleridir. Ayrıntı: `docs/production/MENU_IMPLEMENTATION_LOG.md`.

## Atıflar

- **Menü yazı tipi:** Liberation Sans (SIL Open Font License), TextMesh Pro ile gelen `LiberationSans.ttf`.

Aktif Level1 evi gerçek 3D geometri ve aşağıdaki paketlerin modellerinden oluşur. Dağ ve gökyüzü sprite olarak kalır. Ev referans resimleri yalnız arşivde tutulur, aktif sahnede kullanılmaz.

- **Arşivdeki Level1 referans ev:** Kullanıcının sağladığı şeffaf ev görseli (`L1_ReferenceHouse_Empty.png`); mobilyalı varyant kullanıcının iki referansı kullanılarak ImageGen ile düzenlendi (`L1_ReferenceHouse_Furnished.png`). Kaynak ve üretim istemi: [L1_ReferenceHouse_SOURCE.md](Assets/Levels/Level1/Art/L1_ReferenceHouse_SOURCE.md).

- **KayKit : Adventurers Character Pack 2.0** — Kay Lousberg (www.kaylousberg.com) — CC0. Level 1 sakin karakteri (Barbarian modeli, dokusu ve Rig_Medium animasyonları). Lisans: `Assets/Levels/Level1/Art/Characters/KayKit_Barbarian/KayKit_License.txt`.

- **Medieval Room Kit** — 1Luka2 — [Unity Asset Store](https://assetstore.unity.com/packages/3d/environments/medieval-room-kit-399232) — Standard Unity Asset Store EULA. Son Denge'nin Level 1 modüler oda/kat görsellerinde kullanıldı (zemin, arka duvar, kolon, süpürgelik, tavan pervazı). Repoya yalnız kullanılan modeller/dokular eklendi.

- **DOLLHOUSE - FREE - Modular Low Poly Interiors: Furniture & Room Kit** (ForgeWorks): Level1 yatak, kanepe, küvet, kitaplık, tekstiller, fener ve ahşap merdiven modelleri. [Unity Asset Store](https://assetstore.unity.com/packages/3d/props/interior/dollhouse-free-modular-low-poly-interiors-furniture-room-kit-411314). Lisans: Standard Unity Asset Store EULA.
- **Stylized Cozy Tavern: Low Poly Single Material Props / Stylized Tavern Prop Pack (Free Starter Edition)** (VEE-TREX): Level1 demir lamba parçaları, masa, sandalye, tezgâh, sandık, fıçı, tabure, şişeler ve mumlar. [Yayıncı sayfası](https://assetstore.unity.com/publishers/vee-trex). Lisans: Unity Asset Store üzerinden edinilen paketin Standard Unity Asset Store EULA koşulları.
- **Modular Sewer Props** (Ata Khani): `Wooden_Plank_01` modelinden kullanılan döşeme mesh'i ve `Trim_Wood_basecolor` dokusu Level1 içine kopyalandı; yerel URP materyali oluşturuldu. [Unity Asset Store](https://assetstore.unity.com/packages/3d/props/modular-sewer-props-277458). Lisans: Standard Unity Asset Store EULA. Projede zaten bulunan paket kullanıldı; yeni indirme yapılmadı.
- **Yughues Free Wooden Floor Materials** (Nobiax / Yughues): Level1 ahşap platform kaplaması. [Unity Asset Store](https://assetstore.unity.com/packages/2d/textures-materials/wood/yughues-free-wooden-floor-materials-13213). Lisans: Standard Unity Asset Store EULA.
- **Low Poly Interior Props Pack** (3Digitalis): Level1 raf, kitap, saat ve saksı dekorları. Kaynak: Unity Asset Store, 3Digitalis. Lisans: Standard Unity Asset Store EULA.
- **Roadside Tales Free - Modular Fence Narrative Pack** (EmacEArt): Level1 balkon ve seyir terası korkulukları. [Unity Asset Store](https://assetstore.unity.com/packages/3d/environments/landscapes/roadside-tales-free-modular-fence-narrative-pack-133601). Lisans: Standard Unity Asset Store EULA.
- **Level1 dağ ve gün batımı görselleri:** Bu proje için konsept referanslarından üretilmiş özgün görseller; `Assets/Levels/Level1/Art/` içinde.

## Level1 ev önizlemesi

![Gerçek 3D üç katlı Level1 evi](Assets/Levels/Level1/Art/L1_Playable3D_Preview.png)

Sahne `Son Denge > Level1 > Oynanabilir 3D Cozy Evi Kur` ile yeniden kurulabilir. Komut Play kapalıyken çalışır ve Level1 görsel kökünü yeniden oluşturur; bu kökün altındaki elle düzenlemeleri değiştirir. Önceki resim tabanlı sürüm `Arsiv` menüsüne alınmıştır.

Ev, kemerli pencere açıklıkları, ahşap kapılar, eğimli kiremit çatı, pahlı eğri kirişler, balkon, demir çiviler ve aşınmış döşemelerle gerçek 3D olarak kurulur. Mobilyalar paket prefab'larıdır. Sıva dokusu kodla üretilen yüzey malzemesidir. Evin cephesinde resim kullanılmaz. Sahnede yalnız dağ ve gökyüzü için iki SpriteRenderer vardır.

### Kullanım

1. `Assets/Scenes/Level1.unity` sahnesini açın, Game görünümünü 16:9 yapıp Play'e basın.
2. `A/D` veya sağ/sol okla karakteri yürütün.
3. Alt katta sağdaki merdivenin yanında `W`/yukarı okla orta kata çıkın. Orta katta soldaki merdiven çatı katına çıkar. Üst girişte `S`/aşağı okla inin. Tırmanırken kapak açılır; karakterin ilgili kat zeminiyle çarpışması geçici olarak kaldırılıp çıkışta geri açılır.
4. Sol fareyle mobilyayı tutup sürükleyin, bırakınca fizik tekrar zemine oturtur. 12 mobilya bağımsız Rigidbody ve BoxCollider kullanır. Taşıma mevcut kat içinde, başlangıç yüksekliğinin ±1.1 metresiyle sınırlıdır; katlar arası eşya taşıma veya karakterin elinde taşıma eklenmedi.
5. `ESC` mevcut ortak duraklatma sistemidir. `3D Ev Onizleme Kaydet` menüsü 1920×1080 kamera görüntüsü kaydeder.

### Kapsam ve kontrol durumu

Bu çalışma 3D ev, karakter hareketi, merdiven geçişi ve fizik tabanlı eşya sürükleme altyapısını ekler. Denge/deprem/ekonomi oyun döngüsü bu değişikliğin parçası değildir. Dekoratif raflar, kapılar ve vinç sandığı sabittir; taşınabilir mobilyaların üstündeki yastık, örtü, kupa ve kitaplar mobilyayla birlikte hareket eder.

Editör derlemesi ve kamera önizlemesi kontrol edildi. Kontrollerin tamamını kapsayan Play testi veya platform build'i çalıştırılmadı. Sahnede 1 karakter kontrolcüsü, 2 merdiven rotası ve 12 taşınabilir eşya bulunur. Kod ve kurulum `Assets/Levels/Level1/` altında tutulur; ortak paket, ayar ve arayüz dosyaları değiştirilmez.
