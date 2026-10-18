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

## FAZ 11 — Entegrasyon ve build
- Lead onayıyla `EditorBuildSettings`: Menu, Level1, Level2, Level3, Level4, Level5 (CoreTest yok).
- `Jam > Build > Windows`: **Succeeded** — `Builds/Windows/Son Denge.exe`, 502 MB (Console: "[Jam] Build tamam").
- Build çalıştırıldı (1280×720 pencere): Menü → OYNA → BÖLÜM SEÇ (yeni kurulumda yalnız Bölüm 1 açık) → Bölüm 1 → Wilson brifingi (3D portre, Türkçe). `Player.log`: exception/error 0.
- NOT_RUN: build içinde afet akışı, Level4→Level5 geçişi, FPS ölçümü.
- Build sırasında Unity'nin `ProjectSettings.asset`'e yazdığı farklar (preloadedAssets, batching) commit'lenmedi.

## FAZ 12 — Belgeler
- `docs/production/FINAL_VALIDATION_REPORT.md`, `docs/production/SAFETY_REVIEW.md` (resmî kaynaklarla çevrim içi doğrulama yapılmadı, açıkça belirtildi), README "Wilson görev modu" bölümü, `ZORUNLU_KURALLAR.md` kural 18'e Level5 eklendi.

## Geri bildirim 1 — Deprem çantası sırta takılır (Lead isteği)
- Çanta artık düşük poligonlu gerçek bir sırt çantası (gövde, ön cep, kapak, askılar, sap, beyaz artı). Üstünde süzülen **"E" rozeti** var; rozet tüm görev noktalarında sarı kürenin yerini aldı.
- E ile alınınca çanta yerden kalkar ve **doğrudan karakterin sırtına takılır** (`EquipEmergencyBag`). Yan görünümde görünsün diye `L1_BackpackFollow` çantayı baktığı yönün tersindeki sırt tarafında tutar.
- Çantanın kütlesi (10 kg) karakterle birlikte dengeye girer: ölçülen karakter yükü 25 → 35 kg.
- L1_T03 artık "Çantanın yanına git, E ile sırtına tak". Şartnamedeki bölgeye bırakma kuralı Lead isteğiyle değiştirildi.
- Play (gerçek E/A/D girdisi): **PASS** — L1_T03 Completed, `bagAttached=True`. Ekran görüntüleri: `screenshots/L1_bag_E_badge.jpg`, `screenshots/L1_bag_on_back.jpg`.

## Geri bildirim 2 — Ortak AFET RAPORU şablonu (UI_DisasterReportTemplate)
- Kaynak görsel: `Dağ Günbatımında Oyun Sonuç Paneli.png` (1672×941, 16:9) → `Assets/Levels/Level1/Art/UI/L1_UI_DisasterReportTemplate_Source.png` (orijinal, değiştirilmedi).
- Şablon: `L1_UI_DisasterReportTemplate.png`. Görsele işlenmiş 1 altın + 2 gri yıldız, madde noktaları/çizgileri ve Wilson resmi, kartın kendi renkleriyle (sütun/satır geçişli dolgu + hafif bulanıklaştırma) temizlendi. Böylece dinamik UI üst üste binmiyor. Çerçeveler, kutular, düğme çerçeveleri ve üst orta denge çubuğu aynen kaldı.
- Yıldız sprite'ları üretildi: `L1_UI_Star_Active.png`, `L1_UI_Star_Inactive.png`.
- İçe aktarma (editör aracıyla): Sprite (2D and UI), Single, Full Rect, CompressedHQ, mipmap kapalı, Clamp, en fazla 2048.
- TextMeshPro: Unity 6 `com.unity.ugui` içinde; TMP Essential Resources `Assets/TextMesh Pro` altına aktarıldı (Packages/ProjectSettings değişmedi). TMP kendi ayar dosyası için `Resources/` klasörü kullanır; bu kural 27'nin zorunlu bir istisnasıdır. Türkçe karakterler ve ●○×–·° sembolleri fontta doğrulandı.
- Prefab `Assets/Levels/Level1/Prefabs/UI_DisasterReportTemplate.prefab` (`Jam > Wilson > Afet raporu prefab'ını kur`):
  - Canvas (Screen Space Overlay, sıra 75) + CanvasScaler (Scale With Screen Size, 1920×1080, 0,5) + GraphicRaycaster + CanvasGroup.
  - Şablon görseli `AspectRatioFitter (FitInParent)` ile 16:9 korunarak sığar; 16:9 dışı ekranlarda koyu kenar dolgusu gelir, kart kesilmez.
  - Bütün UI öğeleri görselin piksel koordinatlarından normalize anchor'larla yerleşir; çözünürlük değişince görselle birlikte ölçeklenir.
  - Öğeler (hepsi TMP / gerçek UI):
    - Başlık "AFET RAPORU" ve bölüm adı.
    - Sol üst çerçevede durum (EV AYAKTA KALDI / DENGE BOZULDU).
    - Sağ üst çerçevede toplam puan.
    - Üst orta çubukta en yüksek eğim işaretçisi ve yazısı.
    - 3 yıldız (aktif/pasif sprite).
    - 4 kategori satırı.
    - Dinamik madde listesi (VerticalLayoutGroup; ● başarı yeşil, × eksik kırmızı, kritik kalın kırmızı, – bilgi).
    - Canlı 3D Wilson portresi (RawImage), "WILSON:" mesajı ve yeşil italik gerçek hayat notu.
    - Gerçek Button bileşenli 3 düğme; orta düğme vurgulu ve daha büyük, üzerine gelince/basınca vurgu.
- Kod: `Assets/Scripts/UI/DisasterReportPanel.cs` (`Show(Data)`). Bölüm adı, puanlar, yıldız, maddeler, Wilson mesajı ve notu, eğim ile düğme metinleri ve callback'leri dışarıdan gelir.
- `L1_MissionDirector.reportPrefab` → 5 sahnenin hepsinde bu prefab (sahne YAML'larında GUID `25ff1010…` doğrulandı). Eski kodla kurulan rapor yedek olarak kaldı.
- Düğme davranışı:
  - Başarı: TEKRAR DENE / **SONRAKİ BÖLÜM** / ANA MENÜ.
  - Son bölüm: **YENİDEN OYNA**.
  - Başarısızlık: WILSON'I TEKRAR DİNLE / **BU BÖLÜMÜ TEKRAR DENE** / ANA MENÜ.
- Rapor açıkken oyun HUD'u tamamen gizlenir (ilk testte HUD yarı saydam olarak raporun üstünde görünüyordu — düzeltildi). Yıldızlar renk çarpımıyla turuncuya dönüyordu — düzeltildi.
- Play testleri:
  - **L1 başarı** (gerçek BOŞLUK ile Çök–Kapan–Tutun): 84/100, 2 yıldız, düzen temiz.
  - **L2 başarısızlık** (devrilme zorlandı): 0 yıldız, kırmızı neden satırı, düğmeler doğru; "WILSON'I TEKRAR DİNLE" gerçek tıklamayla bölümü tam brifingle yeniden açtı.
  - **L5 başarısızlık** (su bastı): karışık başarı/eksik maddeler hizalı.
  - Ekranlar: `screenshots/Report_L1_success.jpg`, `Report_L2_fail.jpg`, `Report_L5_fail.jpg`.
- NOT_RUN: L3 ve L4 raporu bu turda yeniden çekilmedi (aynı prefab ve aynı kod yolu). L5 başarı/final ("YENİDEN OYNA") görünümü bu turda ekranda görülmedi; test sırasında oyun menüye geçti.
- Commit'lenmeyenler: Unity'nin kendiliğinden değiştirdiği `Assets/Settings/*` (URP) ve `ProjectSettings.asset`.
