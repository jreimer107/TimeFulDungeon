using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentUI : MonoBehaviour {
	[SerializeField] private EquipmentSlot meleeSlotUI = null;
	[SerializeField] private EquipmentSlot rangedSlotUI = null;
	[SerializeField] private EquipmentSlot shieldSlotUI = null;

	private EquipmentManager equipmentManager;

	// Start is called before the first frame update
	void Start() {
		equipmentManager = EquipmentManager.instance;
		equipmentManager.onEquipmentChangedCallback += UpdateUI;
	}

	// Update is called once per frame
	private void UpdateUI() {
		meleeSlotUI.EquipItem(equipmentManager.currentEquipment[(int)EquipType.Melee]);
		rangedSlotUI.EquipItem(equipmentManager.currentEquipment[(int)EquipType.Ranged]);
		shieldSlotUI.EquipItem(equipmentManager.currentEquipment[(int)EquipType.Shield]);
	}
}
