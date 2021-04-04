using System;
using TimefulDungeon.Core;
using TimefulDungeon.Misc;
using UnityEngine;

namespace TimefulDungeon.Items {
    public abstract class Equippable : Item {
        private readonly Inventory _inventory;

        protected readonly HoldingPoint holdingPoint;
        public int level;
        protected Enum prefix;

        protected Equippable(EquippableTemplate template) : base(template) {
            var player = Player.instance;
            holdingPoint = player.HoldingPoint;
            _inventory = player.Inventory;
        }

        public EquipType type => ((EquippableTemplate) template).type;

        public bool Activated { get; private set; }

        public override void Select() { }

        public override void Use() {
            base.Use();
            _inventory.Equip(this);
        }

        public virtual void Update() { }

        public virtual void OnEnable() {
            if (idleClip && actionClip) {
                holdingPoint.AnimatorOverrideController["idle"] = idleClip;
                holdingPoint.AnimatorOverrideController["action"] = actionClip;
                holdingPoint.Animator.enabled = true;
            }
            else {
                holdingPoint.Animator.enabled = false;
                holdingPoint.SpriteRenderer.sprite = sprite;
            }

            holdingPoint.SpriteRenderer.enabled = true;
        }

        public virtual void OnDisable() { }

        public virtual void Activate() {
            Activated = true;
        }

        public virtual void Deactivate() {
            Activated = false;
        }

        public virtual void OnActionLoop() { }

        public virtual void OnCollision(Collider2D other) { }

        protected string GetNameLevelDescription() {
            return
                $"<size=32>{Translations.Get(prefix) + " " + Translations.Get(name)}</size>\n" +
                $"Lv. {level}\n" +
                (description != "" ? $"{Translations.Get(description)}\n" : "");
        }
    }
}