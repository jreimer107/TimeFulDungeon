using UnityEngine;

[CreateAssetMenu(fileName = "New Melee", menuName = "Interactables/Melee")]
public class Melee : Equipment {
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

	protected Melee(Melee copy) : base(copy) {
		this.damage = copy.damage;
		this.arc = copy.arc;
		this.range = copy.range;
		this.speed = copy.speed;
		this.cooldown = copy.cooldown;
	}

    public override void Equip(Animator animator, EdgeCollider2D hitbox) {
        base.Equip(animator, hitbox);
		hitbox.points = new Vector2[] { new Vector2(0, 0), new Vector2(this.range, 0) };
		float animationTime = this.actionClip.length;
		float numUpdates = this.arc / this.speed;
		float moveTime = numUpdates * Time.fixedDeltaTime;
		float speedMultiplier = animationTime / moveTime;
		Debug.Log("Setting speed to " + speedMultiplier);
		animator.SetFloat("speed", speedMultiplier);
    }

    

	public override string GetTooltipText() {
		return
			$"<size=32>{name}</size>\n" +
			(description != "" ? $"{description}\n" : "") +
			$"{damage} dmg\n" +
			$"{arc}\u00b0 arc\n" +
			$"{speed}\u00b0/sec\n" +
			$"{range}m range\n" +
			$"{cooldown}s cooldown\n" + 
			(redText != "" ? $"<color=red>{redText}</color>" : "");
	}
}