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
