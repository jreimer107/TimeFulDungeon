using TimefulDungeon.Core;
using UnityEngine;

namespace TimefulDungeon {
	public enum EquipType { Melee, Ranged, Shield }

	public class Equipment : Item {

		public EquipType type;
		protected HoldingPoint holdingPoint;
		protected bool activated;

		public Equipment(string name, int ID, string description, Sprite sprite, float cooldown, EquipType type) : base(name, ID, description, sprite, false, 1,  cooldown, false) {
			this.type = type;
		}

		protected Equipment(Equipment copy) : base(copy) {
			this.type = copy.type;
		}
		public override Item Clone() => new Equipment(this);

		public override void Select() { }
		public override void Use() {
			base.Use();
			EquipmentManager.instance.Equip(this);
		}

		public virtual void Equip() {
			this.holdingPoint = HoldingPoint.instance;
			activated = false;
		}

		public virtual void Activate() {
			this.activated = true;
		}
		public virtual void Deactivate() {
			this.activated = false;
		}

		public virtual bool ControlHoldingPoint() {
			return false;
		}

	}
}