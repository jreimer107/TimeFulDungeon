using System.Collections;
using System.Collections.Generic;
using TimefulDungeon.Core;
using UnityEngine;

namespace TimefulDungeon.Items {
    public class Melee : Weapon {
        public readonly float arc;

        public Melee(MeleeTemplate template) : base(template) {
            arc = template.arc;
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
        
        
        protected float endSwingAngle;
        protected float DeltaAngle => arc * rate * Time.deltaTime;

        public override void Update() {
            base.Update();

            if (!Activated) return;

            holdingPoint.angle -= DeltaAngle;

            var vertexList = new List<Vector2>();
            holdingPoint.SpriteRenderer.sprite.GetPhysicsShape(0, vertexList);
            holdingPoint.Hitbox.SetPath(0, vertexList);
        }

        public override void OnEnable() {
            base.OnEnable();

            // Configure animation speed
            var animationTime = actionClip.length;
            var speedMultiplier = animationTime * rate;
            Debug.Log("Setting speed to " + speedMultiplier);
            holdingPoint.Animator.speed = speedMultiplier;
            holdingPoint.AudioSource.clip = soundEffect;

            // Configure particles
            holdingPoint.Particles.gameObject.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            other.TryGetComponent(out IDamageable damageable);
            damageable?.Damage(damage);
        }

        public override void Activate() {
            base.Activate();
            holdingPoint.Hitbox.enabled = true;
            holdingPoint.ControlledByInHand = true;
        }

        public override void OnActionLoop() {
            base.OnActionLoop();
            holdingPoint.RotateToMouse();
            var halfArc = arc / 2;
            endSwingAngle = holdingPoint.angle - halfArc;
            holdingPoint.angle += halfArc;
        }

        public override void Deactivate() {
            holdingPoint.StartCoroutine(DelayedDeactivate());
        }

        protected IEnumerator DelayedDeactivate() {
            yield return new WaitUntil(() => holdingPoint.angle < endSwingAngle);
            base.Deactivate();
            holdingPoint.ControlledByInHand = false;
            holdingPoint.Hitbox.enabled = false;
        }
    }
}