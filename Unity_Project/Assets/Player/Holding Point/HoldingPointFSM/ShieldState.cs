using TimefulDungeon.Items;
using UnityEngine;

namespace TimefulDungeon.Core.HoldingPointFSM {
    public class ShieldState : EquippableState {
        private EquipType _previousType;
        private Shield _shield;

        public ShieldState() {
            Name = EquipType.Shield;
            AddTransition(EquipType.None, ToNone);
            AddTransition(EquipType.Melee, ToMelee);
            AddTransition(EquipType.Ranged, ToRanged);
        }

        public override void Start() {
            if (holdingPoint.CurrType != EquipType.Shield) {
                _previousType = holdingPoint.CurrType;
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