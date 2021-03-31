using TimefulDungeon.AI;
using TimefulDungeon.AI.AggroFSM;
using TimefulDungeon.Core;
using UnityEngine;

namespace TimefulDungeon.Enemies {
    [RequireComponent(typeof(ContextSteering), typeof(WanderModule))]
    public class Terror : Enemy {
        private FiniteStateMachine<AggroStates> _searchFsm;

        protected override void Start() {
            base.Start();
            _searchFsm = new FiniteStateMachine<AggroStates>();
            _searchFsm.Initialize(AggroStates.Wander, this);
            SetTarget(Player.instance.transform);
        }

        protected override void Update() {
            base.Update();
            _searchFsm.Update();
        }
        
        
    }
}