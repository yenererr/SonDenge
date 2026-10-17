# Level 1: project-files birleşimi

`Downloads/project-files` içindeki oyun kuralları, mevcut oynanabilir Unity sahnesine uyarlandı. Ana ev, odalar, katlar ve mobilyalar mevcut 3D modellerdir. Kaynak klasörün kendisi çalışma zamanı bağımlılığı değildir; yalnızca eksik eşya/afet görselleri ve müzikler `Art/ProjectFiles` ile `Audio` altında projeye kopyalandı. Kaynaktaki `.png` uzantılı resimler WebP biçimindeydi; Unity'nin okuyabilmesi için gerçek PNG biçimine dönüştürüldüler.

## Sahnede kullanılanlar

- Kaynak prototipin resimli ev ve oda arka planları sahnede kullanılmaz. Katlar, merdivenler, karakter ve var olan mobilyalar gerçek 3D nesnelerdir; yürünebilir ve fizik etkileşimine açıktır.
- Oyun iki katlı 3D evle başlar. Üçüncü kat, ikinci merdiven ve üst balkon/vinç parçaları 3. seviyede açılır; çatı o sırada üst kata yükselir.
- Mevcut dağ/gökyüzü ve mevcut 3D mobilya modelleri korunur. Markette karşılığı olan eşyalar bu modellerin kopyasını üretir. Karşılığı bulunmayan eşyalar, kaynak resimlerinden oluşturulan geçici 2D görseller ve fizik kutuları kullanır.
- Kaynak listedeki 12 mobilya, deprem çantası, sabitleme, fırtına desteği, yangın tüpü/dedektörü, paratoner/gerilim koruması ve yağmur ekipmanları markettedir. Resmi bulunmayan 9 afet ekipmanı için şeffaf geçici simgeler vardır.
- Kaynak oyunun iki müziği ve Web Audio tonlarına dayanan kısa satın alma, çivileme, yıldırım, başarı ve yenilgi efektleri kullanılır.

## Oynanış

Beş afet sırası: deprem, fırtına, yangın, yıldırım, şiddetli yağmur. Hazırlık süresi 45 saniye, başlangıç bütçesi 750 altın, sonraki aşamalara geçiş geliri 450 altındır. Eşyaların kütlesi ve yatay konumu dengeye etki eder; eğim ve afet sırasında çivilenmemiş eşyalar kayar. 26° kritik eğim, deprem çantası gereği, yangın söndürme, yıldırım hasarı, su tahliyesi, başlangıç/oyun menüleri, market çekmecesi ve aşama raporları `L1_ProjectGameController` içinde uygulanır.

Sarkan lambalar evin katlarına bağlıdır; ev eğildiğinde konumları evle birlikte değişir ve tavandaki bağlantı çevresinde hafifçe sallanır. Başlangıçtaki iki katın mobilyaları ağırlık merkezi yaklaşık ortada olacak şekilde yerleştirilmiştir. Karakter ve mobilya ağırlığı sağa geçince evin sağı aşağı iner; açı ve eşya kayması filtrelenmiş, yavaş bir tepki verir. Çivilenmemiş eşyalar eğimde kayar, çivilenenler eve sabit kalır. Deprem dışındaki aşamalarda hızlı deprem sallantısı uygulanmaz.

Katların yan duvar çarpışması karakteri içeride tutar, fakat mobilyaları durdurmaz. Serbest eşyalar sağ veya sol kenardan taşıp zeminin desteğini kaybedince fizik etkisiyle aşağı düşer.

`A/D` ile eşyanın yanına yürüyün; hazırlık aşamasında `E` ile yakındaki eşyayı alın, karakterle birlikte taşıyın ve yeniden `E` ile boş bir yere bırakın. Yakındaki eşyayı sol fareyle sürüklemek de mümkündür. `W/S` ile merdiven çıkın/in, `I` ile marketi açın, `F` ile seçili mobilyayı sabitleyin, `Boşluk` ile afeti başlatın. Yangın sırasında `E` yangın tüpünü kullanır. Dokunmatik ekranda eşya sürükleme, yön tuşları ve bağlama göre alma/bırakma ya da yangın söndürme yapan `E` düğmesi kullanılabilir.

Sahneyi yeniden kurmak gerektiğinde `Son Denge/Level1/Oynanabilir 3D Cozy Evi Kur` menüsü kuralları da ekler. Var olan sahneye yalnızca bu birleşimi uygulamak için `Son Denge/Level1/Proje Dosyası Mekaniklerini 3D Eve Uygula` menüsü kullanılabilir.

## `Downloads/yener1` mekanik karşılaştırması

`yener1/App.tsx`, `constants.ts` ve `playground_config.json` ayrıca incelendi. Unity sürümünde başlangıç bütçesi 750, bölüm başına 450 gelir, 45 saniye hazırlık, afet süreleri 10/12/30/25/25 saniye, 26° devrilme sınırı, 18° afet başlatma uyarısı, market fiyatları ve bölüm bonusları kaynakla uyumludur. Yıldırım hasarı korunmasız 37, gerilim koruyucuyla 25 puandır. Yağmurun saniyelik girişi 16; oluk, drenaj ve pompanın azaltmaları sırasıyla 5,5 / 6 / 7'dir. Biriken su `net giriş × süre × 2,2` ile, içerideki su ise eşik aşımı ve yalıtım katsayısıyla hesaplanır.

Başlangıçtaki 3D eşyaların Rigidbody kütlesi görsel/fizik kararlılığı için küçük tutulur; denge hesabı ve arayüz, kaynak katalogdaki kilogram değerlerini kullanır. Katalog dışındaki 3D eşyalar için denge ağırlığı fizik kütlesinden türetilir. Marketten alınan yeni eşya merkeze yakın boş konuma yerleştirilir. Tork, evin metre cinsinden konumuna göre ölçeklenir ve filtrelenir; 2D prototipin piksel cinsinden katsayısı doğrudan kopyalanmaz. Kaynak oyunda karakterin denge yükü hazırlıkta 5 kg, afette 0 kg'dır. Bu 3D sürümde karakterin yürüyüşü önceki oyun isteği gereği 8 kg, taşıdığı eşya da kendi denge kütlesinin %75'i kadar etki eder. Yangın/yıldırım/yağmur aşamalarında kaynak oyundaki kilitli dengeye benzer güçlü stabilizasyon vardır; 3D evde küçük bir eğim görsel geri bildirim olarak kalır. Başlangıçta iki kat görünür, üçüncü kat 3. seviyede açılır.

Yangın iki farklı can değerli alevle başlar (100 ve 85). Bir alev ancak 16 saniye sonra, canı 60'ın üzerindeyken ve toplam alev sayısı dördün altındayken yayılır; yayılan alevin canı 75'tir. Köpük aynı kattaki, karakterin baktığı yöndeki yakın alevi saniyede 75 can azaltır. Her söndürülen alev 300 puan kazandırır. Duman dedektörü ve su sensörü ekranda erken uyarı verir. Aşama raporu söndürülen/yayılan alevleri, yıldırım darbelerini ve gerçekten tahliye edilen suyu gösterir.
