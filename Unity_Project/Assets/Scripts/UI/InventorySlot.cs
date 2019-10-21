using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : ItemUISlot {
	[SerializeField] private Text countUI = null;

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
}
