# SON DENGE — YEDİ EKRANLI MENÜ SİSTEMİ
## Claude Code / Unity MCP için UÇTAN UCA UYGULAMA PROMPTU

**Ürün:** Son Denge — Unity 6.3 LTS (`6000.3.26f1`) / URP 3D / 2.5D sabit yandan oynanış.  
**Görev:** Yedi görsel referansa çok yakın, tamamı işlevsel, animasyonlu, Türkçe, modüler ve test edilmiş **gerçek oyun menüleri** oluşturmak.  
**Depo:** `yenererr/SonDenge`. Mevcut `Assets/Scenes/Menu.unity`, menü yöneticileri, UI toolkit yardımcıları, Wilson modeli ve Level1-5 oyun akışı korunacak.  
**Teslim standardı:** Sadece mockup, ekran görüntüsü, HTML veya üretilmiş C# sınıfları değil; Unity sahnesine bağlanmış, tıklanıp gezilebilen, kontrolleri çalışan, test edilmiş ve gerekirse build alınabilir menü sistemi.  
**Kaynak klasörü:** Bu belgeyle aynı paketteki `references/` içindeki 7 ayrı PNG dosyası. **Yedi görselin HER BİRİNİ ayrı olarak açıp incele.**

---

# 0. GERÇEK UYGULAMA İÇİN ZORUNLU KURALLAR

1. **Bu belgeyi okuyup işi gerçekten uygula.** Sadece plan hazırlama, özet verme veya TODO üretmeyle durma. Faz 0–11 sırasıyla çalış, test et, raporla, sonraki faza geç. İnsan onayı yalnız gerçek yetki/erişim engeli, potansiyel veri kaybı veya başka kişinin aktif dosyalarının ezilmesi riski olduğunda gereklidir.
2. Önce `CLAUDE.md`, `docs/ZORUNLU_KURALLAR.md`, `docs/OYUN_TASARIMI.md`, `docs/TEAM_PLAN.md`, `docs/API_CONTRACTS.md`, `docs/MCP_KURULUM.md`, `README.md` oku; `git status`, `git branch -a`, `git log`, Git LFS ve Unity sürümünü doğrula. Kullanıcının menü tasarımı talebi, önceki oyun tasarım dokümanlarının menü/ekran görsel hedefleri için güncel gereksinimidir, ancak Git sahiplik kurallarını kendiliğinden geçersiz kılmaz.
3. Güncel `main` ile `kisi1-level1` ve diğer aktif dalları incele. Özellikle eski `Assets/Scripts/UI/MainMenuUI.cs`, `Assets/Scripts/UI/UiKit.cs`, `Assets/Levels/Level1/L1_Hud.cs`, `L1_Hud.Panels.cs`, `GameManager`, `SceneLoader`, `AudioManager`, `GameSettings` kodlarını ve mevcut Canvas/Scene referanslarını kontrol et. Çalışan sistemleri yeniden yazmak yerine **yeniden kullan / güvenli refactor yap**. Eş zamanlı çalışan branch'leri rastgele merge etme.
4. Lead'e ait menü ekranında çalış. Ayrı çalışma/entegrasyon branch'i oluştur: örnek `feature/menu-redesign` (yapılacak dosyaların sahiplik onayı varsa). `main`'e doğrudan push YOK; `git reset --hard`, `push --force`, kör rebase YOK. Kirli çalışma ağacını koru; başka çalışmaları silme. Gerekli `ProjectSettings/`, `Packages/`, `Contracts.cs` değişikliklerini yetki/sahiplikle uyumlu yürüt.
5. Projedeki Unity MCP'yi kurulu gerçek araçlarıyla kullan; hayali MCP metodları uydurma. Gerekirse gerçek Unity Editor API ve Editor scriptleriyle sahne/prefab oluştur; **Editor'de somut nesne oluşturulmadan iş tamamlanmış sayılmaz**.
6. Her sahne için tek `Main Camera`, tek `AudioListener`, ihtiyaç halinde tek `EventSystem`, tek aktif UI input module olmalı. Var olan `[Systems]` bootstrap'ını ikinci kez kurma. Sahne geçişlerinde projedeki `SceneLoader` sözleşmesine uy. Eski UI'yi saklamak yerine görünürde/etkileşimde çift UI ve çift input oluşmasını önle.
7. 3D oyun fiziğini ve sahneleri bozma. `Resources.Load`, `.meta` kimliklerini silme veya scene YAML'ini elle kör düzenleme yapma. Editor üzerinden taşımalar GUID korumalı olsun. UI Screen Space–Overlay, `CanvasScaler` **Scale With Screen Size**, 1920×1080, match 0.5. Tercihen TMPro (`TMP_Text`), kurulumu yoksa repo onayıyla ekle; yeni paket kurmaya gerek yoksa kurma.
8. **PNG'leri asla bütün halinde tıklanabilir menü diye kullanma.** Bu görsellerin içinde **önceden çizilmiş başlıklar, Wilson, kartlar ve butonlar var**. PNG'leri **referans (design board)** olarak içeri al. Son üründe, gerçek UI öğelerini ekran üzerinde ikinci kez göstermeye neden olacak baked screenshot + overlay yaklaşımı yasaktır. Arka planı mevcut Unity low-poly sahne/3D ev/ortam varlıklarıyla yeniden kur; dekoratif zeminde kullanılacak görüntü gerekiyorsa yazısız/arayüzsüz ayrı bir `background-only` katmanı hazırla. Görseldeki yazılar yanlış/eksikse **bu belgede tanımlanan doğru metni uygula**. HUD/menu metnini gerçek `TMP_Text`, butonları `Button`, ayarları `Slider`/`Toggle`/`TMP_Dropdown`, kartları UI prefablardan yap.
9. Bütün oyuncuya görünen yazılar **Türkçe**. Sanat yönü: sıcak ahşap, fildişi/parşömen, altın vurgu, koyu kahve yazı, pastel turuncu-günbatımı, zeytin yeşili/teal destek. Low-poly doğa, kayalık zirvede iki katlı ahşap ev, manzara, çam ağaçları, sıcak ışıklar ve Wilson rehber tavuk. Neon/cyberpunk/flat kurumsal UI istemiyorum.
10. **Wil​son:** Var olan lisanslı low-poly tavuk modelini ve kredilerini koru (`L1_WilsonChicken`, Wilson OBJ ve `ThirdParty/Wilson/CREDITS.md` gibi kaynakları doğrula). Gerekirse UI için sahneden bağımsız `RenderTexture` + ikinci yalnız-Wilson kamerası tasarla; ekstra kamera `MainCamera` tag'i veya `AudioListener` taşımasın. Daha pratikse uygun izinle orijinal modelden portrait üret. Menüde **ev içindeki gezinen oyun tavuklarıyla iki kopya yönetme hatası oluşturma**.
11. **Öncelik:** Önce tüm ekranlar işlevsel ve tutarlı; ardından görsel ince işçilik. Sistemleri parçalara ayır, gerçek kullanıcı etkileşimini doğrula, aşırı animasyondan kaçın. Mümkün olan her yerde Inspector'dan ayarlanabilir parametreler kullan.

---

# 1. REFERANS GÖRSELLER — MUTLAKA DOĞRU EŞLE

Kaynak dosyaları referans klasöründe bul. Kaynak boyutları yaklaşık **1672×941, 16:9**; bu, hedef Canvas 1920×1080 ile **aynı oranlı** ama farklı çözünürlüktür. Görselleri referans gösterirken kaynak boyutunu ezme/esnetme.

| No | Dosya (paketteki yolu) | Gerçek Unity ekranı | Görsel yerleşim |
|---|---|---|---|
| 01 | `references/01_ANA_MENU.png` | **Ana Menü** | Solda ahşap SON DENGE logosu ve dikey dört buton; sağda gün batımlı kayalık ev; sağ alt Wilson ve konuşma balonu |
| 02 | `references/02_BOLUM_SECIMI.png` | **Bölüm Seçimi** | Üstte ahşap başlık; yatay 5 afet kartı, yıldız/ikon/süre; altta Wilson mesajı ve Devam Et |
| 03 | `references/03_WILSON_GOREV_TANITIMI.png` | **Bölüm Öncesi Görev Tanıtımı** | Ahşap iç mekân, solda Wilson ve konuşma, sağda parşömen görev paneli ve afet güvenliği kartı |
| 04 | `references/04_YUKLEME.png` | **Yükleme / Geçiş** | Oyunun dağı ve evi, logo, yatay yükleme çubuğu, alt köşe Wilson ipucu |
| 05 | `references/05_NASIL_OYNANIR.png` | **Nasıl Oynanır?** | Parşömen eğitim paneli, sol yan sekme listesi, sağda kontroller/demolar, altta Wilson |
| 06 | `references/06_AYARLAR.png` | **Ayarlar** | Sol tarafta ayar kategorileri, sağda form kontrolleri, altta uygula ve varsayılana dön |
| 07 | `references/07_OYUN_HAKKINDA.png` | **Oyun Hakkında / Krediler** | Orta geniş parşömen, solda oyun açıklaması, merkez ev görseli, sağda ekip ve lisans bilgisi |

**İstenen sadakat:** Görsellerin estetik kompozisyonunu, oran hissini, tipografi hiyerarşisini, boşlukları, renklerini, ahşap buton biçimlerini ve atmosferini yakından yeniden üret. Ama görüntüde yanlış olan yazım/kullanım/oyun kontrolü/örnek skor gerçek oynanışa empoze edilmez. Rehberler ve görevler güncel kodun gerçek kontrollerine uymalı.

# 2. TASARIM SİSTEMİ / ORTAK PREFABLAR

## 2.1 UI paleti ve tipografi (yaklaşık hedef değerler)

- `WoodDark`: `#503022`, `WoodMid`: `#80502D`, `WoodLight`: `#B37C42`; ahşap yüzey çok hafif lif/oyma dokulu.
- `Parchment`: `#F8E8C9`, `ParchmentLight`: `#FFF3DA`, `Ink`: `#33251C`.
- `Gold`: `#F4B640`, `GoldHover`: `#FFCC5F`, `Danger`: `#B64D3E`, `Teal`: `#399C8B`, `Muted`: `#7D7265`.
- Başlık: kalın, doğal oyun başlığı hissi; kullanıcının uygun lisanslı fontu veya projede mevcut iyi okunaklı font. Türkçe `ğ ü ş ı ö ç İ` bütün font ağırlıklarında doğru görünmeli. Başlıkta koyu kontur + hafif açık gölge, küçük yazıda aşırı outline yok.
- Metinler `TextMeshPro` ile dinamik. Hiyerarşi: logo 90–130 referans px, ekran başlığı 55–70, kart başlığı 28–36, gövde 25–30, yardımcı metin 19–23. Gerçek ekranda taşma varsa düzenle; bu değerler kör zorunluluk değil.
- UI hitbox'ları: fare/touch >= 44 CSS-eşdeğeri px (1920×1080 ölçeğinde rahat tıklanır), kontroller arasında güvenli boşluk.

## 2.2 Önerilen ortak UI prefab bileşenleri

```
Assets/Prefabs/UI/Menu/
  UI_MainMenuRoot.prefab
  UI_BrandLogo.prefab
  UI_WoodButton_Primary.prefab
  UI_WoodButton_Secondary.prefab
  UI_WoodTabButton.prefab
  UI_ParchmentPanel.prefab
  UI_ChapterCard.prefab
  UI_StarRating.prefab
  UI_WilsonMessage.prefab
  UI_WilsonPortrait.prefab
  UI_TipCard.prefab
  UI_SettingsRow.prefab
  UI_ConfirmDialog.prefab
  UI_LoadingProgress.prefab
```

Bunlar **önerilen** adlardır; projede aynı işlevi gören iyi prefab varsa onu genişlet. Gereksiz duplicate oluşturma. Sahneye rastgele yüzlerce primitive UI elemanı bırakıp bakım maliyeti yaratma. Ortak tasarımı prefab/variant ile yönet.

## 2.3 Katman sırası ve geçişler

- Arka plan: düşük maliyetli sabit/canlı **3D environment**; hafif kamera pan, ağaç/bulut/parçacık hareketleri yalnız performans uygunsa. Oyun kamerasını bozmadan Menu sahnesine ait render.
- Orta: yarı saydam koyu vignette / kontrast overlay (metin okunaklılığı için).
- Üst: Screen Space–Overlay UI; ayrı raycast kontrollü gerçek paneller.
- Modallar/ayarlar/help: tek bir menü-root altında panel geçişi; navigasyon bir **tek aktif ekran** üzerinden yönetilsin. Fade/scale/small slide ~0.18–0.3 sn. `CanvasGroup` ve `unscaledDeltaTime`; bir panel kapanırken diğerine tıklanma engeli olmayacak.
- Ses: kısa ahşap click, hover, onay, hata ve panel açılma sesi; `AudioManager` kullan, ikincil AudioManager spawn etme. `ReduceMotion` açıkken animasyonları kısalt/kapat.
- Harfler, etiketler, buton durumları ve UI renkleri tüm ekranlarda birbiriyle tutarlı.

## 2.4 Kullanıcı akışı (tek gerçek bilgi kaynağı)

```
Menu
 ├── OYUNA BAŞLA → Bölüm Seçimi → Bölüm Görev Tanıtımı → Yükleme → ilgili Level sahnesi
 ├── NASIL OYNANIR? → eğitim ekranı → geri
 ├── AYARLAR → ayarlar ekranı → geri
 ├── OYUN HAKKINDA → kredi ekranı → geri (açık buton veya yardım içinden)
 └── ÇIKIŞ → onay iletişim kutusu → uygulamadan çıkış

Level ESC → Duraklat → DEVAM ET / TEKRAR DENE / AYARLAR / ANA MENÜ
Level sonucu → TEKRAR DENE / SONRAKİ BÖLÜM / ANA MENÜ
```

- `Level1` ilk oyun başlatıldığında oynanabilir; diğer bölümler kayıt durumuna göre unlock. Oyuncu tamamladığı bölümün yıldızını yükseltmek için tekrar oynayabilir.
- Kaydedilmiş oyun varsa ana menüde **DEVAM ET** (son erişilebilir bölüm) gösterme seçeneği kullanılabilir; referansın kompozisyonunu bozmadan. Kayıt yoksa **OYUNA BAŞLA**.
- Kullanıcı geri geldiğinde aktif sayfa/gösterilen seçim restore edilebilsin; sahne reload üzerine boşa kopyalanmış menü overlay'leri kalmasın.

---

# 3. EKRAN 01 — ANA MENÜ (`01_ANA_MENU.png`)

## Görsel ve yerleşim

- **Sol %42:** üstte büyük `SON DENGE` ahşap tabela/logosu. Altına dört büyük ahşap düğme dikey, her birinde ikon ve Türkçe yazı; hepsi gerçek `Button` bileşeni.
- **Sağ %58:** gün batımlı orman/dağ vadisi; sivri kayalık üzerinde ışıkları yanan stilize iki katlı ahşap ev en büyük görsel odak; uzak dağlar, turuncu-mor atmosfer, az canlı bulutlar.
- **Alt-sağ/orta:** Wilson 3D tavuk portrait/RenderTexture, sevimli ama gerçek oyun modeline sadık; konuşma balonu: `Merhaba! Ben Wilson! Afetlere karşı hazırlanmaya var mısın?`
- Wilson yanında küçük ahşap slogan levhası: `Küçük hazırlıklar büyük fark yaratır!`.
- Alt köşede küçük sürüm bilgisi varsa projedeki gerçek sürüm `Application.version` üzerinden gelsin; `v1.0.0` diye sabit uydurma.

## Butonlar ve işlevleri

1. `OYUNA BAŞLA` (altın, varsayılan odak) → ChapterSelect paneli; kayıtlı oyun varsa tercihen `DEVAM ET` eylemi veya küçük ek buton.
2. `NASIL OYNANIR?` → Help paneli, Temel Kontroller sekmesi.
3. `AYARLAR` → Settings, son açılan sekme korunabilir.
4. `ÇIKIŞ` → Türkçe `Oyundan çıkmak istediğine emin misin?` onay; `EVET, ÇIK` / `VAZGEÇ`. WebGL'de `Application.Quit` çalışmayacağı için çıkışı gizle veya duruma uygun `Menüden çıkış tarayıcıda desteklenmiyor` bildirimi ver.
5. `OYUN HAKKINDA` erişimi: alt küçük bağlantı veya Help içindeki bağlantı; ana tasarım kompozisyonunu bozma.

## Mikro etkileşim

- `OYUNA BAŞLA`: altın glow, hovered state ve hafif scale `1→1.04`; klavye ile Enter/Space aktivasyonu.
- Ana sayfa yüklenirken sol menü kademeli fade; Wilson bir kez kısa selamlama animasyonu; sürekli agresif sekme/zoom yok.
- UI güvenli alan dışına taşmamalı; TV/monitör 16:9 hedef, 16:10/21:9 için arka plan crop/letterbox ama metin ve CTA'lar korunsun.

## Kabul

- Her buton tek kez çalışır; ESC menüdeyse onay modalını kapatır, ana menüyü kapatmaz.
- Oyundan sonuç ekranıyla dönüldüğünde menü tekrar düzgün açılır, modal/input kilitleri kalmaz.

---

# 4. EKRAN 02 — BÖLÜM SEÇİMİ (`02_BOLUM_SECIMI.png`)

## Görsel ve yerleşim

- Üstte merkez büyük ahşap `BÖLÜM SEÇİMİ`; üst-sol `ANA MENÜ` back button.
- Orta alanda yatay **beş ayrı chapter card**: her kart özel afet teması/tonları, tema ikonu, Türkçe başlık, ev sahnesinden temsil görseli, **gerçek kayıt verilerinden** 0–3 yıldız, afet süresi ve kilit/başarı durumu.
- Card oranı ~0.55–0.60 (dikey kart), boşluklar eşit, seçili kart altın kenarlıklı ve görsel olarak yükseltilmiş. Seçili olmayan kartlar daha mat. Kartların kendisi seçilebilir buton.
- Altta sol Wilson küçük portrait + konuşma balonu; sağ altta ana altın CTA `DEVAM ET` / `BÖLÜME GİR`.

## Bölüm verileri (otoriter görüntü etiketleri)

| ID | Kart başlığı | Alt başlık | Afet süresi | Temel görsel tonu |
|---|---|---|---|---|
| Level1 | `1. BÖLÜM · DEPREM` | `İlk Sarsıntı` | `12 saniye` | Altın/toprak |
| Level2 | `2. BÖLÜM · FIRTINA` | `Rüzgâra Karşı` | `12 saniye` | Lacivert/soğuk mavi |
| Level3 | `3. BÖLÜM · YANGIN` | `Dumanın Ardında` | `30 saniye` | Kızıl/turuncu |
| Level4 | `4. BÖLÜM · YILDIRIM` | `Gökyüzünün Öfkesi` | `25 saniye` | Mor/gece |
| Level5 | `5. BÖLÜM · YAĞMUR` | `Son Denge` | `25 saniye` | Derin mavi/teal |

**Süreler afet süresidir, bölümün toplam oynanış süresi değildir.** Kayıt verisinden gösterilen en yüksek yıldız sayısı; kaynak kodda demo değerlerine hard-code etme. Örnek görseldeki 2/1/2/1/0 yıldızlar yalnızca konsepttir, gerçek oyuncu kaydı değildir.

## Durumlar

- `Available`, `Locked`, `Completed`, `Selected`; `Locked` kartlarda padlock, açıklama: `Bu bölüm için önceki bölümü tamamla.` Seçimi gösterilebilir ama başlatılamaz.
- Başarı `1-3 yıldız`; oynanmış ama başarısız bölüme `0 yıldız` ile yeniden giriş olabilir.
- `ChapterCard` prefab'ı `LevelDefinition` veya eşdeğer kaynaktan veri alsın; menü prefabına fizik kontrolleri kopyalama.
- İlk kez girişte Wilson: `İlk olarak deprem bölümünü oynayalım. Evimizi sarsıntıya karşı birlikte hazırlayacağız!` Sonrakiler ilgili afete uyarlanmış mesaj; kilit için sebep açıklansın.

## Davranış

- Seçilen kart bilgilerinden menü state'i güncellenir, seçim görseli animasyonlanır.
- `DEVAM ET` → Wilson Görev Tanıtımı; `ANA MENÜ` geri; klavye sol-sağ ile kart değişimi, Enter ile onay; tıkla da çalışsın.
- Bölümler arasında geçiş sahne stringini kör `"Level" + index` yapmak yerine `LevelDefinition.sceneName` doğrulanarak `SceneLoader` üzerinden yapılır.
- Erişilebilir bölüm kayıtları varsayılan olarak bir önceki bölümü **başarıyla tamamlama** üzerinden açılır; replay serbest.

---

# 5. EKRAN 03 — WILSON BÖLÜM ÖNCESİ GÖREV TANITIMI (`03_WILSON_GOREV_TANITIMI.png`)

**Bu ekran, mevcut Wilson görev sistemine bağlanacak; ayrık, ikinci görev durum makinesi üretmeyeceksin.**

## Görsel

- Zemin: ev içi sıcak ahşap salon (koltuk, ocak/şömine, kitaplık, pencereden dağ, güneşin vurduğu duvar); yoğun ama UI'yi boğmayan dekor.
- Üstte `1. BÖLÜM · DEPREM` gibi büyük ahşap başlık. Sol-alt Wilson, solda açık renk büyük konuşma balonu.
- Sağ ~%53 genişliğinde açılan parşömen `BU BÖLÜMDEKİ GÖREVLER`; her görev satırında ikon + tam ve doğru Türkçe başlık.
- Sağ tarafta `UNUTMA:` güvenlik kartı; depremde basit Çök–Kapan–Tutun simgeleri; diğer afetlerde o afete ait **gerçek** güvenlik mesajı.
- Parşömen alt sağda altın `HAZIRLIĞA BAŞLA`; sol veya üstte `BÖLÜM SEÇİMİNE DÖN`.

## Her bölüm için görevler (dynamic data)

**Level1 / Deprem:** `Kitaplığı sabitle`, `Buzdolabını sabitle`, `Deprem çantasını hazırla`, `Ağırlıkları dengele`. Güvenlik notu: `Deprem sırasında Çök–Kapan–Tutun. Eşya taşımaya çalışma.`

**Level2 / Fırtına:** `Çatı bağlantılarını güçlendir`, `Pencere kilitlerini kapat`, `Balkondaki gevşek eşyaları içeri al`, `El fenerini hazırla`. Güvenlik notu: `Şiddetli rüzgârda pencerelerden uzak, güvenli bir iç alanda kal.`

**Level3 / Yangın:** `Duman alarmını kur ve test et`, `Ocağı kapat`, `Yangın tüpünü erişilebilir yere yerleştir`, `Kaçış yolunu aç`. Güvenlik notu: `Yoğun dumanda yangınla mücadeleye girişme; güvenli tahliye önceliklidir.`

**Level4 / Yıldırım:** `Yıldırımdan korunma sistemini tamamla`, `Fırtına öncesinde elektroniklerin fişini çek`, `Gerilim koruma ekipmanını hazırla`. Güvenlik notu: `Gök gürlerken dışarı çıkma ve elektrikli cihazların fişini çekme.`

**Level5 / Şiddetli Yağmur:** `Kum torbalarını yerleştir`, `Drenajı temizle`, `Hassas eşyaları güvenli yüksek yere taşı`, `Su yalıtımındaki açıkları kapat`. Güvenlik notu: `Su baskınında elektrikli cihazlardan uzak dur; güvenli yere geç.`

**Wilson diyalogları** (konsept cümleleri; kısa ve bölüme özel, tümü Türkçe):
- Deprem: `Gıdak! Bu ev biraz sallanabilir. Önce ağır eşyaları güvenli hâle getirelim!`
- Fırtına: `Gıdak! Rüzgâr geliyor. Çatıyı, pencereleri ve balkonu kontrol edelim!`
- Yangın: `Gıdak! Yangın çıkmadan önlem alalım. Alarmı, tüpü ve kaçış yolunu hazırlayalım!`
- Yıldırım: `Gıdak! Gök gürültüsü yaklaşmadan elektrik güvenliği hazırlıklarını tamamlayalım!`
- Yağmur: `Gıdak! Çok yağmur yağacak! Suyun eve girmemesi için hazırlanalım!`

## Durum ve zamanlayıcı

- Bölüm görev tanıtımı **hazırlık süresini başlatmaz**. `HAZIRLIĞA BAŞLA` butonuna basınca görev ekranından gerçek Level sahnesine geç. Eğer tanıtım ekranı Level sahnesi yüklendikten sonra açılıyorsa süre ayrıca gate edilir; çift sayma/iki kez başlatma olmaz.
- Görev listesi `TaskDefinition`/`LevelDefinition` verilerinden okunmalı; elle ayrı liste hard-code edip ileride görev sayısı farklılaşınca uyuşmazlık yaratma.
- Wilson konuşması kısa geçişle, istendiğinde `SONRAKİ` ile satır satır ilerleyebilir; görevi başlamayı engelleyen gereksiz beş ekran yaratma.
- `ESC`: bölüm seçimine dön veya tanıtım açıkken güvenli kapatma; hiçbir durumda tanıtımı bypass edip hazırlık sayacını uygunsuz başlatma.
- Görev ikonları mevcut lisanslı sprite/3D asset önizlemelerinden gelir; ikon yoksa geçici ama okunaklı Unity UI sembolü ile temsil edilip eksik kayıt altına alınır.

---

# 6. EKRAN 04 — YÜKLEME (`04_YUKLEME.png`)

## Görsel

- Turuncu-mor gün batımlı uzak dağlar, kayalık üstünde ev, solda Wilson'ın açtığı parşömen ipucu, ortada ahşap çerçeveli sarı ilerleme barı.
- Yükleme metni: `Bölüm yükleniyor...` (dinamik bölüm adı da eklenebilir).
- İpucu bankası 12–20 kısa, doğru ve oyundaki gerçek işlevlerle uyumlu mesaj; ayrım: **oyun mekaniği ipucu** ve **gerçek hayatta güvenlik**.

## Zorunlu gerçek davranış

- **Sahte her seferinde 3 saniyelik %0→100 animasyonu KULLANMA**. Unity sahne geçişinin gerçek async ilerleme değerini veya açıkça belirlenmiş görsel geçiş durumunu göster.
- Mevcut `SceneLoader` async progress API sağlamıyorsa onu bilinçli şekilde genişlet (sahne transition servisinin tek kaynağı kalsın); `AsyncOperation.progress` değerinin `0–0.9` yükleme aralığını uygun normalize et; gereksiz %100'ü erkenden göstermeyip aktivasyon durumunu doğrula. Tamamlanma/activate sonrasında ekran kapanmalı.
- İstisna: Aşırı hızlı yükleme olup bar görünmeden sahne değişiyorsa **min. görsel gösterim süresi** 0.4–0.6 sn olabilir; bu, sahte ilerleme yüzdesi göstermeye gerekçe değildir.
- `Time.timeScale`, `AudioListener`, keyboard focus, cursor state ve kalıcı olmayan UI nesneleri sahne sonunda temiz kalmalı. Yükleme başarısızsa hata mesajı ve menüye dönüş çalışmalı.
- Menüye dönme, tekrar oynama, sonraki bölüm ve ilk açılış aynı geçiş yöneticisini kullansın.

---

# 7. EKRAN 05 — NASIL OYNANIR? (`05_NASIL_OYNANIR.png`)

## Görsel

- Üstte ahşap `NASIL OYNANIR?`; sıcak ahşap oda/arka plan; ortada büyük açık parşömen.
- Solda alt alta sekmeler: `Temel Kontroller`, `Görevler`, `Denge Sistemi`, `Afet Aşamaları`, `Puanlama`, `İpuçları`.
- Orta sağda aktif sekme başlığı ve iki sütun açıklama; sağ alt Wilson rehber görseli + konuşma balonu.
- Bilgiyi küçük yazıya sıkıştırma. Her sekmede 3–7 kısa somut öğe; gerekirse kaydırma (`ScrollRect`) ve içeriğe uygun ikonlar.

## İçerikler / gerçek kodla uyum

1. **Temel Kontroller:** A/D veya oklar yürüyüş; W/S merdiven varsa yukarı/aşağı, E alma/bırakma (gerçekten etkinse), Fare nesne sürükleme (sahne gerçekten destekliyorsa), F sabitleme, ESC menü, Space depremde Çök–Kapan–Tutun (sadece depremde). **Görselde W=Zıpla yazıyor olabilir; mevcut `L1_ResidentController` W'yi tırmanış için kullanıyor. Hatalı kontrolü aynen kopyalama; güncel binding'i öğrenerek doğru metni göster.**
2. **Görevler:** Wilson başlangıçta anlatır, görev kaynağı/varış noktası, doğru yerleştirme ve tamamlandığında check işareti. Görev listesi istenirse TAB ile açılır **ancak gerçekten TAB implement edilmişse**; değilse uygun mevcut butonla eşleşsin.
3. **Denge Sistemi:** Kütle × yatay uzaklık; eşya taşınması evin açısını değiştirir; 26°'yi yaklaşık 1 sn aşarsa oyun içinde ev devrilir; bunun fantastik olduğu açıkça söylenir.
4. **Afet Aşamaları:** Hazırlık → Afet → Afet Raporu; süre bitince eksik görevle afet başlar; hazırlık önlemleri zararı azaltır.
5. **Puanlama:** Hazırlık 35 / doğru davranış 35 / denge 20 / eşya koruma 10; toplam 100, yıldızlar gerçek skor kaydından gelir; başarısızlık ve kritik güvenlik koşulları yıldızı sınırlar.
6. **İpuçları:** Ana güvenlik notları: mobilyayı önceden sabitle, deprem sırasında koşma, yangında tahliye yolunu açık tut, yıldırım tehlikesinde içeri gir, selde elektrikli cihazlardan uzak dur.

## Etkileşim

- Soldaki sekme tıklanırsa panel içeriği ve Wilson mesajı güncellenir; eski sekme içeriği aktif/klickable kalmaz.
- Ekran `Menüden mi açıldı / Oyun içinden mi?` giriş noktasını hatırlar, `GERİ` ile doğru ekrana döner.
- Klavye sekmeler arası Tab/ok geçişi ve görsel focus state; scroll alanında mouse wheel çalışır.

---

# 8. EKRAN 06 — AYARLAR (`06_AYARLAR.png`)

## Görsel

- Gün batımlı dağ ve ev arka planda; sağda geniş parşömen paneli; üstte ahşap `AYARLAR`.
- Sol kategori kolonu: `Ses`, `Görüntü`, `Oynanış`, `Dil`, `Erişilebilirlik`.
- Sağ dinamik form; seçilen kategoriye ait kontroller ve kısa açıklamalar.
- Alt: `VARSAYILANA DÖN` ve daha belirgin `UYGULA` (gerçekten pending/apply modeli varsa); ayrıca görünür `GERİ`.

## Teknik anlamda gerçek ayar seçenekleri

**Ses:** Ana ses, müzik, efekt sesleri 0–100; sessiz seçeneği. `AudioManager`/`AudioMixer` varsa onunla senkron, ses değerleri kaydedilir ve diğer sahnelerde uygulanır.

**Görüntü:** `Çözünürlük` (yalnız gerçekten `Screen.resolutions`'ta desteklenen seçenekler), `Tam Ekran`, `VSync`, `Grafik Kalitesi` (projede mevcut quality indexlerine göre), `Parlaklık` (URP Volume/gerçek uygulaması varsa; yoksa çalışmayan sahte slider gösterme). Pencere ve WebGL desteğine göre seçenekleri disable/gizle. Riskli display değişikliklerinde geri alma veya güvenli doğrulama davranışı olsun.

**Oynanış:** Kamera sarsıntısı, ipucu gösterimi, fare hassasiyeti/zoom hızı gibi gerçekten çalışan ayarlar. Kodda `GameSettings.ReduceShake` varsa onunla birleştir. Zorluk modu proje gereksinimi değilse ekleme.

**Dil:** Oyun Türkçe; `Türkçe (varsayılan)` göster. Hazır İngilizce çeviri yoksa seçilebilir ama çalışmayan İngilizce seçeneği ekleme.

**Erişilebilirlik:** Kamera sarsıntısını azalt, hareketleri azalt, UI ölçeği (tasarım sınırlarında), altyazı/konuşma metinleri, yüksek kontrast veya renk körlüğüne yardımcı durum ikonları gibi **gerçekte implement edilenler**. Örnek görseldeki checkbox/toggle görüntülerini gerçek görsel durumla senkron tut.

## Kayıt, geri ve uygulama

- Tek merkezi `SettingsService` (mevcut `GameSettings` varsa refactor / adapt), `PlayerPrefs` + versiyonlu anahtarlar (ör. `SonDenge.Settings.*`), `Save()` çağrısı; oyun içinden değişiklik yapıldığında ilgili sistem anında/uygulama ile yenilenmeli. Menüden oyuna döndükten sonra ayarlar sıfırlanmasın.
- `UYGULA` değişiklikleri commit eder ve kullanıcıya kısa onay verir; `GERİ` henüz uygulanmamış değişiklikler varsa `Kaydetmeden çıkılsın mı?` modalı veya tasarım tutarlı otomatik-uygula davranışı. İkisini rastgele karıştırma.
- `VARSAYILANA DÖN` yalnız ayarlar kategorisi/tüm ayarlar için açık kapsamlı onayla yapılır; oyuncu bölüm kaydını silmez.
- `ESC`: panel aktifse doğru önceki ekrana döner, açık dropdown veya modal varsa önce onu kapatır. EventSystem focus yönetimi sorunsuz.

---

# 9. EKRAN 07 — OYUN HAKKINDA / KREDİLER (`07_OYUN_HAKKINDA.png`)

## Görsel

- Üst ahşap `OYUN HAKKINDA`; orta büyük iki/üç sütunlu parşömen.
- Sol: `SON DENGE` başlığı + oyunun ana fikrinin 3–5 satırlık gerçek, Türkçe açıklaması.
- Orta: kayalıkta evin düşük maliyetli 3D/screenshot görseli, ahşap çerçeve.
- Sağ: `EKİBİMİZ` ve gerçek isimler/roller (repoda doğrulanabiliyorsa; yoksa **isim UYDURMA**, sırf referans görselindeki kategori adlarını sahte katkı gibi yazma).
- Alt: `TEŞEKKÜRLER`, `GERİ`.

## İçerik

Oyun açıklaması: `Son Denge, sivri bir dağın zirvesindeki fantastik evin dengesini korurken gerçek afet hazırlıklarını öğrendiğin fizik ve strateji oyunudur. Eşyalarını güvenli yerleştir, görevleri tamamla, afetlere hazırlıklı ol.`

- **Krediler:** Repodaki bilinen 3. parti asset lisansları dahil Wilson tavuk modelinin/egg modelinin kredileri, müzik, efekt, font ve kullanılan materyal kaynakları; gerekiyorsa scroll alanı. İzin olmayan harici marka logoları ekleme.
- `TEŞEKKÜRLER`: yardım edenler için doğrulanmış isimler listesi yoksa nötr metin. `GERİ` önceki ekrana döner.
- Güvenlik dipnotu: `Ev dengeleme sistemi kurgusaldır; gerçek afetlerde resmî güvenlik yönergelerini izleyin.`

---

# 10. EKRAN 08 — DURAKLATMA MENÜSÜ (aynı tasarım dilinde, ayrı PNG yok)

- Oyun içinde `ESC` ile aktif; zemin oyun görüntüsünü gösterir, üzeri hafif karartılır/blur; ortada küçük ahşap + parşömen panel.
- Başlık `OYUN DURAKLATILDI`.
- Butonlar `DEVAM ET`, `YENİDEN BAŞLAT`, `AYARLAR`, `ANA MENÜ`.
- Restart/menü işlemlerine `İlerlemen kaydedilmeyen bölüm baştan başlayacak. Emin misin?` türünden doğru ve kısa onay; `DEVAM ET` basınca oyun/input/pause state tamamen temizlenir.
- `Time.timeScale = 0` ile açılsa bile UI animasyonları **unscaled** çalışır; ses sistemi pause stratejisi tutarlıdır; oyun devam ederken Wilson tanıtımı veya sonuç ekranı üstüne yanlışlıkla pause menüsü binmez.
- `GameManager` ve varsa `L1_ProjectGameController` kendi ESC mekanizmasıyla **çifte pause** tetiklememeli. Tek uygulama akışı kur.
- Kullanıcının 7 menü görseliyle **birebir aynı estetik**; sekizinci görsel gerekli değil, mevcut ortak prefablarla uygulanacak.

---

# 11. MİMARİ, DOSYA YAPISI VE VERİ MODELLERİ

## 11.1 Önerilen dizinler (sahiplik kurallarına göre ayarla)

```
Assets/Scripts/UI/Menu/
  MenuFlowController.cs
  MenuPageBase.cs
  MainMenuView.cs
  ChapterSelectView.cs
  ChapterCardView.cs
  WilsonBriefingView.cs
  LoadingView.cs
  HowToPlayView.cs
  SettingsView.cs
  AboutView.cs
  PauseMenuView.cs
  WoodButtonFeedback.cs
  MenuInputRouter.cs
  MenuAudioBridge.cs
  MenuTransitionController.cs
Assets/Scripts/Core/Progress/
  ChapterProgressService.cs
Assets/Scripts/Core/Settings/
  SettingsService.cs  (mevcut servis varsa KOPYA oluşturma)
Assets/Prefabs/UI/Menu/
  ... yukarıdaki ortak prefablardan gerekli olanlar ...
Assets/Art/UI/Menu/
  Sprites, 9Slice, icons, wood/parchment decorations
Assets/Scenes/Menu.unity
Assets/Scenes/Level1..Level5.unity (yalnız gerekli ve yetkili bağlantı değişiklikleri)
docs/production/MENU_IMPLEMENTATION_LOG.md
```

İsimlendirme kararını kodu görerek ver. `Jam.UI`, `Jam.Core` mevcut namespace ve event kontratlarıyla uyumlu olsun. Prefab hiyerarşisi Inspector'dan bağlanmış `[SerializeField]` alanlarıyla kurulmalı; sürekli `GameObject.Find` veya sahne adlarına bağlı string aramalarıyla runtime bağımlılığı kurma. Aktif olmayan UI `CanvasGroup.blocksRaycasts=false`, `interactable=false`; açık olan tek sayfa input alır. Event aboneliklerini `OnDisable`/`OnDestroy`'da kaldır.

## 11.2 Chapter/Menu state

- `MenuPage` (Main, ChapterSelect, Briefing, Loading, Help, Settings, About).
- `LevelSelection` (level id, scene name, title, subtitle, weather theme, intro line, task list, safety tip, disaster duration).
- `ChapterProgress` (highestUnlockedIndex, highestStarsByLevel, bestScoreByLevel, completed flags; yıldızlar önceki yüksek skorla düşmemeli).
- `MenuReturnContext` (menüye oyun içi ayarlardan mı, nasıl oynanırdan mı gidildi; önceki ekran ve seçim bilgisi).
- Kayıt yokken Level1 açık, Level2+ kilitli. Geçiş yalnız teyit edilmiş bölüm tamamlanmasıyla açılır. Eski ekonomi save verisi varsa doğru migration veya zararsız ignore.

## 11.3 Uygulama işlevleri

- `OpenMain()`, `OpenChapterSelect()`, `SelectChapter(id)`, `OpenBriefing(id)`, `StartChapter(id)`, `OpenHowTo(section)`, `OpenSettings(from)`, `OpenAbout()`, `Back()`, `ShowLoading(target)`, `ReturnToMenu()`, `TogglePause()` gibi metotlar **tek menü flow controller** üzerinden çalışsın.
- Scene geçişi mevcut `SceneLoader.Instance.Load(...)` üzerinden; async load için gerekiyorsa mevcut sınıfa uyumlu ve test edilen overload/event eklenir.
- Menü UI state machine, oyun state machine'iyle çatışmasın. `GamePhase.Menu` oyun yüklüyken hazırlık time'ını düşürmemeli. Menüde iken karakter hareket komutları oyun karakterine geçmemeli.
- Her UI panelini `UI Toolkit`/`uGUI` karışımı yapmak yerine projedeki **uGUI Canvas** standardına sadık kal.

---

# 12. FAZ FAZ UYGULAMA PLANI (FAZ GEÇMEK İÇİN TEST ZORUNLU)

| Faz | İş | Kabul kapısı |
|---|---|---|
| **0 — Repo denetimi** | Branch, sahne, var olan UI, Wilson ve ilerleme sistemi, mevcut assetleri haritala | Dosya/sahiplik raporu + risksiz plan |
| **1 — Tasarım temeli** | Referans PNG'leri doğru import et, renk/font/sprite atlas, 9-slice wood/parchment, ortak Canvas, UI prefabs | Unity'de okunaklı gerçek button/panel demo; 16:9 |
| **2 — Menü navigasyonu** | `MenuFlowController`, geri, ESC, modal, state/focus, audio | Main→Help→Back→Settings→Back; çift ekran yok |
| **3 — Ana Menü** | Ev ortamı, logo, 4 buton, Wilson portrait, animasyon | Tüm butonlar çalışır, oyundan dönüş temiz |
| **4 — Bölüm Seçimi** | Beş kart, kilit/yıldız/süre/veri, klavye ve mouse seçim, next CTA | Kilitli seçilmez; ilerleme doğru görünür |
| **5 — Wilson Görev Tanıtımı** | 5 bölümün ayrı görev listesi ve uyarısı, briefing ekranı, gerçek start gate | Tanıtım bitmeden hazırlık timer akmaz |
| **6 — Gerçek Yükleme** | Transition UI ve gerçek async load, progress, ipuçları | Scene yüklendikten sonra panel kapanır; fake % yok |
| **7 — Nasıl Oynanır?** | 6 sekme, gerçek kontroller, scroll/focus, Wilson tips | Her sekme ve GERİ doğru çalışır |
| **8 — Ayarlar** | Ses/görüntü/oynanış/dil/erişilebilirlik, save/apply/default | Ayarlar gerçek etkili, restart'ta korunur |
| **9 — Hakkında / Pause** | Credits/lisanslar, geri, oyun içi pause ve ayar geçişi | Oyunda ESC tek pause; devam/input temiz |
| **10 — Entegrasyon ve görsel QA** | Level1-5 zincirine bağla; prefab, layout, 5 çözünürlük; düğmeler, 9-slice/tipografi, Wilson | Tüm flow pass; referansa yakın ekran görüntüleri |
| **11 — Build ve teslim** | Windows build, başka oturumda açılma; kalite/bellek; README, test dokümanı, Git PR | Kanıtlı build, commit, ekran görüntüsü, sorun listesi |

**Her faz sonrası:** Console compile red error **0**; gerekli PlayMode/EditMode testi; yanlış/hayali çalışan davranış yok; sahne/prefab kaydedilmiş; yalnız kendi yetkili dosyaları değişmiş; log ve test kanıtı yazılmış olmalı. FAIL ise aynı faza dönerek onar, gerekirse izolasyonla regresyon sebebini bul. Başarılı fazdan sonra kullanıcı onayı beklemeden sonraki faza geç.

---

# 13. TEST MATRİSİ / OTOMATİK VE MANUEL DOĞRULAMA

## 13.1 Temel akış

- [ ] İlk launch → `MainMenu` görünüyor, bütün yazılar Türkçe, ikonlar ve Wilson yükleniyor.
- [ ] Ana menü 4 butonu doğru sayfaları açıyor.
- [ ] Chapter select 5 kart; ilk save'te 1 açık/4 kapalı; tamamlamadan sonraki bölüm açılıyor.
- [ ] Kart seçimi Wilson konuşmasını değiştiriyor; kilitli kart CTA aktive etmiyor.
- [ ] Intro → gerçek Level1 sahnesine geçiyor, tanıtım sırasında hazırlık sayacı işlemiyor.
- [ ] Level completion → rapor → chapter progress save → sonraki bölüm seçimi doğru.
- [ ] Oyun sırasında ESC pause aç/kapat; içeride Settings aç/geri; karakter kontrolü pause altında pasif.
- [ ] Ana menüye dönüş, yeniden giriş, tekrar başlatma, art arda 10 sahne geçişi memory leak/duplicate listener üretmiyor.
- [ ] Ses/müzik/grafik ayarları oyunda gerçekten etkili ve yeniden başlatmada korunuyor.
- [ ] 6 Help sekmesi + kaydırma + Geri çalışıyor; kontroller mevcut Input System binding'leri ile uyuşuyor.
- [ ] About ekranında tüm kaynak kredileri ve geri dönüş çalışıyor.
- [ ] WebGL platformu varsa Çıkış doğru şekilde gizleniyor, Windows build'de Quit çalışıyor.

## 13.2 Görsel doğrulama (Screenshot + gözle kıyas)

- [ ] Her bir 7 referans için `docs/production/screenshots/menu-01..07.png` oyun içinden **gerçek screenshot** üretildi.
- [ ] Yeni pause overlay için `menu-08-pause.png` üretildi.
- [ ] Gerçek screenshot üzerindeki tüm görünür metinler ayrı TMP Text; kullanıcı referans PNG'sindeki eski metin, render altında çift görünmüyor.
- [ ] Canvas ve UI güvenli alan testleri: `1920×1080`, `1600×900`, `1280×720`, `2560×1440`, `1920×1200`; bazı ekranların overflow/scroll durumu test edildi.
- [ ] Başlıklar, butonlar, kartlar, HUD/metinler kesilmiyor; uzun Türkçe kelimeler `ğ ü ş ı ö ç İ` doğru.
- [ ] Referansa yakın: gün batımı rengi, ev manzarası/konumu, ahşap/parşömen panel yapısı, Wilson, altın CTA hiyerarşisi, beş kartın hizası.
- [ ] Kilit, seçili, disabled, hover, focus durumları birbirinden ayrılıyor; hover görüntüsü input erişimini bozmaz.
- [ ] Fotoğraftaki dekoratif yazı ve butonlar gameplay logic'e bağlanmıyor; bunların yerini real UI alıyor.
- [ ] Wilson portrait Model/RenderTexture yoksa fallback placeholder ile teslim edip 'tamamlandı' deme; doğru gerçek model bağlantısını çöz.

## 13.3 Kritik negatif senaryolar

- Kayıt dosyası yok / bozuk / eski sürüm; butona hızlı çift tıklama; arka arkaya ESC; yükleme sırasında duraklatma; çözünürlük değişiminde UI raycast kayması; kamera boyutu değişince hitbox kayması; settings auto-apply'da kaybolan değişiklik; chapter unlock'a rağmen erişilemeyen sahne; `Level5` build listesinde yok; yanlış SceneLoader target; `NullReferenceException`; iki EventSystem; iki EventHandler; iki AudioListener; duplicate widget.

**Sorunsuz derleme, tam oynanış testi ve build aynı şey değildir.** Her biri raporda ayrı statü taşır. Unity Editor erişilemiyorsa hangi bileşenin neden test edilemediğini belgeleyip işi uydurma PASS ile kapatma.

---

# 14. OTOMATİK RAPORLAMA / GIT / ÇIKTI

## Her faz sonunda rapor formatı

```md
### FAZ X — [Başlık]
- Tarih / aktif branch / başlangıç ve bitiş commit:
- Çalışılan dosyalar ve klasör sahipliği doğrulaması:
- Oluşturulan/değiştirilen UI prefab'ları:
- Sahne ve Inspector referansları:
- Unity Console kırmızı hata sayısı:
- EditMode / PlayMode test listesi / gerçek sonuç:
- Manuel test adımları ve sonuç:
- Screenshot yolları ve referans karşılaştırması:
- Önceki ekranlara regresyon kontrolü:
- Sınırlamalar / açık hatalar:
- PASS / FAIL / BLOCKED / NOT_RUN:
- Sonraki faz:
```

`docs/production/MENU_IMPLEMENTATION_LOG.md` içinde kalıcı güncelle. Kod değişikliklerinden sonra derlenebilir küçük commit ve Türkçe mesaj at; `push` yetkisi varsa doğru çalışma branch'ine push yap. Başka ekip üyesinin branch'ine doğrudan push yapma; `main`'e commit atma. İlgili PR için özet/diff sun.

## Son teslimde mutlaka

1. Çalışan `Menu.unity` ve alt ekranları aynı tasarım diliyle.
2. Her ekranı gerçek UI olarak uygulayan kontrolcüler ve yeniden kullanılabilir prefablar.
3. Wilson ve lisansları korunmuş entegrasyon.
4. Gerçek sahne geçişleri ve 5 bölüm seçim/kayıt entegrasyonu.
5. Video/ekran görüntüleri (varsa); her referans için screenshot ve görsel farkların kısa açıklaması.
6. Komut/klavye/fare erişilebilirliği; farklı çözünürlükler QA.
7. Build sonucu, commit hash ve test log'u; kalan hataları şeffaf listele.
8. `README`'de menü kullanımı ve kaynak atıfları.

---

# 15. OTURUMU ŞİMDİ BAŞLAT — İLK ÇALIŞTIRMA EMRİ

**Bu bir görev brifingi değil, uygulama emridir.** Aşağıdaki sırayla harekete geç:

1. `references/` altındaki **7 PNG'yi tek tek aç** ve yukarıdaki eşleme ile kontrol et. Gerekirse onları yalnız referans amacıyla repo dışındaki `DesignReferences` veya doküman klasörüne kopyala; runtime PNG bütçesine sokma.
2. Unity projesinin aktif durumu ile docs/git sahipliğini kontrol edip en güvenli branch'i belirle.
3. Var olan ana menünün sahne hiyerarşisini, UI kodunu, Wilson modelini, `SceneLoader` ve `GameSettings`'i incele.
4. `docs/production/MENU_IMPLEMENTATION_LOG.md` ilk raporunu yaz.
5. **FAZ 1'e geç ve gerçek Unity editlerini yap**; rapor/test/güvenli commit → fazlar sırayla devam.
6. Eksik bir asset'i mevcut projeden bulabiliyorsan bul. Kaynağı doğrulanmamış 3. parti paket indirme; diğer aktif sahnelere zarar verme. Gerekirse Editor araçlarıyla 9-slice ahşap/parşömen UI üret.
7. Bütün fazlar PASS olmadan işi tamamlandı diye ilan etme. Kalan blokajları açıkça ayır.

**Kullanıcı onayı beklemeden otomatik devam et; gerçek engel dışında soru sormak için durma. Başla.**
