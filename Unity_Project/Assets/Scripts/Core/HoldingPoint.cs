using TimefulDungeon.Entities;
using TimefulDungeon.Items;
using UnityEngine;
using UnityEngine.EventSystems;
using VoraUtils;

namespace TimefulDungeon.Core {
    /// <summary>
    ///     The point which in-hand items will be attached to.
    ///     This is a controls object that changes the values in the player object.
    /// </summary>
    public class HoldingPoint : MonoBehaviour {
        // Configuration
        [Range(0, 5)] [SerializeField] private float radius = 1;
        
        // State controlled by currently held item
        [HideInInspector] public float angle;
        private bool controlledByInHand;
        
        // Dependencies
        private EventSystem eventSystem;
        private Equippable inHand;
        private Inventory playerEquipment;
        private Stamina playerStamina;
        private Transform playerTransform;
        
        // State
        private EquipType prevType = EquipType.None;
        private bool shouldActivate;
        private bool shouldShield;
        
        // Components, configured by currently held item
        public SpriteRenderer SpriteRenderer { get; private set; }
        public EdgeCollider2D Hitbox { get; private set; }
        public Animator Animator { get; private set; }
        public AnimatorOverrideController AnimatorOverrideController { get; private set; }
        public AudioSource AudioSource { get; private set; }
        public ParticleSystem Particles { get; private set; }
        public BulletParticle Bullet { get; private set; }
        
        // Shorthands
        private EquipType CurrType => inHand ? inHand.type : EquipType.None;
        private bool IsShielding => CurrType == EquipType.Shield;
        private bool IsActive => inHand && inHand.Activated;

        private void Awake() {
            SpriteRenderer = GetComponent<SpriteRenderer>();
            Hitbox = GetComponent<EdgeCollider2D>();
            Hitbox.enabled = false;
            Particles = GetComponentInChildren<ParticleSystem>();
            Bullet = Particles.GetComponent<BulletParticle>();
            Animator = GetComponent<Animator>();
            AnimatorOverrideController = new AnimatorOverrideController(Animator.runtimeAnimatorController);
            Animator.runtimeAnimatorController = AnimatorOverrideController;
            AudioSource = GetComponent<AudioSource>();
        }

        private void Start() {
            var player = Player.instance;
            playerEquipment = player.Inventory;
            playerStamina = player.Stamina;
            playerTransform = player.transform;
            eventSystem = EventSystem.current;
            SwapTo(EquipType.Melee);
        }

        private void Update() {
            if (Input.GetButtonDown("Swap Weapon")) ToggleWeapon();

            if (Input.GetButtonDown("Shield")) shouldShield = true;
            else if (Input.GetButtonUp("Shield")) shouldShield = false;

            if (Input.GetButtonDown("Fire1") && !eventSystem.IsPointerOverGameObject()) shouldActivate = true;
            else if (Input.GetButtonUp("Fire1")) shouldActivate = false;

            // If not attacking, check if we should shield/unshield
            if (!controlledByInHand)
                switch (shouldShield) {
                    case true when !IsShielding:
                        Shield();
                        break;
                    case false when IsShielding:
                        Unshield();
                        break;
                }

            // Check for weapon use
            if (!inHand) return;
            switch (shouldActivate) {
                case true when !IsActive:
                    inHand.Activate();
                    break;
                case false when IsActive:
                    inHand.Deactivate();
                    break;
            }
        }

        private void FixedUpdate() {
            // If we have a weapon, see if it is controlling our position
            if (inHand) controlledByInHand = inHand.ControlHoldingPoint();

            // If we're not being controlled, freely rotate
            if (!controlledByInHand) RotateToMouse();

            SetPosition();
        }

        private void OnTriggerEnter2D(Collider2D other) {
            other.TryGetComponent(out IDamageable damageable);
            damageable?.Damage(((Weapon) inHand).damage);
        }

        private void ResetComponents() {
            SpriteRenderer.sprite = null;
            AnimatorOverrideController["idle"] = null;
            AnimatorOverrideController["action"] = null;
            SpriteRenderer.enabled = false;
        }

        private void SetPosition() {
            //Assign the angle as the rotation of the held object
            transform.localEulerAngles = new Vector3(0, 0, angle);

            // Use the angle to determine the position of the held object
            var x = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            var y = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            transform.localPosition = new Vector2(x, y);

            // Flip the sprite if on left side, but only we're not melee attacking - this messes up animations
            var flip = (CurrType != EquipType.Melee || !controlledByInHand) && x < 0;
            SpriteRenderer.flipY = flip;
            SpriteRenderer.sortingOrder = flip ? -1 : 1;
        }

        /// <summary>
        ///     Rotates the hand object to be between the player and the mouse pointer.
        /// </summary>
        public void RotateToMouse() {
            var mousePos = Utils.GetMouseWorldPosition2D() - playerTransform.Position2D();

            //Determine angle of mouse
            angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
            angle = angle < 0 ? angle + 360 : angle;
        }

        /// <summary>
        ///     Toggles the current weapon from Melee to Ranged or vice versa.
        /// </summary>
        private void ToggleWeapon() {
            if (IsShielding) return;
            SwapTo(CurrType == EquipType.Melee ? EquipType.Ranged : EquipType.Melee);
        }

        private void Shield() {
            if (playerStamina.Exhausted) {
                print("Player exhausted, cannot shield.");
                return;
            }

            SwapTo(EquipType.Shield);
            if (inHand) playerStamina.StartContinuousUse(((Shield) inHand).staminaUse);
        }

        private void Unshield() {
            playerStamina.StopContinuousUse(((Shield) inHand).staminaUse);
            SwapTo(prevType, true);
        }

        public void OnStaminaEmpty() {
            SwapTo(prevType, true);
        }

        /// <summary>
        ///     Event listener for when the equipment changes. If the changed equipment is what we're holding,
        ///     reloads the held object.
        /// </summary>
        /// <param name="type">Which equipment was updated.</param>
        public void OnEquipmentChange(EquipType type) {
            if (type == CurrType || !inHand && type != EquipType.Shield) SwapTo(type, true);
        }

        /// <summary>
        ///     Swaps the currently rendered item to the in hand item.
        /// </summary>
        private void SwapTo(EquipType type, bool resetOnNull = false) {
            var newInHand = playerEquipment.GetEquipment(type);
            if (!newInHand) {
                print($"No {type} equipped.");
                if (resetOnNull)
                    ResetComponents();
                else
                    return;
            }

            prevType = CurrType;
            inHand = newInHand;

            if (!inHand) return;
            inHand.Equip(this);
        }
    }
}