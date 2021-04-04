using System;
using System.IO;
using TimefulDungeon.Misc;
using UnityEngine;

namespace TimefulDungeon.Items.Shield {
    public class Shield : Equippable {
        public readonly float staminaUseMod;
        public readonly float arcMod;
        
        
        [NonSerialized] public readonly int staminaUse;
        protected readonly float arc;

        public Shield(ShieldTemplate template) : base(template) {
            staminaUseMod = GetModifier();
            arcMod = GetModifier();
            prefix = Utils.GetRandomEnum<ShieldPrefixes>();
            
            staminaUse = (int)(template.staminaUse * staminaUseMod);
            arc = template.arc * arcMod;
        }

        public override void Activate() {
            base.Activate();
            
            SaveTest();
            
        }

        private void SaveTest() {
            name = "garbage";
            var path = Application.persistentDataPath + "/shieldTest.dat";
            File.WriteAllText(path, JsonUtility.ToJson(this));
            name = "";
            Debug.Log("Wrote file to " + path);
            JsonUtility.FromJsonOverwrite(File.ReadAllText(path), this);
        }

        protected override string CalculateTooltipText() {
            return
                GetNameLevelDescription() +
                $"{FormatFloat(arc)}\u00b0 guard\n" +
                $"{staminaUse} stamina/second" +
                GetFormattedRedText();
        }
    }
    
}