# Son Denge — Wilson 5 Bölüm: Son Doğrulama Raporu

Tarih: 2026-10-18 · Dal: `feature/wilson-missions-five-levels` · Yedek: `backup/pre-wilson-5level` (`c7639d6`)
Unity 6000.3.26f1 · URP · Windows (DX12)

## 1. Faz durumu

| Faz | Kapı | Kanıt |
|---|---|---|
| 0 Denetim | PASS | `PHASE_00_AUDIT.md` |
| 1 Mimari/karar | PASS | Lead kararları günlükte |
| 2 Görev çekirdeği | PASS | 21/21 mantık testi (MCP) |
| 3 Wilson brifingi | PASS | Play + ekran görüntüleri |
| 4 Etkileşim | PASS | Gerçek E/F/fare girdisi; 2 hata düzeltildi |
| 5 Level1 | PASS | 4/4 gerçek girdi; 2 yıldız; başarısızlık/tekrar dene |
| 6 Level2 | PASS | Editor Play |
| 7 Level3 | PASS | Editor Play (tahliye senaryosu, 3 yıldız) |
| 8 Level4 | PASS | Editor Play (afette fiş kilidi) |
| 9 Level5 | PASS | Editor Play (final raporu) |
| 10 Menü/kayıt | PASS | Bölüm seç, kilit, yıldız, sıfırla |
| 11 Build | PASS (kısmi test) | `Builds/Windows/Son Denge.exe` 502 MB, açıldı, Menü→Level1 brifingi; Player.log hata 0 |
| 12 Belgeler | PASS | Bu dosya, günlük, SAFETY_REVIEW, README |

## 2. 19 görevin test dökümü

Test biçimi: **G** = gerçek klavye/fare girdisiyle tamamlandı. **S** = görev koşulu, eşya hedefe konarak ya da nokta etkinleştirilerek test hızında kodla sağlandı; algılama, ödül ve raporlama gerçek sistemden geçti.

| Görev | Test | Sonuç |
|---|---|---|
| L1_T01 Kitaplığı sabitle | G (fare seç + F) | ✓ |
| L1_T02 Buzdolabını sabitle | G | ✓ |
| L1_T03 Deprem çantası | G (E al, yürü, E bırak) | ✓ |
| L1_T04 Denge ≤7°/2 sn | G | ✓ |
| L2_T01 Çatı bağlantısı | S | ✓ |
| L2_T02 Pencereler (3) | G (1 pencere E) + S | ✓ |
| L2_T03 Balkon eşyaları | S | ✓ |
| L2_T04 El feneri | S | ✓ |
| L3_T01 Alarm kur + test | S (test kurulumdan önce reddedildi ✓) | ✓ |
| L3_T02 Ocağı kapat | S | ✓ |
| L3_T03 Söndürücü | S | ✓ |
| L3_T04 Kaçış yolu | S | ✓ |
| L4_T01 Yıldırım bağlantısı | S | ✓ |
| L4_T02 Fişler | G (TV fişi E) ; afette 2. fiş kilitli ✓ | Missed (bilerek) |
| L4_T03 Gerilim koruması | S | ✓ |
| L5_T01 Kum torbaları | S | ✓ |
| L5_T02 Drenaj | S | ✓ |
| L5_T03 Eşyaları yukarı taşı | S | ✓ |
| L5_T04 Yalıtım | S | ✓ |

## 3. Bilinen eksikler / riskler (dürüst liste)

**P1**
- Level2–5'in tam oynanışı gerçek girdiyle **baştan sona oynanmadı**. Özellikle merdivenle kat arası taşıma (L3 alarm, L5 radyo/albüm) gerçek girdiyle test edilmedi.
- Build'de yalnız Menü → Level1 brifingi doğrulandı. Level4 → Level5 geçişi ve build'de afet akışı **NOT_RUN**.
- Afet sırasında ev ~23°'ye kadar çıkıyor (sarsıntı/rüzgâr + eşya kayması), bu yüzden denge puanı çoğunlukla düşük. Ayar gerekiyor.
- `ProjectSettings.asset` içinde build sırasında Unity'nin yazdığı farklar (preloadedAssets, batching) commit'lenmedi.

**P2 / şartnameden sapmalar**
- UI paneller prefab asset'i değil, çalışma anında kodla kurulan gerçek UGUI. Wilson metinleri bölüm başına bir `WilsonDialogueSet` asset'inde (satır başına ayrı asset değil).
- Fırtınada "elektrik kesintisi" görsel olayı yok; el feneri görevi bu yüzden rapora yalnız "erişilebilirdi" diye yazılıyor.
- Depremde "uygun koruma alanı" seçimi ölçülmüyor; yalnız Çök–Kapan–Tutun süresi ölçülüyor.
- Ayrı `MissionEventBus` / `DisasterCoordinator` sınıfları yok. Afet mantığı mevcut `L1_ProjectGameController` içinde kaldı, görev etkileri koruma anahtarlarıyla bağlandı.
- Görev nesneleri Unity primitive birleşimleriyle yapılmış low-poly modeller (lisans gerektirmiyor). Sanatçı işi modellerle değiştirilebilir.
- Projede Unity Test Framework kurulu değil (onaylanmadı). Testler MCP üzerinden çalıştırıldı, otomatik test dosyası yok.
- 1920×1080 ve 2560×1440 Editor Game görünümünde, 1280×720 build penceresinde görüldü. FPS ölçülmedi.
- Kişi 2'nin `kisi2-level2` dalındaki mağaza çalışması bu dala alınmadı; birleştirme Lead kararı.

## 4. Commit / dal

Commit'ler `feature/wilson-missions-five-levels` dalında, `main`'e dokunulmadı. Birleştirme PR ile yapılmalı.

## 5. Kullanılan varlıklar ve lisanslar

- Wilson tavuk: Pjager, CC BY 4.0. Yumurta: Casca_Servillius, CC BY 4.0 (`ThirdParty/Wilson/CREDITS.md`). Konum: `Assets/Levels/Level1/Art/Characters/Wilson`.
- Söndürücü: LowPolyInteriorsFree (Asset Store EULA, projede mevcut).
- Diğer görev nesneleri: Unity primitive birleşimleri (bu projede üretildi).
