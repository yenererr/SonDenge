# Kişi 2 · `Level2` "Artan Tehlike" · branch `kisi2-level2`

Önce oku: **`docs/ZORUNLU_KURALLAR.md`**, `CLAUDE.md`, `docs/OYUN_TASARIMI.md`, `docs/TEAM_PLAN.md`, `docs/API_CONTRACTS.md`.
Senin alanın: `Assets/Data/Furniture/`, `Assets/Scripts/Shop/`, `Assets/Audio/`, `Level2.unity`, `Assets/Levels/Level2/`.
Denemeler: `Assets/_Sandbox/kisi2/` (git'e gitmez).

---

## 2–12. saat — Ortak çekirdeğe katkı (Lead bunları kullanacak)

- [ ] **Mobilya verileri** (`Assets/Data/Furniture/`): `FurnitureData` ScriptableObject + 6 asset. **İlk bunu bitir, Lead'e haber ver.**

  | Eşya | Kütle | Değer | Sabitleme |
  |---|---|---|---|
  | Sandalye | 1 | 10 | 5 |
  | Masa | 2 | 15 | 10 |
  | Kanepe | 3 | 25 | 12 |
  | Kitaplık | 4 | 30 | 15 |
  | Gardırop | 5 | 40 | 20 |
  | Buzdolabı | 6 | 50 | 25 |
  Alanlar: ad (Türkçe), kütle, değer, sabitleme bedeli, satın alma fiyatı, dayanıklılık, sprite referansı.
- [ ] **Mağaza** (`Scripts/Shop/`): hazırlıkta yeni mobilya satın alma paneli; para yetmezse alınamaz. Lead'in ekonomi API'sini kullanır (hazır değilse basit bir arayüzle bekle).
- [ ] **Sesler** (`Assets/Audio/`): mobilya seçme, yerleştirme, ağır darbe, kırılma, ahşap gıcırtısı, deprem gürültüsü, kritik denge uyarısı, ortam müziği. Ücretsiz/lisanslı kaynak → README "Atıflar". `AudioManager.Instance.PlaySfx(clip)`.

## 12–30. saat — `Level2` "Artan Tehlike"

- [ ] `LevelTemplate` prefab'ını `Level2.unity`'ye sürükle.
- [ ] Daha güçlü deprem, daha çok ve daha ağır mobilya (gardırop, buzdolabı dahil), biraz daha büyük bütçe, karmaşık ağırlık dağılımı.
- [ ] Mağaza bu bölümde ilk kez açılır; kısa ipucu: "Yeni eşya alabilirsin — ama her eşya dengeyi etkiler!"
- [ ] Radyo uyarısı metni bölüme özel.
- [ ] Bölüm sonu → `Level3`. Kazanılabilir ama düşündürücü olsun; baştan sona oyna, PR aç.

## Claude Code'a ilk mesaj (kopyala-yapıştır)

```text
I own Level2 in Son Denge, a 48-hour Unity 2D game jam. I have no game-dev experience; explain Unity steps simply.
Read docs/ZORUNLU_KURALLAR.md (mandatory), CLAUDE.md, docs/OYUN_TASARIMI.md, docs/TEAM_PLAN.md,
docs/API_CONTRACTS.md and docs/tasks/KISI2_LEVEL2.md.
First task: create a FurnitureData ScriptableObject and the 6 furniture assets with the exact values in my task file
(in Assets/Data/Furniture/). Then build a simple shop panel in Assets/Scripts/Shop/ for buying furniture during
Preparation. Work only in my folders. All visible text in Turkish.
Commit and push after each working step. Before coding, list the files you will create.
After each step, tell me exactly how to test it in Unity.
```
