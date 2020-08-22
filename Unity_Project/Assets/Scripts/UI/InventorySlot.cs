using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : ItemUISlot {
	[SerializeField] private Text countUI = null;

	//To write changes back to inventory list
	public int slotNumber;
	private Inventory inventory;

	protected new void Start() {
		base.Start();
		inventory = Inventory.instance;
	}

	public override void SetItem(Item newItem) {
		base.SetItem(newItem);

		//Initialize the count UI if item is stackable
		if (item.stackable) {
			int count = base.item.count;
			countUI.enabled = true;
			if (count > 1000000)
				countUI.text = (count / 1000000) + "M";
			else if (item.count > 1000)
				countUI.text = (count / 1000) + "K";
			else
				countUI.text = count.ToString();
		} else
			countUI.enabled = false;
	}

	public override void UnsetItem() {
		base.UnsetItem();
		countUI.enabled = false;
	}

	protected override void DropOn(ItemUISlot otherSlot) {
		if (otherSlot is InventorySlot) {
			Debug.Log("Dropped inventory item on inventory slot.");
			inventory.Swap(slotNumber, (otherSlot as InventorySlot).slotNumber);
		} else if (otherSlot is EquipmentSlot) {
			Debug.Log("Dropped inventory item on equipment slot.");
			EquipmentSlot otherEquipmentSlot = (otherSlot as EquipmentSlot);
			if (item is Equipment) {
				Equipment equipment = (item as Equipment);
				if (equipment.type == otherEquipmentSlot.equipSlotType) {
					EquipmentManager.instance.Equip(equipment);
				}
			}
		}
	}

	protected override void DiscardItem() {
		base.DiscardItem();
		item.RemoveFromInventory();
	}
}