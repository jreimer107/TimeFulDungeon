using UnityEngine;

public class Weapon : Equipment {
    public int damage;
    public int range;
    public float rate;

    public Weapon(string name, Sprite sprite, int damage, int range, int rate, float cooldown, EquipType type) : 
        base(name, 1, "weapon", sprite, cooldown, type) {
        this.damage = damage;
        this.range = range;
        this.rate = rate;
    }

    protected Weapon(Weapon copy): base(copy) {
        this.damage = copy.damage;
        this.range = copy.range;
        this.rate = copy.rate;
    }

    public override void Equip(Animator animator, EdgeCollider2D hitbox) {
        base.Equip(animator, hitbox);
        float animationTime = this.actionClip.length;
		float speedMultiplier = animationTime * rate;
		Debug.Log("Setting speed to " + speedMultiplier);
		animator.speed = speedMultiplier;
    }
}
