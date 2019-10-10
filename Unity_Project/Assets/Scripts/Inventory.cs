using UnityEngine;

public class Inventory : MonoBehaviour {
    public GameObject inventory;
    public GameObject bag;
    public GameObject slot;
    public int maxSlots;

    private bool enable;
    private int enabledSlots;
    private GameObject[] slots;

    void Start() {
        enabledSlots = 40;
        for (int i = 0; i < enabledSlots; i++) {
            GameObject newSlot = Instantiate(slot, new Vector3(0, 0, 0), Quaternion.identity);
            newSlot.transform.SetParent(bag.transform, false);
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.I))
            enable = !enable;

        inventory.SetActive(enable);
    }

    public bool add(Item newItem) {
        return true;
    }
}
