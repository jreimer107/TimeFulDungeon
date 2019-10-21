using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class ItemUISlot : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler {
	protected Item item;
	[SerializeField] private Image icon = null;
	private Button button;

	void Awake() {
		button = GetComponent<Button>();
	}

	public virtual void SetItem(Item newItem) {
		item = newItem;

		//Set up slot to render sprite
		icon.sprite = item.sprite;
		icon.preserveAspect = true;
		icon.enabled = true;

		//Allow the button to be clickable
		button.interactable = true;
	}

	public virtual void UnsetItem() {
		icon.enabled = false;
		button.interactable = false;
	}

	public bool isEmpty {
		get {
			return !button.interactable;
		}
	}

	public virtual void Use() {
		item.Use();
	}

	public virtual void DropOn(ItemUISlot otherSlot) {
		Item other = otherSlot.item;
		if (isEmpty) {
			SetItem(other);
			otherSlot.UnsetItem();
		} else if (otherSlot.isEmpty) {
			otherSlot.SetItem(item);
			UnsetItem();
		} else {
			if (item.ID == other.ID && item.stackable) {
				other.count += item.count;
				UnsetItem();
			} else {
				otherSlot.SetItem(item);
				SetItem(other);
			}
		}
	}


	public void OnBeginDrag(PointerEventData eventData) {
		if (!isEmpty && eventData.button == PointerEventData.InputButton.Left) {
			Debug.Log("Left Click drag.");
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
			Debug.Log("Dropped item.");

			GameObject droppedLocation = eventData.pointerCurrentRaycast.gameObject;
			if (droppedLocation != null) {
				ItemUISlot droppedSlot = droppedLocation.GetComponent<ItemUISlot>();
				if (droppedSlot != null) {
					Debug.Log("Dropped on UI slot.");
					DropOn(droppedSlot);
				} else
					Debug.Log("Gameobject was not ItemUISlot.");
			} else
				Debug.Log("Dropped location was null.");

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
