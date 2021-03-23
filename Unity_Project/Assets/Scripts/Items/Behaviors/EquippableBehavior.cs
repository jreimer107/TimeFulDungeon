using System;
using TimefulDungeon.Core;
using UnityEngine;

namespace TimefulDungeon.Items.Behaviors {
    public abstract class EquippableBehavior : MonoBehaviour, IComparable<EquippableBehavior> {
        protected HoldingPoint holdingPoint;
        protected Inventory inventory;
        protected Equippable that;
        public bool Activated { get; private set; }
        public EquipType Type { get; protected set; }

        private void Start() {
            var player = Player.instance;
            holdingPoint = player.HoldingPoint;
            inventory = player.Inventory;
            enabled = false;
        }

        protected virtual void Update() { }

        protected virtual void OnEnable() {
            if (!that) return;
            if (that.idleClip && that.actionClip) {
                holdingPoint.AnimatorOverrideController["idle"] = that.idleClip;
                holdingPoint.AnimatorOverrideController["action"] = that.actionClip;
                holdingPoint.Animator.enabled = true;
            }
            else {
                holdingPoint.Animator.enabled = false;
            }

            holdingPoint.SpriteRenderer.enabled = true;
        }

        public int CompareTo(EquippableBehavior other) {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Type.CompareTo(other.Type);
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