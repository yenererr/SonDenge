# FAZ 0 — Adli Envanter / Baseline

Tarih: 2026-10-18 · Denetleyen: Claude Code (Lead/Kişi 1 oturumu)

## 1. Git durumu

| Dal | HEAD | Son yazar | Not |
|---|---|---|---|
| `main` | `e207db4` (17.10) | Yener Er | MCP kurulum PR #3 merge |
| `kisi1-level1` | `c7639d6` (18.10) | Yener | En gelişmiş dal; 5 afet prototipi tek sahnede |
| `kisi2-level2` | `4c5b49c` (17.10) | ElmaYiyenAdam | **Aktif çalışma var** (main'e göre 3056 dosya farkı, mağaza kodu) |
| `kisi3-level3` | `e207db4` | — | main ile aynı, aktif iş yok |
| `kisi4-level4` | `e207db4` | — | main ile aynı, aktif iş yok |

- Yedek: `backup/pre-wilson-5level` etiketi → `c7639d6`.
- Çalışma dalı: `feature/wilson-missions-five-levels` (`kisi1-level1`'den açıldı).
- Çalışma ağacında bu işe ait olmayan ~138 kirli dosya var (materyal/asset paketleri, `.mcp.json`). **Dokunulmadı, commit'lenmeyecek.**
- Git LFS 3.7.1 kurulu.

## 2. Sahneler ve build

- `Assets/Scenes/`: `Menu`, `Level1`, `Level2`, `Level3`, `Level4`, `CoreTest`. **`Level5` yok.**
- `EditorBuildSettings`: Menu, Level1–4 (CoreTest yok).
- `ProjectSettings.runInBackground: 0` → Editor arka plandayken Play Mode kare ilerletmiyor; MCP ile otomatik Play testi bu yüzden donuyor (frame 2'de kalıyor, gözlemlendi).

## 3. Kod envanteri (Level1)

| Dosya | Satır | Rol |
|---|---:|---|
| `L1_ProjectGameController.cs` | 1097 | Ana döngü, denge, mağaza `Catalog`, `budget` |
| `.Quake/.Fire/.Grow/.UI/.Api/.Wilson.cs` | 29–406 | Afetler, kat büyütme, eski GUI, API |
| `L1_PropDrag.cs` | 494 | Fare sürükleme + E taşıma |
| `L1_MovableProp.cs` | 76 | Kütle, sabitleme |
| `L1_ResidentController.cs` / `L1_LadderClimb.cs` | 189 / 75 | Karakter, merdiven |
| `L1_WilsonChicken.cs` | 381 | Wilson yürüme/yumurta, **`Resources.Load` kullanıyor (kural 27 ihlali)** |
| `L1_Hud.cs`, `L1_Hud.Panels.cs`, `L1_HudSkin.cs`, `L1_PaperGui.cs` | ~1030 | IMGUI tabanlı HUD + mağaza paneli |

## 4. Asset envanteri

- Wilson: `Assets/Resources/Wilson/low_poly_chicken.obj/.png`, `egg.obj` (CC BY 4.0, `ThirdParty/Wilson/CREDITS.md`).
- Yangın tüpü: `Assets/AK STUDIO ART/Fire Extinguisher/Prefabs/Fire Extinguisher.prefab`, `Assets/LowPolyInteriorsFree/Prefabs/20_Utility/SM_LPI_Util_Extinguisher_01.prefab`.
- Mobilya: StylArts, MIX, Realistic Bed, LowPolyInteriorsFree, Medieval Room Kit.

## 5. Çalışan / kırık / teknik borç

Çalışan (bu oturumda doğrulanan): Level1 derleniyor (Console hata 0), denge hesabı, eşya kayması, görünmez yan duvarlar, Wilson tavuk, deprem/fırtına/yangın/yıldırım/yağmur prototipi.

Teknik borç (şartname §1.1 ile uyumlu, kod okumasıyla doğrulandı):
1. 5 afet tek sahnede `level` sayacıyla oynuyor; hedef 5 ayrı sahne.
2. Mağaza/altın (`Catalog`, `budget`, `budgetBonusPerLevel`, altın yumurta +100).
3. Çökme kontrolü `level <= 2` ile sınırlı; `level >= 3`'te eğim %16'ya kısılıyor.
4. HUD IMGUI tabanlı; şartname UGUI istiyor.
5. `Resources.Load` (Wilson).
6. Hazırlık süresi `45f` sabit.

## 6. Testler

- Projede EditMode/PlayMode test klasörü ve `.asmdef` **yok**. Test Framework paketi kurulu mu doğrulanmadı.

## 7. Engeller (BLOCKED — kullanıcı/takım kararı gerekli)

1. **Sahne sahipliği:** `Level2` (Kişi 2, aktif iş var), `Level3` (Kişi 3), `Level4` (Kişi 4). Kural 7: başkasının sahnesi kaydedilmez.
2. **`Level5` + build listesi:** Kural 18 ve `ProjectSettings/` (kural 10) — takım onayı.
3. **`runInBackground`:** `ProjectSettings/` değişikliği; otomatik Play testleri için gerekli.
4. **Test Framework / asmdef:** `Packages/` değişikliği olabilir (kural 10).

## 8. Karar

Esas dal: `kisi1-level1`. Sahiplik onayı gelene kadar güvenli kapsam: Lead klasörleri (`Assets/Scripts/Core|UI|Preparation|Disasters`, `Assets/Levels/Level1/`, `Menu`, `Level1`, `CoreTest`).
