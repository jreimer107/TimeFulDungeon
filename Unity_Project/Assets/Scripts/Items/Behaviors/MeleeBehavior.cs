using System.Collections;
using System.Collections.Generic;
using TimefulDungeon.Core;
using UnityEngine;

namespace TimefulDungeon.Items.Behaviors {
    public class MeleeBehavior : EquippableBehavior {
        protected float endSwingAngle;
        protected float DeltaAngle => data.arc * data.rate * Time.deltaTime;

        protected Melee data {
            get => that as Melee;
            set => that = value;
        }

        private void Awake() {
            Type = EquipType.Melee;
        }

        protected override void Update() {
            base.Update();

            if (!Activated) return;

            holdingPoint.angle -= DeltaAngle;

            var vertexList = new List<Vector2>();
            holdingPoint.SpriteRenderer.sprite.GetPhysicsShape(0, vertexList);
            holdingPoint.Hitbox.SetPath(0, vertexList);
        }

        protected override void OnEnable() {
            data = inventory.Melee;

            base.OnEnable();

            // Configure animation speed
            var animationTime = data.actionClip.length;
            var speedMultiplier = animationTime * data.rate;
            Debug.Log("Setting speed to " + speedMultiplier);
            holdingPoint.Animator.speed = speedMultiplier;
            holdingPoint.AudioSource.clip = data.soundEffect;

            // Configure particles
            holdingPoint.Particles.gameObject.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            other.TryGetComponent(out IDamageable damageable);
            damageable?.Damage(data.damage);
        }

        public override void Activate() {
            base.Activate();
            holdingPoint.Hitbox.enabled = true;
            holdingPoint.ControlledByInHand = true;
        }

        public override void OnActionLoop() {
            base.OnActionLoop();
            holdingPoint.RotateToMouse();
            var halfArc = data.arc / 2;
            endSwingAngle = holdingPoint.angle - halfArc;
            holdingPoint.angle += halfArc;
        }

        public override void Deactivate() {
            StartCoroutine(DelayedDeactivate());
        }

        protected IEnumerator DelayedDeactivate() {
            yield return new WaitUntil(() => holdingPoint.angle < endSwingAngle);
            base.Deactivate();
            holdingPoint.ControlledByInHand = false;
            holdingPoint.Hitbox.enabled = false;
        }
    }
}