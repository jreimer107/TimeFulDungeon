using UnityEngine;

namespace TimefulDungeon {
	[CreateAssetMenu(fileName = "New Melee", menuName = "Interactables/Melee")]
	public class Melee : Weapon {
		public float arc;

		private float endSwingAngle;
	
		/// <summary>
		/// Create a Melee equipment item.
		/// </summary>
		/// <param name="name">The name of the melee weapon.</param>
		/// <param name="sprite">A stationary image of the weapon, will be used for inventory.</param>
		/// <param name="damage">How much damage the weapon does per hit.</param>
		/// <param name="arc">How far the weapon swings from side to side.</param>
		/// <param name="range">How long the weapon is, i.e. its reach.</param>
		/// <param name="rate">How many times per second the weapon can attack.</param>
		/// <param name="cooldown">How long the player must wait in between strikes.</param>
		/// <returns></returns>
		public Melee(string name, Sprite sprite, int damage, float arc, int range, int rate, int cooldown) : base(name, sprite, damage, range, rate, cooldown, EquipType.Melee) {
			this.damage = damage;
			this.arc = arc;
			this.range = range;
			this.rate = rate;
			this.cooldown = cooldown;
		}

		protected Melee(Melee copy) : base(copy) {
			this.damage = copy.damage;
			this.arc = copy.arc;
			this.range = copy.range;
			this.rate = copy.rate;
			this.cooldown = copy.cooldown;
		}

		public override void Equip() {
			base.Equip();
			holdingPoint.hitbox.points = new Vector2[] { new Vector2(0, 0), new Vector2(this.range, 0) };
		}

		public float DeltaAngle {
			get {
				return this.arc * this.rate * Time.fixedDeltaTime;
			}
		}

		/// <summary>
		/// Start attacking.
		/// </summary>
		public override void Activate() {
			base.Activate();
			holdingPoint.hitbox.enabled = true;
		}

		public override void ActionStart() {
			base.ActionStart();
			holdingPoint.RotateToMouse();
			endSwingAngle = holdingPoint.angle - arc / 2;
			holdingPoint.angle += arc / 2;
		}

		public override bool CheckIfActionDone() {
			holdingPoint.angle -= DeltaAngle;
			return holdingPoint.angle < endSwingAngle;
		}

		public override string GetTooltipText() {
			return
				$"<size=32>{name}</size>\n" +
				(description != "" ? $"{description}\n" : "") +
				$"{damage} dmg\n" +
				$"{arc}\u00b0 arc\n" +
				$"{rate}\u00b0/sec\n" +
				$"{range}m range\n" +
				$"{cooldown}s cooldown\n" + 
				(redText != "" ? $"<color=red>{redText}</color>" : "");
		}
	}
}