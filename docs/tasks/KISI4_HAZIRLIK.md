# Kişi 4 — Hazırlık, Ekonomi & Farkındalık · branch `kisi4-hazirlik`

Önce oku: `CLAUDE.md`, `docs/TEAM_PLAN.md`, `docs/GDD.md`, `docs/API_CONTRACTS.md`.
Senin dosyaların: `Assets/Scripts/Preparation/`, `Economy/`, `Awareness/`, `Assets/Data/`, `Assets/Audio/`, `Level4.unity`, `Assets/Levels/Level4/`.
Denemeler için: `Assets/_Sandbox/kisi4/` içinde kendi test sahneni aç (git'e gitmez).

---

## Aşama 1 — Ortak çekirdek (0–12. saat)

- [ ] **`FurnitureData`** (ScriptableObject): id, ad, kütle, değer, kırılganlık, sabitleme bedeli.
- [ ] 4 veri asset'i (`Assets/Data/Furniture/`): dolap, koltuk, masa, kitaplık. **Önce bunu yap** — Kişi 1 ve 2 kullanacak.
- [ ] **`Furniture` bileşeni**: `IFurnitureItem` uygular, `FurnitureData` okur, `SetAnchored` ile sabitlenir.
- [ ] **`Wallet`**: başlangıç bütçesi, harcama, `OnBudgetChanged(int)` event'i.
- [ ] **`PreparationManager`** (`IPreparationService`): `TryPurchaseAnchoring` / `TryRemoveAnchoring` — **yalnızca Preparation fazında** (`GameManager.Instance.Phase`). Para yetmezse sabitleme olmaz.
- [ ] **`ScoreCalculator`**: tur sonu skor. Öncelik: insan güvenliği > ev ayakta mı > kurtarılan eşya değeri.
- [ ] **Farkındalık mesajları** (Türkçe, en az 3): mobilyaları duvara sabitle, ağır eşyaları alt raflara/katlara koy, deprem anında Çök–Kapan–Tutun. Oyundaki "eşya taşıma gücü"nün gerçek deprem davranışı **olmadığını** açıkça söyle. (AFAD sitesinden kontrol et.)
- [ ] **Ses** (`Assets/Audio/`): deprem gürültüsü, eşya düşme, buton, müzik — `AudioManager.Instance.PlaySfx(clip)`. Kaynağı README "Atıflar"a yaz.

Kontrol: sabitleme bütçeden düşüyor; afet sırasında sabitleme yapılamıyor; skor güvenliği eşya toplamaktan üstün tutuyor.

## Aşama 2 — Bölüm: `Level4` "Büyük Sınav" (14–30. saat)

- [ ] `LevelTemplate` prefab'ını `Level4.unity`'ye sürükle.
- [ ] Final: en güçlü deprem, en çok mobilya, kısıtlı bütçe — doğru hazırlanan kazanır.
- [ ] Bitişte başarı / başarısızlık metni (GDD §2) ve toplam skor; "Ana Menü" butonu → `Menu`.
- [ ] Baştan sona oyna, kazanılabilir olduğundan emin ol, PR aç.

## Claude Code'a ilk mesaj (kopyala-yapıştır)

```text
I own preparation, economy and awareness in Son Denge, a 48-hour Unity 2D game jam. I have no game-dev experience; explain Unity steps simply.
Read CLAUDE.md, docs/TEAM_PLAN.md, docs/GDD.md, docs/API_CONTRACTS.md and docs/tasks/KISI4_HAZIRLIK.md.
Start with Phase 1: create a FurnitureData ScriptableObject and 4 assets (wardrobe, sofa, table, bookshelf),
a Furniture component implementing IFurnitureItem from Assets/Scripts/Core/Contracts.cs, a Wallet with OnBudgetChanged,
a PreparationManager (IPreparationService) that only allows anchoring during GamePhase.Preparation, and a ScoreCalculator
that rewards human safety over item value. Add at least 3 short Turkish awareness messages that teach pre-disaster
mitigation and say fantasy furniture movement is not real earthquake advice. All numbers must be Inspector-configurable.
Work only in Scripts/Preparation/, Economy/, Awareness/, Data/ and a test scene in Assets/_Sandbox/kisi4/.
Before coding, list the files you will create. After coding, tell me exactly how to test it in Unity.
```
