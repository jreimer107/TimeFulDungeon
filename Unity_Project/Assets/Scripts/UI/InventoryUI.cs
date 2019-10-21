using UnityEngine;

public class InventoryUI : MonoBehaviour {
	[SerializeField] private GameObject inventoryUI = null;
	[SerializeField] private Transform BagUI = null;
	[SerializeField] private GameObject slotUI = null;

	private Inventory inventory;
	private InventorySlot[] slots;

	void Start() {
		inventory = Inventory.instance;
		inventory.onItemChangedCallback += UpdateUI;
		// slots = BagUI.GetComponentsInChildren<InventorySlot>();

		slots = new InventorySlot[inventory.enabledSlots];
		for (int i = 0; i < inventory.enabledSlots; i++) {
			GameObject newSlot = Instantiate(slotUI, new Vector3(0, 0, 0), Quaternion.identity);
			newSlot.transform.SetParent(BagUI, false);
			slots[i] = newSlot.GetComponent<InventorySlot>();
		}
		inventoryUI.SetActive(false);
	}

	void Update() {
		if (Input.GetButtonDown("Inventory")) {
			inventoryUI.SetActive(!inventoryUI.activeSelf);
		}
	}

	private void UpdateUI() {
		for (int i = 0; i < slots.Length; i++) {
			if (i < inventory.Bag.Count) {
				slots[i].SetItem(inventory.Bag[i]);
			} else {
				slots[i].UnsetItem();
			}
		}
	}
}