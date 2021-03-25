using System.IO;
using UnityEngine;

namespace TimefulDungeon.Items {
    public class Shield : Equippable {
        public readonly int staminaUse;
        public readonly float arc;

        public Shield(ShieldTemplate template) : base(template) {
            staminaUse = template.staminaUse;
            arc = template.arc;
        }

        public Shield Clone() {
            return (Shield) MemberwiseClone();
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
    }
    
}