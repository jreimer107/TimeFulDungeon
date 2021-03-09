using TimefulDungeon.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TimefulDungeon.UI {
    public class InventorySlot : ItemUISlot {
        [SerializeField] private Text countUI;

        //To write changes back to inventory list
        public int slotNumber;
        private Inventory inventory;

        protected override void Start() {
            base.Start();
            inventory = Player.instance.Inventory;
            Refresh();
        }

        public override void Refresh() {
            item = inventory.GetItem(slotNumber);

            base.Refresh();

            //Initialize the count UI if item is stackable
            if (item && item.stackable) {
                var count = item.count;
                countUI.enabled = true;
                if (count > 1000000)
                    countUI.text = count / 1000000 + "M";
                else if (item.count > 1000)
                    countUI.text = count / 1000 + "K";
                else
                    countUI.text = count.ToString();
            }
            else {
                countUI.enabled = false;
            }
        }

        protected override void DropOn(ItemUISlot otherSlot) {
            switch (otherSlot) {
                case InventorySlot slot:
                    Debug.Log("Dropped inventory item on inventory slot.");
                    inventory.Swap(slotNumber, slot.slotNumber);
                    break;
                case EquipmentSlot slot: {
                    Debug.Log("Dropped inventory item on equipment slot.");
                    var otherEquipmentSlot = slot;
                    if (item is Equippable { } equippable && equippable.type == otherEquipmentSlot.type)
                        inventory.Equip(equippable);

                    break;
                }
            }
        }

        protected override void DiscardItem() {
            base.DiscardItem();
            inventory.RemoveAt(slotNumber);
        }
    }
}