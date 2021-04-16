using TimefulDungeon.Misc;
using UnityEngine;
using Random = System.Random;

namespace TimefulDungeon.Items.Ranged {
    public class Ranged : Weapon {
        public readonly float speedMod;
        public readonly float penetrateMod;
        public readonly float spreadMod;
        public readonly Sprite projectile;
        
        
        protected readonly float speed;
        protected readonly int penetrate;
        protected readonly float spread;

        public Ranged(RangedTemplate template) : base(template) {
            speedMod = GetModifier();
            penetrateMod = GetModifier();
            spreadMod =  GetModifier();
            projectile = template.projectile;
            prefix = new Prefix<Ranged>();
            
            
            speed = template.speed * speedMod;
            penetrate = (int)(template.penetrate * penetrateMod);
            spread = template.spread * spreadMod;
            prefix.Apply(this);
        }
        
        public override void OnEnable() {
            base.OnEnable();

            if (holdingPoint.Animator.speed < 1) holdingPoint.Animator.speed = 1;
            holdingPoint.Bullet.damage = damage;
            holdingPoint.Particles.gameObject.SetActive(true);

            // Particle system properties are faux-read-only. You can't edit them directly, but storing them in a
            // variable and modifying that variable works for some reason.
            var shape = holdingPoint.Particles.shape;
            shape.arc = spread;
            shape.rotation = new Vector3(0, 0, -spread / 2);

            var bulletSpawnPoint = (sprite.rect.width - sprite.pivot.x) / sprite.pixelsPerUnit;
            shape.position = new Vector2(bulletSpawnPoint, 0);
        }

        public override void OnActionLoop() {
            holdingPoint.Particles.Play();
        }

        protected override string CalculateTooltipText() {
            return
                base.CalculateTooltipText() +
                $"{FormatFloat(speed)} m/s\n" +
                $"{FormatFloat(spread)}\u00b0 spread\n" +
                (penetrate > 1
                    ? $"Penetrates {penetrate} enemies\n"
                    : "");
        }
    }
}