using UnityEngine;

namespace TimefulDungeon {
    public class Weapon : Equippable {
        public int damage;
        public int range;
        public float rate;

        private bool attacking;
        private static readonly int Action = Animator.StringToHash("action");

        public Weapon(string name, Sprite sprite, int damage, int range, int rate, float cooldown, EquipType type) :
            base(name, 1, "weapon", sprite, cooldown, type) {
            this.damage = damage;
            this.range = range;
            this.rate = rate;
        }

        protected Weapon(Weapon copy) : base(copy) {
            damage = copy.damage;
            range = copy.range;
            rate = copy.rate;
        }

        public override void Equip() {
            base.Equip();
            var animationTime = actionClip.length;
            var speedMultiplier = animationTime * rate;
            Debug.Log("Setting speed to " + speedMultiplier);
            holdingPoint.animator.speed = speedMultiplier;
            attacking = false;
            holdingPoint.audioSource.clip = soundEffect;
        }

        public override void Activate() {
            base.Activate();
            if (attacking) return;
            ActionStart();
            holdingPoint.animator.SetBool(Action, true);
            attacking = true;
        }

        /// <summary>
        ///     Controls the holding point when button is clicked, and releases when button
        ///     is released and action completes.
        /// </summary>
        /// <returns>True if controlling holding point's position.</returns>
        public override bool ControlHoldingPoint() {
            if (!attacking || !CheckIfActionDone()) return attacking;
            if (activated)
                ActionStart();
            else
                DelayedDeactivate();

            return attacking;
        }

        /// <summary>
        ///     Called during ControlHoldingPoint to check if the current action has been completed.
        /// </summary>
        /// <returns>True if action completed, false otherwise.</returns>
        protected virtual bool CheckIfActionDone() {
            return false;
        }

        /// <summary>
        ///     Called during ControlHoldingPoint when a new action is started.
        /// </summary>
        protected virtual void ActionStart() {
            holdingPoint.audioSource.Play();
        }

        /// <summary>
        ///     Called during ControlHoldingPoint when control is relinquished.
        /// </summary>
        protected virtual void DelayedDeactivate() {
            attacking = false;
            holdingPoint.animator.SetBool(Action, false);
        }
    }
}