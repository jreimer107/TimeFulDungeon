using TimefulDungeon.Core;
using UnityEngine;

namespace TimefulDungeon.Items {
    public enum EquipType {
        Melee,
        Ranged,
        Shield
    }

    public class Equippable : Item {
        public EquipType type;
        protected bool activated;
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

        public virtual void Equip() {
            holdingPoint = HoldingPoint.instance;
            activated = false;
        }

        public virtual void Activate() {
            activated = true;
        }

        public virtual void Deactivate() {
            activated = false;
        }

        public virtual bool ControlHoldingPoint() {
            return false;
        }
    }
}