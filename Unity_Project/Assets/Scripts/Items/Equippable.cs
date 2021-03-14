using TimefulDungeon.Core;
using UnityEngine;

namespace TimefulDungeon.Items {
    public class Equippable : Item {
        public EquipType type;
        public bool Activated { get; private set; }
        protected HoldingPoint holdingPoint;

        public Equippable(string name, int id, string description, Sprite sprite, float cooldown, EquipType type) : base(
            name, id, description, sprite, false, 1, cooldown, false) {
            this.type = type;
        }

        protected Equippable(Equippable copy) : base(copy) {
            type = copy.type;
        }

        public virtual Equippable Clone() {
            return CreateInstance<Equippable>();
        }

        public override void Select() { }

        public override void Use() {
            base.Use();
            Player.instance.Inventory.Equip(this);
        }

        public virtual void Equip(HoldingPoint holding) {
            holdingPoint = holding;
            Activated = false;
            
            holdingPoint.SpriteRenderer.sprite = sprite;
            if (idleClip && actionClip) {
                holdingPoint.AnimatorOverrideController["idle"] = idleClip;
                holdingPoint.AnimatorOverrideController["action"] = actionClip;
                holdingPoint.Animator.enabled = true;
            }
            else {
                holdingPoint.Animator.enabled = false;
            }
            holdingPoint.SpriteRenderer.enabled = true;        }

        public virtual void Activate() {
            Activated = true;
        }

        public virtual void Deactivate() {
            Activated = false;
        }

        public virtual bool ControlHoldingPoint() {
            return false;
        }
    }
}