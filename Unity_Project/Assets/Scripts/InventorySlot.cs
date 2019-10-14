using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour {
    public Image icon;
    private Item item;

    public void AddItem(Item newItem) {
        item = newItem;
        icon.sprite = item.sprite;
        icon.enabled = true;
    }

    public void RemoveItem() {
        item = null;
        icon.sprite = null;
        icon.enabled = false;
    }
}
