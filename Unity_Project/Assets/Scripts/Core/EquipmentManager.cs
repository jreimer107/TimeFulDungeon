using System;
using UnityEngine;

namespace TimefulDungeon.Core {
    public class EquipmentManager : MonoBehaviour {
        public delegate void OnEquipmentChanged();

        public Melee testMelee;
        private Equipment[] currentEquipment;
        private Inventory inventory;
        public OnEquipmentChanged onEquipmentChangedCallback;

        public Melee Melee => currentEquipment[(int) EquipType.Melee] as Melee;
        public Ranged Ranged => currentEquipment[(int) EquipType.Ranged] as Ranged;
        public Shield Shield => currentEquipment[(int) EquipType.Shield] as Shield;

        #region Singleton

        public static EquipmentManager instance;
        private void Awake() {
            if (instance != null) Debug.LogWarning("More than one instance of Equipment Manager found.");
            instance = this;
        }
        #endregion
        
        private void Start() {
            inventory = Inventory.instance;

            var numSlots = Enum.GetNames(typeof(EquipType)).Length;
            currentEquipment = new Equipment[numSlots];

            if (testMelee) Equip(testMelee);
        }

        /// <summary>
        ///     Assigns equipment item to correct slot. If there was already something
        ///     equipped, puts that back in the inventory.
        /// </summary>
        /// <param name="newEquip">The new item to equip.</param>
        public void Equip(Equipment newEquip) {
            var slotIndex = (int) newEquip.type;
            var currentEquip = currentEquipment[slotIndex];
            Debug.Log($"Equipping {newEquip.name}");

            //Add currently equipped item back into inventory
            if (currentEquipment[slotIndex]) //The new equip came from inventory, so swap it out.
                inventory.SwapOut(currentEquip, newEquip);
            else
                newEquip.RemoveFromInventory();

            //Equip new item
            currentEquipment[slotIndex] = newEquip;

            //Invoke callback function
            onEquipmentChangedCallback?.Invoke();
        }

        /// <summary>
        ///     Unequips an item and deletes it.
        /// </summary>
        /// <param name="slotIndex">EquipType index of item to delete.</param>
        public void DeleteEquipped(int slotIndex) {
            currentEquipment[slotIndex] = null;
            onEquipmentChangedCallback?.Invoke();
        }

        /// <summary>
        ///     Unequips an item and adds it back into the inventory.
        /// </summary>
        /// <param name="slotIndex">EquipType index of item to unequip.</param>
        public void Unequip(int slotIndex) {
            var unequipped = currentEquipment[slotIndex];
            if (unequipped == null) return;
            if (inventory.Add(unequipped)) DeleteEquipped(slotIndex);
        }

        public Equipment GetEquipment(int index) {
            return currentEquipment[index];
        }

        public Equipment GetEquipment(EquipType type) {
            return currentEquipment[(int) type];
        }
    }
}