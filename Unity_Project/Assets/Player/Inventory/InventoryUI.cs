using UnityEngine;

namespace TimefulDungeon.UI {
    public class InventoryUI : MonoBehaviour {
        [SerializeField] private GameObject inventoryUI;
        
        private void Start() {
            inventoryUI.SetActive(false);
        }
        
        private void Update() {
            if (Input.GetButtonDown("Inventory")) inventoryUI.SetActive(!inventoryUI.activeSelf);
        }
    }
}