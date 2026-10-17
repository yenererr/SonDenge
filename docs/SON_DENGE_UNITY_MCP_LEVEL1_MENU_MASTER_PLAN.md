# SON DENGE — Unity MCP ile Menü + Level 1 Uygulama Master Planı

**Sürüm:** 1.0 · **Tarih:** 17 Ekim 2026 · **Hedef:** çalışan ve görsel olarak cilalı *Menu → Level1 → Afet Raporu* dikey dilimi  
**Kaynak repo:** `yenererr/SonDenge` (private) · **Çalışma branch'i:** `kisi1-level1` · **İncelenen HEAD:** `54e2d59aba759a9f4c56013c1e348c6e68217cfd`  
**Motor:** Unity 6000.3.26f1 (6.3 LTS), Universal Render Pipeline 3D 17.3.0, Input System 1.20.0 · **Hedef platform:** önce Windows 16:9  
**MCP:** `com.coplaydev.unity-mcp`, `Packages/manifest.json` üzerinde `v10.3.0`; `.mcp.json` adresi `http://localhost:8080/mcp`.

> **Bu bir uygulama sözleşmesidir.** Aşağıdaki işlerin Unity sahnesinde yapıldığı veya oynanarak test edildiği iddia edilmiyor. Repo GitHub bağlantısı üzerinden incelendi; yerel Unity Editor, import edilmiş gerçek materyal görünümü, MCP sunucusu ve build bu oturumda açılmadı. Gerçek testler, çalışan Unity üzerinden MCP ajanı tarafından yapılmalıdır.
>
> **Öncelik ve kapsam:** 1. Kullanıcının son 2D tasarım kararları, 2. çalışan `kisi1-level1` kaynak kodu ve gerçek mekanikler, 3. `docs/OYUN_TASARIMI.md` / `docs/ZORUNLU_KURALLAR.md`, 4. eski `docs/GDD.md`. Çelişkiler tabloda açıklanmış; tasarım değişikliği takım onayı ve sınırlı scope ile yapılmalıdır.

---

## 0. ÇALIŞTIRACAK AJAN İÇİN KESİN TALİMAT

Sen, GitHub'daki **SonDenge** projesini **Unity MCP ile Editor içinde** geliştiren kıdemli bir Unity 3D gameplay, fizik ve technical-art mühendisisin. Görevin yeni bir proje oluşturmak **DEĞİL**: `kisi1-level1` branch'indeki mevcut Unity projesini açıp mevcut bileşenleri incelemek, çalışır özellikleri kaybetmeden **yeni tek katlı Level 1 ve bütün menü akışını** üretmek.

- **İLK İŞ:** `git status`, `git branch --show-current`, `git log -1 --oneline`; temiz çalışma alanı, branch ve Unity sürümünü teyit et. Kullanıcının yerel değişikliği varsa **asla üzerine yazma**.
- `docs/ZORUNLU_KURALLAR.md`, `CLAUDE.md`, `docs/OYUN_TASARIMI.md`, `docs/TEAM_PLAN.md`, `docs/API_CONTRACTS.md`, `docs/tasks/KISI1_LEVEL1.md` ve `Assets/Levels/Level1/PROJECT_FILES_INTEGRATION.md` dosyalarını **oku**. Bu belgelerin bazıları eski; aşağıdaki “çelişki ve karar” bölümünde son tasarım kararları yazılı.
- `ProjectSettings/`, `Packages/`, `Assets/Settings/`, `Assets/Scripts/Core/Contracts.cs` ve başka kişilerin bölümleri **takım onayı olmadan değiştirilmez**. `main`e yazılmaz.
- MCP sunucusunda gerçekten mevcut araçları keşfet. Araç adları, argümanları veya başarı sonucunu **uydurma**. Unity açık değilse manuel işlem adımlarını ver.
- Her değişiklik öncesi dokunulacak dosyaları listele; küçük partilerde uygula; Unity Console'u kontrol et; sahneleri kaydet; kısa Türkçe commit mesajlarıyla branch'e commit + push yap. Yetki/bağlantı yoksa işlem gerçekleşti demeden komutu bildir.
- **Öncelik P0:** `Menu` açılır, `Oyna` `Level1`e geçirir; tek geniş odalı ev ve kayalık 3D görünür; mobilyalar yalnız yatay taşınır; kamera yakınlaşır/uzaklaşır; ev torkla eğilir; sabitleme/mağaza/deprem/rapor/yeniden başlatma oynanabilir. P1 görsel ve ses cilası, P2 sonraki seviyeler.
- Unity Editor üzerinden test etmeden “tamamlandı” deme. Düşük öncelikli ayrıntılar yetişmezse yalnız bir deprem bölümünü **eksiksiz** bitir.

---

## 1. REPO İNCELEME SONUÇLARI: MEVCUT, EKSİK, RİSKLİ

### 1.1 Gerçek dosyalar / tekrar kullan

| Dosya / klasör | Gerçek durum ve yapacağımız iş |
|---|---|
| `Assets/Scenes/Menu.unity` | Var; menü içeriği/bağlantıları Editor'de doğrula; yeni `Menu` UI ile tamamla. |
| `Assets/Scenes/Level1.unity` | Var; mevcut oynanabilir 3D sahne ve sahne-kurucu çıktıları olabilir; açıp hiyerarşiyi incele. **Silme.** |
| `Assets/Scenes/CoreTest.unity` | Ortak fizik test sahnesi; denge, raycast, kamera ve deprem birim testleri burada. |
| `Assets/Scripts/Core/{Bootstrap,GameManager,SceneLoader,AudioManager,Juice,Contracts}.cs` | Mevcut ortak altyapı; scene yükleme/phase/ses/shake sözleşmesini **koru**. |
| `Assets/Scripts/UI/StatePanels.cs` | Mevcut Pause/Results/GameOver panel bağlayıcısı; yeniden kullan veya mevcut API ile uyumlu iyileştir. |
| `Assets/Levels/Level1/L1_ProjectGameController.cs` | **901 satır, var.** 5 afet, katalog, 750 bütçe, tork, hazırlık, rapor ve mağaza mantığını tek sınıfta yönetiyor. Level 1 deprem akışını ayrıştır; çalışan mekaniği yok etme. |
| `Assets/Levels/Level1/L1_ProjectGameController.UI.cs` | **IMGUI `OnGUI()` arayüzü.** HUD, market, başlangıç, nasıl oynanır, duraklatma ve rapor mevcut; üretim UI'sini `Canvas`/uGUI prefablarına taşı, eski IMGUI'yi kontrollü devreden çıkar. |
| `Assets/Levels/Level1/L1_PropDrag.cs` | Yeni Input System'li fare/touch seçme ve sürükleme; ayrıca `E` ile karakterin yakınından eşya alma/bırakma. **Mevcut sürüklemede dikey `maxLift=1.1f` var; son 2D kararına göre sadece yatay taşımaya çevir.** |
| `Assets/Levels/Level1/L1_MovableProp.cs` | `IFurnitureItem`, `balanceMassKg`, `Rigidbody`, `BoxCollider`, `SetAnchored`. Fiziğe bağlı kütle ile oyun dengesi kütlesi ayrılmış; koru. |
| `Assets/Levels/Level1/{L1_ResidentController,L1_LadderClimb,L1_ClimbRoute,L1_TouchControls}.cs` | Karakter ve merdiven altyapısı mevcut. Level 1'de tek kata uyarlayıp sonraki katların kodunu silme. |
| `Assets/Levels/Level1/Editor/L1_CozyPlayableBuilder.cs` | **68 KB Editor builder.** Asset yollarını çözüp evi/ortamı kurar. `Son Denge > Level1 > Oynanabilir 3D Cozy Evi Kur` çağrısı mevcut görsel kökü **yeniden inşa ederek elle yapılanları silebilir**; sahnenin yedeği/commit'i olmadan çalıştırma. |
| `Assets/Levels/Level1/Art/*`, `Materials/*`, `Meshes/*` | Dağ, gökyüzü, özgün kaplamalar, zeminler ve önizlemeler var. Bunları önce kullan. |
| `Assets/LowPolyInteriorsFree/`, `Assets/Low Poly Tarvern Pack/`, `Assets/3Digitalis/`, `Assets/EmaceArt/`, `Assets/Ata Khani/` | Üçüncü taraf 3D model/prefab paketleri zaten branch'te bulunuyor. Yeniden indirme yerine `AssetDatabase` ile önce bu paketleri tara. |
| `Assets/StylArts/`, `Assets/ZNS3D/`, `Assets/FarmHouse/` | Builder kaynak kodunda bu klasörlerden varlıklar kullanılıyor. Varlık varlığı ve lisans eşleşmesi Editor'de ayrıca doğrulansın. |

**Koddan kesin görülen güncel davranışlar:** `L1_ProjectGameController` 5 ayrı afet tipini tek Level1 sahnesinde sıraya sokuyor; Level 1 başlangıç bütçesi `750`, yeni bölüm bonusu `450`, hazırlık `45s`; Level 1 deprem süresi kaynak entegrasyon notuna göre `10s` (eski GDD `25s`), tork/denge eşiği `26°`. Güncel `L1_ProjectGameController.UI.cs` küçük yatay su terazisi çiziyor. `L1_PropDrag` fareyle raycast sürüklüyor, eşya taşıma için karaktere yakınlığı kontrol ediyor; mevcut davranış “yalnız yatay sürükleme” kararıyla tam aynı değil. Bunlar **kaynak kod okumasıdır**, çalışır build onayı değildir.

### 1.2 Dikkat: Git LFS ve varlıkların doğrulanması

GitHub ağacında `.png`, `.mp3`, bazı `.tga` girdileri yaklaşık **128–133 bayt** görünüyor. Bu dosyalar gerçek görsel/ses içeriği yerine **Git LFS pointer** olabilir. Yalnız repo ağacında görünmeleri, Editor'e gerçek binary verinin geldiğini doğrulamaz.

```bash
git lfs install
git lfs pull
git lfs ls-files
git status
```

Ardından Unity Project Browser'da kaynak görsellerin gerçek çözünürlükle açıldığını, prefab'lardaki model ve texture referanslarının *Missing* olmadığını doğrula. LFS indirme eksikse kod üretmeye devam etmeden düzelt.

### 1.3 ÇELİŞKİLERİ SESSİZCE GEÇME — karar matrisi

| Konu | Eski repo belgeleri / mevcut kod | Kullanıcının son 2D tasarım kararı | **Bu işte uygulanacak karar** |
|---|---|---|---|
| Level 1 kat sayısı | GDD: 2 kat × 2 oda; mevcut builder: 3 katlı ev, 3. kat ileride açılıyor | **Level 1 yalnız tek geniş oturma odası**, yeni katlar ileriki bölümlerde ekleniyor | Level1'de **tek geniş oda**, başlangıçta ikinci/üçüncü kat görünmez ve fizik yükü yok; eski geometri geleceğe korunur. |
| Level 2 / 3 | Eski repo 2: deprem veya fırtına, 3: yangın; güncel controller 5 farklı afet sırasını Level1'de çalıştırıyor | Level2: yukarıdan düşen **mutfak bloğunu** hizalama + fırtına; Level3: yatak odası bloğu + yangın | **Bu dökümanın kapsamı Level1 + Menü.** Gelecekteki bölümleri not et; kişilerin branch'lerine dokunma. |
| Çatı / biçim | Builder: eğimli kiremit çatı | **Üçgen/eğimli çatı yok**, düz modüler katlar | Level1 yalnız **düz tavan/döşeme**, yatay modüler kat. |
| Oyuncu | Repo: karakter A/D ve E ile yakın nesneyi taşıyabiliyor | 2D'de ana etkileşim fareyle nesneyi **yalnız yatay** sürüklemek | Fare ana kontrol; karakter hareketi opsiyonel ikinci kontrol; yakınlık şartı fareyi engellemeyecek. |
| Kamera | FOV40 perspektif, sabit açı | Zoom-in / zoom-out ve sahneyi rahat takip | **Dönmeyen perspektif yan kamera**, FOV veya dolly ile kontrollü zoom; uygun durumda yatay/az dikey pan. |
| HUD | Önceden daha büyük merkez bar | **Üstte küçük yatay denge göstergesi**, ev görünümünü kapatmayan HUD | Kompakt gauge. Çatı üstü dev UI yok. |
| Ortam | Dağ spritesi temel sahne | Zengin dağ/orman/bulut/sis, yan çıkış kapısı ve ahşap platform | Katmanlı arka plan, yan kapı, geçiş platformu; deprem sırasında tahliye bonusu yok. |
| Bütçe | Eski GDD 100; güncel branch 750 | Mevcut 2D entegrasyonun güncel ekonomisi | Şimdilik **750** ve sabitleme **30**, `LevelSetup` üzerinden ayarlanabilir. |
| Deprem süresi | Eski GDD/Task 25s; son projeye entegre 2D kuralları 10s | Oynanabilir, tekrar edilebilir deprem | İlk testte **10s**, Inspector'dan 10–25s ayarlanabilir; menü testinde kısa tutorial. |
| Çöküş | Eski 28°/1s; entegre kod 26° | Eğim/tehlike net anlaşılmalı | `26° + süre şartı` başlangıç; 7°/16° durumsal HUD; sayıları ScriptableObject'te. |
| Arayüz teknolojisi | Level1 içinde `OnGUI()` | Daha iyi 2D estetik/işlevsellik | `Canvas`/uGUI + prefab, eski UI'yi geçici geliştirme modunda tut; çakışmadan kaldır. |

> **Takım koordinasyonu:** `docs/OYUN_TASARIMI.md` “bağlayıcı” olarak işaretlenmiş. Buradaki yeni kat/çatı/Level2-3 kararları kullanıcıdan geldiği için Lead olarak *onaylı tasarım revizyonu* ayrı, kontrollü commit'le belgelenmeli; diğer takım arkadaşlarının Level2–4 işleri habersiz değiştirilmemeli.

---

## 2. OYUN HEDEFİ VE OYNANIŞ KABUL TESTİ

**Tür:** stilize 3D görünümlü, 2D yan düzlemde oynanan fizik ve afet hazırlık stratejisi.  
**Premis:** sivri kaya sütununun tepesindeki modüler evde mobilyaları dengeleyerek deprem öncesi risk azalt.  
**Anlatı:** “Burası Sivritepe. Burada her şey biraz dengesiz.”  
**Level1 adı:** `İLK SARSINTI` · **Mekân:** tek katlı oturma odası · **Afet:** deprem.  
**Temel keyif döngüsü:** sürükle → dengeyi gör → sabitle/alışveriş → deprem → kayma/düşme → afet raporu → tekrar oyna/sonraki bölüm.

### 2.1 Gerçek 3D, 2.5D kontrol

- Camera `(0, yaklaşık 2, -14)` başlangıç perspektifi; `FOV=40` referans; **kamera roll/yaw ile dönmez**.
- Oyun eksenleri: `X` yatay; `Y` yukarı; `Z` derinlik. Oyun nesneleri hareketi XY, ev eğimi **Z rotasyonu**.
- Tüm taşınabilir eşyalarda **3D** `Rigidbody` + uygun 3D `BoxCollider`/`CapsuleCollider`; `Freeze Position Z`, `Freeze Rotation X/Y`. 2D collider veya Rigidbody2D kullanma.
- Gölgelendirme `URP/Lit`, low-poly stil, bir Unity birimi bir metre. Görsel ölçek ve collider boyutu birbirine uymalı.
- Arka duvar, yan duvar ve zemin 3D; **ön duvar görünmez**. kesit/dollhouse okuması net.
- Mobilya sürüklemek eğimi gerçek zamanlı etkiler; ev pivot çevresinde **görünür şekilde** döner. Gösterge ile görünen dönüş yönü tutarlı.

### 2.2 Level1 ilerleme aşamaları

1. `Menu`: arka planda ev + dağ görseli / hafif parallax; “OYNA”, “NASIL OYNANIR”, “AYARLAR”, “ÇIKIŞ”.
2. `Level1` açılışı: “RADYO SİVRİTEPE — Bu gece sarsıntı bekleniyor. Hazırlan!” kısa mesaj ve 1 katlı ev.
3. Rehber 1: “Eşyayı sol tıkla tut, **yalnız sağa–sola** sürükle”; sahne canlı gösterir.
4. Rehber 2: “Ağır eşyalar sivri kayanın üzerinde daha fazla denge etkisi yaratır”; eğim barı güncellenir.
5. Rehber 3: “Kitaplığı sabitle”; bütçe düşer, eşya üzerinde çapa simgesi çıkar.
6. Rehber 4: mağazadan yeni eşya seçme (isteğe bağlı atlanabilir), bütçe yetersizliği ve geçersiz yerleştirme açıklanır.
7. `AFETE HAZIRIM` → onay/uyarı → 3, 2, 1 geri sayım → deprem.
8. Depremde yatay yer sarsıntısı, serbest mobilya kayması/devrilmesi, hafif kamera shake, ses/toz; sınırlı fantastik müdahale enerjisi (zorunlu öğretici değil). Kayan eşya kütle hesabında yeni yerine geçer; düşen eşya hesaptan çıkar.
9. `AFET RAPORU`: ev ayakta mı, seçili sabitlemeler, korunmuş/kırık/kaybolan eşya, hazırlık puanı, kazanılan para, güvenlik mesajı; `TEKRAR DENE`/`SONRAKİ BÖLÜM`/`ANA MENÜ`.
10. `SONRAKİ BÖLÜM` yalnız Level2 sahnesinin bağımsız oynanışı doğrulanmışsa aktif; değilse “Yakında” / pasif olup hata fırlatmaz. Ancak mevcut `SceneLoader` API'sine uyum korunur.

### 2.3 Level1 görsel planı

```text
                [ RADYO SİVRİTEPE ]   [ mini denge çubuğu ]    [ Bütçe: 750 ]

                yumuşak gökyüzü · uzak dağ · bulut / orman · sis

                         ┌── DÜZ TAVAN / MODÜLER KAT ──┐
             giriş/      │                              │
             ahşap       │  🛋️          ▤          🪑    │   tek geniş oda
             platform ── │                              │
                         └──────── AHŞAP ZEMİN ────────┘
                                    ● pivot
                                    ╱╲
                                   ╱  ╲     sivri kayalık
                                  ╱    ╲
                     [ Seçili eşya • kg • Sabitle ] [ Mağaza ] [ Afete Hazırım ]
```

ASCII yerleşim **ölçek çizimi değildir**. Oyun 16:9 ekranda ev, kaya ve arka planı tek kadrajda okutmalıdır. Level1'de hiçbir iç bölme / üst kat dekorasyonu, açık alana tıklama veya mobilya seçimini engellememeli.

---

## 3. ASSET ARAŞTIRMASI: ÜCRETSİZ, LİSANSLI VE GERÇEK İHTİYAÇLA EŞLEŞMİŞ

**İlke:** önce repodaki asset'i **yeniden kullan**. Eksikse Kenney / Quaternius **CC0** varlıklarını indir. Unity Asset Store “FREE” **CC0 demek değildir**: Standart EULA çerçevesinde gömülü oyunda kullanılır ama çıplak `.fbx`/`.png` paketlerini ayrı ürün gibi dağıtma hakkı vermez. Üçüncü taraf paketleri indirirken mümkünse ilgili hesabın lisans/ekip erişimini doğrula. Lisansı doğrulanmamış veya paket kapsamı belirsiz asset'i finalde kullanma.

### 3.1 **Önce mevcut repo assetleri** (dışarıdan yeniden yükleme gerekmez)

| Varolan kaynak | Yollar / doğrulanabilir adlar | Level1 / Menu kullanımı | Uygulama |
|---|---|---|---|
| **LowPolyInteriorsFree / DOLLHOUSE kit** | `Assets/LowPolyInteriorsFree/Prefabs/` | Kanepe, kitaplık, oturma odası parçaları, düz döşeme/kapı adayları | Prefab Preview ile seç; scale / collider / URP normal kontrolü. Kaynak URL `README`de; web erişiminde **ürün sayfası doğrulanamadı**, mevcut yerel paket ve lisans metadata öncelikli. |
| **Low Poly Tarvern Pack** | `Assets/Low Poly Tarvern Pack/Prefabs/Probs/` | Masa, sandalye, fener, sandık gibi rustik parçalar | Stil uygun prefabları kopyalamadan referans üzerinden Level1 prefab variant olarak kullan. |
| **3Digitalis Low Poly Interior Props Pack** | `Assets/3Digitalis/LowPolyInteriorPropsPack/Prefabs/P_Books_01.prefab`, `P_Clock.prefab`, `P_Plant.prefab`, `P_Shelf.prefab` | Raf, kitap, saat, bitki | **Sabit dekor**; fizik oyun kütlesine girmez. |
| **Modular Sewer Props** | `Assets/Ata Khani/Modular Sewer Props/Prefabs/Wooden_Plank_01.prefab`, `Lamp_Hanging_2M.prefab` | Ahşap zemin/yan platform ve sarkan lambalar | URP materyal dönüştürme ve collider kontrollü. |
| **Roadside Tales Free — Fences** | `Assets/EmaceArt/Fences 2026/...` | Ahşap dış platform korkuluğu | Fizik collider oyunun oda sınırına engel olmamalı. |
| **Hazır Level1 üretimleri** | `Assets/Levels/Level1/Materials/L1_3D_*`, `Meshes/L1_Wall_*.asset`, `Art/L1_MistyMountainValley.png`, `L1_DuskSky.png`, `L1_RockSpire.png` | Sıva, zemin, dağ, sis, renkler | Texture gerçek binary mi / import ayarı doğru mu kontrol et. |
| **Diğer paket yolları** | `Assets/StylArts/...`, `Assets/ZNS3D/Mix_Furniture_Pack/...`, `Assets/FarmHouse/...` | Mevcut builder'daki kanepe, kitaplık, sandalye, buzdolabı vb. | Tek tek içeriği doğrula. Lisansları `.unitypackage`/Asset Store kayıtlarıyla eşleştir. |

**Paket mağazası kayıtları:** [Modular Sewer Props](https://assetstore.unity.com/packages/3d/props/modular-sewer-props-277458), [Roadside Tales Free](https://assetstore.unity.com/packages/3d/environments/landscapes/roadside-tales-free-modular-fence-narrative-pack-133601), [Yughues Free Wooden Floor Materials](https://assetstore.unity.com/packages/2d/textures-materials/wood/yughues-free-wooden-floor-materials-13213). Bu paketler **Unity Asset Store EULA** kapsamındadır; kaynaklarını ayrıca oyunun içine koyup indirme linksiz dağıtma veya AI eğitimi için kullanma.

### 3.2 Yeni indirilebilecek **ücretsiz CC0 assetler** — öncelik sırasıyla

| Öncelik | Paket (doğrudan kaynak) | İçerik / doğrulanan sayı | Kullanım | Lisans / not |
|---|---|---:|---|---|
| **P0** | [Kenney Furniture Kit](https://kenney.nl/assets/furniture-kit) | **140** 3D obje | Sandalye, masa, kanepe, kitaplık alternatifleri | **CC0**, düşük poligon; birinci yedek |
| **P0** | [Quaternius Ultimate House Interior Pack](https://quaternius.com/packs/ultimatehomeinterior.html) | **123** model; FBX/OBJ/Blend | Düz oda, pencere, kapı, oturma odası, ek kat parçaları | **CC0**; en iyi mimari tamamlayıcı |
| **P0** | [Kenney UI Pack](https://kenney.nl/assets/ui-pack) | **430** UI parçası | Başlat / mağaza / küçük HUD / buton / slider | **CC0**; seçilen birkaç parça ile özel stil tasarla |
| **P0** | [Kenney Input Prompts](https://kenney.nl/assets/input-prompts) | **1500** prompt/glyph | `A/D`, `E`, `F`, `ESC`, fare, scroll açıklaması | **CC0**; sadece gereken ikonları projeye al |
| **P0** | [Kenney Interface Sounds](https://kenney.nl/assets/interface-sounds) | **100** ses | Hover, tıklama, onay, hata | **CC0** |
| **P0** | [Kenney Impact Sounds](https://kenney.nl/assets/impact-sounds) | **130** ses | Mobilya düşmesi, çarpma, tahta gıcırtısına aday | **CC0**; sesleri dinleyerek seç |
| **P1** | [Quaternius Furniture Pack](https://quaternius.com/packs/furniture.html) | **23** model | Mevcut pakette görünüm uyuşmazsa koltuk/masa | **CC0**; hızlı alternatif |
| **P1** | [Kenney Nature Kit](https://kenney.nl/assets/nature-kit) | **330** 3D obje | Kaya zirvesi, uzakta ağaçlar, bulutların çevresi | **CC0**; uzak plan 3D objeler |
| **P1** | [Quaternius Ultimate Stylized Nature](https://quaternius.com/packs/ultimatestylizednature.html) | **63** model; FBX/OBJ/glTF/Blend | Dağ/orman/sis atmosferi | **CC0**; çok fazla iç içe model yığma |
| **P1** | [Kenney UI Pack – Adventure](https://kenney.nl/assets/ui-pack-adventure) | **130** UI parçası | Taş/ahşap hissiyatlı ayarlar ve rapor | **CC0**; aynı ekranda 2 farklı UI paketi rastgele karıştırma |
| **P1** | [Kenney Game Icons](https://kenney.nl/assets/game-icons) | **105** ikon | Para, ayar, bilgi, yeniden başlat | **CC0**; ankraj ikonu gerekirse özgün çiz / seç |
| **P1** | [Kenney Particle Pack](https://kenney.nl/assets/particle-pack) | **80** VFX sprite | Toz, sarsıntı noktaları, uyarı pulse | **CC0**, URP Particle System ile göster |
| **P1** | [Kenney RPG Audio](https://kenney.nl/assets/rpg-audio) | **50** ses | Adım/ahşap hareketleri adayları | **CC0** |
| **P2** | [Kenney Music Jingles](https://kenney.nl/assets/music-jingles) | **85** müzik jingle | Menü/başarı/kayıp kısa sting | **CC0**; uzun süreli ambiyans yerine jingle |
| **P2** | [Quaternius Ultimate Nature Pack](https://quaternius.com/packs/ultimatenature.html) | **150** model | Daha dolu panoramik dağ peyzajı | **CC0** |

**Lisans bilgisi doğrulanan resmi kaynaklar:** [Kenney lisans ve atıf SSS](https://kenney.nl/support), [Unity Asset Store Terms & EULA](https://unity.com/legal/as-terms). Quaternius her listelenen paket sayfasında CC0 bildiriyor. **Yükleyici paketinin içindeki lisans dosyasını da sakla.**

### 3.3 P0 minimum asset listesi — *paket değil gerçekten kullanılacak nesne*

| Gereken asset | İlk tercih | Yedek/üretim | Üretilecek Unity prefabı |
|---|---|---|---|
| Oda arka duvarı/yanlar | Mevcut `L1_3D_AgedPlaster` materyali ve oluşturulan mesh'ler | Quaternius House Interior duvar parçaları | `L1_RoomShell.prefab` |
| Düz tavan, döşeme, kapı, kapı kasası | Repo `L1_3D_WornOak` / `L1_SalvagedPlanks` | Quaternius House Interior | `L1_FlatRoofAndFloor.prefab` |
| Yandan görülen sivri kaya, pivot | Mevcut `L1_RockSpire.png`, `L1_PivotStone.mat`; tercihen özgün 3D mesh | Quaternius/Kenney kaya parçalarını gruplayarak | `L1_RockSupport.prefab` |
| Kanepe | Mevcut DOLLHOUSE / Mix sofa | Kenney Furniture | `L1_Furniture_Sofa.prefab` |
| Kitaplık | Mevcut Mix / DOLLHOUSE bookshelf | Kenney / Quaternius | `L1_Furniture_Bookshelf.prefab` |
| Sehpa / masa | Mevcut Tavern/3D | Kenney Furniture | `L1_Furniture_CoffeeTable.prefab` |
| Sandalye veya tekli koltuk | Mevcut StylArts / Tavern | Kenney Furniture | `L1_Furniture_Chair.prefab` |
| Lambader / dekor | Mevcut 3Digitalis / Tavern | Quaternius Interior | `L1_Decor_Lamp.prefab` |
| Çapa/sabitleme simgesi | UGUI procedural vector veya Kenney ikon setinden uygun ikon | Özgün basit çizgi SVG/PNG | `L1_AnchorMarker.prefab` |
| Bulut, sis, orman | Mevcut `L1_MistyMountainValley.png`, `L1_DuskSky.png` | Kenney Nature / Quaternius | `L1_BackgroundLayers.prefab` |
| HUD/buton/market paneli | Kenney UI Pack + özel 9-slice | uGUI `Image` + tek renk/çerçeve | `L1_HUD.prefab`, `L1_ShopPanel.prefab` |
| Fare/klavye açıklaması | Kenney Input Prompts | metin fallback | `L1_InputHint.prefab` |
| SFX | mevcut `L1_ProjectSfx.cs` + Kenney Interface/Impact | özgün prosedürel ses | `L1_AudioBank.asset` |

**Önemli:** Paketlerin indirilmiş arşivlerinin iç dosya adlarını buradan doğrulamadım; **yukarıdaki üretilecek prefab adları bizim standart önerimizdir, paket dosya adları değildir.** MCP ajanı arşivi açıp Unity `AssetDatabase.FindAssets`/Project penceresiyle gerçek dosyayı tespit etmeden sabit vendor yolu uydurmasın.

### 3.4 Ses ve müzik kararı

- Mevcut `Assets/Levels/Level1/Audio/bgm_cozy_mountain.mp3` ve `bgm_disaster_tension.mp3` dosyalarını önce incele (LFS dahil). **Kaynağı ve kullanım hakkı belgeliyse** kullan; değilse ambiyansı kısa, özgün sentez veya açık lisans alternatifleriyle değiştir.
- `Sfx_Select`, `Sfx_DropLight`, `Sfx_DropHeavy`, `Sfx_Anchor`, `Sfx_Purchase`, `Sfx_Error`, `Sfx_QuakeRumble`, `Sfx_TimberCreak`, `Sfx_Notification`, `Sfx_ResultWin/Fail` gerektiğinde Kenney seslerinden seçilip kısa listeye konmalı.
- URP ile fizik sürükleme sesi sürekli spam yapmamalı; olay tabanlı ve frekans limiti olmalı. SFX 0–1, müzik 0–1, master 0–1; ayarlar menüsünde ses slider'ı. UI butonları tıklandıkça yalnız bir click çalmalı.

---

## 4. ÖNERİLEN KLASÖR AĞACI (MEVCUT DOSYALARI KORU)

```text
Assets/
├─ Scenes/
│  ├─ Menu.unity                   # Lead
│  ├─ Level1.unity                 # Lead
│  └─ CoreTest.unity               # Lead
├─ Levels/Level1/
│  ├─ Art/                       # mevcut texture/sprite/attributions korunur
│  ├─ Materials/                 # mevcut URP materyalleri korunur
│  ├─ Meshes/                    # mevcut .asset mesh'ler korunur
│  ├─ Editor/                    # mevcut builder script'leri korunur, körlemesine çalıştırma
│  ├─ Prefabs/
│  │  ├─ L1_RoomShell.prefab
│  │  ├─ L1_Furniture_*.prefab
│  │  ├─ L1_RockSupport.prefab
│  │  └─ L1_LevelComposition.prefab
│  ├─ UI/
│  │  ├─ L1_HUD.prefab
│  │  ├─ L1_ShopPanel.prefab
│  │  ├─ L1_TutorialOverlay.prefab
│  │  ├─ L1_ReportPanel.prefab
│  │  └─ L1_FurnitureTooltip.prefab
│  ├─ Data/
│  │  ├─ L1_LevelConfig.asset
│  │  ├─ L1_FurnitureCatalog.asset
│  │  └─ L1_AudioBank.asset
│  ├─ Tests/                     # edit/play testler, namespace Jam.Levels.Level1.Tests
│  ├─ L1_LevelOrchestrator.cs     # Level1 state ile GameManager arası adapter
│  ├─ L1_CameraController.cs      # zoom/pan/focus, sahne kamera açısı değişmez
│  ├─ L1_DragPlacement.cs        # mevcut L1_PropDrag üstünden güvenli refactor
│  ├─ L1_HouseBalanceController.cs
│  ├─ L1_EarthquakeController.cs
│  ├─ L1_AnchoringService.cs
│  ├─ L1_LevelScore.cs
│  └─ L1_FurnitureView.cs
├─ Prefabs/UI/                   # Lead ortaklaştıysa Menu UI prefabları
├─ Scripts/Core/                # mevcut API'ler korunur
├─ Scripts/UI/                  # Lead'in ortak UI alanı
└─ Art/                         # başkasının alanı: yeni vendor assetleri buraya kendi başına dökme

docs/
├─ LEVEL1_MENU_IMPLEMENTATION_PLAN.md
└─ ASSET_ATTRIBUTIONS_LEVEL1.md
```

**Vendor import stratejisi:** Harici CC0 assetler takımca paylaşılmış `Art/` alanı içine **yalnız Lead onayıyla** alınıp bütün oyun için merkezî yönetilir; kişisel Level1 assetleri `Assets/Levels/Level1/` altında tutulabilir. Repo kuralları gereği paket/folder sınırlarını çiğneme. Yeni paket yüklemek için `Packages/manifest.json` değiştirmen gerekiyorsa önce onay al. Unity dosyaları Editor içinden taşı; `.meta` dosyalarını koru.

---

## 5. ANA MENÜ — TAM TASARIM VE ETKİLEŞİM

### 5.1 Görsel kompozisyon

- **Çözünürlük:** 16:9 `1920×1080`, Canvas `Screen Space – Overlay`, `Canvas Scaler: Scale With Screen Size`, `Reference 1920×1080`, `Match=0.5`.
- Oyunun arka planında tek katlı ev ve dik kayalık/orman panoraması; yavaş sis ve bulut (menüde oynanış fiziği başlamaz). Ev görseli sağda/ortada; metin ve menü sol/orta alanda. Menüde **siyah neon, cyberpunk, web dashboard veya fazla kart kullanma**.
- Başlık: **SON DENGE**, küçük alt metin: *“Afet gelmeden önce dengeni kur.”* Butonlar: `OYNA`, `NASIL OYNANIR`, `AYARLAR`, `ÇIKIŞ`. Footer: sürüm ve küçük lisans/attribution bağlantısı.
- Stil: sıcak krem, mat ahşap, soft teal, toz turuncu, zeytin yeşili; sıcak gölgeler; keskin okunabilir Türkçe tipografi. Prototip IMGUI kağıt/ahşap tonu korunabilir ama kalite uGUI ile yükseltilmeli.
- Buton hover: 0.12–0.18s renk/scale değişimi; buton etkinlik sesi; paneller 0.2–0.3s fade; 16:9 ekranın %40'ını aşmayan menü paneli.

### 5.2 İşlevsel akış

| Menü elemanı | Eylem | Bitiş kriteri |
|---|---|---|
| `OYNA` | `SceneLoader.Instance.Load("Level1")` | fade geçişi, tek Level1 instance, UI tek EventSystem. |
| `NASIL OYNANIR` | Modal: seçim/sürükleme, denge, sabitleme, afet, kontrol glyphleri ve gerçek yaşam uyarısı | ESC/geri ile kapanır; içerik kaydırılabilir. |
| `AYARLAR` | master/music/SFX, çözünürlük/pencere modu (Windows), sarsıntı azaltma, kamera hızı, yazı okunabilirliği | kaydet/yükle `PlayerPrefs` ya da proje ayar servisi ile; varsayılana döndür. |
| `ÇIKIŞ` | `SceneLoader.Instance.Quit()` | Editor'de Play biter; Windows build'te uygulama kapanır. |
| `Emeği geçenler / Atıflar` | Küçük panel | asset kökenleri ve lisansları listeler. |

**Sahne tek otoritesi:** `GameManager.SetPhase(GamePhase.Menu)` menü aktif olduğunda çağrılmalı. Menüde Level1'in `OnGUI` açılış menüsü **ikinci kez** görünmemeli. `Menu.unity` ile `Level1.unity` iki ayrı sahnedir; GameManager ve SceneLoader sahnelere elle eklenmez, Bootstrap kurar.

### 5.3 HUD ölçü ve yerleşim (Level1)

- **Sol üst:** `SEVİYE 01 · İLK SARSINTI`, küçük `RADYO SİVRİTEPE` uyarısı (dikey alan en çok ~110 px).
- **Üst orta:** **KOMPAKT** yatay denge barı, `GÜVENLİ / DENGESİZ / KRİTİK`, açı ör. `+4.2°`, sağ/sol yön oku; yaklaşık 220–280 px genişlik (1920 referans çözünürlük). Evin üzerinde büyük denge ikonu / dev çatı göstergesi **yok**.
- **Sağ üst:** 750 para göstergesi, 45s hazırlık saati (tutorial açıkken durabilir), gerektiğinde geri sayım.
- **Alt sol:** seçili eşya adı, oyun kütlesi, değer ve sabitli mi; tooltip/outline.
- **Alt orta:** `SABİTLE (30)`, `MAĞAZA` (ikon+kelime), `GERİ AL` (hazırlıkta), faz düğmesi `AFETE HAZIRIM`.
- **Alt sağ veya kamera köşesi:** `+`, `−`, `Sığdır` kamera düğmeleri; zoom UI yalnız seçim modunu bozmadan kullanılmalı.
- **Afet sırasında:** 3-2-1, `DEPREM!`, kalan saniye, tehlike, enerji barı ve gerçek yaşam güvenlik satırı.
- **Rapor:** `EV AYAKTA KALDI` / `DENGE BOZULDU`, 4–6 veri ve davranışa bağlı doğru mesaj. Skor miktarlarını UI sabitleri yerine gerçek modelden çek.

### 5.4 UI Prefab hiyerarşisi

```text
[MenuCanvas]                         [Screen Space Overlay, sorting order 100]
 ├─ BackgroundGradient
 ├─ TitleGroup
 ├─ MenuButtons
 │   ├─ PlayButton
 │   ├─ HowToButton
 │   ├─ SettingsButton
 │   └─ QuitButton
 ├─ HowToPanel (default inactive)
 ├─ SettingsPanel (default inactive)
 └─ CreditsPanel (default inactive)
[Level1Canvas]                       [Screen Space Overlay, sorting order 100]
 ├─ TopHud
 │   ├─ RadioAlert
 │   ├─ BalanceGauge
 │   └─ EconomyAndTimer
 ├─ BottomBar
 ├─ ZoomControls
 ├─ FurnitureTooltip
 ├─ ShopDrawer (inactive)
 ├─ TutorialOverlay
 ├─ CountdownOverlay (inactive)
 ├─ DisasterNotice (inactive)
 ├─ PausePanel (inactive)
 ├─ ResultPanel (inactive)
 └─ GameOverPanel (inactive)
```

UI raycast yalnız interaktif kontrol üzerinde olsun; dekoratif UI `Raycast Target=false`, saydam panel arka planı ev mouse click'ini yutmasın. Bir sahnede **en fazla bir** EventSystem; `Input System UI Input Module` kullan.

---

## 6. LEVEL1 FİZİK — GERÇEK TORK, STABİL SAHNE

### 6.1 Sahne hiyerarşisi önerisi

```text
Level1
├─ Main Camera                  # existing Camera/AudioListener, unique
├─ Directional Light            # existing, unique
├─ [L1] Background              # far sky/clouds, mountain silhouettes, fog
├─ [L1] RockSpire               # structural/visual static rock; collision optional
├─ [L1] HousePivot              # rock contact, center rotation along local Z
│  └─ [L1] HouseRoot            # wooden base + flat roof + back wall + side door
│     ├─ Room_Living
│     ├─ FloorColliders
│     ├─ WallColliders
│     ├─ StaticDecor
│     └─ ExitPlatform            # platform may need attach or remain external by design
├─ [L1] DynamicFurniture       # independent rigidbodies (NOT children of rotated collider root)
│  ├─ Sofa
│  ├─ Bookshelf
│  ├─ CoffeeTable
│  └─ Chair
├─ [L1] Gameplay               # balance, disaster, economy, tutorial, score
├─ [L1] UI                     # Canvas, EventSystem
└─ [L1] Audio                  # scene-scoped sources, not persistent duplicate
```

**Kritik fizik notu:** Eşyalar `Rigidbody` olarak dönmekte olan kinematik ev/kat collider'larının altında gelişigüzel parent'lanmamalı. **Kinematik HousePivot/HouseRoot** dönerken collider hareketi fizik adımında düzgün güncellenmeli; bağımsız dynamic mobilyalar world-space pozisyonlarını fizik çözücüsünden almalı; oyun dengesi hesaplamasında bunların `HousePivot.InverseTransformPoint(worldPosition).x` değeri kullanılmalı. Sabitlenmiş mobilya kinematik olarak evin **local** pozunu takip etmeli ve sabitleme çözülünce dünya uzayında fizik hareketi geri açılmalı. Ani `transform` teleportları sürtünme/çarpışma kaymasına neden olmamalı.

### 6.2 Denge hesabının önerilen, test edilebilir modeli

Fizik ve oyun puanı kütlesi iki ayrı değerdir. `Rigidbody.mass` çarpışma çözümü için, `IFurnitureItem.Mass` / `balanceMassKg` **oyun dengesi** için kullanılır.

```text
localXi = HousePivot.InverseTransformPoint(furniture.worldCenterOfMass).x
momentKgM = sum(aliveFurniture.balanceMassKg * localXi)
structuralMomentKgM = houseStructuralMass * houseLocalCenterOfMass.x
quakeMoment = earthquakeAcceleration(t) * (houseEffectiveLeverArm * effectiveMass)
T = g * (momentKgM + structuralMomentKgM) + quakeMoment - kSpring*theta - cDamping*omega
alpha = T / effectiveMomentOfInertia
omega += alpha * fixedDeltaTime
theta += omega * fixedDeltaTime
housePivot.MoveRotation(Quaternion.Euler(0,0,thetaRadiansInDegrees))
```

Bu, eğlenceli oyun simülasyonudur; tam bina mühendisliği hesabı değildir. Pozitif `localX` sağdaki yükü artırdığında görsel olarak **sağ aşağı** eğilme için Unity Z dönüşünün işareti doğru ayarlanmalı. `momentKgM` tek başına `N·m` değildir; `g` ve uzunluk ölçeği tutarlı kullanılmalı. `kSpring`/`cDamping`/`inertia` playtest'te ayarlanacak. Fizik `FixedUpdate` döngüsünde; UI `Update/LateUpdate` ile filtrelenmiş açıyı okur. `NaN`, `Infinity`, açı zıplaması, sonsuz hız, sürekli mikro-titreşim önlenmeli.

**Başlangıç eşiği (config):**

| Durum | Açı | UI / feedback |
|---|---:|---|
| GÜVENLİ | `0°–7°` | soft teal / küçük onay sesi |
| DENGESİZ | `>7°–16°` | hardal uyarı, ufak gıcırtı |
| KRİTİK | `>16°–26°` | mercan/kırmızı pulse, ses ve ekranda kırmızı durum |
| ÇÖKÜŞ | `≥26°` ~**1.0s kesintisiz** | GameOver + kısa çökme; anlık spike oyun bitirmez |

- Mobilya **taşındığı anda** önizleme torku değişir; ev birkaç frame içinde yumuşak cevap verir. Kütle 2× ise etki de aynı `x` konumunda yaklaşık 2×. Eşya merkeze yaklaşınca eğim düşer.
- Düşen, kırılıp yok olan veya yapıdan kopan eşya **bir kez** `UnregisterFurniture` olur. Hâlâ canlı ve evde kalan eşya yeni `x` konumuyla hesaba katılır.
- Sabitleme **kütleyi sıfırlamaz**, yalnız hareketini kilitler.
- Oyuncu karakteri mekaniği korunacaksa 8 kg oyun yükü olarak tek kez hesaplanabilir; hayalet “telekinezi” ana oyun mantığında karakter olmadan sürüklemeye izin vermelidir.
- `HouseStructuralMass`, `CenterOfMassOffset`, `GravityScale`, `SpringStrength`, `Damping`, `Inertia`, `SafeAngle`, `WarningAngle`, `CollapseAngle`, `CollapseHoldSeconds`, `EarthquakeTorqueScale` Inspector/ScriptableObject parametreleri olmalı.

### 6.3 Fizik ve sahne testleri

1. Tüm eşyalar ortadayken 5 saniye stabil; eğim `±0.5°` içinde.
2. 60 kg kanepeyi sağa götür → gösterge sağ aşağı ve ev sağa eğilir; sonra sola götür → ters tepki.
3. Aynı yere 20 kg masa ve 85 kg kitaplık koy → kitaplık daha güçlü tork üretir.
4. Kritik açı 26° bir anlık geçerse çökme yok; süre dolarsa çökme.
5. Ev dönerken yer, duvar ve anchored mobilya kaymaz; loose mobilya fiziksel olarak kayabilir.
6. Kaya referansı/hud ve zemin arasında “floating house” görünümü yok.
7. Kayıp eşya listeden yalnız bir kere silinir ve rapora bir kere eklenir.

---

## 7. MOBİLYA SEÇME, YALNIZ YATAY SÜRÜKLEME, SİLÜET VE YERLEŞTİRME

### 7.1 İstediğimiz kesin giriş davranışı

| Eylem | Kontrol | Uygulama |
|---|---|---|
| Eşya seç | Sol tık / touch | `Physics.Raycast` ile `Furniture` layer'ında 3D collider; seçili outline ve tooltip. |
| Sürükle | Sol tık basılı / parmağı hareket ettir | **Sadece oda local X** güncellenir; `localY` **zemine oturmuş sabit** konum; `localZ` sabit. `maxLift` kullanılmaz. |
| İptal | `ESC` (pause önceliği net) / sağ tık | Geçerli yerleşim başlamadıysa eski konuma geri; bırakma anında olay bir kez çalışır. |
| Bırak | Sol tuş release | Raycast/plane ile uygun X, `BoxCast`/`OverlapBox` yer kontrolü; yer yoksa orijinal pozisyona dön. |
| Sabitle | `F` veya HUD butonu | Yalnız hazırlıkta ve yeterli bütçeyle; `-30`; anchor ikonu; afette hareket etmez. |
| Sabitlemeyi sök | seçili sabitli eşyada aynı buton | Yalnız hazırlıkta; iade politikası belirlenir (varsayılan **ücret iadesi yok**, exploit önleme). |
| E ile al / bırak | Mevcut karakter sistemi varsa | Opsiyonel, yakınlık ister; **fare ile global sürüklemeyi engellemez**. |
| İşlem ipucu | Hover | `Kitaplık · 85 kg · 180 altın · Sabitli değil`. |

**Ray/plane tekniği:** `Mouse.current.position.ReadValue()` → `Camera.ScreenPointToRay` → seçili mobilyanın Z düzleminde `Plane.Raycast` → world hit → `HousePivot.InverseTransformPoint` → sadece X. Dünya dönüşü `HousePivot.TransformPoint(new Vector3(clampedX, originalLocalY, fixedLocalZ))`. Evin açı değiştirdiği sırada hedef sahne-local düzleme göre hesaplanır; zoom seviyesi seçimi bozamaz. UI üzerindeyse (`EventSystem.IsPointerOverGameObject` uygun pointer ID) world raycast başlatma.

### 7.2 Geçerli yerleşim

- Odanın açıklığı yaklaşık **8.0m**, kullanılabilir merkez aralığı `[-3.6m,+3.6m]` (gerçek collider boyutundan alınan yarım genişlik çıkarılarak).
- `BoxCollider.bounds` ve **gerçek mesh görünür sınırları** dikkate alınır; mobilya duvarın içinden geçmez, başka mobilyanın içinde spawn olmaz, havada kalmaz, UI'nin arkasında gizlenmez.
- Seçim sırasında rigidbody kontrollü `isKinematic`/`MovePosition` ile taşınır; serbest bırakınca `useGravity` ve fizik durumu geri gelir; 0.1s snapping easing isteğe bağlı.
- **Sadece local X hareketi**: mobil mobilyanın bulunduğu kat ve oda değişmez; Level2'de düşen yeni blok içinde ayrı bir mekanik olacak. Level1'de dikey sürükleme veya katlar arası eşya uçurma yok.
- Bırakma geçersizken mobilya kırmızı saydam dış hat; geçerliyken soft teal. Ghost preview kullanılıyorsa oyunun gerçek dengesini **iki kez** sayma.
- Sabitlenen eşyaya çapa ikonu ev dönerken eşyanın yanında kalmalı; zoom'da okunabilir boyuta uyarlanmalı.
- Afette taşınabilen serbest eşya için “sihir enerjisi” varsa azalma/dolum tek bir otoriteden yönetilir; enerji bittiyse hareket gerçekleşmez, anlaşılır bildirim çıkar. Bu **gerçek hayatta yapılmaması gereken fantastik oyun aksiyonu**.

### 7.3 İlk oturma odası başlangıç eşyaları ve ekonomi

| Eşya | Oyun kütlesi (mevcut katalogla uyumlu örnek) | Mağaza fiyatı | Başlangıç tercihi |
|---|---:|---:|---|
| Kanepe (`green_sofa`) | 60 kg | 150 | başlangıçta mevcut, orta-sol |
| Kitaplık (`bookshelf`) | 85 kg | 180 | başlangıçta mevcut, duvara yakın; sabitleme öğreticisi |
| Sehpa (`coffee_table`) | 20 kg | 60 | başlangıçta mevcut, orta |
| Tekli koltuk (`red_armchair`) | 35 kg | 90 | başlangıçta mevcut, orta-sağ |
| Lambader (`standing_lamp`) | 15 kg | 45 | mağazada opsiyonel / sabit dekor varyantı |

Bu tablo **mevcut `L1_ProjectGameController.Catalog` örnek değerlerini** temel alır. `Rigidbody.mass` aynı değerde olmak zorunda değildir. Başlangıçta yerleşim denge açısından **güvenli** olmalı; 4 eşya yeterlidir. Eğitim sırasında başlangıç pozisyonu oyuncuyu kesin çöküşe zorlamamalı.

---

## 8. KAMERA ZOOM / UZAKLAŞTIRMA / PAN / ODAK

**Görünüm asla dönmesin.** Üçüncü şahıs serbest orbit veya rastgele FPS kamera kullanılmasın.

### 8.1 Kontroller

- Fare tekerleği yukarı: **yakınlaş**, aşağı: **uzaklaş**.
- `+` / `-` UI butonları: aynı zoom metodu.
- `F` eşyayı sabitlemek için ayrıldığı için kamera focus ile çatışmayacak; kamera `Sığdır` düğmesi veya `Home` tuşuyla başlangıç kadrajına döner.
- Orta fare basılı veya `Space`+sol sürükleme: kontrollü pan (ancak `Space` afeti başlatıyorsa iki işlev birlikte kullanılmaz; **tercihen yalnız orta fare pan**).
- Touch: iki parmak pinch zoom ve iki parmak pan (P1); UI butonları bütün platformlarda çalışır.
- Mouse ile mobilya taşırken zoom komutu verilirse nesne aniden zıplamaz; drag plane tekrar hesaplanır.

### 8.2 Varsayılan parametreler

| Parametre | Başlangıç değer | Not |
|---|---:|---|
| Camera projection | Perspective | Repodaki teknik kurallarla uyumlu |
| Rest FOV | `40°` | Eski kamerayı korur |
| Minimum FOV | `27°` | Odayı incelemek için |
| Maximum FOV | `52°` | Kayalık + ev genel görünümü için |
| Zoom smoothing | `0.10–0.18s` | `unscaledDeltaTime` ile duraklatma sırasında UI bozulmaz |
| Pan X | oda/sahne sınırlarına göre | Sonsuz boşluk gösterme |
| Pan Y | küçük tolerans | Ev ve kaya kadrajdan çıkmasın |
| Shake | UI'den bağımsız çocuk offset | Zoom target / gerçek kamera koordinatını bozma |

`FOV` üzerinden zoom dolly'den daha basit ve daha güvenli olabilir; perspektif bozulması artarsa kamerayı Z ekseninde ileri-geri taşıyan *dolly* modeli seç, ikisini aynı anda karıştırma. Zoom imleç altında odak koruma istenirse zoom öncesi/sonrası ray-plane intersection farkını küçük kamera center offseti ile telafi et, ama kadraj sınırlarını kontrol et.

### 8.3 Kamera kabul testleri

- Mouse tekeri 20 kez çevrilse FOV sınırları korunur ve kamera ters bakmaz.
- En yakın zoom'da mobilya seçilebilir; en uzakta ev + kaya bütün olarak görünür.
- Orta fare pan sadece izinli sınırda çalışır; UI tıklaması pan başlatmaz.
- Deprem shake'i bitince kamera zoom/pan **doğru konuma** döner; kalıcı drift olmaz.
- `Pause` ve `Report` açıkken zoom devre dışı veya politika gereği kontrollü; diğer panellerin butonları doğru çalışır.

---

## 9. MAĞAZA, SABİTLEME VE HAZIRLIK SÜRESİ

### 9.1 Mağaza tasarımı

- Alt `MAĞAZA` düğmesiyle **sağdan açılan küçük çekmece** (~410–440px), sahneyi tamamen kapatmayan yarı saydam arka plan.
- Level1 filtreleri: `Mobilya` ve `Deprem Önlemleri`; yangın, yağmur, paratoner vb. sonraki seviyelerde açılacak fakat katalog/ürün tanımı kodda bozulmayacak.
- Her öğe: küçük render thumbnail, Türkçe isim, **fiyat**, **denge kütlesi**, kısa açıklama, “SATIN AL” düğmesi. Para yetersizse pasif + gerekçe.
- Satın alma **tek** parasal işlem ve **tek** item spawn eder; **çift tıklama** aynı item'i iki kez almamalı. Spawn için güvenli boş X aranır; bulunmazsa ödeme alınmaz. `itemId` tekil instance kimliğiyle ayrılır.
- Mağaza açıkken dünya sürükleme devre dışı; açıkça kapatma butonu ve ESC/back.

### 9.2 Sabitleme

- Sadece `Preparation` fazında (tutorial geçişleri dahil) çalışır; afette kilitli.
- Varsayılan bedel **30 para**; Level1'de kitaplık bunu öğretir. Fiyata `LevelConfig` hakim olmalı; mevcut kodda `ToggleAnchor` davranışı korunup servisleştirilmeli.
- Sabitlenmiş eşyada çapa ikonu/vida animasyonu; istersen duvar bağlantısı görseli (`LineRenderer`/ikili köşe kelepçesi) fakat gereksiz büyük sabitleme aparatı kullanma.
- Sabitleme sonrası oyun kütlesi **değişmez**; fizik objesi evle birlikte hareket eder ama kaymaz. Afet sırasında otomatik çözülmez.
- Hazırlıkta sökme ücretsiz olabilir, ama **ödediği bedeli iade etmeme** başlangıç kararı sonsuz al-sat istismarını önler. İade gerekiyorsa oyun tasarım onayıyla değiştir.

### 9.3 Hazırlık sayacı

- Güncel 2D entegrasyon: **45 saniye**. Rehber kutuları açıkken süreyi durdurmak / ilk tutorial adımlarında zaman saymamak, yeni oyuncunun sırf okumaktan kaybetmesini önler.
- Süre sıfıra gelince zorla afet başlatma yerine `hazırlık bitti` uyarısı ve kullanıcı onayı (varsayılan MVP politika) veya açıkça tanımlı otomatik başlama. İkisi rastgele karıştırılmamalı. Seçim `LevelConfig` bool alanında.
- Eğim `>18°` ise `AFETE HAZIRIM` düğmesinde **risk uyarısı** göster, oyuncuyu körlemesine cezalandırma; onayla devam edebilsin.

---

## 10. DEPREM — LEVEL 1 AFET VE RAPOR

### 10.1 Aşamalar (tek durum otoritesi)

```text
Menu -> Preparation -> Countdown (Preparation alt durumu) -> Disaster
            |                                                   |
            +-- Pause/Shop/Tutorial                               +-- GameOver
                                                                |
                                                                v
                                                           Results/Report
                                                       / Restart / Menu / Level2
```

`GameManager.Phase` ana fazın **tek kaynağı**. `L1_ProjectGameController.RoundState` özel alt durumlarını `L1_LevelOrchestrator`a taşırken **iki ayrı bağımsız state machine** oluşturarak senkronizasyon hatası bırakma. `GameManager.SetPaused` ile `L1_ProjectGameController.SyncPause()` çakışması önlenmeli. `Time.timeScale` sonuçtan çıkarken daima `1`.

### 10.2 Deprem modeli

- `AFETE HAZIRIM` ile 3s geri sayım, `DEPREM` banner ve siren/gerilim sesi.
- Level1 `EarthquakeIntensity` ayarlanabilir; ilk tur **orta şiddet**. `QuakeDuration=10s` mevcut entegre prototiple başlangıç; istenirse 25s daha sinematik ayarlanabilir.
- Değişen işaretli yatay ivme/kuvvet: örnek `a(t)=A1 sin(2πf1t)+A2 sin(2πf2t+phase)+bounded noise`; `AnimationCurve` ile güçlü/güçsüz sekanslar, sabit tek frekanslı sonsuz titreşim değil.
- Mobilyalara `Rigidbody.AddForce(force, ForceMode.Acceleration)` (yalnız XY), fizik sürtünmesi ve bina eğimi devrilme/yer kayması yaratır. Ev torkuna ayrıca afet momenti uygulanır.
- Sabitlenmiş eşyalar deprem kuvvetinden etkilenmez ve yerinde kalır; serbestler kayabilir, devrilebilir, düşebilir. Deprem **yalnız kamerayı sallayan sahte efekt** olamaz.
- Çıkış/oyun güvenliği: gerçek yaşam eğitim mesajı ekranın altında, kısa ve okunur: **“Gerçek depremde eşya taşımayın. Çök–Kapan–Tutun.”** “Deprem sırasında evden kaç/korkuluğa koş” gibi yanlış teşvikler yok.
- Hafif `Juice.Shake` sadece camera visual offset; evin torque rotasyonuna sahte kırılma ekleme. Toz partikülleri az ve amaçlı; performansı gözet.
- Deprem sona erince tüm geçici kuvvet uygulanması durur, sakinleşme / net sonuç hesaplaması yapılır, UI rapor açılır.

### 10.3 Eşya sonuçlarının ölçülmesi

Her `FurnitureInstance` için `id`, `catalogId`, `balanceMassKg`, `value`, `isAnchored`, `isBroken`, `isLost`, `startPosition`, `endPosition`, `distanceTravelled` gibi alanlar. `Lost` bir kez tetiklenir (evin altına düşme ve sınır sensörü); renderer gizlense bile kaybın para/denge hesabında kaydı tutulur. **Aynı eşya hem `broken` hem `lost` olabilirse rapor sınıfları çifte sayılmayacak şekilde tanımlanır.**

### 10.4 Puanlama (başlangıç taslağı — Inspector'dan dengelenebilir)

```text
baseSafety = houseSurvived ? 40 : 0
anchorBonus = 10 * correctlyAnchoredHeavyCount (max 30)
protectedBonus = round(20 * protectedValue / max(1,totalValue))
balanceBonus = max(0, 10 - round(maxAbsTilt / 3))
penalty = 10 * lostHeavyCount + 5 * brokenCount
safetyScore = clamp(baseSafety + anchorBonus + protectedBonus + balanceBonus - penalty, 0, 100)
```

Bu **yeni önerilen** puan formülüdür, mevcut repoda birebir olduğunu iddia etmiyoruz. Başarılı bir turda ayrıca `+450` para **sonraki seviye girişinde bir kez**; Level1 raportajındaki “kazanılan para” katmandan çekilmeli. `ResultCalculated` yeniden çağrılsa bile bir tur/instance için ödül tekrar verilmez.

**Rapor mesajları davranışa bağlı:** kitaplık gerçekten sabitlendiyse sabitleme başarısı; sabitlenmemiş ağır eşya gerçekten devrildiyse risk; tek katta ağır eşya üst kata kondu mesajı Level1'de **gösterilmez**. Gerçek evlerin tork/eşya dağılımı ile yapısal deprem dayanımı arasındaki kurgusal fark dipnotla açıklanır.

---

## 11. KARAKTER, MERDİVEN, KAT EKLEME VE GELECEK MODÜLLER

- **Level1 tek kat**: mevcut `L1_ResidentController` ve A/D yürüme kontrolü korunabilir fakat mouse drag için yürümek **zorunlu olmayacak**. `W/S` merdiven kontrolü Level1'de gizli/pasif.
- **Level2**: yeni mutfak modülü yukarıdan düşer; oyuncu modülü **yana kaydırarak** alt katla hizalar; snap ve temas doğrulamasından sonra kat bağlanır, sonra fırtına mekaniği. Bu çalışma **Level2 branch'inin işi**; sadece modüler `HouseStack` API tasarımı için not.
- **Level3**: yeni yatak odası bloğu, yangın sistemi; **Level3 branch'ine dokunma**.
- Gelecek katların hepsi **düz**, üçgen çatı yok; ahşap merdivenle katlar arası dolaşım. Evin gövdesi yeni katla aynı pivot etrafında dönmeli; yığılma ve kütle merkezi yeniden hesaplanmalı.
- Yan kapı ve ahşap seyir platformu Level1'de **görsel/gezilebilir unsur** olabilir, ama depremde “kaçıp çıkma” ödülü yok. Safety mesajı korunur.
- Gelecekte kullanılabilecek `ILevelHouseModule` / `FloorAttachmentSlot` public arayüzlerini `Contracts.cs` içine şimdilik **ekleme**; gerekirse Lead onayı al ve adapter tasarla.

---

## 12. ASSET İMPORT / UNITY EDITOR İŞLEM SIRASI

1. **Yedek:** kendi branch'te temiz commit; istenirse `git tag l1-before-layout-overhaul` (tag ekip kurallarıyla uyumlu mu teyit et). `main`e yazma.
2. `git lfs pull`; Library import tamamlanana kadar bekle; Console hatalarını kayıt altına al.
3. `Assets/Scenes/Level1.unity` **kopya** sahnesinde veya ayrı test alanında mevcut builder/prefablarını incele. Editörde *Scene Hierarchy* ve Game View ekran görüntüsü al.
4. Asset kullanım envanteri hazırla: `foundPath`, `type`, `meshTris`, `materialShader`, `renderPipeline`, `dimensions`, `hasCollider`, `license`, `useCase`.
5. Mevcut prefab/model yoksa CC0 yedek paketleri **resmi sayfadan** indir; ZIP içinde yalnız ihtiyacın olan FBX + texture + lisans dosyasını Unity'ye al. Proje shader ayarlarını değiştirme.
6. FBX import: `Scale Factor` + **1m** gerçek ölçü, `Read/Write` kapalı (gerekmedikçe), import material tutarlı; collider'ı sade box veya uygun compound ile yaz. 3D FBX modeli **Prefab Variant** olarak yerleştir.
7. `URP/Lit` ile tek palet, en fazla 2048px texture, furniture önerisi `≤5000 tri`; eksik normals/tangentleri kontrol et.
8. Kaya 3D mesh ile ya da kaliteli sprite + alt gölge ile derinlik hissi versin. Evin altındaki **tek nokta pivot** sahnede anlaşılır.
9. Düz `RoomShell` oluştur; duvarları tek büyük altıgen kutu gibi değil, okunabilir **ahşap kolon/zemin** görseliyle üret. Ön cephe kapalı olmayacak.
10. Dekorların collider'ları yalnız gerekliyse açık; şişe/saat/saksı gibi 15 küçük dekor sahneyi fiziksel olarak boğmamalı.
11. Menu ve HUD uGUI prefablarını oluştur; kompozisyon 16:9 testinde metin kesilmeyecek.
12. `LevelConfig` üzerinden başlangıç bütçe/afetin süresi/tek kat/başlangıç eşyalarını bağla; hardcoded inspector dışı değer kalmasını azalt.
13. Zoom/pan/selection, tutarlı tek EventSystem ve tek AudioListener kontrolü.
14. Fizik + deprem + rapor yalıtık testleri; sonrasında Menu → Level1 tam sahne testleri.
15. Atıf dosyasına paket + sürüm + URL + lisans + projede kullanılan file path ekle. **Dış kaynaklı seslerin özel lisansını** ayrıca kontrol et.
16. Windows build oluştur; Editor dışında çalıştır; grafik ve input regresyonu yap. Test edilmediyse “tested” yazma.

**Temel kalite çıtası:** tek seferde bütün vendor paketlerini import edip yüzlerce MB gereksiz prefab şişirmesi yaratmak yasak. 5–10 iyi seçilmiş modelin görsel tutarlılığı, 200 uyumsuz modelden değerlidir.

---

## 13. UNITY MCP İÇİN UYGULAMA FAZLARI (HER FAZ SONUNDA KONTROL)

### Faz A — *Audit & Savepoint* [P0]

- Branch ve HEAD doğrulaması; Unity MCP bağlantısı; sürüm uyumu (`manifest` v10.3 vs eski belge v10.0); LFS varlığı.
- `Menu`, `Level1`, `CoreTest` sahnelerini açıp hiyerarşiyi kayda al; Scene/Game View screenshot; kırmızı console/missing refs.
- Bir sayfalık **current-vs-target** analizi üret. `L1_ProjectGameController`/`RoundState`/`GameManager` çakışması, UI OnGUI ile uGUI, mevcut store/depremin başka level'i tetiklemesi.
- **Test:** Tüm varlıklar Editor'de açılıyor; `git status` beklenmedik değişiklik yok; sahne yedeği var.

### Faz B — *Level1 Single-Floor 3D Environment* [P0]

- Sahnede **tek geniş oturma odası**; düz çatı/tavan; cam pencere, giriş/çıkış yan kapı, ahşap platform, zemin; sivri kayalık; dağ/orman/sis bulut.
- Eski `L1_CozyPlayableBuilder.Build()` komutu aktif görsel kökü silebildiğinden mevcut üreticiye tek-kat modu ekleyerek veya sahne-özel prefab variant üreterek güvenli geçiş yap.
- **Test:** 16:9'daki ev gerçek 3D; oda boş hâlde de iyi görünür; mobilyalarla 3D kesit okunur; çatı düz; ev yalnız 1 katlı; pivot kaya üzerinde.

### Faz C — *Drag / Balance / Zoom* [P0]

- `L1_PropDrag` mevcut fare/touch raycast korunarak **horizontal-only** hedef hesaplamasına uyarlanır; yakınlık zorunluluğu fare kontrolünden kaldırılır; anchored/invalid/gravity tutarlı.
- Tork/kütle metodu izole edilir; `CoreTest` sahnesinde 4 furniture fixture; gerçek HousePivot dönüşü; tilt gauge.
- Camera zoom/pan/fit (fare tekeri, orta fare, düğme) + drag ile olay çakışması yok.
- **Test:** 85 kg kitaplık uçta eğimi artırır; zoom yapıp aynı nesne bırakılır; zemin/duvar penetration yok; reset çalışır.

### Faz D — *Menu + New HUD + Shop* [P0]

- Menu.uGUI; Level1 Canvas mini gauge; market; tutorial adımları; Settings + HowTo + pause + sceneLoader.
- Eski `OnGUI()` ekranları aynı anda görünmemeli; bir debug toggle dışında üretim ekranında devre dışı.
- **Test:** `Menu → Oyna → Level1`, scroll/ESC/quit; mağaza parayı tek sefer eksiltir; HUD gerçek kütle/angle gösterir.

### Faz E — *Earthquake + Reporting* [P0]

- Hazırlık → geri sayım → deprem → results; sabitli/serbest eşya farklı hareket; tehlike; toz/ses/shake; kayıplar ve ekonomi.
- `SceneLoader` ile restart/main/Level2 koşullu geçiş; sıfır key/double reward.
- **Test:** 3 ayrı deprem denemesi; en az birinde sabitleme eşyayı koruyor, birinde kayıp doğru tork düşürüyor, birinde başarısızlık raporu geliyor.

### Faz F — *Polish / Build / Documentation* [P1]

- Yerleştirme outline/ghost, kısa tutorial, input glyph, sfx, menu backdrop, density/LOD/lighting, imleç/efekt optimizasyonu.
- 1920×1080 / 1366×768 / 1280×720 test, tam ekran ve pencere modu, pause/alt-tab, ikinci cihazda Windows build.
- Kaynak/lisans atıfları ve rapor. **40. saatten sonra** yeni özellik değil hata düzeltme (jam iş planı uygulanıyorsa).

---

## 14. OLASI HATALAR / TEKNİK BORÇ (AÇIKÇA TAKİP EDİLİR)

| Risk | Nasıl anlaşılır? | Önlem |
|---|---|---|
| **Çifte GameState** | HUD Preparation, Controller Disaster; pause takılıyor | `GameManager` tek kaynak, `RoundState` adapter. |
| **Menü iki kere açılıyor** | Scene Menu buton + Level1 OnGUI start overlay | Level1'in internal StartMenu'sunu üretim modu için devre dışı bırak. |
| **Yanlış collider** | Sürüklerken eşya duvar üstüne çıkıyor / kaya seçiliyor | Furniture layer mask, BoxCast/OverlapBox, local X. |
| **Ev eğilirken dynamic rigidbody kopuyor** | Zemin cisimden geçiyor, rigidbody ışınlanıyor | Kinematic house collider + FixedUpdate + non-parented furniture. |
| **Yatay sürükleme yerine dikey kaldırma** | `maxLift` davranışı sürüyor | `L1_PropDrag.Update` hedef Y'yi sabit tut. |
| **Kamerada zoom yok** | Mouse scroll yalnız UI'de işliyor | InputAction/Mouse.scroll kamera scripti + clamp. |
| **Kamera mouse ile çakışıyor** | World select UI üstünden tetikleniyor | EventSystem check, pointer capture, action order. |
| **LFS asset yok** | 131-byte PNG veya missing sprite | `git lfs pull`, Unity reimport; log alın. |
| **Ölçek tutmuyor** | FBX çok büyük/küçük | Prefab import scale, 1m kuralı. |
| **Sahne builder silme** | Yeniden kurunca el ile yerleştirilen şeyler kayboluyor | Önce scene backup/commit, builder idempotence, prefab ownership. |
| **Mevcut afetler Level1 içinde ilerliyor** | Level1 Deprem sonrası Fırtına'ya geçiyor | Level1 sonuç sonrası `SceneLoader.Level2`; mevcut 5 aşama kodunu kaybetme, flag ile izole et. |
| **Lisansa aykırı kullanım** | Asset kaynak dosyaları kamuya açık ayrıca servis ediliyor | License manifest, izin kontrolü; Asset Store EULA/CC0 ayrımı. |
| **Skor iki defa** | Panel iki kez açılınca +450 iki kez | Round GUID/idempotent result flag. |
| **Gerçek deprem tavsiyesi yanlış** | Oyuncuya deprem sırasında dolap taşı deniyor | Fantastik güç ayrımı, hazırlıkta sabitleme vurgusu, Çök–Kapan–Tutun. |

**Ek kod inceleme noktası:** `Bootstrap` kalıcı `[Systems]` nesnesi için `DontDestroyOnLoad` kullanıyor; repo kuralları “bölüm nesneleri `DontDestroyOnLoad` kullanmasın” diyor. Bu **kasıtlı çekirdek istisnası** olarak görülmeli; Bootstrap'u silme ya da sahnelere kopya manager ekleme. `SceneLoader` varsayılan fade overlay üretiyor; ikinci bağımsız full-screen fade yapmak çift kararma doğurabilir.

---

## 15. ACCEPTANCE TEST MATRIX — OTOMATİK / ELLE TEST

| ID | Test | Başarı koşulu | Test yeri |
|---|---|---|---|
| T01 | `Menu` Play | Başlık ve 4 düğme Türkçe, arka plan yükleniyor | Menu.unity |
| T02 | `Oyna` | Fade sonrası Level1 ve Preparation | Menu → Level1 |
| T03 | `Nasıl oynanır` / `Ayarlar` | Popup açılır, ESC ile kapanır, sesler kaydedilir | Menu.unity |
| T04 | Tek oda | Tek geniş oda, **düz tavan**, görünür kaya + pivot | Level1 |
| T05 | Yatay drag | Y dışında kütle hareketi yok, kat değişmiyor | Level1 |
| T06 | Sürükleme sınırı | Duvara/başka eşyaya giremez; bırakınca zeminde | Level1 |
| T07 | Kütle etkisi | Ağır nesne aynı X'te daha çok tork | CoreTest |
| T08 | Eğilme | Pozitif sağ yük → sağ aşağı, mini gauge aynı | CoreTest |
| T09 | Anchoring | Sabitli eşya depremde sabit, kütle korunur | Level1 |
| T10 | Budget | Satın alma, 30 altın sabitleme, yetmeyince ret | Level1 |
| T11 | Zoom | Wheel +/- / Fit, clamp, drag zoom sırasında bozulmaz | Level1 |
| T12 | Pan | Orta fare ile limitli, UI üzerine pan yok | Level1 |
| T13 | Prepare → Disaster | 3s countdown, deprem başlar, pause doğru | Level1 |
| T14 | Earthquake physics | Sabitsiz eşya kayar/devrilir; sabitli yerinde | Level1 |
| T15 | Lost furniture | Kayıp eşyayı bir kez sayar, torktan çıkar | Level1 |
| T16 | Critical collapse | 26°/hold süresi sonrası GameOver | CoreTest |
| T17 | Report | Gerçek davranış + tek ödül + doğru mesaj | Level1 |
| T18 | Restart | Budget/furniture/score başlangıca döner | Level1 |
| T19 | Return to Menu | `SceneLoader` ile ve timeScale=1 | Level1 → Menu |
| T20 | Scene sanity | Tek Camera, Light, AudioListener, EventSystem | Menu/Level1 |
| T21 | Console | Derleme/runtime kırmızı hata **yok** | Unity Console |
| T22 | Missing assets | Tüm prefab materyaller dolu, LFS gerçek dosya | Unity Project Browser |
| T23 | Performance | Win 1080p ~60FPS hedef (cihaza göre ölç) | Windows build |
| T24 | Gamepad/touch (opsiyon) | Eklendiyse UI girişleri çakışmıyor | Input Test |
| T25 | Security message | Gerçek ve fantastik mekanik ayrılmış | Level1 report/HUD |

**Zorunlu saha testi:** En az 2 gerçek kullanıcıya oyunu hiç anlatmadan oynat; ilk mobilyayı sürüklemeyi, sabitlemeyi ve afet başlatmayı bulabilmeli. Test video/notlarında UI tıkanmaları işaretlenmeli.

---

## 16. MCP AJANININ HER FAZ SONUNDA RAPOR ŞABLONU

```markdown
### Faz [A/B/C/D/E/F] sonucu
- Branch ve HEAD:
- Unity Editor sürümü ve MCP bağlantı durumu:
- Değiştirilen dosyalar (tam path):
- Yeni oluşturulan prefab/scene/ScriptableObject:
- Varlık lisansları / yeni indirilen paketler:
- Test edilenler (Txx):
- Unity Console: hata / uyarı sayısı:
- Editor Game View ekran görüntüsü yolu (varsa):
- Commit SHA / push sonucu (yapıldıysa):
- Açık kusurlar ve bir sonraki en yüksek öncelik:
```

Test etmediğin hiçbir adımı ✅ işaretleme; console temiz değilse teslim tam değil. Sahne `Menu`den ve bağımsız `Level1`den çalışmalı. Kod inceleme kaynaklarının **GitHub üzerinden** okunduğu ve Unity Editor Play'in bu doküman hazırlanırken çalıştırılmadığı özellikle ayrılmalıdır.

---

## 17. CLAUDE CODE + UNITY MCP'YE VERİLECEK İLK KOPYALA-YAPIŞTIR PROMPT

> Aşağıdaki bloğu Claude Code'a ver. Bu Markdown dosyası **proje köküne veya `docs/` içine de ekli olsun** ki ajan okuyabilsin.

```text
We are implementing SON DENGE using the EXISTING Unity project and Unity MCP. Do not start a new project.
Target repository: yenererr/SonDenge, branch kisi1-level1.
Read SON_DENGE_UNITY_MCP_LEVEL1_MENU_MASTER_PLAN.md in full before changing anything.
Also read docs/ZORUNLU_KURALLAR.md, CLAUDE.md, docs/OYUN_TASARIMI.md,
docs/TEAM_PLAN.md, docs/API_CONTRACTS.md, docs/tasks/KISI1_LEVEL1.md,
and Assets/Levels/Level1/PROJECT_FILES_INTEGRATION.md.

Critical latest design override, requiring Lead-approved design documentation update:
- Level 1 begins with EXACTLY ONE wide living-room floor on a sharp mountain rock pivot.
- Flat modular upper slab; NO triangular or pitched roof in Level 1.
- 3D stylized low-poly assets with 2.5D XY gameplay and 3D physics; no Rigidbody2D.
- Horizontal-only furniture mouse dragging, real torque/tilt, anchored furniture, 750 initial budget,
  shop, tutorial, preparation -> earthquake -> result, and a compact horizontal balance gauge.
- Camera stays fixed SIDE VIEW with mouse-wheel zoom, bounded middle-mouse pan, zoom +/- and fit controls.
- Mountain / trees / layered clouds / fog backdrop, side exit door and wooden platform.
- Menu scene: PLAY / HOW TO PLAY / SETTINGS / QUIT. All visible text Turkish.
- Level 2 will add a kitchen module dropping from above and a storm; Level 3 a bedroom module
  and fire. DO NOT edit Level2/Level3/Level4 branches or scenes.
- Preserve functional code and already imported asset packs. Do NOT wipe existing Level1 builders/objects
  blindly: L1_CozyPlayableBuilder may destructively rebuild the visual root.
- The existing Level1 controller currently bundles five disaster phases, so isolate Level1 earthquake
  without deleting future logic. Avoid double state machines and double menus.
- Use only legitimately licensed FREE/CC0 assets, first reuse asset inventory in repository.
- Verify Git LFS actual binary downloads before using image/audio prefabs.
- Packages/manifest pins unity-mcp v10.3.0; old setup docs mention v10.0.0. Verify actual connection.

FIRST RESPONSE: inspect git branch/status, Unity Editor version and MCP connection, list existing Menu/Level1
hierarchies, list files to change, and give a minimal execution plan aligned with Phase A-F.
Do not write code or touch scenes before this audit.
Then implement Phase A, test, save, report, commit/push if authorized; continue phase by phase.
After each phase, report exact test outcomes, console errors, changes, and commit SHA.
Never claim Unity tests were run unless Unity was actually opened and the tests executed.
```

---

## 18. KAYNAKLAR, DOĞRULAMA VE ATIF İZİ

### Repo incelenen referanslar (private; bağlı GitHub hesabıyla erişilir)

- https://github.com/yenererr/SonDenge/tree/kisi1-level1
- https://github.com/yenererr/SonDenge/blob/kisi1-level1/docs/OYUN_TASARIMI.md
- https://github.com/yenererr/SonDenge/blob/kisi1-level1/docs/ZORUNLU_KURALLAR.md
- https://github.com/yenererr/SonDenge/blob/kisi1-level1/docs/tasks/KISI1_LEVEL1.md
- https://github.com/yenererr/SonDenge/blob/kisi1-level1/Assets/Levels/Level1/PROJECT_FILES_INTEGRATION.md
- https://github.com/yenererr/SonDenge/blob/kisi1-level1/Assets/Levels/Level1/L1_PropDrag.cs
- https://github.com/yenererr/SonDenge/blob/kisi1-level1/Assets/Levels/Level1/L1_ProjectGameController.cs
- https://github.com/yenererr/SonDenge/blob/kisi1-level1/Assets/Levels/Level1/L1_ProjectGameController.UI.cs
- https://github.com/yenererr/SonDenge/blob/kisi1-level1/Packages/manifest.json

### Resmî asset/teknik lisans kaynakları

- Kenney Furniture Kit — https://kenney.nl/assets/furniture-kit (CC0, 140 varlık)
- Kenney UI Pack — https://kenney.nl/assets/ui-pack (CC0, 430 varlık)
- Kenney Input Prompts — https://kenney.nl/assets/input-prompts (CC0, 1500 glyph)
- Kenney Interface Sounds — https://kenney.nl/assets/interface-sounds (CC0)
- Kenney Impact Sounds — https://kenney.nl/assets/impact-sounds (CC0)
- Kenney Nature Kit — https://kenney.nl/assets/nature-kit (CC0, 330 varlık)
- Kenney UI Pack Adventure — https://kenney.nl/assets/ui-pack-adventure (CC0)
- Kenney Game Icons — https://kenney.nl/assets/game-icons (CC0)
- Kenney Particle Pack — https://kenney.nl/assets/particle-pack (CC0)
- Kenney RPG Audio — https://kenney.nl/assets/rpg-audio (CC0)
- Kenney Music Jingles — https://kenney.nl/assets/music-jingles (CC0)
- Quaternius Ultimate House Interior Pack — https://quaternius.com/packs/ultimatehomeinterior.html (CC0, 123 model)
- Quaternius Furniture Pack — https://quaternius.com/packs/furniture.html (CC0, 23 model)
- Quaternius Ultimate Stylized Nature Pack — https://quaternius.com/packs/ultimatestylizednature.html (CC0, 63 model)
- Quaternius Ultimate Nature Pack — https://quaternius.com/packs/ultimatenature.html (CC0, 150 model)
- Unity Asset Store EULA — https://unity.com/legal/as-terms
- CoplayDev Unity MCP — https://github.com/CoplayDev/unity-mcp
- Unity `Camera.ScreenPointToRay` — https://docs.unity3d.com/ScriptReference/Camera.ScreenPointToRay.html
- Unity `Physics.Raycast` — https://docs.unity3d.com/ScriptReference/Physics.Raycast.html

**Doğrulama notu:** Kenney/Quaternius paketlerindeki CC0, resmî paket sayfalarında görüldü. Bazı paketlerin tekil mesh dosyaları, asset ZIP'i ve import testi yerel Unity Editor'de ayrıca görülmelidir. DOLLHOUSE paketinin README'deki Asset Store bağlantısı bu araştırmada erişim hatası verdi; **kaynak paketin varlığı repoda görüldü, mağaza sayfası bağımsız doğrulanamadı**. Üçüncü taraf paketlerin Unity Asset Store Standard EULA kapsamı ayrıca kaynakla teyit edilmeli. Cited sources updated/visited 17 Ekim 2026.

---

## 19. SON TESLİM TANIMI

**Level1 ve Menü “bitti” denebilmesi için:** Menüden girilen, bir katta dört gerçek 3D mobilya içeren, gözle görülür kütle-tork dengesi olan, yatay sürüklemenin ve zoom/pan'ın çalıştığı, kitaplığın sabitlenip gerçek depremde korunabildiği, en az bir eşyanın kayıp olarak sayılabildiği, Türkçe HUD+mağaza+eğitim+afet raporu sunan, sahne tekrar başlatılabilen ve Windows build'de oynanabilen oyun gerekir. Tüm bunları **oyun sırasında** doğrulamadan “tamamlandı” demeyin.

**Esas ilke:** Çalışan ve görsel olarak güçlü **bir bölüm**, yüzlerce varlıkla dolu ama oynanamayan bir prototipten değerlidir.
