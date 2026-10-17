using UnityEngine;

namespace Jam.Levels.Level1
{
    public sealed partial class L1_ProjectGameController
    {
        private L1_WilsonChicken wilson;

        private void SpawnWilson()
        {
            if (houseRoot == null || wilson != null) return;
            var companion = new GameObject("Wilson — evcil tavuk");
            companion.transform.SetParent(houseRoot, false);
            wilson = companion.AddComponent<L1_WilsonChicken>();
            wilson.Initialize(this, houseRoot, resident);
        }

        public void WilsonEggCollected(bool golden)
        {
            if (golden)
            {
                budget += 100;
                Warn("Wilson'un altın yumurtası! +100 altın");
                if (sfx != null) sfx.Play("purchase");
            }
            else Warn("Wilson'un yumurtası toplandı; 5 kg yük kalktı.");
        }
    }
}
