using System.Collections;
using System.Collections.Generic;
using TimefulDungeon.Core;
using UnityEngine;

namespace TimefulDungeon.Items.Melee {
    public class Melee : Weapon {
        public readonly float arcMod;
        
        protected readonly float arc;

        public Melee(MeleeTemplate template) : base(template) {
            arcMod = GetModifier();
            
            arc = template.arc * arcMod;
            prefix = new Prefix<Melee>();
            prefix.Apply(this);

        }

        protected override string CalculateTooltipText() {
            return
                GetNameLevelDescription() +
                $"{damage} damage/hit\n" +
                $"{FormatFloat(arc)}\u00b0 arc\n" +
                $"{FormatFloat(rate)} attacks/sec\n" +
                $"{range}m range\n" +
                $"{FormatFloat(cooldown)}s cooldown\n";
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
            var animationTime = ActionClip.length;
            var speedMultiplier = animationTime * rate;
            Debug.Log("Setting speed to " + speedMultiplier);
            holdingPoint.Animator.speed = speedMultiplier;
            holdingPoint.AudioSource.clip = SoundEffect;

            // Configure particles
            holdingPoint.Particles.gameObject.SetActive(false);
        }

        public override void OnDisable() {
            DoDeactivate();
        }

        public override void OnCollision(Collider2D other) {
            base.OnCollision(other);
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
            DoDeactivate();
        }

        protected void DoDeactivate() {
            base.Deactivate();
            holdingPoint.ControlledByInHand = false;
            holdingPoint.Hitbox.enabled = false;
        }
    }
}