# Güvenlik Metinleri İncelemesi (şartname §11)

**Durum: İç inceleme yapıldı — resmî kaynaklarla çevrim içi doğrulama YAPILMADI.**
Teslimden önce aşağıdaki metinler AFAD, 112 / itfaiye ve Meteoroloji Genel Müdürlüğü yayınlarıyla bir kişi tarafından karşılaştırılmalıdır.
Bu belge "doğrulandı" iddiası taşımaz.

## Oyun içi ayrım

- Evin ağırlıkla dengelenmesi **hayal ürünü oyun kuralıdır**. Bu bilgi brifingin son sayfasında, NASIL OYNANIR ekranında ve her AFET RAPORU'nun alt satırında yazılıdır.
- Gerçek davranış önerileri ayrı satırlarda verilir: `LevelDefinition.realLifeTipTr` ve `safetyWarningsTr`.

## Bölüm bazında metinler ve kontrol listesi

| Bölüm | Oyunda verilen gerçek hayat mesajı | Kırmızı çizgi kontrolü |
|---|---|---|
| 1 Deprem | Ağır eşyaları önceden duvara sabitle; deprem anında ÇÖK–KAPAN–TUTUN; deprem sırasında eşya taşıma. | Afette sürükleme/taşıma kapalı (`drag.canDrag` yalnız hazırlıkta). Çanta afette taşınmaz. |
| 2 Fırtına | Fırtınadan önce pencereleri kapat, gevşek eşyaları içeri al; fırtınada camlardan uzak dur; çatıya/balkona çıkma. | Çatı bağlantısı içeriden ve yalnız hazırlıkta yapılır. Afette tüm görev noktaları kilitlenir. Eski radyo satırı ("ağır eşyaları rüzgârın tersine al") kaldırıldı. |
| 3 Yangın | Yangın büyükse söndürmeye çalışma, hemen çık ve 112'yi ara; duman alarmı yangını söndürmez, erken uyarır. | Kaçış yolu açıkken **tahliye başarı sayılır** (söndürme zorunlu değil). Alarm söndürme etkisi vermez. |
| 4 Yıldırım | Fırtınada elektrikli cihazlara ve pencerelere yaklaşma; paratoner kurulumu uzman işidir; gerilim koruması mutlak koruma değildir. | Paratoner görevi "önceden kurulu sistemin sembolik son bağlantısı"dır; çatıya çıkılmaz. Afette fiş çekmek kilitlidir (test edildi). Eski radyo satırı ("Paratoner kur!") kaldırıldı. |
| 5 Şiddetli yağmur | Su basan alanda elektriğe dokunma, yüksek ve güvenli yere geç; resmî uyarıları takip et. | Pompa önceden kurulu ve otomatiktir; oyuncu ıslak ortamda pompaya veya prize dokunmaz. Drenaj ve yalıtım yalnız hazırlıkta yapılır. |

## Bilinen eksikler

- Metinler kısa ve oyun içi kullanıma göre yazıldı; resmî broşür ifadeleriyle birebir aynı değildir.
- Yıldırım bölümündeki "pencere/elektrik yakınında kalma" ölçümü yaklaşık bir mesafe kontrolüdür.
