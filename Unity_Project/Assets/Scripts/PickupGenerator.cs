using UnityEngine;

public class PickupGenerator : MonoBehaviour {
    //Item fields
    new public string name;
    public string description;
    public Sprite sprite;
    public bool stackable;
    public int count;

    //Equippable fields
    public int damage, range, speed, cooldown;
    public float arc;

    //Item to create a pickup for.
    private Item item;

    //Pickup prefab
    public GameObject pickupPrefab;

    // Start is called before the first frame update
    void Start() {
        //Generate item and the pickup for it
        item = new Melee(name, sprite, damage, arc, range, speed, cooldown);
        GameObject pickup_object = Instantiate(pickupPrefab, transform.position, transform.rotation);

        //Set the pickup to have the item in it
        pickup_object.GetComponent<Pickup>().SetItem(item);

        //No need for this object anymore
        Destroy(gameObject);
    }
}
