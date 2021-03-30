using TimefulDungeon.Items;
using UnityEngine;

namespace TimefulDungeon.Core.HoldingPointFSM {
    public class RangedState : EquippableState {
        public RangedState() {
            Name = EquipType.Ranged;
        }

        public override bool CanEnter() {
            return playerEquipment.Ranged != null;
        }

        public override EquipType Update() {
            if (Input.GetButtonDown("Swap Weapon")) return EquipType.Melee;
            if (Input.GetButton("Shield") && !holdingPoint.ControlledByInHand) return EquipType.Shield;

            return EquipType.Ranged;
        }
    }
}