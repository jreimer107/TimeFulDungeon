using TimefulDungeon.Items;
using UnityEngine;

namespace TimefulDungeon.Core.HoldingPointFSM {
    public class NoneState : EquippableState {
        public NoneState() {
            Name = EquipType.None;
            AddTransition(EquipType.Melee, ToMelee);
            AddTransition(EquipType.Ranged, ToRanged);
            AddTransition(EquipType.Shield, ToShield);
        }

        public override EquipType Update() {
            if (Input.GetButton("Shield")) return EquipType.Shield;

            return EquipType.None;
        }
    }
}