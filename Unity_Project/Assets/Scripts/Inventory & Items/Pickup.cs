using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

//Simple GameObject wrapper for item class. Item contains all functional aspects of the item.
//This simply a dropped object that, when picked up, gives the player the attached item.
[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D))]
public class Pickup : MonoBehaviour {
	public Item item;
	private Player player;

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
	[SerializeField] private SpriteRenderer spriteRenderer = null;
	private PolygonCollider2D collisionCollider;
	private CircleCollider2D triggerCollider;

	//Discard velocity parameters, presets feel nice
	[SerializeField] private float discardVelocityScaler = 5;
	[SerializeField] private float discardMinVelocity = 10f;
	[SerializeField] private float discardMaxVelocity = 30f;

	// Tooltip timer
	private float tooltipHoverTime = 0f;
	private bool mouseHovering = false;
	[SerializeField] [Range(0, 5)] private float showTooltipAfter = 1f;
	private Tooltip tooltip;
	private TooltipManager tooltipManager;

	//Called before first frame update
	// Spawning script should call SetItem() before this happens
	private void Start() {
		tooltipManager = TooltipManager.instance;

		//If exists in scene (not spawned) need to call SetItem to get sprite
		//If spawned, spawning script needs to call SetItem.
		if (!itemSet)
			SetItem(item);

		//Dropped items need to delay collider creation until free of player
		if (!beingDropped) {
			player = Player.instance;
		}
		triggerCollider = GetComponentInChildren<CircleCollider2D>();
		triggerCollider.enabled = false;
		StartCoroutine(WaitCreateTrigger());
	}

	private IEnumerator WaitCreateTrigger() {
		// Debug.Log("Waiting for collision collider.");
		yield return new WaitUntil(() => collisionCollider != null);
		triggerCollider.enabled = true;
		// Debug.Log("Trigger collider enabled.");
	}

	//This should only be run once when the item is spawned (before Start)
	public void SetItem(Item item) {
		// Debug.Log("set item called.");
		if (item != null) {
			this.item = item;
			spriteRenderer.sprite = item.sprite;
			// Debug.Log("Item set.");
			itemSet = true;

			//Now that we have a sprite, create the collision collider
			collisionCollider = GetComponent<PolygonCollider2D>();
			if (collisionCollider == null) {
				collisionCollider = gameObject.AddComponent<PolygonCollider2D>();
				collisionCollider.sharedMaterial = (PhysicsMaterial2D)Resources.Load("Materials/PickupMaterial");
			}
			// Debug.Log("Created collision collider.");
		} else {
			// Debug.Log("Item not set.");
		}
	}

	// Update is called once per frame
	void Update() {
		//If we have a targetPosition, scale size based on distance to it.
		if (target) {
			if (beingPickedUp)
				pickup();
			Merge();
			scaleSizeByDistance();
		}

		// Mouse hover over for tooltip
		if (mouseHovering) {
			tooltipHoverTime += Time.deltaTime;
			if (!tooltip && tooltipHoverTime >= showTooltipAfter) {
				tooltip = tooltipManager.ShowTooltip(item.GetTooltipText(), 300, () => transform.position);
			}
		}
	}

	//This should be called between SetItem and Start.
	//This sets beingdropped, which prevents Start from creating the collider at the wrong time
	public void Discard() {
		Debug.Log("Discard called.");
		//Disable picking up for a few seconds.
		//Don't care if stacking is also disabled.
		StartCoroutine(DiscardCoroutine());
	}

	private IEnumerator DiscardCoroutine() {
		//Disable picking up
		beingDropped = true;
		gameObject.layer = 8;

		//Fetch player information, start is too slow
		player = Player.instance;

		//Shoot pickup towards mouse location, not too slow or fast
		//Convert mouse position to be relative to player position
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.transform.position;

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
		gameObject.layer = 10; //Pickup layer

		//Reenable picking up after longer period so that player can leave
		yield return new WaitForSeconds(3);
		beingDropped = false;
		Debug.Log("Picking up reenabled.");
		if (beingPickedUp) {
			gameObject.layer = 8;
			target = player.gameObject;
		}
	}

	private void FixedUpdate() {
		if (target) {
			//MoveTowards for linar movement, smoothdamp for smoothed movement.
			// transform.position = Vector2.MoveTowards(transform.position, player.transform.position, maxSpeed * Time.fixedDeltaTime);
			transform.position = Vector2.SmoothDamp(transform.position, target.transform.position, ref velocity, smoothTime, maxSpeed, Time.fixedDeltaTime);
			distanceToTarget = Vector2.Distance(target.transform.position, transform.position);
		}
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.CompareTag("Player")) {
			if (Inventory.instance.CanAdd(item)) {
				Debug.Log("Item picked up by player");
				beingPickedUp = true;
				if (beingDropped)
					return;
				gameObject.layer = 8;
				target = player.gameObject;
			}
		} else if (item.stackable && other.CompareTag("Pickup")) {
			//Merge nearby items of same type
			Pickup other_pickup = other.GetComponentInParent<Pickup>();
			if (item == other_pickup.item) {
				target = other.gameObject;
				Debug.Log("Target position set to " + target.transform.position);
				//Item with greater ID gets destroyed to avoid race conditions
				triggerCollider.enabled = false;
				if (gameObject.GetInstanceID() > target.GetInstanceID()) {
					markedForDestruction = true;
					collisionCollider.enabled = false;
					gameObject.layer = 11; //Pickup vacuum
				} else {
					item.count += other_pickup.item.count;
					Debug.Log("Incrementing item count to " + item.count);
				}
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (beingDropped && other.CompareTag("Player")) {
			beingPickedUp = false;
		}
	}

	private bool Merge() {
		if (distanceToTarget <= mergeRadius) {
			if (markedForDestruction) {
				Destroy(gameObject);
			} else {
				triggerCollider.enabled = true;
			}
			target = null;
			distanceToTarget = float.MaxValue;
			return true;
		}
		return false;
	}

	private void pickup() {
		if (target != player.gameObject) {
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

	#region TooltipMouseOver
	private void OnMouseEnter() {
		Debug.Log("Mouse is over " + name);
		mouseHovering = true;
		tooltipHoverTime = 0f;
	}

	private void OnMouseExit() {
		Debug.Log("Mouse is no longer over " + name);
		mouseHovering = false;
		tooltip.Destroy();
		tooltip = null;
	}
	#endregion
}
