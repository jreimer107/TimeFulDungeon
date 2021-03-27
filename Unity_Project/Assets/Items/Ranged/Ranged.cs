using UnityEngine;

namespace TimefulDungeon.Items {
    public class Ranged : Weapon {
        public readonly Sprite projectile;
        public readonly float speed;
        public readonly int penetrate;
        public readonly float spread;

        public Ranged(RangedTemplate template) : base(template) {
            projectile = template.projectile;
            speed = template.speed;
            penetrate = template.penetrate;
            spread = template.spread;
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
    }
}