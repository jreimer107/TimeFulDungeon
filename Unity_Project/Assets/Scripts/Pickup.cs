using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Simple GameObject wrapper for item class. Item contains all functional aspects of the item.
//This simply a dropped object that, when picked up, gives the player the attached item.
[RequireComponent(typeof(Rigidbody2D))]
public class Pickup : MonoBehaviour {
    // [SerializeField] private NavMeshAgent agent = new NavMeshAgent();
    public Item item;
    [SerializeField] private GameObject player;
    [SerializeField] private Rigidbody2D rbody;
    [SerializeField] private float smoothTime, maxSpeed;
    private Vector2 velocity = Vector2.zero;

    private bool beingPickedUp = false;
    [SerializeField] private float mergeRadius = 1f;

    private Vector2 playerDistance;

    public void SetItem(Item item) {
        this.item = item;
        SpriteRenderer sr = this.GetComponent<SpriteRenderer>();
        sr.sprite = item.sprite;
    }

    // Update is called once per frame
    void Update() {
        //Try to merge if stackable, may want to only run when an item drops
        MergeNearby();
        //If being picked up, check if agent is done and pick up
        if (beingPickedUp) {
            playerDistance = player.transform.position - transform.position;
            pickup();
        }
    }

    void FixedUpdate() {
        if (beingPickedUp) {
            Vector2 targetVelocity = new Vector2(playerDistance.x * Time.fixedDeltaTime, playerDistance.y * Time.fixedDeltaTime);
            rbody.velocity = Vector2.SmoothDamp(transform.position, playerDistance, ref velocity, smoothTime, maxSpeed);
        }
    }

    private void MergeNearby() {
        if (item.stackable) {
            Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position, mergeRadius);
            foreach (Collider2D collision in collisions) {
                if (collision.gameObject.CompareTag("Pickup")) {
                    Item other = collision.GetComponent<Pickup>().item;
                    if (item.ID == other.ID) {
                        item.count += other.count;
                        Destroy(collision.gameObject);
                    }
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject == player) {
            Inventory inventory = player.GetComponent<Inventory>();
            if (inventory.CanAdd(item)) {
                Debug.Log("Item picked up by player");
                beingPickedUp = true;
            }
        }
    }

    public void pickup() {
        if (player.transform.position == transform.position) {
            beingPickedUp = false;
            if (player.GetComponent<Inventory>().Add(item)) {
                Destroy(gameObject);
            }
        }
    }
}
