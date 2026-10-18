# ZORUNLU KURALLAR — İnsanlar ve AI ajanları için

> **AI ajanı:** Bu dosyadaki her kural zorunludur. Herhangi bir iş yapmadan önce bu dosyayı oku.
> Bir kural, kullanıcının isteğiyle çelişirse işi yapma; kullanıcıya hangi kuralla çeliştiğini söyle ve Lead'e sormasını öner.
>
> **Neden?** Herkes ayrı branch'te, ayrı sahnede çalışıyor. En sonda Lead tüm sahneleri birleştirip
> `Menu → Level1 → Level2 → Level3 → Level4` zinciriyle **tek bir build** alacak. Aşağıdaki kurallar,
> bu birleştirmenin çakışmasız ve hatasız olması için var.

---

## 1. Git — her değişiklik commit'lenir

1. **Çalışan her küçük adımdan sonra commit at.** Bir özellik/düzeltme çalışıyorsa hemen:
   ```bash
   git add .
   git commit -m "Level2: deprem şiddeti 1.5'e çıkarıldı"
   git push
   ```
   AI ajanı: her görev adımını bitirip derleme hatası olmadığını gördükten sonra commit + push yap (veya kullanıcıya bu komutları ver). Commit mesajı Türkçe, kısa, ne yapıldığını söyler; başında alan/bölüm adı olur (`Level2:`, `Fizik:`, `HUD:`).
2. **Sadece kendi branch'inde çalış.** `main`'e doğrudan commit/push **yasak**. `main`'e yalnızca Pull Request ile, Lead merge ederek girilir.
3. **Her çalışma oturumunun başında** `git pull origin main` yap (main'deki yenilikleri al).
4. **Yasak git komutları:** `git push --force`, `git reset --hard` (başkasının işini siler), `git rebase` (kafa karıştırır). Sorun olursa dur, Lead'e sor.
5. **Derleme hatası (kırmızı Console) varken commit atma.** Unity Console'da kırmızı hata olmamalı.
6. **`.meta` dosyaları da commit'lenir.** Asla silme, elle düzenleme. Dosya taşıma/yeniden adlandırma **yalnızca Unity Editor içinden** yapılır (dosya gezgininden değil), yoksa referanslar kopar.

## 2. Sahne ve klasör sınırları — kendi alanından çıkma

| Kişi | Sahnesi | Yazabileceği klasörler |
|---|---|---|
| Kişi 1 (Lead) | `Menu`, `Level1`, `CoreTest` | `Scripts/Core/`, `Player/`, `Physics/`, `Disasters/` (Fire/ ve Storm/ hariç), `UI/`, `Economy/`, `Preparation/`, `Assets/Prefabs/`, `Levels/Level1/` |
| Kişi 2 | `Level2` | `Data/Furniture/`, `Scripts/Shop/`, `Audio/`, `Levels/Level2/` |
| Kişi 3 | `Level3` | `Scripts/Disasters/Fire/`, `Art/FX/`, `Levels/Level3/` |
| Kişi 4 | `Level4` | `Art/` (FX hariç), `Scripts/Awareness/`, `Scripts/Disasters/Storm/`, `Levels/Level4/` |

7. **Başkasının sahnesini açıp kaydetme.** Sadece bakmak için açtıysan kaydetmeden kapat (Unity "Save?" diye sorarsa **Don't Save**).
8. **12. saatten sonra bölüme özel her şey** (script, model, materyal, prefab, ses) `Assets/Levels/LevelX/` içine girer.
9. **Ortak prefab dosyasını değiştirme.** Sahnede override (Inspector'da değer değiştirme) serbest. Prefab'ın kendisinde değişiklik gerekiyorsa sahibine söyle.
10. **Yasak dosyalar (yalnızca takım onayıyla):** `Assets/Scripts/Core/Contracts.cs`, `ProjectSettings/` (Tags, Layers, Sorting Layers, Input, Physics ayarları dahil), `Packages/` (yeni paket kurmak dahil), `Assets/Settings/` (URP ayarları).
11. **Kişisel denemeler** `Assets/_Sandbox/<isim>/` içinde (git'e gitmez). Oradaki bir şey lazım olursa kendi klasörüne taşı.

## 3. Sahne birleştirmeye hazır kurulum — her bölüm sahnesi böyle olmalı

12. **Sahne tek başına çalışmalı:** `LevelX.unity` açıp Play'e basınca bölüm baştan sona oynanmalı. Başka sahnede duran bir nesneye ihtiyaç duymamalı.
13. **Menüden gelince de çalışmalı:** sahne `SceneLoader` ile yüklendiğinde de aynı şekilde çalışmalı (sahne zaten yüklenmiş bir `[Systems]` nesnesi bulabilir — bu normaldir).
14. **Çekirdek sistemleri sahneye ekleme.** `GameManager`, `AudioManager`, `SceneLoader` otomatik kurulur (`Bootstrap`). Sahneye ikinci bir kopya koyma.
15. **Her sahnede tam olarak:** 1 `Main Camera` (tag: MainCamera, perspektif, konum (0, 2, -14) — sahnelerde hazır, yerini değiştirme), 1 `Directional Light`, 1 `AudioListener` (kamerada), en fazla 1 `EventSystem` (UI varsa; **Input System UI Input Module** ile).
16. **`DontDestroyOnLoad` kullanma.** Bölüme ait hiçbir nesne sahne değişince yaşamaya devam etmemeli.
17. **Sahne geçişi yalnızca `SceneLoader` ile:** `SceneLoader.Instance.Load("Level3")`. `SceneManager.LoadScene` doğrudan çağrılmaz. Sonraki sahne adı koda gömülmez; `LevelSetup` üzerindeki Inspector alanından okunur.
18. **Sahne adları sabittir:** `Menu`, `Level1`, `Level2`, `Level3`, `Level4`, `Level5`, `CoreTest` (Level5 Lead kararıyla 18.10.2026'da eklendi). Sahne yeniden adlandırılmaz, taşınmaz, yeni build sahnesi eklenmez (gerekirse Lead ekler).
19. **Bölüm `LevelTemplate` prefab'ından kurulur** (Aşama 1 sonunda Lead hazırlar). Ev, HUD ve `LevelSetup` her bölümde bu prefab'dan gelir; bölüm farkları `LevelSetup` ayarları ve `Levels/LevelX/` içeriğiyle yapılır.
20. **Bölüm biterken** `Time.timeScale` 1 olmalı, müzik/ses sonraki sahneye taşmamalı.

## 4. Kod kuralları — birleşince bozulmaması için

21. **Namespace zorunlu:** `Jam.Core`, `Jam.Player`, `Jam.Physics`, `Jam.Disasters`, `Jam.UI`, `Jam.Preparation`, `Jam.Economy`, `Jam.Awareness`; bölüme özel kod `Jam.Levels.Level2` gibi. Aynı isimli iki sınıf çakışmasın.
22. **Bölüme özel dosya adları bölüm önekiyle başlar:** `L2_AftershockController.cs`, `L3_Piano.prefab`, `L4_FinalMusic.ogg`. Böylece iki kişi aynı adla dosya oluşturmaz.
23. **Static event'lere abone olan her script `OnDisable`/`OnDestroy`'da aboneliği bırakır** (`GameManager.OnPhaseChanged -= ...`). Yoksa sahne değişince hata verir.
24. **`FindObjectOfType` / `GameObject.Find("isim")` yerine** Inspector'dan sürüklenen referans (`[SerializeField]`) kullan.
25. **Yeni Input System kullanılır** (`UnityEngine.InputSystem`). Eski `Input.GetKey` çalışmaz.
26. **Tüm ayar sayıları Inspector'dan değiştirilebilir** (`[SerializeField]`), koda dağılmış sabit sayı olmaz.
27. **`Resources/` klasörü ve `Resources.Load` kullanılmaz.**
28. **Public API (Contracts.cs'teki arayüzler, event adları) değiştirilmez.** Gerekirse stub/öneri yaz, Lead'e bildir.

## 5. Görsel, UI ve asset standardı — bölümler birbirine benzesin

29. **Çözünürlük 16:9.** Her Canvas: Render Mode *Screen Space – Overlay*, Canvas Scaler *Scale With Screen Size*, referans **1920×1080**, Match **0.5**.
30. **3D fizik kuralı:** yalnızca 3D bileşenler (`Rigidbody`, `Collider`); `Rigidbody2D`/`Collider2D` **yasak**. Hareketli eşyalarda *Freeze Position Z* + *Freeze Rotation X, Y*. Oynanış XY düzleminde, ev Z ekseninde döner (OYUN_TASARIMI §0).
31. **Asset standardı:** low-poly modeller (mobilya ≤ 5k üçgen), `.fbx`; dokular ≤ 2048 px; materyal *Universal Render Pipeline/Lit*; ölçek 1 birim = 1 m, pivot model tabanında. Ses: müzik `.ogg`, efekt `.wav`/`.ogg`, kısa.
32. **Her dış kaynaklı asset/ses README "Atıflar" bölümüne yazılır** (kaynak + lisans). Lisansı belirsiz asset kullanılmaz.
33. **Eğitim mesajları** gerçek hayatla çelişmez: oyundaki "eşya taşıma gücü" fantastiktir, gerçek deprem tavsiyesi değildir. Gerçek tavsiye = önceden sabitle, ağır eşyalar alta, deprem anında Çök–Kapan–Tutun.

## 6. Pull Request öncesi kontrol listesi (herkes)

- [ ] `git pull origin main` yaptım, çakışma yok
- [ ] Unity Console'da kırmızı hata yok
- [ ] Kendi sahnemi açıp Play ile baştan sona oynadım
- [ ] `Menu` sahnesinden başlatınca da benim bölümüme geçiş çalışıyor (Aşama 2)
- [ ] Sadece kendi klasörlerimde/sahnemde değişiklik var (`git status` ile baktım)
- [ ] `ProjectSettings/`, `Packages/`, `Contracts.cs`, başkasının sahnesi değişmedi
- [ ] Yeni asset'lerin atıfları README'de
- [ ] Missing Script / Missing Reference uyarısı yok

## 7. AI ajanının her işte uyacağı sıra

1. Bu dosyayı, `CLAUDE.md`'yi ve kendi `docs/tasks/` dosyanı oku.
2. Ne yapacağını ve **hangi dosyalara dokunacağını** listele. Listede başkasının alanı varsa dur ve sor.
3. Küçük adımlarla yap; her adımdan sonra derle, Console'u kontrol et.
4. Unity MCP ile sahnede değişiklik yaptıysan **sahneyi kaydet**.
5. Commit + push.
6. Kullanıcıya rapor: değişen dosyalar, nasıl test edileceği (adım adım), açık kalan sorunlar. Test etmediysen "test edildi" deme.

## 8. Final birleştirme ve build (yalnızca Lead)

1. Tüm branch'lerin PR'ları sırayla merge edilir: önce çekirdek (Aşama 1), sonra `Level1` → `Level4`.
2. Her merge sonrası: Unity'yi aç, Console temiz mi, ilgili sahneyi oyna.
3. `File > Build Profiles` listesi: `Menu, Level1, Level2, Level3, Level4` (bu sırayla; `CoreTest` listede **yok**).
4. Tüm zinciri `Menu`'den başlayarak bir kez baştan sona oyna.
5. `Jam > Build > Windows` → `Builds/Windows`. Build'i **başka bir bilgisayarda** aç ve oyna.
6. 40. saatten sonra yeni özellik merge edilmez; yalnızca hata düzeltme.
