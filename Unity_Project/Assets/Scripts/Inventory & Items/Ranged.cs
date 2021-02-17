using UnityEngine;

[CreateAssetMenu(fileName = "New Ranged", menuName = "Interactables/Ranged")]
public class Ranged : Weapon {
	public Sprite projectile;
	public int speed, penetrate;

	public Ranged(string name, Sprite sprite, Sprite projectile, int damage, int range, int rate, float cooldown, int speed, int penetrate) : 
		base(name, sprite, damage, range, rate, cooldown, EquipType.Ranged) {
		this.projectile = projectile;
		this.damage = damage;
		this.rate = rate;
		this.speed = speed;
		this.penetrate = penetrate;
	}

	public Projectile fire() {
		return new Projectile(this.damage, this.speed, this.projectile, this.penetrate);
	}

	public override void Equip(Animator animator, EdgeCollider2D hitbox) {
		base.Equip(animator, hitbox);
		if (animator.speed > 1) {
			animator.speed = 1;
		}
		Debug.Log("Swap rendered to ranged.");
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
