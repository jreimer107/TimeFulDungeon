using TimefulDungeon.Items;
using UnityEngine;

namespace TimefulDungeon.Core.HoldingPointFSM {
    public class ShieldState : EquippableState {
        private EquipType _previousType;
        private bool _shouldShield;

        public ShieldState(HoldingPoint fsm) : base(fsm) {
            Name = EquipType.Shield;
            transitions.Add(EquipType.None, ToNone);
            transitions.Add(EquipType.Melee, ToMelee);
            transitions.Add(EquipType.Ranged, ToRanged);
        }

        public override void Start() {
            _previousType = fsm.CurrType;
        }

        public override EquipType Update() {
            if (Input.GetButtonUp("Shield") || playerStamina.Exhausted) return _previousType;

            return EquipType.Shield;
        }
    }
}