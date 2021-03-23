using UnityEngine;

namespace TimefulDungeon.Items.Behaviors {
    public class RangedBehavior : EquippableBehavior {
        protected Ranged data {
            get => that as Ranged;
            set => that = value;
        }

        private void Awake() {
            Type = EquipType.Ranged;
        }

        protected override void OnEnable() {
            data = inventory.Ranged;

            base.OnEnable();

            if (holdingPoint.Animator.speed < 1) holdingPoint.Animator.speed = 1;
            holdingPoint.Bullet.damage = data.damage;
            holdingPoint.Particles.gameObject.SetActive(true);

            // Particle system properties are faux-read-only. You can't edit them directly, but storing them in a
            // variable and modifying that variable works for some reason.
            var shape = holdingPoint.Particles.shape;
            shape.arc = data.spread;
            shape.rotation = new Vector3(0, 0, -data.spread / 2);

            var bulletSpawnPoint = (data.sprite.rect.width - data.sprite.pivot.x) / data.sprite.pixelsPerUnit;
            shape.position = new Vector2(bulletSpawnPoint, 0);
        }

        public override void OnActionLoop() {
            holdingPoint.Particles.Play();
        }
    }
}