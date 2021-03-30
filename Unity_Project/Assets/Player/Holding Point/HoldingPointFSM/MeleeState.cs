using TimefulDungeon.Items;
using UnityEngine;

namespace TimefulDungeon.Core.HoldingPointFSM {
    public class MeleeState : EquippableState {
        public MeleeState() {
            Name = EquipType.Melee;
        }

        public override bool CanEnter() {
            return playerEquipment.Melee != null;
        }

        public override EquipType Update() {
            if (Input.GetButtonDown("Swap Weapon")) return EquipType.Ranged;
            if (Input.GetButton("Shield")) return EquipType.Shield;

            return EquipType.Melee;
        }
    }
}