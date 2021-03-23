using TimefulDungeon.Core;
using UnityEngine;

namespace TimefulDungeon.Items {
    public class Equippable : Item {
        public EquipType type;
        
        public override void Select() { }

        public override void Use() {
            base.Use();
            Player.instance.Inventory.Equip(this);
        }
    }
}