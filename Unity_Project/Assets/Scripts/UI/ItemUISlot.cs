using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class ItemUISlot : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler {
	public Item item;
	[SerializeField] private Image icon = null;
	private Button button;

	void Awake() {
		button = GetComponent<Button>();
	}

	//This method should ONLY be called by a UI manager script.
	//If you want to edit the contents of this slot, go through a UI manager.
	public virtual void SetItem(Item newItem) {
		item = newItem;
		if (item == null) {
			UnsetItem();
			return;
		}

		//Set up slot to render sprite
		icon.sprite = item.sprite;
		icon.preserveAspect = true;
		icon.enabled = true;

		//Allow the button to be clickable
		button.interactable = true;
	}

	//This method should ONLY be called by a UI manager script.
	//If you want to edit the contents of this slot, go through a UI manager.
	public virtual void UnsetItem() {
		item = null;
		icon.enabled = false;
		button.interactable = false;
	}

	public bool isEmpty {
		get {
			return item == null;
		}
	}

	protected virtual void Use() {
		item.Use();
	}

	protected virtual void DiscardItem() {
		PickupManager.instance.DiscardItem(item);
	}

	protected virtual void DropOn(ItemUISlot otherSlot) { }

	public void OnBeginDrag(PointerEventData eventData) {
		if (!isEmpty && eventData.button == PointerEventData.InputButton.Left) {
			Debug.Log("Left Click drag.");
			if (ClickAndDrag.instance == null)
				Debug.Log("heck");
			ClickAndDrag.instance.SetHeldItem(item);
			// icon.enabled = false;
			icon.color = Color.gray;
		}
	}

	public void OnDrag(PointerEventData eventData) {
		// Debug.Log("Dragging...");
	}

	public void OnEndDrag(PointerEventData eventData) {
		if (!isEmpty && eventData.button == PointerEventData.InputButton.Left) {
			GameObject droppedLocation = eventData.pointerCurrentRaycast.gameObject;
			if (droppedLocation != null) {
				ItemUISlot droppedSlot = droppedLocation.GetComponent<ItemUISlot>();
				if (droppedSlot != null) {
					Debug.Log("Dropped item on other slot.");
					DropOn(droppedSlot);
				} else {
					Debug.Log("Dropped item in UI area.");
				}
				//Else drop item in UI area, player prolly missed. do nothin.
			} else {
				Debug.Log("Dropped item to discard.");
				//Discard item
				DiscardItem();
			}

			ClickAndDrag.instance.UnsetHeldItem();
			// icon.enabled = true;
			icon.color = Color.white;
		}
	}

	public void OnPointerClick(PointerEventData eventData) {
		if (!isEmpty && eventData.button == PointerEventData.InputButton.Right) {
			Debug.Log("Right click");
			Use();
		}
	}
}