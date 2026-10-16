# Kişi 1 — Lead · branch `kisi1-lead`

Önce oku: `CLAUDE.md`, `docs/TEAM_PLAN.md`, `docs/GDD.md`, `docs/API_CONTRACTS.md`.
Senin dosyaların: `Assets/Scripts/Core/`, `Assets/Scripts/Player/`, `Assets/Prefabs/Furniture/`, `Menu.unity`, `CoreTest.unity`, `Level1.unity`, `Assets/Levels/Level1/`.

---

## Aşama 1 — Ortak çekirdek (0–12. saat)

- [ ] **Oyun akışı (`RoundController`)**: Preparation (geri sayım veya "Başla" butonu) → Disaster → Results / GameOver. `GameManager.SetPhase` kullan.
- [ ] **Mobilya sürükleme (`Scripts/Player/`)**: fareyle tıkla–sürükle–bırak, yalnızca Preparation fazında, sabitlenmiş eşya sürüklenmez. Yeni Input System.
- [ ] **Furniture prefab'ları** (`Assets/Prefabs/Furniture/`): dolap, koltuk, masa, kitaplık — Kişi 4'ün `Furniture` bileşeni + Kişi 3'ün görseli + `Rigidbody2D`/`Collider2D`. Diğerleri hazır değilse kutu placeholder kullan.
- [ ] **`LevelSetup` bileşeni** — her bölümün ayar paneli (Inspector'dan): hazırlık süresi, başlangıç bütçesi, deprem şiddeti ve süresi, kritik eğim eşiği, sonraki sahne adı. Bölüm bitince başarılıysa sonraki sahneye geçer.
- [ ] **`CoreTest.unity`**: ev + 4 mobilya + HUD + `LevelSetup` ile bir tam tur.
- [ ] **Birleştirme 1 (10–12. saat)**: herkesin PR'ını merge et, `CoreTest`'te birlikte dene. Sonra `House`, `HUD`, `LevelSetup` içeren hazır bir **`LevelTemplate` prefab'ı** yap — herkes bölümüne bunu sürükleyecek.

Kontrol: `CoreTest`'te mobilyayı sağa taşıyınca ev sağa eğiliyor; deprem başlıyor, bitince sonuç paneli açılıyor; Restart çalışıyor.

## Aşama 2 — Bölüm: `Level1` "İlk Sarsıntı" + `Menu` (14–30. saat)

- [ ] `Menu.unity`: başlık, açılış metni (GDD §2), Başla / Çıkış butonları.
- [ ] `Level1`: öğretici. 2 katlı ev, 4 mobilya, bol bütçe, hafif deprem. Ekranda adım adım ipuçları: "Mobilyayı sürükle", "Dolabı sabitle", "Ağır eşyaları alta koy".
- [ ] Bölüm sonu → `Level2`.
- [ ] **Birleştirme 2 (30–32. saat)**: tüm bölüm PR'larını merge et, `Menu → Level1 → Level2 → Level3 → Level4 → bitiş` zincirini dene.
- [ ] Windows build (`Jam > Build > Windows`), başka bilgisayarda dene.

## Claude Code'a ilk mesaj (kopyala-yapıştır)

```text
I am the lead on Son Denge, a 48-hour Unity 2D game jam. I have no game-dev experience; explain Unity steps simply.
Read CLAUDE.md, docs/TEAM_PLAN.md, docs/GDD.md, docs/API_CONTRACTS.md and docs/tasks/KISI1_LEAD.md.
Start with Phase 1: build RoundController (Preparation -> Disaster -> Results/GameOver using the existing GameManager),
mouse drag-and-drop for furniture during Preparation (new Input System), and a LevelSetup component with
Inspector settings (prep time, budget, quake intensity/duration, critical tilt, next scene).
Work only in Scripts/Core/, Scripts/Player/, Prefabs/Furniture/ and CoreTest.unity.
If other team modules are missing, use simple placeholder implementations of the interfaces in Contracts.cs.
Before coding, list the files you will create. After coding, tell me exactly how to test it in Unity.
```
