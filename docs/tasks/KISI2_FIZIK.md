# Kişi 2 — Fizik & Deprem · branch `kisi2-fizik`

Önce oku: `CLAUDE.md`, `docs/TEAM_PLAN.md`, `docs/GDD.md` (§3 Denge sistemi), `docs/API_CONTRACTS.md`.
Senin dosyaların: `Assets/Scripts/Physics/`, `Assets/Scripts/Disasters/`, `Assets/Prefabs/Visual/House*` (ev prefab'ının fizik kısmı), `Level2.unity`, `Assets/Levels/Level2/`.
Denemeler için: `Assets/_Sandbox/kisi2/` içinde kendi test sahneni aç (git'e gitmez) — `CoreTest` Lead'in.

---

## Aşama 1 — Ortak çekirdek (0–12. saat)

- [ ] **`HouseBalanceController`** (`IBalanceService`): mobilyaların kütlesi ve konumu evi eğer. `TiltDegrees`, `NormalizedDanger` (0 = güvenli, 1 = kritik).
  Basit model yeter: `tork = Σ(kütle × 9.81 × yatay uzaklık) − damping × açısal hız`. Gerçekçi fizik **şart değil**, okunaklı ve eğlenceli olsun.
- [ ] **`House` prefab'ı**: kayanın tepesinde dönen ev gövdesi + 2 kat zemin (placeholder kutular olabilir, görseli Kişi 3 değiştirecek).
- [ ] **`EarthquakeController`** (`IDisasterService`): `StartEarthquake(şiddet, süre)`, `StopDisaster()`, `IsRunning`. Depremde **sabitlenmemiş** eşyalar kayar/düşer, **sabitlenmiş** (`IsAnchored`) olanlar kaymaz.
- [ ] Event'ler: `OnDisasterStarted(DisasterType)`, `OnDisasterEnded()`, `OnFurnitureLost(string itemId)` (eşya evden düşünce).
- [ ] Tüm sayılar Inspector'dan ayarlanabilir (eşik, damping, şiddet).
- [ ] Gerçek mobilya hazır değilse test için basit bir sahte `IFurnitureItem` kutusu kullan.

Kontrol: kutuyu sağa taşıyınca ev sağa eğiliyor, sola taşıyınca düzeliyor; depremde sabitlenmemiş kutu kayıyor, sabitlenen kaymıyor.

## Aşama 2 — Bölüm: `Level2` "Artan Tehlike" (14–30. saat)

- [ ] `LevelTemplate` prefab'ını `Level2.unity`'ye sürükle.
- [ ] Daha fazla / daha ağır mobilya, daha güçlü ve uzun deprem, daha az bütçe.
- [ ] İsteğe bağlı: depremde artçı sarsıntı (2 kısa deprem).
- [ ] `LevelSetup` → sonraki sahne: `Level3`.
- [ ] Baştan sona oyna, zorluğu ayarla (kazanılabilir ama düşündürücü olsun), PR aç.

## Claude Code'a ilk mesaj (kopyala-yapıştır)

```text
I own physics and earthquakes in Son Denge, a 48-hour Unity 2D game jam. I have no game-dev experience; explain Unity steps simply.
Read CLAUDE.md, docs/TEAM_PLAN.md, docs/GDD.md, docs/API_CONTRACTS.md and docs/tasks/KISI2_FIZIK.md.
Start with Phase 1: implement HouseBalanceController (IBalanceService) and EarthquakeController (IDisasterService)
from Assets/Scripts/Core/Contracts.cs, plus a House prefab with placeholder visuals.
Furniture mass and position must tilt the house; anchored furniture must not slide in an earthquake.
Use a simple, stable, tunable model (Inspector values), not a realistic structural simulation.
Work only in Scripts/Physics/, Scripts/Disasters/ and a test scene in Assets/_Sandbox/kisi2/.
Before coding, list the files you will create. After coding, tell me exactly how to test it in Unity.
```
