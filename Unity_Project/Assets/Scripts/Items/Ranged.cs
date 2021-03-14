using TimefulDungeon.Core;
using UnityEngine;

namespace TimefulDungeon.Items {
    [CreateAssetMenu(fileName = "New Ranged", menuName = "Interactables/Ranged")]
    public class Ranged : Weapon {
        public Sprite projectile;
        public int speed, penetrate;

        private float shootTimer;

        public Ranged(string name, Sprite sprite, Sprite projectile, int damage, int range, int rate, float cooldown,
            int speed, int penetrate) :
            base(name, sprite, damage, range, rate, cooldown, EquipType.Ranged) {
            this.projectile = projectile;
            this.damage = damage;
            this.rate = rate;
            this.speed = speed;
            this.penetrate = penetrate;
        }

        public void fire() {
            holdingPoint.Particles.Play();
            // return new Projectile(this.damage, this.speed, this.projectile, this.penetrate);
        }

        public override void Equip(HoldingPoint holding) {
            base.Equip(holding);
            if (holdingPoint.Animator.speed < 1) holdingPoint.Animator.speed = 1;
            shootTimer = 0;
            Debug.Log("Swap rendered to ranged.");
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

    public class Projectile : MonoBehaviour {
        public int damage;
        public int speed;
        public Sprite sprite;
        public int penetrate;

        public Projectile(int damage, int speed, Sprite sprite, int penetrate) {
            this.damage = damage;
            this.speed = speed;
            this.sprite = sprite;
            this.penetrate = penetrate;
        }

        private void FixedUpdate() {
            //move projectile and check for enemies in its hitbox
        }
    }
}