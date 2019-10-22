using UnityEngine;

public class EquipmentManager : MonoBehaviour {
	private Inventory inventory;
	public Equipment[] currentEquipment;

	public delegate void OnEquipmentChanged();
	public OnEquipmentChanged onEquipmentChangedCallback;

	#region Singleton
	public static EquipmentManager instance;
	void Awake() {
		if (instance != null) {
			Debug.LogWarning("More than one instance of Equipment Manager found.");
		}
		instance = this;
	}
	#endregion

	void Start() {
		inventory = Inventory.instance;

		int numSlots = System.Enum.GetNames(typeof(EquipType)).Length;
		currentEquipment = new Equipment[numSlots];
	}

	// Assigns equipment item to correct slot.
	//If there was already something equipped, puts that back in the inventory.
	public void Equip(Equipment newEquip) {
		int slotIndex = (int)newEquip.type;

		//Add currently equipped item back into inventory
		if (currentEquipment[slotIndex] != null) {
			inventory.Add(currentEquipment[slotIndex]);
		}

		//Equip new item
		currentEquipment[slotIndex] = newEquip;

		//Invoke callback function
		if (onEquipmentChangedCallback != null) {
			onEquipmentChangedCallback.Invoke();
		}
	}

	public void Unequip(int slotIndex) {
		Equipment unequipped = currentEquipment[slotIndex];
		if (unequipped != null) {
			if (inventory.Add(unequipped)) {
				currentEquipment[slotIndex] = null;
			}
		}
	}
}
