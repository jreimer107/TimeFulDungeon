using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class EquipmentSlot : MonoBehaviour {
	[SerializeField] private Image icon = null;
	[SerializeField] private Button button = null;
	private Equipment equipped;
	private EquipmentManager equipmentManager;

	void Start() {
		equipmentManager = EquipmentManager.instance;
	}


	public void EquipItem(Equipment newEquip) {
		if (newEquip == null)
			return;

		equipped = newEquip;

		//Set up slot to render sprite
		icon.sprite = equipped.sprite;
		icon.preserveAspect = true;
		icon.enabled = true;

		//Allow the button to be clickable
		button.interactable = true;

	}

	public void UnequipItem() {
		icon.sprite = null;
		icon.enabled = false;
		button.interactable = false;
		equipmentManager.Unequip((int)equipped.type);
		equipped = null;
	}
}
