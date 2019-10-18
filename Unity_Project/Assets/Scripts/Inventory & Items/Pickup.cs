using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Simple GameObject wrapper for item class. Item contains all functional aspects of the item.
//This simply a dropped object that, when picked up, gives the player the attached item.
[RequireComponent(typeof(SpriteRenderer))]
public class Pickup : MonoBehaviour {
	public Item item;
	private GameObject player;

	[SerializeField] private float smoothTime = 0.05f;
	[SerializeField] private float maxSpeed = 7;
	[SerializeField] private float pickupDistance = 0.75f;
	[SerializeField] private float shrinkDistance = 2f;
	[Range(0, 1)] [SerializeField] private float minShrinkScale = 0.5f;
	[SerializeField] private float mergeRadius = 0.1f;

	private GameObject target = null;
	private float distanceToTarget = float.MaxValue;
	private Vector2 velocity = Vector2.zero;
	private bool beingPickedUp = false;
	private bool markedForDestruction = false;

	void Start() {
		player = Player.instance.gameObject;
		if (item) {
			GetComponent<SpriteRenderer>().sprite = item.sprite;
		}
	}

	public void SetItem(Item item) {
		this.item = item;
		GetComponent<SpriteRenderer>().sprite = item.sprite;
	}

	// Update is called once per frame
	void Update() {
		//If being picked up, check if agent is done and pick up
		if (beingPickedUp) {
			pickup();
		}

		//If we have a targetPosition, scale size based on distance to it.
		if (target) {
			Merge();
			scaleSizeByDistance();
		}
	}

	void FixedUpdate() {
		if (beingPickedUp || target) {
			//MoveTowards for linar movement, smoothdamp for smoothed movement.
			// transform.position = Vector2.MoveTowards(transform.position, player.transform.position, maxSpeed * Time.fixedDeltaTime);
			transform.position = Vector2.SmoothDamp(transform.position, target.transform.position, ref velocity, smoothTime, maxSpeed, Time.fixedDeltaTime);
			distanceToTarget = Vector2.Distance(target.transform.position, transform.position);
		}
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (!beingPickedUp && (other.gameObject == player || other.gameObject.CompareTag("Player"))) {
			if (Inventory.instance.CanAdd(item)) {
				Debug.Log("Item picked up by player");
				beingPickedUp = true;
				target = player;
				//Destroy the collider so that it looks like its going into the player.
				Destroy(GetComponent<CircleCollider2D>());

			}
		} else if (item.stackable && other.CompareTag("Pickup")) {
			//Merge nearby items of same type
			Pickup other_pickup = other.GetComponent<Pickup>();
			if (item.ID == other_pickup.item.ID) {
				target = other.gameObject;
				Debug.Log("Target position set to " + target.transform.position);
				//Item with greater ID gets destroyed to avoid race conditions
				if (gameObject.GetInstanceID() > target.GetInstanceID()) {
					markedForDestruction = true;
					Destroy(GetComponent<CircleCollider2D>());
				} else {
					item.count += other_pickup.item.count;
				}
			}
		}
	}

	private bool Merge() {
		if (distanceToTarget <= mergeRadius) {
			if (markedForDestruction) {
				Destroy(gameObject);
			}
			target = null;
			distanceToTarget = float.MaxValue;
			return true;
		}
		return false;
	}

	//Assumes that target is the player.
	private void pickup() {
		if (target != player) {
			return;
		}

		if (distanceToTarget <= pickupDistance) {
			beingPickedUp = false;
			target = null;
			distanceToTarget = float.MaxValue;
			if (Inventory.instance.Add(item)) {
				Destroy(gameObject);
			}
		}
	}

	private void scaleSizeByDistance() {
		if (!target || distanceToTarget >= shrinkDistance) {
			transform.localScale = Vector3.one;
		} else {
			//Normalize scale to be within set min scale and 1 between range of shrink and pickup
			float newScale = (distanceToTarget - pickupDistance) / (shrinkDistance - pickupDistance) * (1 - minShrinkScale) + minShrinkScale;
			transform.localScale = new Vector3(newScale, newScale, 1);
		}
	}
}
