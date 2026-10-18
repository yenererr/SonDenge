using System.Collections.Generic;
using System.IO;
using Jam.Missions;
using UnityEditor;
using UnityEngine;

namespace Jam.EditorTools
{
    /// <summary>
    /// Şartnamedeki (SON_DENGE_WILSON_5_LEVEL) 5 bölüm, 19 görev ve Wilson metinlerini
    /// ScriptableObject olarak üretir/günceller. Mevcut asset'lerin GUID'i korunur.
    /// Menü: Jam > Wilson > Bölüm verilerini oluştur
    /// </summary>
    public static class WilsonContentBuilder
    {
        private const string LevelDir = "Assets/Data/Levels";
        private const string TaskDir = "Assets/Data/Missions";
        private const string DialogueDir = "Assets/Data/Wilson";

        [MenuItem("Jam/Wilson/Bölüm verilerini oluştur")]
        public static void BuildAll()
        {
            Directory.CreateDirectory(LevelDir);
            Directory.CreateDirectory(TaskDir);
            Directory.CreateDirectory(DialogueDir);
            AssetDatabase.Refresh();

            BuildLevel1();
            BuildLevel2();
            BuildLevel3();
            BuildLevel4();
            BuildLevel5();
            AssetDatabase.SaveAssets();
            Debug.Log("[Wilson] 5 bölüm verisi güncellendi.");
        }

        // ---------------------------------------------------------------- yardımcılar
        private static T Load<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static TaskDefinition Task(string id, string title, string desc, string why, string hint,
            string done, string report, TaskType type, string protection, bool critical, float weight = 1f,
            params string[] prerequisites)
        {
            var t = Load<TaskDefinition>(TaskDir + "/" + id + ".asset");
            t.id = id;
            t.levelId = id.Substring(0, 2);
            t.titleTr = title;
            t.descriptionTr = desc;
            t.whyItMattersTr = why;
            t.hintTr = hint;
            t.completionTr = done;
            t.reportTr = report;
            t.taskType = type;
            t.protectionKey = protection;
            t.safetyCritical = critical;
            t.scoreWeight = weight;
            t.prerequisiteTaskIds = prerequisites;
            EditorUtility.SetDirty(t);
            return t;
        }

        private static WilsonLine Line(string id, WilsonTrigger trigger, WilsonMood mood, string text) =>
            new WilsonLine { id = id, trigger = trigger, mood = mood, textTr = text };

        private static WilsonDialogueSet Dialogue(string file, List<WilsonLine> lines)
        {
            var d = Load<WilsonDialogueSet>(DialogueDir + "/" + file + ".asset");
            d.speaker = "WILSON";
            d.lines = lines;
            EditorUtility.SetDirty(d);
            return d;
        }

        private static void Level(string file, string id, int number, string scene, string next, string title,
            string disasterName, LevelDisaster disaster, int floors, float prep, float duration,
            TaskDefinition[] tasks, WilsonDialogueSet dialogue, string tip, params string[] warnings)
        {
            var l = Load<LevelDefinition>(LevelDir + "/" + file + ".asset");
            l.levelId = id;
            l.levelNumber = number;
            l.sceneName = scene;
            l.nextSceneName = next;
            l.titleTr = title;
            l.disasterNameTr = disasterName;
            l.disaster = disaster;
            l.floorCount = floors;
            l.preparationSeconds = prep;
            l.disasterSeconds = duration;
            l.timeWarningSeconds = 20f;
            l.tasks = tasks;
            l.dialogue = dialogue;
            l.realLifeTipTr = tip;
            l.safetyWarningsTr = warnings;
            EditorUtility.SetDirty(l);
        }

        // ---------------------------------------------------------------- Bölüm 1
        private static void BuildLevel1()
        {
            var tasks = new[]
            {
                Task("L1_T01", "KİTAPLIĞI SABİTLE", "Kitaplığı seç ve F ile duvara sabitle.",
                    "Ağır ve devrilebilir mobilyalar sarsıntıda tehlike yaratır.",
                    "Ağır ve devrilebilir mobilyaları önceden duvara sabitlemek önemlidir.",
                    "İşte bu! Kitaplık artık daha güvenli.",
                    "Kitaplık sabitlendi ve sarsıntıda devrilmedi.",
                    TaskType.AnchorFurniture, "anchor_bookshelf", true),
                Task("L1_T02", "BUZDOLABINI SABİTLE", "Mutfaktaki buzdolabını seç ve F ile sabitle.",
                    "Ağır eşyalar sabit değilse sarsıntıda tehlike yaratabilir.",
                    "Buzdolabı üst katta, mutfakta. Seç ve F'ye bas.",
                    "Buzdolabı yerinde kalacak!",
                    "Buzdolabı yerinde kaldı.",
                    TaskType.AnchorFurniture, "anchor_fridge", true),
                Task("L1_T03", "DEPREM ÇANTASINI HAZIRLA", "Çantanın yanına git, E ile sırtına tak.",
                    "Acil durum çantası kolay ulaşılabilir yerde olmalı.",
                    "Kırmızı çanta alt katta. Yanına git, üstündeki E'ye bas: sırtına takılır.",
                    "Çanta sırtında! Gerektiğinde hazır.",
                    "Acil durum çantası deprem öncesinde hazırlanmıştı.",
                    TaskType.CarryToTarget, "emergency_bag", false),
                Task("L1_T04", "EVİN DENGESİNİ KUR", "Eşyaları taşıyarak evi 2 saniye boyunca 7°'nin altında tut.",
                    "Bu evde eşyalar bütün binayı eğer. Bu oyunun fantastik kuralıdır.",
                    "Sivritepe'de bu evin eşyaları bütün binayı eğiyor. Bu bizim oyunumuzun fantastik kuralı!",
                    "Denge tamam! Gösterge yeşilde.",
                    "Hazırlıkta eğimi güvenli seviyeye indirdin.",
                    TaskType.RebalanceHouse, "balanced", false, 1f, "L1_T01", "L1_T02", "L1_T03"),
            };
            var d = Dialogue("L1_Dialogue", new List<WilsonLine>
            {
                Line("L1_INTRO_01", WilsonTrigger.Intro, WilsonMood.Normal, "Gıdak! Ben Wilson. Sivritepe'nin en dengesiz evine hoş geldin!"),
                Line("L1_INTRO_02", WilsonTrigger.Intro, WilsonMood.Endiseli, "Birazdan deprem olacak. Sarsıntıyı durduramayız ama önceden önlem alabiliriz."),
                Line("L1_TASKS_01", WilsonTrigger.TaskPreview, WilsonMood.Anlatici, "Önce kitaplığı, sonra buzdolabını güvenli biçimde sabitle."),
                Line("L1_TASKS_02", WilsonTrigger.TaskPreview, WilsonMood.Anlatici, "Deprem çantasını kolay erişilebilecek yere koy. Sonra eşyaları dengele!"),
                Line("L1_READY", WilsonTrigger.PrepStart, WilsonMood.Heyecanli, "Dört işimiz var. Denge göstergesine dikkat et. Haydi başlayalım!"),
                Line("L1_TIME", WilsonTrigger.TimeWarning, WilsonMood.Endiseli, "Gıdak! Deprem yaklaşıyor, önce en önemli işleri bitir!"),
                Line("L1_DISASTER", WilsonTrigger.DisasterWarning, WilsonMood.Endiseli, "Deprem başladı! Güvenli yerde ÇÖK–KAPAN–TUTUN! Eşya taşıma!"),
                Line("L1_REPORT_GOOD", WilsonTrigger.ReportGood, WilsonMood.Sevincli, "Gıdak! Hazırlıklarımız gerçekten işe yaradı!"),
                Line("L1_REPORT_BAD", WilsonTrigger.ReportBad, WilsonMood.Uzgun, "Bu defa olmadı. Yeniden deneyip tehlikeli eşyaları daha iyi sabitleyelim."),
            });
            Level("L1_Deprem", "L1", 1, "Level1", "Level2", "İLK SARSINTI", "DEPREM", LevelDisaster.Earthquake, 2, 90f, 12f,
                tasks, d, "Gerçek hayatta: ağır eşyaları önceden duvara sabitle, deprem anında ÇÖK – KAPAN – TUTUN.",
                "Deprem sırasında eşya taşıma.", "Sarsıntıda koşma; güvenli yerde Çök–Kapan–Tutun.");
        }

        // ---------------------------------------------------------------- Bölüm 2
        private static void BuildLevel2()
        {
            var tasks = new[]
            {
                Task("L2_T01", "ÇATI BAĞLANTILARINI GÜÇLENDİR", "İçerideki servis noktasında E ile çatı bağlantısını tak.",
                    "Fırtına gelmeden yapılan bağlantı, rüzgârın yapıya etkisini azaltır.",
                    "Çatı bağlantısı üst kattaki servis noktasında. Fırtınada çatıya çıkılmaz!",
                    "Çatı bağlantısı yerinde!",
                    "Çatı bağlantısı rüzgâr hasarını azalttı.",
                    TaskType.InstallEquipment, "storm_brace", true),
                Task("L2_T02", "PENCERELERİ KAPAT", "Açık pencerelerin hepsine gidip E ile kapat ve kilitle.",
                    "Kapalı pencereler içeri giren rüzgârı ve yağmuru azaltır.",
                    "Pencerelerin yanına git, E'ye bas. Hepsi kapanınca görev biter.",
                    "Pencereler kilitli. Rüzgâr içeri giremez!",
                    "Kapalı pencereler içeri gelen rüzgârı azalttı.",
                    TaskType.InteractSwitch, "windows_closed", true),
                Task("L2_T03", "BALKONDAKİ EŞYALARI İÇERİ AL", "Balkondaki gevşek eşyaları E ile al, içerideki depo alanına bırak.",
                    "Rüzgârda savrulan eşyalar tehlikelidir.",
                    "Balkondaki saksılar savrulabilir. İçerideki işaretli alana taşı.",
                    "Balkon boşaldı, savrulacak bir şey kalmadı!",
                    "Balkondan savrulabilecek eşyalar güvenli yere alındı.",
                    TaskType.CarryToTarget, "balcony_cleared", false),
            };
            var d = Dialogue("L2_Dialogue", new List<WilsonLine>
            {
                Line("L2_INTRO_01", WilsonTrigger.Intro, WilsonMood.Heyecanli, "Gıdak! Dağın üstünde rüzgâr şaka yapmıyor!"),
                Line("L2_INTRO_02", WilsonTrigger.Intro, WilsonMood.Endiseli, "Fırtına gelmeden pencereyi, çatıyı ve balkonu güvenli hâle getirmeliyiz."),
                Line("L2_TASKS_01", WilsonTrigger.TaskPreview, WilsonMood.Anlatici, "Çatı bağlantısını güçlendir, pencereleri kilitle ve balkondaki gevşek eşyaları içeri al."),
                Line("L2_READY", WilsonTrigger.PrepStart, WilsonMood.Heyecanli, "Rüzgâra karşı hazırlık yapalım; dengemizi de kaybetmeyelim!"),
                Line("L2_TIME", WilsonTrigger.TimeWarning, WilsonMood.Endiseli, "Gıdak! Fırtına geliyor, önce en önemli işleri bitir!"),
                Line("L2_DISASTER", WilsonTrigger.DisasterWarning, WilsonMood.Endiseli, "Fırtına başladı! Camlardan uzak, güvenli bir yerde kal!"),
                Line("L2_REPORT_GOOD", WilsonTrigger.ReportGood, WilsonMood.Sevincli, "Tıkırtılar azaldı! Önceden yaptığımız işler fark yarattı."),
                Line("L2_REPORT_BAD", WilsonTrigger.ReportBad, WilsonMood.Uzgun, "Rüzgâr bu sefer bizi yendi. Hazırlıkları tamamlayıp tekrar deneyelim."),
            });
            Level("L2_Firtina", "L2", 2, "Level2", "Level3", "DAĞ FIRTINASI", "FIRTINA", LevelDisaster.Storm, 2, 100f, 12f,
                tasks, d, "Gerçek hayatta: fırtınadan önce pencereleri kapat, gevşek eşyaları içeri al; fırtınada camlardan uzak dur.",
                "Fırtına sırasında çatıya veya balkona çıkma.", "Camlardan uzak dur.");
        }

        // ---------------------------------------------------------------- Bölüm 3
        private static void BuildLevel3()
        {
            var tasks = new[]
            {
                Task("L3_T01", "DUMAN ALARMINI KUR VE TEST ET", "Alarmı tavandaki montaj noktasına götür, E ile kur, sonra E ile test et.",
                    "Duman alarmı erken uyarır; yangını kendisi söndürmez.",
                    "Montaj noktası orta kattaki tavanda. Kurduktan sonra test düğmesine bas.",
                    "Bip bip! Alarm çalışıyor.",
                    "Duman alarmı erken uyarı verdi.",
                    TaskType.InstallEquipment, "duman_dedektoru", true),
                Task("L3_T02", "OCAĞI GÜVENLİ BİÇİMDE KAPAT", "Mutfaktaki ocağın yanına git ve E ile kapat.",
                    "Açık bırakılan ocak yangın kaynağı olabilir.",
                    "Ocak mutfakta. Yanına git, E'ye bas.",
                    "Ocak kapalı. Bir risk eksildi!",
                    "Ocak kapatıldığı için kaynak riski azaldı.",
                    TaskType.InteractSwitch, "stove_off", true),
                Task("L3_T03", "YANGIN SÖNDÜRÜCÜYÜ YERLEŞTİR", "Yangın tüpünü al, çıkışa yakın askı alanına bırak.",
                    "Söndürücü yalnız küçük ve güvenli yangında, çıkış yolu açıkken kullanılır.",
                    "Askı çıkışa yakın ama kaçış yolunu kapatmayan yerde.",
                    "Tüp yerinde ve ulaşılabilir!",
                    "Söndürücü erişilebilir durumdaydı.",
                    TaskType.CarryToTarget, "yangin_tupu", false),
                Task("L3_T04", "KAÇIŞ YOLUNU AÇ", "Merdiven önündeki kutuları depo alanına taşı; yol tamamen boşalmalı.",
                    "Açık kaçış yolu tahliyeyi hızlandırır.",
                    "Merdivenin önündeki kutuları kenardaki depo alanına götür.",
                    "Kaçış yolu açık! En önemli iş buydu.",
                    "Kaçış yolu engellenmemişti.",
                    TaskType.ClearZone, "exit_clear", true),
            };
            var d = Dialogue("L3_Dialogue", new List<WilsonLine>
            {
                Line("L3_INTRO_01", WilsonTrigger.Intro, WilsonMood.Endiseli, "Üçüncü kat açıldı! Ama evde bu kez yangın riski var."),
                Line("L3_INTRO_02", WilsonTrigger.Intro, WilsonMood.Anlatici, "Duman alarmı bizi erkenden uyarır. Yangını kendi başına söndürmez."),
                Line("L3_TASKS_01", WilsonTrigger.TaskPreview, WilsonMood.Anlatici, "Alarmı yerleştir, ocağı kapat ve yangın tüpünü erişilebilir yere getir."),
                Line("L3_TASKS_02", WilsonTrigger.TaskPreview, WilsonMood.Anlatici, "En önemlisi, kaçış yolunu açık tut."),
                Line("L3_READY", WilsonTrigger.PrepStart, WilsonMood.Heyecanli, "Gıdak! Bu bölümde hızdan önce güvenlik!"),
                Line("L3_TIME", WilsonTrigger.TimeWarning, WilsonMood.Endiseli, "Gıdak! Zaman daralıyor, önce kaçış yolunu aç!"),
                Line("L3_DISASTER", WilsonTrigger.DisasterWarning, WilsonMood.Endiseli, "Duman var! Küçük ve güvenli biçimde kontrol edilemiyorsa yangından uzaklaş ve çık!"),
                Line("L3_REPORT_GOOD", WilsonTrigger.ReportGood, WilsonMood.Sevincli, "Alarm, açık çıkış ve doğru karar hayat kurtarır."),
                Line("L3_REPORT_BAD", WilsonTrigger.ReportBad, WilsonMood.Uzgun, "Yangın bu kez zor oldu. Alarmı ve kaçış yolunu hazırlayıp tekrar deneyelim."),
            });
            Level("L3_Yangin", "L3", 3, "Level3", "Level4", "DUMANIN ARDINDA", "YANGIN", LevelDisaster.Fire, 3, 120f, 30f,
                tasks, d, "Gerçek hayatta: yangın büyükse söndürmeye çalışma, hemen çık ve 112'yi ara. Duman alarmını düzenli test et.",
                "Kontrol edilemeyen yangında tahliye doğru karardır.", "Yangına doğru gitme.");
        }

        // ---------------------------------------------------------------- Bölüm 4
        private static void BuildLevel4()
        {
            var tasks = new[]
            {
                Task("L4_T01", "YILDIRIM KORUMA BAĞLANTISINI TAMAMLA", "İçerideki kontrol noktasında E ile önceden kurulu sistemin son bağlantısını yap.",
                    "Gerçek hayatta paratoner kurulumu profesyonel iştir. Bu sembolik bir oyun görevidir.",
                    "Kontrol noktası üst katta, içeride. Fırtınada çatıya çıkılmaz!",
                    "Bağlantı tamam. Riskleri azalttık!",
                    "Yıldırım koruma sistemi hasar riskini azalttı.",
                    TaskType.ActivatePreInstalledDevice, "lightning_rod_system", true),
                Task("L4_T02", "ELEKTRONİK CİHAZLARIN FİŞİNİ ÇEK", "Televizyonun ve bilgisayarın fişini E ile çek.",
                    "Fırtına öncesinde fişten çekilen cihazlar dalgalanmadan daha az etkilenir.",
                    "TV alt katta, bilgisayar orta katta. İkisinin de fişini çek.",
                    "Fişler çekildi!",
                    "Cihazların fişleri fırtına öncesinde ayrılmıştı.",
                    TaskType.InteractSwitch, "unplugged", true),
                Task("L4_T03", "GERİLİM KORUMASINI KUR", "Gerilim koruyucuyu al, priz yanındaki işaretli alana E ile yerleştir.",
                    "Bazı elektriksel dalgalanmaların etkisini azaltır; doğrudan yıldırım için tam garanti değildir.",
                    "Gerilim koruyucuyu prizin yanındaki işarete götür.",
                    "Gerilim koruması yerinde!",
                    "Gerilim koruması bazı elektriksel etkileri azalttı.",
                    TaskType.CarryToTarget, "surge_protector", false),
            };
            var d = Dialogue("L4_Dialogue", new List<WilsonLine>
            {
                Line("L4_INTRO_01", WilsonTrigger.Intro, WilsonMood.Endiseli, "Gıdak! Gök gürlüyor. Bu kez yıldırım tehlikesi var!"),
                Line("L4_INTRO_02", WilsonTrigger.Intro, WilsonMood.Anlatici, "Elektrik ve yıldırım önlemleri fırtınadan önce alınır."),
                Line("L4_TASKS_01", WilsonTrigger.TaskPreview, WilsonMood.Anlatici, "Önceden tasarlanmış yıldırım koruma bağlantısını tamamla, cihazların fişini güvenliyken çek."),
                Line("L4_TASKS_02", WilsonTrigger.TaskPreview, WilsonMood.Anlatici, "Uygun gerilim koruma cihazını yerleştir. Ama bunun mutlak koruma sağlamadığını unutma."),
                Line("L4_READY", WilsonTrigger.PrepStart, WilsonMood.Heyecanli, "İşimizi şimdi bitirelim; fırtınada elektrikli cihazlara dokunmayalım!"),
                Line("L4_TIME", WilsonTrigger.TimeWarning, WilsonMood.Endiseli, "Gıdak! Gök gürültüsü yaklaşıyor, fişleri unutma!"),
                Line("L4_DISASTER", WilsonTrigger.DisasterWarning, WilsonMood.Endiseli, "Yıldırım! Pencerelerden ve kablolu elektrikli cihazlardan uzak dur!"),
                Line("L4_REPORT_GOOD", WilsonTrigger.ReportGood, WilsonMood.Sevincli, "Önceden hazırlık yapmak riskleri azalttı!"),
                Line("L4_REPORT_BAD", WilsonTrigger.ReportBad, WilsonMood.Uzgun, "Fırtına ağır geldi. Bağlantıyı ve fişleri önceden halledip tekrar deneyelim."),
            });
            Level("L4_Yildirim", "L4", 4, "Level4", "Level5", "GÖKYÜZÜNÜN ÖFKESİ", "YILDIRIM", LevelDisaster.Lightning, 3, 110f, 25f,
                tasks, d, "Gerçek hayatta: fırtınada elektrikli cihazlara ve pencerelere yaklaşma; paratoner kurulumu uzman işidir.",
                "Yıldırım sırasında elektrikli cihaza dokunma.", "Pencerelerden uzak dur.");
        }

        // ---------------------------------------------------------------- Bölüm 5
        private static void BuildLevel5()
        {
            var tasks = new[]
            {
                Task("L5_T01", "KUM TORBALARINI YERLEŞTİR", "Kum torbalarını E ile tek tek al, giriş kapısının önündeki işarete diz.",
                    "Kum torbaları kapı eşiğinden gelen suyu geciktirir; tamamen durdurmaz.",
                    "Torbalar ağır; taşırken dengeyi izle!",
                    "Bariyer hazır!",
                    "Kum torbaları girişte suyun ilerlemesini geciktirdi.",
                    TaskType.PlaceBarrier, "sandbags", true),
                Task("L5_T02", "DRENAJI AÇ", "Tıkalı drenaj ızgarasının yanına git ve E ile temizle.",
                    "Açık drenaj, biriken suyu evden uzaklaştırır.",
                    "Izgara alt katın kenarında. Su basmadan önce temizle.",
                    "Drenaj açıldı, su akıp gidecek!",
                    "Açık drenaj daha fazla suyu uzaklaştırdı.",
                    TaskType.InteractSwitch, "drainage_channel", true),
                Task("L5_T03", "HASSAS EŞYALARI YUKARI TAŞI", "Alt kattaki iki hassas eşyayı üst kattaki kuru depo alanına taşı.",
                    "Yüksekte duran eşyalar su baskınından korunur.",
                    "Eşyaları E ile al, merdivenden (W) çıkıp işaretli alana bırak.",
                    "Eşyalar kuru ve güvende!",
                    "Üst kata taşınan eşyalar sudan korundu.",
                    TaskType.CarryToTarget, "valuables_up", false),
                Task("L5_T04", "SU YALITIMINI KONTROL ET", "Conta kitini al, sızdıran pencerenin yanında E ile uygula.",
                    "Yalıtım sızmayı azaltır; tamamen sıfırlamaz.",
                    "Sızıntı alt kat penceresinde. Kiti oraya götür.",
                    "Sızıntı kapandı!",
                    "Yalıtım onarımı içeri sızan suyu azalttı.",
                    TaskType.InstallEquipment, "waterproof_sealing", false),
            };
            var d = Dialogue("L5_Dialogue", new List<WilsonLine>
            {
                Line("L5_INTRO_01", WilsonTrigger.Intro, WilsonMood.Endiseli, "Gıdak! Dağın üstüne çok şiddetli yağmur geliyor!"),
                Line("L5_INTRO_02", WilsonTrigger.Intro, WilsonMood.Anlatici, "Bu kez sorun dalgalar değil: yamaçtan akan yağmur suyu evin girişine birikiyor."),
                Line("L5_TASKS_01", WilsonTrigger.TaskPreview, WilsonMood.Anlatici, "Kum torbalarını girişe yerleştir ve tıkalı drenajı aç."),
                Line("L5_TASKS_02", WilsonTrigger.TaskPreview, WilsonMood.Anlatici, "Hassas eşyaları güvenli yüksek yere taşı, su yalıtımındaki açıklığı kapat."),
                Line("L5_READY", WilsonTrigger.PrepStart, WilsonMood.Heyecanli, "Üç kat, dört görev, tek denge! Hazırsan son sınav başlıyor."),
                Line("L5_TIME", WilsonTrigger.TimeWarning, WilsonMood.Endiseli, "Gıdak! Yağmur yaklaşıyor, önce bariyeri kur!"),
                Line("L5_DISASTER", WilsonTrigger.DisasterWarning, WilsonMood.Endiseli, "Su yükseliyor! Elektriğe dokunma; güvenli yere geç. Pompa yalnız güvenli ön kurulumla çalışsın!"),
                Line("L5_REPORT_GOOD", WilsonTrigger.ReportGood, WilsonMood.Sevincli, "Başardık! Afete hazırlık hayat kurtarır!"),
                Line("L5_REPORT_BAD", WilsonTrigger.ReportBad, WilsonMood.Uzgun, "Su bu sefer içeri girdi. Bariyeri ve drenajı hazırlayıp tekrar deneyelim."),
                Line("L5_FINAL", WilsonTrigger.Final, WilsonMood.Normal, "Bu evin dengesi hayal ürünü. Gerçek hayatta önlem almak ve doğru davranmak çok önemlidir."),
            });
            Level("L5_Yagmur", "L5", 5, "Level5", "", "SON DENGE", "ŞİDDETLİ YAĞMUR", LevelDisaster.HeavyRain, 3, 140f, 25f,
                tasks, d, "Gerçek hayatta: su basan alanda elektriğe dokunma, yüksek ve güvenli yere geç; resmî uyarıları takip et.",
                "Islak ortamda priz, fiş veya pompaya dokunma.", "Su basan yere girme.");
        }
    }
}
