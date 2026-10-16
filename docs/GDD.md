# SON DENGE — GAME JAM MASTER PLAN

> **Not:** Bu ilk plandır. Oyun artık **3D** (sabit yandan kamera) ve görev dağılımı değişti. Güncel kaynaklar: `OYUN_TASARIMI.md`, `TEAM_PLAN.md`, `ZORUNLU_KURALLAR.md`, `tasks/`.

**Tema:** Afete Hazırlık ve Farkındalık  
**Tür:** 2D physics / strategy / disaster preparedness  
**Motor:** Unity 2D (ekibin birlikte sabitleyeceği tek Unity Editor sürümü)  
**Geliştirme:** 4 kişi, her kişiye ayrı Claude Code oturumu; isteğe bağlı Unity MCP  
**Versiyon kontrolü:** GitHub + Git LFS  
**Hedef:** 48 saatte oynanabilir, Windows build'i alınmış tek bir dikey dilim (vertical slice).

## 1. Oyun adı ve tek cümlelik sunum

**Son Denge (Last Balance)**: Sivri bir kaya üzerindeki fantastik evin koruyucususun; her afet öncesinde eşyaları doğru yerleştir, güvenlik önlemleri al, afet sırasında evin dengesini yönet ve sakinlerini koru.

**Ana mesaj:** “Denge, afet başladığında değil; afet gelmeden önce kurulur.”

## 2. Hikâye: Sivritepe'nin Son Evi

Sivritepe, sarp dağların arasında kurulmuş küçük bir kasaba. Kasabanın en uç noktasındaki ince kaya sütununun tepesinde nesillerdir ayakta duran tuhaf bir ev var. Bu evin sırrı: içerisindeki her eşyanın konumu, bütün binanın dengesini etkiler. Bir dolabın yerini değiştirmek bile evin uçuruma doğru eğilmesine neden olabilir.

Bir gün kasabada art arda afetler yaşanmaya başlar. Önce deprem, sonra yangın, ardından fırtına. Kasaba sakinleri yardım ararken gözlerini bu eve çevirir. Oyuncu, evin görünmez koruyucu ruhu **Denge**'dir. Denge, hareketli eşyaların konumunu fantastik gücüyle değiştirebilir, ancak önceden duvara sabitlenen bir eşyayı afet sırasında anında oynatamaz.

Afetten önce bütçesiyle güvenlik önlemleri alır: kitaplıkları sabitler, ağır nesneleri alt katlara yerleştirir, duman alarmı veya sprinkler kurar. Afet geldiğinde yaptığı hazırlığın sonuçlarını görür. Korunan eşyalar bir sonraki tur için kaynak sağlar; insanlar ise güvenli davranışlar ve doğru hazırlıklar sayesinde korunur.

Her tur sonunda evin yeni odaları veya katları açılır. Gelişen evin dengesi daha karmaşıklaşırken Denge önemli bir gerçeği öğrenir: Afet anındaki olağanüstü güç bile afet öncesi yapılan bilinçli hazırlığın yerini tutmaz.

### Üç perde
1. **İlk Sarsıntı:** Oyuncu iki katlı evde yerleştirme, sabitleme ve deprem mekaniğini öğrenir.
2. **Artan Tehlike:** Yeni eşyalar ve isteğe bağlı yangın/fırtına sistemleri riski çeşitlendirir.
3. **Büyük Sınav:** Önceki hazırlıkların gerçek sonucunu gösteren final afet turu.

### Açılış metni
- “Burası Sivritepe. Burada her şey biraz dengesiz.”
- “Şu ev mi? Evet, gerçekten o kayanın üzerinde duruyor.”
- “İçindeki her eşyanın bir ağırlığı; her kararın bir sonucu var.”
- “Afetler kapıyı çalmadan gelir. Sen hazır mısın?”

### Oyun sonu metinleri
**Başarı:** “Evi ayakta tutan tek şey ağırlık değildi. Doğru zamanda aldığın önlemlerdi.”
**Başarısızlık:** “Her afet bir ders bırakır. Bir dahaki sefere hazırlığa daha erken başla.”

## 3. Oyun döngüsü

1. **Hazırlık (60–90 sn):** Afet uyarısı (örn. deprem), başlangıç bütçesi, mobilya konumlandırma, sabitleme ve risk göstergesi.
2. **Afet (30–60 sn):** Deprem titreşimi, hareketli eşyalarda kayma ve düşme, evin tork ve eğim tepkisi; oyuncu Denge'nin fantastik yeteneğiyle mobil eşyaları sınırlı biçimde kontrol eder. Sakinler güvenli pozisyon alır.
3. **Değerlendirme (15–30 sn):** Güvenlik, yapı dengesi, eşya kaybı, kaynak kazanımı, bir gerçek yaşam farkındalık mesajı.
4. **Gelişim:** Daha zor afet, yeni eşya/oda; yalnızca ilk döngü istikrarlıysa kat ekleme.

### Kazanma/kaybetme
- **Başarı:** Afet süresi tamamlandığında ev kritik eğim eşiğini aşmamış ve sakinlerin güvenliği sağlanmış.
- **Kaybetme:** Ev kritik açıya uzun süre maruz kalır veya güvenlik göstergesi sıfırlanır.
- **Öncelik:** İnsan güvenliği > yapı bütünlüğü > eşyaların maddi değeri.

### Denge sistemi (oynanış modeli)
- Her mobil mobilyanın `mass`, `localX`, `isAnchored`, `value`, `fragility` alanları var.
- Eşyaların yatay momenti: `T_furniture = Σ(m_i * 9.81 * localX_i)`.
- Ev gövdesinin kütlesi, düşey/yatay konumu ve eğim açısı ayrıca hesaplanır; bu formül tek başına tam tork modeli değildir.
- `T_total = T_furniture + T_house + T_disaster - damping * angularVelocity` yaklaşımıyla kontrollü açısal dinamik.
- `tiltAngle` kritik eşik üstünde örn. 2 sn kalırsa kayıp (eşikler playtest ile ayarlanır).
- İstek: ilk prototipte stabil, okunaklı, eğlenceli model; gerçekçi fizik simülasyonu zorunlu değil.

### Gerçek dünya güvenliği
- Deprem anında eşya taşımak gerçek yaşam tavsiyesi değildir: oyunun doğaüstü gücü ile insan davranışı net ayrılmalı.
- Gerçek hayatta afet öncesi mobilya sabitleme ve ağır eşyaları güvenli yerlere yerleştirme öne çıkarılmalı.
- Deprem sırasında güvenli durumda Çök–Kapan–Tutun vurgulanmalı; tehlike geçmeden tahliye eylemi bir ödül mekaniği olmamalı.
- Duman dedektörü uyarı verir; otomatik söndürme için ayrı sprinkler gerekir.
- Eğitim metinleri yayımlanmadan önce AFAD ve ilgili resmî kaynaklarla doğrulanmalı.

## 4. 48 saatlik MVP kapsamı

### P0 — mutlaka oynanabilir
- Tek ana sahnede 2 katlı açık kesit ev, sivri kaya ve basit arka plan.
- 4 mobilya: dolap, koltuk, masa, kitaplık; farklı kütle ve ekonomik değer.
- Mobilya seç ve taşı; alana yerleştir; sabitle/sök (hazırlıkta).
- Evin sağ/sola eğilmesi ve HUD denge çubuğu.
- Tek deprem senaryosu; sabitlenmemiş eşya kayması/kaybı.
- Hazırlık → deprem → sonuç → tekrar oyna.
- En az 2 sonuç mesajı ve 2 hazırlık farkındalık kartı.
- Windows oynanabilir build ve README.

### P1 — P0 biterse
- 2. tur, daha şiddetli deprem, sonraki tur bütçesi.
- Yeni mobilya satın alma ve tek oda/kat açılması.
- Basit yangın tehdidi, yangın alarmı / sprinkler ayrımı.

### P2 — ancak vakit kalırsa
- Fırtına, tam karakter animasyonları, kapsamlı hikâye ara sahneleri, WebGL, kayıt sistemi.

### Kesilecek özellikler
Multiplayer, procedural level generation, online leaderboard, kompleks eşya envanteri, gerçekçi bina yıkımı ve her katı ayrı fizik motoruyla simüle etmek.

## 5. Dört kişiye AI-dostu iş bölümü

**Önemli:** Her kişi kendi Claude Code oturumunu ve tercihen kendi Unity çalışma kopyasını kullanır. Birden fazla insan/ajan aynı anda aynı Unity Editor oturumuna MCP ile değişiklik yapmaz. Herkes ayrı branch veya tercihen ayrı git worktree üzerinde çalışır.

| Sorumlu | Kod alanı | Sorumluluklar | Bağımsız teslim | Branch |
|---|---|---|---|---|
| **Kişi 1 — Lead & Integration** | `Scripts/Core/`, `Scenes/Main.unity`, `Scripts/Player/` | Game state, sahne bağlantıları, oyuncu etkileşimi, integration, build | Hazırlık→Afet→Sonuç akışı çalışan proje | `feature/core-loop` |
| **Kişi 2 — Physics** | `Scripts/Physics/`, `Scripts/Disasters/`, `Scenes/PhysicsSandbox.unity` | Ev dengesi, tork, eğim, deprem, kayma / sabitleme davranışı | Sahne bağımsız HouseBalanceController ve EarthquakeController | `feature/physics` |
| **Kişi 3 — Art & UI** | `Art/`, `Prefabs/Visual/`, `Scripts/UI/`, `Scenes/UISandbox.unity` | 2D görseller, prefab sunumu, HUD, uyarı/sonuç paneli | Görseller + basit API ile kontrol edilebilen HUD prefab'ı | `feature/art-ui` |
| **Kişi 4 — Preparation & Awareness** | `Scripts/Preparation/`, `Scripts/Economy/`, `Scripts/Awareness/`, `Data/`, `Scenes/PreparationSandbox.unity` | Sabitleme satın alımı, tur bütçesi, eşya data, farkındalık ve skor | PreparationManager, Wallet, ScoreCalculator, data asset tanımları | `feature/preparation` |

**İnce ayrım:** `FurnitureData` veri sözleşmesini Kişi 1 ilk 2 saatte tanımlar. Kişi 2, ağırlık ve sabitleme durumunu tüketir; Kişi 4 ekonomi ve maliyetleri ekler. Aynı C# dosyasını iki kişi düzenlemez.

### Kişi 1 kabul kriterleri
- Erişilebilir `GameState` enum: `Menu`, `Preparation`, `Disaster`, `Results`, `GameOver`.
- `GameManager` state geçişlerini yönetir ve event yayınlar.
- `Main.unity` entegrasyon sahnesi yalnızca Kişi 1 tarafından düzenlenir.
- Yeni branch merge edildiğinde derleme ve kısa smoke test yapılır.
- Windows build alınır.

### Kişi 2 kabul kriterleri
- `HouseBalanceController` public API: `RegisterFurniture`, `UnregisterFurniture`, `RecalculateBalance`, `CurrentTiltNormalized`.
- Sabitlenmemiş eşyalar depremde fiziksel tepki verir; sabitlenenler kaymaz.
- Deprem şiddeti `float intensity`, süre `float duration` ile ayarlanır.
- Denge göstergesi için 0–1 normalize değer çıkar.
- `PhysicsSandbox` içinde bağımsız test yapılır.

### Kişi 3 kabul kriterleri
- 16:9 referans çözünürlükte okunabilir kesit ev ve HUD.
- 4 mobilya görseli, sade arka plan, hazırlık ve afet görsel farklılığı.
- HUD: denge, kalan afet süresi, bütçe, afet uyarısı.
- Tek UI prefab'ı bir veri metodu ile (`RefreshHUD`) güncellenebilir.
- Görselde okunabilirlik/kontrast kontrolü yapılır.

### Kişi 4 kabul kriterleri
- 4 mobilyalık `ScriptableObject`/veri seti.
- Başlangıç bütçesi, sabitleme bedeli, satın alma/iptal mantığı.
- Sabitleme sadece hazırlık state'inde mümkün; afet sırasında yapılamaz.
- Tur sonunda güvenlik ve korunan eşya üzerinden skor.
- En az 3 açıklamalı farkındalık mesajı; yanlış davranışı ödüllendirme yok.

## 6. Entegrasyon sözleşmesi — herkesin uyması gereken API

```csharp
public enum GamePhase { Menu, Preparation, Disaster, Results, GameOver }
public enum DisasterType { Earthquake, Fire, Storm }

public interface IFurnitureItem {
    string ItemId { get; }
    float Mass { get; }
    int Value { get; }
    bool IsAnchored { get; }
    void SetAnchored(bool value);
}

public interface IBalanceService {
    float TiltDegrees { get; }
    float NormalizedDanger { get; } // 0 = safe, 1 = critical
    void RegisterFurniture(IFurnitureItem item);
    void UnregisterFurniture(IFurnitureItem item);
    void RecalculateBalance();
}

public interface IDisasterService {
    bool IsRunning { get; }
    void StartEarthquake(float intensity, float duration);
    void StopDisaster();
}

public interface IPreparationService {
    int Budget { get; }
    bool TryPurchaseAnchoring(IFurnitureItem item);
    bool TryRemoveAnchoring(IFurnitureItem item);
}
```

Event sözleşmeleri: `OnPhaseChanged(GamePhase)`, `OnFurnitureLost(string itemId)`, `OnDisasterStarted(DisasterType)`, `OnDisasterEnded()`, `OnBudgetChanged(int)`.

**Kural:** Bu sözleşmeler değişecekse önce PR veya takım mesajı ile mutabakat gerekir. Claude Code kendi başına public API değiştirmez.

## 7. Önerilen repo yapısı

```text
SonDenge/
├── .github/
│   ├── pull_request_template.md
│   └── workflows/                   # varsa build/CI
├── .gitattributes                   # Unity YAML & Git LFS
├── .gitignore                       # Unity template
├── README.md
├── CLAUDE.md                        # tüm ajanlar için kurallar
├── docs/
│   ├── GDD.md
│   ├── API_CONTRACTS.md
│   ├── TASK_BOARD.md
│   └── PLAYTEST.md
├── Assets/
│   ├── Art/
│   │   ├── Environment/
│   │   ├── Furniture/
│   │   └── UI/
│   ├── Audio/
│   ├── Data/
│   │   └── Furniture/
│   ├── Prefabs/
│   │   ├── Furniture/
│   │   ├── Visual/
│   │   └── UI/
│   ├── Scenes/
│   │   ├── Boot.unity
│   │   ├── Main.unity
│   │   ├── PhysicsSandbox.unity
│   │   ├── UISandbox.unity
│   │   └── PreparationSandbox.unity
│   └── Scripts/
│       ├── Core/
│       ├── Player/
│       ├── Physics/
│       ├── Disasters/
│       ├── UI/
│       ├── Preparation/
│       ├── Economy/
│       └── Awareness/
├── Packages/
└── ProjectSettings/
```

Unity repo kuralları: `Assets/**/*.meta` dosyaları commit edilir. `Library/`, `Temp/`, `Obj/`, `Logs/`, `Build/` commit edilmez. **Asset Serialization = Force Text**, **Version Control = Visible Meta Files**. Repo kurulurken resmi Unity `.gitignore`, Unity YAML merge ayarı ve büyük binary assetler için Git LFS kullanılır. `Packages/packages-lock.json`, `Packages/manifest.json` ve `ProjectSettings/` commit edilir. Tüm ekip tek Unity Editor sürümü kullanır.

## 8. GitHub akışı

1. Lead boş GitHub repo oluşturur; Unity projesini ilk commit olarak `main`e gönderir.
2. Lead `docs/API_CONTRACTS.md`, `CLAUDE.md`, sahne ve klasör iskeletini oluşturur.
3. Her üye kendi branch'ini `main`den açar; paralel geliştirir.
4. Her PR sadece kendi klasörlerini değiştirir; `Main.unity` ve proje ayarları lead'e aittir.
5. PR açmadan önce Unity derlemesi ve görev alanına özel smoke test yapılır.
6. Lead PR'leri art arda merge eder ve bütün akışı oynar.
7. Release build'i sadece lead alır; build harici bir bilgisayarda çalıştırılarak doğrulanır.

**Worktree örneği:**
```bash
git worktree add ../sondenge-physics -b feature/physics
git worktree add ../sondenge-art -b feature/art-ui
git worktree add ../sondenge-preparation -b feature/preparation
```
Bu komutların tek kişinin bilgisayarında çoklu çalışma alanı açmak için örnek olduğu; ayrı bilgisayarlarda normal clone+branch'in yeterli olduğu unutulmamalı.

## 9. Claude Code + Unity MCP kullanım politikası

- Claude Code öncelikle kod, dosya oluşturma, test, dokümantasyon ve küçük refactor işleri için kullanılır.
- Unity MCP kuruluysa sahneye nesne ekleme, component bağlama, prefab yönetimi ve Editor içi gözlem için kullanılabilir; **MCP araç isimleri bağlanan sunucuya göre değişir**, promptlarda var olmayan araç adları uydurulmaz.
- Aynı Unity Editor'a iki ajan aynı anda yazmaz.
- Her prompt mevcut Unity sürümünü ve `docs/API_CONTRACTS.md` dosyasını okumayı şart koşar.
- Başka ekibin sorumluluğundaki dosya gerekiyorsa değiştirmek yerine stub/interface önerilir ve lead'e bildirilir.
- AI'dan çıkan kodun derlenmesi, Unity Editor'da çalışma testi ve kısa manuel oyun testi yapılmadan PR kabul edilmez.
- Her ajan başlangıçta değişecek dosyaları açıklar; işlem sonunda değişen dosyaları, test sonucunu ve entegrasyon adımlarını raporlar.

### CLAUDE.md içeriği (kopyalanabilir)

```md
# Son Denge — AI Agent Rules
We are building a 48-hour Unity 2D game jam MVP about disaster preparedness.
Goal: a playable Preparation -> Earthquake -> Results loop.
Read docs/GDD.md and docs/API_CONTRACTS.md before editing.
Do NOT expand scope beyond your assigned feature.
Do NOT change public interfaces without approval.
Do NOT edit Assets/Scenes/Main.unity unless you are the integration owner.
Do NOT modify ProjectSettings or Packages without approval.
Always preserve Unity .meta files, avoid reimport churn, and keep scripts compilable.
Write small, testable C# components; prefer clarity to abstractions.
When MCP is enabled, use only capabilities verified to exist in the installed MCP server.
Before coding, state planned files. After coding, report files changed, test result,
manual Unity steps, and open issues. Never claim tests ran if they did not.
Safety: clearly distinguish fantasy furniture manipulation from real-life earthquake advice.
```

## 10. Dört Claude Code ajanı için doğrudan görev promptları

### Ajan 1 — Core / Integration

```text
You are the core gameplay/integration owner for Son Denge, a 48-hour Unity 2D game jam.
Read CLAUDE.md, docs/GDD.md, docs/API_CONTRACTS.md. Work only in Core/, Player/, and Main.unity.
Build GameManager with Menu -> Preparation -> Disaster -> Results -> GameOver phases.
Implement input for selecting and repositioning mobile furniture during preparation.
Wire services through serialized references/interfaces; use fallbacks if other modules are not ready.
Create a short vertical slice with restart, win/lose, and game-end UI hooks.
Do not implement physics or economy internally; define integration points. Produce compilable
scripts and a checklist of scene wiring. If Unity MCP exists, verify tools before using it.
```

### Ajan 2 — Physics / Disaster

```text
You own physics and earthquake behavior in Son Denge. Read all project docs and contracts.
Change only Scripts/Physics/, Scripts/Disasters/ and PhysicsSandbox.unity.
Implement IBalanceService and IDisasterService with understandable, tunable values.
Furniture positions and mass must affect house tilt. Anchored furniture must not slide
as unsecured furniture does. Earthquake strength and duration must be configurable.
Expose current angle and danger 0..1 for HUD. Use a simple stable game-feel model, not
complex full structural simulation. Do not edit Main.unity. Provide sandbox test steps.
```

### Ajan 3 — Art / UI

```text
You own the 2D presentation of Son Denge. Read CLAUDE.md, GDD, API contracts.
Change Art/, Prefabs/Visual/, Prefabs/UI/, Scripts/UI/ and UISandbox.unity only.
Use coherent readable 2D hand-painted or stylized visuals with clear silhouettes.
Build HUD for balance, countdown, budget, preparedness and disaster alert.
Build preparation and results UI panels including short awareness feedback.
Avoid editing Main.unity. Expose simple public methods for lead integration.
Use placeholders if final sprites are unavailable. Validate 16:9 layout readability.
```

### Ajan 4 — Preparation / Economy / Awareness

```text
You own preparation mechanics, economy and learning outcomes for Son Denge.
Read project docs and API contracts. Change only Preparation/, Economy/, Awareness/,
Data/, and PreparationSandbox.unity.
Implement a Wallet, furniture data, anchor purchase/remove rules restricted to
Preparation, and score/report computation after disaster.
Define starting budget and values as serialized config, not hardcoded scattered numbers.
Reward protection and human safety over collecting valuables. Implement at least
three concise Turkish messages that teach pre-disaster mitigation and clarify that
fantasy furniture movement is not a real earthquake safety behavior.
Do not edit Main.unity or change common contracts. Include test cases.
```

## 11. 48 saatlik zaman planı

| Zaman | Ekip hedefi | Go/No-Go kriteri |
|---|---|---|
| 0–2. saat | Repo, Editor sürümü, mimari, API sözleşmesi, taslak assetler | Herkes proje açabiliyor |
| 2–8. saat | Dört ayrı sandbox + kod iskeleti | Her alan kendi başına derleniyor |
| 8–16. saat | İlk fizik, HUD, oyun döngüsü, ekonomi | Mobilya taşıma eğimi değiştiriyor |
| 16–24. saat | İlk ortak entegrasyon | Baştan sona deprem turu oynanıyor |
| 24–32. saat | Ayar / bugfix / ses / eğitim mesajları | Kritik hata yok |
| 32–40. saat | P1 ancak P0 temizse; kalite, görsel ve ses | Build test edilmiş |
| 40–44. saat | Son smoke test, Windows build, ekran görüntüleri | Başka bilgisayarda açılıyor |
| 44–48. saat | Submission, kısa oynanış videosu, pitch, yedek dosyalar | Teslim dosyası hazır |

**Kural:** 24. saatte ilk oynanabilir build çıkmıyorsa yangın, fırtına ve kat ekleme iptal edilir. 40. saatten sonra yeni özellik eklenmez.

## 12. Test senaryoları / Definition of Done

- Oyun ana menüden başlayabiliyor ve 60 sn içinde ilk hazırlık eylemi yapılabiliyor.
- Ağırlığı sağa taşıdığımda sağa eğim belirgin artıyor; sola taşıyınca azalıyor.
- Dolabı sabitlersem depremde kaymıyor; sabitleme bütçeden düşüyor.
- Deprem süresi bitince sonuç ekranı açılıyor ve hasar/korunan eşya doğru hesaplanıyor.
- Oyun kaybedilirse neden kaybedildiği anlaşılabiliyor.
- Yeniden başlatma butonu döngüyü sıfırlıyor.
- Hazırlık mesajları ve afet anı uyarıları birbirini çelişmiyor.
- Tüm sahnelerde Missing Script / Missing Reference hatası yok.
- Windows build'i ve ses ayarları çalışıyor.
- README oyun kontrollerini ve üçüncü taraf asset / ses atıflarını içeriyor.

## 13. 60 saniyelik jüri sunumu

“Son Denge'de sivri bir kayanın tepesindeki fantastik bir evi koruyoruz. Bu evde her eşyanın ağırlığı var ve tek bir dolabın yerini değiştirmek bile tüm yapıyı etkiliyor. Oyuncu afet gelmeden önce bütçesini nasıl kullanacağına karar veriyor: Eşyaları sabitleyecek mi, daha güvenli yerlere mi taşıyacak? Sonra deprem geliyor ve bütün kararlar bir anda test ediliyor. Korunamayan eşyalar yalnızca fiziksel dengeyi değil, sonraki turun kaynaklarını da etkiliyor. Böylece oyuncu, afetlere hazırlığın kriz anında verilen tepkiden önce başladığını oynayarak öğreniyor. Biz eğlenceyi fizik tabanlı denge sistemiyle, farkındalığı ise hazırlık kararlarının sonuçlarıyla birleştiriyoruz.”

## 14. Takım içi ilk mesaj

“İlk 2 saatte herkes aynı Unity sürümünü açsın ve API sözleşmesini okusun. Core/main sahnesi tek sahibi olan kişide kalacak. Diğerleri kendi sandbox sahnelerinde ve branch'lerinde çalışacak. Claude Code ajanlarına yalnızca kendi dosya alanlarını yazma yetkisi veriyoruz. 16. saatte tüm modüllerin ilk PR'leri, 24. saatte ilk oynanabilir build hedefimiz var. Yangın ve fırtına, ilk deprem döngüsü sorunsuz çalışana kadar kapsam dışı.”

## 15. Bilinçli varsayımlar

- Game Jam süresinin **48 saat** olduğu varsayılmıştır; gerçek süre farklıysa takvim oransal güncellenir.
- Ekip üyelerinin isim/rolleri belirtilmediğinden Kişi 1–4 geçici atanmıştır.
- AI/Unity MCP servisinin hangisi olduğu kesinleşmediği için satıcıya özel araç çağrısı uydurulmamıştır.
- Gerçek dünya deprem tavsiyeleri güncel resmî kaynaklarla son kontrolden geçirilmelidir.
