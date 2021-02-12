using UnityEngine;

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

	public override void Equip(Animator animator, EdgeCollider2D hitbox) {
		base.Equip(animator, hitbox);
		float animationTime = this.actionClip.length;
		float cycleTime = this.rate / 60f; // Rounds per minute -> seconds
		if (animationTime > cycleTime) {
			Debug.Log("Setting speed to " + animationTime / cycleTime);
			animator.SetFloat("speed", animationTime / cycleTime);
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
