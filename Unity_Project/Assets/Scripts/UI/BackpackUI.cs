using TimefulDungeon.Core;
using UnityEngine;

namespace TimefulDungeon.UI {
    public class BackpackUI : MonoBehaviour {
        // Resources
        private static InventorySlot slotPrefab;
        
        // Dependencies
        private Inventory inventory;
        
        // Instance fields
        private InventorySlot[] slots;

        private void Start() {
            inventory = Player.instance.Inventory;
            slotPrefab ??= Resources.Load<InventorySlot>("Prefabs/Slot");

            // Create slots, count based on player's capacity
            slots = new InventorySlot[inventory.EnabledSlots];
            for (var i = 0; i < inventory.EnabledSlots; i++) {
                var newSlot = Instantiate(slotPrefab, transform);
                newSlot.slotNumber = i;
                slots[i] = newSlot;
            }
        }

        public void UpdateUI(int slotIndex) {
            slots?[slotIndex]?.Refresh();
        }
    }
}