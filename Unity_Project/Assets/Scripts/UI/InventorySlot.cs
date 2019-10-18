using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class InventorySlot : MonoBehaviour {
	[SerializeField] private Image icon = null;
	[SerializeField] private Text countUI = null;
	[SerializeField] private Button button = null;
	private Item item;

	public void AddItem(Item newItem) {
		item = newItem;

		//Set up bag slot to render sprite
		icon.sprite = item.sprite;
		icon.preserveAspect = true;
		icon.enabled = true;

		//Allow the button to be clickable
		button.interactable = true;

		//Initialize the count UI if item is stackable
		if (item.stackable) {
			countUI.enabled = true;
			if (item.count > 1000000)
				countUI.text = (item.count / 1000000) + "M";
			else if (item.count > 1000)
				countUI.text = (item.count / 1000) + "K";
			else
				countUI.text = item.count.ToString();
		} else
			countUI.enabled = false;
	}

	public void RemoveItem() {
		item = null;
		icon.sprite = null;
		icon.enabled = false;
		button.interactable = false;
		countUI.enabled = false;
	}

	public void UseItem() {
		if (item != null) {

		}
	}
}
