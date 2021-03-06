using TimefulDungeon.Core;
using TimefulDungeon.UI;
using UnityEngine;
using VoraUtils;

//Simple GameObject wrapper for item class. Item contains all functional aspects of the item.
//This simply a dropped object that, when picked up, gives the player the attached item.
// [RequireComponent(typeof(SpriteRenderer), typeof(MovementController))]
namespace TimefulDungeon {
	public class Pickup : MonoBehaviour {
		public Item item;
		private static Transform player;

		[SerializeField] private float smoothTime = 0.05f;
		[SerializeField] private float maxSpeed = 7;
		[SerializeField] private float pickupDistance = 0.75f;
		[SerializeField] private float shrinkDistance = 2f;
		[Range(0, 1)] [SerializeField] private float minShrinkScale = 0.5f;
		[SerializeField] private float mergeRadius = 0.1f;

		private Transform target = null;
		private Vector2 velocity = Vector2.zero;
		private float distanceToTarget = float.MaxValue;

		private bool beingPickedUp = false;
		private bool beingDropped = false;
		private bool beingMerged = false;

		//Need these references as GetComponent is too slow.
		private SpriteRenderer spriteRenderer;
		private PolygonCollider2D collisionCollider;
		private CircleCollider2D triggerCollider;
		// private MovementController movementController;

		//Discard velocity parameters, presets feel nice
		[SerializeField] private float discardVelocityScaler = 5;
		[SerializeField] private float discardMinVelocity = 10f;
		[SerializeField] private float discardMaxVelocity = 30f;

		// Tooltip timer
		private Tooltip tooltip;

		public static Pickup SpawnPickup(Item item, Vector2 position) {
			if (!item) {
				Debug.LogWarning("Null item in pickup!");
				return null;
			}

			Pickup pickup = Instantiate(
				Resources.Load<GameObject>("Prefabs/Pickup"),
				position,
				Quaternion.identity
			).GetComponent<Pickup>();
			pickup.SetItem(item);
			Debug.Log("Pickup spawned;");
			return pickup;
		}

		public static void DiscardPickup(Item item) {
			if (!player) {
				player = Player.instance.transform;
			}
			SpawnPickup(item, player.transform.position)?.Discard();
		}


		private void Awake() {
			// Get own references
			spriteRenderer = GetComponent<SpriteRenderer>();
			triggerCollider = GetComponentInChildren<CircleCollider2D>();
			// movementController = GetComponent<MovementController>();

			// Preconstructed item, not created by SpawnPickup
			if (item && !collisionCollider) {
				SetItem(item);
			}
		}

		//Called before first frame update
		private void Start() {
			// Get other's references
			if (!tooltip) {
				tooltip = Tooltip.instance;
			}
			if (!player) {
				player = Player.instance.transform;
			}
		}

		// Update is called once per frame
		private void Update() {
			//If we have a targetPosition, scale size based on distance to it.
			if (target) {
				distanceToTarget = Vector2.Distance(target.position, transform.position);

				// Check if we can merge or pickup
				if (beingMerged && distanceToTarget <= mergeRadius) {
					// Merges destroy instantly, as they cannot fail
					Destroy(gameObject);
				} else if (beingPickedUp && distanceToTarget < pickupDistance) {
					// Pickups can fail, so undo picking up if it does
					if (Inventory.instance.Add(item)) {
						Debug.Log("Item picked up by player");
						Destroy(gameObject);
					} else {
						target = null;
					}
				}

				// Scale size by distance
				if (!target || distanceToTarget >= shrinkDistance) {
					transform.localScale = Vector3.one;
				} else {
					//Normalize scale to be within set min scale and 1 between range of shrink and pickup
					float newScale = (distanceToTarget - pickupDistance) / (shrinkDistance - pickupDistance) * (1 - minShrinkScale) + minShrinkScale;
					transform.localScale = Vector2.one * newScale;
				}
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

		//This should only be run once after instantiation
		private void SetItem(Item item) {
			this.item = item;
			spriteRenderer.sprite = item.sprite;

			//Now that we have a sprite, create the collision collider
			collisionCollider = GetComponent<PolygonCollider2D>();
			if (collisionCollider == null) {
				collisionCollider = gameObject.AddComponent<PolygonCollider2D>();
				collisionCollider.sharedMaterial = (PhysicsMaterial2D)Resources.Load("Materials/PickupMaterial");
			}
		}

		private void Discard() {
			Debug.Log("Discard called.");
			beingDropped = true;

			//Shoot pickup towards mouse location, not too slow or fast
			Vector2 mousePos = Utils.GetMouseWorldPosition3D() - player.transform.position;

			//First scale the vector up. If too large clamp it, if too small set it to a minimum
			Vector2 push = mousePos * discardVelocityScaler;
			if (push.magnitude > discardMaxVelocity) {
				push = Vector2.ClampMagnitude(push, discardMaxVelocity);
			} else if (push.magnitude < discardMinVelocity) {
				push = push.normalized * discardMinVelocity;
			}
			// GetComponent<MovementController>().Push(push);
			GetComponent<Rigidbody2D>().velocity = push;
			// Debug.Log(push);
		}

		private void OnTriggerEnter2D(Collider2D other) {
			if (other.CompareTag("Player")) {
				if (!beingDropped && Inventory.instance.CanAdd(item)) {
					Debug.Log("Pickup check passed!");
					beingPickedUp = true;
					target = player;
				}
			} else if (item.stackable && other.CompareTag("Pickup")) {
				//Merge nearby items of same type
				Pickup other_pickup = other.GetComponentInParent<Pickup>();
				if (item == other_pickup.item) {
					target = other.transform;
					Debug.Log("Target position set to " + target.transform.position);
					//Item with greater ID gets destroyed to avoid race conditions
					if (transform.GetInstanceID() > target.GetInstanceID()) {
						beingMerged = true;
						collisionCollider.enabled = false;
						triggerCollider.enabled = false;
					} else {
						item.count += other_pickup.item.count;
						Debug.Log("Incrementing item count to " + item.count);
					}
				}
			}
		}

		private void OnTriggerExit2D(Collider2D other) {
			if (other.CompareTag("Player")) {
				Debug.Log("Exited pickup raidus, picking up reenabled.");
				beingPickedUp = false;
				beingDropped = false;
			}
			target = null;
			// movementController.SetMoveDirection(0, 0);
			transform.localScale = Vector3.one;
		}

		#region TooltipHover
		private void OnMouseEnter() {
			tooltip.ShowTextOnDelay(item.GetTooltipText(), 300);
		}

		private void OnMouseExit() {
			tooltip.Hide();
		}
		#endregion
	}
}