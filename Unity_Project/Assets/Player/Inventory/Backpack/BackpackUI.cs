using TimefulDungeon.Core;
using UnityEngine;

namespace TimefulDungeon.UI {
    public class BackpackUI : MonoBehaviour {
        // Resources
        private static InventorySlot slotPrefab;
        
        // Dependencies
        private Inventory _inventory;
        
        // Instance fields
        private InventorySlot[] _slots;

        private void Start() {
            _inventory = Player.instance.Inventory;
            slotPrefab ??= Resources.Load<InventorySlot>("Slot");

            // Create slots, count based on player's capacity
            _slots = new InventorySlot[_inventory.EnabledSlots];
            for (var i = 0; i < _inventory.EnabledSlots; i++) {
                var newSlot = Instantiate(slotPrefab, transform);
                newSlot.slotNumber = i;
                _slots[i] = newSlot;
            }
        }

        public void UpdateUI(int slotIndex) {
            _slots?[slotIndex]?.Refresh();
        }
    }
}