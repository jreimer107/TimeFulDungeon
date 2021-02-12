using UnityEngine;

public enum EquipType { Melee, Ranged, Shield }

public class Equipment : Item {

	public EquipType type;

	public Equipment(string name, int ID, string description, Sprite sprite, EquipType type) : base(name, ID, description, sprite, false, 1, false) {
		this.type = type;
	}

	public override void Select() { }
	public override void Use() {
		base.Use();
		EquipmentManager.instance.Equip(this);
	}

	public virtual void Equip(Animator animator, EdgeCollider2D hitbox) { }

	protected Equipment(Equipment copy) : base(copy) {
		this.type = copy.type;
	}

	public override Item Clone() => new Equipment(this);
}