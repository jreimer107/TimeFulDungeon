using TimefulDungeon.AI;
using TimefulDungeon.Core;
using UnityEngine;

namespace TimefulDungeon.Enemies {
    [RequireComponent(typeof(ContextSteering), typeof(WanderModule))]
    public class Terror : Enemy {
        private Transform _player;
        
        protected override void Start() {
            base.Start();
            _player = Player.instance.transform;
            contextSteering.AddInterest(_player);
        }
    }
}