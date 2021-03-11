using TimefulDungeon.Entities;
using TimefulDungeon.Items;
using TimefulDungeon.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using VoraUtils;

namespace TimefulDungeon.Core {
    /// <summary>
    ///     The point which in-hand items will be attached to.
    ///     This is a controls object that changes the values in the player object.
    /// </summary>
    public class HoldingPoint : MonoBehaviour {
        [Range(0, 5)] [SerializeField] private float radius = 1;
        
        // Dependencies
        private Player player;
        private Inventory playerEquipment;
        private BulletParticle bulletParticle;
        private EventSystem eventSystem;

        // The thing we are currently holding. Can be weapon or shield
        private Equippable inHand;

        // Stuff controlled by our inHand
        [HideInInspector] public float angle;
        public SpriteRenderer SpriteRenderer {get; private set; }
        public EdgeCollider2D Hitbox {get; private set; }
        public Animator Animator {get; private set; }
        public AnimatorOverrideController AnimatorOverrideController {get; private set; }
        public AudioSource AudioSource {get; private set; }
        public ParticleSystem Particles {get; private set; }

        // State
        private bool IsShielding => inHand.type == EquipType.Shield;
        private bool controlledByInHand;
        private EquipType prevType = EquipType.Melee;
        private float startSwingAngle;
        
        private void Start() {
            player = Player.instance;
            playerEquipment = player.Inventory;
            eventSystem = EventSystem.current;
            player.onStaminaEmptyCallback += Unshield;
            SwapTo(EquipType.Melee);
        }

        private void Update() {
            // Check for weapon swapping
            if (Input.GetButtonDown("Swap Weapon")) {
                ToggleWeapon();
            }

            // If not attacking, check if we should shield/unshield
            if (!controlledByInHand) {
                if (Input.GetButton("Shield") && !IsShielding) {
                    Shield();
                }
                else if (!Input.GetButton("Shield") && IsShielding) {
                    Unshield();
                }
            }

            // Check for weapon use
            if (!inHand) return;
            if (Input.GetButtonDown("Fire1") && !eventSystem.IsPointerOverGameObject())
                inHand.Activate();
            else if (Input.GetButtonUp("Fire1")) inHand.Deactivate();
        }

        // Update is called once per frame
        private void FixedUpdate() {
            // If we have a weapon, see if it is controlling our position
            if (inHand) controlledByInHand = inHand.ControlHoldingPoint();

            // If the weapon 
            if (!controlledByInHand) RotateToMouse();

            SetPosition();
        }

        private void OnTriggerEnter2D(Collider2D other) {
            other.TryGetComponent(out IDamageable damageable);
            damageable?.Damage(((Weapon) inHand).damage);
        }

        private void SetPosition() {
            //Assign the angle as the rotation of the held object
            transform.localEulerAngles = new Vector3(0, 0, angle);

            // Use the angle to determine the position of the held object
            var x = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            var y = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            transform.localPosition = new Vector3(x, y, 0);

            // Flip the sprite if on left side, but only we're not melee attacking - this messes up animations
            SpriteRenderer.flipY = (inHand && inHand.type != EquipType.Melee || !controlledByInHand) && x < 0;
        }

        /// <summary>
        ///     Rotates the hand object to be between the player and the mouse pointer.
        /// </summary>
        /// <returns></returns>
        public void RotateToMouse() {
            var mousePos = Utils.GetMouseWorldPosition2D() - player.transform.Position2D();

            //Determine angle of mouse
            angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
            angle = angle < 0 ? angle + 360 : angle;
        }

        /// <summary>
        ///     Swaps the currently used weapon in the player's hand.
        ///     Unlike shielding, can be done while shielding.
        /// </summary>
        private void ToggleWeapon() {
            if (IsShielding) return;
            SwapTo(inHand.type == EquipType.Melee ? EquipType.Ranged : EquipType.Melee);
            
            // Reactivate if holding button down while swapping
            if (Input.GetButton("Fire1") && !EventSystem.current.IsPointerOverGameObject() &&
                !ClickAndDrag.instance.Active) inHand.Activate();
        }

        private void Shield() {
            if (player.exhausted) {
                print("Player exhausted, cannot shield.");
                return;
            }

            SwapTo(EquipType.Shield);
            player.StartConsumeStaminaContinuous(((Shield) inHand).staminaUse);
        }

        private void Unshield() {
            SwapTo(prevType);
            player.StopConsumeStaminaContinuous(((Shield) inHand).staminaUse);
        }
        

        /// <summary>
        ///     Event listener for when the equipment changes. If the changed equipment is what we're holding,
        ///     reloads the held object.
        /// </summary>
        /// <param name="type">Which equipment was updated.</param>
        public void OnEquipmentChange(EquipType type) {
            if (type == inHand.type) SwapTo(type);
        }
        
        /// <summary>
        ///     Swaps the currently rendered item to the in hand item.
        /// </summary>
        private void SwapTo(EquipType type) {
            var newInHand = playerEquipment.GetEquipment(type);
            if (!newInHand) {
                // We should not be able to swap to equippables that we do not have
                if (type != inHand.type) return;
                
                // If we are discarding our currently held item
                SpriteRenderer.sprite = null;
                AnimatorOverrideController["idle"] = null;
                AnimatorOverrideController["action"] = null;
                SpriteRenderer.enabled = false;
                print($"No {type} equipped.");
                return;
            }

            prevType = inHand.type;
            inHand = newInHand;
            inHand.Equip();
            SpriteRenderer.sprite = inHand.sprite;
            AnimatorOverrideController["idle"] = inHand.idleClip;
            AnimatorOverrideController["action"] = inHand.actionClip;
            SpriteRenderer.enabled = true;
        }

        #region Singleton

        public static HoldingPoint instance;

        private void Awake() {
            if (instance != null) Debug.LogWarning("More than one instance of holding point detected.");
            instance = this;

            SpriteRenderer = GetComponent<SpriteRenderer>();
            Hitbox = GetComponent<EdgeCollider2D>();
            Hitbox.enabled = false;
            Particles = GetComponentInChildren<ParticleSystem>();
            bulletParticle = Particles.GetComponent<BulletParticle>();
            if (bulletParticle) print("Got bullet particle!");

            // Setup runtime clip changes
            Animator = GetComponent<Animator>();
            AnimatorOverrideController = new AnimatorOverrideController(Animator.runtimeAnimatorController);
            Animator.runtimeAnimatorController = AnimatorOverrideController;
            AudioSource = GetComponent<AudioSource>();
        }

        #endregion
    }
}