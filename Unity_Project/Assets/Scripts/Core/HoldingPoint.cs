using System;
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
    public class HoldingPoint : FiniteStateMachine<EquipType> {
        // Configuration
        [Range(0, 5)] [SerializeField] private float radius = 1;
        
        // State controlled by currently held item
        [HideInInspector] public float angle;
        public bool ControlledByInHand { get; private set; }
        
        // Dependencies
        private EventSystem _eventSystem;
        public Equippable InHand { get; private set; }
        private Inventory _playerEquipment;
        private Stamina _playerStamina;
        private Transform _playerTransform;
        
        // State
        private EquipType _prevType = EquipType.None;
        private bool _shouldActivate;
        private bool _shouldShield;
        
        // Components, configured by currently held item
        public SpriteRenderer SpriteRenderer { get; private set; }
        public EdgeCollider2D Hitbox { get; private set; }
        public Animator Animator { get; private set; }
        public AnimatorOverrideController AnimatorOverrideController { get; private set; }
        public AudioSource AudioSource { get; private set; }
        public ParticleSystem Particles { get; private set; }
        public BulletParticle Bullet { get; private set; }
        
        // Shorthands
        public EquipType CurrType => InHand ? InHand.type : EquipType.None;
        private bool IsShielding => CurrType == EquipType.Shield;
        private bool IsActive => InHand && InHand.Activated;

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
            
            Initialize(EquipType.Melee);
        }

        private void Start() {
            var player = Player.instance;
            _playerEquipment = player.Inventory;
            _playerStamina = player.Stamina;
            _playerTransform = player.transform;
            _eventSystem = EventSystem.current;
            // SwapTo(EquipType.Melee);
        }

        protected override void Update() {
            base.Update();
            // if (Input.GetButtonDown("Swap Weapon")) ToggleWeapon();
            //
            // if (Input.GetButtonDown("Shield")) _shouldShield = true;
            // else if (Input.GetButtonUp("Shield")) _shouldShield = false;
            //
            // if (Input.GetButtonDown("Fire1") && !_eventSystem.IsPointerOverGameObject()) _shouldActivate = true;
            // else if (Input.GetButtonUp("Fire1")) _shouldActivate = false;
            //
            // // If not attacking, check if we should shield/unshield
            // if (!_controlledByInHand)
            //     switch (_shouldShield) {
            //         case true when !IsShielding:
            //             Shield();
            //             break;
            //         case false when IsShielding:
            //             Unshield();
            //             break;
            //     }

            // Check for weapon use
            if (!InHand) return;
            switch (_shouldActivate) {
                case true when !IsActive:
                    InHand.Activate();
                    break;
                case false when IsActive:
                    InHand.Deactivate();
                    break;
            }
        }

        protected override void FixedUpdate() {
            base.FixedUpdate();
            // If we have a weapon, see if it is controlling our position
            if (InHand) ControlledByInHand = InHand.ControlHoldingPoint();

            // If we're not being controlled, freely rotate
            if (!ControlledByInHand) RotateToMouse();

            SetPosition();
        }

        private void OnTriggerEnter2D(Collider2D other) {
            other.TryGetComponent(out IDamageable damageable);
            damageable?.Damage(((Weapon) InHand).damage);
        }
        
        public override bool Transition(EquipType toState) {
            var didTransition = base.Transition(toState);
            if (!didTransition) return false;

            InHand = _playerEquipment.GetEquipment(currentState.Name);
            if (InHand) {
                InHand.Equip(this);
            }
            else {
                ResetComponents();
            }
            return true;
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
            var flip = (CurrType != EquipType.Melee || !ControlledByInHand) && x < 0;
            SpriteRenderer.flipY = flip;
            SpriteRenderer.sortingOrder = flip ? -1 : 1;
        }

        /// <summary>
        ///     Rotates the hand object to be between the player and the mouse pointer.
        /// </summary>
        public void RotateToMouse() {
            var mousePos = Utils.GetMouseWorldPosition2D() - _playerTransform.Position2D();

            //Determine angle of mouse
            angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
            angle = angle < 0 ? angle + 360 : angle;
        }

        // /// <summary>
        // ///     Toggles the current weapon from Melee to Ranged or vice versa.
        // /// </summary>
        // private void ToggleWeapon() {
        //     if (IsShielding) return;
        //     SwapTo(CurrType == EquipType.Melee ? EquipType.Ranged : EquipType.Melee);
        // }
        //
        // private void Shield() {
        //     if (_playerStamina.Exhausted) {
        //         print("Player exhausted, cannot shield.");
        //         return;
        //     }
        //
        //     SwapTo(EquipType.Shield);
        //     if (_inHand) _playerStamina.StartContinuousUse(((Shield) _inHand).staminaUse);
        // }
        //
        // private void Unshield() {
        //     _playerStamina.StopContinuousUse(((Shield) _inHand).staminaUse);
        //     SwapTo(_prevType, true);
        // }
        //
        // public void OnStaminaEmpty() {
        //     SwapTo(_prevType, true);
        // }
        //
        // /// <summary>
        // ///     Event listener for when the equipment changes. If the changed equipment is what we're holding,
        // ///     reloads the held object.
        // /// </summary>
        // /// <param name="type">Which equipment was updated.</param>
        // public void OnEquipmentChange(EquipType type) {
        //     if (type == CurrType || !_inHand && type != EquipType.Shield) SwapTo(type, true);
        // }
        //
        // /// <summary>
        // ///     Swaps the currently rendered item to the in hand item.
        // /// </summary>
        // private void SwapTo(EquipType type, bool resetOnNull = false) {
        //     var newInHand = _playerEquipment.GetEquipment(type);
        //     if (!newInHand) {
        //         print($"No {type} equipped.");
        //         if (resetOnNull) ResetComponents();
        //         else return;
        //     }
        //
        //     _prevType = CurrType;
        //     _inHand = newInHand;
        //
        //     if (!_inHand) return;
        //     _inHand.Equip(this);
        // }
    }
}