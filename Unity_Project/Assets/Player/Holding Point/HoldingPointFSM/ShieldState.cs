using TimefulDungeon.Items;
using TMPro;
using UnityEngine;

namespace TimefulDungeon.Core.HoldingPointFSM {
    public class ShieldState : EquippableState {
        private EquipType _previousType;
        private Shield _shield;

        public ShieldState(HoldingPoint fsm) : base(fsm) {
            Name = EquipType.Shield;
            transitions.Add(EquipType.None, ToNone);
            transitions.Add(EquipType.Melee, ToMelee);
            transitions.Add(EquipType.Ranged, ToRanged);
        }

        public override void Start() {
            if (fsm.CurrType != EquipType.Shield) {
                _previousType = fsm.CurrType;
            }
            _shield = playerEquipment.Shield;
            playerStamina.StartContinuousUse(_shield.staminaUse);
        }

        public override void Exit() {
            playerStamina.StopContinuousUse(_shield.staminaUse);
        }

        public override EquipType Update() {
            if (!Input.GetButton("Shield") || playerStamina.Exhausted) return _previousType;

            return EquipType.Shield;
        }
    }
}