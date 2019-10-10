using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour {
    public Item item;
    public Sprite sprite;

    // Start is called before the first frame update
    void Start(Item item) {
        this.item = item;
        this.sprite = item.sprite;
        this.tag = "Pickup";
    }

    // Update is called once per frame
    void Update() {

    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Item")) {
            Item otherItem = other.gameObject.GetComponent<Item>();
            if (this.item.stackable && otherItem.ID == this.item.ID) {
                this.item.stackSize += otherItem.stackSize;
                Destroy(other.gameObject);
            }
        } else if (other.CompareTag("Player")) {
            this.pickup();
        }
    }

    public void pickup() {
        Inventory inventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();
        if (inventory.add(this.item)) {
            Destroy(this);
        }
    }
}
