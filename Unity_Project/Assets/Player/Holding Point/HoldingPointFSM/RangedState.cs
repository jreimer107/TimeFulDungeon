using TimefulDungeon.Items;
using UnityEngine;

namespace TimefulDungeon.Core.HoldingPointFSM {
    public class RangedState : EquippableState {
        public RangedState(HoldingPoint fsm) : base(fsm) {
            Name = EquipType.Ranged;
            transitions.Add(EquipType.None, ToNone);
            transitions.Add(EquipType.Melee, ToMelee);
            transitions.Add(EquipType.Shield, ToShield);
        }

        public override EquipType Update() {
            if (Input.GetButtonDown("Swap Weapon")) return EquipType.Melee;
            if (Input.GetButton("Shield") && !fsm.ControlledByInHand) return EquipType.Shield;

            return EquipType.Ranged;
        }
    }
}