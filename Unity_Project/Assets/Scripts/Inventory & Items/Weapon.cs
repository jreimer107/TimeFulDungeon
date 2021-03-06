using UnityEngine;

namespace TimefulDungeon {
    public class Weapon : Equipment {
        public int damage;
        public int range;
        public float rate;

        protected bool attacking;

        public Weapon(string name, Sprite sprite, int damage, int range, int rate, float cooldown, EquipType type) : 
            base(name, 1, "weapon", sprite, cooldown, type) {
            this.damage = damage;
            this.range = range;
            this.rate = rate;
        }

        protected Weapon(Weapon copy): base(copy) {
            this.damage = copy.damage;
            this.range = copy.range;
            this.rate = copy.rate;
        }

        public override void Equip() {
            base.Equip();
            float animationTime = this.actionClip.length;
            float speedMultiplier = animationTime * rate;
            Debug.Log("Setting speed to " + speedMultiplier);
            holdingPoint.animator.speed = speedMultiplier;
            attacking = false;
            holdingPoint.audioSource.clip = this.soundEffect;
        }

        public override void Activate() {
            base.Activate();
            if (!attacking) {
                ActionStart();
                holdingPoint.animator.SetBool("action", true);
                attacking = true;
            }
        }

        /// <summary>
        /// Controls the holding point when button is clicked, and releases when button
        /// is released and action completes.
        /// </summary>
        /// <returns>True if controlling holding point's position.</returns>
        public override bool ControlHoldingPoint() {
            if (attacking) {
                if (CheckIfActionDone()) {
                    if (activated) {
                        ActionStart();
                    }
                    else {
                        DelayedDeactivate();
                    }
                }
            }
            return attacking;
        }

        /// <summary>
        /// Called during ControlHoldingPoint to check if the current action has been completed.
        /// </summary>
        /// <returns>True if action completed, false otherwise.</returns>
        public virtual bool CheckIfActionDone() { return false; }

        /// <summary>
        /// Called during ControlHoldingPoint when a new action is started.
        /// </summary>
        public virtual void ActionStart() {
            holdingPoint.audioSource.Play();
        }

        /// <summary>
        /// Called during ControlHoldingPoint when control is reliquished.
        /// </summary>
        public virtual void DelayedDeactivate() {
            attacking = false;
            holdingPoint.animator.SetBool("action", false);
        }
    }
}
