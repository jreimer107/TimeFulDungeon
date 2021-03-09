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
        public float angle;

        public Equippable inHand;

        public SpriteRenderer spriteRenderer;
        public EdgeCollider2D hitbox;
        public Animator animator;
        public AnimatorOverrideController animatorOverrideController;
        public AudioSource audioSource;

        public ParticleSystem particles;

        private bool controlledByInHand;
        private EquipType currentWeaponType = EquipType.Melee;
        private Player player;
        private Inventory playerEquipment;
        private bool shieldToggleBuffer;
        private float startSwingAngle;

        private void Start() {
            player = Player.instance;
            playerEquipment = player.Inventory;
            player.onStaminaEmptyCallback += ExhaustedUnshield;
            UpdateRendered();
        }

        private void Update() {
            // Check for weapon swapping
            if (Input.GetButtonDown("Swap Weapon")) {
                Debug.Log("Swap in-hand");
                ToggleWeapon();

                // Activate if holding button down while swapping
                if (inHand && Input.GetButton("Fire1") && !EventSystem.current.IsPointerOverGameObject() &&
                    !ClickAndDrag.instance.Active) inHand.Activate();
            }

            // If not attacking and shield is buffered, shield and consume the buffer
            shieldToggleBuffer = !player.exhausted && (Input.GetButtonDown("Shield") || Input.GetButtonUp("Shield"));
            if (shieldToggleBuffer && !controlledByInHand) ToggleShield();

            // Check for button press or release
            if (!inHand) return;
            if (Input.GetButtonDown("Fire1") && !EventSystem.current.IsPointerOverGameObject() &&
                !ClickAndDrag.instance.Active)
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

        private void SetPosition() {
            //Assign the angle as the rotation of the held object
            transform.localEulerAngles = new Vector3(0, 0, angle);

            // Use the angle to determine the position of the held object
            var x = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            var y = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            transform.localPosition = new Vector3(x, y, 0);

            // Flip the sprite if on left side, but only we're not melee attacking - this messes up animations
            spriteRenderer.flipY = (inHand && inHand.type != EquipType.Melee || !controlledByInHand) && x < 0;
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
            currentWeaponType = currentWeaponType == EquipType.Melee ? EquipType.Ranged : EquipType.Melee;
            if (player.Shielding) return;
            inHand = playerEquipment.GetEquipment(currentWeaponType);
            SwapRendered();
        }

        /// <summary>
        ///     Swaps the currently used item to the equipped shield.
        /// </summary>
        private void ToggleShield() {
            // TODO: Needs to be passed an input. Exhaustion has a bug otherwise.
            shieldToggleBuffer = false;
            if (player.exhausted) {
                Debug.Log("Player is exhausted!");
                return;
            }

            Equippable shield = playerEquipment.Shield;
            if (shield) {
                player.ToggleShielding();
                inHand = player.Shielding ? shield : playerEquipment.GetEquipment(currentWeaponType);
                SwapRendered();
            }
            else {
                Debug.Log("No shield equipped!");
            }
        }

        private void ExhaustedUnshield() {
            Debug.Log("Force unshield");
            if (player.Shielding) ToggleShield();
        }

        /// <summary>
        ///     Swaps the currently rendered item to the in hand item.
        /// </summary>
        private void SwapRendered() {
            Debug.Log("In hand: " + inHand);
            if (inHand) {
                spriteRenderer.sprite = inHand.sprite;
                animatorOverrideController["idle"] = inHand.idleClip;
                animatorOverrideController["action"] = inHand.actionClip;
                inHand.Equip();
                spriteRenderer.enabled = true;
            }
            else {
                spriteRenderer.sprite = null;
                animatorOverrideController["idle"] = null;
                animatorOverrideController["action"] = null;
                spriteRenderer.enabled = false;
            }
        }

        public void UpdateRendered() {
            inHand = playerEquipment.GetEquipment(inHand ? inHand.type : currentWeaponType);
            SwapRendered();
        }

        #region Singleton

        public static HoldingPoint instance;

        private void Awake() {
            if (instance != null) Debug.LogWarning("More than one instance of holding point detected.");
            instance = this;

            spriteRenderer = GetComponent<SpriteRenderer>();
            hitbox = GetComponent<EdgeCollider2D>();
            hitbox.enabled = false;

            // Setup runtime clip changes
            animator = GetComponent<Animator>();
            animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = animatorOverrideController;
            audioSource = GetComponent<AudioSource>();
        }

        #endregion
    }
}