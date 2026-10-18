using UnityEngine;

namespace Jam.Levels.Level1
{
    /// <summary>
    /// E ile kullanılan dünya içi görev noktası (pencere kapat, ocağı kapat, fişi çek, drenajı aç...).
    /// Karakter aynı katta ve yakınken çalışır. Görsel durum, açık/kapalı nesneler ve kısa
    /// bir dönüş animasyonuyla gösterilir. Afet başlayınca kilitlenir (gerçek güvenlik kuralı).
    /// </summary>
    public sealed class L1_MissionSwitch : MonoBehaviour
    {
        public string switchId;
        [Tooltip("Oyuncuya gösterilen eylem, ör. 'PENCEREYİ KAPAT'.")]
        public string actionTr = "KULLAN";
        [Tooltip("Etkinleştirildikten sonra gösterilen kısa durum, ör. 'Kapalı'.")]
        public string doneTr = "Tamam";
        public float reach = 1.1f;
        [Tooltip("Kapalıysa yalnız işarettir (ör. afette kaçınılacak pencere); E ile kullanılamaz.")]
        public bool interactable = true;
        [Tooltip("Etkinleşince görünür olur (ör. kapalı pencere kanadı, takılı bağlantı).")]
        public GameObject onVisual;
        [Tooltip("Etkinleşince gizlenir (ör. açık kanat, tıkanıklık).")]
        public GameObject offVisual;
        [Tooltip("Bu parça etkinleşince yerel Z ekseninde döner (ör. kanat menteşesi).")]
        public Transform rotatePart;
        public float rotateDegrees = 0f;
        [Tooltip("Afette pencere/elektrik gibi kaçınılması gereken bir tehlike noktası mı?")]
        public bool hazardDuringDisaster;
        [Tooltip("Boşsa her zaman kullanılabilir; doluysa bu alanın içinde ilgili eşya olmalı (montaj).")]
        public L1_MissionZone requiresItemInZone;
        public L1_MovableProp requiredItem;
        [Tooltip("Takılınca taşınan eşya gizlenir; yerine 'onVisual' içindeki monte edilmiş görsel görünür.")]
        public bool consumeItem;
        [Tooltip("Bu nokta ancak bu diğer nokta etkinleştikten sonra kullanılabilir (ör. önce kur, sonra test et).")]
        public L1_MissionSwitch requiresSwitch;

        public bool Activated { get; private set; }
        public bool Locked { get; set; }
        public event System.Action<L1_MissionSwitch> Changed;

        private float animT = 1f;
        private Quaternion rotateFrom, rotateTo;

        private void Awake()
        {
            if (rotatePart != null)
            {
                rotateFrom = rotatePart.localRotation;
                rotateTo = rotateFrom * Quaternion.Euler(0f, 0f, rotateDegrees);
            }
            ApplyVisual(true);
        }

        public bool PrerequisiteMet
        {
            get
            {
                if (requiresSwitch != null && !requiresSwitch.Activated) return false;
                if (requiresItemInZone == null || requiredItem == null) return true;
                return requiredItem.gameObject.activeInHierarchy && requiredItem.Body != null &&
                       !requiredItem.Body.isKinematic && requiresItemInZone.Contains(requiredItem.Body.position);
            }
        }

        public bool InReach(Transform houseRoot, L1_ResidentController resident)
        {
            if (resident == null || houseRoot == null || !isActiveAndEnabled) return false;
            var player = houseRoot.InverseTransformPoint(resident.transform.position);
            var me = houseRoot.InverseTransformPoint(transform.position);
            var feet = player.y - resident.footOffset;
            // Aynı kat: anahtar oyuncunun ayak hizasıyla en fazla 2,1 m yukarıda olmalı.
            return Mathf.Abs(player.x - me.x) <= reach && me.y > feet - 0.4f && me.y < feet + 2.1f;
        }

        /// <summary>Etkinleştirir. Kilitliyse, zaten etkinse veya montaj ön koşulu yoksa false döner.</summary>
        public bool TryActivate()
        {
            if (!interactable || Activated || Locked || !PrerequisiteMet) return false;
            Activated = true;
            animT = 0f;
            if (consumeItem && requiredItem != null) requiredItem.gameObject.SetActive(false);
            ApplyVisual(false);
            Changed?.Invoke(this);
            return true;
        }

        public void ResetSwitch()
        {
            Activated = false;
            animT = 1f;
            if (rotatePart != null) rotatePart.localRotation = rotateFrom;
            ApplyVisual(true);
        }

        private void ApplyVisual(bool instant)
        {
            if (onVisual != null) onVisual.SetActive(Activated);
            if (offVisual != null) offVisual.SetActive(!Activated);
            if (instant && rotatePart != null) rotatePart.localRotation = Activated ? rotateTo : rotateFrom;
        }

        private void Update()
        {
            if (rotatePart == null || animT >= 1f) return;
            animT = Mathf.MoveTowards(animT, 1f, Time.deltaTime * 2.5f);
            var e = 1f - (1f - animT) * (1f - animT);
            rotatePart.localRotation = Quaternion.Slerp(rotateFrom, rotateTo, e);
        }
    }
}
