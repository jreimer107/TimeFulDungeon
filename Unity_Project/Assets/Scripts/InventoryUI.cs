using UnityEngine;

public class InventoryUI : MonoBehaviour {
	public Transform BagUI;
	private Inventory inventory;
	private InventorySlot[] slots;

	void Start() {
		inventory = Inventory.instance;
		inventory.onItemChangedCallback += UpdateUI;
		slots = BagUI.GetComponentsInChildren<InventorySlot>();
	}

	void UpdateUI() {
		for (int i = 0; i < slots.Length; i++) {
			if (i < inventory.Bag.Count) {
				slots[i].AddItem(inventory.Bag[i]);
			} else {
				slots[i].RemoveItem();
			}
		}
	}
}