using TimefulDungeon.Core;
using UnityEngine;

namespace TimefulDungeon.Items {
    [CreateAssetMenu(fileName = "New Melee", menuName = "Interactables/Melee")]
    public class Melee : Weapon {
        public float arc;

        private float endSwingAngle;

        /// <summary>
        ///     Create a Melee equipment item.
        /// </summary>
        /// <param name="name">The name of the melee weapon.</param>
        /// <param name="sprite">A stationary image of the weapon, will be used for inventory.</param>
        /// <param name="damage">How much damage the weapon does per hit.</param>
        /// <param name="arc">How far the weapon swings from side to side.</param>
        /// <param name="range">How long the weapon is, i.e. its reach.</param>
        /// <param name="rate">How many times per second the weapon can attack.</param>
        /// <param name="cooldown">How long the player must wait in between strikes.</param>
        /// <returns></returns>
        public Melee(string name, Sprite sprite, int damage, float arc, int range, int rate, int cooldown) : base(name,
            sprite, damage, range, rate, cooldown, EquipType.Melee) {
            this.damage = damage;
            this.arc = arc;
            this.range = range;
            this.rate = rate;
            this.cooldown = cooldown;
        }

        protected Melee(Melee copy) : base(copy) {
            damage = copy.damage;
            arc = copy.arc;
            range = copy.range;
            rate = copy.rate;
            cooldown = copy.cooldown;
        }

        public float DeltaAngle => arc * rate * Time.fixedDeltaTime;

        public override void Equip(HoldingPoint holding) {
            base.Equip(holding);
            holdingPoint.Hitbox.points = new[] {new Vector2(0, 0), new Vector2(range, 0)};
        }

        /// <summary>
        ///     Start attacking.
        /// </summary>
        public override void Activate() {
            base.Activate();
            holdingPoint.Hitbox.enabled = true;
        }

        protected override void ActionStart() {
            base.ActionStart();
            holdingPoint.RotateToMouse();
            endSwingAngle = holdingPoint.angle - arc / 2;
            holdingPoint.angle += arc / 2;
        }

        protected override bool CheckIfActionDone() {
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