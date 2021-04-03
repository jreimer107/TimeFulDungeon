using TimefulDungeon.Items;
using TimefulDungeon.Items.Melee;
using UnityEngine;

namespace TimefulDungeon.Core {
    public class Inventory : MonoBehaviour{
        [SerializeField] private Equipment equipment;
        [SerializeField] private Backpack backpack;

        public bool AddItem(Item item) {
            if (!(item is Equippable equippable) || equipment.GetEquipment(equippable.type) == null) return backpack.Add(item);
            Equip(equippable);
            return true;
        }

        public bool CanAdd(Item item) => backpack.CanAdd(item);
        public bool Remove(Item item) => backpack.Remove(item);
        public bool RemoveAt(int index) => backpack.RemoveAt(index);
        public void Swap(int newItem, int oldItem) => backpack.Swap(newItem, oldItem);
        public Item GetItem(int index) => backpack.GetItem(index);
        public int Count => backpack.Count;
        public int EnabledSlots => backpack.enabledSlots;

        // TODO: Use inventory index and take from there
        public void Equip(Equippable newEquip) {
            var oldEquip = equipment.Equip(newEquip);
            if (oldEquip) {
                backpack.SwapOut(oldEquip, newEquip);
            }
            else {
                Remove(newEquip);
            }
        }

        public void Unequip(EquipType type, bool drop = false) {
            var unequipped = equipment.Unequip(type);
            if (!drop) {
                backpack.Add(unequipped);
            }
        }

        public Melee Melee => equipment.Melee;
        public Ranged Ranged => equipment.Ranged;
        public Shield Shield => equipment.Shield;
        public Equippable GetEquipment(EquipType type) => equipment.GetEquipment(type);
        public Equippable GetEquipment(int index) => equipment.GetEquipment(index);
    }
}