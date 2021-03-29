using TimefulDungeon.Items;
using UnityEngine;

namespace TimefulDungeon.Core.HoldingPointFSM {
    public class MeleeState : EquippableState {
        public MeleeState() {
            Name = EquipType.Melee;
            AddTransition(EquipType.None, ToNone); 
            AddTransition(EquipType.Ranged, ToRanged);
            AddTransition(EquipType.Shield, ToShield);
        }

        public override EquipType Update() {
            if (Input.GetButtonDown("Swap Weapon")) return EquipType.Ranged;
            if (Input.GetButton("Shield")) return EquipType.Shield;

            return EquipType.Melee;
        }
    }
}