using System.Collections;
using TimefulDungeon.Core;
using TimefulDungeon.UI;
using UnityEngine;

namespace TimefulDungeon.Items {
    //Simple GameObject wrapper for item class. Item contains all functional aspects of the item.
    //This simply a dropped object that, when picked up, gives the player the attached item.
    [RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D))]
    public class Pickup : MonoBehaviour {
        [SerializeField] private ItemTemplate template;
        
        // Resources
        private static Pickup pickupPrefab;
        private static PhysicsMaterial2D pickupMaterial;
        
        // Dependencies
        private static Tooltip tooltip;
        private static Inventory inventory;
        
        // The item we contain
        public Item item;
        public Vector2 spawnVelocity = Vector2.zero;

        // Movement and shrinking parameters
        [SerializeField] private float smoothTime = 0.05f;
        [SerializeField] private float maxSpeed = 7;
        [SerializeField] private float shrinkDistance = 2f;
        [Range(0, 1)] [SerializeField] private float minShrinkScale = 0.5f;

        // Radii to execute pickup and merge 
        [SerializeField] private float pickupDistance = 0.75f;
        [SerializeField] private float mergeRadius = 0.1f;

        // Discard velocity parameters, presets feel nice
        [SerializeField] private float minPushVelocity = 10f;
        [SerializeField] private float maxPushVelocity = 30f;
        
        // State
        private Transform _target;
        private bool _pickUpDisabled;
        private bool _beingMerged;
        private bool _beingPickedUp;
        private Vector2 _velocity = Vector2.zero;

        // References to own components
        private PolygonCollider2D _hitbox;
        private CircleCollider2D _triggerCollider;
        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rigidbody;
        private float _distanceToTarget = float.MaxValue;
        

        private void Awake() {
            // Get own references
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _triggerCollider = GetComponent<CircleCollider2D>();
            _rigidbody = GetComponent<Rigidbody2D>();

            pickupPrefab ??= Resources.Load<Pickup>("Prefabs/Pickup");
            pickupMaterial ??= Resources.Load<PhysicsMaterial2D>("Materials/PickupMaterial");

            if (template) {
                item = template.GetInstance();
            }
            
        }

        private void Start() {
            // Get dependencies
            tooltip ??= Tooltip.instance;
            inventory ??= Player.instance.Inventory;

            // Set sprite and create the hitbox
            _spriteRenderer.sprite = item.sprite;
            _hitbox = gameObject.AddComponent<PolygonCollider2D>();
            _hitbox.sharedMaterial = pickupMaterial;
            
            // Push the pickup
            if (spawnVelocity != Vector2.zero) {
                _rigidbody.velocity = spawnVelocity.ClampMagnitude(minPushVelocity, maxPushVelocity);
            }
            
            // Disable picking up for a bit to allow for discarding
            StartCoroutine(TemporarilyDisablePickup());
        }

        private void Update() {
            //If we have a targetPosition, scale size based on distance to it.
            if (!_target) return;
            _distanceToTarget = Vector2.Distance(_target.position, transform.position);
            
            // Check if we can merge or pickup
            if (_beingMerged && _distanceToTarget <= mergeRadius) {
                // Merges destroy instantly, as they cannot fail
                Destroy(gameObject);
            }
            else if (_beingPickedUp && _distanceToTarget < pickupDistance) {
                // Pickups can fail, so undo picking up if it does
                if (inventory.AddItem(item)) {
                    Debug.Log("Item picked up by player");
                    Destroy(gameObject);
                }
                else {
                    _target = null;
                }
            }

            // Scale size by distance
            if (!_target || _distanceToTarget >= shrinkDistance) {
                transform.localScale = Vector3.one;
            }
            //Normalize scale to be within set min scale and 1 between range of shrink and pickup
            else {
                var newScale =
                    (_distanceToTarget - pickupDistance) / (shrinkDistance - pickupDistance) * (1 - minShrinkScale) +
                    minShrinkScale;
                transform.localScale = Vector2.one * newScale;
            }
        }

        private void FixedUpdate() {
            if (!_target) return;
            //MoveTowards for linear movement, SmoothDamp for smoothed movement.
            // transform.position = Vector2.MoveTowards(transform.position, target.transform.position, maxSpeed * Time.fixedDeltaTime);
            transform.position = Vector2.SmoothDamp(transform.position, _target.transform.position, ref _velocity,
                smoothTime, maxSpeed, Time.fixedDeltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            Debug.Log("Trigger enter: " + other.name, gameObject);
            // Player entered pickup range, begin pick up
            if (other.CompareTag("Player")) {
                if (_pickUpDisabled || !inventory.CanAdd(item)) return;
                Debug.Log("Pickup check passed!");
                _beingPickedUp = true;
                _target = other.transform;
            }
            // We are stackable and can stack with nearby pickup, begin merge
            else if (item.stackable && other.CompareTag("Pickup")) {
                var otherPickup = other.GetComponent<Pickup>();
                if (item != otherPickup.item) return;
                _target = other.transform;
                Debug.Log("Target position set to " + _target.transform.position);
                //Item with greater ID gets destroyed to avoid race conditions
                if (transform.GetInstanceID() > _target.GetInstanceID()) {
                    _beingMerged = true;
                    _hitbox.enabled = false;
                    _triggerCollider.enabled = false;
                }
                else {
                    item.count += otherPickup.item.count;
                    Debug.Log("Incrementing item count to " + item.count);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.CompareTag("Player")) {
                Debug.Log("Exited pickup radius, picking up re-enabled.", gameObject);
                _beingPickedUp = false;
            }

            _target = null;
            transform.localScale = Vector3.one;
        }

        #region Static methods

        /// <summary>
        /// Creates a pickup of the given item at the given position.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="position">Where the pickup should spawn.</param>
        /// <param name="velocity">When instantiated, this velocity is applied. Clamped by parameters.</param>
        public static void Create(Item item, Vector2 position, Vector2 velocity) {
            if (!item) {
                Debug.LogWarning("Null item in pickup!");
                return;
            }

            var pickup = Instantiate(pickupPrefab, position, Quaternion.identity);
            pickup.item = item;
            pickup.spawnVelocity = velocity;

            Debug.Log("Pickup spawned;");
        }
        #endregion

        private IEnumerator TemporarilyDisablePickup() {
            _pickUpDisabled = true;
            yield return new WaitForSeconds(2f);
            _pickUpDisabled = false;
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