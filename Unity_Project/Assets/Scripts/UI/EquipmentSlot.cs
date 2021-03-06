using TimefulDungeon.Core;
using UnityEngine;

namespace TimefulDungeon.UI {
	public class EquipmentSlot : ItemUISlot {
		private EquipmentManager equipmentManager;

		//To know what type of item goes here.
		public EquipType equipSlotType;

		public Equipment equipment {
			get {
				return (base.item as Equipment);
			}
		}

		protected new void Start() {
			base.Start();
			equipmentManager = EquipmentManager.instance;
		}

		private void Unequip() => equipmentManager.Unequip((int)equipment.type);

		protected override void Use() {
			Unequip();
		}

		protected override void DropOn(ItemUISlot otherslot) {
			if (otherslot is InventorySlot) {
				Debug.Log("Dropped equipment item on inventory slot.");
				if (!otherslot.isEmpty) {
					if (otherslot.item is Equipment) {
						Equipment otherEquipment = (otherslot.item as Equipment);
						if (equipment.type == otherEquipment.type) {
							equipmentManager.Equip(otherEquipment);
						}
					}
				} else {
					Unequip();
				}
			}
		}

		protected override void DiscardItem() {
			base.DiscardItem();
			equipmentManager.DeleteEquipped((int)equipment.type);
		}

	}
}