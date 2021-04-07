using System;
using TimefulDungeon.Core.FiniteStateMachine;
using TimefulDungeon.Entities;
using TimefulDungeon.Items;
using TimefulDungeon.Misc;
using UnityEngine;
using VoraUtils;

namespace TimefulDungeon.Core {
    /// <summary>
    ///     The point which in-hand items will be attached to.
    ///     This is a controls object that changes the values in the player object.
    /// </summary>
    public class HoldingPoint : FiniteStateMachineBehavior<EquipType> {
        // Configuration
        [Range(0, 5)] public float radius = 1;

        // State controlled by currently held item
        [NonSerialized] public float angle;
        private Transform _playerTransform;
        private Inventory _playerInventory;
        private static readonly int Action = Animator.StringToHash("action");

        // State
        public bool ControlledByInHand { get; set; }

        // Dependencies

        private Equippable InHand { get; set; }

        // Components, configured by currently held item
        public SpriteRenderer SpriteRenderer { get; private set; }
        public PolygonCollider2D Hitbox { get; private set; }
        public EdgeCollider2D Barrier { get; private set; }
        public Animator Animator { get; private set; }
        public AnimatorOverrideController AnimatorOverrideController { get; private set; }
        public AudioSource AudioSource { get; private set; }
        public ParticleSystem Particles { get; private set; }
        public BulletParticle Bullet { get; private set; }

        // Shorthands
        public EquipType CurrType => InHand ? InHand.type : EquipType.None;
        private bool IsActive => InHand != null && InHand.Activated;

        protected void Awake() {
            SpriteRenderer = GetComponent<SpriteRenderer>();
            Hitbox = GetComponent<PolygonCollider2D>();
            Hitbox.enabled = false;
            Barrier = GetComponent<EdgeCollider2D>();
            Barrier.enabled = false;
            Particles = GetComponentInChildren<ParticleSystem>();
            Bullet = Particles.GetComponent<BulletParticle>();
            Particles.gameObject.SetActive(false);
            Animator = GetComponent<Animator>();
            AnimatorOverrideController = new AnimatorOverrideController(Animator.runtimeAnimatorController);
            Animator.runtimeAnimatorController = AnimatorOverrideController;
            AudioSource = GetComponent<AudioSource>();
        }

        private void Start() {
            var player = Player.instance;
            _playerTransform = player.transform;
            _playerInventory = player.Inventory;

            Initialize(_playerInventory.Melee ? EquipType.Melee : EquipType.None);
            InHand = _playerInventory.GetEquipment(currentState.Name);
        }

        public void OnActionStart() {
            AudioSource.Play();
            InHand?.OnActionLoop();
        }

        protected override void Update() {
            base.Update();

            if (!InHand) return;
            
            InHand.Update();
            
            // Check for weapon use
            switch (Input.GetButton("Fire1")) {
                case true when !IsActive:
                    Animator.SetBool(Action, true);
                    InHand.Activate();
                    break;
                case false when IsActive:
                    Animator.SetBool(Action, false);
                    InHand.Deactivate();
                    break;
            }
        }

        private void FixedUpdate() {
            // If we're not being controlled, freely rotate
            if (!ControlledByInHand) RotateToMouse();

            SetPosition();
        }

        private void OnTriggerEnter2D(Collider2D other) {
            InHand?.OnCollision(other);
        }

        public void OnEquipmentChange(EquipType changedType) {
            if (changedType == CurrType || changedType != EquipType.Shield && CurrType == EquipType.None) {
                Transition(changedType);
            }
        }
        
        protected override void OnTransition() {
            InHand?.OnDisable();
            InHand = _playerInventory.GetEquipment(currentState.Name);
            if (InHand) {
                InHand.OnEnable();
            }
            else {
                ResetComponents();
            }
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

        private void ResetComponents() {
            SpriteRenderer.sprite = null;
            SpriteRenderer.enabled = false;
            AnimatorOverrideController["idle"] = null;
            AnimatorOverrideController["action"] = null;
        }
    }
}