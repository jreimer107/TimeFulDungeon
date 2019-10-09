using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon {
	public enum WeaponType {
		Melee, Ranged, Shield,
	}
	public WeaponType type;
	public Sprite sprite;
	public string name;

	public Weapon(string name, Sprite sprite, WeaponType type) {
		this.sprite = sprite;
		this.name = name;
	}
}

public class Melee : Weapon {
	public float arc;
	public int damage;
	public int range;
	public int speed;
	public int cooldown;

	public Melee(string name, Sprite sprite, int damage, float arc, int range, int speed, int cooldown) : base(name, sprite, WeaponType.Melee) {
		this.damage = damage;
		this.arc = arc;
		this.range = range;
		this.speed = speed;
		this.cooldown = cooldown;
	}
}

public class Ranged : Weapon {
	public Sprite projectile;
	public int damage, rate, speed, penetrate;

	public Ranged(string name, Sprite sprite, Sprite projectile, int damage, int rate, int speed, int penetrate) : base(name, sprite, WeaponType.Ranged) {
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

public class Shield : Weapon {
	public int staminaUse;
	public float arc;

	public Shield(string name, Sprite sprite, int staminaUse, float arc) : base(name, sprite, WeaponType.Shield) {
		this.staminaUse = staminaUse;
		this.arc = arc;
	}
}