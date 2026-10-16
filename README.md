# Son Denge (Last Balance)

Afete hazırlık temalı 2D fizik/strateji oyunu. Sivri bir kayanın tepesindeki evin dengesini koru, afet gelmeden önce hazırlan.
Oyun tasarımı: [docs/GDD.md](docs/GDD.md) · API: [docs/API_CONTRACTS.md](docs/API_CONTRACTS.md) · Görevler: [docs/TASK_BOARD.md](docs/TASK_BOARD.md)

## 0. Takım kuralları (önce bunu okuyun)

1. **Herkes aynı Unity sürümünü kullanır:** `6000.3.26f1` (Unity 6.3 LTS), Universal 2D şablonu.
2. **Önce ortak çekirdek, sonra bölüm bölüm:** plan [docs/TEAM_PLAN.md](docs/TEAM_PLAN.md). Herkes yalnızca kendi `LevelX.unity` sahnesine ve `Assets/Levels/LevelX/` klasörüne dokunur; `Menu` ve `CoreTest` Lead'in.
3. Çekirdek parçalar `CoreTest` sahnesinde denenir. Kişisel denemeler `Assets/_Sandbox/<isim>/` içinde (git'e gitmez).
4. Çekirdek sistemler (GameManager, AudioManager, SceneLoader) her sahnede otomatik kurulur; sahneye eklemeyin.
5. Küçük ve sık commit. Commit'ten önce `git pull`. Herkes kendi branch'inde (`kisi1-lead`, `kisi2-fizik`, `kisi3-gorsel`, `kisi4-hazirlik`).
6. `main` her zaman çalışır durumda olur. `Contracts.cs`, `ProjectSettings/`, `Packages/` değişikliği takım onayı ister.
7. AI ajanları için kurallar: [CLAUDE.md](CLAUDE.md).

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
    Core/        ← GameManager, Contracts (Kişi 1)
    Player/      ← oyuncu etkileşimi (Kişi 1)
    Physics/ Disasters/                  (Kişi 2)
    UI/                                  (Kişi 3)
    Preparation/ Economy/ Awareness/     (Kişi 4)
  Editor/        ← proje ayarları + build menüsü
  _Sandbox/      ← kişisel deneme alanı (git'e gitmez)
docs/            ← GDD, API sözleşmesi, görev panosu, playtest
```

## Atıflar

_(Üçüncü taraf asset ve sesler buraya.)_
