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

        private const string EggTipKey = "sondenge.v1.eggTipSeen";
        private bool eggTipOpen;

        /// <summary>Bölüm 1'de Wilson ilk kez yumurtlayınca oyun durur ve yumurtaları anlatan kısa pencere açılır (bir kez).</summary>
        public bool IsEggTipOpen => eggTipOpen;

        public void WilsonEggLaid(bool golden)
        {
            if (sfx != null) sfx.PlayClip("cluck", 0.7f, 0.1f);
            if (level != 1 || state != RoundState.Planning || PlayerPrefs.GetInt(EggTipKey, 0) == 1) return;
            PlayerPrefs.SetInt(EggTipKey, 1);
            PlayerPrefs.Save();
            eggTipOpen = true;
            SyncPause();
            if (sfx != null) sfx.Play("confirm");
        }

        public void UiCloseEggTip()
        {
            eggTipOpen = false;
            SyncPause();
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
