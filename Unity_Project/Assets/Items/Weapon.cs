using System;
using UnityEngine;

namespace TimefulDungeon.Items {
    public abstract class Weapon : Equippable {
        public float damageMod;
        public float rangeMod;
        public float rateMod;

        protected readonly int damage;
        protected readonly int range;
        protected readonly float rate;

        protected Weapon(WeaponTemplate template) : base(template) {
            damageMod = GetModifier();
            rangeMod = GetModifier();
            rateMod = GetModifier();
            
            damage = (int)(template.damage * damageMod);
            range = (int)(template.range * rangeMod);
            rate = template.rate * rateMod;
        }

        protected override string CalculateTooltipText() {
            return
                GetNameLevelDescription() +
                $"{damage} damage/hit\n" +
                $"{rate.ToString(FloatFormat)} attacks/sec\n" +
                $"{range}m range\n";
        }
    }
}