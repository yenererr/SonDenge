# Kişi 1 — Lead · `Level1` "İlk Sarsıntı" + Ortak Çekirdek · branch `kisi1-level1`

Önce oku: **`docs/ZORUNLU_KURALLAR.md`**, `CLAUDE.md`, `docs/OYUN_TASARIMI.md`, `docs/TEAM_PLAN.md`, `docs/API_CONTRACTS.md`.
Senin alanın: `Assets/Scripts/Core/`, `Player/`, `Physics/`, `Disasters/` (Fire/ ve Storm/ hariç), `UI/`, `Economy/`, `Assets/Prefabs/`, `Menu.unity`, `CoreTest.unity`, `Level1.unity`, `Assets/Levels/Level1/`.

---

## 2–12. saat — Ortak çekirdek (`CoreTest.unity`'de, öncelik sırasıyla)

1. [ ] **Dönen ev** (`HouseBalanceController`, `IBalanceService`): 2 kat × 2 oda, sivri kaya, görünür pivot. Tork = Σ(kütle × pivota yatay uzaklık) + ev ağırlık merkezi + afet kuvveti − sönümleme. Atalet + sönümlemeyle yumuşak dönüş. Durumlar: GÜVENLİ 0–7°, DENGESİZ 7–16°, KRİTİK 16–28°, ÇÖKÜŞ 28°+ ~1 sn. **Ev gerçekten döner.**
2. [ ] **Mobilya sürükleme** (`Scripts/Player/`): sol tık seç, sürükle, bırak; hover'da kütle + değer; ESC iptal; R yeniden başlat; seçiliyken dış çizgi ve beklenen denge; zemine oturur, duvardan geçmez.
3. [ ] **Sabitleme**: ücretli, çapa ikonu, hazırlıkta geri alınabilir, **kütleyi kaldırmaz**; afette sabitli eşya taşınamaz.
4. [ ] **Deprem** (`IDisasterService`): "AFETİ BAŞLAT" → 3 sn geri sayım → "DEPREM!" → değişen yönlü yatay kuvvet, 25 sn. Sabitsiz eşya kayar/devrilir/kırılır/düşer; düşen eşya dengeden çıkar. Kamera sarsıntısı (`Juice.Shake`), toz. Afette enerjiyle sınırlı sihirli taşıma. Ekranda "Gerçek depremde eşya taşımayın. Çök–Kapan–Tutun."
5. [ ] **Döngü + HUD + rapor**: Hazırlık ("RADYO SİVRİTEPE" uyarısı, 100 para) → Afet → "AFET RAPORU" (Ev Durumu, Hazırlık Puanı, Korunan/Kaybedilen Eşya, Kazanılan Para, Güvenlik Puanı; ödül tur başına bir kez). HUD yerleşimi OYUN_TASARIMI §12'deki gibi.
6. [ ] **`LevelSetup`** (Inspector): bütçe, afet türü, şiddet, süre, mobilya listesi, sonraki sahne. **`LevelTemplate` prefab'ı**: ev + HUD + rapor + `LevelSetup`.

Diğerlerinin parçaları hazır değilse placeholder kullan (küp placeholder modeller, sabit mobilya değerleri). 12. saatte PR'ları merge et (Birleştirme 1), herkese haber ver.

## 12–30. saat — `Level1` + `Menu`

- [ ] `Level1`: orta şiddet deprem, 4–5 mobilya (sandalye, masa, kanepe, kitaplık), bol bütçe. Adım adım ipuçları: "Mobilyayı sürükle", "Evin eğildiğini gör", "Kitaplığı sabitle", "AFETİ BAŞLAT".
- [ ] `Menu`: SON DENGE · OYNA · NASIL OYNANIR · AYARLAR; kısa açılış metni.
- [ ] Bölüm sonu → `Level2`.
- [ ] **Birleştirme 2 (30–32):** tüm PR'lar, `Menu → Level1 → … → Level4`, Windows build, başka bilgisayarda test.

## Claude Code'a ilk mesaj (kopyala-yapıştır)

```text
I am the lead on Son Denge, a 48-hour Unity 3D game jam (fixed side-view camera, see OYUN_TASARIMI §0). I have no game-dev experience; explain Unity steps simply.
Read docs/ZORUNLU_KURALLAR.md (mandatory), CLAUDE.md, docs/OYUN_TASARIMI.md, docs/TEAM_PLAN.md,
docs/API_CONTRACTS.md and docs/tasks/KISI1_LEVEL1.md.
Build the shared core in CoreTest.unity, in this priority order: (1) a 2-story house on a sharp rock that really
rotates around a pivot using a simple torque model with inertia and damping and the SAFE/UNSTABLE/CRITICAL/COLLAPSE
thresholds from OYUN_TASARIMI §4; (2) mouse drag-and-drop furniture (new Input System) with hover info;
(3) paid anchoring that keeps mass; (4) a 25 s earthquake where unanchored furniture slides/falls and lost furniture
leaves the balance; (5) Preparation -> Disaster -> AFET RAPORU loop with HUD; (6) LevelSetup + LevelTemplate prefab.
Use placeholder visuals and data where teammates' parts are not ready. All visible text in Turkish.
Commit and push after each working step. Before coding, list the files you will create.
After each step, tell me exactly how to test it in Unity.
```
