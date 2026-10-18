using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jam.Missions
{
    [Serializable]
    public sealed class WilsonLine
    {
        public string id;
        public WilsonTrigger trigger;
        public WilsonMood mood;
        [TextArea(2, 6)] public string textTr;
        [Tooltip("Opsiyonel gıdak sesi.")]
        public AudioClip cluck;
    }

    /// <summary>Bir bölümün bütün Wilson cümleleri. Metinler koda gömülmez, bu asset'ten okunur.</summary>
    [CreateAssetMenu(menuName = "SonDenge/Wilson Dialogue Set", fileName = "L1_Dialogue")]
    public sealed class WilsonDialogueSet : ScriptableObject
    {
        public string speaker = "WILSON";
        public List<WilsonLine> lines = new List<WilsonLine>();

        public List<WilsonLine> For(WilsonTrigger trigger)
        {
            var result = new List<WilsonLine>();
            foreach (var line in lines) if (line != null && line.trigger == trigger) result.Add(line);
            return result;
        }

        public WilsonLine First(WilsonTrigger trigger)
        {
            foreach (var line in lines) if (line != null && line.trigger == trigger) return line;
            return null;
        }
    }
}
