using TimefulDungeon.Items;
using UnityEngine;

namespace TimefulDungeon.Core.HoldingPointFSM {
    public class MeleeState : EquippableState {
        public MeleeState() {
            Name = EquipType.Melee;
            transitions.Add(EquipType.None, ToNone);
            transitions.Add(EquipType.Ranged, ToRanged);
            transitions.Add(EquipType.Shield, ToShield);
        }

        public override EquipType Update() {
            if (Input.GetButtonDown("Swap Weapon")) return EquipType.Ranged;
            if (Input.GetButton("Shield")) return EquipType.Shield;

            return EquipType.Melee;
        }
    }
}