using TimefulDungeon.Items;
using UnityEngine;

namespace TimefulDungeon.Core.HoldingPointFSM {
    public class MeleeState : EquippableState {
        private bool _shouldActivate;
        private bool _shouldShield;

        public MeleeState(HoldingPoint fsm) : base(fsm) {
            Name = EquipType.Melee;
            transitions.Add(EquipType.None, ToNone);
            transitions.Add(EquipType.Ranged, ToRanged);
            transitions.Add(EquipType.Shield, ToShield);
        }

        public override EquipType Update() {
            if (Input.GetButtonDown("Shield")) _shouldShield = true;
            else if (Input.GetButtonUp("Shield")) _shouldShield = false;

            if (Input.GetButtonDown("Swap Weapon")) return EquipType.Ranged;
            if (_shouldShield) return EquipType.Shield;

            return EquipType.Melee;
        }
    }
}