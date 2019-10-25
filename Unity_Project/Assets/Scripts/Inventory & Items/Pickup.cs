using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Simple GameObject wrapper for item class. Item contains all functional aspects of the item.
//This simply a dropped object that, when picked up, gives the player the attached item.
[RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D))]
public class Pickup : MonoBehaviour {
	public Item item;
	private GameObject player;
	private BoxCollider2D playerCollider;

	[SerializeField] private float smoothTime = 0.05f;
	[SerializeField] private float maxSpeed = 7;
	[SerializeField] private float pickupDistance = 0.75f;
	[SerializeField] private float shrinkDistance = 2f;
	[Range(0, 1)] [SerializeField] private float minShrinkScale = 0.5f;
	[SerializeField] private float mergeRadius = 0.1f;

	private GameObject target = null;
	private Vector2 velocity = Vector2.zero;
	private float distanceToTarget = float.MaxValue;

	private bool itemSet = false;
	private bool beingPickedUp = false;
	private bool beingDropped = false;
	private bool markedForDestruction = false;

	//Need these references as GetComponent is too slow.
	[SerializeField] private CircleCollider2D triggerCollider = null;
	[SerializeField] private SpriteRenderer spriteRenderer = null;
	private PolygonCollider2D collisionCollider;

	//Discard velocity parameters, presets feel nice
	[SerializeField] private float discardVelocityScaler = 5;
	[SerializeField] private float discardMinVelocity = 10f;
	[SerializeField] private float discardMaxVelocity = 30f;

	//Called before first frame update
	// Spawning script should call SetItem() before this happens
	void Start() {
		Debug.Log("start called.");
		player = Player.instance.gameObject;
		playerCollider = player.GetComponent<BoxCollider2D>();

		//If exists in scene (not spawned) need to call SetItem to get sprite
		//If spawned, spawning script needs to call SetItem.
		if (!itemSet)
			SetItem(item);

		//Dropped items need to delay collider creation until free of player
		if (!beingDropped) {
			collisionCollider = gameObject.AddComponent<PolygonCollider2D>();
			collisionCollider.sharedMaterial = (PhysicsMaterial2D)Resources.Load("Materials/PickupMaterial");
		}
	}

	//This should only be run once when the item is spawned (before Start)
	public void SetItem(Item item) {
		Debug.Log("set item called.");
		if (item != null) {
			this.item = item;
			spriteRenderer.sprite = item.sprite;
			Debug.Log("Item set.");
			itemSet = true;
		} else
			Debug.Log("Item not set.");
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

	//This should be called between SetItem and Start.
	//This sets beingdropped, which prevents Start from creating the collider at the wrong time
	public void Discard() {
		Debug.Log("Discard called.");
		//Disable picking up for a few seconds.
		//Don't care if stacking is also disabled.
		StartCoroutine(DisableforSeconds());
	}

	private IEnumerator DisableforSeconds() {
		//Shouldn't need this, but cached value is null here for some reason
		Player player = Player.instance;

		//Disable picking up
		beingDropped = true;

		//Shoot pickup towards mouse location, not too slow or fast
		//Convert mouse position to be relative to player position
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = player.transform.position.z - Camera.main.transform.position.z;
		mousePos = Camera.main.ScreenToWorldPoint(mousePos) - player.transform.position;

		//First scale the vector up. If too large clamp it, if too small set it to a minimum
		Vector2 push = mousePos * discardVelocityScaler;
		if (push.magnitude > discardMaxVelocity) {
			push = Vector2.ClampMagnitude(push, discardMaxVelocity);
		} else if (push.magnitude < discardMinVelocity) {
			push = push.normalized * discardMinVelocity;
		}
		GetComponent<Rigidbody2D>().velocity = push;

		//Create collider after pickup is free of player to enable player collisions
		yield return new WaitForSeconds(0.75f);
		collisionCollider = gameObject.AddComponent<PolygonCollider2D>();
		collisionCollider.sharedMaterial = (PhysicsMaterial2D)Resources.Load("Materials/PickupMaterial");

		//Reenable picking up after longer period so that player can leave
		yield return new WaitForSeconds(3);
		beingDropped = false;
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
		if (!beingPickedUp && !beingDropped && (other.gameObject == player || other.gameObject.CompareTag("Player"))) {
			if (Inventory.instance.CanAdd(item)) {
				Debug.Log("Item picked up by player");
				beingPickedUp = true;
				target = player;
				//Disable the collider so that it looks like its going into the player.
				collisionCollider.enabled = false;
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
					collisionCollider.enabled = false;
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
