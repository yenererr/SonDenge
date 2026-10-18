using UnityEngine;

namespace Jam.Levels.Level1
{
    public sealed partial class L1_ProjectGameController
    {
        private L1_WilsonChicken wilson;
        [Header("Wilson (CC BY 4.0 — ThirdParty/Wilson/CREDITS.md)")]
        public GameObject wilsonModel;
        public GameObject wilsonEggModel;
        public Texture2D wilsonTexture;

        private void SpawnWilson()
        {
            if (houseRoot == null || wilson != null) return;
            var companion = new GameObject("Wilson — evcil tavuk");
            companion.transform.SetParent(houseRoot, false);
            wilson = companion.AddComponent<L1_WilsonChicken>();
            wilson.Initialize(this, houseRoot, resident, wilsonModel, wilsonEggModel, wilsonTexture);
        }

        public void WilsonEggCollected(bool golden)
        {
            // Para ödülü yok (şartname §3.5): yumurta yalnız 5 kg denge yüküdür.
            Warn(golden ? "Wilson'un parlak yumurtası! Wilson çok sevindi; 5 kg yük kalktı."
                : "Wilson'un yumurtası toplandı; 5 kg yük kalktı.");
            if (golden && sfx != null) sfx.Play("confirm");
        }
    }
}
