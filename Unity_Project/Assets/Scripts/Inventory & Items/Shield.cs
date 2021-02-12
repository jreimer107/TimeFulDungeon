using UnityEngine;

[CreateAssetMenu(fileName = "New Shield", menuName = "Interactables/Shield")]
public class Shield : Equipment {
	public int staminaUse;
	public float arc;

	public Shield(string name, Sprite sprite, int staminaUse, float arc) : base(name, 3, "random shield", sprite, EquipType.Shield) {
		this.staminaUse = staminaUse;
		this.arc = arc;
	}
}