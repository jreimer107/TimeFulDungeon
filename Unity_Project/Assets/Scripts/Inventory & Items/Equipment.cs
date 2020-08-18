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

	protected Equipment(Equipment copy) : base(copy) {
		this.type = copy.type;
	}

	public override Item Clone() => new Equipment(this);
}

[CreateAssetMenu(fileName = "New Ranged", menuName = "Interactables/Ranged")]
public class Ranged : Equipment {
	public Sprite projectile;
	public int damage, rate, speed, penetrate;

	public Ranged(string name, Sprite sprite, Sprite projectile, int damage, int rate, int speed, int penetrate) : base(name, 2, "random ranged", sprite, EquipType.Ranged) {
		this.projectile = projectile;
		this.damage = damage;
		this.rate = rate;
		this.speed = speed;
		this.penetrate = penetrate;
	}

	public Projectile fire() {
		return new Projectile(this.damage, this.speed, this.projectile, this.penetrate);
	}
}

public class Projectile : MonoBehaviour {
	public int damage;
	public int speed;
	public Sprite sprite;
	public int penetrate;

	public Projectile(int damage, int speed, Sprite sprite, int penetrate) {
		this.damage = damage;
		this.speed = speed;
		this.sprite = sprite;
		this.penetrate = penetrate;
	}

	void FixedUpdate() {
		//move projectile and check for enemies in its hitbox
	}
}

[CreateAssetMenu(fileName = "New Shield", menuName = "Interactables/Shield")]
public class Shield : Equipment {
	public int staminaUse;
	public float arc;

	public Shield(string name, Sprite sprite, int staminaUse, float arc) : base(name, 3, "random shield", sprite, EquipType.Shield) {
		this.staminaUse = staminaUse;
		this.arc = arc;
	}
}