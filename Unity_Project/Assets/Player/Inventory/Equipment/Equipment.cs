using System;
using TimefulDungeon.Items;
using TimefulDungeon.Items.Melee;
using UnityEngine;
using UnityEngine.Events;

namespace TimefulDungeon.Core {
    [Serializable]
    public class Equipment {
        [Serializable]
        private class EquipmentChangeEvent : UnityEvent<EquipType> { }

        [SerializeField] private Equippable[] currentEquipment;
        [SerializeField] private EquipmentChangeEvent onEquipmentChanged;
        
        public Melee Melee => currentEquipment[(int) EquipType.Melee] as Melee;
        public Ranged Ranged => currentEquipment[(int) EquipType.Ranged] as Ranged;
        public Shield Shield => currentEquipment[(int) EquipType.Shield] as Shield;

        public Equipment() {
            var numSlots = Enum.GetNames(typeof(EquipType)).Length;
            currentEquipment = new Equippable[numSlots];
        }

        /// <summary>
        ///     Assigns equipment item to correct slot. If there was already something
        ///     equipped, puts that back in the inventory.
        /// </summary>
        /// <param name="newEquip">The new item to equip.</param>
        public Equippable Equip(Equippable newEquip) {
            var slotIndex = (int) newEquip.type;
            var currentEquip = currentEquipment[slotIndex];

            //Equip new item
            currentEquipment[slotIndex] = newEquip;

            //Invoke callback function
            onEquipmentChanged.Invoke((EquipType) slotIndex);
            return currentEquip;
        }

        /// <summary>
        ///     Unequips an item and deletes it.
        /// </summary>
        /// <param name="type">What type of equipment to unequip.</param>
        public Equippable Unequip(EquipType type) {
            var index = (int) type;
            var unequipped = currentEquipment[index];
            currentEquipment[index] = null;
            onEquipmentChanged.Invoke(type);
            return unequipped;
        }

        public Equippable GetEquipment(int index) {
            return index < 0 ? null : currentEquipment[index];
        }

        public Equippable GetEquipment(EquipType type) {
            return type == EquipType.None ? null : currentEquipment[(int) type];
        }
    }
}