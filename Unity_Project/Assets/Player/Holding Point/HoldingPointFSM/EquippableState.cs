using TimefulDungeon.Items;

namespace TimefulDungeon.Core.HoldingPointFSM {
    public abstract class EquippableState : State<EquipType> {
        protected readonly Inventory playerEquipment;
        protected readonly HoldingPoint holdingPoint;
        protected readonly Stamina playerStamina;

        protected EquippableState() {
            var player = Player.instance;
            playerEquipment = player.Inventory;
            playerStamina = player.Stamina;
            holdingPoint = player.HoldingPoint;
        }
    }
}