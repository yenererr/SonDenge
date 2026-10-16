# Takım Planı — Önce Ortak Çekirdek, Sonra Bölüm Bölüm

Hepimiz oyunu AI (Claude Code + Unity MCP) ile yapıyoruz. Bu yüzden iş, AI'a net tarif edilebilecek parçalara bölündü:
önce herkesin kullanacağı **ortak parçalar (prefab)** yapılır, sonra herkes bu parçalarla **kendi bölümünü (sahnesini)** kurar.

Kendi görev dosyan: `docs/tasks/`. **Zorunlu kurallar: [ZORUNLU_KURALLAR.md](ZORUNLU_KURALLAR.md).**

## Kim, hangi branch

| Kişi | Branch | Aşama 1 (çekirdek) | Aşama 2 (bölüm) |
|---|---|---|---|
| Kişi 1 — Lead | `kisi1-lead` | Oyun akışı, mobilya sürükleme, Furniture prefab | `Menu` + `Level1` (İlk Sarsıntı, öğretici) + bölümleri birleştirme |
| Kişi 2 | `kisi2-fizik` | Ev dengesi (eğim) + deprem | `Level2` (Artan Tehlike) |
| Kişi 3 | `kisi3-gorsel` | Görseller + HUD + paneller | `Level3` (Yeni Kat) |
| Kişi 4 | `kisi4-hazirlik` | Bütçe, sabitleme, skor, farkındalık mesajları, ses | `Level4` (Büyük Sınav, final) |

## Takvim

| Saat | Ne olur | Kontrol |
|---|---|---|
| 0–2 | Herkes projeyi açar, MCP'yi bağlar, `CLAUDE.md` + kendi görev dosyasını okur | Herkes Play'e basabiliyor |
| 2–10 | **Aşama 1:** herkes kendi çekirdek parçasını yapar, `CoreTest` / kendi sandbox'ında dener | Her parça tek başına çalışıyor |
| 10–12 | **Birleştirme 1:** herkes PR açar, Lead merge eder, `CoreTest`'te hepsi birlikte denenir | Mobilya taşıyınca ev eğiliyor, deprem olunca kayıyor |
| 12–14 | Herkes `git pull origin main` yapar → artık tüm prefab'lar herkeste | — |
| 14–30 | **Aşama 2:** herkes kendi `LevelX` sahnesini kurar | Her bölüm baştan sona oynanıyor |
| 30–32 | **Birleştirme 2:** bölümler merge edilir, Lead `Menu → Level1 → … → Level4` zincirini bağlar | Oyun baştan sona oynanıyor |
| 32–40 | Ayar, hata düzeltme, ses, metinler | Kritik hata yok |
| 40–44 | Özellik dondurma, Windows build, başka bilgisayarda test | Build açılıyor |
| 44–48 | Teslim, video, sunum | Teslim edildi |

**Kural:** 12. saatte çekirdek birlikte çalışmıyorsa herkes çekirdeğe yardım eder; bölümlere geçilmez.
**Kural:** 24. saatte en az `Level1` oynanabilir değilse yangın / yeni kat iptal, bölümler sadece deprem olur.

## Altın kurallar (AI'a da söyleyin)

1. **Herkes sadece kendi dosyalarına dokunur.** Bölüm sahnende (`LevelX.unity`) ve `Assets/Levels/LevelX/` klasöründe istediğini yap; başkasınınkine dokunma.
2. **Ortak prefab'ları bölümde değiştirme.** Sahnede ayar değiştirmek (override) serbest; prefab dosyasının kendisini değiştirmek gerekirse sahibine söyle.
3. **`Contracts.cs`, `ProjectSettings/`, `Packages/`** — sadece takım onayıyla.
4. **Küçük ve sık commit + push.** Her gün en az birkaç kez `git pull origin main`.
5. **İki AI aynı Unity'ye bağlanmaz.** Herkes kendi bilgisayarında kendi Unity'si + kendi MCP'si.
6. Bölüm sonu: `SceneLoader.Instance.Load("Level2")` gibi sonraki sahneye geçilir (Lead bağlar).

## Bölüm sahnesi nasıl kurulur (Aşama 2, herkes için aynı)

1. `Assets/Scenes/LevelX.unity`'yi aç.
2. Ortak prefab'ları sürükle: `House`, mobilyalar, `HUD`, `LevelSetup` (Aşama 1'de oluşacak).
3. `LevelSetup` üzerinde bölüm ayarlarını yap: bütçe, deprem şiddeti/süresi, hazırlık süresi, hangi mobilyalar, sonraki sahne.
4. Bölüme özel her şeyi (yeni görsel, yeni script) `Assets/Levels/LevelX/` içine koy.
5. Play'e bas, baştan sona oyna, PR aç.

## Her gün kullanılacak git komutları

```bash
git pull origin main          # main'deki yenilikleri al
git add .
git commit -m "ne yaptım"
git push                      # kendi branch'ine gönder
```
Bir parça bitince GitHub'da kendi branch'inden `main`'e **Pull Request** aç.
