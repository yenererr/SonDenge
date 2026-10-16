# Takım Planı — Herkes Bir Bölüm, Ortak Parçaları Lead Kurar

Oyunun nasıl olacağı: **[OYUN_TASARIMI.md](OYUN_TASARIMI.md)** (ana referans).
Zorunlu kurallar: **[ZORUNLU_KURALLAR.md](ZORUNLU_KURALLAR.md)**. Kendi görev dosyan: `docs/tasks/`.

Hepimiz AI (Claude Code + Unity MCP) ile çalışıyoruz. Her kişi **bir bölüm sahnesinin** sahibi.
Dört bölüm de aynı ev / mobilya / denge / HUD / rapor sistemini kullanır; bu ortak parçaları **Lead `Level1`'i yaparken** kurar
ve prefab olarak paylaşır. Diğerleri o sırada kendi bölümlerine özel parçaları hazırlar.

## Bölümler ve sahipleri

| Sahne | Kim | Branch | Afet | İçerik |
|---|---|---|---|---|
| `Level1` "İlk Sarsıntı" | Kişi 1 — Lead | `kisi1-level1` | Orta deprem | Öğretici: 2 katlı ev, 4 oda, az mobilya, temel sabitleme, adım adım ipuçları |
| `Level2` "Artan Tehlike" | Kişi 2 | `kisi2-level2` | Güçlü deprem | Daha çok/ağır mobilya (gardırop, buzdolabı), biraz daha büyük bütçe, **mağaza** |
| `Level3` "Yangın" | Kişi 3 | `kisi3-level3` | **Yangın** | Odada başlayıp yayılan yangın; yanan eşyanın kütlesi gider → denge değişir; duman dedektörü / söndürücü / sprinkler |
| `Level4` "Büyük Sınav" | Kişi 4 | `kisi4-level4` | Şiddetli deprem (+ ops. fırtına) | **3. kat** eklenir, en zor bölüm, final raporu + "AFETE HAZIRLIK HAYAT KURTARIR." |
| `Menu` + bağlantılar + build | Lead | `kisi1-level1` | — | OYNA / NASIL OYNANIR / AYARLAR, `Menu → Level1 → … → Level4`, Windows build |

## Kim ne zaman ne yapar

| Saat | Kişi 1 — Lead | Kişi 2 | Kişi 3 | Kişi 4 |
|---|---|---|---|---|
| 0–2 | Proje + MCP kurulumu, herkes Play'e basabiliyor | aynı | aynı | aynı |
| 2–12 | **Ortak çekirdek** (`CoreTest`'te): dönen ev + tork, sürükleme, sabitleme, deprem, HUD, AFET RAPORU | 6 mobilyanın **verileri**, **mağaza**, **sesler** | **Yangın sistemi** + alev/duman efektleri (kendi sandbox'ında) | **Görseller** (ev, kaya, arka plan, 6 mobilya), **eğitici mesajlar** |
| 12 | **Birleştirme 1:** PR'lar merge edilir, `LevelTemplate` prefab'ı hazır | `git pull origin main` | `git pull origin main` | `git pull origin main` |
| 12–30 | `Level1` + `Menu` | `Level2` | `Level3` | `Level4` (3. kat) |
| 30–32 | **Birleştirme 2:** tüm bölümler merge, zincir bağlanır | PR | PR | PR |
| 32–40 | Hata düzeltme, ayar, ses, metin | aynı | aynı | aynı |
| 40–44 | Özellik dondurma, Windows build, başka bilgisayarda test | test | test | test |
| 44–48 | Teslim, video, sunum | | | |

**Kural:** 12. saatte ortak çekirdek çalışmıyorsa herkes çekirdeğe yardım eder (Lead'in söylediği parçada).
**Kural:** 24. saatte `Level1` oynanabilir değilse yangın ve fırtına iptal; `Level3` ve `Level4` deprem bölümü olur.
**Öncelik sırası (OYUN_TASARIMI §16):** denge → sürükleme/sabitleme → deprem → döngü → görsel/ses → ek afet/kat.

## Ortak parçalar (Lead kurar, herkes kullanır)
- Pivot etrafında dönen ev: tork, atalet, sönümleme, GÜVENLİ/DENGESİZ/KRİTİK/ÇÖKÜŞ
- Mobilya: sürükleme, hover bilgisi, sabitleme + çapa ikonu, kayma/devrilme/kırılma/düşme, düşen eşya dengeden çıkar
- Deprem sistemi (şiddet + süre ayarlı), afet enerji göstergesi
- HUD, "RADYO SİVRİTEPE" uyarısı, "AFET RAPORU" ekranı, ekonomi
- `LevelSetup` (bölüm ayarları: bütçe, afet türü/şiddeti/süresi, mobilya listesi, sonraki sahne) ve `LevelTemplate` prefab'ı

## Bölüm sahnesi nasıl kurulur (12. saatten sonra, herkes için aynı)
1. `Assets/Scenes/LevelX.unity`'yi aç, `LevelTemplate` prefab'ını sürükle.
2. `LevelSetup` üzerinde bölüm ayarlarını yap.
3. Bölüme özel her şeyi `Assets/Levels/LevelX/` içine koy (dosya adları `LX_` önekiyle).
4. Play ile baştan sona oyna, commit + push, PR aç.

## Her gün kullanılacak git komutları
```bash
git pull origin main          # main'deki yenilikleri al
git add .
git commit -m "Level2: ne yaptım"
git push                      # kendi branch'ine gönder
```
Bir parça bitince GitHub'da kendi branch'inden `main`'e **Pull Request** aç; Lead merge eder.
