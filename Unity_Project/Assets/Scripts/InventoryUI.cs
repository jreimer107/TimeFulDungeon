using UnityEngine;

public class InventoryUI : MonoBehaviour {
    private Inventory inventory;

    void Start() {
        inventory = Inventory.instance;
        inventory.onItemChangedCallback += UpdateUI;
    }

    void UpdateUI() {
        Debug.Log("Updating UI");
    }
}