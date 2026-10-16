# Kişi 4 · `Level4` "Büyük Sınav" + Görseller · branch `kisi4-level4`

Önce oku: **`docs/ZORUNLU_KURALLAR.md`**, `CLAUDE.md`, `docs/OYUN_TASARIMI.md` (özellikle §2 Görsel, §14 Eğitim), `docs/TEAM_PLAN.md`, `docs/API_CONTRACTS.md`.
Senin alanın: `Assets/Art/` (FX hariç), `Assets/Scripts/Awareness/`, `Assets/Scripts/Disasters/Storm/` (opsiyonel), `Level4.unity`, `Assets/Levels/Level4/`.
Denemeler: `Assets/_Sandbox/kisi4/` (git'e gitmez).

---

## 2–12. saat — Görseller + eğitici mesajlar (herkes kullanacak)

- [ ] **3D modeller ve materyaller** (`Assets/Art/`), stilize low-poly, OYUN_TASARIMI §0 ve §2 paletine göre (krem, tozlu turuncu, zeytin yeşili, teal, ahşap kahve, gök mavisi, sarı vurgu):
  - Sivri kayalık dağ zirvesi; 2 katlı **kesit** ahşap ev (her katta 2 oda, ön duvar yok, çatı, görünür pivot) — ev 4 m × 3 m odalar, 1 birim = 1 m
  - Arka plan: uzak dağlar, köy, bulutlar (basit modeller veya arka plan düzlemi)
  - 6 mobilya: sandalye, masa, kanepe, kitaplık, gardırop, buzdolabı (net silüet, ≤ 5k üçgen, pivot tabanda)
  - Çapa (sabitleme) ikonu (UI), kırık eşya varyantı, korunma pozisyonundaki ev sakini
  - Kaynak: CC0 paketler (Kenney, Quaternius, Poly Pizza) veya MCP ile 3D model üretimi; kaynağı README "Atıflar"a yaz. Materyal: URP/Lit, doku ≤ 2048 px.
  - Her model için `Assets/Prefabs/` yerine kendi `Assets/Art/Models/` altında prefab; Lead bunları mobilya prefab'larına takar.
- [ ] **Eğitici mesajlar** (`Scripts/Awareness/`): oyuncunun gerçek eylemine bağlı kısa Türkçe mesajlar (OYUN_TASARIMI §9 örnekleri) + yükleme/ipucu metinleri. AFAD kaynaklarıyla kontrol et. Deprem sırasında eşya taşımayı veya koşmayı **asla** öğretme.

## 12–30. saat — `Level4` "Büyük Sınav"

- [ ] `LevelTemplate` prefab'ını `Level4.unity`'ye sürükle.
- [ ] **3. kat** eklenir (bu bölüme özel ev varyantı `Assets/Levels/Level4/`) → denge anlamlı şekilde zorlaşır ama imkânsız olmaz.
- [ ] Şiddetli deprem, en çok mobilya, kısıtlı bütçe.
- [ ] Opsiyonel (sadece her şey çalışıyorsa): fırtına — yön değiştiren yanal rüzgâr, yağmur/şimşek.
- [ ] Final: AFET RAPORU + toplam skor + **"AFETE HAZIRLIK HAYAT KURTARIR."** + "ANA MENÜ" butonu → `Menu`.
- [ ] Baştan sona oyna, kazanılabilir olduğundan emin ol, PR aç.

## Claude Code'a ilk mesaj (kopyala-yapıştır)

```text
I own Level4 and the art in Son Denge, a 48-hour Unity 3D game jam (fixed side-view camera, see OYUN_TASARIMI §0). I have no game-dev experience; explain Unity steps simply.
Read docs/ZORUNLU_KURALLAR.md (mandatory), CLAUDE.md, docs/OYUN_TASARIMI.md (§2 visuals, §14 education),
docs/TEAM_PLAN.md, docs/API_CONTRACTS.md and docs/tasks/KISI4_LEVEL4.md.
First help me get and import stylized low-poly 3D art in Assets/Art/: a sharp rocky peak, a 2-story cutaway wooden
house (2 rooms per floor, no front wall, roof, visible pivot; 1 unit = 1 m, rooms ~4 m x 3 m), a mountain/village/cloud
background and 6 furniture models (<= 5k triangles, pivot at base), with URP/Lit materials in the OYUN_TASARIMI §2
palette. Use CC0 asset packs or MCP 3D model generation, and record sources for the README credits.
Then write short Turkish awareness messages in Assets/Scripts/Awareness/ that depend on the player's actual actions.
Work only in my folders. Commit and push after each working step. Before coding, list the files you will create.
After each step, tell me exactly how to test it in Unity.
```
