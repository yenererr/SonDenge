# Kişi 4 · `Level4` "Büyük Sınav" + Görseller · branch `kisi4-level4`

Önce oku: **`docs/ZORUNLU_KURALLAR.md`**, `CLAUDE.md`, `docs/OYUN_TASARIMI.md` (özellikle §2 Görsel, §14 Eğitim), `docs/TEAM_PLAN.md`, `docs/API_CONTRACTS.md`.
Senin alanın: `Assets/Art/` (FX hariç), `Assets/Scripts/Awareness/`, `Assets/Scripts/Disasters/Storm/` (opsiyonel), `Level4.unity`, `Assets/Levels/Level4/`.
Denemeler: `Assets/_Sandbox/kisi4/` (git'e gitmez).

---

## 2–12. saat — Görseller + eğitici mesajlar (herkes kullanacak)

- [ ] **Görseller** (`Assets/Art/`), OYUN_TASARIMI §2 paletine göre (krem, tozlu turuncu, zeytin yeşili, teal, ahşap kahve, gök mavisi, sarı vurgu):
  - Sivri kayalık dağ zirvesi, 2 katlı kesit ahşap ev (her katta 2 oda, çatı, görünür pivot)
  - Arka plan: dağlar, uzakta köy, bulutlar
  - 6 mobilya: sandalye, masa, kanepe, kitaplık, gardırop, buzdolabı (net silüet)
  - Çapa (sabitleme) ikonu, kırık eşya görünümü, korunma pozisyonundaki ev sakini
  - AI görsel üretimi veya lisanslı ücretsiz asset; kaynağı README "Atıflar"a yaz. Görsel ≤ 2048 px.
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
I own Level4 and the art in Son Denge, a 48-hour Unity 2D game jam. I have no game-dev experience; explain Unity steps simply.
Read docs/ZORUNLU_KURALLAR.md (mandatory), CLAUDE.md, docs/OYUN_TASARIMI.md (§2 visuals, §14 education),
docs/TEAM_PLAN.md, docs/API_CONTRACTS.md and docs/tasks/KISI4_LEVEL4.md.
First help me produce and import the 2D art in Assets/Art/: a sharp rocky peak, a 2-story cutaway wooden house
(2 rooms per floor, roof, visible pivot), a mountain/village/cloud background and 6 furniture sprites, using the
palette in OYUN_TASARIMI §2, 16:9, max 2048 px, imported as Sprite (2D and UI).
Then write short Turkish awareness messages in Assets/Scripts/Awareness/ that depend on the player's actual actions.
Work only in my folders. Commit and push after each working step. Before coding, list the files you will create.
After each step, tell me exactly how to test it in Unity.
```
