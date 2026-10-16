# SON DENGE (LAST BALANCE) — Oyun Tasarımı (Ana Referans)

> Bu dosya oyunun **nasıl olacağını** anlatır. Sayılar ve kurallar bağlayıcıdır; değiştirmek için takım onayı gerekir.
> Motor: Unity 6.3 LTS, 2D (URP). Tüm görünen yazılar **Türkçe**.
> Ana mesaj: **"Afet geldiğinde değil, gelmeden önce hazırlan."** · Son mesaj: **"AFETE HAZIRLIK HAYAT KURTARIR."**

Oynanabilir bir oyun yapıyoruz: konsept, mockup, slayt veya sadece menü değil.
Temel mekanik: sivri bir dağ zirvesinde, pivot üzerinde dengede duran çok katlı bir ev. Mobilyaların kütlesi ve konumu dengeyi etkiler, afetler dengeyi bozar.

## 1. Hikâye ve dünya
- Kurgusal dağ köyü **Sivritepe**. İnanılmaz ince ve sivri bir dağın tepesinde tuhaf, ahşap bir ev.
- İçindeki her eşya evin dengesini etkiler. Afetler giderek tehlikeli hale geliyor.
- Oyuncu: görünmez sihirli koruyucu **"Denge"**. Hareketli mobilyaları telekinezi ile oynatabilir.
- Ev oyun ilerledikçe yükselir (kat eklenir), denge zorlaşır.
- Dünya hafif fantastik, esprili, abartılı; ama afet bilinci dersleri gerçek güvenlik ilkelerine dayanır.

## 2. Görsel yön
- El yapımı çizgi film tarzı, premium görünümlü 2D indie. Var olan oyun/karakterleri kopyalama.
- 2D yandan görünüş; ev **kesit / bebek evi** görünümünde; altında sivri kayalık dağ net görünür.
- Sıcak pastel renkler, ahşap duvar/zemin, yumuşak ışık, belirgin mobilya silüetleri, net oda sınırları, renkli hava efektleri.
- Palet: sıcak krem, tozlu turuncu, zeytin yeşili, yumuşak teal, kahverengi ahşap, soluk gök mavisi, sıcak sarı vurgular.
- Kaçın: kurumsal web estetiği, neon/cyberpunk, kalabalık görsel, korku, fotogerçekçi mobilya, **final oyunda düz renkli placeholder dikdörtgenler**.
- Ağırlık, dengesizlik ve yaklaşan tehlike görsel olarak hissedilmeli. **16:9** yatay.

## 3. Kamera ve bölüm yapısı
- Sabit 2D yandan kamera; ev her zaman tamamen görünür.
- Başlangıç: tek ince kayalık zirve, **2 katlı ahşap ev, her katta 2 oda**, görünür merkezi **pivot**, zemin, duvar, çatı, hareketli mobilyalar, illüstrasyon dağ arka planı, uzakta köy ve bulutlar.
- **Tüm ev pivot etrafında sağa/sola gerçekten döner.** Ev eğilirken mobilyalar görünür kalır.
- Düz zeminde normal ev **yok**: sivri zirve oyunun özü.

## 4. Denge mekaniği — EN YÜKSEK ÖNCELİK
Mobilya kütleleri:

| Eşya | Kütle | Değer | Sabitleme bedeli |
|---|---|---|---|
| Sandalye | 1 | 10 | 5 |
| Masa | 2 | 15 | (≈10) |
| Kanepe | 3 | 25 | (≈12) |
| Kitaplık | 4 | 30 | 15 |
| Gardırop | 5 | 40 | 20 |
| Buzdolabı | 6 | 50 | 25 |

- Mobilyanın etkisi = kütle × pivota yatay uzaklık. Uzak sola ağır eşya → güçlü sola dönme; merkeze yaklaştırınca etki azalır.
- Oyuncu yerleşimin dengeyi nasıl değiştirdiğini **hemen** görmeli.
- Basitleştirilmiş ama tutarlı **tork modeli**: evin yapısal kütlesi ve ağırlık merkezi var; mobilya torku eklenir; deprem geçici bozucu kuvvet ekler; dönüş **atalet + sönümleme** ile yumuşak, anında açıya zıplamaz.
- Çökmeden önce oyuncuya anlaşılır uyarı verilir. Gerçekçilikten çok eğlenceli, kontrol edilebilir, okunaklı.

| Durum | Açı |
|---|---|
| GÜVENLİ | 0–7° |
| DENGESİZ | 7–16° |
| KRİTİK | 16–28° (tehlike geri bildirimi) |
| ÇÖKÜŞ | 28°'yi ~1 sn aşarsa |

Eşikler oynanışa göre ayarlanabilir. **Ev görsel olarak gerçekten döner; sadece gösterge yetmez.**

## 5. Mobilya etkileşimi
Fare ile:
- Sol tık: seç · Sürükle: geçerli konuma taşı · Bırak: yerleştir
- Üzerine gelince: kütle ve değer bilgisi · **ESC**: seçimi iptal · **R**: başarısız turu yeniden başlat

Seçili mobilya: hafif dış çizgi, kütlesini gösterir, imleci takip eder, geçerli yerleri gösterir, sürüklerken **beklenen dengeyi** günceller.
- Mobilya zemine oturur, havada durmaz; duvarların içinden geçemez.
- Hazırlıkta serbestçe taşınır. Afet sırasında **sabitlenmemiş** eşya sihirle taşınabilir ama yavaş/**enerji göstergesiyle** sınırlı. **Sabitlenmiş eşya afette taşınamaz.**
- Mobilya: güvende kalır / kayar / devrilir / kırılır / evden düşer. **Evden düşen eşya denge hesabından çıkar.**
- Düşme ve kırılmada net görsel + ses tepkisi.

## 6. Hazırlık fazı
Her tur hazırlıkla başlar: sınırlı bütçe, afet uyarısı, düzenleme süresi, güvenlik yükseltmeleri.
Radyo uyarısı örneği: **"RADYO SİVRİTEPE"** — "Bu gece bölgede şiddetli sarsıntı bekleniyor. Önlemlerinizi alın!"

1. Mobilyaları taşıyarak ağırlığı dağıt · 2. Parası yeterse yeni mobilya al · 3. Mobilyaları duvara sabitle · 4. Uygun güvenlik ekipmanı al · 5. Denge göstergesine bak · 6. **"AFETİ BAŞLAT"**

- Başlangıç bütçesi: **100 para.**
- Sabitleme: para tutar; depremde kaymayı/devrilmeyi önler; afette taşımayı engeller; görünür **çapa ikonu**; hazırlıkta geri alınabilir.
- **Sabitleme kütleyi veya ağırlık merkezine katkıyı ortadan kaldırmaz.**
- Oyun gerçek deprem hazırlığını ödüllendirir.

## 7. Afet fazı — DEPREM (ilk zorunlu afet)
"AFETİ BAŞLAT" sonrası:
- 3 sn geri sayım → **"DEPREM!"** → ev sarsılır, değişen yönlü yatay kuvvetler.
- Sabitlenmemiş eşyalar kayar, devrilebilir, açık kenarlardan düşebilir; eşya yer değiştirdikçe denge değişir.
- Kamera sarsıntısı, toz parçacıkları, ses efektleri; kalan süre gösterilir. Önerilen süre: **25 sn**.
- Oyuncu sınırlı enerjiyle sihirli müdahale yapabilir — bu **fantastik mekaniktir, gerçek tavsiye değildir**.
- Ev sakinleri koşturmaz, **uygun korunma pozisyonu** alır.
- Ekranda: **"Gerçek depremde eşya taşımayın. Çök–Kapan–Tutun."**
- Süre bitince deprem biter. Ev çökerse kayıp: dramatik ama kısa çöküş animasyonu + yeniden başlat.

## 8. Eşya kaybı ve ekonomi
Her eşya: kütle, değer, yapısal dayanıklılık, sabitli/sabitsiz, sağlam/kırık.
Afet sonunda:
- Sağlam kalan eşya → koruma ödülü · Kaybolan → ödül yok · Kırık → ceza · Hazırlık eylemleri → ek bonus
- **Evin ayakta kalması ve sakinlerin güvenliği paradan önemlidir.**
- Sonsuz para kasma açığı yok: ödül **tur başına bir kez** hesaplanır.

## 9. Sonuç ekranı — "AFET RAPORU"
Gösterilir: Ev Durumu · Hazırlık Puanı · Korunan Eşya · Kaybedilen Eşya · Kazanılan Para · Güvenlik Puanı
Örnek: "EV AYAKTA KALDI!" · "6 / 8 EŞYA KORUNDU" · "+60 PARA" · "HAZIRLIK PUANI: 80 / 100"

Oyuncunun **gerçek** davranışına göre eğitici mesaj:
- Kitaplık sabitlendiyse: "Başarılı! Sabitlediğin kitaplık deprem sırasında devrilmedi."
- Ağır dolap sabitlenmediyse: "Dikkat! Sabitlenmeyen ağır dolaplar deprem sırasında tehlike oluşturabilir."
- Ağır eşya üst kattaysa: "Ağır eşyaları uygun ve güvenli konumlara yerleştirmek önemlidir."
- Sakinler korunduysa: "Harika! Afet öncesi hazırlıklar ev sakinlerinin güvende kalmasına yardımcı oldu."

Oyuncunun seçimleriyle ilgisiz mesaj **asla** gösterilmez. Buton: **"SONRAKİ TURA GEÇ"**.

## 10. İlerleme
- Tur 1: orta deprem, az mobilya, temel sabitleme
- Tur 2: daha güçlü deprem, daha çok mobilya, biraz daha büyük bütçe, karmaşık ağırlık dağılımı
- Tur 3: şiddetli deprem, ek mobilya, daha zor; güvenilir şekilde yapılabilirse 3. kat
- Kat eklemek zorluğu **anlamlı** artırmalı ama oyunu imkânsız yapmamalı. Zorluk kademeli artar.

## 11. Ek afetler (deprem döngüsü çalıştıktan SONRA)
**YANGIN**
- Belirli bir odada başlar, yakındaki mobilyalara yayılır. Korumasız mobilya dayanıklılık kaybeder, yanıp yok olabilir.
- Eşya yok olunca **kütlesi kaybolur ve denge değişebilir.**
- Yükseltmeler: **duman dedektörü** erken uyarı verir · **yangın söndürücü** küçük, kontrol edilebilir yangınlar için · **sprinkler** otomatik söndürür.
- **Duman dedektörü yangını kendisi SÖNDÜRMEZ.**
- Yangın, yeni bir kaynak yönetimi kararı yaratmalı.

**FIRTINA**
- Güçlü rüzgâr eve yanal kuvvet uygular, yön zamanla değişir; mobilya dağılımı rüzgâra dayanımı etkiler.
- Hazırlık: pencereleri güçlendir, gevşek eşyaları sabitle, çatıyı koru, iç yükleri dengele.
- Görsel: bulut, yağmur, rüzgâr, şimşek.

Deprem döngüsü tamamlanmadan ek afet eklenmez.

## 12. Arayüz (tamamen Türkçe)
- **Ana menü:** SON DENGE · OYNA · NASIL OYNANIR · AYARLAR
- **HUD sol üst:** tur numarası, mevcut faz, afet uyarısı
- **HUD üst orta:** büyük, şık yatay denge göstergesi; animasyonlu eğim işareti; GÜVENLİ / UYARI / KRİTİK
- **HUD sağ üst:** para, afet geri sayımı, hazırlık puanı
- **Alt:** seçili mobilya bilgisi, sabitle butonu, mağaza butonu, ana faz butonu ("AFETE HAZIRIM"; sonuçta "DEVAM ET")
- Büyük, okunaklı kontroller, ince animasyon; büyük paneller ekranı kaplamaz; **ev her zaman ana odak.**

## 13. Oyun hissi
Mobilya seçme sesi, yerleştirme animasyonu, ağır darbe sesi, ahşap gıcırtısı, deprem gürültüsü, kritik denge uyarısı, toz parçacıkları, yumuşak UI geçişleri, sakin ortam müziği, afette hafif ekran sarsıntısı.

**Oyunun kalbi olan an:** Oyuncu ağır gardırobu sağa taşır → ev yavaşça sağa eğilir → kanepeyi sola taşır → ev dengeye döner → deprem sabitlenmemiş kitaplığı kaydırır → denge aniden yine bozulur.

## 14. Eğitim doğruluğu
Tema menü yazısında değil, oynanışta görünmeli. Önleyici önlemler ödüllendirilir.
Öğretilenler: uzun ve ağır mobilyaları sabitlemek · afet öncesi plan yapmak · çıkış yollarını açık tutmak · duman alarmıyla erken tespit · tespit ile söndürmeyi ayırt etmek · depremde doğru korunma davranışı.
- Ev dengesi **kurgusal, sihirli** bir öncüldür. Gerçek binaların mobilya dengesiyle depreme dayanıklı olacağı ima edilmez.
- Deprem sırasında eşya taşımak veya ev içinde koşmak **öğretilmez.**
- Mesajlar kısa, faydalı ve oyuncu eylemine bağlı olur. (Yayından önce AFAD kaynaklarıyla kontrol.)

## 15. Oynanabilirlik gereksinimleri (hepsi zorunlu)
Ana menü oyunu başlatır · mobilya seçilir ve sürüklenir · kütleler farklı · taşıma dengeyi görünür şekilde değiştirir · ev sağa/sola eğilir · gösterge gerçek eğimle eşleşir · sabitleme çalışır · afet başlat butonu çalışır · deprem gerçek fiziksel bozulma yaratır · eşyalar kaybolabilir · kaybolan eşya dengeyi etkilemez · afet biter · ödüller hesaplanır · sonuç ekranı açılır · oyuncu yeniden başlatabilir veya devam edebilir.

Kararlı, basit fizik. Bir özellik zorsa oyuncunun gördüğü davranışı koruyarak basitleştir.
Oyunu tıklama oyununa, teste, sıra tabanlı metin oyununa veya statik bulmacaya **dönüştürme**. Etkileşimli 2D ev ve mobilya fiziği korunmalı.

## 16. Öncelikler
1. Oynanabilir ev dengesi
2. Mobilya sürükleme ve sabitleme
3. Kayan/düşen mobilyalarla deprem
4. Hazırlık → afet → sonuç döngüsü
5. Görsel, parçacık, ses, animasyon
6. Ek afetler ve ek katlar

Kısıt varsa: çok sayıda yarım özellik yerine **cilalı, tekrar oynanabilir tek bir deprem bölümü**.
