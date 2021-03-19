using TimefulDungeon.Items;
using UnityEngine;

namespace TimefulDungeon.Core.HoldingPointFSM {
    public class RangedState : EquippableState {
        private bool _shouldActivate;
        private bool _shouldShield;

        public RangedState(HoldingPoint fsm) : base(fsm) {
            Name = EquipType.Ranged;
            transitions.Add(EquipType.None, ToNone);
            transitions.Add(EquipType.Melee, ToMelee);
            transitions.Add(EquipType.Shield, ToShield);
        }

        public override EquipType Update() {
            if (Input.GetButtonDown("Shield")) _shouldShield = true;
            else if (Input.GetButtonUp("Shield")) _shouldShield = false;

            if (Input.GetButtonDown("Swap Weapon")) return EquipType.Melee;
            if (_shouldShield && !fsm.ControlledByInHand) return EquipType.Shield;

            return EquipType.Ranged;
        }
    }
}