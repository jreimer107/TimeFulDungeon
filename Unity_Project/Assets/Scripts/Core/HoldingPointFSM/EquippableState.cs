using TimefulDungeon.Items;

namespace TimefulDungeon.Core.HoldingPointFSM {
    public abstract class EquippableState : State<EquipType> {
        private readonly Inventory _playerEquipment;
        protected readonly HoldingPoint fsm;
        protected readonly Stamina playerStamina;

        protected EquippableState(HoldingPoint fsm) {
            this.fsm = fsm;
            var player = Player.instance;
            _playerEquipment = player.Inventory;
            playerStamina = player.Stamina;
        }

        protected static bool ToNone() {
            return true;
        }

        protected bool ToMelee() {
            return _playerEquipment.Melee;
        }

        protected bool ToRanged() {
            return _playerEquipment.Ranged;
        }

        protected bool ToShield() {
            return !playerStamina.Exhausted && _playerEquipment.Shield;
        }
    }
}