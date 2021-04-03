using TimefulDungeon.Enemies;
using TimefulDungeon.Misc;
using UnityEngine;
using VoraUtils;

namespace TimefulDungeon.AI.AggroFSM {
    public class PursueState : AggroStateBase {
        private Vector2 _startSearchPos;
        
        public PursueState(Enemy owner) : base(owner) {
            Name = AggroStates.Pursue;
        }

        public override void Start() {
            _startSearchPos = owner.target.Position2D();
            owner.AddInterest(_startSearchPos);
            Debug.Log("Begin pursue");
        }

        public override AggroStates Update() {
            if (owner.CanSeeTarget()) return AggroStates.Attack;

            if (owner.CloserThan(_startSearchPos, 2)) {
                return AggroStates.Search;
            }
            
            if (owner.debug) Utils.DrawDebugCircle(_startSearchPos, 0.5f, Color.green);
            return Name;
        }

        public override void Exit() {
            owner.RemoveInterest(_startSearchPos);
        }
    }
}