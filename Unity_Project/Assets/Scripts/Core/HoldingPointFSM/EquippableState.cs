using TimefulDungeon.Items;

namespace TimefulDungeon.Core.HoldingPointFSM {
    public abstract class EquippableState : State<EquipType> {
        protected readonly Inventory playerEquipment;
        protected readonly HoldingPoint fsm;
        protected readonly Stamina playerStamina;

        protected EquippableState(HoldingPoint fsm) {
            this.fsm = fsm;
            var player = Player.instance;
            playerEquipment = player.Inventory;
            playerStamina = player.Stamina;
        }

        protected static bool ToNone() {
            return true;
        }

        protected bool ToMelee() {
            return playerEquipment.Melee != null;
        }

        protected bool ToRanged() {
            return playerEquipment.Ranged != null;
        }

        protected bool ToShield() {
            return !playerStamina.Exhausted && playerEquipment.Shield != null;
        }
    }
}