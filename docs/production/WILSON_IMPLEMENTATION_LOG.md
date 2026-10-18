# Wilson 5 Bölüm — Uygulama Günlüğü

## FAZ 0 — Adli Envanter / Baseline
- Tarih / branch / HEAD: 2026-10-18 / `feature/wilson-missions-five-levels` / `c7639d6` (yedek etiketi `backup/pre-wilson-5level`)
- Amaç: Repo, dal, sahne, asset ve MCP durumunu belgelemek.
- Dokunulan dosyalar: `docs/production/PHASE_00_AUDIT.md`, bu dosya.
- Oluşturulan prefab / sahne / ScriptableObject: yok.
- Tamamlanan işler: 5 dal karşılaştırıldı; sahne/build listesi, Level1 kod ve asset envanteri çıkarıldı; teknik borç listelendi.
- Unity açıldı mı / versiyon: Evet, 6000.3.26f1, Level1 açık. MCP (`UnityMCP`) bağlı, `execute_code` / `read_console` / `refresh_unity` çalışıyor.
- Compile: PASS — `read_console` error=0, warning=0.
- EditMode / PlayMode testleri: NOT_RUN — projede test assembly'si yok.
- Manuel Play testi: NOT_RUN — `runInBackground=0`, Editor arka plandayken Play frame 2'de donuyor (gözlemlendi).
- Sahne/prefab missing reference: NOT_RUN.
- Görsel doğrulama: NOT_RUN.
- Git status / commit hash: bu fazın commit'i aşağıda.
- Açık risk ve çözümü: Sahiplik (Level2–4), Level5/build listesi, `runInBackground` ve test paketi kullanıcı/takım onayı bekliyor (bkz. PHASE_00_AUDIT §7).
- Kapı kararı: **PASS (denetim)** / ilerleyen fazlar için kısmi **BLOCKED**.
- Sonraki faz: FAZ 1 — engeller cevaplanınca.

## FAZ 1 — Mimari göç, branch ve kararlar
- Tarih / branch / HEAD: 2026-10-18 / `feature/wilson-missions-five-levels` / `e710c0a`
- Amaç: Sahiplik ve mimari kararlarını kayda geçirmek.
- Kullanıcı (Lead) kararları: Level2–5 sahneleri bu feature dalında Level1 şablonundan kurulacak (Kişi 2/3/4 dallarına dokunulmaz, birleştirme PR ile). `runInBackground` açıldı. Level5'in build listesine eklenmesi ve Test Framework kurulumu **onaylanmadı** → Level5 sahnesi oluşturulacak ama build listesine eklenmeyecek; testler MCP `execute_code` ile yapılacak.
- Mimari: `L1_ProjectGameController` beş sahnenin ortak oynanış motoru olarak kalır (her sahnede bölüm numarası farklı). Görev/veri/kayıt/puan yeni ortak katmanda: `Assets/Scripts/Missions` (`Jam.Missions`). Metinler `Assets/Data/{Levels,Missions,Wilson}` ScriptableObject'lerinde.
- Dokunulan dosyalar: `ProjectSettings/ProjectSettings.asset` (yalnız `runInBackground: 1`).
- Compile: PASS. Kapı kararı: PASS.

## FAZ 2 — Ortak Mission + Level çekirdeği
- Dokunulan dosyalar: `Assets/Scripts/Missions/{MissionTypes,TaskDefinition,WilsonDialogueSet,LevelDefinition,MissionManager,MissionScoring,LevelProgressSave}.cs`, `Assets/Editor/WilsonContentBuilder.cs`.
- Oluşturulan ScriptableObject: 5 `LevelDefinition` (`Assets/Data/Levels/L1_Deprem…L5_Yagmur`), 19 `TaskDefinition` (`Assets/Data/Missions/L1_T01…L5_T04`), 5 `WilsonDialogueSet` (`Assets/Data/Wilson/L1…L5_Dialogue`). Üretici menü: `Jam > Wilson > Bölüm verilerini oluştur`.
- Veri doğrulaması (MCP): 5 bölüm; süreler 90/100/120/110/140 – 12/12/30/25/25; kat 2/2/3/3/3; görev 4+4+4+3+4 = **19**; tekrar eden ID yok.
- Compile: PASS — Console error 0.
- Mantık testleri (MCP `execute_code`, EditMode eşdeğeri): **21/21 PASS** — bilinmeyen ID reddi, idempotent tamamlama (tek olay), hazırlık puanı 35 normalize, kritik görev kontrolü, koruma anahtarı, afette Missed, kilit sonrası tamamlanmama, reset, ön koşul, yıldız eşikleri (0/1/2@70/3@90+kritik), denge puanı, toplam clamp.
- PlayMode testi: NOT_RUN (henüz sahneye bağlanmadı).
- Kapı kararı: PASS. Sonraki faz: FAZ 3.

## FAZ 3 — Wilson anlatıcı ve briefing modalı
- Dokunulan dosyalar: `Assets/Scripts/UI/{WilsonPortrait,WilsonBriefingPanel,MissionHudPanel,DisasterResultPanel}.cs`, `L1_WilsonChicken.cs`, `L1_ProjectGameController.Wilson.cs`.
- Wilson modelleri Unity Editor içinden (`AssetDatabase.MoveAsset`) `Assets/Resources/Wilson` → `Assets/Levels/Level1/Art/Characters/Wilson` taşındı; GUID'ler aynı (chicken obj `9f821595…`, png `5f8f092e…`, egg `683b928c…`). `Resources.Load` kaldırıldı; model/doku Inspector referansı. `Assets/Resources` boş kaldığı için silindi.
- Portre: mevcut 3D tavuğun kütlesiz kopyası, sahneden uzakta (y=-400) yardımcı kamera → RenderTexture. Kamera MainCamera değil, AudioListener yok.
- Yumurta: en fazla 2; altın yumurta para vermez (yalnız 5 kg denge yükü).
- Manuel Play (Unity öne alınarak, gerçek fare/klavye): **PASS** — 5 sayfa (Giriş×2, Neden önemli, 4 görev kartı, Kontroller, Hazır mısın) sırayla, daktilo efekti, GERİ/İLERİ/ATLA/GÖREVLERE BAŞLA çalışıyor. Briefing açıkken `timeScale=0`, hazırlık süresi 89,96 sn'de sabit kaldı (frame 1863'te ölçüldü). Kamera sayısı 2 (ana + portre), MainCamera 1, AudioListener 1, EventSystem 1, dünya Wilson'ı 1.
- Kapı: PASS.

## FAZ 4 — Fiziksel etkileşim / taşıma / görev hedefleri
- Yeni: `L1_MissionZone` (yerel kutu + parlayan işaret), `L1_MissionSwitch` (E noktası; zincirleme ön koşul, montajda eşyayı tüketme, tehlike işareti), `L1_MissionTask` (Anchored / ItemsInZone / Switches / Balance / ZoneClear), `L1_MissionDirector`.
- `L1_PropDrag`: E yalnız `carryable` eşyaları alır; yakında görev noktası varsa E onu kullanır (tek giriş sözleşmesi); taşınabilir eşyalar fareyle kat atlatılamaz. **Hata düzeltmesi:** bırakılan eşya evin dışına düşüyordu (z=-1,55) → bırakma konumu 2 fizik adımı sabitleniyor.
- Denge: taşınan kütle tam ve bir kez sayılır; 26°+1 sn kuralı görev modunda 5 bölümde ve hazırlıkta etkin; Level≥3 eğim kısıtı görev modunda kaldırıldı.
- **Hata düzeltmesi:** görünmez yan duvarlar simetrik ±5,55'teydi, katların zemini asimetrik (Kat1 −5,8…4,8; Kat2 −4,77…5,87) → eşyalar duvara varmadan düşüyordu. Duvarlar artık her katın zemin kenarında.
- Ayar (görev sahneleri): tork hassasiyeti 0,0057 → 0,002 (karakter uçtan uca ~14°; eski değerde tek başına 23° sınırına dayanıyordu ve 7° görevi yapılamıyordu). Hazırlıkta kayma yalnız 8° üstünde, 0,06 m/sn.
- Kapı: PASS (aşağıdaki Level1 testi ile).

## FAZ 5 — Level 1: Deprem vertical slice
- Sahne kurulumu: `Jam > Wilson > Level1 görevlerini kur` (`Assets/Editor/WilsonSceneBuilder.cs`). Low-poly deprem çantası, `EmergencyBagZone`, 4 `L1_MissionTask`. Materyaller `Assets/Levels/Level1/Materials/Missions/`.
- Manuel Play (gerçek girdi): **PASS**
  1. Briefing görünür, süre durur. ✓
  2. ATLA → GÖREVLERE BAŞLA; HUD "Görev 0/4", "Hazırlık 87 sn", görev listesi, İPUCU/TEKRAR DİNLE. ✓ Altın simgesi ve mağaza/sat düğmeleri gizli. ✓
  3. E ile çanta alındı, D ile taşındı, E ile hedefe bırakıldı → L1_T03 ✓. Fare ile buzdolabı ve kitaplık seçilip F ile sabitlendi (bütçe değişmedi) → L1_T02, L1_T01 ✓. Karakter merkeze dönünce eğim 6,5° → L1_T04 ✓ (4/4).
  4. BOŞLUK → 3-2-1 → DEPREM 12 sn; BOŞLUK ile Çök–Kapan–Tutun. ✓
  5. Rapor: 83/100 (hazırlık 35, davranış 35, denge 3, koruma 10), **2 yıldız**, kayıt L1=2, L2 açıldı. ✓ Wilson ipucu: "Denge çok bozuldu!" uyarısı görüldü.
  6. Süre bitişi (zamanlayıcı 0,3 sn'ye çekilerek hızlandırıldı): onaysız 3-2-1 ile afet başladı, 4 görev Missed (×). ✓
  7. 26° üstü zorlandı → "DENGE BOZULDU", 0 yıldız, nedenler; en iyi kayıt 2 yıldız korundu; TEKRAR DENE kısa brifingle yeniden açtı. ✓
  8. Eski IMGUI/uGUI raporu görev modunda kapatıldı (ilk denemede iki rapor üst üste çıkıyordu — düzeltildi).
- Ekran görüntüsü: `docs/production/screenshots/L1_fail_report.jpg`.
- Bilinen: Deprem sarsıntısı (quakeTiltPerAcceleration) maks. eğimi ~23°'ye çıkarıyor → denge puanı düşük (3/20). Ayar gerekebilir.
- Kapı kararı: PASS. Sonraki faz: FAZ 6–9.

## FAZ 6–9 — Level2 (Fırtına), Level3 (Yangın), Level4 (Yıldırım), Level5 (Şiddetli yağmur)
- Üretim: `Jam > Wilson > Level2-5 sahnelerini Level1'den üret`. Level1 açılır, bölüm n görevleri kurulur, `LevelN.unity` olarak kaydedilir. Level2–4'ün mevcut sahne GUID'leri korundu (`3d5653c4…`, `c1c81bfd…`, `ff571958…`); `Level5.unity` yeni (`546315ad…`). Kişi 2/3/4 dallarına dokunulmadı (Lead kararı).
- Bölüm verisi: L2 2 kat/100+12 sn, L3–L5 3 kat (3. kat + merdiven açılır, kamera yukarı kayar), 120+30 / 110+25 / 140+25 sn.
- Görev nesneleri (low-poly, Unity primitive birleşimi + URP Lit materyal): çatı bağlantısı, kepenkli 3 pencere, 2 balkon saksısı, el feneri, duman alarmı (kur + test), ocak düğmesi, söndürücü (LowPolyInteriors prefab), 2 koli + kaçış yolu hacmi, yıldırım kontrol kutusu, 2 fiş, dekor bilgisayar, gerilim koruyucu, 2 kum torbası, drenaj ızgarası, radyo + albüm, conta kiti + sızıntı.
- Play testleri (Unity öne alındı; bir kısım görev yerleştirmesi test hızında kodla, diğerleri gerçek girdiyle):
  - **L2 PASS** — brifing "DAĞ FIRTINASI — FIRTINA"; pencere gerçek **E** ile kapandı; 4/4 görev; fırtına; rapor 48/100, 1 yıldız (pencere dibinde kalındığı için davranış 0 — doğru ölçüldü); L3 açıldı.
  - **L3 PASS** — alarm testi kurulumdan önce reddedildi, kurulunca taşınan alarm monte görsele dönüştü; 4/4; tek küçük tutuşma; süre bitince açık kaçış yolundan **tahliye** başarı: 95/100, 3 yıldız.
  - **L4 PASS** — TV fişi gerçek **E** ile çekildi; afet başladıktan sonra bilgisayar fişi **çekilemedi** (kilit) ve görev Missed; paratoner 2 darbeyi engelledi; 53/100, 1 yıldız.
  - **L5 PASS** — kit sızıntıya uygulanınca tüketildi; pompa önceden kurulu/otomatik (oyuncu dokunmaz); 4/4; su içeri sızmadı (indoorFlood 0); final raporu "YENİDEN OYNA" + beş bölüm özeti + Wilson final cümlesi.
- Testlerde bulunan ve düzeltilen: radyo satırları ("Ağır eşyaları rüzgârın tersine al!", "Paratoner kur!") gerçek güvenlikle çelişiyordu → güvenli metinler; "El feneri kesintide kullanılabildi" (olay yok) → "kolay erişilebilir yerdeydi"; gerilim koruması yalnız darbe geldiyse rapora yazılır; AK STUDIO söndürücü URP'de pembe → LowPolyInteriors; HUD başlığı taşıyordu → "BÖLÜM 03 · YANGIN"; 3 katta kamera üst katı kesiyordu → kadraj kaydırıldı; final özeti Wilson satırıyla çakışıyordu → yerleşim; sandık araması ("sandık"≠"sandığı") düzeltildi.
- Kapı kararı: PASS (her bölüm Editor Play'de). Level4→Level5 sahne geçişi: **BLOCKED** — Level5 build listesinde değil (onay bekliyor).

## FAZ 10 — Skor, rapor, menü, kayıt
- `MainMenuUI`: OYNA → BÖLÜM SEÇ (5 bölüm, ●○ yıldız, KİLİTLİ), İLERLEMEYİ SIFIRLA; NASIL OYNANIR mağaza/altın yerine görev akışını anlatır; atıflara Wilson CC BY eklendi.
- Kayıt (`sondenge.v1.*`): en iyi yıldız korunur (L1: 2 yıldızdan sonra 0 yıldızlık deneme 2'yi silmedi), 1+ yıldız sonraki bölümü açar, sıfırlama sonrası yalnız Bölüm 1 açık — **PASS** (Play'de görüldü).
- Menü → Bölüm 1 → Wilson brifingi (SceneLoader ile) — **PASS**.
- Ekran görüntüleri: `docs/production/screenshots/` (Menu_level_select, L2_preparation, L3_wilson_briefing, L3_preparation_3floors, L3_report_3stars, L1_fail_report).
