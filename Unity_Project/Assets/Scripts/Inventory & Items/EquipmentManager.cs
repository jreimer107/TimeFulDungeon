using UnityEngine;

public class EquipmentManager : MonoBehaviour {
	private Inventory inventory;
	private Equipment[] currentEquipment;

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

	/// <summary>
	/// Assigns equipment item to correct slot. If there was already something
	/// equipped, puts that back in the inventory.
	/// </summary>
	/// <param name="newEquip">The new item to equip.</param>
	public void Equip(Equipment newEquip) {
		int slotIndex = (int)newEquip.type;
		Equipment currentEquip = currentEquipment[slotIndex];

		//Add currently equipped item back into inventory
		if (currentEquipment[slotIndex] != null) {
			//The new equip came from inventory, so swap it out.
			inventory.SwapOut(currentEquip, newEquip);
		} else {
			newEquip.RemoveFromInventory();
		}

		//Equip new item
		currentEquipment[slotIndex] = newEquip;

		//Invoke callback function
		if (onEquipmentChangedCallback != null) {
			onEquipmentChangedCallback.Invoke();
		}
	}

	/// <summary>
	/// Unequips an item and deletes it.
	/// </summary>
	/// <param name="slotIndex">EquipType index of item to delete.</param>
	public void DeleteEquipped(int slotIndex) {
		currentEquipment[slotIndex] = null;
		if (onEquipmentChangedCallback != null) {
			onEquipmentChangedCallback.Invoke();
		}
	}

	/// <summary>
	/// Unequips an item and adds it back into the inventory.
	/// </summary>
	/// <param name="slotIndex">EquipType index of item to unequip.</param>
	public void Unequip(int slotIndex) {
		Equipment unequipped = currentEquipment[slotIndex];
		if (unequipped != null) {
			if (inventory.Add(unequipped)) {
				DeleteEquipped(slotIndex);
			}
		}
	}

	public Equipment GetEquipment(int index) => currentEquipment[index];
	public Equipment GetEquipment(EquipType type) => currentEquipment[(int)type];

	public Melee Melee => currentEquipment[(int)EquipType.Melee] as Melee;
	public Ranged Ranged => currentEquipment[(int)EquipType.Ranged] as Ranged;
	public Shield Shield => currentEquipment[(int)EquipType.Shield] as Shield;

}
