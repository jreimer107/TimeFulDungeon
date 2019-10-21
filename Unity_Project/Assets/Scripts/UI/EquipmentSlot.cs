
public class EquipmentSlot : ItemUISlot {
	private EquipmentManager equipmentManager;

	void Start() {
		equipmentManager = EquipmentManager.instance;
	}


	public void EquipItem(Equipment newEquip) {
		if (newEquip == null)
			return;

		base.SetItem(newEquip);
	}

	public void UnequipItem() {
		equipmentManager.Unequip((int)(base.item as Equipment).type);
		base.UnsetItem();
	}

	public override void Use() {
		UnequipItem();
	}

}
