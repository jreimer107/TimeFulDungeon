using TimefulDungeon.Core;
using TimefulDungeon.Items;
using UnityEngine;

namespace TimefulDungeon.UI {
    public class EquipmentSlot : ItemUISlot {
        //To know what type of item goes here.
        public EquipType type;
        private Inventory equipmentManager;

        private Equippable Equippable => item as Equippable;

        protected new void Start() {
            base.Start();
            equipmentManager = Player.instance.Inventory;
            Refresh();
        }

        private void Unequip() {
            equipmentManager.Unequip(Equippable.type);
        }

        protected override void Use() {
            Unequip();
        }

        public override void Refresh() {
            item = equipmentManager.GetEquipment(type);
            base.Refresh();
        }

        protected override void DropOn(ItemUISlot otherSlot) {
            if (!(otherSlot is InventorySlot)) return;
            Debug.Log("Dropped equipment item on inventory slot.");
            if (!otherSlot.IsEmpty) {
                if (otherSlot.item is Equippable { } otherEquippable && Equippable.type == otherEquippable.type)
                    equipmentManager.Equip(otherEquippable);
            }
            else {
                Unequip();
            }
        }

        protected override void DiscardItem() {
            base.DiscardItem();
            equipmentManager.Unequip(type, true);
        }
    }
}