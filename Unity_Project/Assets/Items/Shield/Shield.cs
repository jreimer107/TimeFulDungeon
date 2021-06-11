using System;
using System.Linq;
using TimefulDungeon.Core;
using TimefulDungeon.Misc;
using UnityEngine;

namespace TimefulDungeon.Items.Shield {
    public class Shield : Equippable {
        public readonly float staminaMod;
        public readonly float arcMod;
        public readonly float armorMod;
        public readonly float knockbackMod;
        
        [NonSerialized] public readonly int staminaUse;
        protected readonly int armor;
        protected readonly float arc;
        protected readonly float knockback;

        public Shield(ShieldTemplate template) : base(template) {
            staminaMod = GetModifier();
            arcMod = GetModifier();
            armorMod = GetModifier();
            knockbackMod = GetModifier();
            prefix = new Prefix<Shield>();
            
            staminaUse = (int)(template.staminaUse * staminaMod);
            arc = template.arc * arcMod;
            armor = (int)(template.armor * armorMod * levelScale);
            knockback = template.knockback * knockbackMod;
            prefix.Apply(this);
        }

        public override void OnEnable() {
            base.OnEnable();
            
            var arcCenter = new Vector2(-holdingPoint.radius, 0);
            holdingPoint.Barrier.enabled = true;
            holdingPoint.Barrier.SetPoints(Utils.GetArcPoints(holdingPoint.radius, arc, arcCenter).ToList());
        }

        public override void OnCollision(Collider2D other) {
            base.OnCollision(other);
            Debug.Log("Shield collision");
            other.TryGetComponent<IDamaging>(out var damaging);
            if (damaging != null) {
                ConsumeStaminaOnDamage(damaging.GetDamage());
            }

            other.TryGetComponent<IPushable>(out var pushable);
            pushable?.Push(holdingPoint.Position2D(), knockback);
        }

        public override void OnDisable() {
            base.OnDisable();
            holdingPoint.Barrier.enabled = false;
        }
        
        protected override string CalculateTooltipText() {
            return
                GetNameLevelDescription() +
                $"{armor} armor\n" +
                $"{FormatFloat(arc)}\u00b0 guard\n" +
                $"{staminaUse} stamina/second";
        }

        protected void ConsumeStaminaOnDamage(int damage) {
            // Armor is amount of damage shield would absorb at 100 stamina
            var stamina = Player.instance.Stamina;
            stamina.SingleUse(damage / (float) armor * 100);
        }
    }
    
}