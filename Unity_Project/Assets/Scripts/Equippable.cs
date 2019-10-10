﻿using UnityEngine;

public class Equippable : Item {
    public enum EquipType {
        Melee, Ranged, Shield,
    }
    public EquipType type;

    public Equippable(string name, int ID, string description, Sprite sprite, EquipType type) : base(name, ID, description, sprite, false, 1) {
        this.type = type;
    }

    public override void Select() { }
    public override void Use() { }
}

public class Melee : Equippable {
    public float arc;
    public int damage;
    public int range;
    public int speed;
    public int cooldown;

    public Melee(string name, Sprite sprite, int damage, float arc, int range, int speed, int cooldown) : base(name, 1, "random melee", sprite, EquipType.Melee) {
        this.damage = damage;
        this.arc = arc;
        this.range = range;
        this.speed = speed;
        this.cooldown = cooldown;
    }
}

public class Ranged : Equippable {
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

public class Shield : Equippable {
    public int staminaUse;
    public float arc;

    public Shield(string name, Sprite sprite, int staminaUse, float arc) : base(name, 3, "random shield", sprite, EquipType.Shield) {
        this.staminaUse = staminaUse;
        this.arc = arc;
    }
}