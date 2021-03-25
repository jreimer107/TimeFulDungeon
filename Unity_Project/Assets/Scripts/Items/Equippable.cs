﻿using TimefulDungeon.Core;

namespace TimefulDungeon.Items {
    public abstract class Equippable : Item {
        public EquipType type => ((EquippableTemplate) template).type;

        public Equippable(EquippableTemplate template) : base(template) {
            var player = Player.instance;
            holdingPoint = player.HoldingPoint;
            _inventory = player.Inventory;
        }

        public override void Select() { }

        public override void Use() {
            base.Use();
            _inventory.Equip(this);
        }
        
        protected HoldingPoint holdingPoint;
        private Inventory _inventory;
        
        public bool Activated { get; private set; }

        public virtual void Update() { }

        public virtual void OnEnable() {
            if (idleClip && actionClip) {
                holdingPoint.AnimatorOverrideController["idle"] = idleClip;
                holdingPoint.AnimatorOverrideController["action"] = actionClip;
                holdingPoint.Animator.enabled = true;
            }
            else {
                holdingPoint.Animator.enabled = false;
            }

            holdingPoint.SpriteRenderer.enabled = true;
        }

        public virtual void Activate() {
            Activated = true;
        }

        public virtual void Deactivate() {
            Activated = false;
        }

        public virtual void OnActionLoop() { }
    }
}