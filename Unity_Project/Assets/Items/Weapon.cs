namespace TimefulDungeon.Items {
    public abstract class Weapon : Equippable {
        public readonly int damage;
        public readonly int range;
        public readonly float rate;

        public Weapon(WeaponTemplate template) : base(template) {
            damage = template.damage;
            range = template.range;
            rate = template.rate;

        }

    }
}