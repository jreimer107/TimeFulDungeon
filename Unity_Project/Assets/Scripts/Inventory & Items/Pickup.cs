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
	private bool beingDropped = false;

	//Need these references as GetComponent is too slow.
	[SerializeField] private CircleCollider2D triggerCollider = null;
	[SerializeField] private CircleCollider2D collisionCollider = null;
	[SerializeField] private SpriteRenderer spriteRenderer = null;

	//Discard velocity parameters, presets feel nice
	[SerializeField] private float discardVelocityScaler = 5;
	[SerializeField] private float discardMinVelocity = 10f;
	[SerializeField] private float discardMaxVelocity = 30f;

	void Start() {
		player = Player.instance.gameObject;
		if (item) {
			spriteRenderer.sprite = item.sprite;
		}
	}

	public void SetItem(Item item) {
		this.item = item;
		spriteRenderer.sprite = item.sprite;
		// GetComponent<SpriteRenderer>().sprite = item.sprite;
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

	public void Discard() {
		//Disable picking up for a few seconds.
		//Don't care if stacking is also disabled.
		StartCoroutine(DisableforSeconds());
	}

	private IEnumerator DisableforSeconds() {
		//Shouldn't need this, but cached value is null here for some reason
		Player player = Player.instance;

		//Disable collisions with player and picking up
		beingDropped = true;
		triggerCollider.enabled = false;
		Physics2D.IgnoreCollision(player.GetComponent<BoxCollider2D>(), collisionCollider);

		//Shoot pickup towards mouse location, not too slow or fast
		//Convert mouse position to be relative to player position
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = player.transform.position.z - Camera.main.transform.position.z;
		mousePos = Camera.main.ScreenToWorldPoint(mousePos) - player.transform.position;

		//First scale the vector up
		Vector2 push = mousePos * discardVelocityScaler;
		//If too large clamp it, if too small set it to a minimum
		if (push.magnitude > discardMaxVelocity) {
			push = Vector2.ClampMagnitude(push, discardMaxVelocity);
		} else if (push.magnitude < discardMinVelocity) {
			push = push.normalized * discardMinVelocity;
		}
		GetComponent<Rigidbody2D>().velocity = push;

		//Reenable player collisions after short time so that pickup gets free of player
		yield return new WaitForSeconds(0.75f);
		Physics2D.IgnoreCollision(player.GetComponent<BoxCollider2D>(), collisionCollider, false);

		//Reenable picking up after longer period so that player can leave
		yield return new WaitForSeconds(3);
		triggerCollider.enabled = true;
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
