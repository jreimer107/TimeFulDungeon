using TimefulDungeon.Enemies;
using UnityEngine;

namespace TimefulDungeon.AI.AggroFSM {
    public class WanderState: AggroStateBase {
        private bool _speedSlowed;
        
        public WanderState(Enemy owner) : base(owner) {
            Name = AggroStates.Wander;
        }

        public override void Start() {
            Debug.Log("Begin wander");
            owner.WanderAround(owner.spawn);
        }

        public override AggroStates Update() {
            if (owner.CanSeeTarget()) return AggroStates.Attack;

            switch (_speedSlowed) {
                case false when !owner.OutsideWanderLimit:
                    owner.SetSpeed(0.5f);
                    _speedSlowed = true;
                    break;
                case true when owner.OutsideWanderLimit:
                    owner.RestoreSpeed();
                    _speedSlowed = false;
                    break;
            }
            return Name;
        }

        public override void Exit() { 
            owner.RestoreSpeed();
            owner.StopWandering();
        }
    }
}