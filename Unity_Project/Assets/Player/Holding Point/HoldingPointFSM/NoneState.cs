using TimefulDungeon.Items;
using UnityEngine;

namespace TimefulDungeon.Core.HoldingPointFSM {
    public class NoneState : EquippableState {
        public NoneState(HoldingPoint fsm) : base(fsm) {
            Name = EquipType.None;
            transitions.Add(EquipType.Melee, ToMelee);
            transitions.Add(EquipType.Ranged, ToRanged);
            transitions.Add(EquipType.Shield, ToShield);
        }

        public override EquipType Update() {
            if (Input.GetButton("Shield")) return EquipType.Shield;

            return EquipType.None;
        }
    }
}