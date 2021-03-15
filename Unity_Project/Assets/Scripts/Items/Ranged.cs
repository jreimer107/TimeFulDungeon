using TimefulDungeon.Core;
using UnityEngine;

namespace TimefulDungeon.Items {
    [CreateAssetMenu(fileName = "New Ranged", menuName = "Interactables/Ranged")]
    public class Ranged : Weapon {
        public Sprite projectile;
        public float speed;
        public int penetrate;
        public float spread;

        private float shootTimer;

        public Ranged(string name, Sprite sprite, Sprite projectile, int damage, int range, int rate, float cooldown,
            float speed, int penetrate) :
            base(name, sprite, damage, range, rate, cooldown, EquipType.Ranged) {
            this.projectile = projectile;
            this.damage = damage;
            this.rate = rate;
            this.speed = speed;
            this.penetrate = penetrate;
        }

        private void fire() {
            holdingPoint.Particles.Play();
            // return new Projectile(this.damage, this.speed, this.projectile, this.penetrate);
        }

        public override void Equip(HoldingPoint holding) {
            base.Equip(holding);
            if (holdingPoint.Animator.speed < 1) holdingPoint.Animator.speed = 1;
            shootTimer = 0;
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

        public override bool ControlHoldingPoint() {
            base.ControlHoldingPoint();
            return false;
        }

        protected override void ActionStart() {
            base.ActionStart();
            shootTimer = 0;
            fire();
        }

        protected override bool CheckIfActionDone() {
            shootTimer += Time.fixedDeltaTime;
            return shootTimer > 1f / rate;
        }
    }
}