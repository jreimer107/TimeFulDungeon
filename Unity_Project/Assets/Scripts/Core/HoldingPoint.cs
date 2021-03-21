using TimefulDungeon.Entities;
using TimefulDungeon.Items;
using UnityEngine;
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
        private Inventory _playerEquipment;
        private Transform _playerTransform;

        // State
        public bool ControlledByInHand { get; private set; }

        // Dependencies
        private Equippable InHand { get; set; }

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
            _playerTransform = player.transform;
        }

        protected override void Update() {
            base.Update();
            
            // Check for weapon use
            if (!InHand) return;
            switch (Input.GetButton("Fire1")) {
                case true when !IsActive:
                    InHand.Activate();
                    break;
                case false when IsActive:
                    InHand.Deactivate();
                    break;
            }
        }

        private void FixedUpdate() {
            // If we have a weapon, see if it is controlling our position
            if (InHand) ControlledByInHand = InHand.ControlHoldingPoint();

            // If we're not being controlled, freely rotate
            if (!ControlledByInHand) RotateToMouse();

            SetPosition();
        }

        public void OnEquipmentChange(EquipType changedType) {
            if (changedType == CurrType || changedType != EquipType.Shield && CurrType == EquipType.None) {
                Transition(changedType);
            }
        }

        private void OnTriggerEnter2D(Collider2D other) {
            other.TryGetComponent(out IDamageable damageable);
            damageable?.Damage(((Weapon) InHand).damage);
        }

        protected override void OnTransition() {
            InHand = _playerEquipment.GetEquipment(currentState.Name);
            if (InHand) { 
                InHand.Equip(this);
            }
            else {
                ResetComponents();
            }
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
    }
}