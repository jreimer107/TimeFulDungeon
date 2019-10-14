using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour {
    public GameObject inventoryUI;
    public GameObject bagUI;
    public GameObject slotUI;
    public int maxSlots;

    private bool enable;
    private int enabledSlots;
    private GameObject[] slots;

    private List<Item> Bag;
    public Equippable EquippedMelee;
    public Equippable EquippedRanged;
    public Equippable EquippedShield;

    #region Singleton
    public static Inventory instance;
    void Awake() {
        if (instance != null) {
            Debug.LogWarning("More than one instance of inventory found.");
        }
        instance = this;
    }
    #endregion

    //Event to subscribe to for when the inventory contents changes.
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    void Start() {
        Bag = new List<Item>();
        enabledSlots = 40;
        for (int i = 0; i < enabledSlots; i++) {
            GameObject newSlot = Instantiate(slotUI, new Vector3(0, 0, 0), Quaternion.identity);
            newSlot.transform.SetParent(bagUI.transform, false);
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.I))
            enable = !enable;

        inventoryUI.SetActive(enable);
    }

    public bool CanAdd(Item newItem) {
        if (newItem.stackable) {
            foreach (Item item in Bag) {
                if (item.ID == newItem.ID) {
                    return true;
                }
            }
        }
        if (Bag.Count < enabledSlots)
            return true;
        return false;
    }

    public bool Add(Item newItem) {
        //If item is stackable, check if we already have some
        if (newItem.stackable) {
            foreach (Item item in Bag) {
                if (item.ID == newItem.ID) {
                    item.count += newItem.count;
                    if (onItemChangedCallback != null)
                        onItemChangedCallback.Invoke();
                    return true;
                }
            }
        }

        //If we have space, add item in new slot
        if (Bag.Count < enabledSlots) {
            Bag.Add(newItem);
            if (onItemChangedCallback != null)
                onItemChangedCallback.Invoke();
            Debug.Log("Added new item to inventory.");
            return true;
        }

        //Couldn't stack and couldn't fit, so can't add to inventory.
        return false;
    }

    public void Remove(Item item) {
        Bag.Remove(item);
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }
}
