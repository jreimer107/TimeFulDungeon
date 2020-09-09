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

	public override string GetTooltipText() {
		return $"<size=32>{name}</size>\n{description}\n{damage} dmg\n{arc} degrees\n{speed} angle/sec\n{range}m range\n{cooldown}s cooldown<color=red>{redText}</color>";
	}
}