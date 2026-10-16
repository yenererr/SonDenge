# Kişi 3 — Görsel & UI · branch `kisi3-gorsel`

Önce oku: **`docs/ZORUNLU_KURALLAR.md`**, `CLAUDE.md`, `docs/TEAM_PLAN.md`, `docs/GDD.md`, `docs/API_CONTRACTS.md`.
Senin dosyaların: `Assets/Art/`, `Assets/Prefabs/UI/`, `Assets/Prefabs/Visual/` (House hariç), `Assets/Scripts/UI/`, `Level3.unity`, `Assets/Levels/Level3/`.
Denemeler için: `Assets/_Sandbox/kisi3/` içinde kendi test sahneni aç (git'e gitmez).

---

## Aşama 1 — Ortak çekirdek (0–12. saat)

- [ ] **Görseller** (`Assets/Art/`): 2 katlı açık kesit ev, sivri kaya, sade arka plan, 4 mobilya (dolap, koltuk, masa, kitaplık). Net silüet, 16:9. AI görsel üretimi ya da ücretsiz asset kullanabilirsin — kaynağı README "Atıflar"a yaz.
- [ ] Sabitlenmiş eşya için işaret (vida/kelepçe ikonu).
- [ ] **`HUD` prefab'ı**: denge çubuğu, kalan süre, bütçe, afet uyarısı. Tek metotla güncellenir:
  `RefreshHUD(float danger, float timeLeft, int budget, string alert)`
- [ ] **Paneller**: hazırlık paneli, sonuç paneli (farkındalık mesajı alanıyla), pause ve game over. Mevcut `StatePanels.cs` bunları faza göre açıp kapatır.
- [ ] Hazırlık ve afet anı görsel farkı (ör. afette kırmızı ton, ekran sarsıntısı `Juice.Shake`).
- [ ] Sahte verilerle test et (fizik/ekonomi henüz yokken de HUD çalışmalı).

Kontrol: HUD 16:9'da okunaklı, `RefreshHUD` ile değerler değişiyor, paneller doğru fazda açılıyor.

## Aşama 2 — Bölüm: `Level3` "Yeni Kat" (14–30. saat)

- [ ] `LevelTemplate` prefab'ını `Level3.unity`'ye sürükle.
- [ ] Ev bir kat büyür (3. kat) veya yeni oda açılır → denge daha zor.
- [ ] Yeni bir mobilya veya iki (ör. buzdolabı, piyano — ağır!) — görselini sen çiz, verisini Kişi 4'ten iste.
- [ ] İsteğe bağlı (P1): basit yangın uyarısı görseli (duman dedektörü ≠ sprinkler).
- [ ] `LevelSetup` → sonraki sahne: `Level4`. Baştan sona oyna, PR aç.

## Claude Code'a ilk mesaj (kopyala-yapıştır)

```text
I own art and UI in Son Denge, a 48-hour Unity 2D game jam. I have no game-dev experience; explain Unity steps simply.
Read docs/ZORUNLU_KURALLAR.md (mandatory), CLAUDE.md, docs/TEAM_PLAN.md, docs/GDD.md, docs/API_CONTRACTS.md and docs/tasks/KISI3_GORSEL.md.
Start with Phase 1: build a HUD prefab (balance bar, countdown, budget, disaster alert) with a single public
RefreshHUD(float danger, float timeLeft, int budget, string alert) method, plus preparation, results, pause and
game-over panels that work with the existing StatePanels.cs. Use placeholder sprites until final art exists.
Then help me set up the house, rock, background and 4 furniture sprites in Assets/Art/ for a readable 16:9 view.
Work only in Art/, Prefabs/UI/, Prefabs/Visual/, Scripts/UI/ and a test scene in Assets/_Sandbox/kisi3/.
Before coding, list the files you will create. After coding, tell me exactly how to test it in Unity.
```
