using UnityEngine;
public class EquipmentSlot : ItemUISlot {
	private EquipmentManager equipmentManager;
	private Inventory inventory;

	//To know what type of item goes here.
	public EquipType equipSlotType;

	public Equipment equipment {
		get {
			return (base.item as Equipment);
		}
	}

	void Start() {
		equipmentManager = EquipmentManager.instance;
		inventory = Inventory.instance;
	}

	public void EquipItem(Equipment newEquip) {
		if (newEquip == null)
			return;

		base.SetItem(newEquip);
	}

	public void UnequipItem() {
		equipmentManager.Unequip((int)equipment.type);
		base.UnsetItem();
	}

	public override void Use() {
		UnequipItem();
	}

	public override void DropOn(ItemUISlot otherslot) {
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
				UnequipItem();
			}
		}
	}

}