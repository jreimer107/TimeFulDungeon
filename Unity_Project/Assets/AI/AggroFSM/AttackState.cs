using TimefulDungeon.Enemies;
using UnityEngine;

namespace TimefulDungeon.AI.AggroFSM {
    public class AttackState : AggroStateBase {
        public AttackState(Enemy owner) : base(owner) {
            Name = AggroStates.Attack;
        }

        public override void Start() {
            Debug.Log("Begin attack");
            owner.AddInterest(owner.target);
        }

        public override void Exit() {
            owner.RemoveInterest(owner.target);
        }

        public override AggroStates Update() {
            if (!owner.CanSeeTarget()) {
                return AggroStates.Pursue;
            }
            return Name;
        }
    }
}