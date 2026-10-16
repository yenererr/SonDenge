# Kişi 3 · `Level3` "Yangın" · branch `kisi3-level3`

Önce oku: **`docs/ZORUNLU_KURALLAR.md`**, `CLAUDE.md`, `docs/OYUN_TASARIMI.md` (özellikle §11 Yangın), `docs/TEAM_PLAN.md`, `docs/API_CONTRACTS.md`.
Senin alanın: `Assets/Scripts/Disasters/Fire/`, `Assets/Art/FX/`, `Level3.unity`, `Assets/Levels/Level3/`.
Denemeler: `Assets/_Sandbox/kisi3/` (git'e gitmez).

---

## 2–12. saat — Yangın sistemi (kendi sandbox'ında, ev olmadan da çalışsın)

- [ ] **`FireController`** (namespace `Jam.Disasters.Fire`): belirli bir odada başlar, zamanla yakındaki mobilyalara yayılır.
- [ ] Korumasız mobilya dayanıklılık kaybeder → yanıp **yok olur** → `IBalanceService.UnregisterFurniture` ile **kütlesi dengeden çıkar** (denge değişir).
- [ ] Güvenlik ekipmanları (hazırlıkta satın alınır):
  - **Duman dedektörü:** yangını **erken haber verir**, söndürmez.
  - **Yangın söndürücü:** küçük yangını oyuncu tıklayarak söndürür (sınırlı kullanım).
  - **Sprinkler:** odadaki yangını otomatik söndürür (pahalı).
- [ ] Efektler (`Assets/Art/FX/`): alev, duman, kül, söndürme buharı; sesler Kişi 2'den.
- [ ] Ayarlar Inspector'dan: yayılma hızı, hasar, ekipman fiyatları.
- [ ] Ev hazır değilse kutularla test et; mobilya için `IFurnitureItem` sahtesi kullan.

## 12–30. saat — `Level3` "Yangın"

- [ ] `LevelTemplate` prefab'ını `Level3.unity`'ye sürükle, afet türünü yangın yap.
- [ ] Radyo uyarısı: yangın riski (ör. "Kuru ve rüzgârlı hava, yangın riski yüksek!").
- [ ] Zor karar: sınırlı bütçeyle dedektör mü, söndürücü mü, sprinkler mi, sabitleme mi?
- [ ] AFET RAPORU'nda yangına özel mesajlar (oyuncunun gerçek seçimine göre):
  - Dedektör aldıysa: "Duman dedektörü yangını erken fark etmeni sağladı."
  - Dedektör yoksa: "Duman dedektörü erken uyarı verir; yangını fark etmek için zaman kazandırır."
  - Sadece dedektör varsa: "Dedektör uyarır ama söndürmez; söndürme için ayrı ekipman gerekir."
- [ ] Bölüm sonu → `Level4`. Baştan sona oyna, PR aç.

## Claude Code'a ilk mesaj (kopyala-yapıştır)

```text
I own Level3 (fire) in Son Denge, a 48-hour Unity 2D game jam. I have no game-dev experience; explain Unity steps simply.
Read docs/ZORUNLU_KURALLAR.md (mandatory), CLAUDE.md, docs/OYUN_TASARIMI.md (§11 Fire), docs/TEAM_PLAN.md,
docs/API_CONTRACTS.md and docs/tasks/KISI3_LEVEL3.md.
First build a standalone fire system in Assets/Scripts/Disasters/Fire/: fire starts in a room, spreads to nearby
furniture, damages and destroys unprotected furniture, and destroyed furniture is removed from the balance
(IBalanceService.UnregisterFurniture). Add smoke detector (early warning only, never extinguishes), extinguisher
(manual, limited) and sprinkler (automatic). Use placeholder boxes and a fake IFurnitureItem until the shared house exists.
Test in a scene under Assets/_Sandbox/kisi3/. Work only in my folders. All visible text in Turkish.
Commit and push after each working step. Before coding, list the files you will create.
After each step, tell me exactly how to test it in Unity.
```
