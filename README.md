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

## Atıflar

Aktif Level1 görünümü aşağıdaki referans görselini kullanır. Önceki 3D taslakta kullanılan paketlerin atıfları arşiv kurulum için korunmuştur.

- **Level1 referans ev:** Kullanıcının sağladığı şeffaf ev görseli (`L1_ReferenceHouse_Empty.png`); mobilyalı varyant, kullanıcının iki referansı kullanılarak yerleşik ImageGen ile düzenlendi (`L1_ReferenceHouse_Furnished.png`). Yeni bir Asset Store paketi indirilmedi. Kaynak ve üretim istemi: [L1_ReferenceHouse_SOURCE.md](Assets/Levels/Level1/Art/L1_ReferenceHouse_SOURCE.md).

- **DOLLHOUSE - FREE - Modular Low Poly Interiors: Furniture & Room Kit** (ForgeWorks): Level1 ön direkleri, kirişleri, baca, fener ve katlar arası ahşap merdiven. [Unity Asset Store](https://assetstore.unity.com/packages/3d/props/interior/dollhouse-free-modular-low-poly-interiors-furniture-room-kit-411314). Lisans: Standard Unity Asset Store EULA.
- **Stylized Cozy Tavern: Low Poly Single Material Props / Stylized Tavern Prop Pack (Free Starter Edition)** (VEE-TREX): Level1 pencereli duvarları, zemin parçaları, çatı detayı, meşale lambaları ve mobilyalar. [Yayıncı sayfası](https://assetstore.unity.com/publishers/vee-trex). Lisans: Unity Asset Store üzerinden edinilen paketin Standard Unity Asset Store EULA koşulları.
- **Yughues Free Wooden Floor Materials** (Nobiax / Yughues): Level1 ahşap platform kaplaması. [Unity Asset Store](https://assetstore.unity.com/packages/2d/textures-materials/wood/yughues-free-wooden-floor-materials-13213). Lisans: Standard Unity Asset Store EULA.
- **Low Poly Interior Props Pack** (3Digitalis): Level1 raf, kitap, saat ve saksı dekorları. Kaynak: Unity Asset Store, 3Digitalis. Lisans: Standard Unity Asset Store EULA.
- **Roadside Tales Free - Modular Fence Narrative Pack** (EmacEArt): Level1 balkon ve seyir terası korkulukları. [Unity Asset Store](https://assetstore.unity.com/packages/3d/environments/landscapes/roadside-tales-free-modular-fence-narrative-pack-133601). Lisans: Standard Unity Asset Store EULA.
- **Level1 dağ ve gün batımı görselleri:** Bu proje için konsept referanslarından üretilmiş özgün görseller; `Assets/Levels/Level1/Art/` içinde.

## Level1 ev önizlemesi

![Referans görselinden üç katlı Level1 evi](Assets/Levels/Level1/Art/L1_ReferenceHouse_Preview.png)

Sahne `Son Denge > Level1 > Gorsel Ev ve Dagi Kur` ile yeniden kurulabilir. `Referanstaki Bos Evi Goster` aynı yapının kullanıcının verdiği boş görselini açar. Önceki model tabanlı kurulum `Arsiv > Eski 3D Taslagi Kur` menüsündedir. Bu komutlar Level1 görsel kökünü yeniden oluşturur; elle yapılan kök altı düzenlemeleri değiştirir.

Aktif ev **görsel tabanlı 2.5D** olarak kurulmuştur: şeffaf ev resmi, ayrı 3D zemin çarpışmaları ve üç merdiven tetikleyicisi aynı denge pivotu altındadır. Yıpranmış ahşap, mobilyalar ve fener görüntüleri resmin parçasıdır; bağımsız taşınabilir 3D eşya değildir. Fener noktalarında ayrıca karakteri/3D eşyaları aydınlatabilecek sıcak ışıklar bulunur.

Merdiven bileşeni `Player` etiketli 3D `Rigidbody` için `W/S` veya yukarı/aşağı ok girişini kullanır. Sahnede oyuncu olmadığı için karakter kontrolcüsüyle katlar arası oynanış henüz doğrulanmadı. Mevcut çalışma ev/dağ görseli ve fizik alanlarının yerleşimidir.

Görmek için `Assets/Scenes/Level1.unity` sahnesini açıp **Game** görünümünü 16:9 yapın. `Referans Ev Onizleme Kaydet` menüsü kameradan 1920×1080 önizleme kaydeder. Unity Console hata/uyarı taraması ve sahne doğrulamasında eksik script veya bozuk prefab bulunmadı.
