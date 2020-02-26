using System;
using UnityEngine;

public class EquipmentUI : MonoBehaviour {
	private EquipmentManager equipmentManager;
	private EquipmentSlot[] slots = null;


	// Start is called before the first frame update
	void Start() {
		equipmentManager = EquipmentManager.instance;
		equipmentManager.onEquipmentChangedCallback += UpdateUI;
		slots = GetComponentsInChildren<EquipmentSlot>();
		Array.Sort(slots,
			delegate (EquipmentSlot x, EquipmentSlot y) { return x.equipSlotType.CompareTo(y.equipSlotType); });
	}

	// Update is called once per frame
	private void UpdateUI() {
		for (int i = 0; i < slots.Length; i++) {
			Equipment equipment = equipmentManager.GetEquipment(i);
			if (equipment)
				slots[i].SetItem(equipment);
			else
				slots[i].UnsetItem();
		}
	}
}
