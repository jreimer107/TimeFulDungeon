using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon{
    public enum WeaponType {
		Melee, Ranged, Shield,
	}
	public WeaponType type;
	public Sprite sprite;

	public Weapon(Sprite sprite) {
		this.sprite = sprite;
	}
}

public class Melee : Weapon {
	public int arcDegrees;
	public int range;
	public int speed;

	public Melee(Sprite sprite, int arcDegrees, int range, int speed) : base(sprite) {
		this.sprite = sprite;
		this.arcDegrees = arcDegrees;
		this.range = range;
		this.speed = speed;
		type = WeaponType.Melee;

	}
}

public class Ranged : Weapon {
	public Projectile projectile;
	public int rate;

	public Ranged(Sprite sprite, Projectile projectile, int rate) : base(sprite)  {
		this.sprite = sprite;
		this.projectile = projectile;
		this.rate = rate;
	}
}



public class Projectile : MonoBehaviour {
	public int damage;
	public int speed;
	public Sprite sprite;
	public int penetrate;

	void Start(int damage, int speed, Sprite sprite, int penetrate) {
		this.damage = damage;
		this.speed = speed;
		this.sprite = sprite;
		this.penetrate = penetrate;
	}

	void FixedUpdate() {
		//move projectile and check for enemies in its hitbox
	}
}
